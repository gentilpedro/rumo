using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using EnadeAds.Accounts;
using EnadeAds.Data;
using EnadeAds.MockExams;
using EnadeAds.Questions;
using EnadeAds.Study;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

public class PaveRoadmapSeedTests
{
    public static TheoryData<int, int> Stages => new()
    {
        { 1, Tracks.Pave1 },
        { 2, Tracks.Pave2 },
        { 3, Tracks.Pave3 },
    };

    private static IReadOnlyList<RoadmapNode> NodesOf(int stage) => stage switch
    {
        1 => PaveRoadmapSeed.Stage1Nodes,
        2 => PaveRoadmapSeed.Stage2Nodes,
        _ => PaveRoadmapSeed.Stage3Nodes,
    };

    [Theory]
    [MemberData(nameof(Stages))]
    public void EachStageHasTheDisciplinesOfTheMatrixInTheOrderOfTheExam(int stage, int trackId)
    {
        var nodes = NodesOf(stage);
        var subjects = nodes.Where(n => n.Kind == NodeKind.Subject).OrderBy(n => n.Order).ToList();

        // Matriz de Referência PAVE 2026: as mesmas disciplinas nas três etapas; só a Etapa 3 tem redação.
        IEnumerable<string> expected = stage == 3 ? [.. PaveRoadmapSeed.ObjectiveSubjectSlugs, PaveRoadmapSeed.EssaySlug] : PaveRoadmapSeed.ObjectiveSubjectSlugs;
        Assert.Equal(expected, subjects.Select(s => s.Slug));

        // Ordem das áreas na prova (edital, item 12.5).
        StudyBlock[] blocks = [StudyBlock.NaturalSciences, StudyBlock.Humanities, StudyBlock.Mathematics, StudyBlock.Languages];
        Assert.Equal(stage == 3 ? [.. blocks, StudyBlock.Essay] : blocks, subjects.Select(s => s.Block).Distinct());

        Assert.All(subjects, s => Assert.Contains(nodes, t => t.ParentId == s.Id));
        Assert.All(nodes, n => Assert.Equal(trackId, n.TrackId));
        Assert.Equal(nodes.Count, nodes.Select(n => n.Slug).Distinct().Count());
        Assert.All(nodes, n => Assert.InRange(n.Summary.Length, 1, 2000));
    }

    [Fact]
    public void IdsNeverCrossTheOtherTracks()
    {
        var all = RoadmapSeed.Nodes.Concat(EnemRoadmapSeed.Nodes).Concat(RedesRoadmapSeed.Nodes).Concat(PaveRoadmapSeed.Nodes).ToList();

        Assert.Equal(all.Count, all.Select(n => n.Id).Distinct().Count());
        Assert.All(PaveRoadmapSeed.Stage1Nodes, n => Assert.InRange(n.Id, PaveRoadmapSeed.Stage1IdBase + 1, PaveRoadmapSeed.Stage2IdBase - 1));
        Assert.All(PaveRoadmapSeed.Stage2Nodes, n => Assert.InRange(n.Id, PaveRoadmapSeed.Stage2IdBase + 1, PaveRoadmapSeed.Stage3IdBase - 1));
        Assert.All(PaveRoadmapSeed.Stage3Nodes, n => Assert.True(n.Id > PaveRoadmapSeed.Stage3IdBase));
    }

    [Fact]
    public void ExamDatesFollowTheEdital()
    {
        // Edital CRA nº 16/2026, item 11.1: Etapa 1 no sábado, Etapas 2 e 3 no domingo.
        Assert.Equal(new DateOnly(2026, 12, 5), Tracks.Seed.Single(t => t.Id == Tracks.Pave1).ExamDate);
        Assert.Equal(new DateOnly(2026, 12, 6), Tracks.Seed.Single(t => t.Id == Tracks.Pave2).ExamDate);
        Assert.Equal(new DateOnly(2026, 12, 6), Tracks.Seed.Single(t => t.Id == Tracks.Pave3).ExamDate);
        Assert.All(Tracks.Seed.Where(t => t.Slug.StartsWith("pave-")), t =>
        {
            Assert.Equal(MockExamStyle.Pave, t.MockExamStyle);
            Assert.Equal(TrackLevel.HighSchool, t.Level);
            Assert.Null(t.SecondExamDate);
        });
    }
}

