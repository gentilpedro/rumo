using System.Net;
using System.Text.Json;
using EnadeAds.Accounts;
using EnadeAds.Data;
using EnadeAds.Duels;
using EnadeAds.Friends;
using EnadeAds.Questions;
using EnadeAds.Rooms;
using EnadeAds.Study;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

/// <summary>O que as duas classes abaixo dividem: contas, respostas a questões e amizades prontas.</summary>
public abstract class SocialTestBase(EnadeAdsFactory factory)
{
    protected EnadeAdsFactory Factory => factory;

    protected HttpClient NewClient() => factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    protected async Task<T> WithAsync<TService, T>(Func<TService, Task<T>> action) where TService : notnull
    {
        using var scope = factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }

    protected Task<ApplicationUser> StudentAsync(string email) => TestAccounts.CreateConfirmedAsync(factory, email, Roles.Student);

    protected async Task<HttpClient> LoggedInAsync(string email)
    {
        var client = NewClient();
        await TestAccounts.LoginAsync(client, email);
        return client;
    }

    /// <summary>Questões objetivas de uma trilha, na ordem do id, com a letra certa e uma errada.</summary>
    protected async Task<List<(int Id, char Right, char Wrong, string Subject)>> ObjectivesAsync(int trackId, int count)
    {
        using var scope = factory.Services.CreateScope();
        var questions = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Questions.IgnoreQueryFilters().AsNoTracking()
            .Include(q => q.Options).Include(q => q.Subject)
            .Where(q => q.Subject!.TrackId == trackId && q.Kind == QuestionKind.Objective && q.ArchivedAt == null)
            .OrderBy(q => q.Id).Take(count)
            .ToListAsync();
        return questions.Select(q => (q.Id, q.Options.First(o => o.IsCorrect).Letter, q.Options.First(o => !o.IsCorrect).Letter, q.Subject!.Title)).ToList();
    }

    protected Task AnswerAsync(string userId, int questionId, char letter) =>
        WithAsync<QuestionBank, AnswerResult?>(bank => bank.AnswerAsync(userId, questionId, letter));

    /// <summary>Faz as duas contas amigas e devolve o número da amizade.</summary>
    protected async Task<int> BefriendAsync(ApplicationUser a, ApplicationUser b)
    {
        await WithAsync<FriendService, FriendRequestOutcome>(friends => friends.RequestAsync(a.Id, b.Id));
        var request = (await WithAsync<FriendService, FriendsOverview>(friends => friends.OverviewAsync(b.Id))).Received.Single();
        await WithAsync<FriendService, FriendActionOutcome>(friends => friends.AcceptAsync(b.Id, request.Id));
        return request.Id;
    }

    protected async Task<string> RoomAsync(ApplicationUser owner, string name, params ApplicationUser[] members)
    {
        var code = (await WithAsync<StudyRoomService, CreateRoomResult>(rooms => rooms.CreateAsync(owner.Id, name))).Code!;
        foreach (var member in members)
        {
            await WithAsync<StudyRoomService, JoinRoomOutcome>(rooms => rooms.JoinAsync(code, member.Id));
        }

        return code;
    }
}

[Collection(AppCollection.Name)]
public class RoomRankingAndDuelTests(EnadeAdsFactory factory) : SocialTestBase(factory)
{
    private async Task<RoomRanking> RankingAsync(string code, string userId) =>
        await WithAsync<StudyRoomService, RoomRanking>(async rooms => await rooms.RankingAsync((await rooms.FindAsync(code))!, userId));

