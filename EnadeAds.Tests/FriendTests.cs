using System.Net;
using System.Text.Json;
using EnadeAds.Accounts;
using EnadeAds.Data;
using EnadeAds.Duels;
using EnadeAds.Friends;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

[Collection(AppCollection.Name)]
public class FriendTests(EnadeAdsFactory factory)
{
    private HttpClient NewClient() => factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private async Task<T> InScopeAsync<T>(Func<FriendService, Task<T>> action)
    {
        using var scope = factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<FriendService>());
    }

    private Task<ApplicationUser> StudentAsync(string email) => TestAccounts.CreateConfirmedAsync(factory, email, Roles.Student);

    private Task<FriendsOverview> OverviewAsync(string userId) => InScopeAsync(friends => friends.OverviewAsync(userId));

    private async Task<HttpClient> LoggedInAsync(string email)
    {
        var client = NewClient();
        await TestAccounts.LoginAsync(client, email);
        return client;
    }

    /// <summary>Faz as duas contas amigas: uma pede, a outra aceita.</summary>
    private async Task BefriendAsync(ApplicationUser a, ApplicationUser b)
    {
        await InScopeAsync(friends => friends.RequestAsync(a.Id, b.Id));
        var request = (await OverviewAsync(b.Id)).Received.Single();
        await InScopeAsync(friends => friends.AcceptAsync(b.Id, request.Id));
    }

    [Fact]
    public async Task Link_IsCreatedOnceAndRenewingKillsTheOldOne()
    {
        var student = await StudentAsync("amigo.link@gmail.com");

        var first = await InScopeAsync(friends => friends.LinkAsync(student.Id));
        var again = await InScopeAsync(friends => friends.LinkAsync(student.Id));
        var owner = await InScopeAsync(friends => friends.OwnerOfAsync(first.ToUpperInvariant()));
        var renewed = await InScopeAsync(friends => friends.RenewLinkAsync(student.Id));

        Assert.Equal(FriendService.CodeLength, first.Length);
        Assert.Equal(first, again);
        Assert.Equal(student.Id, owner!.Value.UserId);
        // Só o primeiro nome e a inicial saem daqui.
        Assert.Equal("Teste A.", owner.Value.Name);
        Assert.NotEqual(first, renewed);
        Assert.Null(await InScopeAsync(friends => friends.OwnerOfAsync(first)));
        Assert.NotNull(await InScopeAsync(friends => friends.OwnerOfAsync(renewed)));
    }

    [Fact]
    public async Task Request_OnlyBecomesFriendshipAfterTheOtherAccepts()
    {
        var ana = await StudentAsync("amigo.pede.ana@gmail.com");
        var bia = await StudentAsync("amigo.pede.bia@gmail.com");
        var code = await InScopeAsync(friends => friends.LinkAsync(bia.Id));

        var requested = await InScopeAsync(friends => friends.RequestByCodeAsync(ana.Id, code));
        var twice = await InScopeAsync(friends => friends.RequestByCodeAsync(ana.Id, code));
        var pendingForAna = await OverviewAsync(ana.Id);
        var pendingForBia = await OverviewAsync(bia.Id);
        var friendsBefore = await InScopeAsync(friends => friends.AreFriendsAsync(ana.Id, bia.Id));

        // Quem pediu não aceita o próprio pedido.
        var requestId = pendingForBia.Received.Single().Id;
        var acceptedByRequester = await InScopeAsync(friends => friends.AcceptAsync(ana.Id, requestId));
        var accepted = await InScopeAsync(friends => friends.AcceptAsync(bia.Id, requestId));
        var afterForAna = await OverviewAsync(ana.Id);
        var again = await InScopeAsync(friends => friends.RequestAsync(bia.Id, ana.Id));

        Assert.Equal(FriendRequestOutcome.Requested, requested);
        Assert.Equal(FriendRequestOutcome.AlreadyRequested, twice);
        Assert.Single(pendingForAna.Sent);
        Assert.Empty(pendingForAna.Friends);
        Assert.Single(pendingForBia.Received);
        Assert.Empty(pendingForBia.Friends);
        Assert.False(friendsBefore);
        Assert.Equal(FriendActionOutcome.NotFound, acceptedByRequester);
        Assert.Equal(FriendActionOutcome.Done, accepted);
        Assert.Equal("Teste A.", Assert.Single(afterForAna.Friends).Name);
        Assert.Empty(afterForAna.Sent);
        Assert.True(await InScopeAsync(friends => friends.AreFriendsAsync(bia.Id, ana.Id)));
        Assert.Equal(FriendRequestOutcome.AlreadyFriends, again);
    }

    [Fact]
    public async Task Request_BackFromTheOtherSideIsAnAcceptance_AndOddCasesAreRefused()
    {
        var ana = await StudentAsync("amigo.mutuo.ana@gmail.com");
        var bia = await StudentAsync("amigo.mutuo.bia@gmail.com");

        var first = await InScopeAsync(friends => friends.RequestAsync(ana.Id, bia.Id));
        var back = await InScopeAsync(friends => friends.RequestAsync(bia.Id, ana.Id));
        var self = await InScopeAsync(friends => friends.RequestAsync(ana.Id, ana.Id));
        var nobody = await InScopeAsync(friends => friends.RequestAsync(ana.Id, "conta-que-nao-existe"));
        var badCode = await InScopeAsync(friends => friends.RequestByCodeAsync(ana.Id, "naoexiste"));

        Assert.Equal(FriendRequestOutcome.Requested, first);
        Assert.Equal(FriendRequestOutcome.Accepted, back);
        Assert.True(await InScopeAsync(friends => friends.AreFriendsAsync(ana.Id, bia.Id)));
        Assert.Equal(FriendRequestOutcome.Self, self);
        Assert.Equal(FriendRequestOutcome.NotFound, nobody);
        Assert.Equal(FriendRequestOutcome.NotFound, badCode);
    }

    [Fact]
    public async Task Remove_DeclinesCancelsAndUndoes_ButOnlyForWhoIsPartOfIt()
    {
        var ana = await StudentAsync("amigo.remove.ana@gmail.com");
        var bia = await StudentAsync("amigo.remove.bia@gmail.com");
        var carla = await StudentAsync("amigo.remove.carla@gmail.com");

        // Recusar um pedido recebido.
        await InScopeAsync(friends => friends.RequestAsync(ana.Id, bia.Id));
        var received = (await OverviewAsync(bia.Id)).Received.Single().Id;
        var byStranger = await InScopeAsync(friends => friends.RemoveAsync(carla.Id, received));
        var declined = await InScopeAsync(friends => friends.RemoveAsync(bia.Id, received));
        var afterDecline = await OverviewAsync(ana.Id);

        // Cancelar um pedido enviado.
        await InScopeAsync(friends => friends.RequestAsync(ana.Id, bia.Id));
        var sent = (await OverviewAsync(ana.Id)).Sent.Single().Id;
        var cancelled = await InScopeAsync(friends => friends.RemoveAsync(ana.Id, sent));

        // Desfazer uma amizade: qualquer um dos dois.
        await BefriendAsync(ana, bia);
        var friendship = (await OverviewAsync(bia.Id)).Friends.Single().Id;
        var undone = await InScopeAsync(friends => friends.RemoveAsync(bia.Id, friendship));

        Assert.Equal(FriendActionOutcome.NotFound, byStranger);
        Assert.Equal(FriendActionOutcome.Done, declined);
        Assert.Empty(afterDecline.Sent);
        Assert.Equal(FriendActionOutcome.Done, cancelled);
        Assert.Equal(FriendActionOutcome.Done, undone);
        Assert.False(await InScopeAsync(friends => friends.AreFriendsAsync(ana.Id, bia.Id)));
        Assert.Empty((await OverviewAsync(ana.Id)).Friends);
    }

    [Fact]
    public async Task Block_IsSilent_TheRequestLooksSentButNeverReachesWhoBlocked()
    {
        var ana = await StudentAsync("amigo.bloqueio.ana@gmail.com");
        var bia = await StudentAsync("amigo.bloqueio.bia@gmail.com");
        await BefriendAsync(ana, bia);
        var friendship = (await OverviewAsync(bia.Id)).Friends.Single().Id;

        var blocked = await InScopeAsync(friends => friends.BlockAsync(bia.Id, friendship));
        var retry = await InScopeAsync(friends => friends.RequestAsync(ana.Id, bia.Id));
        var anaSees = await OverviewAsync(ana.Id);
        var biaSees = await OverviewAsync(bia.Id);
        var hiddenRequest = anaSees.Sent.Single().Id;
        var acceptHidden = await InScopeAsync(friends => friends.AcceptAsync(bia.Id, hiddenRequest));
        var biaRequests = await InScopeAsync(friends => friends.RequestAsync(bia.Id, ana.Id));

        var unblocked = await InScopeAsync(friends => friends.UnblockAsync(bia.Id, biaSees.Blocked.Single().Id));
        var byOther = await InScopeAsync(friends => friends.UnblockAsync(ana.Id, biaSees.Blocked.Single().Id));
        var afterUnblock = await OverviewAsync(bia.Id);

        Assert.Equal(FriendActionOutcome.Done, blocked);
        Assert.False(await InScopeAsync(friends => friends.AreFriendsAsync(ana.Id, bia.Id)));
        // Para quem foi bloqueado, tudo parece normal: o pedido "foi enviado" e está na lista dele.
        Assert.Equal(FriendRequestOutcome.Requested, retry);
        Assert.Single(anaSees.Sent);
        Assert.Empty(anaSees.Blocked);
        // Quem bloqueou não vê o pedido nem consegue aceitá-lo pelo número.
        Assert.Empty(biaSees.Received);
        Assert.Equal("Teste A.", Assert.Single(biaSees.Blocked).Name);
        Assert.Equal(FriendActionOutcome.NotFound, acceptHidden);
        Assert.Equal(FriendRequestOutcome.YouBlocked, biaRequests);
        Assert.Equal(FriendActionOutcome.Done, unblocked);
        Assert.Equal(FriendActionOutcome.NotFound, byOther);
        // Desbloqueou: o pedido que estava guardado aparece.
        Assert.Single(afterUnblock.Received);
        Assert.Empty(afterUnblock.Blocked);
    }

    [Fact]
    public async Task Suggestions_ComeFromSharedDuelsAndTheTokenOnlyWorksForItsOwner()
    {
        var ana = await StudentAsync("amigo.sugestao.ana@gmail.com");
        var bia = await StudentAsync("amigo.sugestao.bia@gmail.com");
        var carla = await StudentAsync("amigo.sugestao.carla@gmail.com");
        using (var scope = factory.Services.CreateScope())
        {
            var duels = scope.ServiceProvider.GetRequiredService<DuelService>();
            var code = (await duels.CreateAsync(ana.Id)).Code!;
            await duels.JoinAsync(code, ana.Id);
            await duels.JoinAsync(code, bia.Id);
        }

        var forAna = await OverviewAsync(ana.Id);
        var forCarla = await OverviewAsync(carla.Id);
        var token = forAna.Suggestions.Single().Token;

        var stolen = await InScopeAsync(friends => friends.RequestSuggestedAsync(carla.Id, token));
        var tampered = await InScopeAsync(friends => friends.RequestSuggestedAsync(ana.Id, token + "x"));
        var requested = await InScopeAsync(friends => friends.RequestSuggestedAsync(ana.Id, token));
        var afterRequest = await OverviewAsync(ana.Id);

        Assert.Equal("Teste A.", forAna.Suggestions.Single().Name);
        // O token não é o id da conta e não carrega o e-mail.
        Assert.DoesNotContain(bia.Id, token);
        Assert.Empty(forCarla.Suggestions);
        Assert.Equal(FriendRequestOutcome.NotFound, stolen);
        Assert.Equal(FriendRequestOutcome.NotFound, tampered);
        Assert.Equal(FriendRequestOutcome.Requested, requested);
        Assert.Empty(afterRequest.Suggestions);
        Assert.Single((await OverviewAsync(bia.Id)).Received);
    }

    [Fact]
    public async Task Pages_TakeARequestFromTheLinkToAFriendshipWithoutLeakingAccountData()
    {
        var ana = await StudentAsync("amigo.pagina.ana@gmail.com");
        var bia = await StudentAsync("amigo.pagina.bia@gmail.com");
        var anaClient = await LoggedInAsync(ana.Email!);
        var biaClient = await LoggedInAsync(bia.Email!);

        // A Bia abre a página Amigos (o link nasce aí) e manda o link para a Ana.
        var biaHome = WebUtility.HtmlDecode(await biaClient.GetStringAsync("/amigos"));
        var code = await InScopeAsync(friends => friends.LinkAsync(bia.Id));

        var visitor = WebUtility.HtmlDecode(await NewClient().GetStringAsync($"/amigos/{code}"));
        var ownLink = WebUtility.HtmlDecode(await biaClient.GetStringAsync($"/amigos/{code}"));
        var invite = WebUtility.HtmlDecode(await anaClient.GetStringAsync($"/amigos/{code}"));
        var requested = await TestAccounts.SubmitFormAsync(anaClient, $"/amigos/{code}", "request-friend", []);

        var received = WebUtility.HtmlDecode(await biaClient.GetStringAsync("/amigos"));
        var requestId = (await OverviewAsync(bia.Id)).Received.Single().Id;
        var accepted = await TestAccounts.SubmitFormAsync(biaClient, "/amigos", "friend-action", new() { ["Command"] = $"accept:{requestId}" });
        var friends = WebUtility.HtmlDecode(await anaClient.GetStringAsync("/amigos"));
        var forged = await TestAccounts.SubmitFormAsync(anaClient, "/amigos", "friend-action", new() { ["Command"] = "accept:999999" });
        var missing = await NewClient().GetAsync("/amigos/naoexiste");
        var visitorHome = WebUtility.HtmlDecode(await NewClient().GetStringAsync("/amigos"));

        Assert.Contains($"amigos/{code}", biaHome);
        Assert.Contains("Seu link de amigo", biaHome);
        Assert.Contains("Teste A. te chamou para ser amigo", visitor);
        Assert.Contains("Entrar para pedir a amizade", visitor);
        Assert.Contains("Este é o seu link", ownLink);
        Assert.Contains("Vocês só viram amigos se Teste A. aceitar", invite);
        Assert.Contains("Pedir amizade", invite);
        Assert.Contains("amigos?aviso=pedido", requested.Headers.Location?.ToString());
        Assert.Contains("Pedidos de amizade", received);
        Assert.Contains("Aceitar", received);
        Assert.Contains("amigos?aviso=aceito", accepted.Headers.Location?.ToString());
        Assert.Contains("amigos desde", friends);
        Assert.Contains("Desfazer amizade", friends);
        // Nem o e-mail nem o id da conta do amigo vão para a página.
        Assert.DoesNotContain(bia.Email!, friends);
        Assert.DoesNotContain(bia.Id, friends);
        Assert.DoesNotContain(ana.Email!, received);
        Assert.DoesNotContain(ana.Id, received);
        Assert.Equal(HttpStatusCode.Conflict, forged.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Contains("crie sua conta", visitorHome);
        Assert.DoesNotContain("Seu link de amigo", visitorHome);
    }

    [Fact]
    public async Task DeletingTheAccount_ErasesLinkFriendshipsRequestsAndBlocks()
    {
        var leaving = await StudentAsync("amigo.exclui@gmail.com");
        var friend = await StudentAsync("amigo.exclui.amigo@gmail.com");
        var pending = await StudentAsync("amigo.exclui.pedido@gmail.com");
        var blockedOne = await StudentAsync("amigo.exclui.bloqueado@gmail.com");
        await InScopeAsync(friends => friends.LinkAsync(leaving.Id));
        await BefriendAsync(leaving, friend);
        await BefriendAsync(blockedOne, leaving);
        await InScopeAsync(friends => friends.RequestAsync(pending.Id, leaving.Id));
        int blockedFriendship;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            blockedFriendship = (await db.Friendships.SingleAsync(f => f.RequesterId == blockedOne.Id && f.AddresseeId == leaving.Id)).Id;
        }

        await InScopeAsync(friends => friends.BlockAsync(leaving.Id, blockedFriendship));
        var before = await OverviewAsync(leaving.Id);
        var client = await LoggedInAsync(leaving.Email!);

        var page = await client.GetStringAsync("/Account/Manage/DeletePersonalData");
        await TestAccounts.PostFormAsync(client, "/Account/Manage/DeletePersonalData", page, "delete-user", new Dictionary<string, string>
        {
            ["Input.Password"] = TestAccounts.Password,
        });

        using var check = factory.Services.CreateScope();
        var data = check.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Equal((1, 1, 1), (before.Friends.Count, before.Received.Count, before.Blocked.Count));
        Assert.Null(await check.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByIdAsync(leaving.Id));
        Assert.False(await data.FriendLinks.AnyAsync(l => l.UserId == leaving.Id));
        Assert.False(await data.Friendships.AnyAsync(f => f.RequesterId == leaving.Id || f.AddresseeId == leaving.Id));
        Assert.False(await data.FriendBlocks.AnyAsync(b => b.BlockerId == leaving.Id || b.BlockedId == leaving.Id));
        // Para quem ficou, a pessoa só some das listas.
        Assert.Empty((await OverviewAsync(friend.Id)).Friends);
        Assert.Empty((await OverviewAsync(pending.Id)).Sent);
    }

    [Fact]
    public async Task Download_BringsFriendsWithShortNamesOnly()
    {
        var student = await StudentAsync("amigo.download@gmail.com");
        var friend = await StudentAsync("amigo.download.amigo@gmail.com");
        var code = await InScopeAsync(friends => friends.LinkAsync(student.Id));
        await BefriendAsync(student, friend);
        var client = await LoggedInAsync(student.Email!);

        var page = await client.GetStringAsync("/Account/Manage/PersonalData");
        var download = await TestAccounts.PostFormAsync(client, "/Account/Manage/DownloadPersonalData", page, "", []);
        var body = await download.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        var relation = json.GetProperty("AmigosEPedidos").EnumerateArray().Single();

        Assert.Equal(code, json.GetProperty("LinkDeAmigo").GetString());
        Assert.Equal("Teste A.", relation.GetProperty("Pessoa").GetString());
        Assert.True(relation.GetProperty("PedidoPorVoce").GetBoolean());
        Assert.Equal(JsonValueKind.String, relation.GetProperty("AceitoEm").ValueKind);
        Assert.Empty(json.GetProperty("Bloqueados").EnumerateArray());
        // O arquivo é da pessoa: do amigo vai só o nome abreviado.
        Assert.DoesNotContain(friend.Email!, body);
        Assert.DoesNotContain(friend.Id, body);
    }

    [Fact]
    public async Task Pages_LinkFriendsAndExplainTheRules()
    {
        var student = await StudentAsync("amigo.menu@gmail.com");
        var client = await LoggedInAsync(student.Email!);

        var home = WebUtility.HtmlDecode(await client.GetStringAsync("/"));
        var privacy = WebUtility.HtmlDecode(await NewClient().GetStringAsync("/privacidade"));

        Assert.Contains("href=\"amigos\"", home);
        Assert.Contains("Só vira amigo quem você aceita", privacy);
        Assert.Contains("Um adolescente só vira amigo de quem ele mesmo aceitar", privacy);
    }
}
