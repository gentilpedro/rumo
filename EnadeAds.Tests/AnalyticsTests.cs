using System.Net;
using EnadeAds.Security;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EnadeAds.Tests;

public class AnalyticsOptionsTests
{
    [Theory]
    [InlineData("https://stats.aamora.com.br", "https://stats.aamora.com.br")]
    [InlineData(" https://stats.aamora.com.br/ ", "https://stats.aamora.com.br")]
    [InlineData("https://stats.exemplo.org:8443", "https://stats.exemplo.org:8443")]
    // O valor entra na CSP: o que não for uma origem HTTPS exata é ignorado, para não abrir a política.
    [InlineData("", null)]
    [InlineData("http://stats.aamora.com.br", null)]
    [InlineData("https://stats.aamora.com.br/umami", null)]
    [InlineData("https:", null)]
    [InlineData("*", null)]
    [InlineData("https://stats.aamora.com.br; script-src *", null)]
    public void OnlyAnExactHttpsOriginIsAccepted(string configured, string? expected)
    {
        Assert.Equal(expected, new AnalyticsOptions { Origin = configured }.ValidOrigin);
    }

    [Fact]
    public void PolicyAllowsTheOriginOnlyForScriptsAndConnections()
    {
        var policy = SecurityHeaders.ContentSecurityPolicyFor("https://stats.aamora.com.br");
        var directives = policy.Split("; ").ToDictionary(d => d.Split(' ')[0]);

        Assert.EndsWith(" https://stats.aamora.com.br", directives["script-src"]);
        Assert.Equal("connect-src 'self' https://stats.aamora.com.br", directives["connect-src"]);
        Assert.Equal("default-src 'self'", directives["default-src"]);
        Assert.Equal("connect-src 'self'", SecurityHeaders.ContentSecurityPolicyFor(null).Split("; ").Single(d => d.StartsWith("connect-src")));
    }
}

[Collection(AppCollection.Name)]
public class AnalyticsTests(EnadeAdsFactory factory)
{
    [Fact]
    public async Task WithoutTheSetting_NothingChanges()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/privacidade");
        var page = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Contains("connect-src 'self';", response.Headers.GetValues("Content-Security-Policy").Single());
        Assert.DoesNotContain("Contagem de visitas", page);
    }

    [Fact]
    public async Task WithTheSetting_PolicyAllowsItAndPrivacyPageExplainsIt()
    {
        using var counted = factory.WithWebHostBuilder(builder => builder.UseSetting("Analytics:Origin", "https://stats.aamora.com.br"));
        var client = counted.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/privacidade");
        var page = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        var csp = response.Headers.GetValues("Content-Security-Policy").Single();

        Assert.Contains("connect-src 'self' https://stats.aamora.com.br;", csp);
        Assert.Contains("Contagem de visitas", page);
        Assert.Contains("não usa cookies, não guarda o endereço IP", page);
    }
}
