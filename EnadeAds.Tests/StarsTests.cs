using System.Net;
using EnadeAds.Accounts;
using EnadeAds.Data;
using EnadeAds.Performance;
using EnadeAds.Questions;
using EnadeAds.Study;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

public class StarRuleTests
{
    private static SubjectScore Subject(int total, int answered, int correct) =>
        new(1, "materia", "Matéria", StudyBlock.Fundamentals, total, new Score(answered, correct));

    [Theory]
    [InlineData(20, 4, 4, null)]
    [InlineData(20, 5, 1, 0)]
    [InlineData(20, 5, 2, 1)]
    [InlineData(20, 5, 3, 2)]
    [InlineData(20, 5, 4, 3)]
    [InlineData(20, 10, 8, 3)]
    // Matéria com 3 questões no banco: basta responder as 3.
    [InlineData(3, 3, 3, 3)]
    [InlineData(0, 0, 0, null)]
    public void Stars_FollowTheHitRateAfterEnoughAnswers(int total, int answered, int correct, int? stars)
    {
        Assert.Equal(stars, Stars.Of(Subject(total, answered, correct)));
    }

    [Fact]
    public void NextStar_SaysTheTarget()
    {
        Assert.Equal(40, Stars.NextAt(0));
        Assert.Equal(80, Stars.NextAt(2));
        Assert.Null(Stars.NextAt(3));
    }
}

[Collection(AppCollection.Name)]
public class StarsTests(EnadeAdsFactory factory)
{
    [Fact]
    public async Task Roadmap_ShowsTheStarsOfASubject()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "estrelas.roadmap@gmail.com", Roles.Student);
        RoadmapNode subject;
        List<Question> questions;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // A matéria com mais objetivas, para ter as 5 respostas que liberam as estrelas.
            var subjectId = await db.Questions.Where(q => q.Kind == QuestionKind.Objective && q.ArchivedAt == null)
                .GroupBy(q => q.SubjectId).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstAsync();
            subject = await db.RoadmapNodes.SingleAsync(n => n.Id == subjectId);
            questions = await db.Questions.Include(q => q.Options)
                .Where(q => q.SubjectId == subjectId && q.Kind == QuestionKind.Objective && q.ArchivedAt == null)
                .OrderBy(q => q.Id).Take(5).ToListAsync();

            var bank = scope.ServiceProvider.GetRequiredService<QuestionBank>();
            // 4 de 5: 80%, três estrelas.
            foreach (var (question, index) in questions.Select((q, i) => (q, i)))
            {
                await bank.AnswerAsync(student.Id, question.Id, question.Options.First(o => o.IsCorrect == index < 4).Letter);
            }
        }

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestAccounts.LoginAsync(client, student.Email!);
        var roadmap = WebUtility.HtmlDecode(await client.GetStringAsync($"/roadmap?topico={subject.Slug}"));
        var performance = WebUtility.HtmlDecode(await client.GetStringAsync("/meu-desempenho"));

        Assert.Contains("3 de 3 estrelas", roadmap);
        Assert.Contains("Matéria completa", roadmap);
        Assert.Contains("3 de 3 estrelas", performance);

        // Visitante não vê estrelas.
        var visitor = WebUtility.HtmlDecode(await factory.CreateClient().GetStringAsync("/roadmap"));
        Assert.DoesNotContain("estrelas\"", visitor);
    }
}
