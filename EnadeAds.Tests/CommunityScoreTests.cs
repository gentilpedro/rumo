using System.Net;
using EnadeAds.Accounts;
using EnadeAds.Data;
using EnadeAds.Questions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

public class CommunityScoreRuleTests
{
    [Theory]
    [InlineData(2, 0, false, false)]
    [InlineData(3, 1, true, true)]
    [InlineData(5, 2, true, false)]
    [InlineData(10, 9, true, false)]
    public void ShowsOnlyFromThreePeopleAndFlagsTrickyQuestions(int answered, int correct, bool visible, bool tricky)
    {
        var score = new CommunityScore(answered, correct);

        Assert.Equal(visible, score.IsVisible);
        Assert.Equal(tricky, score.IsTricky);
    }
}

[Collection(AppCollection.Name)]
public class CommunityScoreTests(EnadeAdsFactory factory)
{
    /// <summary>Uma questão que ninguém respondeu ainda, para a contagem não depender da ordem dos testes.</summary>
    private async Task<Question> UntouchedQuestionAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Questions.AsNoTracking()
            .Include(q => q.Options)
            .Where(q => q.Kind == QuestionKind.Objective && q.ArchivedAt == null && !db.QuestionAttempts.Any(a => a.QuestionId == q.Id))
            .OrderByDescending(q => q.Id).FirstAsync();
    }

    [Fact]
    public async Task QuestionPage_ShowsHowTheClassDidAfterAnswering()
    {
        var question = await UntouchedQuestionAsync();
        var right = question.Options.First(o => o.IsCorrect).Letter;
        var wrong = question.Options.First(o => !o.IsCorrect).Letter;
        var students = new List<ApplicationUser>();
        for (var i = 0; i < 4; i++)
        {
            students.Add(await TestAccounts.CreateConfirmedAsync(factory, $"turma.{i}@gmail.com", Roles.Student));
        }

        using (var scope = factory.Services.CreateScope())
        {
            var bank = scope.ServiceProvider.GetRequiredService<QuestionBank>();
            await bank.AnswerAsync(students[0].Id, question.Id, wrong);
            // Refazer até acertar não muda a conta: vale a primeira resposta.
            await bank.AnswerAsync(students[0].Id, question.Id, right);
            await bank.AnswerAsync(students[1].Id, question.Id, wrong);

            // Com duas pessoas, o número ainda não aparece.
            Assert.False((await bank.CommunityAsync(question.Id)).IsVisible);
            await bank.AnswerAsync(students[2].Id, question.Id, wrong);
        }

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestAccounts.LoginAsync(client, students[3].Email!);
        var response = await TestAccounts.SubmitFormAsync(client, $"/questoes/{question.Id}", "answer", new() { ["Letter"] = right.ToString() });
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        // 1 acerto de primeira em 4 pessoas: 25%, abaixo de 40%.
        Assert.Contains("25%", html);
        Assert.Contains("acertaram de primeira · 4 pessoas responderam", html);
        Assert.Contains("Pegadinha", html);
    }
}
