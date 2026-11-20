using System.Net;
using EnadeAds.Accounts;
using EnadeAds.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

[Collection(AppCollection.Name)]
public class AccountAdminTests(EnadeAdsFactory factory)
{
    private HttpClient NewClient() => factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task OnlyAdminsOpenTheAccountsPage()
    {
        await TestAccounts.CreateConfirmedAsync(factory, "contas.prof@faculdade.edu.br", Roles.Student, Roles.Teacher);
        var teacher = NewClient();
        await TestAccounts.LoginAsync(teacher, "contas.prof@faculdade.edu.br");

        var response = await teacher.GetAsync("/admin/contas");

        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Search_FindsByNameOrEmailAndFiltersByRole()
    {
        await TestAccounts.CreateConfirmedAsync(factory, "contas.busca.aluna@gmail.com", Roles.Student);
        await TestAccounts.CreateConfirmedAsync(factory, "contas.busca.prof@faculdade.edu.br", Roles.Student, Roles.Teacher);

        using var scope = factory.Services.CreateScope();
        var admin = scope.ServiceProvider.GetRequiredService<AccountAdmin>();
        var byEmail = await admin.SearchAsync("CONTAS.BUSCA", AccountFilter.All, 1);
        var teachers = await admin.SearchAsync("contas.busca", AccountFilter.Teachers, 1);
        var students = await admin.SearchAsync("contas.busca", AccountFilter.Students, 1);
        var byName = await admin.SearchAsync("teste contas.busca.aluna", AccountFilter.All, 1);

        Assert.Equal(2, byEmail.TotalCount);
        Assert.Equal("contas.busca.prof@faculdade.edu.br", Assert.Single(teachers.Items).Email);
        Assert.Equal("contas.busca.aluna@gmail.com", Assert.Single(students.Items).Email);
        Assert.Single(byName.Items);
    }

    [Fact]
    public async Task RevokeTeacher_KeepsTheStudentRole()
    {
        var admin = await TestAccounts.CreateConfirmedAsync(factory, "contas.admin1@faculdade.edu.br", Roles.Student, Roles.Admin);
        var teacher = await TestAccounts.CreateConfirmedAsync(factory, "contas.revoga@faculdade.edu.br", Roles.Student, Roles.Teacher);

        var result = await ApplyAsync(admin.Id, teacher.Id, AccountAction.RevokeTeacher);
        var again = await ApplyAsync(admin.Id, teacher.Id, AccountAction.RevokeTeacher);

        Assert.True(result.Succeeded);
        Assert.Equal([Roles.Student], await TestAccounts.RolesOfAsync(factory, teacher.Email!));
        Assert.False(again.Succeeded);
    }

    [Fact]
    public async Task BlockedAccountCannotLogInUntilUnblocked()
    {
        var admin = await TestAccounts.CreateConfirmedAsync(factory, "contas.admin2@faculdade.edu.br", Roles.Student, Roles.Admin);
        var student = await TestAccounts.CreateConfirmedAsync(factory, "contas.bloqueio@gmail.com", Roles.Student);

        await ApplyAsync(admin.Id, student.Id, AccountAction.Block);
        var blocked = await TryLoginAsync(student.Email!);
        await ApplyAsync(admin.Id, student.Id, AccountAction.Unblock);
        var unblocked = await TryLoginAsync(student.Email!);

        Assert.Contains("/Account/Lockout", blocked);
        Assert.DoesNotContain("/Account/Lockout", unblocked);
        Assert.DoesNotContain("Account/Login", unblocked);
    }

    [Fact]
    public async Task AdminCannotChangeOwnAccountButCanRemoveAnotherAdmin()
    {
        var admin = await TestAccounts.CreateConfirmedAsync(factory, "contas.admin3@faculdade.edu.br", Roles.Student, Roles.Admin);
        var other = await TestAccounts.CreateConfirmedAsync(factory, "contas.admin4@faculdade.edu.br", Roles.Student, Roles.Admin);

        var selfBlock = await ApplyAsync(admin.Id, admin.Id, AccountAction.Block);
        var selfRemove = await ApplyAsync(admin.Id, admin.Id, AccountAction.RemoveAdmin);
        var removeOther = await ApplyAsync(admin.Id, other.Id, AccountAction.RemoveAdmin);

        Assert.False(selfBlock.Succeeded);
        Assert.False(selfRemove.Succeeded);
        Assert.Contains(Roles.Admin, await TestAccounts.RolesOfAsync(factory, admin.Email!));
        Assert.True(removeOther.Succeeded);
        Assert.DoesNotContain(Roles.Admin, await TestAccounts.RolesOfAsync(factory, other.Email!));
    }

    [Fact]
    public async Task Page_AppliesActionsAndHidesThemOnOwnRow()
    {
        await TestAccounts.CreateConfirmedAsync(factory, "contas.pagina.admin@faculdade.edu.br", Roles.Student, Roles.Admin);
        var target = await TestAccounts.CreateConfirmedAsync(factory, "contas.pagina.alvo@gmail.com", Roles.Student);
        var client = NewClient();
        await TestAccounts.LoginAsync(client, "contas.pagina.admin@faculdade.edu.br");

        var own = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/contas?busca=contas.pagina.admin"));
        var url = "/admin/contas?busca=contas.pagina.alvo";
        var response = await TestAccounts.SubmitFormAsync(client, url, "account-action", new() { ["Command"] = $"Block:{target.Id}" });
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("você", own);
        Assert.DoesNotContain("Bloquear", own);
        Assert.Contains("foi bloqueado", html);
        Assert.Contains("Bloqueada", html);
    }

    [Fact]
    public async Task MakeTeacher_WorksForAnyAccountAndClosesAPendingRequest()
    {
        var admin = await TestAccounts.CreateConfirmedAsync(factory, "contas.admin5@faculdade.edu.br", Roles.Student, Roles.Admin);
        // E-mail comum, sem pedido: é uma decisão do admin, não passa pela regra do e-mail institucional.
        var student = await TestAccounts.CreateConfirmedAsync(factory, "contas.vira.prof@gmail.com", Roles.Student);
        var pending = await TestAccounts.CreateConfirmedAsync(factory, "contas.pendente@faculdade.edu.br", Roles.Student, Roles.PendingTeacher);

        var promoted = await ApplyAsync(admin.Id, student.Id, AccountAction.MakeTeacher);
        var again = await ApplyAsync(admin.Id, student.Id, AccountAction.MakeTeacher);
        var fromPending = await ApplyAsync(admin.Id, pending.Id, AccountAction.MakeTeacher);

        Assert.True(promoted.Succeeded);
        Assert.Equal([Roles.Student, Roles.Teacher], (await TestAccounts.RolesOfAsync(factory, student.Email!)).Order());
        Assert.False(again.Succeeded);
        Assert.True(fromPending.Succeeded);
        Assert.Equal([Roles.Student, Roles.Teacher], (await TestAccounts.RolesOfAsync(factory, pending.Email!)).Order());
    }

    [Fact]
    public async Task MakeAdmin_GivesTheRoleAndTheNewAdminOpensThePanel()
    {
        var admin = await TestAccounts.CreateConfirmedAsync(factory, "contas.admin6@faculdade.edu.br", Roles.Student, Roles.Admin);
        var student = await TestAccounts.CreateConfirmedAsync(factory, "contas.vira.admin@gmail.com", Roles.Student);

        var promoted = await ApplyAsync(admin.Id, student.Id, AccountAction.MakeAdmin);
        var again = await ApplyAsync(admin.Id, student.Id, AccountAction.MakeAdmin);
        var client = NewClient();
        await TestAccounts.LoginAsync(client, student.Email!);
        var panel = await client.GetAsync("/admin/contas");

        Assert.True(promoted.Succeeded);
        Assert.Contains(Roles.Admin, await TestAccounts.RolesOfAsync(factory, student.Email!));
        Assert.False(again.Succeeded);
        Assert.Equal(HttpStatusCode.OK, panel.StatusCode);
    }

    [Theory]
    [InlineData(AccountAction.MakeTeacher)]
    [InlineData(AccountAction.MakeAdmin)]
    public async Task Promotion_NeedsAConfirmedEmailAndAnUnblockedAccount(AccountAction action)
    {
        var admin = await TestAccounts.CreateConfirmedAsync(factory, $"contas.admin.{action}@faculdade.edu.br", Roles.Student, Roles.Admin);
        var unconfirmed = await TestAccounts.CreateConfirmedAsync(factory, $"contas.sem.email.{action}@gmail.com", Roles.Student);
        var blocked = await TestAccounts.CreateConfirmedAsync(factory, $"contas.bloqueada.{action}@gmail.com", Roles.Student);
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = (await users.FindByIdAsync(unconfirmed.Id))!;
            user.EmailConfirmed = false;
            await users.UpdateAsync(user);
        }

        await ApplyAsync(admin.Id, blocked.Id, AccountAction.Block);
        var withoutEmail = await ApplyAsync(admin.Id, unconfirmed.Id, action);
        var whileBlocked = await ApplyAsync(admin.Id, blocked.Id, action);
        var self = await ApplyAsync(admin.Id, admin.Id, action);

        Assert.False(withoutEmail.Succeeded);
        Assert.Contains("confirmou o e-mail", withoutEmail.Message);
        Assert.False(whileBlocked.Succeeded);
        Assert.Contains("bloqueada", whileBlocked.Message);
        Assert.False(self.Succeeded);
        Assert.Equal([Roles.Student], await TestAccounts.RolesOfAsync(factory, unconfirmed.Email!));
        Assert.Equal([Roles.Student], await TestAccounts.RolesOfAsync(factory, blocked.Email!));
    }

