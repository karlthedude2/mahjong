using System.Net.Http.Json;

namespace Mahjong.Web.Client.Api;

/// <summary>Calls to the server API. Only used for signed-in players; guests play entirely offline.</summary>
public sealed class GameApi(HttpClient http)
{
    private ClientConfig? config;

    public async Task<ClientConfig> GetConfigAsync()
    {
        return config ??= await http.GetFromJsonAsync<ClientConfig>("api/client-config")
            ?? new ClientConfig(null, null, null, null, false);
    }

    public async Task<StartGameResponse> StartGameAsync(string layout)
    {
        var response = await http.PostAsJsonAsync("api/games", new StartGameRequest(layout));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<StartGameResponse>())!;
    }

    public async Task<FinishGameResponse?> FinishGameAsync(Guid gameId, FinishGameRequest request)
    {
        var response = await http.PostAsJsonAsync($"api/games/{gameId}/finish", request);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<FinishGameResponse>() : null;
    }

    public Task<PlayerProfile?> GetProfileAsync() => http.GetFromJsonAsync<PlayerProfile>("api/me");

    public async Task UpdateProfileAsync(UpdateProfileRequest request)
    {
        var response = await http.PutAsJsonAsync("api/me", request);
        response.EnsureSuccessStatusCode();
    }
}
