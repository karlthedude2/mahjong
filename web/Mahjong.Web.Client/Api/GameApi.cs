using System.Net.Http.Json;

namespace Mahjong.Web.Client.Api;

/// <summary>
/// Calls to the server API. Signed-in players' games use their sign-in; guests' games carry the
/// token the server handed out when the game started. The client config and replay deals are public.
/// </summary>
public sealed class GameApi(HttpClient http)
{
    private const string GuestTokenHeader = "X-Guest-Token";

    private ClientConfig? config;

    public async Task<ClientConfig> GetConfigAsync()
    {
        return config ??= await http.GetFromJsonAsync<ClientConfig>("api/client-config")
            ?? new ClientConfig(null, null, null, null, false);
    }

    /// <summary>
    /// Starts a server-verified game (a guest's if guest is true); with replayOf, a replay of that
    /// leaderboard game's deal.
    /// </summary>
    public async Task<StartGameResponse> StartGameAsync(string layout, Guid? replayOf = null, bool guest = false, bool randomDeal = false)
    {
        var response = await http.PostAsJsonAsync(guest ? "api/guest-games" : "api/games", new StartGameRequest(layout, replayOf, randomDeal));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<StartGameResponse>())!;
    }

    public async Task<FinishGameResponse?> FinishGameAsync(Guid gameId, FinishGameRequest request, string? guestToken = null)
    {
        var message = GameRequest($"{gameId}/finish", guestToken);
        message.Content = JsonContent.Create(request);
        var response = await http.SendAsync(message);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<FinishGameResponse>() : null;
    }

    /// <summary>Tells the server a game is paused (or resumed). Returns false if the call failed.</summary>
    public async Task<bool> SetPausedAsync(Guid gameId, bool paused, string? guestToken = null)
    {
        try
        {
            var response = await http.SendAsync(GameRequest($"{gameId}/{(paused ? "pause" : "resume")}", guestToken));
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    /// <summary>
    /// Saves a guest's verified win to the signed-in player. Returns null if the server turned it
    /// down; throws <see cref="HttpRequestException"/> if it couldn't be reached.
    /// </summary>
    public async Task<ClaimGameResponse?> ClaimGameAsync(Guid gameId, string guestToken)
    {
        var response = await http.PostAsJsonAsync($"api/games/{gameId}/claim", new ClaimGameRequest(guestToken));
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ClaimGameResponse>() : null;
    }

    /// <summary>A layout's leaderboard, or null if it couldn't be loaded.</summary>
    public async Task<IReadOnlyList<LeaderboardEntry>?> GetLeaderboardAsync(string layout)
    {
        try
        {
            return await http.GetFromJsonAsync<List<LeaderboardEntry>>($"api/leaderboards/{Uri.EscapeDataString(layout)}");
        }
        catch (Exception e) when (e is HttpRequestException or System.Text.Json.JsonException)
        {
            return null;
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

    // A POST to a game: the signed-in player's (api/games) or, with a token, a guest's (api/guest-games).
    private static HttpRequestMessage GameRequest(string path, string? guestToken)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, $"{(guestToken == null ? "api/games" : "api/guest-games")}/{path}");
        if (guestToken != null)
        {
            message.Headers.Add(GuestTokenHeader, guestToken);
        }

        return message;
    }
}
