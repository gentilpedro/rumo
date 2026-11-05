using System.Net;
using EnadeAds.Accounts;
using EnadeAds.Data;
using EnadeAds.Duels;
using EnadeAds.Questions;
using EnadeAds.Study;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

public class DuelNameTests
{
    [Theory]
    [InlineData("Maria Clara Souza", "Maria S.")]
    [InlineData("  joão   pedro ", "joão P.")]
    [InlineData("Ana", "Ana")]
    [InlineData("", "Participante")]
    [InlineData(null, "Participante")]
    public void Standings_ShowOnlyTheFirstNameAndAnInitial(string? fullName, string expected)
    {
        Assert.Equal(expected, DuelService.ShortName(fullName));
    }
}

[Collection(AppCollection.Name)]
public class DuelTests(EnadeAdsFactory factory)
{
    private HttpClient NewClient() => factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private async Task<T> InScopeAsync<T>(Func<IServiceProvider, Task<T>> action, int? trackId = null)
    {
        using var scope = factory.Services.CreateScope();
        if (trackId is not null)
        {
            scope.ServiceProvider.GetRequiredService<TrackScope>().Use(trackId.Value);
        }

        return await action(scope.ServiceProvider);
    }

    private Task<string> CreateAsync(string userId, int? trackId = null) =>
        InScopeAsync(async services => (await services.GetRequiredService<DuelService>().CreateAsync(userId)).Code!, trackId);

    private Task<Duel> FindAsync(string code) =>
        InScopeAsync(async services => (await services.GetRequiredService<DuelService>().FindAsync(code))!);

    /// <summary>Letras certas (ou erradas) das questões do duelo.</summary>
    private static Dictionary<int, char> Answers(Duel duel, int correct) =>
        duel.Questions.Select((item, index) => (item.QuestionId,
                Letter: item.Question!.Options.First(o => o.IsCorrect == index < correct).Letter))
            .ToDictionary(a => a.QuestionId, a => a.Letter);

    private Task<SubmitDuelOutcome> SubmitAsync(string code, string userId, Dictionary<int, char> letters) =>
        InScopeAsync(services => services.GetRequiredService<DuelService>().SubmitAsync(code, userId, letters));

    private Task<JoinDuelOutcome> JoinAsync(string code, string userId) =>
        InScopeAsync(services => services.GetRequiredService<DuelService>().JoinAsync(code, userId));

