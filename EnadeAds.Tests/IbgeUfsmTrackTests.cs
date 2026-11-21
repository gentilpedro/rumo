using System.Net;
using System.Text.Json;
using EnadeAds.Accounts;
using EnadeAds.Data;
using EnadeAds.MockExams;
using EnadeAds.Questions;
using EnadeAds.Study;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

public class IbgeUfsmSeedTests
{
    // Gabaritos definitivos oficiais; X = anulada (não entra). Nas da UFSM, as questões 38 a 40 têm gabarito próprio
    // em espanhol e em inglês.
    private static readonly Dictionary<string, string> IbgeKeys = new()
    {
        // Cesgranrio, questões 1 a 20 (Língua Portuguesa e Matemática); 2010 é o caderno branco (gabarito 1).
        ["2006"] = "AEBDCACBDEACBDBAECBD",
        ["2009"] = "CBCEADBEADBACECABDDD",
        ["2010"] = "BDACCCDDABDACABDBCDD",
    };

    private static readonly Dictionary<(int Year, int Prova), (string Common, string Es, string En)> UfsmKeys = new()
    {
        [(2026, 1)] = ("EBACBCDCEEEAABDDCEEAADBDBCBDBECAEDBAD", "BEB", "ECB"),
        [(2026, 2)] = ("ECEDAADXBAEAECDCBCBCEADEDDBCABCBAAECE", "BDA", "EDD"),
        [(2026, 3)] = ("CBCEDABAEACBCADBEDBDADABCCDBEDCCAEDEE", "BCE", "BDD"),
        [(2025, 1)] = ("EBDECDDAABDBDACCAACDBBCDEEBAEACAEEBEC", "DBC", "ECB"),
        [(2025, 2)] = ("ADECCDAACCEDBDCDDEBBCADEBABDBADECACBE", "EAC", "DBE"),
        [(2025, 3)] = ("DBADDBCXCEBDBABDBDEEACACACDEECEDDCAEB", "ACD", "BCE"),
    };

    private static SeedExam Load(string folder, string file) =>
        JsonSerializer.Deserialize<SeedExam>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data", "Seed", folder, file)), InepQuestionImporter.JsonOptions)!;

    private static Dictionary<string, int> SubjectsOf(IReadOnlyList<RoadmapNode> nodes) =>
        nodes.Where(n => n.Kind == NodeKind.Subject).ToDictionary(n => n.Slug, n => n.Id);

    private static IReadOnlyList<RoadmapNode> UfsmRoadmap(int prova) => prova switch
    {
        1 => UfsmRoadmapSeed.Prova1Nodes,
        2 => UfsmRoadmapSeed.Prova2Nodes,
        _ => UfsmRoadmapSeed.Prova3Nodes,
    };

    [Theory]
    [InlineData("2006", 5)]
    [InlineData("2009", 5)]
    [InlineData("2010", 4)]
    public void IbgeExams_MatchTheOfficialKeyAndHaveTheirNumberOfOptions(string year, int options)
    {
        var exam = Load("ibge", $"{year}-recenseador.json");
        var subjects = SubjectsOf(IbgeRoadmapSeed.Nodes);

        Assert.Equal("ibge-recenseador", exam.Track);
        Assert.Equal(20, exam.Questions.Count);
        Assert.All(exam.Questions.Select((q, i) => (q, i)), item =>
        {
            var (question, index) = item;
            Assert.Null(InepQuestionImporter.Validate(question, subjects));
            Assert.Equal(options, question.Options!.Count);
            Assert.Equal($"{IbgeKeys[year][index]}", question.Correct);
            // 1 a 10 são de Língua Portuguesa e 11 a 20 de Matemática, como no caderno.
            Assert.Equal(index < 10 ? "lingua-portuguesa" : "matematica", question.Subject);
            Assert.StartsWith($"IBGE {year} – Recenseador, questão {index + 1}", question.Source);
        });
    }

    [Fact]
    public void IbgeTechnicalQuestions_AreOursAndCiteTheBookletPage()
    {
        var exam = Load("ibge", "conhecimentos-tecnicos.json");
        var subjects = SubjectsOf(IbgeRoadmapSeed.Nodes);

        Assert.Equal(60, exam.Questions.Count);
        Assert.All(exam.Questions, question =>
        {
            Assert.Null(InepQuestionImporter.Validate(question, subjects));
            Assert.Equal("conhecimentos-tecnicos", question.Subject);
            Assert.Matches(@"^Autoria própria – Rumo, com base na apostila do IBGE .*\(p\. \d+(–\d+)?\)$", question.Source);
            Assert.Single(question.Options!, o => o.Explanation.StartsWith("Correta"));
        });
        // Letra correta espalhada entre A e E.
        Assert.All(exam.Questions.GroupBy(q => q.Correct), group => Assert.InRange(group.Count(), 6, 18));
    }