public class PaveScoringTests
{
    [Fact]
    public void Format_FollowsTheExam()
    {
        var full = MockExamFormat.Of(MockExamStyle.Pave, MockExamSize.Full);
        var quotas = MockExamFormat.PaveQuotas(MockExamSize.Full).ToDictionary(q => q.Part, q => q.Count);

        // 32 objetivas em 3h30: 9 de Natureza, 9 de Humanas, 5 de Matemática e 9 de Linguagens, das quais 3 de língua estrangeira.
        Assert.Equal((0, 32, 0), (full.GeneralEducation, full.Specific, full.Discursive));
        Assert.Equal(210, MockExamFormat.MinutesFor(MockExamStyle.Pave, 32));
        Assert.Equal((9, 9, 5, 9, 3), (quotas[PavePart.NaturalSciences], quotas[PavePart.Humanities], quotas[PavePart.Mathematics],
            quotas[PavePart.Languages] + quotas[PavePart.ForeignLanguage], quotas[PavePart.ForeignLanguage]));
        Assert.All(MockExamFormat.For(MockExamStyle.Pave), format =>
            Assert.Equal(format.Specific, MockExamFormat.PaveQuotas(format.Size).Sum(q => q.Count)));
    }

    [Theory]
    // Etapas 1 e 2: 3,125 por acerto e -0,3125 por erro (edital, item 13.1).
    [InlineData(Tracks.Pave1, 32, 0, 100)]
    [InlineData(Tracks.Pave1, 20, 8, 60)]
    [InlineData(Tracks.Pave2, 0, 32, -10)]
    [InlineData(Tracks.Pave2, 0, 0, 0)]
    // Etapa 3: 2,6875 por acerto e -0,26875 por erro (item 13.2); os outros 14 pontos são da redação.
    [InlineData(Tracks.Pave3, 32, 0, 86)]
    [InlineData(Tracks.Pave3, 10, 10, 24.1875)]
    public void Score_DiscountsTenPercentOfAHitForEachMiss(int trackId, int correct, int wrong, double expected)
    {
        // Atributo não aceita decimal; os valores são frações exatas em binário.
        Assert.Equal((decimal)expected, PaveScoring.Score(trackId, correct, wrong));
    }

    [Fact]
    public void Format_UsesDecimalComma()
    {
        Assert.Equal("24,19", PaveScoring.Format(24.1875m));
        Assert.Equal("3,13", PaveScoring.Format(3.125m));
        Assert.Equal("60", PaveScoring.Format(60m));
        Assert.Equal(100m, PaveScoring.PointsPerQuestion(Tracks.Pave3) * 32 + PaveScoring.EssayMaxPoints);
    }
}

/// <summary>Confere os arquivos de Data/Seed/pave contra os gabaritos definitivos publicados pela UFPel.</summary>
public class PaveSeedFileTests
{
    // GABARITO-DEFINITIVO-PAVE-2024-1.pdf e GABARITO-DEFINITIVO-PAVE-2025.pdf (wp.ufpel.edu.br/pave/provas-anteriores),
    // questões 1 a 32 de cada etapa; '-' = anulada.
    private static readonly Dictionary<(int Year, int Stage), string> AnswerKeys = new()
    {
        [(2024, 1)] = "DDBCAEBACECACDABECBEDDCDBECBDEA-",
        [(2024, 2)] = "CBBE-EACADDCAEECABBADDABCCABEDDA",
        [(2024, 3)] = "CACBBDEAEEDAEBBACCDE-EAABCDCEBAA",
        [(2025, 1)] = "CBACEDBEACEDDCEEAB-BDAEBCCDBADAC",
        [(2025, 2)] = "ACCDDEECABCADEAABD-ABDDBCCEBCBA-",
        [(2025, 3)] = "EEDCDAABDECCEDAADBECCAEDBBADEABC",
    };

