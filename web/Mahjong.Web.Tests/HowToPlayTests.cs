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

    [Theory]
    [InlineData("/How-To-Play/Connect", "How to play Connect", "en")]
    [InlineData("/ES/how-to-play/connect", "Cómo jugar Conectar", "es")]
    public async Task AddressesInAnyCaseShowTheRightPage(string path, string heading, string language)
    {
        string html = await app.CreateClient().GetStringAsync(path);

        Assert.Contains(heading, html);
        Assert.Contains($"<html lang=\"{language}\"", html);
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

    // The Connect diagram teaches the rule, so each example must be what the game itself decides:
    // the allowed ones join with at most two turns, the refused one can't be joined at all.
    [Fact]
    public void TheConnectDiagramMatchesTheRule()
    {
        var layout = (Mahjong.Core.LayoutDefinition)Activator.CreateInstance(typeof(Mahjong.Core.LayoutDefinition),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null,
            ["diagram", Array.Empty<Mahjong.Core.Position>(), Mahjong.Core.GameKind.Connect,
             Mahjong.Web.Components.HowToPlay.ConnectExamples.Columns, Mahjong.Web.Components.HowToPlay.ConnectExamples.Rows,
             Mahjong.Core.Gravity.None, "diagram"], null)!;
        static Mahjong.Core.Position At((int Col, int Row) cell) => new Mahjong.Core.Cell(cell.Col, cell.Row).ToPosition();

        foreach (var example in Mahjong.Web.Components.HowToPlay.ConnectExamples.All)
        {
            var occupied = example.Blocks.Append(example.A).Append(example.B).Select(At).ToHashSet();
            var path = Mahjong.Core.ConnectRules.FindPath(layout, occupied, At(example.A), At(example.B));

            Assert.True(example.Allowed == (path != null), $"{example.English}: the rule says {(path == null ? "no" : "yes")}");
        }
    }

    [GeneratedRegex("<h1[ >]")]
    private static partial Regex H1();
}
