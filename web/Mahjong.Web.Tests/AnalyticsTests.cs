using Microsoft.AspNetCore.Hosting;

namespace Mahjong.Web.Tests;

public sealed class AnalyticsTests : IDisposable
{
    private readonly MahjongAppFactory app = new();

    public void Dispose() => app.Dispose();

    [Fact]
    public async Task TheAnalyticsScriptIsAddedWhenATokenIsSet()
    {
        using var withAnalytics = app.WithWebHostBuilder(b => b.UseSetting("Analytics:CloudflareToken", "abc123"));

        string html = await withAnalytics.CreateClient().GetStringAsync("/leaderboards");

        Assert.Contains("https://static.cloudflareinsights.com/beacon.min.js", html);
        Assert.Contains("data-cf-beacon=\"{&quot;token&quot;: &quot;abc123&quot;}\"", html);
    }

    [Fact]
    public async Task NoAnalyticsScriptWithoutAToken()
    {
        string html = await app.CreateClient().GetStringAsync("/leaderboards");
        Assert.DoesNotContain("cloudflareinsights", html);
    }
}
