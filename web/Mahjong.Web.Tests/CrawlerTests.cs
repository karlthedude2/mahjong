using System.Net;

namespace Mahjong.Web.Tests;

/// <summary>
/// What a search or ads crawler finds: robots.txt and a sitemap to lead it to the pages, and on the
/// home page (a game built in the browser) a readable "About Mahjong Haus" from the server.
/// </summary>
public sealed class CrawlerTests : IDisposable
{
    private readonly MahjongAppFactory app = new();

    public void Dispose() => app.Dispose();

    [Fact]
    public async Task RobotsTxtAllowsThePagesAndNamesTheSitemap()
    {
        var response = await app.CreateClient().GetAsync("/robots.txt");
        string robots = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("User-agent: *", robots);
        Assert.Contains("Disallow: /Account/", robots);
        Assert.Contains("Disallow: /api/", robots);
        Assert.Contains("Sitemap: http://localhost/sitemap.xml", robots);
    }

    [Fact]
    public async Task TheSitemapListsThePublicPages()
    {
        var response = await app.CreateClient().GetAsync("/sitemap.xml");
        string sitemap = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/xml", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("<loc>http://localhost/</loc>", sitemap);
        Assert.Contains("<loc>http://localhost/leaderboards</loc>", sitemap);
        Assert.Contains("<loc>http://localhost/privacy</loc>", sitemap);
    }

    [Fact]
    public async Task TheHomePageHasReadableTextFromTheServer()
    {
        string html = await app.CreateClient().GetStringAsync("/");

        Assert.Contains("About Mahjong Haus", html);
        Assert.Contains("Mahjong Solitaire", html);
        Assert.Contains("Connect (Shisen-Sho)", html);
    }

    [Fact]
    public async Task TheHomePageTextIsInSpanishForSpanishVisitors()
    {
        var client = app.CreateClient(new() { AllowAutoRedirect = false });
        var response = await client.GetAsync("/language/es?returnUrl=%2F");
        string cookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith(".AspNetCore.Culture="));
        var page = new HttpRequestMessage(HttpMethod.Get, "/");
        page.Headers.Add("Cookie", cookie.Split(';')[0]);

        string html = await (await client.SendAsync(page)).Content.ReadAsStringAsync();

        Assert.Contains("Acerca de Mahjong Haus", html);
    }

    [Fact]
    public async Task OtherPagesDontRepeatIt()
    {
        string html = await app.CreateClient().GetStringAsync("/privacy");

        Assert.DoesNotContain("About Mahjong Haus", html);
    }
}
