using System.Net;

namespace Mahjong.Web.Tests;

public sealed class HealthTests : IDisposable
{
    private readonly MahjongAppFactory app = new();

    public void Dispose() => app.Dispose();

    [Fact]
    public async Task TheHealthCheckNamesTheRunningVersion()
    {
        var response = await app.CreateClient().GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
        Assert.True(response.Headers.TryGetValues("X-App-Version", out var version));
        Assert.False(string.IsNullOrEmpty(version.Single()));
    }
}
