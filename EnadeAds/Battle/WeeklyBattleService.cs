using EnadeAds.Data;
using EnadeAds.Study;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Battle;

public sealed record BattleScore(Track Track, int Points, int People);

/// <param name="Monday">Primeiro dia da semana do placar.</param>
/// <param name="LastWeekWinner">Prova que venceu a semana anterior; nula se ninguém pontuou ou deu empate.</param>
public sealed record BattleBoard(DateOnly Monday, IReadOnlyList<BattleScore> Scores, Track? LastWeekWinner)
{
    public DateOnly Sunday => Monday.AddDays(6);
    public int MaxPoints => Scores.Count == 0 ? 0 : Scores.Max(s => s.Points);
}

/// <summary>
/// Batalha da semana entre as trilhas: cada questão acertada soma 1 ponto para a trilha dela, uma vez por pessoa e
/// questão na semana. Só números por trilha, sem nomes. Calculada das tentativas, sem nada gravado.
/// </summary>
public sealed class WeeklyBattleService(ApplicationDbContext db, TrackCatalog tracks, LocalCalendar calendar)
{
    public async Task<BattleBoard> GetAsync(CancellationToken cancellationToken = default)
    {
        var monday = MondayOf(calendar.Today());
        var all = await tracks.ListAsync(cancellationToken);
        var thisWeek = await PointsAsync(monday, cancellationToken);
        var lastWeek = await PointsAsync(monday.AddDays(-7), cancellationToken);

        var scores = all
            .Select(t => new BattleScore(t, thisWeek.GetValueOrDefault(t.Id).Points, thisWeek.GetValueOrDefault(t.Id).People))
            .OrderByDescending(s => s.Points).ThenBy(s => s.Track.Order)
            .ToList();

        var ranked = lastWeek.OrderByDescending(p => p.Value.Points).ToList();
        var winner = ranked.Count > 0 && ranked[0].Value.Points > 0 && (ranked.Count == 1 || ranked[1].Value.Points < ranked[0].Value.Points)
            ? all.FirstOrDefault(t => t.Id == ranked[0].Key)
            : null;

        return new BattleBoard(monday, scores, winner);
    }

    public static DateOnly MondayOf(DateOnly date) => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

    private async Task<Dictionary<int, (int Points, int People)>> PointsAsync(DateOnly monday, CancellationToken cancellationToken)
    {
        var (start, _) = calendar.DayRange(monday);
        var (_, end) = calendar.DayRange(monday.AddDays(6));

        // Acertos fora dos desafios, com a trilha pela matéria da questão (de qualquer trilha: por isso sem filtro).
        var hits = await db.QuestionAttempts.AsNoTracking()
            .Where(a => a.IsCorrect && a.ChallengeId == null && a.AnsweredAt >= start && a.AnsweredAt < end)
            .Join(db.Questions.IgnoreQueryFilters(), a => a.QuestionId, q => q.Id, (a, q) => new { a.UserId, a.QuestionId, q.Subject!.TrackId })
            .Distinct()
            .ToListAsync(cancellationToken);

        return hits.GroupBy(h => h.TrackId)
            .ToDictionary(g => g.Key, g => (g.Count(), g.Select(h => h.UserId).Distinct().Count()));
    }
}
