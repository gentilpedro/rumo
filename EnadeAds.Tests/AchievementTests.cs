using System.Net;
using EnadeAds.Accounts;
using EnadeAds.Achievements;
using EnadeAds.Data;
using EnadeAds.Duels;
using EnadeAds.Questions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

public class XpRuleTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(49, 1)]
    [InlineData(50, 2)]
    [InlineData(149, 2)]
    [InlineData(150, 3)]
    [InlineData(300, 4)]
    [InlineData(1_800, 9)]
    [InlineData(1_000_000, 9)]
    public void Levels_NeedFiftyMoreXpEachTime(int xp, int level)
    {
        Assert.Equal(level, Level.Of(xp).Number);
    }

    [Fact]
    public void Levels_ShowTheWayToTheNext()
    {
        var level = Level.Of(100);

        Assert.Equal(("Aprendiz", 50, 150), (level.Title, level.Floor, level.NextAt!.Value));
        Assert.Equal(50, level.PercentTo(100));
        Assert.Null(Level.Of(5_000).NextAt);
        Assert.Equal(100, Level.Of(5_000).PercentTo(5_000));
    }

    [Theory]
    [InlineData(false, false, true, 12)]
    [InlineData(false, false, false, 2)]
    [InlineData(true, false, true, 10)]
    [InlineData(true, true, true, 0)]
    [InlineData(true, false, false, 0)]
    public void Answer_RewardsOnlyTheFirstAnswerAndTheFirstHit(bool answeredBefore, bool correctBefore, bool isCorrect, int xp)
    {
        Assert.Equal(xp, XpRules.ForAnswer(answeredBefore, correctBefore, isCorrect));
    }

    [Fact]
    public void Streaks_CountConsecutiveDaysAndSurviveUntilTheEndOfTheNextDay()
    {
        var today = new DateOnly(2026, 11, 20);
        DateOnly Ago(int days) => today.AddDays(-days);

        Assert.Equal((3, 3), AchievementService.Streaks(new HashSet<DateOnly> { Ago(0), Ago(1), Ago(2) }, today));
        // Estudou até ontem: a sequência continua valendo hoje.
        Assert.Equal((2, 4), AchievementService.Streaks(new HashSet<DateOnly> { Ago(1), Ago(2), Ago(5), Ago(6), Ago(7), Ago(8) }, today));
        Assert.Equal((0, 1), AchievementService.Streaks(new HashSet<DateOnly> { Ago(3) }, today));
        Assert.Equal((0, 0), AchievementService.Streaks(new HashSet<DateOnly>(), today));
    }
}

