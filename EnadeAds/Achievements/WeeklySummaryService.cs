using EnadeAds.Battle;
using EnadeAds.Data;
using EnadeAds.Study;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Achievements;

/// <summary>Os números de uma semana de estudo de uma pessoa, em todas as provas.</summary>
public sealed record WeekSummary(DateOnly Monday, int Answered, int Correct, int DaysStudied, int Duels, string? BestSubject)
{
    public DateOnly Sunday => Monday.AddDays(6);
    public int Percent => Answered == 0 ? 0 : (int)Math.Round(100.0 * Correct / Answered);
}

/// <summary>
/// Resumo da semana (segunda a domingo, horário de Brasília) para Meu desempenho. Cada questão conta uma vez na
/// semana, pela primeira resposta dela na semana; desafios ficam de fora, como no XP.
/// </summary>
public sealed class WeeklySummaryService(ApplicationDbContext db, LocalCalendar calendar)
{
    public async Task<(WeekSummary This, WeekSummary Last)> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        var monday = WeeklyBattleService.MondayOf(calendar.Today());
        return (await WeekAsync(userId, monday, cancellationToken), await WeekAsync(userId, monday.AddDays(-7), cancellationToken));
    }

    private async Task<WeekSummary> WeekAsync(string userId, DateOnly monday, CancellationToken cancellationToken)
    {
        var (start, _) = calendar.DayRange(monday);
        var (_, end) = calendar.DayRange(monday.AddDays(6));

        var attempts = await db.QuestionAttempts.AsNoTracking()
            .Where(a => a.UserId == userId && a.ChallengeId == null && a.AnsweredAt >= start && a.AnsweredAt < end)
            .Join(db.Questions.IgnoreQueryFilters(), a => a.QuestionId, q => q.Id,
                (a, q) => new { a.Id, a.QuestionId, a.IsCorrect, a.AnsweredAt, Subject = q.Subject!.Title })
            .ToListAsync(cancellationToken);
        var firsts = attempts.GroupBy(a => a.QuestionId).Select(g => g.MinBy(a => a.Id)!).ToList();

        var discursiveDays = await db.DiscursiveAnswers.AsNoTracking()
            .Where(a => a.UserId == userId && a.AnsweredAt >= start && a.AnsweredAt < end)
            .Select(a => a.AnsweredAt)
            .ToListAsync(cancellationToken);
        var days = attempts.Select(a => a.AnsweredAt).Concat(discursiveDays)
            .Select(moment => DateOnly.FromDateTime(calendar.ToLocal(moment)))
            .Distinct()
            .Count();

        var duels = await db.DuelPlayers.AsNoTracking()
            .CountAsync(p => p.UserId == userId && p.FinishedAt >= start && p.FinishedAt < end, cancellationToken);

        var best = firsts.Where(a => a.IsCorrect)
            .GroupBy(a => a.Subject)
            .OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
            .Select(g => g.Key)
            .FirstOrDefault();

        return new WeekSummary(monday, firsts.Count, firsts.Count(a => a.IsCorrect), days, duels, best);
    }
}