    private static readonly Dictionary<string, string> Languages = new()
    {
        ["es"] = PaveRoadmapSeed.SpanishSlug,
        ["en"] = PaveRoadmapSeed.EnglishSlug,
    };

    private static string WwwRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "EnadeAds", "wwwroot"));

    private sealed record Item(int Year, int Stage, int Number, string? Language, SeedQuestion Question);

    private static readonly Regex KeyPattern = new(@"^pave-(?<year>\d{4})-e(?<stage>[123])-q(?<number>\d{2})(-(?<language>es|en))?$");

    private static List<Item> AllQuestions() =>
        Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "pave"), "*.json")
            .Select(file => JsonSerializer.Deserialize<SeedExam>(File.ReadAllText(file), InepQuestionImporter.JsonOptions)!)
            .SelectMany(exam => exam.Questions.Select(q =>
            {
                var key = KeyPattern.Match(q.Key);
                Assert.True(key.Success, $"chave fora do padrão: {q.Key}");
                Assert.Equal($"pave-{key.Groups["stage"].Value}", exam.Track);
                Assert.Equal(exam.Year, int.Parse(key.Groups["year"].Value));
                return new Item(exam.Year, int.Parse(key.Groups["stage"].Value), int.Parse(key.Groups["number"].Value),
                    key.Groups["language"].Success ? key.Groups["language"].Value : null, q);
            }))
            .ToList();

    private static IReadOnlyList<RoadmapNode> RoadmapOf(int stage) => stage switch
    {
        1 => PaveRoadmapSeed.Stage1Nodes,
        2 => PaveRoadmapSeed.Stage2Nodes,
        _ => PaveRoadmapSeed.Stage3Nodes,
    };

    [Fact]
    public void EveryExamIsCompleteExceptForTheAnnulledQuestions()
    {
        var keys = AllQuestions().Select(q => q.Question.Key).ToList();
        var expected = AnswerKeys.SelectMany(exam => Enumerable.Range(1, 32)
            .Where(number => exam.Value[number - 1] != '-')
            // As questões 30 a 32 têm uma versão em espanhol e outra em inglês, com o mesmo gabarito.
            .SelectMany(number => number >= 30
                ? Languages.Keys.Select(language => $"pave-{exam.Key.Year}-e{exam.Key.Stage}-q{number:00}-{language}")
                : [$"pave-{exam.Key.Year}-e{exam.Key.Stage}-q{number:00}"]));

        Assert.Equal(expected.Order(), keys.Order());
    }

    [Fact]
    public void EveryQuestionIsValidForTheImporter()
    {
        Assert.All(AllQuestions(), item =>
        {
            var subjects = RoadmapOf(item.Stage).Where(n => n.Kind == NodeKind.Subject).ToDictionary(n => n.Slug, n => n.Id);
            Assert.Null(InepQuestionImporter.Validate(item.Question, subjects));
        });
    }

    [Fact]
    public void AnswersMatchTheOfficialAnswerKey()
    {
        Assert.All(AllQuestions(), item =>
        {
            var official = AnswerKeys[(item.Year, item.Stage)][item.Number - 1];

            Assert.Equal(QuestionKind.Objective, item.Question.Kind);
            Assert.Equal($"{official}", item.Question.Correct);
            Assert.Single(item.Question.Options!, o => o.Explanation.StartsWith("Correta"));
            Assert.StartsWith("Correta", item.Question.Options![official - 'A'].Explanation);
        });
    }

    [Fact]
    public void SubjectFollowsTheAreaOfTheQuestionNumber()
    {
        Assert.All(AllQuestions(), item =>
        {
            // Edital, item 12.5: 1 a 9 Natureza, 10 a 18 Humanas, 19 a 23 Matemática, 24 a 32 Linguagens (30 a 32 língua estrangeira).
            var subject = RoadmapOf(item.Stage).Single(n => n.Kind == NodeKind.Subject && n.Slug == item.Question.Subject);
            var area = item.Number switch
            {
                <= 9 => StudyBlock.NaturalSciences,
                <= 18 => StudyBlock.Humanities,
                <= 23 => StudyBlock.Mathematics,
                _ => StudyBlock.Languages,
            };

            Assert.Equal(area, subject.Block);
            Assert.Equal(item.Language is null ? null : Languages[item.Language],
                PaveRoadmapSeed.ForeignLanguageSlugs.Contains(subject.Slug) ? subject.Slug : null);
            Assert.Equal(item.Number >= 30, item.Language is not null);
        });
    }

    [Fact]
    public void SourceNamesTheExamAndTheUniversity()
    {
        Assert.All(AllQuestions(), item =>
        {
            var language = item.Language switch { "es" => ", espanhol", "en" => ", inglês", _ => "" };
            Assert.Equal($"PAVE {item.Year} – Etapa {item.Stage}, questão {item.Number}{language} (UFPel)", item.Question.Source);
        });
    }

    [Fact]
    public void TheIgnoredAnswerOptionIsNotStored()
    {
        // A alternativa "f" da prova ("Ignoro a resposta", I.R.) é uma opção do simulado, não uma alternativa da questão.
        Assert.All(AllQuestions(), item =>
        {
            Assert.Equal(5, item.Question.Options!.Count);
            Assert.DoesNotContain(item.Question.Options, o => Regex.IsMatch(o.Text, @"^\s*(I\.\s*R\.?|Ignoro a resposta)\s*$", RegexOptions.IgnoreCase));
            Assert.DoesNotMatch(@"\(f\)\s*I\.\s*R", item.Question.Statement);
        });
    }

    [Fact]
    public void EveryReferencedImageExistsAndRenders()
    {
        Assert.All(AllQuestions(), item =>
        {
            var texts = item.Question.Options!.Select(o => o.Text).Prepend(item.Question.Statement);
            foreach (var text in texts)
            {
                var html = StatementFormatter.ToHtml(text);
                foreach (var path in Regex.Matches(text, @"\]\((img/[^)\s]+)\)").Select(m => m.Groups[1].Value))
                {
                    Assert.StartsWith($"img/pave/{item.Year}/etapa-{item.Stage}/", path);
                    Assert.True(File.Exists(Path.Combine(WwwRoot, path)), $"{item.Question.Key}: imagem {path} não existe");
                    Assert.Contains($"src=\"{path}\"", html);
                }
            }
        });
    }
}