[Collection(AppCollection.Name)]
public class AchievementTests(EnadeAdsFactory factory)
{
    private async Task<XpReport> ReportAsync(string userId)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AchievementService>().GetAsync(userId);
    }

    private async Task<(Question Question, char Right, char Wrong)> ObjectiveAsync(int skip)
    {
        using var scope = factory.Services.CreateScope();
        var question = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Questions.AsNoTracking()
            .Include(q => q.Options)
            .Where(q => q.Kind == QuestionKind.Objective && q.ArchivedAt == null)
            .OrderBy(q => q.Id).Skip(skip).FirstAsync();
        return (question, question.Options.First(o => o.IsCorrect).Letter, question.Options.First(o => !o.IsCorrect).Letter);
    }

    private async Task<AnswerResult> AnswerAsync(string userId, int questionId, char letter)
    {
        using var scope = factory.Services.CreateScope();
        return (await scope.ServiceProvider.GetRequiredService<QuestionBank>().AnswerAsync(userId, questionId, letter))!;
    }

    [Fact]
    public async Task NewAccount_StartsAtLevelOneWithNothingUnlocked()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "xp.zero@gmail.com", Roles.Student);

        var report = await ReportAsync(student.Id);

        Assert.Equal(0, report.Xp);
        Assert.Equal(1, report.Level.Number);
        Assert.Equal(0, report.CurrentStreak);
        Assert.Equal(0, report.UnlockedCount);
    }

    [Fact]
    public async Task Answering_GivesXpOnceAndRetryingDoesNotFarm()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "xp.questoes@gmail.com", Roles.Student);
        var (first, right, wrong) = await ObjectiveAsync(0);
        var (second, secondRight, _) = await ObjectiveAsync(1);

        Assert.Equal(XpRules.FirstAnswer, (await AnswerAsync(student.Id, first.Id, wrong)).XpGained);
        Assert.Equal(XpRules.FirstCorrect, (await AnswerAsync(student.Id, first.Id, right)).XpGained);
        Assert.Equal(0, (await AnswerAsync(student.Id, first.Id, right)).XpGained);
        Assert.Equal(XpRules.FirstAnswer + XpRules.FirstCorrect, (await AnswerAsync(student.Id, second.Id, secondRight)).XpGained);

        var report = await ReportAsync(student.Id);
        Assert.Equal(2 * (XpRules.FirstAnswer + XpRules.FirstCorrect), report.Xp);
        Assert.Equal(1, report.CurrentStreak);
        Assert.True(report.StudiedToday);
        Assert.Contains(report.Achievements, a => a.Key == "primeira-resposta" && a.Unlocked);
        Assert.Contains(report.Achievements, a => a.Key == "dez-acertos" && !a.Unlocked);
    }

    [Fact]
    public async Task DuelWin_CountsOnlyWithAnOpponent()
    {
        var winner = await TestAccounts.CreateConfirmedAsync(factory, "xp.duelo1@gmail.com", Roles.Student);
        var loser = await TestAccounts.CreateConfirmedAsync(factory, "xp.duelo2@gmail.com", Roles.Student);
        using var scope = factory.Services.CreateScope();
        var duels = scope.ServiceProvider.GetRequiredService<DuelService>();
        var code = (await duels.CreateAsync(winner.Id)).Code!;
        var duel = (await duels.FindAsync(code))!;
        var right = duel.Questions.ToDictionary(q => q.QuestionId, q => q.Question!.Options.First(o => o.IsCorrect).Letter);

        await duels.JoinAsync(code, winner.Id);
        await duels.SubmitAsync(code, winner.Id, right);
        var alone = await ReportAsync(winner.Id);

        await duels.JoinAsync(code, loser.Id);
        await duels.SubmitAsync(code, loser.Id, new Dictionary<int, char>());
        var won = await ReportAsync(winner.Id);

        Assert.Contains(alone.Achievements, a => a.Key == "duelista" && a.Unlocked);
        Assert.Contains(alone.Achievements, a => a.Key == "campeao" && !a.Unlocked);
        Assert.Contains(won.Achievements, a => a.Key == "campeao" && a.Unlocked);
        Assert.Equal(alone.Xp + XpRules.DuelWin, won.Xp);
        Assert.Contains(won.Breakdown, line => line.Label == "Duelos vencidos" && line.Count == 1);
    }

    [Fact]
    public async Task Pages_ShowTheLevel()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "xp.paginas@gmail.com", Roles.Student);
        var (question, right, _) = await ObjectiveAsync(2);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestAccounts.LoginAsync(client, student.Email!);

        var answered = await TestAccounts.SubmitFormAsync(client, $"/questoes/{question.Id}", "answer", new() { ["Letter"] = right.ToString() });
        var answeredHtml = WebUtility.HtmlDecode(await answered.Content.ReadAsStringAsync());
        var home = WebUtility.HtmlDecode(await client.GetStringAsync("/"));
        var performance = WebUtility.HtmlDecode(await client.GetStringAsync("/meu-desempenho"));

        Assert.Contains($"+{XpRules.FirstAnswer + XpRules.FirstCorrect} XP", answeredHtml);
        Assert.Contains("Seu nível", home);
        Assert.Contains("Nível 1 · Calouro", home);
        Assert.Contains("1 dia de sequência", home);
        Assert.Contains("Primeira resposta", performance);
        Assert.Contains("Conquistada", performance);
        Assert.Contains("De onde vem seu XP", performance);
    }
}