    [Theory]
    [InlineData(2025, 1)]
    [InlineData(2025, 2)]
    [InlineData(2025, 3)]
    [InlineData(2026, 1)]
    [InlineData(2026, 2)]
    [InlineData(2026, 3)]
    public void UfsmExams_MatchTheOfficialKeyAndTheEditalDistribution(int year, int prova)
    {
        var exam = Load("ufsm", $"{year}-prova-{prova}.json");
        var subjects = SubjectsOf(UfsmRoadmap(prova));
        var (common, es, en) = UfsmKeys[(year, prova)];

        Assert.Equal($"ufsm-{prova}", exam.Track);
        var expected = Enumerable.Range(1, 37).Where(n => common[n - 1] != 'X').Select(n => ($"ufsm-{year}-p{prova}-q{n:00}", common[n - 1]))
            .Concat(Enumerable.Range(38, 3).Select(n => ($"ufsm-{year}-p{prova}-q{n}-es", es[n - 38])))
            .Concat(Enumerable.Range(38, 3).Select(n => ($"ufsm-{year}-p{prova}-q{n}-en", en[n - 38])))
            .ToDictionary(x => x.Item1, x => x.Item2);

        Assert.Equal(expected.Keys.Order(), exam.Questions.Select(q => q.Key).Order());
        Assert.All(exam.Questions, question =>
        {
            Assert.Null(InepQuestionImporter.Validate(question, subjects));
            Assert.Equal($"{expected[question.Key]}", question.Correct);
            Assert.Equal(question.Key.EndsWith("-es") ? "espanhol" : question.Key.EndsWith("-en") ? "ingles" : question.Subject, question.Subject);
        });

        // Manual do candidato 2027, item 9.3: a mesma distribuição em todas as provas (uma anulada tira uma).
        var exam2027 = SubjectExam.Of(Tracks.Ufsm1)!;
        var annulled = common.Count(c => c == 'X');
        var counts = exam.Questions.Where(q => !q.Key.EndsWith("-en")).GroupBy(q => q.Subject).ToDictionary(g => g.Key, g => g.Count());
        Assert.All(exam2027.Quotas.Where(q => !q.IsForeignLanguage), quota =>
            Assert.InRange(counts.GetValueOrDefault(quota.Slugs[0]), quota.Count - annulled, quota.Count));
    }

    [Fact]
    public void Roadmaps_HaveTheSubjectsTheQuestionsUse()
    {
        Assert.Equal(IbgeRoadmapSeed.SubjectSlugs.Order(), SubjectsOf(IbgeRoadmapSeed.Nodes).Keys.Order());
        foreach (var prova in new[] { 1, 2, 3 })
        {
            IEnumerable<string> expected = prova == 3 ? [.. UfsmRoadmapSeed.ObjectiveSubjectSlugs, UfsmRoadmapSeed.EssaySlug] : UfsmRoadmapSeed.ObjectiveSubjectSlugs;
            Assert.Equal(expected.Order(), SubjectsOf(UfsmRoadmap(prova)).Keys.Order());
        }

        var all = RoadmapSeed.Nodes.Concat(EnemRoadmapSeed.Nodes).Concat(RedesRoadmapSeed.Nodes).Concat(PaveRoadmapSeed.Nodes)
            .Concat(IbgeRoadmapSeed.Nodes).Concat(UfsmRoadmapSeed.Nodes).ToList();
        Assert.Equal(all.Count, all.Select(n => n.Id).Distinct().Count());
    }

    [Fact]
    public void SubjectExams_FollowTheEditalAndShrinkInProportion()
    {
        var ibge = SubjectExam.Of(Tracks.IbgeRecenseador)!;
        var ufsm = SubjectExam.Of(Tracks.Ufsm3)!;

        Assert.Equal((60, 240), (ibge.FullCount, ibge.FullMinutes));
        Assert.Equal("15 de Língua Portuguesa, 15 de Matemática e 30 de Conhecimentos Técnicos.", ibge.Describe(MockExamSize.Full));
        Assert.Equal(new[] { 5, 5, 10 }, ibge.QuotasFor(MockExamSize.Short).Select(q => q.Count));
        Assert.Equal(60, ibge.MinutesFor(15));
        Assert.Equal((40, 180), (ufsm.FullCount, ufsm.FullMinutes));
        Assert.True(ufsm.HasLanguageChoice);
        Assert.All(ufsm.QuotasFor(MockExamSize.Short), q => Assert.True(q.Count >= 1));
        Assert.Null(SubjectExam.Of(Tracks.Enem));
    }
}

