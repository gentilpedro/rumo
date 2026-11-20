using System.Security.Cryptography;
using EnadeAds.Data;
using EnadeAds.Duels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Friends;

public enum FriendRequestOutcome
{
    /// <summary>
    /// Pedido enviado. Quem foi bloqueado pela outra pessoa recebe a mesma resposta e vê o pedido na lista de
    /// enviados: o bloqueio é silencioso. O pedido só não aparece para quem bloqueou.
    /// </summary>
    Requested,

    /// <summary>A outra pessoa já tinha pedido: os dois quiseram, então a amizade vale na hora.</summary>
    Accepted,
    AlreadyFriends,
    AlreadyRequested,
    Self,
    NotFound,

    /// <summary>Quem pede bloqueou a outra pessoa: precisa desbloquear antes.</summary>
    YouBlocked,
    TooManyFriends,
    TooManyRequests,
}

public enum FriendActionOutcome
{
    Done,
    NotFound,
    TooManyFriends,
}

/// <summary>Uma pessoa numa das listas da página Amigos: só o primeiro nome e a inicial do sobrenome.</summary>
public sealed record FriendRow(int Id, string Name, DateTimeOffset Since);

/// <summary>Alguém com quem a pessoa já jogou um duelo. O <see cref="Token"/> leva o pedido sem pôr o id da conta na página.</summary>
public sealed record FriendSuggestion(string Token, string Name);

public sealed record FriendsOverview(
    IReadOnlyList<FriendRow> Received,
    IReadOnlyList<FriendRow> Sent,
    IReadOnlyList<FriendRow> Friends,
    IReadOnlyList<FriendRow> Blocked,
    IReadOnlyList<FriendSuggestion> Suggestions);

/// <summary>
/// Amizades entre contas, no modelo "seguir com aceite": uma pessoa pede, a outra aceita, e só então as duas viram
/// amigas. Não há busca por nome nem por e-mail: chega-se a alguém pelo link que a pessoa mandou ou por um duelo em
/// comum. Não há mensagem: o pedido não leva texto. A plataforma aceita menores de idade (LGPD, art. 14).
/// </summary>
public sealed class FriendService(ApplicationDbContext db, TimeProvider clock, IDataProtectionProvider protection, ILogger<FriendService> logger)
{
    public const int MaxFriends = 200;

    /// <summary>Pedidos enviados que ainda esperam resposta: o teto impede uma conta de pedir amizade em massa.</summary>
    public const int MaxPendingSent = 20;

    public const int MaxSuggestions = 8;
    public const int CodeLength = 10;

    /// <summary>Validade do token de uma sugestão: o tempo de uma visita à página.</summary>
    public static readonly TimeSpan SuggestionLifetime = TimeSpan.FromHours(12);

    // Sem 0/O, 1/I/L: o código pode ser lido em voz alta ou digitado.
    private const string CodeAlphabet = "abcdefghjkmnpqrstuvwxyz23456789";

    private readonly ITimeLimitedDataProtector suggestions = protection.CreateProtector("EnadeAds.Friends.Suggestion").ToTimeLimitedDataProtector();

    /// <summary>O código do link de amigo da pessoa; criado na primeira vez que ela abre a página Amigos.</summary>
    public async Task<string> LinkAsync(string userId, CancellationToken cancellationToken = default)
    {
        var link = await db.FriendLinks.FirstOrDefaultAsync(l => l.UserId == userId, cancellationToken);
        if (link is null)
        {
            link = new FriendLink { UserId = userId, Code = NewCode(), CreatedAt = clock.GetUtcNow() };
            db.FriendLinks.Add(link);
            await db.SaveChangesAsync(cancellationToken);
        }

        return link.Code;
    }

    /// <summary>Gera um link novo; o anterior para de funcionar.</summary>
    public async Task<string> RenewLinkAsync(string userId, CancellationToken cancellationToken = default)
    {
        await LinkAsync(userId, cancellationToken);
        var link = await db.FriendLinks.FirstAsync(l => l.UserId == userId, cancellationToken);
        link.Code = NewCode();
        link.CreatedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
        return link.Code;
    }

