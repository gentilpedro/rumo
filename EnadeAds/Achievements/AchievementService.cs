using EnadeAds.Data;
using EnadeAds.Duels;
using EnadeAds.MockExams;
using EnadeAds.Study;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Achievements;

public sealed record Achievement(string Key, string Name, string Description, bool Unlocked);

/// <summary>De onde veio o XP, para a pessoa entender a conta.</summary>
public sealed record XpLine(string Label, int Count, int Xp);

public sealed record XpReport(
    int Xp,
    Level Level,
    int CurrentStreak,
    bool StudiedToday,
    int LongestStreak,
    IReadOnlyList<XpLine> Breakdown,
    IReadOnlyList<Achievement> Achievements)
{
    public int UnlockedCount => Achievements.Count(a => a.Unlocked);
}

/// <summary>
/// XP, nível, sequência de dias e conquistas, calculados do que a pessoa já fez em todas as trilhas. Nada é
/// gravado: o histórico de antes desta funcionalidade conta, e corrigir uma regra vale para todo mundo na hora.
/// </summary>
public sealed class AchievementService(ApplicationDbContext db, LocalCalendar calendar)
{
    public async Task<XpReport> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        var attempts = await db.QuestionAttempts.AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => new { a.QuestionId, a.IsCorrect, a.AnsweredAt, a.ChallengeId })
            .ToListAsync(cancellationToken);

        var discursives = await db.DiscursiveAnswers.AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => new { a.QuestionId, a.AnsweredAt })
            .ToListAsync(cancellationToken);

        var mockExams = await db.MockExams.IgnoreQueryFilters().AsNoTracking()
            .Where(m => m.UserId == userId && m.SubmittedAt != null)
            .Select(m => new { m.Size, SubmittedAt = m.SubmittedAt!.Value, m.CorrectCount, m.ObjectiveCount })
            .ToListAsync(cancellationToken);

        var challenges = await db.ChallengeSubmissions.AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => s.SubmittedAt)
            .ToListAsync(cancellationToken);

        // Para saber quem venceu, o placar inteiro de cada duelo que a pessoa enviou.
        var duelPlayers = await db.DuelPlayers.AsNoTracking()
            .Where(p => db.DuelPlayers.Any(mine => mine.DuelId == p.DuelId && mine.UserId == userId && mine.FinishedAt != null))
            .ToListAsync(cancellationToken);
        var myDuels = duelPlayers.Where(p => p.UserId == userId).ToList();
        var duelWins = duelPlayers.GroupBy(p => p.DuelId)
            // Vitória com zero acertos não conta: empate em nada não é vitória.
            .Count(g => DuelService.Rank(g) is { Count: >= 2 } ranked && ranked[0].UserId == userId && ranked[0].CorrectCount > 0);

        var topics = await db.RoadmapProgress.AsNoTracking()
            .Where(p => p.UserId == userId && p.Status == NodeStatus.Done)
            .Select(p => p.UpdatedAt)
            .ToListAsync(cancellationToken);

        // Acerto dentro de desafio aberto não pode virar XP: mostraria o gabarito antes da hora.
        var practice = attempts.Where(a => a.ChallengeId is null).ToList();
        var answered = attempts.Select(a => a.QuestionId).Distinct().Count();
        var correct = practice.Where(a => a.IsCorrect).Select(a => a.QuestionId).Distinct().Count();
        var discursiveCount = discursives.Select(a => a.QuestionId).Distinct().Count();
        var fullExams = mockExams.Count(m => m.Size == MockExamSize.Full);

        var breakdown = new List<XpLine>
        {
            new("Questões respondidas", answered, answered * XpRules.FirstAnswer),
            new("Questões acertadas", correct, correct * XpRules.FirstCorrect),
            new("Discursivas", discursiveCount, discursiveCount * XpRules.Discursive),
            new("Simulados entregues", mockExams.Count, mockExams.Count * XpRules.MockExam + fullExams * XpRules.FullMockExamBonus),
            new("Desafios enviados", challenges.Count, challenges.Count * XpRules.Challenge),
            new("Duelos jogados", myDuels.Count, myDuels.Count * XpRules.Duel),
            new("Duelos vencidos", duelWins, duelWins * XpRules.DuelWin),
            new("Tópicos do roadmap concluídos", topics.Count, topics.Count * XpRules.RoadmapTopic),
        };
        var xp = breakdown.Sum(line => line.Xp);

        var moments = attempts.Select(a => a.AnsweredAt)
            .Concat(discursives.Select(a => a.AnsweredAt))
            .Concat(mockExams.Select(m => m.SubmittedAt))
            .Concat(challenges)
            .Concat(myDuels.Select(p => p.FinishedAt!.Value))
            .Concat(topics)
            .Select(calendar.ToLocal)
            .ToList();
        var days = moments.Select(DateOnly.FromDateTime).ToHashSet();
        var today = calendar.Today();
        var (current, longest) = Streaks(days, today);

        Achievement Badge(string key, string name, string description, bool unlocked) => new(key, name, description, unlocked);
        var achievements = new List<Achievement>
        {
            Badge("primeira-resposta", "Primeira resposta", "Responda uma questão objetiva.", answered >= 1),
            Badge("dez-acertos", "Dez na conta", "Acerte 10 questões diferentes.", correct >= 10),
            Badge("cem-acertos", "Cem na conta", "Acerte 100 questões diferentes.", correct >= 100),
            Badge("escritor", "Escritor", "Responda uma questão discursiva.", discursiveCount >= 1),
            Badge("tres-dias", "Três dias seguidos", "Estude três dias seguidos.", longest >= 3),
            Badge("uma-semana", "Uma semana sem parar", "Estude sete dias seguidos.", longest >= 7),
            Badge("duelista", "Duelista", "Jogue um duelo até o fim.", myDuels.Count >= 1),
            Badge("campeao", "Campeão", "Vença um duelo com pelo menos dois jogadores, acertando ao menos uma.", duelWins >= 1),
            Badge("maratonista", "Maratonista", "Entregue um simulado completo, do tamanho da prova.", fullExams >= 1),
            Badge("gabarito", "Gabaritou", "Acerte todas as objetivas de um simulado de 10 questões ou mais.",
                mockExams.Any(m => m.ObjectiveCount >= 10 && m.CorrectCount == m.ObjectiveCount)),
            Badge("trilheiro", "No rumo certo", "Conclua 10 tópicos do roadmap.", topics.Count >= 10),
            Badge("madrugador", "Madrugador", "Estude antes das 7h.", moments.Any(m => m.Hour < 7 && m.Hour >= 4)),
            Badge("coruja", "Coruja", "Estude depois das 23h.", moments.Any(m => m.Hour >= 23 || m.Hour < 4)),
        };

        return new XpReport(xp, Level.Of(xp), current, days.Contains(today), longest, breakdown, achievements);
    }

    /// <summary>
    /// Sequência atual e a mais longa, em dias com alguma atividade. A atual continua viva até o fim do dia
    /// seguinte: quem estudou ontem e ainda não hoje não perde a sequência de manhã.
    /// </summary>
    public static (int Current, int Longest) Streaks(IReadOnlySet<DateOnly> days, DateOnly today)
    {
        var longest = 0;
        foreach (var day in days.Where(d => !days.Contains(d.AddDays(-1))))
        {
            var length = 1;
            while (days.Contains(day.AddDays(length)))
            {
                length++;
            }

            longest = Math.Max(longest, length);
        }

        var start = days.Contains(today) ? today : today.AddDays(-1);
        var current = 0;
        while (days.Contains(start.AddDays(-current)))
        {
            current++;
        }

        return (current, longest);
    }
}
