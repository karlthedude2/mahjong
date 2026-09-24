using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Mahjong.Web.Tests;

public sealed class CanonicalHostTests : IDisposable
{
    private readonly MahjongAppFactory app = new() { CanonicalHost = "mahjong.haus" };

    public void Dispose() => app.Dispose();

    private HttpClient Client() => app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task OtherAddressesRedirectToTheMainDomainKeepingThePath()
    {
        var response = await Client().GetAsync("https://mahjong-karl.azurewebsites.net/leaderboards?layout=Temple");

        Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.Equal("https://mahjong.haus/leaderboards?layout=Temple", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task TheMainDomainIsServedNormally()
    {
        var response = await Client().GetAsync("https://mahjong.haus/leaderboards");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TheHealthCheckIsNotRedirected()
    {
        var response = await Client().GetAsync("https://mahjong-karl.azurewebsites.net/healthz");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
