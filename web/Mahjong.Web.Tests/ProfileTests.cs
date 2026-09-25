using System.Net;
using System.Net.Http.Json;
using Mahjong.Web.Client.Api;

namespace Mahjong.Web.Tests;

public sealed class ProfileTests : IAsyncLifetime
{
    private readonly MahjongAppFactory app = new();

    public Task InitializeAsync() => app.AddUserAsync("alice", "Alice");

    public Task DisposeAsync()
    {
        app.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task TheNewGameSettingsSwitchIsSavedAndLeavesTheRestAlone()
    {
        var client = app.ClientFor("alice");
        var before = await client.GetFromJsonAsync<PlayerProfile>("api/me");

        var response = await client.PutAsJsonAsync("api/me", new UpdateProfileRequest(null, null, null, SkipSettingsOnNewGame: true));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var after = await client.GetFromJsonAsync<PlayerProfile>("api/me");
        Assert.False(before!.SkipSettingsOnNewGame);
        Assert.Equal(before with { SkipSettingsOnNewGame = true }, after);
    }
}
