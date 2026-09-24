using System.Net;
using Microsoft.AspNetCore.Hosting;

namespace Mahjong.Web.Tests;

public sealed class AdsTxtTests : IDisposable
{
    private readonly MahjongAppFactory app = new();

    public void Dispose() => app.Dispose();

    [Fact]
    public async Task AdsTxtListsTheAdSensePublisher()
    {
        using var withAds = app.WithWebHostBuilder(b => b.UseSetting("Ads:ClientId", "ca-pub-8841190346743079"));

        var response = await withAds.CreateClient().GetAsync("/ads.txt");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("google.com, pub-8841190346743079, DIRECT, f08c47fec0942fa0", (await response.Content.ReadAsStringAsync()).Trim());
    }

    [Fact]
    public async Task AdsTxtIsMissingUntilAdSenseIsConfigured()
    {
        var response = await app.CreateClient().GetAsync("/ads.txt");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