    [Fact]
    public async Task Ranking_CountsEachCorrectQuestionOfTheRoomTrackOnceInTheWeek()
    {
        var owner = await StudentAsync("ranking.dono@gmail.com");
        var ana = await StudentAsync("ranking.ana@gmail.com");
        var bia = await StudentAsync("ranking.bia@gmail.com");
        var outsider = await StudentAsync("ranking.fora@gmail.com");
        var code = await RoomAsync(owner, "Sala do ranking", ana, bia);
        var questions = await ObjectivesAsync(Tracks.Default, 3);
        var otherTrack = (await ObjectivesAsync(Tracks.Enem, 1))[0];

        // Ana: duas questões certas (uma repetida), uma errada e uma certa de outra prova.
        await AnswerAsync(ana.Id, questions[0].Id, questions[0].Right);
        await AnswerAsync(ana.Id, questions[0].Id, questions[0].Right);
        await AnswerAsync(ana.Id, questions[1].Id, questions[1].Right);
        await AnswerAsync(ana.Id, questions[2].Id, questions[2].Wrong);
        await AnswerAsync(ana.Id, otherTrack.Id, otherTrack.Right);
        // Dono e Bia empatam com uma cada; quem não é da sala não aparece.
        await AnswerAsync(owner.Id, questions[0].Id, questions[0].Right);
        await AnswerAsync(bia.Id, questions[1].Id, questions[1].Right);
        await AnswerAsync(outsider.Id, questions[0].Id, questions[0].Right);

        // Semana passada: só o dono pontuou.
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var option = await db.QuestionOptions.FirstAsync(o => o.QuestionId == questions[2].Id && o.IsCorrect);
            db.QuestionAttempts.Add(new QuestionAttempt
            {
                QuestionId = questions[2].Id, UserId = owner.Id, OptionId = option.Id, IsCorrect = true, AnsweredAt = DateTimeOffset.UtcNow.AddDays(-7),
            });
            await db.SaveChangesAsync();
        }

        var ranking = await RankingAsync(code, ana.Id);

