using EnadeAds.Data;
using EnadeAds.Questions;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Survival;

/// <summary>
/// Partida do modo sobrevivência: questões em sequência até acabarem as vidas. A ordem é sorteada no começo e
/// gravada em <see cref="Queue"/>, da mais acertada pela turma para a menos acertada.
/// </summary>
public class SurvivalRun
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public int TrackId { get; set; }
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>Preenchido quando as vidas ou as questões acabam, ou quando a pessoa começa outra partida.</summary>
    public DateTimeOffset? EndedAt { get; set; }

    public int Lives { get; set; } = SurvivalService.StartingLives;
    public int Score { get; set; }

    /// <summary>Ids das questões na ordem da partida, separados por vírgula.</summary>
    public required string Queue { get; set; }

    /// <summary>Índice, em <see cref="Queue"/>, da questão da vez.</summary>
    public int Position { get; set; }

    public bool IsOpen => EndedAt is null;

    public IReadOnlyList<int> QuestionIds => Queue.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

    public int? CurrentQuestionId => IsOpen && Position < QuestionIds.Count ? QuestionIds[Position] : null;
}

public enum SurvivalAnswerOutcome
{
    Answered,
    NotFound,
    Ended,

    /// <summary>A resposta é de uma questão que já passou (aba antiga, botão voltar): não conta de novo.</summary>
    Stale,
}

public sealed record SurvivalAnswer(SurvivalAnswerOutcome Outcome, QuestionAttempt? Attempt = null, int XpGained = 0);

public sealed class SurvivalService(ApplicationDbContext db, QuestionBank bank, TimeProvider clock)
{
    public const int StartingLives = 3;

    /// <summary>Questão com menos respostas que isso fica no meio da fila: ainda não dá para dizer se é fácil.</summary>
    private const int MinimumForDifficulty = CommunityScore.MinimumPeople;

    /// <summary>Encerra a partida aberta da trilha ativa, se houver, e começa outra. Nulo se a trilha não tem objetivas.</summary>
    public async Task<int?> StartAsync(string userId, CancellationToken cancellationToken = default)
    {
        var trackId = db.CurrentTrackId;
        var now = clock.GetUtcNow();
        foreach (var open in await db.SurvivalRuns.Where(r => r.UserId == userId && r.TrackId == trackId && r.EndedAt == null).ToListAsync(cancellationToken))
        {
            open.EndedAt = now;
        }

        var queue = await QueueAsync(cancellationToken);
        if (queue.Count == 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var run = new SurvivalRun { UserId = userId, TrackId = trackId, StartedAt = now, Queue = string.Join(',', queue) };
        db.SurvivalRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);
        return run.Id;
    }

    /// <summary>Partida em andamento da pessoa na trilha ativa.</summary>
    public Task<SurvivalRun?> OpenAsync(string userId, CancellationToken cancellationToken = default) =>
        db.SurvivalRuns.AsNoTracking()
            .Where(r => r.UserId == userId && r.EndedAt == null)
            .OrderByDescending(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>Partida da própria pessoa; nula se não existir ou for de outra.</summary>
    public Task<SurvivalRun?> FindAsync(string userId, int runId, CancellationToken cancellationToken = default) =>
        db.SurvivalRuns.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(r => r.Id == runId && r.UserId == userId, cancellationToken);

    /// <summary>Melhor partida da pessoa na trilha, sem contar <paramref name="exceptRunId"/>.</summary>
    public async Task<int> RecordAsync(string userId, int trackId, int? exceptRunId = null, CancellationToken cancellationToken = default) =>
        await db.SurvivalRuns.IgnoreQueryFilters().AsNoTracking()
            .Where(r => r.UserId == userId && r.TrackId == trackId && r.Id != exceptRunId)
            .MaxAsync(r => (int?)r.Score, cancellationToken) ?? 0;

    /// <summary>Questão da vez, com alternativas e explicações (que a página só mostra depois da resposta).</summary>
    public Task<Question?> QuestionAsync(int questionId, CancellationToken cancellationToken = default) =>
        db.Questions.IgnoreQueryFilters().AsNoTracking()
            .Include(q => q.Subject)
            .Include(q => q.Options.OrderBy(o => o.Letter))
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

    /// <summary>Responde a questão da vez e passa para a próxima. Errar tira uma vida; sem vidas, a partida acaba.</summary>
    public async Task<SurvivalAnswer> AnswerAsync(string userId, int runId, int questionId, char letter, CancellationToken cancellationToken = default)
    {
        var run = await db.SurvivalRuns.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == runId && r.UserId == userId, cancellationToken);
        if (run is null)
        {
            return new SurvivalAnswer(SurvivalAnswerOutcome.NotFound);
        }

        if (!run.IsOpen)
        {
            return new SurvivalAnswer(SurvivalAnswerOutcome.Ended);
        }

        if (run.CurrentQuestionId != questionId)
        {
            return new SurvivalAnswer(SurvivalAnswerOutcome.Stale);
        }

        var answer = await bank.AnswerAsync(userId, questionId, letter, cancellationToken);
        if (answer is null)
        {
            return new SurvivalAnswer(SurvivalAnswerOutcome.NotFound);
        }

        run.Position++;
        if (answer.Attempt.IsCorrect)
        {
            run.Score++;
        }
        else
        {
            run.Lives--;
        }

        if (run.Lives == 0 || run.Position >= run.QuestionIds.Count)
        {
            run.EndedAt = clock.GetUtcNow();
        }

        await db.SaveChangesAsync(cancellationToken);
        return new SurvivalAnswer(SurvivalAnswerOutcome.Answered, answer.Attempt, answer.XpGained);
    }

    /// <summary>
    /// Objetivas da trilha ativa, da mais acertada de primeira para a menos acertada. As que ainda não têm respostas
    /// suficientes entram como 50%. O sorteio desempata, para cada partida ter outra ordem.
    /// </summary>
    private async Task<List<int>> QueueAsync(CancellationToken cancellationToken)
    {
        var ids = await db.Questions.AsNoTracking()
            .Where(q => q.Kind == QuestionKind.Objective && q.ArchivedAt == null && q.Options.Any(o => o.IsCorrect))
            .Select(q => q.Id)
            .ToListAsync(cancellationToken);

        // A primeira resposta de cada pessoa, como em CommunityScore.
        var attempts = await db.QuestionAttempts.AsNoTracking()
            .Where(a => ids.Contains(a.QuestionId) && a.ChallengeId == null)
            .Select(a => new { a.Id, a.QuestionId, a.UserId, a.IsCorrect })
            .ToListAsync(cancellationToken);
        var rate = attempts.GroupBy(a => (a.QuestionId, a.UserId))
            .Select(g => g.MinBy(a => a.Id)!)
            .GroupBy(a => a.QuestionId)
            .Where(g => g.Count() >= MinimumForDifficulty)
            .ToDictionary(g => g.Key, g => g.Average(a => a.IsCorrect ? 1.0 : 0.0));

        // Faixas de 10 pontos: questões com acerto parecido saem em ordem sorteada.
        return ids
            .Select(id => (id, band: (int)Math.Round(rate.GetValueOrDefault(id, 0.5) * 10), shuffle: Random.Shared.Next()))
            .OrderByDescending(q => q.band)
            .ThenBy(q => q.shuffle)
            .Select(q => q.id)
            .ToList();
    }
}
