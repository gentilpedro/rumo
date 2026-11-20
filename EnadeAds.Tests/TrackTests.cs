using System.Net;
using EnadeAds.Accounts;
using EnadeAds.Data;
using EnadeAds.MockExams;
using EnadeAds.Questions;
using EnadeAds.Study;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

public class EnemRoadmapSeedTests
{
    private static readonly IReadOnlyList<RoadmapNode> Nodes = EnemRoadmapSeed.Nodes;

    [Fact]
    public void CoversTheFourAreasAndTheEssay()
    {
        var blocks = Nodes.Where(n => n.Kind == NodeKind.Subject).Select(n => n.Block).Distinct().ToList();

        Assert.Equal(
            [StudyBlock.Languages, StudyBlock.Humanities, StudyBlock.Essay, StudyBlock.NaturalSciences, StudyBlock.Mathematics],
            blocks);
        Assert.All(Nodes, n => Assert.Equal(Tracks.Enem, n.TrackId));
    }

    [Fact]
    public void IdsNeverCrossTheEnadeOnes()
    {
        var all = RoadmapSeed.Nodes.Concat(Nodes).ToList();

        Assert.Equal(all.Count, all.Select(n => n.Id).Distinct().Count());
        Assert.All(Nodes, n => Assert.True(n.Id > EnemRoadmapSeed.IdBase));
        Assert.Equal(Nodes.Count, Nodes.Select(n => n.Slug).Distinct().Count());
    }
}

[Collection(AppCollection.Name)]
public class TrackTests(EnadeAdsFactory factory)
{
    private HttpClient NewClient() => factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private HttpClient EnemClient()
    {
        var client = NewClient();
        client.DefaultRequestHeaders.Add("Cookie", $"{TrackScope.CookieName}=enem");
        return client;
    }