[Collection(AppCollection.Name)]
public class PaveTrackTests(EnadeAdsFactory factory)
{
    private IServiceScope ScopeOf(int trackId)
    {
        var scope = factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<TrackScope>().Use(trackId);
        return scope;
    }

    private async Task<(HttpClient Client, ApplicationUser User)> StudentInAsync(string trackSlug, string email)
    {
        var user = await TestAccounts.CreateConfirmedAsync(factory, email, Roles.Student);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestAccounts.LoginAsync(client, email);
        // O link de troca de trilha grava o cookie, como no site.
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync($"/trilha/{trackSlug}")).StatusCode);
        return (client, user);
    }

    [Theory]
    [InlineData(Tracks.Pave1, 1)]
    [InlineData(Tracks.Pave2, 2)]
    [InlineData(Tracks.Pave3, 3)]
    public async Task EachStageGetsOnlyItsOwnExams(int trackId, int stage)
    {
        using var scope = ScopeOf(trackId);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var keys = await db.Questions.Select(q => q.ExternalKey!).ToListAsync();
        var subjects = await db.RoadmapNodes.Where(n => n.Kind == NodeKind.Subject).Select(n => n.Slug).ToListAsync();

        Assert.Contains(keys, k => k.StartsWith($"pave-2024-e{stage}-"));
        Assert.Contains(keys, k => k.StartsWith($"pave-2025-e{stage}-"));
        Assert.All(keys, k => Assert.Matches($"^pave-20\\d\\d-e{stage}-", k));
        Assert.Equal(stage == 3, subjects.Contains(PaveRoadmapSeed.EssaySlug));
    }

    [Fact]
    public async Task FullMockExam_FollowsTheAreasOfTheExamAndTheChosenLanguage()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "pave.simulado@gmail.com", Roles.Student);
        using var scope = ScopeOf(Tracks.Pave1);
        var exams = scope.ServiceProvider.GetRequiredService<MockExamService>();

        var started = await exams.StartAsync(student.Id, MockExamSize.Full, PaveRoadmapSeed.SpanishSlug);
        var exam = (await exams.FindAsync(student.Id, started.MockExamId!.Value))!;
        var subjects = exam.Questions.Select(q => q.Question!.Subject!).ToList();

        Assert.Equal(StartOutcome.Started, started.Outcome);
        Assert.Equal(32, exam.Questions.Count);
        Assert.Equal(32, exam.ObjectiveCount);
        Assert.Equal(210, (int)(exam.DeadlineAt - exam.StartedAt).TotalMinutes);
        Assert.All(subjects, s => Assert.Equal(Tracks.Pave1, s.TrackId));
        // Ordem da prova: 9 de Natureza, 9 de Humanas, 5 de Matemática, 6 de Linguagens e 3 de língua estrangeira.
        Assert.All(subjects.Take(9), s => Assert.Equal(StudyBlock.NaturalSciences, s.Block));
        Assert.All(subjects.Skip(9).Take(9), s => Assert.Equal(StudyBlock.Humanities, s.Block));
        Assert.All(subjects.Skip(18).Take(5), s => Assert.Equal(StudyBlock.Mathematics, s.Block));
        Assert.All(subjects.Skip(23).Take(6), s => Assert.Contains(s.Slug, new[] { "lingua-portuguesa", "literatura", "arte" }));
        Assert.All(subjects.Skip(29), s => Assert.Equal(PaveRoadmapSeed.SpanishSlug, s.Slug));
    }

    [Fact]
    public async Task IgnoredAnswer_NeitherAddsNorDiscounts_AndBlankCountsAsAMiss()
    {
        var (client, user) = await StudentInAsync("pave-3", "pave.nota@gmail.com");
        int id;
        using (var scope = ScopeOf(Tracks.Pave3))
        {
            id = (await scope.ServiceProvider.GetRequiredService<MockExamService>().StartAsync(user.Id, MockExamSize.Short)).MockExamId!.Value;
        }

        var page = WebUtility.HtmlDecode(await client.GetStringAsync($"/simulados/{id}"));
        MockExam exam;
        using (var scope = ScopeOf(Tracks.Pave3))
        {
            exam = (await scope.ServiceProvider.GetRequiredService<MockExamService>().FindAsync(user.Id, id))!;
        }

        // Curto: 11 questões. 5 certas, 2 erradas, 3 com "Ignoro a resposta" e 1 em branco.
        var questions = exam.Questions.Select(q => q.Question!).ToList();
        var answers = new Dictionary<string, string> { ["Command"] = "entregar", ["ConfirmSubmit"] = "true" };
        foreach (var question in questions.Take(5))
        {
            answers[$"q-{question.Id}"] = question.Options.Single(o => o.IsCorrect).Letter.ToString();
        }

        foreach (var question in questions.Skip(5).Take(2))
        {
            answers[$"q-{question.Id}"] = question.Options.First(o => !o.IsCorrect).Letter.ToString();
        }

        foreach (var question in questions.Skip(7).Take(3))
        {
            answers[$"q-{question.Id}"] = "F";
        }

        var response = await TestAccounts.PostFormAsync(client, $"/simulados/{id}", page, "mock-exam", answers);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        using var after = ScopeOf(Tracks.Pave3);
        var graded = (await after.ServiceProvider.GetRequiredService<MockExamService>().FindAsync(user.Id, id))!;
        var ignoredIds = questions.Skip(7).Take(3).Select(q => q.Id).ToList();
        var attempts = await after.ServiceProvider.GetRequiredService<ApplicationDbContext>().QuestionAttempts
            .Where(a => a.UserId == user.Id).Select(a => a.QuestionId).ToListAsync();

        Assert.Equal(11, questions.Count);
        Assert.Contains("Ignoro a resposta", page);
        Assert.Equal((5, 3, 3), (graded.CorrectCount, graded.IgnoredCount, graded.WrongCount));
        // Etapa 3: 2,6875 × (5 − 0,1 × 3) = 12,63 de 29,56 possíveis em 11 questões.
        Assert.Equal(12.63125m, PaveScoring.Score(graded));
        Assert.Contains("Você acertou 5 de 11", html);
        Assert.Contains("Nota: 12,63 de 29,56 pontos.", html);
        Assert.Contains("a redação vale até 14 pontos", html);
        Assert.Contains("não somou nem descontou", html);
        // "Ignoro a resposta" não é tentativa: não vai para o painel nem para o caderno de erros.
        Assert.Equal(7, attempts.Count);
        Assert.DoesNotContain(attempts, ignoredIds.Contains);
    }

    [Fact]
    public async Task OtherTracks_DoNotAcceptTheIgnoredAnswer()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "pave.enade@gmail.com", Roles.Student);
        using var scope = factory.Services.CreateScope();
        var exams = scope.ServiceProvider.GetRequiredService<MockExamService>();
        var id = (await exams.StartAsync(student.Id, MockExamSize.Short)).MockExamId!.Value;
        var question = (await exams.FindAsync(student.Id, id))!.Questions.First(q => q.Question!.Kind == QuestionKind.Objective);

        await exams.SaveAsync(student.Id, id, new Dictionary<int, char> { [question.QuestionId] = MockExamService.IgnoredAnswerLetter },
            new Dictionary<int, string>(), submit: true);
        var graded = (await exams.FindAsync(student.Id, id))!;

        Assert.Equal(0, graded.IgnoredCount);
        Assert.DoesNotContain(graded.Questions, q => q.IgnoredAnswer);
    }

    [Fact]
    public async Task Pages_ShowTheStageTheRuleAndTheLanguageChoice()
    {
        var (client, _) = await StudentInAsync("pave-3", "pave.paginas@gmail.com");

        var home = WebUtility.HtmlDecode(await client.GetStringAsync("/"));
        var roadmap = WebUtility.HtmlDecode(await client.GetStringAsync("/roadmap"));
        var mockExams = WebUtility.HtmlDecode(await client.GetStringAsync("/simulados"));

        Assert.Matches("class=\"track-label\"[^>]*>PAVE 3</span>", home);
        Assert.Contains("Roadmap PAVE 2026 · Etapa 3", roadmap);
        Assert.Contains("Redação", roadmap);
        Assert.Contains("Língua espanhola", roadmap);
        Assert.DoesNotContain("Banco de dados", roadmap);
        Assert.Contains("32 questões · 3h30", mockExams);
        Assert.Contains("9 de Ciências da Natureza, 9 de Ciências Humanas, 5 de Matemática e 9 de Linguagens (3 de língua estrangeira).", mockExams);
        Assert.Contains("Ignoro a resposta", mockExams);
        Assert.Contains("value=\"espanhol\"", mockExams);
    }
}
