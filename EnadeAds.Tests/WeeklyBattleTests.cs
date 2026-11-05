using System.Net;
using EnadeAds.Accounts;
using EnadeAds.Battle;
using EnadeAds.Data;
using EnadeAds.Questions;
using EnadeAds.Study;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

public class WeeklyBattleWeekTests
{
    [Theory]
    [InlineData("2026-11-02", "2026-11-02")]
    [InlineData("2026-11-05", "2026-11-02")]
    [InlineData("2026-11-08", "2026-11-02")]
    [InlineData("2026-11-09", "2026-11-09")]
    public void Week_GoesFromMondayToSunday(string day, string monday)
    {
        Assert.Equal(DateOnly.Parse(monday), WeeklyBattleService.MondayOf(DateOnly.Parse(day)));
    }
}

[Collection(AppCollection.Name)]
public class WeeklyBattleTests(EnadeAdsFactory factory)
{
    private async Task<BattleBoard> BoardAsync()
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<WeeklyBattleService>().GetAsync();
    }

    private async Task<Question> QuestionOfAsync(int trackId)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Questions.IgnoreQueryFilters().AsNoTracking()
            .Include(q => q.Options)
            .Where(q => q.Subject!.TrackId == trackId && q.Kind == QuestionKind.Objective && q.ArchivedAt == null)
            .OrderBy(q => q.Id).FirstAsync();
    }

    private static (int Points, int People) Of(BattleBoard board, int trackId) =>
        board.Scores.Single(s => s.Track.Id == trackId) is var s ? (s.Points, s.People) : default;

    [Fact]
    public async Task EachHitCountsOncePerPersonForTheTrackOfTheQuestion()
    {
        var student = await TestAccounts.CreateConfirmedAsync(factory, "batalha.pontos@gmail.com", Roles.Student);
        var ads = await QuestionOfAsync(Tracks.EnadeAds);
        var redes = await QuestionOfAsync(Tracks.EnadeRedes);
        var before = await BoardAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var bank = scope.ServiceProvider.GetRequiredService<QuestionBank>();
            var right = (Question q) => q.Options.First(o => o.IsCorrect).Letter;
            var wrong = (Question q) => q.Options.First(o => !o.IsCorrect).Letter;
            await bank.AnswerAsync(student.Id, ads.Id, right(ads));
            // Acertar de novo a mesma questão não soma outra vez.
            await bank.AnswerAsync(student.Id, ads.Id, right(ads));
            await bank.AnswerAsync(student.Id, redes.Id, right(redes));
            await bank.AnswerAsync(student.Id, (await QuestionOfAsync(Tracks.Enem)).Id, wrong(await QuestionOfAsync(Tracks.Enem)));
        }

        var after = await BoardAsync();

        Assert.Equal(Of(before, Tracks.EnadeAds).Points + 1, Of(after, Tracks.EnadeAds).Points);
        Assert.Equal(Of(before, Tracks.EnadeRedes).Points + 1, Of(after, Tracks.EnadeRedes).Points);
        // Errar não pontua.
        Assert.Equal(Of(before, Tracks.Enem).Points, Of(after, Tracks.Enem).Points);
        Assert.Equal(Tracks.Seed.Count, after.Scores.Count);
        Assert.True(after.Scores.Zip(after.Scores.Skip(1)).All(pair => pair.First.Points >= pair.Second.Points));
    }

    [Fact]
    public async Task Home_ShowsTheBoardWithTheVisitorsTrack()
    {
        var html = WebUtility.HtmlDecode(await factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }).GetStringAsync("/"));

        Assert.Contains("Batalha da semana", html);
        Assert.Contains("ENADE ADS · sua prova", html);
        Assert.Contains("soma 1 ponto para a sua prova", html);
    }
}
