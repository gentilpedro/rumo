using EnadeAds.Blog;
using EnadeAds.Daily;
using EnadeAds.Data;
using EnadeAds.Questions;
using EnadeAds.Study;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Landing;

/// <summary>Quanto falta para a prova: o próximo dia de prova que ainda não passou.</summary>
public sealed record ExamCountdown(DateOnly? NextDay, int? DaysLeft)
{
    public bool IsToday => DaysLeft == 0;

    public bool IsOver => NextDay is null;

    public static ExamCountdown For(IEnumerable<DateOnly> examDays, DateOnly today)
    {
        var next = examDays.Where(day => day >= today).Order().Cast<DateOnly?>().FirstOrDefault();
        return new ExamCountdown(next, next is { } day ? day.DayNumber - today.DayNumber : null);
    }
}

public sealed record LandingTrack(Track Track, ExamCountdown Countdown);

public sealed record LandingNotice(Post Post, string? TrackLabel);

public sealed record LandingBlock(StudyBlock Block, IReadOnlyList<string> Subjects);

/// <summary>
/// Conteúdo da página inicial de quem ainda não escolheu a prova. Lê de todas as trilhas, então ignora o filtro
/// da trilha ativa do <see cref="ApplicationDbContext"/>.
/// </summary>
public sealed class LandingService(ApplicationDbContext db, TrackCatalog catalog, DailyQuestionService daily, LocalCalendar calendar)
{
    public async Task<IReadOnlyList<LandingTrack>> TracksAsync(CancellationToken cancellationToken = default)
    {
        var today = calendar.Today();
        return (await catalog.ListAsync(cancellationToken))
            .Select(track => new LandingTrack(track, ExamCountdown.For(track.ExamDays, today)))
            .ToList();
    }

    /// <summary>A trilha da amostra quando a pessoa não escolheu: a da prova mais próxima que ainda não passou.</summary>
    public static LandingTrack DefaultSample(IReadOnlyList<LandingTrack> tracks) =>
        tracks.Where(t => !t.Countdown.IsOver).OrderBy(t => t.Countdown.DaysLeft).ThenBy(t => t.Track.Order).FirstOrDefault()
        ?? tracks[0];

    /// <summary>A amostra é a questão do dia da trilha: a mesma para todo mundo, e muda a cada dia.</summary>
    public Task<Question?> SampleAsync(int trackId, CancellationToken cancellationToken = default) =>
        daily.TodayAsync(trackId, cancellationToken);

    /// <summary>A questão respondida no formulário. Só objetivas ativas, de qualquer trilha.</summary>
    public Task<Question?> FindAsync(int questionId, CancellationToken cancellationToken = default) =>
        db.Questions.IgnoreQueryFilters().AsNoTracking()
            .Include(q => q.Subject)
            .Include(q => q.Options.OrderBy(o => o.Letter))
            .FirstOrDefaultAsync(q => q.Id == questionId && q.Kind == QuestionKind.Objective && q.ArchivedAt == null, cancellationToken);

    /// <summary>Últimos avisos oficiais publicados, de todas as trilhas.</summary>
    public async Task<IReadOnlyList<LandingNotice>> NoticesAsync(int count = 3, CancellationToken cancellationToken = default)
    {
        var posts = await db.Posts.IgnoreQueryFilters().AsNoTracking()
            .Where(p => p.IsOfficial && p.PublishedAt != null && p.ArchivedAt == null && p.RemovedAt == null)
            .OrderByDescending(p => p.PublishedAt).ThenByDescending(p => p.Id)
            .Take(count)
            .ToListAsync(cancellationToken);
        var labels = (await catalog.ListAsync(cancellationToken)).ToDictionary(t => t.Id, t => t.Label);
        return posts.Select(p => new LandingNotice(p, p.TrackId is { } id ? labels.GetValueOrDefault(id) : null)).ToList();
    }

    /// <summary>As matérias do roadmap da trilha, agrupadas por bloco na ordem de estudo.</summary>
    public async Task<IReadOnlyList<LandingBlock>> RoadmapAsync(int trackId, CancellationToken cancellationToken = default)
    {
        var subjects = await db.RoadmapNodes.IgnoreQueryFilters().AsNoTracking()
            .Where(n => n.TrackId == trackId && n.Kind == NodeKind.Subject)
            .OrderBy(n => n.Order)
            .Select(n => new { n.Block, n.Title })
            .ToListAsync(cancellationToken);
        return subjects
            .GroupBy(s => s.Block)
            .Select(g => new LandingBlock(g.Key, g.Select(s => s.Title).ToList()))
            .ToList();
    }
}
