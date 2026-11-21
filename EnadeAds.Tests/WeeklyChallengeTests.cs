using System.Net;
using EnadeAds.Battle;
using EnadeAds.Challenges;
using EnadeAds.Data;
using EnadeAds.Questions;
using EnadeAds.Study;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

[Collection(AppCollection.Name)]
public class WeeklyChallengeTests(EnadeAdsFactory factory)
{
    private async Task<int> PublishAsync()
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<WeeklyChallengePublisher>().PublishAsync();
    }

    private async Task<List<Challenge>> AutomaticAsync()
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Challenges.IgnoreQueryFilters().AsNoTracking()
            .Include(c => c.Questions).ThenInclude(q => q.Question!).ThenInclude(q => q.Subject)
            .Where(c => c.AuthorId == null && c.Title.StartsWith(WeeklyChallengePublisher.TitlePrefix))
            .ToListAsync();
    }

    [Fact]
    public async Task Publish_GivesEveryTrackOneChallengeForTheWeekAndNeverTwo()
    {
        await PublishAsync();
        var again = await PublishAsync();
        var challenges = await AutomaticAsync();
        using var scope = factory.Services.CreateScope();
        var monday = WeeklyBattleService.MondayOf(scope.ServiceProvider.GetRequiredService<LocalCalendar>().Today());

        // Chamar de novo na mesma semana não cria nada.
        Assert.Equal(0, again);
        // Uma por prova: as seis trilhas têm banco para isso.
        Assert.Equal(Tracks.Seed.Select(t => t.Id).Order(), challenges.Select(c => c.TrackId).Order());
        Assert.All(challenges, challenge =>
        {
            Assert.Equal((monday, monday.AddDays(6)), (challenge.StartsOn, challenge.EndsOn));
            Assert.Equal($"Desafio da semana · {monday:dd/MM}", challenge.Title);
            Assert.Equal(WeeklyChallengePublisher.QuestionCount, challenge.Questions.Count);
            Assert.Equal(challenge.Questions.Count, challenge.Questions.Select(q => q.QuestionId).Distinct().Count());
            // Só questões objetivas da própria prova.
            Assert.All(challenge.Questions, q =>
            {
                Assert.Equal(QuestionKind.Objective, q.Question!.Kind);
                Assert.Equal(challenge.TrackId, q.Question.Subject!.TrackId);
            });
        });
    }

    [Fact]
    public async Task Publish_SpreadsTheQuestionsAcrossTheAreasOfTheExam()
    {
        await PublishAsync();
        var pave = (await AutomaticAsync()).Single(c => c.TrackId == Tracks.Pave1);

        // O PAVE tem quatro áreas com questões: cinco questões revezando passam por todas.
        Assert.Equal(4, pave.Questions.Select(q => q.Question!.Subject!.Block).Distinct().Count());
    }

    [Fact]
    public async Task Challenge_ShowsUpForStudentsLikeAnyOther()
    {
        await PublishAsync();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var list = WebUtility.HtmlDecode(await client.GetStringAsync("/desafios"));
        var mine = (await AutomaticAsync()).Single(c => c.TrackId == Tracks.Default);
        var detail = await client.GetAsync($"/desafios/{mine.Id}");

        Assert.Contains("Desafio da semana", list);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
    }
}