    private (IServiceScope Scope, ApplicationDbContext Db) EnemScope()
    {
        var scope = factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<TrackScope>().Use(Tracks.Enem);
        return (scope, scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    [Fact]
    public async Task EachTrackSeesOnlyItsRoadmapAndQuestions()
    {
        var (scope, enem) = EnemScope();
        using (scope)
        {
            using var defaultScope = factory.Services.CreateScope();
            var enade = defaultScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var enemSubjects = await enem.RoadmapNodes.Where(n => n.Kind == NodeKind.Subject).Select(n => n.Slug).ToListAsync();
            var enadeSubjects = await enade.RoadmapNodes.Where(n => n.Kind == NodeKind.Subject).Select(n => n.Slug).ToListAsync();
            var enemQuestions = await enem.Questions.Select(q => q.ExternalKey).ToListAsync();

            Assert.Contains("redacao", enemSubjects);
            Assert.DoesNotContain("banco-de-dados", enemSubjects);
            Assert.Contains("banco-de-dados", enadeSubjects);
            Assert.DoesNotContain("redacao", enadeSubjects);

            // As 12 questões de autoria própria do ENEM foram importadas na subida, e só elas aparecem nessa trilha.
            Assert.True(enemQuestions.Count(k => k != null && k.StartsWith("rumo-enem-")) >= 12);
            Assert.DoesNotContain(enemQuestions, k => k != null && k.StartsWith("enade-"));
            Assert.DoesNotContain(await enade.Questions.Select(q => q.ExternalKey).ToListAsync(), k => k != null && k.StartsWith("rumo-enem-"));
        }
    }

    [Fact]
    public async Task Visitor_WithoutAChoiceSeesTheChooserAndNoTrackIsPreselected()
    {
        // Host sem a trilha padrão dos testes: é o que acontece em produção para quem chega pela primeira vez.
        using var fresh = factory.WithWebHostBuilder(builder => builder.UseSetting("Tracks:Default", ""));
        var client = fresh.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var home = WebUtility.HtmlDecode(await client.GetStringAsync("/"));
        var roadmap = await client.GetAsync("/roadmap?topico=pilhas");
        var chooser = WebUtility.HtmlDecode(await client.GetStringAsync(roadmap.Headers.Location!.ToString()));

        Assert.Contains("Para qual prova você vai estudar?", home);
        Assert.Contains("ENADE 2026 · ADS", home);
        Assert.Contains("ENEM 2026", home);
        Assert.Matches("class=\"track-label\"[^>]*>Escolher prova</span>", home);
        Assert.DoesNotContain("Comunidade", home);
        Assert.DoesNotContain("aria-current=\"true\"", home);

        Assert.Equal(HttpStatusCode.Redirect, roadmap.StatusCode);
        Assert.Equal("/?volta=%2Froadmap%3Ftopico%3Dpilhas", roadmap.Headers.Location?.ToString());
        Assert.Contains("trilha/enem?volta=%2Froadmap%3Ftopico%3Dpilhas", chooser);
    }

    [Fact]
    public async Task Switch_SetsTheCookieAndOnlyReturnsToThisSite()
    {
        var client = NewClient();

        var ok = await client.GetAsync("/trilha/enem?volta=%2Fquestoes");
        var outside = await client.GetAsync("/trilha/enem?volta=%2F%2Fexemplo.com");
        var unknown = await client.GetAsync("/trilha/nao-existe");

        Assert.Equal(HttpStatusCode.Redirect, ok.StatusCode);
        Assert.Equal("/questoes", ok.Headers.Location?.ToString());
        Assert.Contains(ok.Headers.GetValues("Set-Cookie"), c => c.StartsWith($"{TrackScope.CookieName}=enem") && c.Contains("httponly"));
        Assert.Equal("/", outside.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
    }

    [Fact]
    public async Task Pages_ShowTheBrandWithTheActiveTrack()
    {
        // A faixa do semestre só existe até a prova. Para o teste não depender do dia em que roda, os dois
        // domingos do ENEM vão para as próximas semanas e voltam para as datas da seed no fim.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await SetEnemDatesAsync(today.AddDays(7), today.AddDays(14));
        string home, roadmap, enadeHome;
        try
        {
            home = WebUtility.HtmlDecode(await EnemClient().GetStringAsync("/"));
            roadmap = WebUtility.HtmlDecode(await EnemClient().GetStringAsync("/roadmap"));
            enadeHome = WebUtility.HtmlDecode(await NewClient().GetStringAsync("/"));
        }
        finally
        {
            var seed = Tracks.Seed.Single(t => t.Id == Tracks.Enem);
            await SetEnemDatesAsync(seed.ExamDate, seed.SecondExamDate);
        }

        Assert.Contains(">Rumo</a>", home);
        // A faixa do semestre do ENEM mostra os dois domingos de prova, sem vazar código Razor.
        Assert.Contains("ENEM · 1º dia", home);
        Assert.Contains("ENEM · 2º dia", home);
        Assert.DoesNotContain("week.Challenges", home);
        Assert.Matches("class=\"track-label\"[^>]*>ENEM</span>", home);
        Assert.Contains("Roadmap ENEM 2026", roadmap);
        Assert.Contains("Redação dissertativo-argumentativa", roadmap);
        Assert.DoesNotContain("Banco de dados", roadmap);
        Assert.Matches("class=\"track-label\"[^>]*>ENADE ADS</span>", enadeHome);
    }

    private async Task SetEnemDatesAsync(DateOnly first, DateOnly? second)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var enem = await db.Tracks.SingleAsync(t => t.Id == Tracks.Enem);
        (enem.ExamDate, enem.SecondExamDate) = (first, second);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task EnemMockExam_IsBalancedAcrossAreasAndSeparateFromTheEnadeOne()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "trilha.simulado@gmail.com", Roles.Student);

        var (scope, _) = EnemScope();
        using (scope)
        {
            var enemExams = scope.ServiceProvider.GetRequiredService<MockExamService>();
            var started = await enemExams.StartAsync(student.Id, MockExamSize.Short);
            var exam = await enemExams.FindAsync(student.Id, started.MockExamId!.Value);

            Assert.Equal(StartOutcome.Started, started.Outcome);
            Assert.Equal(Tracks.Enem, exam!.TrackId);
            Assert.All(exam.Questions, q => Assert.Equal(QuestionKind.Objective, q.Question!.Kind));
            Assert.Equal(
                [StudyBlock.Languages, StudyBlock.Humanities, StudyBlock.NaturalSciences, StudyBlock.Mathematics],
                exam.Questions.Select(q => q.Question!.Subject!.Block).Distinct().Order());
            Assert.Equal(MockExamFormat.MinutesFor(MockExamStyle.Enem, exam.Questions.Count), (int)(exam.DeadlineAt - exam.StartedAt).TotalMinutes);
        }

        // Um simulado aberto por trilha: o do ENEM não impede começar um do ENADE.
        using var enadeScope = factory.Services.CreateScope();
        var enade = await enadeScope.ServiceProvider.GetRequiredService<MockExamService>().StartAsync(student.Id, MockExamSize.Short);
        Assert.Equal(StartOutcome.Started, enade.Outcome);
    }

    [Fact]
    public async Task ExamDates_BelongToEachTrack()
    {
        var (scope, _) = EnemScope();
        using (scope)
        {
            var enem = await scope.ServiceProvider.GetRequiredService<TrackCatalog>().CurrentAsync();
            var date = await scope.ServiceProvider.GetRequiredService<ExamSettings>().ExamDateAsync(new DateOnly(2026, 11, 10));

            Assert.Equal([new DateOnly(2026, 11, 8), new DateOnly(2026, 11, 15)], enem.ExamDays);
            // Entre os dois domingos, a contagem vai até o segundo dia.
            Assert.Equal(new DateOnly(2026, 11, 15), date);
        }
    }
}
