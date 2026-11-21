using EnadeAds.Data;
using EnadeAds.Duels;
using EnadeAds.Rooms;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Friends;

/// <summary>
/// Convite de um amigo para um duelo ou para uma sala de estudos. Não leva texto: é só "fulano te chamou", e
/// aparece dentro do site, sem e-mail. Some quando a pessoa aceita, dispensa, ou quando o duelo fecha.
/// </summary>
public class FriendInvite
{
    public int Id { get; set; }
    public required string FromId { get; set; }
    public required string ToId { get; set; }

    /// <summary>Duelo para o qual a pessoa foi desafiada; nulo no convite de sala.</summary>
    public int? DuelId { get; set; }

    /// <summary>Sala para a qual a pessoa foi convidada; nulo no desafio de duelo.</summary>
    public int? RoomId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>Um convite como aparece para quem recebeu. No duelo vem o código para jogar; na sala, o nome dela.</summary>
public sealed record FriendInviteView(int Id, string FromName, int TrackId, string? DuelCode, string? RoomName, DateTimeOffset CreatedAt)
{
    public bool IsDuel => DuelCode is not null;
}

public sealed class FriendInviteService(ApplicationDbContext db, TimeProvider clock, DuelService duels, StudyRoomService rooms)
{
    /// <summary>
    /// Desafia um amigo: cria um duelo com as questões da prova ativa de quem desafia e deixa o convite para o outro.
    /// Devolve o código do duelo, ou nulo se as contas não são amigas ou a prova não tem questões.
    /// </summary>
    public async Task<string?> ChallengeAsync(string userId, int friendshipId, CancellationToken cancellationToken = default)
    {
        if (await FriendOfAsync(userId, friendshipId, cancellationToken) is not { } friendId)
        {
            return null;
        }

        var created = await duels.CreateAsync(userId, cancellationToken: cancellationToken);
        if (created.Outcome != CreateDuelOutcome.Created)
        {
            return null;
        }

        var duelId = await db.Duels.IgnoreQueryFilters().Where(d => d.Code == created.Code).Select(d => d.Id).FirstAsync(cancellationToken);
        db.FriendInvites.Add(new FriendInvite { FromId = userId, ToId = friendId, DuelId = duelId, CreatedAt = clock.GetUtcNow() });
        await db.SaveChangesAsync(cancellationToken);
        return created.Code;
    }