        Assert.Equal(DayOfWeek.Monday, ranking.Monday.DayOfWeek);
        Assert.Equal([(1, 2, true), (2, 1, false), (2, 1, false)], ranking.ThisWeek.Select(r => (r.Position, r.Points, r.IsMe)));
        // Só o nome abreviado sai daqui.
        Assert.All(ranking.ThisWeek, row => Assert.Equal("Teste R.", row.Name));
        var lastWeek = Assert.Single(ranking.LastWeek);
        Assert.Equal((1, 1), (lastWeek.Position, lastWeek.Points));
    }

    [Fact]
    public async Task Ranking_IsOnlyForMembersAndForgetsWhoLeft()
    {
        var owner = await StudentAsync("ranking.pagina.dono@gmail.com");
        var member = await StudentAsync("ranking.pagina.membro@gmail.com");
        var outsider = await StudentAsync("ranking.pagina.fora@gmail.com");
        var code = await RoomAsync(owner, "Sala da página do ranking", member);
        var question = (await ObjectivesAsync(Tracks.Default, 1))[0];
        await AnswerAsync(member.Id, question.Id, question.Right);

        var asMember = WebUtility.HtmlDecode(await (await LoggedInAsync(member.Email!)).GetStringAsync($"/salas/{code}"));
        var asOutsider = WebUtility.HtmlDecode(await (await LoggedInAsync(outsider.Email!)).GetStringAsync($"/salas/{code}"));
        await WithAsync<StudyRoomService, RoomActionOutcome>(rooms => rooms.LeaveAsync(code, member.Id));
        var afterLeaving = await RankingAsync(code, owner.Id);

        Assert.Contains("Ranking da semana", asMember);
        Assert.Contains("1 ponto", asMember);
        Assert.Contains("nunca quais questões cada um respondeu", asMember);
        // Quem só tem o link é avisado do que vai aparecer, mas não vê o ranking.
        Assert.Contains("quantas questões você acertou na semana", asOutsider);
        Assert.DoesNotContain("Ranking da semana", asOutsider);
        Assert.Equal(0, Assert.Single(afterLeaving.ThisWeek).Points);
    }

    [Fact]
    public async Task RoomDuel_IsOnlyForMembers()
    {
        var owner = await StudentAsync("saladuelo.dono@gmail.com");
        var member = await StudentAsync("saladuelo.membro@gmail.com");
        var outsider = await StudentAsync("saladuelo.fora@gmail.com");
        var code = await RoomAsync(owner, "Sala do duelo", member);

        var byOutsider = await WithAsync<StudyRoomService, string?>(rooms => rooms.ChallengeAsync(code, outsider.Id));
        var duelCode = (await WithAsync<StudyRoomService, string?>(rooms => rooms.ChallengeAsync(code, owner.Id)))!;
        var duel = (await WithAsync<DuelService, Duel?>(duels => duels.FindAsync(duelCode)))!;
        var roomId = (await WithAsync<StudyRoomService, StudyRoom?>(rooms => rooms.FindAsync(code)))!.Id;

        var outsiderJoins = await WithAsync<DuelService, JoinDuelOutcome>(duels => duels.JoinAsync(duelCode, outsider.Id));
        var outsiderPage = WebUtility.HtmlDecode(await (await LoggedInAsync(outsider.Email!)).GetStringAsync($"/duelos/{duelCode}"));
        var memberJoins = await WithAsync<DuelService, JoinDuelOutcome>(duels => duels.JoinAsync(duelCode, member.Id));
        await WithAsync<DuelService, SubmitDuelOutcome>(duels => duels.SubmitAsync(duelCode, member.Id, new Dictionary<int, char>()));
        var roomPage = WebUtility.HtmlDecode(await (await LoggedInAsync(owner.Email!)).GetStringAsync($"/salas/{code}"));
        var memberRoomPage = WebUtility.HtmlDecode(await (await LoggedInAsync(member.Email!)).GetStringAsync($"/salas/{code}"));

        Assert.Null(byOutsider);
        Assert.Equal(roomId, duel.RoomId);
        // A prova do duelo é a da sala.
        Assert.Equal(Tracks.Default, duel.TrackId);
        Assert.Equal(JoinDuelOutcome.NotAMember, outsiderJoins);
        Assert.Contains("Este duelo é de uma sala de estudos", outsiderPage);
        Assert.DoesNotContain("Começar", outsiderPage);
        Assert.Equal(JoinDuelOutcome.Started, memberJoins);
        Assert.Contains("Duelos da sala", roomPage);
        Assert.Contains("Desafiar a sala", roomPage);
        Assert.Contains($"duelos/{duelCode}", roomPage);
        Assert.Contains("Jogar", roomPage);
        Assert.Contains("1 pessoa jogou", roomPage);
        Assert.Contains("Ver placar", memberRoomPage);
    }

    [Fact]
    public async Task RoomDuel_SurvivesTheRoomAndTheChallengeButtonCreatesIt()
    {
        var owner = await StudentAsync("saladuelo.apaga.dono@gmail.com");
        var code = await RoomAsync(owner, "Sala que vai sumir");
        var client = await LoggedInAsync(owner.Email!);

        var challenged = await TestAccounts.SubmitFormAsync(client, $"/salas/{code}", "challenge-room", []);
        var location = challenged.Headers.Location!.ToString();
        var duelCode = location[(location.LastIndexOf('/') + 1)..];
        await WithAsync<DuelService, JoinDuelOutcome>(duels => duels.JoinAsync(duelCode, owner.Id));
        await WithAsync<DuelService, SubmitDuelOutcome>(duels => duels.SubmitAsync(duelCode, owner.Id, new Dictionary<int, char>()));
        await WithAsync<StudyRoomService, RoomActionOutcome>(rooms => rooms.DeleteAsync(code, owner.Id));
        var duel = await WithAsync<DuelService, Duel?>(duels => duels.FindAsync(duelCode));

        Assert.Contains("duelos/", location);
        // Apagar a sala não apaga o placar nem o XP de quem jogou: o duelo fica, sem sala.
        Assert.NotNull(duel);
        Assert.Null(duel.RoomId);
        Assert.Single(duel.Players);
    }
}

[Collection(AppCollection.Name)]
public class FriendActivityAndInviteTests(EnadeAdsFactory factory) : SocialTestBase(factory)
{
    private Task<FriendActivityPage> ActivityAsync(string userId) =>
        WithAsync<FriendActivityService, FriendActivityPage>(activity => activity.ListAsync(userId, 1));

    private Task<IReadOnlyList<FriendInviteView>> InvitesAsync(string userId) =>
        WithAsync<FriendInviteService, IReadOnlyList<FriendInviteView>>(invites => invites.ReceivedAsync(userId));

