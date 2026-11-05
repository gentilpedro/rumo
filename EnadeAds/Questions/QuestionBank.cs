using EnadeAds.Achievements;
using EnadeAds.Data;
using EnadeAds.Study;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Questions;

public enum AttemptOutcome
{
    NotAnswered,
    Correct,
    Wrong,

    /// <summary>Discursiva respondida: não tem certo ou errado automático.</summary>
    Answered,
}

public sealed record QuestionSummary(int Id, QuestionKind Kind, string Excerpt, string SubjectTitle, string SubjectSlug, string Source, AttemptOutcome Outcome, StudyBlock Block = StudyBlock.Fundamentals, string? Code = null);

public sealed record QuestionPage(IReadOnlyList<QuestionSummary> Items, int Page, int TotalPages, int TotalCount);

/// <param name="XpGained">XP que esta resposta rendeu: só a primeira resposta e o primeiro acerto valem.</param>
public sealed record AnswerResult(QuestionAttempt Attempt, QuestionOption Chosen, int XpGained = 0);

public sealed class QuestionBank(ApplicationDbContext db, TimeProvider clock)
{
    public const int PageSize = 20;
    public const int DiscursiveAnswerMaxLength = 5000;
    private const int ExcerptLength = 180;

    public async Task<QuestionPage> ListAsync(string? subjectSlug, int page, string? userId, CancellationToken cancellationToken = default)
    {
        var query = db.Questions.AsNoTracking().Include(q => q.Subject).Where(q => q.ArchivedAt == null);
        if (!string.IsNullOrEmpty(subjectSlug))
        {
            query = query.Where(q => q.Subject!.Slug == subjectSlug);
        }

        var total = await query.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        var questions = await query
            .OrderBy(q => q.Subject!.Order).ThenBy(q => q.Id)
            .Skip((page - 1) * PageSize).Take(PageSize)
            .ToListAsync(cancellationToken);

        var outcomes = await LatestOutcomesAsync(userId, questions.Select(q => q.Id).ToList(), cancellationToken);

        var items = questions.Select(q => new QuestionSummary(
            q.Id, q.Kind, Excerpt(q.Statement), q.Subject!.Title, q.Subject.Slug, q.Source,
            outcomes.GetValueOrDefault(q.Id, AttemptOutcome.NotAnswered), q.Subject.Block, QuestionCode.From(q.ExternalKey))).ToList();

        return new QuestionPage(items, page, totalPages, total);
    }

