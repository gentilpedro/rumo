using System.Net;
using EnadeAds.Accounts;
using EnadeAds.Achievements;
using EnadeAds.Daily;
using EnadeAds.Data;
using EnadeAds.Questions;
using EnadeAds.Study;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

[Collection(AppCollection.Name)]
public class DailyQuestionTests(EnadeAdsFactory factory)
{
    private HttpClient NewClient() => factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private async Task<Question> TodayAsync(int trackId)
    {
        using var scope = factory.Services.CreateScope();
        return (await scope.ServiceProvider.GetRequiredService<DailyQuestionService>().TodayAsync(trackId))!;
    }

    [Fact]
    public void DayRange_FollowsThePlatformTimeZone()
    {
        using var scope = factory.Services.CreateScope();
        var (start, end) = scope.ServiceProvider.GetRequiredService<LocalCalendar>().DayRange(new DateOnly(2026, 11, 5));

        // Brasília é UTC-3: o dia 5 começa às 3h UTC e termina às 3h UTC do dia 6.
        Assert.Equal(new DateTimeOffset(2026, 11, 5, 3, 0, 0, TimeSpan.Zero), start);
        Assert.Equal(new DateTimeOffset(2026, 11, 6, 3, 0, 0, TimeSpan.Zero), end);
    }

    [Fact]
    public async Task Today_IsTheSameQuestionAllDayAndBelongsToTheTrack()
    {
        var first = await TodayAsync(Tracks.EnadeRedes);
        var again = await TodayAsync(Tracks.EnadeRedes);

        Assert.Equal(first.Id, again.Id);
        Assert.Equal(QuestionKind.Objective, first.Kind);
        Assert.Equal(Tracks.EnadeRedes, first.Subject!.TrackId);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(1, await db.DailyQuestions.CountAsync(d => d.TrackId == Tracks.EnadeRedes));
    }

    [Fact]
    public async Task Visitor_SeesTheQuestionOfTheLinkedTrack()
    {
        var question = await TodayAsync(Tracks.EnadeRedes);

        // O cliente dos testes navega em ADS; o link leva para a questão de Redes.
        var html = WebUtility.HtmlDecode(await NewClient().GetStringAsync("/questao-do-dia/enade-redes"));

        Assert.Contains("ENADE Redes", html);
        Assert.Contains(question.Options[0].Text.Split('\n')[0].Trim(), html);
        Assert.Contains("ReturnUrl=%2Fquestao-do-dia%2Fenade-redes", html);
        Assert.Contains("og:description", html);
        Assert.DoesNotContain(question.Options[0].Explanation, html);
        Assert.Equal(HttpStatusCode.NotFound, (await NewClient().GetAsync("/questao-do-dia/nao-existe")).StatusCode);
    }

    [Fact]
    public async Task Student_AnswersOnceAndEarnsTheDailyBonus()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "dia.responde@gmail.com", Roles.Student);
        var question = await TodayAsync(Tracks.EnadeAds);
        var right = question.Options.First(o => o.IsCorrect).Letter;
        var client = NewClient();
        await TestAccounts.LoginAsync(client, student.Email!);

        var answered = await TestAccounts.SubmitFormAsync(client, "/questao-do-dia/enade-ads", "daily-answer", new() { ["Letter"] = right.ToString() });
        var html = WebUtility.HtmlDecode(await answered.Content.ReadAsStringAsync());

        Assert.Contains("Você acertou.", html);
        Assert.Contains($"+{XpRules.FirstAnswer + XpRules.FirstCorrect + XpRules.DailyQuestion} XP", html);
        Assert.Contains("https://wa.me/?text=", html);

        // Voltar à página mostra a resposta do dia, sem formulário de novo.
        var again = WebUtility.HtmlDecode(await client.GetStringAsync("/questao-do-dia"));
        Assert.Contains("Você acertou.", again);
        Assert.DoesNotContain("name=\"Letter\"", again);

        var home = WebUtility.HtmlDecode(await client.GetStringAsync("/"));
        Assert.Contains("Questão do dia", home);
        Assert.Contains("Você já respondeu", home);

        using var scope = factory.Services.CreateScope();
        var report = await scope.ServiceProvider.GetRequiredService<AchievementService>().GetAsync(student.Id);
        Assert.Contains(report.Breakdown, line => line.Label == "Questões do dia" && line.Count == 1 && line.Xp == XpRules.DailyQuestion);
        Assert.Equal(XpRules.FirstAnswer + XpRules.FirstCorrect + XpRules.DailyQuestion, report.Xp);
    }

    [Fact]
    public async Task Stats_CountTheFirstAnswerOfEachPersonToday()
    {
        var question = await TodayAsync(Tracks.Enem);
        var right = question.Options.First(o => o.IsCorrect).Letter;
        var wrong = question.Options.First(o => !o.IsCorrect).Letter;
        var hit = await TestAccounts.CreateConfirmedAsync(factory, "dia.stats1@gmail.com", Roles.Student);
        var miss = await TestAccounts.CreateConfirmedAsync(factory, "dia.stats2@gmail.com", Roles.Student);

        using var scope = factory.Services.CreateScope();
        var bank = scope.ServiceProvider.GetRequiredService<QuestionBank>();
        var daily = scope.ServiceProvider.GetRequiredService<DailyQuestionService>();
        var before = await daily.StatsAsync(question.Id);
        await bank.AnswerAsync(hit.Id, question.Id, right);
        await bank.AnswerAsync(miss.Id, question.Id, wrong);
        // Errou e depois acertou: vale a primeira resposta do dia.
        await bank.AnswerAsync(miss.Id, question.Id, right);
        var after = await daily.StatsAsync(question.Id);

        Assert.Equal(before.Answered + 2, after.Answered);
        Assert.Equal(before.Correct + 1, after.Correct);
    }
}
