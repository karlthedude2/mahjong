using System.Net.Http.Json;

namespace Mahjong.Web.Client.Api;

/// <summary>
/// Calls to the server API. Game calls are only for signed-in players (guests play offline); the
/// client config and replay deals are public.
/// </summary>
public sealed class GameApi(HttpClient http)
{
    private ClientConfig? config;

    public async Task<ClientConfig> GetConfigAsync()
    {
        return config ??= await http.GetFromJsonAsync<ClientConfig>("api/client-config")
            ?? new ClientConfig(null, null, null, null, false);
    }

    /// <summary>Starts a ranked game; with replayOf, a replay of that leaderboard game's deal.</summary>
    public async Task<StartGameResponse> StartGameAsync(string layout, Guid? replayOf = null)
    {
        var response = await http.PostAsJsonAsync("api/games", new StartGameRequest(layout, replayOf));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<StartGameResponse>())!;
    }

    public async Task<FinishGameResponse?> FinishGameAsync(Guid gameId, FinishGameRequest request)
    {
        var response = await http.PostAsJsonAsync($"api/games/{gameId}/finish", request);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<FinishGameResponse>() : null;
    }

    /// <summary>Tells the server a ranked game is paused (or resumed). Returns false if the call failed.</summary>
    public async Task<bool> SetPausedAsync(Guid gameId, bool paused)
    {
        try
        {
            var response = await http.PostAsync($"api/games/{gameId}/{(paused ? "pause" : "resume")}", null);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    /// <summary>A leaderboard game's deal and replay list, or null if it can't be found.</summary>
    public async Task<ReplayInfo?> GetReplayAsync(Guid originalGameId)
    {
        try
        {
            var response = await http.GetAsync($"api/replays/{originalGameId}");
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ReplayInfo>() : null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public Task<PlayerProfile?> GetProfileAsync() => http.GetFromJsonAsync<PlayerProfile>("api/me");

    public async Task UpdateProfileAsync(UpdateProfileRequest request)
    {
        var response = await http.PutAsJsonAsync("api/me", request);
        response.EnsureSuccessStatusCode();
    }
}