    private Task MoveStartAsync(string code, string userId, TimeSpan ago) =>
        InScopeAsync(async services =>
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var player = await db.DuelPlayers.SingleAsync(p => p.UserId == userId && db.Duels.IgnoreQueryFilters().Any(d => d.Id == p.DuelId && d.Code == code));
            player.StartedAt = DateTimeOffset.UtcNow - ago;
            return await db.SaveChangesAsync();
        });

    [Fact]
    public async Task Create_PicksObjectiveQuestionsOfTheActiveTrack()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "duelo.criar@gmail.com", Roles.Student);

        var code = await CreateAsync(student.Id, Tracks.EnadeRedes);
        var duel = await FindAsync(code);

        Assert.Matches("^[a-z2-9]{8}$", code);
        Assert.Equal(Tracks.EnadeRedes, duel.TrackId);
        Assert.Equal(DuelService.QuestionCount, duel.Questions.Count);
        Assert.Equal(DuelService.QuestionCount, duel.Questions.Select(q => q.QuestionId).Distinct().Count());
        Assert.All(duel.Questions, q => Assert.Equal(QuestionKind.Objective, q.Question!.Kind));
        Assert.All(duel.Questions, q => Assert.Equal(Tracks.EnadeRedes, q.Question!.Subject!.TrackId));
        Assert.Empty(duel.Players);
    }

    [Fact]
    public async Task Standings_RankByCorrectAnswersThenByTime()
    {
        var creator = await TestAccounts.CreateConfirmedAsync(factory, "duelo.placar1@gmail.com", Roles.Student);
        var fast = await TestAccounts.CreateConfirmedAsync(factory, "duelo.placar2@gmail.com", Roles.Student);
        var slow = await TestAccounts.CreateConfirmedAsync(factory, "duelo.placar3@gmail.com", Roles.Student);
        var code = await CreateAsync(creator.Id);
        var duel = await FindAsync(code);

        foreach (var player in new[] { creator, fast, slow })
        {
            Assert.Equal(JoinDuelOutcome.Started, await JoinAsync(code, player.Id));
        }

        await MoveStartAsync(code, slow.Id, TimeSpan.FromMinutes(5));
        await MoveStartAsync(code, fast.Id, TimeSpan.FromMinutes(1));
        Assert.Equal(SubmitDuelOutcome.Submitted, await SubmitAsync(code, creator.Id, Answers(duel, 2)));
        Assert.Equal(SubmitDuelOutcome.Submitted, await SubmitAsync(code, slow.Id, Answers(duel, 4)));
        Assert.Equal(SubmitDuelOutcome.Submitted, await SubmitAsync(code, fast.Id, Answers(duel, 4)));
        Assert.Equal(SubmitDuelOutcome.AlreadyFinished, await SubmitAsync(code, fast.Id, Answers(duel, 5)));

        var played = await FindAsync(code);
        var standings = await InScopeAsync(services => services.GetRequiredService<DuelService>().StandingsAsync(played, fast.Id));

        // Empate em 4: quem levou 1 minuto fica na frente de quem levou 5. Quem consulta é o mais rápido.
        Assert.Equal([4, 4, 2], standings.Select(s => s.Correct!.Value));
        Assert.Equal([true, false, false], standings.Select(s => s.IsMe));
        Assert.Equal([1, 2, 3], standings.Select(s => s.Position!.Value));
        Assert.True(standings[0].Elapsed < standings[1].Elapsed);
    }

    [Fact]
    public async Task Answers_GoToThePerformanceAsAttempts()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "duelo.tentativas@gmail.com", Roles.Student);
        var code = await CreateAsync(student.Id);
        var duel = await FindAsync(code);

        await JoinAsync(code, student.Id);
        await SubmitAsync(code, student.Id, Answers(duel, 3));

        var attempts = await InScopeAsync(services => services.GetRequiredService<ApplicationDbContext>().QuestionAttempts
            .Where(a => a.UserId == student.Id).ToListAsync());
        Assert.Equal(DuelService.QuestionCount, attempts.Count);
        Assert.Equal(3, attempts.Count(a => a.IsCorrect));
        Assert.All(attempts, a => Assert.Null(a.ChallengeId));
    }

    [Fact]
    public async Task AfterTheTimeLimit_AnswersDoNotCount()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "duelo.atrasado@gmail.com", Roles.Student);
        var code = await CreateAsync(student.Id);
        var duel = await FindAsync(code);

        await JoinAsync(code, student.Id);
        await MoveStartAsync(code, student.Id, DuelService.TimeLimit + DuelService.GracePeriod + TimeSpan.FromMinutes(1));
        await SubmitAsync(code, student.Id, Answers(duel, 5));

        var player = (await FindAsync(code)).Players.Single();
        Assert.Equal(0, player.CorrectCount);
        Assert.Empty(player.Answers);
        // O tempo do placar para no limite: enviar depois não muda o desempate.
        Assert.Equal(DuelService.TimeLimit, player.Elapsed);
    }

    [Fact]
    public async Task Join_OnlyOncePerPersonAndOnlyWhileTheLinkIsOpen()
    {
        var creator = await TestAccounts.CreateConfirmedAsync(factory, "duelo.entrar1@gmail.com", Roles.Student);
        var late = await TestAccounts.CreateConfirmedAsync(factory, "duelo.entrar2@gmail.com", Roles.Student);
        var code = await CreateAsync(creator.Id);

        Assert.Equal(JoinDuelOutcome.Started, await JoinAsync(code, creator.Id));
        Assert.Equal(JoinDuelOutcome.AlreadyPlaying, await JoinAsync(code, creator.Id));
        Assert.Equal(SubmitDuelOutcome.NotStarted, await SubmitAsync(code, late.Id, new()));

        await InScopeAsync(async services =>
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            var duel = await db.Duels.IgnoreQueryFilters().SingleAsync(d => d.Code == code);
            duel.CreatedAt -= DuelService.JoinWindow + TimeSpan.FromMinutes(1);
            return await db.SaveChangesAsync();
        });

        Assert.Equal(JoinDuelOutcome.Closed, await JoinAsync(code, late.Id));
        Assert.Equal(JoinDuelOutcome.NotFound, await JoinAsync("naoexiste", late.Id));
    }

    [Fact]
    public async Task Visitor_SeesTheInviteWithoutTheStandings()
    {
        var creator = await TestAccounts.CreateConfirmedAsync(factory, "duelo.convite@gmail.com", Roles.Student);
        var code = await CreateAsync(creator.Id);
        var duel = await FindAsync(code);
        await JoinAsync(code, creator.Id);
        await SubmitAsync(code, creator.Id, Answers(duel, 5));

        var response = await NewClient().GetAsync($"/duelos/{code}");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"{DuelService.ShortName(creator.FullName)} te desafiou", html);
        Assert.Contains("1 pessoa já jogou", html);
        Assert.Contains("og:title", html);
        Assert.Contains($"ReturnUrl=%2Fduelos%2F{code}", html);
        // Sem placar e sem enunciado antes de entrar.
        Assert.DoesNotContain("Placar", html);
        Assert.DoesNotContain(duel.Questions[0].Question!.Options[0].Explanation, html);
        Assert.Equal(HttpStatusCode.NotFound, (await NewClient().GetAsync("/duelos/naoexiste")).StatusCode);
    }

    [Fact]
    public async Task Player_JoinsAnswersAndSeesTheResultThroughThePages()
    {
        var creator = await TestAccounts.CreateConfirmedAsync(factory, "duelo.pagina1@gmail.com", Roles.Student);
        var friend = await TestAccounts.CreateConfirmedAsync(factory, "duelo.pagina2@gmail.com", Roles.Student);
        // Criado na trilha de Redes; o colega navega na de ADS e joga as questões de Redes mesmo assim.
        var code = await CreateAsync(creator.Id, Tracks.EnadeRedes);
        var duel = await FindAsync(code);

        var client = NewClient();
        await TestAccounts.LoginAsync(client, friend.Email!);
        var url = $"/duelos/{code}";

        var joined = await TestAccounts.SubmitFormAsync(client, url, "join-duel", new());
        Assert.Equal(HttpStatusCode.Redirect, joined.StatusCode);

        var playing = WebUtility.HtmlDecode(await client.GetStringAsync(url));
        Assert.Contains("Enviar respostas", playing);
        Assert.Contains(duel.Questions[0].Question!.Subject!.Title, playing);

        var fields = Answers(duel, 5).ToDictionary(a => $"q-{a.Key}", a => a.Value.ToString());
        var submitted = await TestAccounts.PostFormAsync(client, url, playing, "submit-duel", fields);
        Assert.Equal(HttpStatusCode.Redirect, submitted.StatusCode);

        var result = WebUtility.HtmlDecode(await client.GetStringAsync(url));
        Assert.Contains("Você está em 1º lugar!", result);
        Assert.Contains("Placar", result);
        Assert.Contains("https://wa.me/?text=", result);
        Assert.Contains(duel.Questions[0].Question!.Options[0].Explanation, result);

        var mine = WebUtility.HtmlDecode(await client.GetStringAsync("/duelos"));
        Assert.Contains($"duelos/{code}", mine);
        Assert.Contains("ENADE Redes", mine);
    }

    [Fact]
    public async Task Visitor_CannotCreateADuel()
    {
        var html = WebUtility.HtmlDecode(await NewClient().GetStringAsync("/duelos"));

        Assert.Contains("para criar um duelo", html);
        Assert.DoesNotContain("Criar duelo</button>", html);
    }
}