[Collection(AppCollection.Name)]
public class IbgeUfsmTrackTests(EnadeAdsFactory factory)
{
    private IServiceScope ScopeOf(int trackId)
    {
        var scope = factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<TrackScope>().Use(trackId);
        return scope;
    }

    [Theory]
    [InlineData(Tracks.IbgeRecenseador, 120)]
    [InlineData(Tracks.Ufsm1, 86)]
    [InlineData(Tracks.Ufsm2, 85)]
    [InlineData(Tracks.Ufsm3, 85)]
    public async Task Questions_AreImportedIntoTheirTrack(int trackId, int count)
    {
        using var scope = ScopeOf(trackId);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Equal(count, await db.Questions.CountAsync());
    }

    [Fact]
    public async Task FourOptionQuestions_AreImportedWithFourOptions()
    {
        using var scope = ScopeOf(Tracks.IbgeRecenseador);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var question = await db.Questions.Include(q => q.Options).SingleAsync(q => q.ExternalKey == "ibge-2010-rec-q01");

        Assert.Equal("ABCD", string.Concat(question.Options.OrderBy(o => o.Letter).Select(o => o.Letter)));
    }

    [Fact]
    public async Task FullIbgeMockExam_FollowsTheEditalQuotasAndTime()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "ibge.simulado@gmail.com", Roles.Student);
        using var scope = ScopeOf(Tracks.IbgeRecenseador);
        var exams = scope.ServiceProvider.GetRequiredService<MockExamService>();

        var started = await exams.StartAsync(student.Id, MockExamSize.Full);
        var exam = (await exams.FindAsync(student.Id, started.MockExamId!.Value))!;
        var subjects = exam.Questions.Select(q => q.Question!.Subject!.Slug).ToList();

        Assert.Equal(60, exam.Questions.Count);
        Assert.Equal(240, (int)(exam.DeadlineAt - exam.StartedAt).TotalMinutes);
        // Na ordem do edital: Português, Matemática, Conhecimentos Técnicos.
        Assert.All(subjects.Take(15), s => Assert.Equal("lingua-portuguesa", s));
        Assert.All(subjects.Skip(15).Take(15), s => Assert.Equal("matematica", s));
        Assert.All(subjects.Skip(30), s => Assert.Equal("conhecimentos-tecnicos", s));
    }

    [Fact]
    public async Task UfsmMockExam_UsesTheChosenLanguage()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "ufsm.simulado@gmail.com", Roles.Student);
        using var scope = ScopeOf(Tracks.Ufsm2);
        var exams = scope.ServiceProvider.GetRequiredService<MockExamService>();

        var started = await exams.StartAsync(student.Id, MockExamSize.Full, "espanhol");
        var exam = (await exams.FindAsync(student.Id, started.MockExamId!.Value))!;
        var subjects = exam.Questions.Select(q => q.Question!.Subject!.Slug).ToList();

        Assert.Equal(40, exam.Questions.Count);
        Assert.Equal(180, (int)(exam.DeadlineAt - exam.StartedAt).TotalMinutes);
        Assert.Equal(3, subjects.Count(s => s == "espanhol"));
        Assert.DoesNotContain("ingles", subjects);
    }

    [Fact]
    public async Task Pages_ShowTheNewTracks()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestAccounts.CreateConfirmedAsync(factory, "ufsm.paginas@gmail.com", Roles.Student);
        await TestAccounts.LoginAsync(client, "ufsm.paginas@gmail.com");
        await client.GetAsync("/trilha/ibge-recenseador");

        var roadmap = WebUtility.HtmlDecode(await client.GetStringAsync("/roadmap"));
        var mockExams = WebUtility.HtmlDecode(await client.GetStringAsync("/simulados"));
        var home = WebUtility.HtmlDecode(await client.GetStringAsync("/"));

        Assert.Contains("Conhecimentos Técnicos", roadmap);
        Assert.Contains("60 questões · 4h", mockExams);
        Assert.Contains("15 de Língua Portuguesa, 15 de Matemática e 30 de Conhecimentos Técnicos.", mockExams);
        Assert.Contains("Concurso público", home);
        Assert.Contains("UFSM 2027 · Prova 1 (PSS1)", home);
    }
}
