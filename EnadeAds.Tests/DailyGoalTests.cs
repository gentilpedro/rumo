using System.Net;
using EnadeAds.Accounts;
using EnadeAds.Achievements;
using EnadeAds.Data;
using EnadeAds.Questions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

[Collection(AppCollection.Name)]
public class DailyGoalTests(EnadeAdsFactory factory)
{
    private async Task<XpReport> ReportAsync(string userId)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AchievementService>().GetAsync(userId);
    }

    private HttpClient NewClient() => factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task Goal_CountsDistinctQuestionsAnsweredToday()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "meta.contagem@gmail.com", Roles.Student);
        Assert.Equal(XpRules.DefaultDailyGoal, (await ReportAsync(student.Id)).DailyGoal);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var questions = await db.Questions.Include(q => q.Options)
                .Where(q => q.Kind == QuestionKind.Objective && q.ArchivedAt == null).OrderBy(q => q.Id).Take(5).ToListAsync();
            var bank = scope.ServiceProvider.GetRequiredService<QuestionBank>();
            foreach (var question in questions)
            {
                await bank.AnswerAsync(student.Id, question.Id, question.Options[0].Letter);
            }

            // Responder a mesma questão de novo não conta duas vezes para a meta.
            await bank.AnswerAsync(student.Id, questions[0].Id, questions[0].Options[1].Letter);
        }

        var report = await ReportAsync(student.Id);
        Assert.Equal(5, report.AnsweredToday);
        Assert.False(report.GoalMetToday);
        Assert.Equal(50, report.GoalPercent);
        Assert.Contains(report.Achievements, a => a.Key == "meta" && !a.Unlocked);

        var client = NewClient();
        await TestAccounts.LoginAsync(client, student.Email!);
        var saved = await TestAccounts.SubmitFormAsync(client, "/meu-desempenho", "daily-goal", new() { ["DailyGoal"] = "5" });
        var html = WebUtility.HtmlDecode(await saved.Content.ReadAsStringAsync());

        Assert.Contains(">5 de 5</span> questões", html);
        Assert.Contains("batida!", html);
        var after = await ReportAsync(student.Id);
        Assert.True(after.GoalMetToday);
        Assert.Contains(after.Achievements, a => a.Key == "meta" && a.Unlocked);
    }

    [Fact]
    public async Task Goal_OnlyAcceptsTheListedOptions()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "meta.invalida@gmail.com", Roles.Student);
        var client = NewClient();
        await TestAccounts.LoginAsync(client, student.Email!);

        var response = await TestAccounts.SubmitFormAsync(client, "/meu-desempenho", "daily-goal", new() { ["DailyGoal"] = "1000" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(XpRules.DefaultDailyGoal, (await ReportAsync(student.Id)).DailyGoal);
        Assert.Contains(">0 de 10</span> questões", WebUtility.HtmlDecode(await client.GetStringAsync("/")));
    }
}