    [Fact]
    public async Task Activity_ShowsWhatAFriendStudiedTodayAndNothingAboutRightOrWrong()
    {
        var ana = await StudentAsync("atividade.ana@gmail.com");
        var bia = await StudentAsync("atividade.bia@gmail.com");
        var carla = await StudentAsync("atividade.carla@gmail.com");
        await BefriendAsync(ana, bia);
        // Pedido pendente não vê nada.
        await WithAsync<FriendService, FriendRequestOutcome>(friends => friends.RequestAsync(carla.Id, bia.Id));
        var questions = await ObjectivesAsync(Tracks.Default, 2);
        await AnswerAsync(bia.Id, questions[0].Id, questions[0].Wrong);
        await AnswerAsync(bia.Id, questions[1].Id, questions[1].Right);

        var seenByAna = Assert.Single((await ActivityAsync(ana.Id)).Items);
        var seenByCarla = await ActivityAsync(carla.Id);
        var page = WebUtility.HtmlDecode(await (await LoggedInAsync(ana.Email!)).GetStringAsync("/amigos"));

        Assert.True(seenByAna.Shares);
        Assert.Equal("Teste A.", seenByAna.Name);
        Assert.Equal(Tracks.Default, seenByAna.TrackId);
        Assert.Equal(2, seenByAna.AnsweredToday);
        Assert.Equal(questions[1].Subject, seenByAna.Subject);
        Assert.Equal(1, seenByAna.Level!.Number);
        Assert.Equal(1, seenByAna.Streak);
        Assert.NotNull(seenByAna.LastStudied);
        Assert.Empty(seenByCarla.Items);
        Assert.Contains($"estudando {questions[1].Subject} · 2 questões hoje", page);
        Assert.Contains("nível 1 (Calouro)", page);
        Assert.Contains("1 dia de sequência", page);
        Assert.Contains("Desafiar", page);
    }

    [Fact]
    public async Task Activity_CanBeTurnedOffWithoutEndingTheFriendship()
    {
        var ana = await StudentAsync("atividade.chave.ana@gmail.com");
        var bia = await StudentAsync("atividade.chave.bia@gmail.com");
        await BefriendAsync(ana, bia);
        var question = (await ObjectivesAsync(Tracks.Default, 1))[0];
        await AnswerAsync(bia.Id, question.Id, question.Right);
        var biaClient = await LoggedInAsync(bia.Email!);

        // Caixa desmarcada não vem no formulário: é assim que se desliga.
        var off = await TestAccounts.SubmitFormAsync(biaClient, "/amigos", "share-activity", []);
        var hidden = Assert.Single((await ActivityAsync(ana.Id)).Items);
        var page = WebUtility.HtmlDecode(await (await LoggedInAsync(ana.Email!)).GetStringAsync("/amigos"));
        var on = await TestAccounts.SubmitFormAsync(biaClient, "/amigos", "share-activity", new() { ["ShareActivity"] = "true" });
        var shown = Assert.Single((await ActivityAsync(ana.Id)).Items);

        Assert.Contains("aviso=atividade-desligada", off.Headers.Location?.ToString());
        Assert.False(hidden.Shares);
        // Com a chave desligada, nada além do nome sai do serviço.
        Assert.Null(hidden.TrackId);
        Assert.Null(hidden.Level);
        Assert.Null(hidden.Subject);
        Assert.Null(hidden.LastStudied);
        Assert.Equal((0, 0), (hidden.Streak, hidden.AnsweredToday));
        Assert.Equal("Teste A.", hidden.Name);
        Assert.Contains("não mostra a atividade", page);
        Assert.DoesNotContain("questão hoje", page);
        Assert.Contains("aviso=atividade-ligada", on.Headers.Location?.ToString());
        Assert.True(shown.Shares);
        Assert.Equal(1, shown.AnsweredToday);
    }

