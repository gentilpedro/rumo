using System.Security.Cryptography;
using EnadeAds.Data;
using EnadeAds.Questions;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Duels;

public enum CreateDuelOutcome
{
    Created,
    NoQuestions,
}

public sealed record CreateDuelResult(CreateDuelOutcome Outcome, string? Code);

public enum JoinDuelOutcome
{
    Started,
    AlreadyPlaying,
    NotFound,
    Closed,
    Full,
}

public enum SubmitDuelOutcome
{
    Submitted,
    NotFound,
    NotStarted,
    AlreadyFinished,
}

/// <summary>
/// Linha do placar. <see cref="Position"/> é nulo para quem ainda não enviou; <see cref="GaveUp"/> marca quem
/// começou, deixou o tempo acabar e não enviou.
/// </summary>
public sealed record DuelStanding(int? Position, string Name, bool IsMe, int? Correct, TimeSpan? Elapsed, bool GaveUp = false);

/// <summary>Um duelo na lista "Meus duelos".</summary>
public sealed record DuelSummary(string Code, int TrackId, DateTimeOffset CreatedAt, int Players, int? MyPosition, int? MyCorrect, int QuestionCount);

/// <summary>
/// Duelos entre colegas. O duelo é da trilha em que foi criado: quem abre o link joga as questões dela, esteja na
/// trilha que estiver. Por isso as consultas daqui ignoram o filtro de trilha do <see cref="ApplicationDbContext"/>.
/// </summary>
public sealed class DuelService(ApplicationDbContext db, TimeProvider clock, ILogger<DuelService> logger)
{
    public const int QuestionCount = 5;
    public const int MaxPlayers = 30;
    public const int CodeLength = 8;

    /// <summary>Tempo de cada pessoa, contado do "Começar". Curto de propósito: é uma partida, não um simulado.</summary>
    public static readonly TimeSpan TimeLimit = TimeSpan.FromMinutes(10);

    /// <summary>Por quanto tempo o link aceita gente nova.</summary>
    public static readonly TimeSpan JoinWindow = TimeSpan.FromDays(7);

    /// <summary>Tolerância para o envio feito no último segundo chegar dentro do prazo.</summary>
    public static readonly TimeSpan GracePeriod = TimeSpan.FromSeconds(30);

    // Sem 0/O, 1/I/L: o código pode ser lido em voz alta ou digitado.
    private const string CodeAlphabet = "abcdefghjkmnpqrstuvwxyz23456789";

    /// <summary>
    /// Cria o duelo com as questões da trilha ativa, a prova que a pessoa está estudando. A revanche passa
    /// <paramref name="trackId"/> para repetir a prova do duelo anterior.
    /// </summary>
    public async Task<CreateDuelResult> CreateAsync(string userId, int? trackId = null, CancellationToken cancellationToken = default)
    {
        var track = trackId ?? db.CurrentTrackId;
        var pool = await db.Questions.IgnoreQueryFilters().AsNoTracking()
            .Where(q => q.Subject!.TrackId == track)
            .Where(q => q.Kind == QuestionKind.Objective && q.ArchivedAt == null && q.Options.Any(o => o.IsCorrect))
            .Select(q => q.Id)
            .ToListAsync(cancellationToken);
        if (pool.Count == 0)
        {
            return new CreateDuelResult(CreateDuelOutcome.NoQuestions, null);
        }

        var picked = pool.OrderBy(_ => Random.Shared.Next()).Take(QuestionCount).ToList();
        var duel = new Duel
        {
            Code = NewCode(),
            TrackId = track,
            CreatorId = userId,
            CreatedAt = clock.GetUtcNow(),
            Questions = picked.Select((id, index) => new DuelQuestion { QuestionId = id, Order = index + 1 }).ToList(),
        };

        db.Duels.Add(duel);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Duelo {DuelId} criado na trilha {TrackId} com {Count} questões.", duel.Id, duel.TrackId, picked.Count);
        return new CreateDuelResult(CreateDuelOutcome.Created, duel.Code);
    }

    /// <summary>Duelo com as questões, as alternativas e quem já jogou; nulo se o código não existir.</summary>
    public Task<Duel?> FindAsync(string code, CancellationToken cancellationToken = default) =>
        db.Duels.IgnoreQueryFilters().AsNoTracking()
            .Where(d => d.Code == Normalize(code))
            .Include(d => d.Questions.OrderBy(q => q.Order))
                .ThenInclude(q => q.Question!)
                    .ThenInclude(q => q.Options.OrderBy(o => o.Letter))
            .Include(d => d.Questions)
                .ThenInclude(q => q.Question!)
                    .ThenInclude(q => q.Subject)
            .Include(d => d.Players)
                .ThenInclude(p => p.Answers)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>Entra no duelo e começa o relógio da pessoa.</summary>
    public async Task<JoinDuelOutcome> JoinAsync(string code, string userId, CancellationToken cancellationToken = default)
    {
        var duel = await db.Duels.IgnoreQueryFilters().Include(d => d.Players)
            .FirstOrDefaultAsync(d => d.Code == Normalize(code), cancellationToken);
        if (duel is null)
        {
            return JoinDuelOutcome.NotFound;
        }

        if (duel.Players.Any(p => p.UserId == userId))
        {
            return JoinDuelOutcome.AlreadyPlaying;
        }

        var now = clock.GetUtcNow();
        if (now > duel.JoinUntil)
        {
            return JoinDuelOutcome.Closed;
        }

        if (duel.Players.Count >= MaxPlayers)
        {
            return JoinDuelOutcome.Full;
        }

        duel.Players.Add(new DuelPlayer { UserId = userId, StartedAt = now });
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Dois cliques em "Começar" ao mesmo tempo: a chave (duelo, pessoa) segura o segundo.
            return JoinDuelOutcome.AlreadyPlaying;
        }

