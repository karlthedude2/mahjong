using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Mahjong.Web.Services;

namespace Mahjong.Web.Tests;

public sealed class MaintenanceTests : IDisposable
{
    private readonly MahjongAppFactory app = new();
    private readonly string flag = Path.Combine(Path.GetTempPath(), $"mahjong-maintenance-{Guid.NewGuid():N}.on");

    public void Dispose()
    {
        app.Dispose();
        File.Delete(flag);
    }

    private HttpClient Client()
    {
        var factory = app.WithWebHostBuilder(b => b.UseSetting("Site:MaintenanceFlag", flag));
        return factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task TheSiteWorksNormallyWithoutTheFlag()
    {
        var response = await Client().GetAsync("/leaderboards");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PagesShowTheMaintenancePageWhileTheFlagExists()
    {
        File.WriteAllText(flag, "on");

        var response = await Client().GetAsync("/leaderboards");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("60", response.Headers.RetryAfter?.ToString());
        Assert.Contains("We'll be right back", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task TheApiAnswersServiceUnavailableDuringMaintenance()
    {
        File.WriteAllText(flag, "on");

        var response = await Client().GetAsync("/api/leaderboards/Temple");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Theory]
    [InlineData("/healthz")]
    [InlineData("/logo.png")]
    public async Task TheHealthCheckAndLogoStillWorkDuringMaintenance(string path)
    {
        File.WriteAllText(flag, "on");

        var response = await Client().GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AnOldFlagIsIgnoredSoAFailedDeployCantLeaveTheSiteDown()
    {
        File.WriteAllText(flag, "on");
        File.SetLastWriteTimeUtc(flag, app.Time.GetUtcNow().UtcDateTime - MaintenanceMode.MaxAge - TimeSpan.FromMinutes(1));

        var response = await Client().GetAsync("/leaderboards");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