    /// <summary>De quem é o link, com o nome abreviado para a tela do convite; nulo se o código não existir.</summary>
    public async Task<(string UserId, string Name)?> OwnerOfAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToLowerInvariant();
        var owner = await db.FriendLinks.AsNoTracking()
            .Where(l => l.Code == normalized)
            .Join(db.Users, l => l.UserId, u => u.Id, (l, u) => new { u.Id, u.FullName })
            .FirstOrDefaultAsync(cancellationToken);
        return owner is null ? null : (owner.Id, DuelService.ShortName(owner.FullName));
    }

    public async Task<FriendRequestOutcome> RequestByCodeAsync(string fromId, string code, CancellationToken cancellationToken = default) =>
        await OwnerOfAsync(code, cancellationToken) is { } owner
            ? await RequestAsync(fromId, owner.UserId, cancellationToken)
            : FriendRequestOutcome.NotFound;

    /// <summary>Pedido a partir de uma sugestão. O token só vale para quem recebeu a sugestão e por algumas horas.</summary>
    public async Task<FriendRequestOutcome> RequestSuggestedAsync(string fromId, string token, CancellationToken cancellationToken = default)
    {
        string payload;
        try
        {
            payload = suggestions.Unprotect(token);
        }
        catch (Exception error) when (error is CryptographicException or FormatException)
        {
            // Token adulterado ou vencido: para quem pediu, é como se a pessoa não existisse.
            return FriendRequestOutcome.NotFound;
        }

        var parts = payload.Split('|');
        return parts.Length == 2 && parts[0] == fromId
            ? await RequestAsync(fromId, parts[1], cancellationToken)
            : FriendRequestOutcome.NotFound;
    }

    public async Task<FriendRequestOutcome> RequestAsync(string fromId, string toId, CancellationToken cancellationToken = default)
    {
        if (fromId == toId)
        {
            return FriendRequestOutcome.Self;
        }

        if (!await db.Users.AnyAsync(u => u.Id == toId, cancellationToken))
        {
            return FriendRequestOutcome.NotFound;
        }

        if (await db.FriendBlocks.AnyAsync(b => b.BlockerId == fromId && b.BlockedId == toId, cancellationToken))
        {
            return FriendRequestOutcome.YouBlocked;
        }

        var existing = await db.Friendships.FirstOrDefaultAsync(
            f => (f.RequesterId == fromId && f.AddresseeId == toId) || (f.RequesterId == toId && f.AddresseeId == fromId), cancellationToken);
        if (existing is { IsAccepted: true })
        {
            return FriendRequestOutcome.AlreadyFriends;
        }

        if (existing is not null && existing.RequesterId == fromId)
        {
            return FriendRequestOutcome.AlreadyRequested;
        }

        if (await FriendCountAsync(fromId, cancellationToken) >= MaxFriends)
        {
            return FriendRequestOutcome.TooManyFriends;
        }

        var now = clock.GetUtcNow();
        if (existing is not null)
        {
            // A outra pessoa já tinha pedido: pedir de volta é aceitar.
            if (await FriendCountAsync(toId, cancellationToken) >= MaxFriends)
            {
                return FriendRequestOutcome.TooManyFriends;
            }

            existing.AcceptedAt = now;
            await db.SaveChangesAsync(cancellationToken);
            return FriendRequestOutcome.Accepted;
        }

        // Bloqueado pela outra pessoa? O pedido é gravado do mesmo jeito, para quem pediu não perceber o bloqueio;
        // ele só não aparece para quem bloqueou (ver OverviewAsync e AcceptAsync).
        if (await db.Friendships.CountAsync(f => f.RequesterId == fromId && f.AcceptedAt == null, cancellationToken) >= MaxPendingSent)
        {
            return FriendRequestOutcome.TooManyRequests;
        }

        db.Friendships.Add(new Friendship { RequesterId = fromId, AddresseeId = toId, CreatedAt = now });
        await db.SaveChangesAsync(cancellationToken);
        return FriendRequestOutcome.Requested;
    }

    /// <summary>Aceita um pedido recebido. Só quem recebeu aceita.</summary>
    public async Task<FriendActionOutcome> AcceptAsync(string userId, int friendshipId, CancellationToken cancellationToken = default)
    {
        var request = await db.Friendships.FirstOrDefaultAsync(f => f.Id == friendshipId && f.AddresseeId == userId && f.AcceptedAt == null, cancellationToken);
        if (request is null || await db.FriendBlocks.AnyAsync(b => b.BlockerId == userId && b.BlockedId == request.RequesterId, cancellationToken))
        {
            return FriendActionOutcome.NotFound;
        }

        if (await FriendCountAsync(userId, cancellationToken) >= MaxFriends || await FriendCountAsync(request.RequesterId, cancellationToken) >= MaxFriends)
        {
            return FriendActionOutcome.TooManyFriends;
        }

        request.AcceptedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
        return FriendActionOutcome.Done;
    }

    /// <summary>
    /// Apaga a relação: recusar um pedido recebido, cancelar um pedido enviado ou desfazer uma amizade. A outra
    /// pessoa não é avisada; a linha só some da lista dela.
    /// </summary>
    public async Task<FriendActionOutcome> RemoveAsync(string userId, int friendshipId, CancellationToken cancellationToken = default)
    {
        var relation = await MineAsync(userId, friendshipId, cancellationToken);
        if (relation is null)
        {
            return FriendActionOutcome.NotFound;
        }

        db.Friendships.Remove(relation);
        await db.SaveChangesAsync(cancellationToken);
        return FriendActionOutcome.Done;
    }

    /// <summary>Bloqueia a outra pessoa de um pedido ou de uma amizade: a relação some e ela não consegue pedir de novo.</summary>
    public async Task<FriendActionOutcome> BlockAsync(string userId, int friendshipId, CancellationToken cancellationToken = default)
    {
        var relation = await MineAsync(userId, friendshipId, cancellationToken);
        if (relation is null)
        {
            return FriendActionOutcome.NotFound;
        }

        var other = relation.OtherThan(userId);
        db.Friendships.Remove(relation);
        if (!await db.FriendBlocks.AnyAsync(b => b.BlockerId == userId && b.BlockedId == other, cancellationToken))
        {
            db.FriendBlocks.Add(new FriendBlock { BlockerId = userId, BlockedId = other, CreatedAt = clock.GetUtcNow() });
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Conta {UserId} bloqueou outra conta nos amigos.", userId);
        return FriendActionOutcome.Done;
    }

    public async Task<FriendActionOutcome> UnblockAsync(string userId, int blockId, CancellationToken cancellationToken = default)
    {
        var block = await db.FriendBlocks.FirstOrDefaultAsync(b => b.Id == blockId && b.BlockerId == userId, cancellationToken);
        if (block is null)
        {
            return FriendActionOutcome.NotFound;
        }

        db.FriendBlocks.Remove(block);
        await db.SaveChangesAsync(cancellationToken);
        return FriendActionOutcome.Done;
    }

    /// <summary>Se as duas contas são amigas (pedido aceito).</summary>
    public Task<bool> AreFriendsAsync(string userId, string otherId, CancellationToken cancellationToken = default) =>
        db.Friendships.AnyAsync(f => f.AcceptedAt != null
            && ((f.RequesterId == userId && f.AddresseeId == otherId) || (f.RequesterId == otherId && f.AddresseeId == userId)), cancellationToken);

    /// <summary>Tudo o que a página Amigos mostra, sempre com nomes abreviados.</summary>
    public async Task<FriendsOverview> OverviewAsync(string userId, CancellationToken cancellationToken = default)
    {
        var relations = await db.Friendships.AsNoTracking()
            .Where(f => f.RequesterId == userId || f.AddresseeId == userId)
            .OrderBy(f => f.Id)
            .ToListAsync(cancellationToken);
        var blocks = await db.FriendBlocks.AsNoTracking().Where(b => b.BlockerId == userId).OrderBy(b => b.Id).ToListAsync(cancellationToken);
        var blockedMe = await db.FriendBlocks.AsNoTracking().Where(b => b.BlockedId == userId).Select(b => b.BlockerId).ToListAsync(cancellationToken);

        // Sugestões: quem jogou os mesmos duelos, do mais recente para o mais antigo, sem quem já tem relação ou bloqueio.
        var myDuels = db.DuelPlayers.Where(p => p.UserId == userId).Select(p => p.DuelId);
        var met = await db.DuelPlayers.AsNoTracking()
            .Where(p => myDuels.Contains(p.DuelId) && p.UserId != userId)
            .GroupBy(p => p.UserId)
            .Select(g => new { UserId = g.Key, Last = g.Max(p => p.DuelId) })
            .OrderByDescending(g => g.Last)
            .Take(50)
            .ToListAsync(cancellationToken);
        var known = relations.Select(f => f.OtherThan(userId)).Concat(blocks.Select(b => b.BlockedId)).Concat(blockedMe).ToHashSet();
        var suggested = met.Select(m => m.UserId).Where(id => !known.Contains(id)).Take(MaxSuggestions).ToList();

        var ids = known.Concat(suggested).ToList();
        var names = await db.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);
        string Name(string id) => DuelService.ShortName(names.GetValueOrDefault(id));

        return new FriendsOverview(
            // Pedido de quem a pessoa bloqueou fica guardado, mas não aparece para ela.
            Received: relations.Where(f => !f.IsAccepted && f.AddresseeId == userId && blocks.All(b => b.BlockedId != f.RequesterId)).Select(f => new FriendRow(f.Id, Name(f.RequesterId), f.CreatedAt)).ToList(),
            Sent: relations.Where(f => !f.IsAccepted && f.RequesterId == userId).Select(f => new FriendRow(f.Id, Name(f.AddresseeId), f.CreatedAt)).ToList(),
            Friends: relations.Where(f => f.IsAccepted).Select(f => new FriendRow(f.Id, Name(f.OtherThan(userId)), f.AcceptedAt!.Value))
                .OrderBy(f => f.Name, StringComparer.CurrentCultureIgnoreCase).ToList(),
            Blocked: blocks.Select(b => new FriendRow(b.Id, Name(b.BlockedId), b.CreatedAt)).ToList(),
            Suggestions: suggested.Select(id => new FriendSuggestion(suggestions.Protect($"{userId}|{id}", SuggestionLifetime), Name(id))).ToList());
    }

    private Task<int> FriendCountAsync(string userId, CancellationToken cancellationToken) =>
        db.Friendships.CountAsync(f => f.AcceptedAt != null && (f.RequesterId == userId || f.AddresseeId == userId), cancellationToken);

    private Task<Friendship?> MineAsync(string userId, int friendshipId, CancellationToken cancellationToken) =>
        db.Friendships.FirstOrDefaultAsync(f => f.Id == friendshipId && (f.RequesterId == userId || f.AddresseeId == userId), cancellationToken);

    private static string NewCode() => RandomNumberGenerator.GetString(CodeAlphabet, CodeLength);
}
