using System.Net;
using EnadeAds.Blog;
using EnadeAds.Daily;
using EnadeAds.Data;
using EnadeAds.Landing;
using EnadeAds.Questions;
using EnadeAds.Study;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnadeAds.Tests;

public class ExamCountdownTests
{
    private static readonly DateOnly Today = new(2026, 11, 16);

    [Fact]
    public void Upcoming_CountsTheDaysToTheNextExamDay()
    {
        var countdown = ExamCountdown.For([new DateOnly(2026, 11, 29)], Today);

        Assert.Equal(13, countdown.DaysLeft);
        Assert.False(countdown.IsOver);
    }

    [Fact]
    public void TwoDayExam_CountsToTheDayThatIsStillAhead()
    {
        var countdown = ExamCountdown.For([new DateOnly(2026, 11, 8), new DateOnly(2026, 11, 16)], Today);

        Assert.Equal(0, countdown.DaysLeft);
        Assert.True(countdown.IsToday);
    }

    [Fact]
    public void PastExam_IsOver()
    {
        var countdown = ExamCountdown.For([new DateOnly(2026, 11, 8), new DateOnly(2026, 11, 15)], Today);

        Assert.True(countdown.IsOver);
        Assert.Null(countdown.DaysLeft);
    }
}

[Collection(AppCollection.Name)]
public class LandingTests(EnadeAdsFactory factory)
{
    /// <summary>Host sem trilha padrão: quem chega pela primeira vez, sem ter escolhido a prova.</summary>
    private WebApplicationFactory<Program> Visitor() =>
        factory.WithWebHostBuilder(builder => builder.UseSetting("Tracks:Default", ""));

    private static HttpClient NewClient(WebApplicationFactory<Program> host) =>
        host.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private async Task<Question> DailyAsync(int trackId)
    {
        using var scope = factory.Services.CreateScope();
        return (await scope.ServiceProvider.GetRequiredService<DailyQuestionService>().TodayAsync(trackId))!;
    }

    [Fact]
    public async Task Visitor_SeesWhatRumoIsBeforeTheDemo()
    {
        using var host = Visitor();

        var html = WebUtility.HtmlDecode(await NewClient(host).GetStringAsync("/"));

        Assert.Contains("Prepare-se para o ENEM e o ENADE no formato da prova.", html);
        Assert.Contains("Para qual prova você vai estudar?", html);
        Assert.Contains("trilha/enade-ads?volta=%2F", html);
        var how = html.IndexOf("Como funciona: do roadmap ao dia da prova", StringComparison.Ordinal);
        var demo = html.IndexOf("Experimente agora, sem conta", StringComparison.Ordinal);
        Assert.True(how > 0 && demo > how, "A explicação vem antes da demo.");
    }

    [Fact]
    public async Task Sample_IsTheDailyQuestionOfTheChosenTrack()
    {
        using var host = Visitor();
        var daily = await DailyAsync(Tracks.EnadeRedes);

        var html = await NewClient(host).GetStringAsync("/?amostra=enade-redes");

        Assert.Contains($"name=\"Answer.QuestionId\" value=\"{daily.Id}\"", html);
        Assert.Contains("aria-current=\"page\" href=\"?amostra=enade-redes#experimente\"", html);
    }

    [Fact]
    public async Task Answer_ShowsTheCommentedKeyWithoutSavingAnything()
    {
        using var host = Visitor();
        var client = NewClient(host);
        var daily = await DailyAsync(Tracks.EnadeRedes);
        var wrong = daily.Options.First(o => !o.IsCorrect);
        var right = daily.Options.Single(o => o.IsCorrect);
        int attemptsBefore;
        using (var scope = factory.Services.CreateScope())
        {
            attemptsBefore = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().QuestionAttempts.CountAsync();
        }

        var url = "/?amostra=enade-redes";
        var response = await TestAccounts.PostFormAsync(client, url, await client.GetStringAsync(url), "landing-answer", new()
        {
            ["Answer.QuestionId"] = daily.Id.ToString(),
            ["Answer.Letter"] = wrong.Letter.ToString(),
        });
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"A resposta é a {right.Letter}", html);
        Assert.All(daily.Options, option => Assert.Contains(option.Explanation, html));
        using (var scope = factory.Services.CreateScope())
        {
            Assert.Equal(attemptsBefore, await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().QuestionAttempts.CountAsync());
        }
    }

    [Fact]
    public async Task Answer_WithoutAChoice_AsksForOne()
    {
        using var host = Visitor();
        var client = NewClient(host);
        var daily = await DailyAsync(Tracks.EnadeAds);

        var response = await TestAccounts.PostFormAsync(client, "/?amostra=enade-ads", await client.GetStringAsync("/?amostra=enade-ads"), "landing-answer", new()
        {
            ["Answer.QuestionId"] = daily.Id.ToString(),
        });
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("Escolha uma das alternativas para ver o gabarito.", html);
        Assert.DoesNotContain(daily.Options[0].Explanation, html);
    }

    [Fact]
    public async Task Notices_ComeFromEveryTrack()
    {
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // O banco é dividido com os outros testes: datas à frente garantem que estes são os mais recentes.
            var now = DateTimeOffset.UtcNow.AddYears(1);
            db.Posts.AddRange(
                OfficialPost("Aviso do ENEM para a landing", Tracks.Enem, now),
                OfficialPost("Aviso do ENADE ADS para a landing", Tracks.EnadeAds, now.AddMinutes(-1)));
            await db.SaveChangesAsync();
        }

        using var host = Visitor();
        var html = WebUtility.HtmlDecode(await NewClient(host).GetStringAsync("/"));

        Assert.Contains("Aviso do ENEM para a landing", html);
        Assert.Contains("Aviso do ENADE ADS para a landing", html);
    }

    [Fact]
    public async Task ChosenTrack_StillGoesToTheFeed()
    {
        var html = WebUtility.HtmlDecode(await factory.CreateClient().GetStringAsync("/"));

        Assert.Contains("Comunidade", html);
        Assert.DoesNotContain("Experimente agora, sem conta", html);
    }

    private static Post OfficialPost(string title, int trackId, DateTimeOffset publishedAt) => new()
    {
        Slug = Slugs.From(title) + "-" + Guid.NewGuid().ToString("N")[..6],
        Title = title,
        Summary = title,
        Body = title,
        Category = PostCategory.ClassAnnouncements,
        TrackId = trackId,
        IsOfficial = true,
        CreatedAt = publishedAt,
        UpdatedAt = publishedAt,
        PublishedAt = publishedAt,
    };
}
