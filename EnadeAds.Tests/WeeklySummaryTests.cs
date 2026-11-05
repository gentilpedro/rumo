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
public class WeeklySummaryTests(EnadeAdsFactory factory)
{
    [Fact]
    public async Task Summary_CountsEachQuestionOnceByItsFirstAnswerOfTheWeek()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "semana.resumo@gmail.com", Roles.Student);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var questions = await db.Questions.Include(q => q.Options)
                .Where(q => q.Kind == QuestionKind.Objective && q.ArchivedAt == null).OrderBy(q => q.Id).Take(3).ToListAsync();
            var bank = scope.ServiceProvider.GetRequiredService<QuestionBank>();
            char Right(Question q) => q.Options.First(o => o.IsCorrect).Letter;
            char Wrong(Question q) => q.Options.First(o => !o.IsCorrect).Letter;

            await bank.AnswerAsync(student.Id, questions[0].Id, Right(questions[0]));
            await bank.AnswerAsync(student.Id, questions[1].Id, Right(questions[1]));
            await bank.AnswerAsync(student.Id, questions[2].Id, Wrong(questions[2]));
            // Errou e depois acertou: na semana vale a primeira resposta.
            await bank.AnswerAsync(student.Id, questions[2].Id, Right(questions[2]));
        }

        using var read = factory.Services.CreateScope();
        var (week, last) = await read.ServiceProvider.GetRequiredService<WeeklySummaryService>().GetAsync(student.Id);

        Assert.Equal((3, 2, 67, 1), (week.Answered, week.Correct, week.Percent, week.DaysStudied));
        Assert.NotNull(week.BestSubject);
        Assert.Equal(DayOfWeek.Monday, week.Monday.DayOfWeek);
        Assert.Equal((0, 0), (last.Answered, last.DaysStudied));

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestAccounts.LoginAsync(client, student.Email!);
        var html = WebUtility.HtmlDecode(await client.GetStringAsync("/meu-desempenho"));

        Assert.Contains("Sua semana", html);
        Assert.Contains("+3 que na semana passada", html);
        Assert.Contains("https://wa.me/?text=", html);
        Assert.Contains(Uri.EscapeDataString("Minha semana no Rumo: 3 questões, 67% de acerto"), html);
    }
}