    [Fact]
    public async Task Page_OffersPromotionAndAppliesIt()
    {
        await TestAccounts.CreateConfirmedAsync(factory, "contas.promove.admin@faculdade.edu.br", Roles.Student, Roles.Admin);
        var target = await TestAccounts.CreateConfirmedAsync(factory, "contas.promove.alvo@gmail.com", Roles.Student);
        var client = NewClient();
        await TestAccounts.LoginAsync(client, "contas.promove.admin@faculdade.edu.br");

        var url = "/admin/contas?busca=contas.promove.alvo";
        var before = WebUtility.HtmlDecode(await client.GetStringAsync(url));
        var response = await TestAccounts.SubmitFormAsync(client, url, "account-action", new() { ["Command"] = $"MakeAdmin:{target.Id}" });
        var after = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("Tornar professor", before);
        // Dar admin pede dois cliques: o botão que envia fica dentro do <details>.
        Assert.Matches("(?s)<details class=\"promote-admin\">.*Confirmar: tornar admin.*</details>", before);
        Assert.Contains("agora é administrador", after);
        Assert.Contains("Remover admin", after);
        Assert.DoesNotContain("Confirmar: tornar admin", after);
    }

    private async Task<EditResultMessage> ApplyAsync(string adminId, string targetId, AccountAction action)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AccountAdmin>().ApplyAsync(adminId, targetId, action);
    }

    /// <summary>Tenta entrar e devolve para onde o login redirecionou.</summary>
    private async Task<string> TryLoginAsync(string email)
    {
        var client = NewClient();
        var response = await TestAccounts.SubmitFormAsync(client, "/Account/Login", "login", new()
        {
            ["Input.Email"] = email,
            ["Input.Password"] = TestAccounts.Password,
        });
        return response.Headers.Location?.ToString() ?? "";
    }
}
