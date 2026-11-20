using System.Net;
using System.Text.Json;
using EnadeAds.Accounts;
using EnadeAds.Data;
using EnadeAds.Rooms;
using EnadeAds.Study;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

public class StudyRoomNameTests
{
    [Theory]
    [InlineData("  Turma   do 3º B ", "Turma do 3º B")]
    [InlineData("abc", "abc")]
    [InlineData("Sala\tdo\ncursinho", "Sala do cursinho")]
    public void Name_IsTrimmedAndKeepsSingleSpaces(string typed, string expected)
    {
        Assert.Equal(expected, StudyRoomService.NormalizeName(typed));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ab  ")]
    [InlineData("0123456789012345678901234567890123456789012345678901234567890")]
    public void Name_OutsideTheSizeIsRefused(string? typed)
    {
        Assert.Null(StudyRoomService.NormalizeName(typed));
    }
}

[Collection(AppCollection.Name)]
public class StudyRoomTests(EnadeAdsFactory factory)
{
    private HttpClient NewClient() => factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private async Task<T> InScopeAsync<T>(Func<StudyRoomService, Task<T>> action)
    {
        using var scope = factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<StudyRoomService>());
    }

    private Task<ApplicationUser> StudentAsync(string email) => TestAccounts.CreateConfirmedAsync(factory, email, Roles.Student);

    private async Task<string> CreateRoomAsync(string ownerId, string name = "Turma de teste") =>
        (await InScopeAsync(rooms => rooms.CreateAsync(ownerId, name))).Code!;

    private async Task<HttpClient> LoggedInAsync(string email)
    {
        var client = NewClient();
        await TestAccounts.LoginAsync(client, email);
        return client;
    }

    [Fact]
    public async Task Create_MakesTheCreatorOwnerAndMemberInTheActiveTrack()
    {
        var owner = await StudentAsync("sala.cria@gmail.com");

        var created = await InScopeAsync(rooms => rooms.CreateAsync(owner.Id, "  Turma   do 3º B "));
        var invalid = await InScopeAsync(rooms => rooms.CreateAsync(owner.Id, "ab"));
        var room = (await InScopeAsync(rooms => rooms.FindAsync(created.Code!)))!;

        Assert.Equal(CreateRoomOutcome.Created, created.Outcome);
        Assert.Equal(StudyRoomService.CodeLength, created.Code!.Length);
        Assert.Equal("Turma do 3º B", room.Name);
        Assert.Equal(Tracks.Default, room.TrackId);
        Assert.Equal(owner.Id, room.OwnerId);
        Assert.Equal(owner.Id, Assert.Single(room.Members).UserId);
        Assert.Equal(CreateRoomOutcome.InvalidName, invalid.Outcome);
    }

    [Fact]
    public async Task Join_NeedsTheLinkAndHappensOnlyOnce()
    {
        var owner = await StudentAsync("sala.entra.dono@gmail.com");
        var member = await StudentAsync("sala.entra.membro@gmail.com");
        var code = await CreateRoomAsync(owner.Id);

        // O código vale com maiúsculas e espaços: pode ter sido digitado.
        var joined = await InScopeAsync(rooms => rooms.JoinAsync($" {code.ToUpperInvariant()} ", member.Id));
        var again = await InScopeAsync(rooms => rooms.JoinAsync(code, member.Id));
        var ownerAgain = await InScopeAsync(rooms => rooms.JoinAsync(code, owner.Id));
        var wrongCode = await InScopeAsync(rooms => rooms.JoinAsync("naoexiste", member.Id));
        var room = (await InScopeAsync(rooms => rooms.FindAsync(code)))!;

        Assert.Equal(JoinRoomOutcome.Joined, joined);
        Assert.Equal(JoinRoomOutcome.AlreadyMember, again);
        Assert.Equal(JoinRoomOutcome.AlreadyMember, ownerAgain);
        Assert.Equal(JoinRoomOutcome.NotFound, wrongCode);
        Assert.Equal(2, room.Members.Count);
    }

