using System.Net;
using System.Text.RegularExpressions;

namespace Mahjong.Web.Tests;

/// <summary>
/// The how-to-play pages: five per language, English at /how-to-play and Spanish under /es/. The
/// address decides the language, and each page tells search engines about its twin.
/// </summary>
public sealed partial class HowToPlayTests : IDisposable
{
    private readonly MahjongAppFactory app = new();

    public void Dispose() => app.Dispose();

    public static TheoryData<string> Pages() =>
    [
        "how-to-play", "how-to-play/mahjong-solitaire", "how-to-play/mahjong-3d", "how-to-play/mahjong-cube", "how-to-play/connect",
    ];

    [Theory]
    [MemberData(nameof(Pages))]
    public async Task EnglishPagesAreEnglishWithLinksToTheirTwin(string page)
    {
        string html = await app.CreateClient().GetStringAsync("/" + page);

        Assert.Contains("<html lang=\"en\"", html);
        Assert.Single(H1().Matches(html));
        Assert.Contains($"<link rel=\"canonical\" href=\"http://localhost/{page}\"", html);
        Assert.Contains($"<link rel=\"alternate\" hreflang=\"en\" href=\"http://localhost/{page}\"", html);
        Assert.Contains($"<link rel=\"alternate\" hreflang=\"es\" href=\"http://localhost/es/{page}\"", html);
        Assert.Contains($"<link rel=\"alternate\" hreflang=\"x-default\" href=\"http://localhost/{page}\"", html);
    }

    [Theory]
    [MemberData(nameof(Pages))]
    public async Task SpanishPagesAreSpanishWithoutACookie(string page)
    {
        string html = await app.CreateClient().GetStringAsync("/es/" + page);

        Assert.Contains("<html lang=\"es\"", html);
        Assert.Single(H1().Matches(html));
        Assert.Contains($"<link rel=\"canonical\" href=\"http://localhost/es/{page}\"", html);
    }

    [Fact]
    public async Task AnEnglishPageStaysEnglishWithASpanishCookie()
    {
        var page = new HttpRequestMessage(HttpMethod.Get, "/how-to-play/connect");
        page.Headers.Add("Cookie", ".AspNetCore.Culture=c%3Des%7Cuic%3Des");

        string html = await (await app.CreateClient().SendAsync(page)).Content.ReadAsStringAsync();

        Assert.Contains("<html lang=\"en\"", html);
    }

    [Fact]
    public async Task VisitingAHelpPageSetsTheSitesLanguage()
    {
        var response = await app.CreateClient().GetAsync("/es/how-to-play");

        string cookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith(".AspNetCore.Culture="));
        Assert.Contains("c%3Des%7Cuic%3Des", cookie);
    }

    [Theory]
    [InlineData("/how-to-play/chess")]
    [InlineData("/es/how-to-play/chess")]
    public async Task AnUnknownGameIsNotFound(string path)
    {
        var response = await app.CreateClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TheLanguageSwitchGoesToTheTwinPage()
    {
        string english = await app.CreateClient().GetStringAsync("/how-to-play/mahjong-cube");
        string spanish = await app.CreateClient().GetStringAsync("/es/how-to-play/mahjong-cube");

        Assert.Contains("language/es?returnUrl=%2Fes%2Fhow-to-play%2Fmahjong-cube", english);
        Assert.Contains("language/en?returnUrl=%2Fhow-to-play%2Fmahjong-cube", spanish);
    }

    [Fact]
    public async Task TheSitemapListsEveryHelpPage()
    {
        string sitemap = await app.CreateClient().GetStringAsync("/sitemap.xml");

        foreach (string page in new[] { "how-to-play", "how-to-play/mahjong-solitaire", "how-to-play/mahjong-3d", "how-to-play/mahjong-cube", "how-to-play/connect" })
        {
            Assert.Contains($"<loc>http://localhost/{page}</loc>", sitemap);
            Assert.Contains($"<loc>http://localhost/es/{page}</loc>", sitemap);
        }
    }

    [Fact]
    public async Task TheHeaderLinksToHelpInANewTabFromTheGame()
    {
        string game = await app.CreateClient().GetStringAsync("/");
        string privacy = await app.CreateClient().GetStringAsync("/privacy");

        Assert.Matches("<a class=\"icon-link\" href=\"how-to-play\" target=\"_blank\" rel=\"noopener\"", game);
        Assert.Contains("<a class=\"icon-link\" href=\"how-to-play\" aria-label", privacy);
    }

    [GeneratedRegex("<h1[ >]")]
    private static partial Regex H1();
}
