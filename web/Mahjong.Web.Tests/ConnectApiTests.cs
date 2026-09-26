using System.Net;
using System.Net.Http.Json;
using Mahjong.Core;
using Mahjong.Web.Client.Api;

namespace Mahjong.Web.Tests;

/// <summary>Connect (Shisen-Sho) games are started, verified and ranked like solitaire ones.</summary>
public sealed class ConnectApiTests : IAsyncLifetime
{
    private const string Board = "Connect Small (Gravity Down)";

    private readonly MahjongAppFactory app = new();

    public Task InitializeAsync() => app.AddUserAsync("alice", "Alice");

    public Task DisposeAsync()
    {
        app.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AConnectWinWithAHintIsVerifiedAndRankedOnItsBoard()
    {
        var client = app.ClientFor("alice");
        var response = await client.PostAsJsonAsync("api/games", new StartGameRequest(Board));
        response.EnsureSuccessStatusCode();
        var started = (await response.Content.ReadFromJsonAsync<StartGameResponse>())!;
        (await client.PostAsync($"api/games/{started.GameId}/resume", null)).EnsureSuccessStatusCode();

        var game = new MahjongGame(LayoutCatalog.Find(Board)!, started.Seed);
        game.Hint();
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
        Assert.Equal(ScoreCard.NoShuffleBonusStart - Scoring.HintPenalty, result.Breakdown.NoShuffleBonus);

        var board = await client.GetFromJsonAsync<List<LeaderboardEntry>>($"api/leaderboards/{Uri.EscapeDataString(Board)}");
        Assert.Single(board!);
        var otherGravity = await client.GetFromJsonAsync<List<LeaderboardEntry>>($"api/leaderboards/{Uri.EscapeDataString("Connect Small")}");
        Assert.Empty(otherGravity!);
    }

    [Fact]
    public async Task TheLeaderboardPageHasAConnectTab()
    {
        string html = await app.ClientFor(null).GetStringAsync($"leaderboards?layout={Uri.EscapeDataString(Board)}");

        Assert.Contains("Connect (Shisen-Sho)", html);
        Assert.Contains($"<option value=\"{Board}\" selected", html);
    }
}