    [Fact]
    public async Task Leave_PassesTheRoomToTheOldestMemberAndDeletesTheEmptyRoom()
    {
        var owner = await StudentAsync("sala.sai.dono@gmail.com");
        var second = await StudentAsync("sala.sai.segundo@gmail.com");
        var third = await StudentAsync("sala.sai.terceiro@gmail.com");
        var code = await CreateRoomAsync(owner.Id);
        await InScopeAsync(rooms => rooms.JoinAsync(code, second.Id));
        await InScopeAsync(rooms => rooms.JoinAsync(code, third.Id));

        await InScopeAsync(rooms => rooms.LeaveAsync(code, owner.Id));
        var afterOwnerLeft = (await InScopeAsync(rooms => rooms.FindAsync(code)))!;
        var notMember = await InScopeAsync(rooms => rooms.LeaveAsync(code, owner.Id));
        await InScopeAsync(rooms => rooms.LeaveAsync(code, second.Id));
        await InScopeAsync(rooms => rooms.LeaveAsync(code, third.Id));
        var afterEveryoneLeft = await InScopeAsync(rooms => rooms.FindAsync(code));

        Assert.Equal(second.Id, afterOwnerLeft.OwnerId);
        Assert.Equal(2, afterOwnerLeft.Members.Count);
        Assert.Equal(RoomActionOutcome.NotAllowed, notMember);
        Assert.Null(afterEveryoneLeft);
    }

    [Fact]
    public async Task OnlyTheOwnerRenamesRenewsTheLinkRemovesMembersAndDeletes()
    {
        var owner = await StudentAsync("sala.dono@gmail.com");
        var member = await StudentAsync("sala.membro@gmail.com");
        var code = await CreateRoomAsync(owner.Id);
        await InScopeAsync(rooms => rooms.JoinAsync(code, member.Id));
        var memberId = (await InScopeAsync(rooms => rooms.FindAsync(code)))!.Members.Single(m => m.UserId == member.Id).Id;
        var ownerMemberId = (await InScopeAsync(rooms => rooms.FindAsync(code)))!.Members.Single(m => m.UserId == owner.Id).Id;

        var memberRenames = await InScopeAsync(rooms => rooms.RenameAsync(code, member.Id, "Nome do membro"));
        var memberRenews = await InScopeAsync(rooms => rooms.RenewCodeAsync(code, member.Id));
        var memberRemoves = await InScopeAsync(rooms => rooms.RemoveMemberAsync(code, member.Id, ownerMemberId));
        var memberDeletes = await InScopeAsync(rooms => rooms.DeleteAsync(code, member.Id));

        var renamed = await InScopeAsync(rooms => rooms.RenameAsync(code, owner.Id, "Nome novo"));
        var badName = await InScopeAsync(rooms => rooms.RenameAsync(code, owner.Id, " "));
        var newCode = (await InScopeAsync(rooms => rooms.RenewCodeAsync(code, owner.Id)))!;
        var oldLink = await InScopeAsync(rooms => rooms.FindAsync(code));
        var ownerRemovesSelf = await InScopeAsync(rooms => rooms.RemoveMemberAsync(newCode, owner.Id, ownerMemberId));
        var removed = await InScopeAsync(rooms => rooms.RemoveMemberAsync(newCode, owner.Id, memberId));
        var room = (await InScopeAsync(rooms => rooms.FindAsync(newCode)))!;
        var deleted = await InScopeAsync(rooms => rooms.DeleteAsync(newCode, owner.Id));

        Assert.Equal(RoomActionOutcome.NotAllowed, memberRenames);
        Assert.Null(memberRenews);
        Assert.Equal(RoomActionOutcome.NotAllowed, memberRemoves);
        Assert.Equal(RoomActionOutcome.NotAllowed, memberDeletes);
        Assert.Equal(RoomActionOutcome.Done, renamed);
        Assert.Equal(RoomActionOutcome.InvalidName, badName);
        Assert.NotEqual(code, newCode);
        // O link antigo para de funcionar: é assim que o dono impede a volta de quem foi removido.
        Assert.Null(oldLink);
        Assert.Equal(RoomActionOutcome.NotFound, ownerRemovesSelf);
        Assert.Equal(RoomActionOutcome.Done, removed);
        Assert.Equal("Nome novo", room.Name);
        Assert.Equal(owner.Id, Assert.Single(room.Members).UserId);
        Assert.Equal(RoomActionOutcome.Done, deleted);
        Assert.Null(await InScopeAsync(rooms => rooms.FindAsync(newCode)));
    }

