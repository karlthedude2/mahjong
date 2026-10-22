namespace Mahjong.Web.Tests;

/// <summary>Outfit, the settings screen's font, is served by the site itself: no request goes to Google Fonts.</summary>
public sealed class FontTests : IDisposable
{
    private readonly MahjongAppFactory app = new();

    public void Dispose() => app.Dispose();

    [Fact]
    public async Task PagesLoadNoFontsFromGoogle()
    {
        string html = await app.CreateClient().GetStringAsync("/");

        Assert.DoesNotContain("fonts.googleapis.com", html);
        Assert.DoesNotContain("fonts.gstatic.com", html);
    }

    [Fact]
    public void TheSiteServesOutfitWithItsLicense()
    {
        string wwwroot = Wwwroot();
        string css = File.ReadAllText(Path.Combine(wwwroot, "app.css"));

        Assert.Contains("font-family: Outfit", css);
        foreach (string file in new[] { "outfit-latin.woff2", "outfit-latin-ext.woff2", "OFL.txt" })
        {
            Assert.True(File.Exists(Path.Combine(wwwroot, "fonts", "outfit", file)), file);
        }
    }

    private static string Wwwroot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            string root = Path.Combine(dir.FullName, "Mahjong.Web", "wwwroot");
            if (File.Exists(Path.Combine(root, "app.css")))
            {
                return root;
            }
        }

        throw new DirectoryNotFoundException("Couldn't find the site's wwwroot.");
    }
}