    /// <summary>
    /// Convida um amigo para uma sala de que a pessoa é membro. Falso se não são amigos, se quem convida não é
    /// membro ou se o amigo já está na sala. Convidar de novo quem já tem um convite aberto não cria outro.
    /// </summary>
    public async Task<bool> InviteToRoomAsync(string userId, int friendshipId, string roomCode, CancellationToken cancellationToken = default)
    {
        if (await FriendOfAsync(userId, friendshipId, cancellationToken) is not { } friendId
            || await rooms.FindAsync(roomCode, cancellationToken) is not { } room
            || room.Members.All(m => m.UserId != userId)
            || room.Members.Any(m => m.UserId == friendId))
        {
            return false;
        }

        if (!await db.FriendInvites.AnyAsync(i => i.ToId == friendId && i.RoomId == room.Id, cancellationToken))
        {
            db.FriendInvites.Add(new FriendInvite { FromId = userId, ToId = friendId, RoomId = room.Id, CreatedAt = clock.GetUtcNow() });
            await db.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    /// <summary>Amigos que a pessoa pode convidar para a sala: os que não são membros e ainda não têm convite para ela.</summary>
    public async Task<IReadOnlyList<FriendRow>> InvitableAsync(string userId, StudyRoom room, CancellationToken cancellationToken = default)
    {
        var friendships = await db.Friendships.AsNoTracking()
            .Where(f => f.AcceptedAt != null && (f.RequesterId == userId || f.AddresseeId == userId))
            .ToListAsync(cancellationToken);
        if (friendships.Count == 0)
        {
            return [];
        }

        var invited = await db.FriendInvites.AsNoTracking().Where(i => i.RoomId == room.Id).Select(i => i.ToId).ToListAsync(cancellationToken);
        var unavailable = room.Members.Select(m => m.UserId).Concat(invited).ToHashSet();
        var candidates = friendships.Where(f => !unavailable.Contains(f.OtherThan(userId))).ToList();
        var ids = candidates.Select(f => f.OtherThan(userId)).ToList();
        var names = await db.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        return candidates
            .Select(f => new FriendRow(f.Id, DuelService.ShortName(names.GetValueOrDefault(f.OtherThan(userId))), f.AcceptedAt!.Value))
            .OrderBy(f => f.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Convites abertos que a pessoa recebeu, do mais novo para o mais antigo. Duelo já jogado ou encerrado e sala
    /// em que a pessoa já entrou não aparecem.
    /// </summary>
    public async Task<IReadOnlyList<FriendInviteView>> ReceivedAsync(string userId, CancellationToken cancellationToken = default)
    {
        var open = await OpenAsync(userId, cancellationToken);
        var fromIds = open.Select(i => i.Invite.FromId).Distinct().ToList();
        var names = await db.Users.AsNoTracking()
            .Where(u => fromIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        return open
            .OrderByDescending(i => i.Invite.Id)
            .Select(i => new FriendInviteView(i.Invite.Id, DuelService.ShortName(names.GetValueOrDefault(i.Invite.FromId)), i.TrackId, i.DuelCode, i.RoomName, i.Invite.CreatedAt))
            .ToList();
    }

    /// <summary>Quantos avisos a pessoa tem em Amigos: pedidos de amizade recebidos e convites abertos.</summary>
    public async Task<int> PendingCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        var requests = await db.Friendships.CountAsync(
            f => f.AddresseeId == userId && f.AcceptedAt == null && !db.FriendBlocks.Any(b => b.BlockerId == userId && b.BlockedId == f.RequesterId), cancellationToken);
        return requests + (await OpenAsync(userId, cancellationToken)).Count;
    }

    /// <summary>Dispensa um convite recebido. Quem convidou não é avisado.</summary>
    public async Task<bool> DismissAsync(string userId, int inviteId, CancellationToken cancellationToken = default)
    {
        var invite = await db.FriendInvites.FirstOrDefaultAsync(i => i.Id == inviteId && i.ToId == userId, cancellationToken);
        if (invite is null)
        {
            return false;
        }

        db.FriendInvites.Remove(invite);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>Aceita o convite para uma sala: a pessoa entra nela. Devolve o código da sala, ou nulo se o convite não vale mais.</summary>
    public async Task<string?> AcceptRoomAsync(string userId, int inviteId, CancellationToken cancellationToken = default)
    {
        var invite = await db.FriendInvites.FirstOrDefaultAsync(i => i.Id == inviteId && i.ToId == userId && i.RoomId != null, cancellationToken);
        if (invite is null)
        {
            return null;
        }

        var code = await db.StudyRooms.Where(r => r.Id == invite.RoomId).Select(r => r.Code).FirstOrDefaultAsync(cancellationToken);
        db.FriendInvites.Remove(invite);
        await db.SaveChangesAsync(cancellationToken);
        if (code is null || await rooms.JoinAsync(code, userId, cancellationToken) is not (JoinRoomOutcome.Joined or JoinRoomOutcome.AlreadyMember))
        {
            return null;
        }

        return code;
    }

    private sealed record OpenInvite(FriendInvite Invite, int TrackId, string? DuelCode, string? RoomName);

    private async Task<List<OpenInvite>> OpenAsync(string userId, CancellationToken cancellationToken)
    {
        var invites = await db.FriendInvites.AsNoTracking().Where(i => i.ToId == userId).ToListAsync(cancellationToken);
        if (invites.Count == 0)
        {
            return [];
        }

        var duelIds = invites.Select(i => i.DuelId).OfType<int>().ToList();
        var roomIds = invites.Select(i => i.RoomId).OfType<int>().ToList();
        var duelRows = await db.Duels.IgnoreQueryFilters().AsNoTracking()
            .Where(d => duelIds.Contains(d.Id))
            .Select(d => new { d.Id, d.Code, d.TrackId, d.CreatedAt, Playing = d.Players.Any(p => p.UserId == userId) })
            .ToListAsync(cancellationToken);
        var roomRows = await db.StudyRooms.AsNoTracking()
            .Where(r => roomIds.Contains(r.Id))
            .Select(r => new { r.Id, r.Name, r.TrackId, Member = r.Members.Any(m => m.UserId == userId) })
            .ToListAsync(cancellationToken);

        var now = clock.GetUtcNow();
        var open = new List<OpenInvite>();
        foreach (var invite in invites)
        {
            if (invite.DuelId is { } duelId && duelRows.FirstOrDefault(d => d.Id == duelId) is { Playing: false } duel && now <= duel.CreatedAt + DuelService.JoinWindow)
            {
                open.Add(new OpenInvite(invite, duel.TrackId, duel.Code, null));
            }
            else if (invite.RoomId is { } roomId && roomRows.FirstOrDefault(r => r.Id == roomId) is { Member: false } room)
            {
                open.Add(new OpenInvite(invite, room.TrackId, null, room.Name));
            }
        }

        return open;
    }

    /// <summary>A outra conta de uma amizade aceita da pessoa; nulo se o número não é de uma amizade dela.</summary>
    private async Task<string?> FriendOfAsync(string userId, int friendshipId, CancellationToken cancellationToken)
    {
        var friendship = await db.Friendships.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == friendshipId && f.AcceptedAt != null && (f.RequesterId == userId || f.AddresseeId == userId), cancellationToken);
        return friendship?.OtherThan(userId);
    }
}

/// <summary>
/// O número de avisos de Amigos para o topo do site. É contado antes de a página começar a renderizar
/// (<see cref="SocialNoticeEndpoints.UseSocialNotices"/>), e o cabeçalho só lê o resultado: o cabeçalho e a página
/// rodam ao mesmo tempo, e um <see cref="ApplicationDbContext"/> não aceita duas consultas simultâneas.
/// </summary>
public sealed class SocialNotices(FriendInviteService invites)
{
    public int Count { get; private set; }

    public async Task PreloadAsync(string userId, CancellationToken cancellationToken = default) =>
        Count = await invites.PendingCountAsync(userId, cancellationToken);
}

public static class SocialNoticeEndpoints
{
    /// <summary>Conta os avisos de quem está logado, só nas requisições de página (não em arquivos nem em outros endpoints).</summary>
    public static void UseSocialNotices(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            var isPage = context.GetEndpoint()?.Metadata.GetMetadata<Microsoft.AspNetCore.Components.Endpoints.ComponentTypeMetadata>() is not null;
            if (isPage && HttpMethods.IsGet(context.Request.Method)
                && context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value is { } userId)
            {
                await context.RequestServices.GetRequiredService<SocialNotices>().PreloadAsync(userId, context.RequestAborted);
            }

            await next();
        });
}