    [Fact]
    public async Task Invite_ShowsTheRoomButNeverTheMembersToWhoIsOutside()
    {
        var owner = await TestAccounts.CreateConfirmedAsync(factory, "sala.convite.dono@gmail.com", Roles.Student);
        var guest = await StudentAsync("sala.convite.visita@gmail.com");
        var code = await CreateRoomAsync(owner.Id, "Turma do convite");
        var ownerName = "Teste S.";

        var visitor = WebUtility.HtmlDecode(await NewClient().GetStringAsync($"/salas/{code}"));
        var guestClient = await LoggedInAsync(guest.Email!);
        var loggedOutsider = WebUtility.HtmlDecode(await guestClient.GetStringAsync($"/salas/{code}"));
        var joined = await TestAccounts.SubmitFormAsync(guestClient, $"/salas/{code}", "join-room", []);
        var asMember = WebUtility.HtmlDecode(await guestClient.GetStringAsync($"/salas/{code}"));
        var missing = await NewClient().GetAsync("/salas/naoexiste");

        // Quem só tem o link vê o nome da sala, a prova e o tamanho, e o aviso do que os membros vão ver.
        Assert.Contains("Você foi convidado para a sala Turma do convite", visitor);
        Assert.Contains("1 membro", visitor);
        Assert.Contains("primeiro nome com a inicial do sobrenome", visitor);
        Assert.Contains("Entrar para participar", visitor);
        Assert.DoesNotContain(ownerName, visitor);
        Assert.DoesNotContain("Link de convite", visitor);
        Assert.Contains("Entrar na sala", loggedOutsider);
        Assert.DoesNotContain(ownerName, loggedOutsider);

        Assert.Equal(HttpStatusCode.Redirect, joined.StatusCode);
        Assert.Contains($"salas/{code}?aviso=entrou", joined.Headers.Location?.ToString());
        // Dentro da sala: nomes abreviados, nunca o e-mail nem o id da conta de outra pessoa.
        Assert.Contains(ownerName, asMember);
        Assert.Contains("criou a sala", asMember);
        Assert.Contains("Link de convite", asMember);
        Assert.DoesNotContain(owner.Email!, asMember);
        Assert.DoesNotContain(owner.Id, asMember);
        Assert.DoesNotContain("Administrar a sala", asMember);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Page_LetsTheOwnerManageAndAMemberLeave()
    {
        var owner = await StudentAsync("sala.pagina.dono@gmail.com");
        var member = await StudentAsync("sala.pagina.membro@gmail.com");
        var ownerClient = await LoggedInAsync(owner.Email!);
        var memberClient = await LoggedInAsync(member.Email!);

        var created = await TestAccounts.SubmitFormAsync(ownerClient, "/salas", "create-room", new() { ["Name"] = "Sala da página" });
        var url = created.Headers.Location!.ToString();
        var code = url[(url.LastIndexOf('/') + 1)..];
        await TestAccounts.SubmitFormAsync(memberClient, $"/salas/{code}", "join-room", []);

        var left = await TestAccounts.SubmitFormAsync(memberClient, $"/salas/{code}", "leave-room", []);
        var membersAfterLeaving = (await InScopeAsync(rooms => rooms.FindAsync(code)))!.Members.Count;
        await TestAccounts.SubmitFormAsync(memberClient, $"/salas/{code}", "join-room", []);
        var list = WebUtility.HtmlDecode(await ownerClient.GetStringAsync("/salas"));
        var manage = WebUtility.HtmlDecode(await ownerClient.GetStringAsync($"/salas/{code}"));
        var unconfirmedDelete = await TestAccounts.SubmitFormAsync(ownerClient, $"/salas/{code}", "delete-room", []);
        var renewed = await TestAccounts.SubmitFormAsync(ownerClient, $"/salas/{code}", "renew-link", []);
        var newUrl = renewed.Headers.Location!.ToString();
        var newCode = newUrl[(newUrl.LastIndexOf('/') + 1)..newUrl.IndexOf('?')];
        var deleted = await TestAccounts.SubmitFormAsync(ownerClient, $"/salas/{newCode}", "delete-room", new() { ["ConfirmDelete"] = "true" });

        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
        // Sair leva de volta para a lista de salas.
        Assert.EndsWith("salas", left.Headers.Location?.ToString());
        Assert.Equal(1, membersAfterLeaving);
        Assert.Contains("Sala da página", list);
        Assert.Contains("você criou", list);
        Assert.Contains("Administrar a sala", manage);
        Assert.Contains("Gerar link novo", manage);
        Assert.Contains("Remover", manage);
        Assert.Equal(HttpStatusCode.Conflict, unconfirmedDelete.StatusCode);
        Assert.NotEqual(code, newCode);
        Assert.EndsWith("salas", deleted.Headers.Location?.ToString());
        Assert.Null(await InScopeAsync(rooms => rooms.FindAsync(newCode)));
    }

    [Fact]
    public async Task Report_GoesToModerationWithoutNamesAndCanRenameOrDeleteTheRoom()
    {
        var owner = await StudentAsync("sala.denuncia.dono@gmail.com");
        var member = await StudentAsync("sala.denuncia.membro@gmail.com");
        var outsider = await StudentAsync("sala.denuncia.fora@gmail.com");
        await TestAccounts.CreateConfirmedAsync(factory, "sala.denuncia.prof@faculdade.edu.br", Roles.Student, Roles.Teacher);
        var code = await CreateRoomAsync(owner.Id, "Nome ofensivo");
        await InScopeAsync(rooms => rooms.JoinAsync(code, member.Id));
        var memberClient = await LoggedInAsync(member.Email!);
        var teacherClient = await LoggedInAsync("sala.denuncia.prof@faculdade.edu.br");

        var fromOutside = await InScopeAsync(rooms => rooms.ReportAsync(code, outsider.Id, "não sou membro"));
        var reported = await TestAccounts.SubmitFormAsync(memberClient, $"/salas/{code}", "report-room", new() { ["Reason"] = "O nome xinga um colega" });
        await InScopeAsync(rooms => rooms.ReportAsync(code, member.Id, "de novo"));
        var open = await InScopeAsync(rooms => rooms.OpenReportsAsync());
        var studentOnModeration = await memberClient.GetAsync("/moderacao");
        var moderation = WebUtility.HtmlDecode(await teacherClient.GetStringAsync("/moderacao"));

        var roomId = (await InScopeAsync(rooms => rooms.FindAsync(code)))!.Id;
        var renamed = await TestAccounts.SubmitFormAsync(teacherClient, "/moderacao", "room-moderation", new() { ["Decision"] = $"rename:{roomId}" });
        var afterRename = (await InScopeAsync(rooms => rooms.FindAsync(code)))!;
        var twice = await TestAccounts.SubmitFormAsync(teacherClient, "/moderacao", "room-moderation", new() { ["Decision"] = $"delete:{roomId}" });

        await InScopeAsync(rooms => rooms.ReportAsync(code, member.Id, "continua errado"));
        var deleted = await TestAccounts.SubmitFormAsync(teacherClient, "/moderacao", "room-moderation", new() { ["Decision"] = $"delete:{roomId}" });

        Assert.Equal(RoomActionOutcome.NotAllowed, fromOutside);
        Assert.Contains("aviso=denuncia", reported.Headers.Location?.ToString());
        // Uma denúncia aberta por pessoa em cada sala.
        var group = Assert.Single(open, g => g.RoomId == roomId);
        Assert.Equal(1, group.Count);
        Assert.Equal(["O nome xinga um colega"], group.Reasons);
        Assert.Contains("/Account/AccessDenied", studentOnModeration.Headers.Location?.ToString());
        // A moderação vê o nome da sala e o motivo, não os membros nem quem denunciou.
        Assert.Contains("Salas de estudo denunciadas", moderation);
        Assert.Contains("Nome ofensivo", moderation);
        Assert.Contains("O nome xinga um colega", moderation);
        Assert.DoesNotContain(member.Email!, moderation);
        Assert.DoesNotContain("sala.denuncia.dono", moderation);
        Assert.Equal(HttpStatusCode.Redirect, renamed.StatusCode);
        Assert.Equal(StudyRoomService.NeutralName, afterRename.Name);
        // Denúncia já resolvida: a segunda decisão não faz nada.
        Assert.Equal(HttpStatusCode.BadRequest, twice.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, deleted.StatusCode);
        Assert.Null(await InScopeAsync(rooms => rooms.FindAsync(code)));
    }

    [Fact]
    public async Task DeletingTheAccount_RemovesMembershipsAndHandsOverOrDeletesOwnedRooms()
    {
        var leaving = await StudentAsync("sala.exclui@gmail.com");
        var colleague = await StudentAsync("sala.exclui.colega@gmail.com");
        var shared = await CreateRoomAsync(leaving.Id, "Sala com colega");
        var alone = await CreateRoomAsync(leaving.Id, "Sala só minha");
        var others = await CreateRoomAsync(colleague.Id, "Sala do colega");
        await InScopeAsync(rooms => rooms.JoinAsync(shared, colleague.Id));
        await InScopeAsync(rooms => rooms.JoinAsync(others, leaving.Id));
        var client = await LoggedInAsync(leaving.Email!);

        var page = await client.GetStringAsync("/Account/Manage/DeletePersonalData");
        await TestAccounts.PostFormAsync(client, "/Account/Manage/DeletePersonalData", page, "delete-user", new Dictionary<string, string>
        {
            ["Input.Password"] = TestAccounts.Password,
        });

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        Assert.Null(await users.FindByIdAsync(leaving.Id));
        Assert.False(await db.StudyRoomMembers.AnyAsync(m => m.UserId == leaving.Id));
        // A sala em que só ela estava some na hora, com o nome que ela escreveu.
        Assert.False(await db.StudyRooms.AnyAsync(r => r.Code == alone));
        Assert.Equal(colleague.Id, (await db.StudyRooms.SingleAsync(r => r.Code == shared)).OwnerId);
        var othersId = (await db.StudyRooms.SingleAsync(r => r.Code == others)).Id;
        Assert.Equal(1, await db.StudyRoomMembers.CountAsync(m => m.RoomId == othersId));
    }

    [Fact]
    public async Task RoomLeftWithoutOwner_IsRepairedOnTheNextVisit()
    {
        // Conta apagada por fora da tela (direto no banco, por exemplo): o banco zera o dono e tira a participação.
        var owner = await StudentAsync("sala.conserto.dono@gmail.com");
        var member = await StudentAsync("sala.conserto.membro@gmail.com");
        var shared = await CreateRoomAsync(owner.Id, "Sala que fica");
        var empty = await CreateRoomAsync(owner.Id, "Sala que some");
        await InScopeAsync(rooms => rooms.JoinAsync(shared, member.Id));

        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await users.DeleteAsync((await users.FindByIdAsync(owner.Id))!);
        }

        var repaired = await InScopeAsync(rooms => rooms.FindAsync(shared));
        var gone = await InScopeAsync(rooms => rooms.FindAsync(empty));

        Assert.Equal(member.Id, repaired!.OwnerId);
        Assert.Null(gone);
    }