    [Fact]
    public async Task Challenge_LeavesAnInviteThatGoesAwayWhenTheFriendPlaysOrDismisses()
    {
        var ana = await StudentAsync("convite.ana@gmail.com");
        var bia = await StudentAsync("convite.bia@gmail.com");
        var carla = await StudentAsync("convite.carla@gmail.com");
        var friendship = await BefriendAsync(ana, bia);
        var anaClient = await LoggedInAsync(ana.Email!);
        var biaClient = await LoggedInAsync(bia.Email!);

        // Só dá para desafiar pelo número de uma amizade da própria pessoa.
        var byStranger = await WithAsync<FriendInviteService, string?>(invites => invites.ChallengeAsync(carla.Id, friendship));
        var challenged = await TestAccounts.SubmitFormAsync(anaClient, "/amigos", "friend-action", new() { ["Command"] = $"challenge:{friendship}" });
        var location = challenged.Headers.Location!.ToString();
        var duelCode = location[(location.LastIndexOf('/') + 1)..];
        var received = await InvitesAsync(bia.Id);
        var count = await WithAsync<FriendInviteService, int>(invites => invites.PendingCountAsync(bia.Id));
        var biaPage = WebUtility.HtmlDecode(await biaClient.GetStringAsync("/amigos"));
        var biaHome = WebUtility.HtmlDecode(await biaClient.GetStringAsync("/"));
        var anaHome = WebUtility.HtmlDecode(await anaClient.GetStringAsync("/"));

        // Jogar o duelo tira o convite da lista.
        await WithAsync<DuelService, JoinDuelOutcome>(duels => duels.JoinAsync(duelCode, bia.Id));
        var afterPlaying = await InvitesAsync(bia.Id);

        // Um segundo desafio, dispensado: só quem recebeu dispensa.
        await WithAsync<FriendInviteService, string?>(invites => invites.ChallengeAsync(ana.Id, friendship));
        var second = (await InvitesAsync(bia.Id)).Single();
        var dismissedByOther = await WithAsync<FriendInviteService, bool>(invites => invites.DismissAsync(carla.Id, second.Id));
        var dismissed = await TestAccounts.SubmitFormAsync(biaClient, "/amigos", "friend-action", new() { ["Command"] = $"dismiss:{second.Id}" });

        Assert.Null(byStranger);
        Assert.Contains("duelos/", location);
        var invite = Assert.Single(received);
        Assert.True(invite.IsDuel);
        Assert.Equal(duelCode, invite.DuelCode);
        Assert.Equal("Teste C.", invite.FromName);
        Assert.Equal(1, count);
        Assert.Contains("Teste C. te desafiou para um duelo de ENADE ADS", biaPage);
        Assert.Contains($"duelos/{duelCode}", biaPage);
        // O aviso no topo aparece só para quem tem o que responder.
        Assert.Contains("class=\"notice-dot\"", biaHome);
        Assert.DoesNotContain("class=\"notice-dot\"", anaHome);
        Assert.Empty(afterPlaying);
        Assert.False(dismissedByOther);
        Assert.Contains("aviso=dispensado", dismissed.Headers.Location?.ToString());
        Assert.Empty(await InvitesAsync(bia.Id));
    }

