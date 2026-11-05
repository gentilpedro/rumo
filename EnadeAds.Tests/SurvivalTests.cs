using System.Net;
using System.Text.RegularExpressions;
using EnadeAds.Accounts;
using EnadeAds.Achievements;
using EnadeAds.Data;
using EnadeAds.Questions;
using EnadeAds.Survival;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

[Collection(AppCollection.Name)]
public partial class SurvivalTests(EnadeAdsFactory factory)
{
    private async Task<T> InScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = factory.Services.CreateScope();
        return await action(scope.ServiceProvider);
    }

    private Task<SurvivalRun> RunAsync(string userId, int runId) =>
        InScopeAsync(async s => (await s.GetRequiredService<SurvivalService>().FindAsync(userId, runId))!);

    /// <summary>Responde a questão da vez, certa ou errada.</summary>
    private Task<SurvivalAnswer> PlayAsync(string userId, int runId, bool correct) =>
        InScopeAsync(async s =>
        {
            var service = s.GetRequiredService<SurvivalService>();
            var run = (await service.FindAsync(userId, runId))!;
            var question = (await service.QuestionAsync(run.CurrentQuestionId!.Value))!;
            var letter = question.Options.First(o => o.IsCorrect == correct).Letter;
            return await service.AnswerAsync(userId, runId, question.Id, letter);
        });

    [Fact]
    public async Task Run_EndsAfterThreeMistakesAndKeepsTheRecord()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "sobrevive.vidas@gmail.com", Roles.Student);
        var runId = (await InScopeAsync(s => s.GetRequiredService<SurvivalService>().StartAsync(student.Id)))!.Value;

        var start = await RunAsync(student.Id, runId);
        Assert.Equal(SurvivalService.StartingLives, start.Lives);
        Assert.Equal(start.QuestionIds.Count, start.QuestionIds.Distinct().Count());

        await PlayAsync(student.Id, runId, correct: true);
        await PlayAsync(student.Id, runId, correct: true);
        await PlayAsync(student.Id, runId, correct: false);
        await PlayAsync(student.Id, runId, correct: false);
        var alive = await RunAsync(student.Id, runId);
        Assert.True(alive.IsOpen);
        Assert.Equal((2, 1), (alive.Score, alive.Lives));

        await PlayAsync(student.Id, runId, correct: false);
        var ended = await RunAsync(student.Id, runId);
        Assert.False(ended.IsOpen);
        Assert.Equal(2, ended.Score);
        Assert.Equal(SurvivalAnswerOutcome.Ended, (await AnswerFirstQuestionAgainAsync(student.Id, runId)).Outcome);

        var record = await InScopeAsync(s => s.GetRequiredService<SurvivalService>().RecordAsync(student.Id, ended.TrackId));
        Assert.Equal(2, record);

        // As respostas contam como tentativas, para o desempenho e o XP.
        var attempts = await InScopeAsync(s => s.GetRequiredService<ApplicationDbContext>().QuestionAttempts.CountAsync(a => a.UserId == student.Id));
        Assert.Equal(5, attempts);
    }

    /// <summary>Tenta responder uma partida encerrada com a primeira questão dela.</summary>
    private Task<SurvivalAnswer> AnswerFirstQuestionAgainAsync(string userId, int runId) =>
        InScopeAsync(async s =>
        {
            var service = s.GetRequiredService<SurvivalService>();
            var run = (await service.FindAsync(userId, runId))!;
            return await service.AnswerAsync(userId, runId, run.QuestionIds[0], 'A');
        });

    [Fact]
    public async Task OldTab_DoesNotCountTwiceAndNewRunClosesTheOpenOne()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "sobrevive.aba@gmail.com", Roles.Student);
        var first = (await InScopeAsync(s => s.GetRequiredService<SurvivalService>().StartAsync(student.Id)))!.Value;
        var run = await RunAsync(student.Id, first);
        var question = run.CurrentQuestionId!.Value;

        await PlayAsync(student.Id, first, correct: true);
        // A mesma questão de novo (aba antiga): não conta.
        var stale = await InScopeAsync(s => s.GetRequiredService<SurvivalService>().AnswerAsync(student.Id, first, question, 'A'));
        Assert.Equal(SurvivalAnswerOutcome.Stale, stale.Outcome);
        Assert.Equal(1, (await RunAsync(student.Id, first)).Position);

        var second = (await InScopeAsync(s => s.GetRequiredService<SurvivalService>().StartAsync(student.Id)))!.Value;
        Assert.False((await RunAsync(student.Id, first)).IsOpen);
        Assert.True((await RunAsync(student.Id, second)).IsOpen);
    }

    [Fact]
    public async Task Pages_PlayARunUntilTheEnd()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "sobrevive.pagina@gmail.com", Roles.Student);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestAccounts.LoginAsync(client, student.Email!);

        var started = await TestAccounts.SubmitFormAsync(client, "/sobrevivencia", "start-survival", new());
        Assert.Equal(HttpStatusCode.Redirect, started.StatusCode);
        var url = started.Headers.Location!.ToString();
        var runId = int.Parse(url.Split('/')[^1]);

        string? last = null;
        for (var round = 0; round < SurvivalService.StartingLives; round++)
        {
            var page = await client.GetStringAsync(url);
            var questionId = QuestionIdField().Match(page).Groups[1].Value;
            var wrong = await InScopeAsync(async s => (await s.GetRequiredService<SurvivalService>().QuestionAsync(int.Parse(questionId)))!
                .Options.First(o => !o.IsCorrect).Letter);
            var answered = await TestAccounts.PostFormAsync(client, url, page, "survival-answer",
                new Dictionary<string, string> { ["QuestionId"] = questionId, ["Letter"] = wrong.ToString() });
            last = WebUtility.HtmlDecode(await answered.Content.ReadAsStringAsync());
            Assert.Contains("Errou: perdeu uma vida.", last);
        }

        Assert.Contains("Não foi dessa vez", last);
        Assert.Contains("Jogar de novo", last);
        Assert.False((await RunAsync(student.Id, runId)).IsOpen);

        var other = await TestAccounts.CreateConfirmedAsync(factory, "sobrevive.outro@gmail.com", Roles.Student);
        var otherClient = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestAccounts.LoginAsync(otherClient, other.Email!);
        Assert.Equal(HttpStatusCode.NotFound, (await otherClient.GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task Survivor_UnlocksWithTenInARun()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "sobrevive.dez@gmail.com", Roles.Student);
        var runId = (await InScopeAsync(s => s.GetRequiredService<SurvivalService>().StartAsync(student.Id)))!.Value;
        for (var i = 0; i < 10; i++)
        {
            await PlayAsync(student.Id, runId, correct: true);
        }

        var report = await InScopeAsync(s => s.GetRequiredService<AchievementService>().GetAsync(student.Id));
        Assert.Contains(report.Achievements, a => a.Key == "sobrevivente" && a.Unlocked);
    }

    [GeneratedRegex("name=\"QuestionId\" value=\"(\\d+)\"")]
    private static partial Regex QuestionIdField();
}
