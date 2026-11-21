using EnadeAds.Battle;
using EnadeAds.Data;
using EnadeAds.Questions;
using EnadeAds.Study;
using Microsoft.EntityFrameworkCore;

namespace EnadeAds.Challenges;

/// <summary>
/// Publica o "Desafio da semana" de cada prova: questões objetivas sorteadas do banco dela, de segunda a domingo,
/// em nome da plataforma (sem autor). Assim nenhuma prova fica sem desafio à espera de um professor montar um.
/// Um desafio automático por prova e por semana: chamar de novo na mesma semana não cria outro.
/// </summary>
public sealed class WeeklyChallengePublisher(ApplicationDbContext db, LocalCalendar calendar, TimeProvider clock, ILogger<WeeklyChallengePublisher> logger)
{
    public const int QuestionCount = 5;

    /// <summary>Por quantas semanas uma questão descansa depois de cair num desafio automático, se o banco tiver outras.</summary>
    public const int NoRepeatWeeks = 8;

    /// <summary>Como o desafio automático se reconhece no banco: sem autor e com este começo de título.</summary>
    public const string TitlePrefix = "Desafio da semana";

    public const string Description =
        "Cinco questões sorteadas do banco da prova, revezando entre as áreas. Responda até domingo e veja depois como a turma foi em cada uma.";

    /// <summary>Cria o desafio desta semana nas provas que ainda não têm. Devolve quantos criou.</summary>
    public async Task<int> PublishAsync(CancellationToken cancellationToken = default)
    {
        var monday = WeeklyBattleService.MondayOf(calendar.Today());
        var tracks = await db.Tracks.AsNoTracking().OrderBy(t => t.Order).Select(t => t.Id).ToListAsync(cancellationToken);
        // As consultas ignoram o filtro de trilha: este serviço roda fora de uma requisição e passa por todas as provas.
        var done = await db.Challenges.IgnoreQueryFilters().AsNoTracking()
            .Where(c => c.StartsOn == monday && c.AuthorId == null && c.Title.StartsWith(TitlePrefix))
            .Select(c => c.TrackId)
            .ToListAsync(cancellationToken);

        var created = 0;
        foreach (var trackId in tracks.Except(done))
        {
            var picked = await PickAsync(trackId, monday, cancellationToken);
            if (picked.Count < QuestionCount)
            {
                // Prova com banco pequeno demais fica sem desafio automático.
                continue;
            }

            db.Challenges.Add(new Challenge
            {
                TrackId = trackId,
                Title = $"{TitlePrefix} · {monday:dd/MM}",
                Description = Description,
                StartsOn = monday,
                EndsOn = monday.AddDays(6),
                AuthorId = null,
                CreatedAt = clock.GetUtcNow(),
                Questions = picked.Select((id, index) => new ChallengeQuestion { QuestionId = id, Order = index + 1 }).ToList(),
            });
            created++;
        }

        if (created > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("{Count} desafios da semana de {Monday} publicados.", created, monday);
        }

        return created;
    }

    /// <summary>Sorteia as questões revezando entre as áreas da prova, deixando de fora as dos últimos desafios automáticos.</summary>
    private async Task<List<int>> PickAsync(int trackId, DateOnly monday, CancellationToken cancellationToken)
    {
        var pool = await db.Questions.IgnoreQueryFilters().AsNoTracking()
            .Where(q => q.Subject!.TrackId == trackId && q.Kind == QuestionKind.Objective && q.ArchivedAt == null && q.Options.Any(o => o.IsCorrect))
            .Select(q => new { q.Id, q.Subject!.Block })
            .ToListAsync(cancellationToken);

        var since = monday.AddDays(-7 * NoRepeatWeeks);
        var recent = await db.Challenges.IgnoreQueryFilters().AsNoTracking()
            .Where(c => c.TrackId == trackId && c.AuthorId == null && c.StartsOn >= since && c.Title.StartsWith(TitlePrefix))
            .SelectMany(c => c.Questions.Select(q => q.QuestionId))
            .ToListAsync(cancellationToken);

        var fresh = pool.Where(q => !recent.Contains(q.Id)).ToList();
        var candidates = fresh.Count >= QuestionCount ? fresh : pool;

        var byArea = candidates.GroupBy(q => q.Block).OrderBy(g => g.Key)
            .Select(g => new Queue<int>(g.Select(q => q.Id).OrderBy(_ => Random.Shared.Next())))
            .ToList();
        var picked = new List<int>();
        while (picked.Count < QuestionCount && byArea.Any(queue => queue.Count > 0))
        {
            foreach (var queue in byArea.Where(queue => queue.Count > 0))
            {
                if (picked.Count == QuestionCount)
                {
                    break;
                }

                picked.Add(queue.Dequeue());
            }
        }

        return picked;
    }
}

/// <summary>
/// Chama o <see cref="WeeklyChallengePublisher"/> quando o app sobe e de hora em hora: a virada da semana não
/// depende de deploy nem de alguém abrir uma página. É o único que publica, então não há duas publicações ao mesmo tempo.
/// </summary>
public sealed class WeeklyChallengeWorker(IServiceScopeFactory scopes, IConfiguration configuration, ILogger<WeeklyChallengeWorker> logger) : BackgroundService
{
    public static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Ligado por padrão; os testes desligam para publicar na hora que quiserem.
        if (!configuration.GetValue("Challenges:AutoWeekly", true))
        {
            return;
        }

        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<WeeklyChallengePublisher>().PublishAsync(stoppingToken);
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                // Uma falha (banco fora do ar, por exemplo) não derruba o app: tenta de novo na próxima hora.
                logger.LogError(error, "Falha ao publicar os desafios da semana.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
