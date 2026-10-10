using System.Net;
using System.Net.Http.Json;
using Mahjong.Core;
using Mahjong.Web.Client.Api;

namespace Mahjong.Web.Tests;

/// <summary>Mahjong Cube games are started, verified and ranked like solitaire ones.</summary>
public sealed class CubeApiTests : IAsyncLifetime
{
    private const string Cube = "Cube Small";

    private readonly MahjongAppFactory app = new();

    public Task InitializeAsync() => app.AddUserAsync("alice", "Alice");

    public Task DisposeAsync()
    {
        app.Dispose();
        return Task.CompletedTask;
    }

    private static async Task<StartGameResponse> StartAsync(HttpClient client, StartGameRequest request)
    {
        var response = await client.PostAsJsonAsync("api/games", request);
        response.EnsureSuccessStatusCode();
        var started = (await response.Content.ReadFromJsonAsync<StartGameResponse>())!;
        (await client.PostAsync($"api/games/{started.GameId}/resume", null)).EnsureSuccessStatusCode();
        return started;
    }

    [Fact]
    public async Task ACubeWinWithAShuffleIsVerifiedAndRankedOnItsSize()
    {
        var client = app.ClientFor("alice");
        var started = await StartAsync(client, new StartGameRequest(Cube));

        var game = new MahjongGame(LayoutCatalog.Find(Cube)!, started.Seed);
        game.Shuffle();
        foreach (var (first, second) in game.Board.LastDeal.Solution)
        {
            game.Tick();
            app.Time.Advance(TimeSpan.FromSeconds(1));
            Assert.True(game.TryRemovePair(game.Board.At(first), game.Board.At(second)));
        }

        game.Finish();
        var finish = await client.PostAsJsonAsync($"api/games/{started.GameId}/finish", new FinishGameRequest(game.Record!));

        Assert.Equal(HttpStatusCode.OK, finish.StatusCode);
        var result = (await finish.Content.ReadFromJsonAsync<FinishGameResponse>())!;
        Assert.Equal((true, game.Scoring.Card.Score, 1), (result.Won, result.Score, result.Rank));
        Assert.Equal(ScoreCard.NoShuffleBonusStart - Scoring.ShufflePenalty, result.Breakdown.NoShuffleBonus);

        var small = await client.GetFromJsonAsync<List<LeaderboardEntry>>($"api/leaderboards/{Uri.EscapeDataString(Cube)}");
        Assert.Single(small!);
        var large = await client.GetFromJsonAsync<List<LeaderboardEntry>>($"api/leaderboards/{Uri.EscapeDataString("Cube Large")}");
        Assert.Empty(large!);
    }

    [Fact]
    public async Task ARandomCubeDealStartsAsARandomDeal()
    {
        var started = await StartAsync(app.ClientFor("alice"), new StartGameRequest(Cube, RandomDeal: true));

        Assert.True(started.RandomDeal);
    }

    [Fact]
    public async Task TheLeaderboardPageHasACubeTab()
    {
        string html = await app.ClientFor(null).GetStringAsync($"leaderboards?layout={Uri.EscapeDataString("Cube Large")}");

        Assert.Contains("Mahjong Cube", html);
        Assert.Contains("<option value=\"Cube Large\" selected", html);
        Assert.Contains("<option value=\"Cube Small\"", html);
    }
}
