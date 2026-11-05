using EnadeAds.Data;
using EnadeAds.Questions;
using EnadeAds.Study;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Daily;

/// <summary>A questão de um dia numa trilha. Gravada no primeiro acesso do dia: é a mesma para todo mundo até a meia-noite.</summary>
public class DailyQuestion
{
    public int TrackId { get; set; }
    public DateOnly Date { get; set; }
    public int QuestionId { get; set; }
}

public sealed class DailyQuestionService(ApplicationDbContext db, QuestionBank bank, LocalCalendar calendar)
{
    /// <summary>Questões dos últimos dias que não voltam tão cedo, se o banco tiver outras.</summary>
    private const int NoRepeatDays = 60;

    public DateOnly Today => calendar.Today();

    /// <summary>A questão de hoje da trilha, sorteando e gravando se for o primeiro acesso do dia. Nulo se a trilha não tem objetivas.</summary>
    public async Task<Question?> TodayAsync(int trackId, CancellationToken cancellationToken = default)
    {
        var today = Today;
        var questionId = await db.DailyQuestions.AsNoTracking()
            .Where(d => d.TrackId == trackId && d.Date == today)
            .Select(d => (int?)d.QuestionId)
            .FirstOrDefaultAsync(cancellationToken) ?? await PickAsync(trackId, today, cancellationToken);

        return questionId is null ? null : await db.Questions.IgnoreQueryFilters().AsNoTracking()
            .Include(q => q.Subject)
            .Include(q => q.Options.OrderBy(o => o.Letter))
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);
    }

    /// <summary>Primeira resposta da pessoa a esta questão hoje, se houver.</summary>
    public Task<QuestionAttempt?> AnsweredTodayAsync(string userId, int questionId, CancellationToken cancellationToken = default)
    {
        var (start, end) = calendar.DayRange(Today);
        return db.QuestionAttempts.AsNoTracking()
            .Where(a => a.UserId == userId && a.QuestionId == questionId && a.ChallengeId == null && a.AnsweredAt >= start && a.AnsweredAt < end)
            .OrderBy(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>Quantas pessoas responderam hoje e quantas acertaram (vale a primeira resposta do dia de cada uma).</summary>
    public Task<CommunityScore> StatsAsync(int questionId, CancellationToken cancellationToken = default)
    {
        var (start, end) = calendar.DayRange(Today);
        return bank.CommunityAsync(questionId, start, end, cancellationToken);
    }

    private async Task<int?> PickAsync(int trackId, DateOnly today, CancellationToken cancellationToken)
    {
        var pool = await db.Questions.IgnoreQueryFilters().AsNoTracking()
            .Where(q => q.Subject!.TrackId == trackId && q.Kind == QuestionKind.Objective && q.ArchivedAt == null && q.Options.Any(o => o.IsCorrect))
            .Select(q => q.Id)
            .ToListAsync(cancellationToken);
        if (pool.Count == 0)
        {
            return null;
        }

        var since = today.AddDays(-NoRepeatDays);
        var recent = await db.DailyQuestions.AsNoTracking()
            .Where(d => d.TrackId == trackId && d.Date >= since)
            .Select(d => d.QuestionId)
            .ToListAsync(cancellationToken);
        var fresh = pool.Except(recent).ToList();
        var candidates = fresh.Count > 0 ? fresh : pool;

        db.DailyQuestions.Add(new DailyQuestion { TrackId = trackId, Date = today, QuestionId = candidates[Random.Shared.Next(candidates.Count)] });
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Duas pessoas abriram ao mesmo tempo na virada do dia: vale a que gravou primeiro.
            db.ChangeTracker.Clear();
        }

        return await db.DailyQuestions.AsNoTracking()
            .Where(d => d.TrackId == trackId && d.Date == today)
            .Select(d => (int?)d.QuestionId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