    [Fact]
    public async Task Download_ListsTheRoomsOfTheAccount()
    {
        var student = await StudentAsync("sala.download@gmail.com");
        var other = await StudentAsync("sala.download.outro@gmail.com");
        await CreateRoomAsync(student.Id, "Minha sala exportada");
        var code = await CreateRoomAsync(other.Id, "Sala de outra pessoa");
        await InScopeAsync(rooms => rooms.JoinAsync(code, student.Id));
        await InScopeAsync(rooms => rooms.ReportAsync(code, student.Id, "motivo exportado"));
        var client = await LoggedInAsync(student.Email!);

        var page = await client.GetStringAsync("/Account/Manage/PersonalData");
        var download = await TestAccounts.PostFormAsync(client, "/Account/Manage/DownloadPersonalData", page, "", []);
        var body = await download.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        var rooms = json.GetProperty("SalasDeEstudo").EnumerateArray().ToList();

        Assert.Equal(2, rooms.Count);
        Assert.Contains(rooms, r => r.GetProperty("Sala").GetString() == "Minha sala exportada" && r.GetProperty("CriadaPorVoce").GetBoolean());
        Assert.Contains(rooms, r => r.GetProperty("Sala").GetString() == "Sala de outra pessoa" && !r.GetProperty("CriadaPorVoce").GetBoolean());
        Assert.Equal("motivo exportado", json.GetProperty("DenunciasDeSalas")[0].GetProperty("Motivo").GetString());
        // Só as salas da própria pessoa: o arquivo não traz os outros membros.
        Assert.DoesNotContain(other.Email!, body);
    }

    [Fact]
    public async Task Pages_LinkTheRoomsAndExplainTheRules()
    {
        var home = WebUtility.HtmlDecode(await NewClient().GetStringAsync("/salas"));
        var privacy = WebUtility.HtmlDecode(await NewClient().GetStringAsync("/privacidade"));

        Assert.Contains("href=\"salas\"", WebUtility.HtmlDecode(await NewClient().GetStringAsync("/")));
        Assert.Contains("Só entra quem tem o link", home);
        Assert.Contains("crie sua conta", home);
        Assert.Contains("Nas salas de estudo, os outros membros da mesma sala veem", privacy);
        Assert.Contains("não têm conversa nem mensagens entre usuários", privacy);
    }
}