    /// <summary>Questão publicada (não arquivada).</summary>
    public async Task<Question?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        var question = await FindIncludingArchivedAsync(id, cancellationToken);
        return question is { ArchivedAt: null } ? question : null;
    }

    /// <summary>Inclui arquivadas: usado só na edição, por quem pode editar.</summary>
    public Task<Question?> FindIncludingArchivedAsync(int id, CancellationToken cancellationToken = default) =>
        db.Questions.AsNoTracking()
            .Include(q => q.Subject)
            .Include(q => q.Options.OrderBy(o => o.Letter))
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public Task<int> CountBySubjectAsync(int subjectId, CancellationToken cancellationToken = default) =>
        db.Questions.CountAsync(q => q.SubjectId == subjectId && q.ArchivedAt == null, cancellationToken);

    public Task<QuestionAttempt?> LatestAttemptAsync(string userId, int questionId, CancellationToken cancellationToken = default) =>
        db.QuestionAttempts.AsNoTracking()
            // Respostas dadas num desafio ficam no desafio: não liberam o gabarito na página da questão.
            .Where(a => a.UserId == userId && a.QuestionId == questionId && a.ChallengeId == null)
            // O id cresce a cada resposta: a maior é a mais recente (e funciona em qualquer banco).
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>Registra a resposta do aluno. Retorna nulo se a questão ou a alternativa não existirem.</summary>
    public async Task<AnswerResult?> AnswerAsync(string userId, int questionId, char letter, CancellationToken cancellationToken = default)
    {
        // Alternativas só existem em questões objetivas, então uma discursiva nunca passa daqui.
        var option = await db.QuestionOptions.AsNoTracking()
            .FirstOrDefaultAsync(o => o.QuestionId == questionId && o.Letter == char.ToUpperInvariant(letter), cancellationToken);
        if (option is null)
        {
            return null;
        }

        // Fora dos desafios, como no cálculo do XP (AchievementService).
        var before = await db.QuestionAttempts.AsNoTracking()
            .Where(a => a.UserId == userId && a.QuestionId == questionId)
            .Select(a => new { a.IsCorrect, a.ChallengeId })
            .ToListAsync(cancellationToken);
        var xp = XpRules.ForAnswer(before.Count > 0, before.Any(a => a.IsCorrect && a.ChallengeId == null), option.IsCorrect);

        var attempt = new QuestionAttempt
        {
            QuestionId = questionId,
            UserId = userId,
            OptionId = option.Id,
            IsCorrect = option.IsCorrect,
            AnsweredAt = clock.GetUtcNow(),
        };
        db.QuestionAttempts.Add(attempt);
        await db.SaveChangesAsync(cancellationToken);

        return new AnswerResult(attempt, option, xp);
    }

    public Task<DiscursiveAnswer?> LatestDiscursiveAnswerAsync(string userId, int questionId, CancellationToken cancellationToken = default) =>
        db.DiscursiveAnswers.AsNoTracking()
            .Where(a => a.UserId == userId && a.QuestionId == questionId)
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>Salva a resposta discursiva. Retorna nulo se a questão não for discursiva ou o texto for inválido.</summary>
    public async Task<DiscursiveAnswer?> AnswerDiscursiveAsync(string userId, int questionId, string? text, CancellationToken cancellationToken = default)
    {
        var trimmed = text?.Replace("\r\n", "\n").Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > DiscursiveAnswerMaxLength)
        {
            return null;
        }

        var isDiscursive = await db.Questions.AnyAsync(
            q => q.Id == questionId && q.Kind == QuestionKind.Discursive && q.ArchivedAt == null, cancellationToken);
        if (!isDiscursive)
        {
            return null;
        }

        var answer = new DiscursiveAnswer { QuestionId = questionId, UserId = userId, Text = trimmed, AnsweredAt = clock.GetUtcNow() };
        db.DiscursiveAnswers.Add(answer);
        await db.SaveChangesAsync(cancellationToken);
        return answer;
    }

    private async Task<Dictionary<int, AttemptOutcome>> LatestOutcomesAsync(string? userId, List<int> questionIds, CancellationToken cancellationToken)
    {
        if (userId is null || questionIds.Count == 0)
        {
            return [];
        }

        var attempts = await db.QuestionAttempts.AsNoTracking()
            .Where(a => a.UserId == userId && questionIds.Contains(a.QuestionId) && a.ChallengeId == null)
            .Select(a => new { a.QuestionId, a.IsCorrect, a.Id })
            .ToListAsync(cancellationToken);

        var outcomes = attempts
            .GroupBy(a => a.QuestionId)
            .ToDictionary(
                g => g.Key,
                g => g.MaxBy(a => a.Id)!.IsCorrect ? AttemptOutcome.Correct : AttemptOutcome.Wrong);

        var answeredDiscursives = await db.DiscursiveAnswers.AsNoTracking()
            .Where(a => a.UserId == userId && questionIds.Contains(a.QuestionId))
            .Select(a => a.QuestionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var questionId in answeredDiscursives)
        {
            outcomes[questionId] = AttemptOutcome.Answered;
        }

        return outcomes;
    }

    private static string Excerpt(string statement)
    {
        var flat = string.Join(' ', statement.Split((char[])['\n', '\r', '\t', ' '], StringSplitOptions.RemoveEmptyEntries));
        return flat.Length <= ExcerptLength ? flat : $"{flat[..ExcerptLength].TrimEnd()}…";
    }
}