    [Fact]
    public async Task RoomInvite_BringsAFriendIntoTheRoom()
    {
        var ana = await StudentAsync("convite.sala.ana@gmail.com");
        var bia = await StudentAsync("convite.sala.bia@gmail.com");
        var carla = await StudentAsync("convite.sala.carla@gmail.com");
        var friendship = await BefriendAsync(ana, bia);
        var code = await RoomAsync(ana, "Sala do convite de amigo");
        var otherRoom = await RoomAsync(carla, "Sala em que a Ana não está");
        var anaClient = await LoggedInAsync(ana.Email!);
        var biaClient = await LoggedInAsync(bia.Email!);

        var roomPage = WebUtility.HtmlDecode(await anaClient.GetStringAsync($"/salas/{code}"));
        var notAMember = await WithAsync<FriendInviteService, bool>(invites => invites.InviteToRoomAsync(ana.Id, friendship, otherRoom));
        var invited = await TestAccounts.SubmitFormAsync(anaClient, $"/salas/{code}", "invite-friend", new() { ["FriendshipId"] = $"{friendship}" });
        var again = await WithAsync<FriendInviteService, bool>(invites => invites.InviteToRoomAsync(ana.Id, friendship, code));
        var received = await InvitesAsync(bia.Id);
        var roomPageAfter = WebUtility.HtmlDecode(await anaClient.GetStringAsync($"/salas/{code}"));
        var biaPage = WebUtility.HtmlDecode(await biaClient.GetStringAsync("/amigos"));
        var joined = await TestAccounts.SubmitFormAsync(biaClient, "/amigos", "friend-action", new() { ["Command"] = $"join-room:{received.Single().Id}" });
        var room = (await WithAsync<StudyRoomService, StudyRoom?>(rooms => rooms.FindAsync(code)))!;

        Assert.Contains("Convidar amigos", roomPage);
        Assert.False(notAMember);
        Assert.Contains("aviso=convite", invited.Headers.Location?.ToString());
        Assert.True(again);
        // Convidar de novo não cria um segundo convite.
        var invite = Assert.Single(received);
        Assert.False(invite.IsDuel);
        Assert.Equal("Sala do convite de amigo", invite.RoomName);
        Assert.DoesNotContain("Convidar amigos", roomPageAfter);
        Assert.Contains("Teste C. te convidou para a sala Sala do convite de amigo", biaPage);
        Assert.Contains($"salas/{code}?aviso=entrou", joined.Headers.Location?.ToString());
        Assert.Contains(room.Members, m => m.UserId == bia.Id);
        Assert.Empty(await InvitesAsync(bia.Id));
    }

    [Fact]
    public async Task Invites_EndWithTheFriendship()
    {
        var ana = await StudentAsync("convite.fim.ana@gmail.com");
        var bia = await StudentAsync("convite.fim.bia@gmail.com");
        var dani = await StudentAsync("convite.fim.dani@gmail.com");
        var withBia = await BefriendAsync(ana, bia);
        var withDani = await BefriendAsync(ana, dani);
        await WithAsync<FriendInviteService, string?>(invites => invites.ChallengeAsync(ana.Id, withBia));
        await WithAsync<FriendInviteService, string?>(invites => invites.ChallengeAsync(ana.Id, withDani));

        await WithAsync<FriendService, FriendActionOutcome>(friends => friends.RemoveAsync(bia.Id, withBia));
        await WithAsync<FriendService, FriendActionOutcome>(friends => friends.BlockAsync(dani.Id, withDani));
        var afterUnfriend = await WithAsync<FriendInviteService, string?>(invites => invites.ChallengeAsync(ana.Id, withBia));

        Assert.Empty(await InvitesAsync(bia.Id));
        Assert.Empty(await InvitesAsync(dani.Id));
        Assert.Null(afterUnfriend);
        using var scope = Factory.Services.CreateScope();
        Assert.False(await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().FriendInvites.AnyAsync(i => i.FromId == ana.Id));
    }

    [Fact]
    public async Task Download_BringsTheSharingSwitchAndTheInvites()
    {
        var ana = await StudentAsync("convite.download.ana@gmail.com");
        var bia = await StudentAsync("convite.download.bia@gmail.com");
        var friendship = await BefriendAsync(ana, bia);
        await WithAsync<FriendInviteService, string?>(invites => invites.ChallengeAsync(ana.Id, friendship));
        var client = await LoggedInAsync(bia.Email!);

        var page = await client.GetStringAsync("/Account/Manage/PersonalData");
        var download = await TestAccounts.PostFormAsync(client, "/Account/Manage/DownloadPersonalData", page, "", []);
        var body = await download.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        var invite = json.GetProperty("ConvitesDeAmigos").EnumerateArray().Single();

        Assert.True(json.GetProperty("Conta").GetProperty("MostrarAtividadeAosAmigos").GetBoolean());
        Assert.Equal("duelo", invite.GetProperty("Tipo").GetString());
        Assert.False(invite.GetProperty("EnviadoPorVoce").GetBoolean());
        Assert.DoesNotContain(ana.Email!, body);
    }
}