        return JoinDuelOutcome.Started;
    }

    /// <summary>
    /// Corrige e fecha a participação. Depois do prazo, o que chega é ignorado e as questões contam em branco,
    /// como no simulado. As respostas também entram como tentativas, para o desempenho e o caderno de erros.
    /// </summary>
    public async Task<SubmitDuelOutcome> SubmitAsync(string code, string userId, IReadOnlyDictionary<int, char> letters,
        CancellationToken cancellationToken = default)
    {
        var duel = await db.Duels.IgnoreQueryFilters()
            .Include(d => d.Questions).ThenInclude(q => q.Question!).ThenInclude(q => q.Options)
            .Include(d => d.Players)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.Code == Normalize(code), cancellationToken);
        if (duel is null)
        {
            return SubmitDuelOutcome.NotFound;
        }

        var player = duel.Players.FirstOrDefault(p => p.UserId == userId);
        if (player is null)
        {
            return SubmitDuelOutcome.NotStarted;
        }

        if (player.HasFinished)
        {
            return SubmitDuelOutcome.AlreadyFinished;
        }

        var now = clock.GetUtcNow();
        var inTime = now <= player.StartedAt + TimeLimit + GracePeriod;
        var correct = 0;
        foreach (var item in duel.Questions)
        {
            var question = item.Question!;
            if (!inTime || !letters.TryGetValue(item.QuestionId, out var letter)
                || question.Options.FirstOrDefault(o => o.Letter == char.ToUpperInvariant(letter)) is not { } option)
            {
                continue;
            }

            correct += option.IsCorrect ? 1 : 0;
            db.DuelAnswers.Add(new DuelAnswer { DuelId = duel.Id, UserId = userId, QuestionId = question.Id, OptionId = option.Id, IsCorrect = option.IsCorrect });
            db.QuestionAttempts.Add(new QuestionAttempt
            {
                QuestionId = question.Id, UserId = userId, OptionId = option.Id, IsCorrect = option.IsCorrect, AnsweredAt = now,
            });
        }

        player.FinishedAt = now;
        player.CorrectCount = correct;
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Duelo {DuelId}: participante enviou {Correct} de {Total}{Late}.",
            duel.Id, correct, duel.Questions.Count, inTime ? "" : " (fora do prazo)");
        return SubmitDuelOutcome.Submitted;
    }

    /// <summary>
    /// Placar: mais acertos primeiro e, no empate, menor tempo. Mostra só o primeiro nome e a inicial do sobrenome,
    /// e só para quem jogou o duelo (a página não mostra o placar a quem não entrou).
    /// </summary>
    public async Task<IReadOnlyList<DuelStanding>> StandingsAsync(Duel duel, string? userId, CancellationToken cancellationToken = default)
    {
        var ids = duel.Players.Select(p => p.UserId).ToList();
        var names = await db.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        var now = clock.GetUtcNow();
        var finished = Rank(duel.Players);
        var standings = finished
            .Select((p, index) => new DuelStanding(index + 1, ShortName(names.GetValueOrDefault(p.UserId)), p.UserId == userId, p.CorrectCount, p.Elapsed))
            .ToList();

        standings.AddRange(duel.Players.Where(p => !p.HasFinished).OrderBy(p => p.StartedAt)
            .Select(p => new DuelStanding(null, ShortName(names.GetValueOrDefault(p.UserId)), p.UserId == userId, null, null,
                GaveUp: now > p.StartedAt + TimeLimit + GracePeriod)));
        return standings;
    }

    /// <summary>Duelos de que a pessoa participou, do mais novo para o mais antigo.</summary>
    public async Task<IReadOnlyList<DuelSummary>> MineAsync(string userId, CancellationToken cancellationToken = default)
    {
        var duels = await db.Duels.IgnoreQueryFilters().AsNoTracking()
            .Where(d => d.Players.Any(p => p.UserId == userId))
            .Include(d => d.Players)
            .Include(d => d.Questions)
            .OrderByDescending(d => d.Id)
            .Take(50)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return duels.Select(d =>
        {
            var ranked = Rank(d.Players);
            var mine = d.Players.Single(p => p.UserId == userId);
            int? position = mine.HasFinished ? ranked.FindIndex(p => p.UserId == userId) + 1 : null;
            return new DuelSummary(d.Code, d.TrackId, d.CreatedAt, d.Players.Count, position, mine.CorrectCount, d.Questions.Count);
        }).ToList();
    }

    /// <summary>Quem já enviou, do primeiro ao último lugar.</summary>
    public static List<DuelPlayer> Rank(IEnumerable<DuelPlayer> players) =>
        players.Where(p => p.HasFinished)
            .OrderByDescending(p => p.CorrectCount)
            .ThenBy(p => p.Elapsed)
            .ThenBy(p => p.FinishedAt)
            .ToList();

    /// <summary>"Maria Clara Souza" vira "Maria S.": o placar é visto por colegas, não precisa do nome completo.</summary>
    public static string ShortName(string? fullName)
    {
        var parts = (fullName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "Participante",
            1 => parts[0],
            _ => $"{parts[0]} {char.ToUpperInvariant(parts[^1][0])}.",
        };
    }

    public static string Normalize(string code) => code.Trim().ToLowerInvariant();

    private static string NewCode() => RandomNumberGenerator.GetString(CodeAlphabet, CodeLength);
}
