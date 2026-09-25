using System.Net;
using System.Net.Http.Json;
using Mahjong.Core;
using Mahjong.Web.Client.Api;
using Mahjong.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Mahjong.Web.Tests;

/// <summary>Replaying a leaderboard game's deal, and the deal's separate replay list.</summary>
public sealed class ReplayTests : IAsyncLifetime
{
    private const string TestLayout = "Test";

    private readonly MahjongAppFactory app = new();

    public async Task InitializeAsync()
    {
        await app.AddUserAsync("alice", "Alice");
        await app.AddUserAsync("bob", "Bob");
    }

    public Task DisposeAsync()
    {
        app.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AReplayDealsTheOriginalGame()
    {
        var (original, seed) = await WinOriginalAsync("alice", secondsPerMove: 5);

        var replay = await StartAsync(app.ClientFor("bob"), original);

        Assert.Equal(seed, replay.Seed);
        Assert.NotEqual(original, replay.GameId);
    }

    [Fact]
    public async Task OnlyLeaderboardGamesCanBeReplayed()
    {
        var (original, _) = await WinOriginalAsync("alice", secondsPerMove: 5);
        var bob = app.ClientFor("bob");
        var (replayGame, _) = await WinAsync("bob", secondsPerMove: 1, replayOf: original);

        // An unknown game, and a replay (replays never go on the leaderboard).
        foreach (var id in new[] { Guid.NewGuid(), replayGame })
        {
            var response = await bob.PostAsJsonAsync("api/games", new StartGameRequest(TestLayout, id));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Fact]
    public async Task AReplayThatBeatsTheOriginalGoesOnTheReplayListNotTheLeaderboard()
    {
        var (original, _) = await WinOriginalAsync("alice", secondsPerMove: 10);
        var before = await LeaderboardAsync();

        var (_, result) = await WinAsync("bob", secondsPerMove: 1, replayOf: original);

        Assert.Null(result.Rank);
        Assert.Equal(1, result.ReplayRank);

        // The original is untouched, even though the replay scored more.
        var after = await LeaderboardAsync();
        var entry = Assert.Single(after);
        Assert.Equal(("Alice", before[0].Score, original), (entry.DisplayName, entry.Score, entry.GameId));
        Assert.True(result.Score > entry.Score);
        Assert.Equal(1, entry.ReplayCount);

        var info = await ReplayInfoAsync(original);
        Assert.Equal(("Alice", entry.Score, TestLayout, true), (info.OriginalPlayer, info.OriginalScore, info.Layout, info.CanPlay));
        var replay = Assert.Single(info.Entries);
        Assert.Equal(("Bob", result.Score, 1), (replay.DisplayName, replay.Score, replay.Rank));
    }

    [Fact]
    public async Task EachPlayerKeepsOnlyTheirBestReplay()
    {
        var (original, _) = await WinOriginalAsync("alice", secondsPerMove: 5);

        var (_, slow) = await WinAsync("bob", secondsPerMove: 10, replayOf: original);
        var (_, slower) = await WinAsync("bob", secondsPerMove: 15, replayOf: original);
        var (fastGame, fast) = await WinAsync("bob", secondsPerMove: 0, replayOf: original);

        Assert.Equal(1, slow.ReplayRank);
        Assert.Null(slower.ReplayRank);
        Assert.Equal(1, fast.ReplayRank);

        var entry = Assert.Single((await ReplayInfoAsync(original)).Entries);
        Assert.Equal((fast.Score, fastGame), (entry.Score, entry.GameId));

        // Only the listed replay keeps its moves.
        var withMoves = await app.WithDbAsync(db => db.Games.Where(g => g.ReplayOfGameId == original && g.RecordJson != null).Select(g => g.Id).ToListAsync());
        Assert.Equal([fastGame], withMoves);
    }

    [Fact]
    public async Task TheReplayListKeepsTheTop20()
    {
        var (original, _) = await WinOriginalAsync("alice", secondsPerMove: 5);
        var scores = new List<int>();
        for (int i = 0; i < GameService.LeaderboardSize + 2; i++)
        {
            string user = $"player{i}";
            await app.AddUserAsync(user, $"Player {i}");
            var (_, result) = await WinAsync(user, secondsPerMove: i, replayOf: original);
            scores.Add(result.Score);
        }

        var info = await ReplayInfoAsync(original);

        Assert.Equal(scores.OrderByDescending(s => s).Take(GameService.LeaderboardSize), info.Entries.Select(e => e.Score));
        Assert.Equal(GameService.LeaderboardSize, await app.WithDbAsync(db => db.ReplayScores.CountAsync()));
    }

    [Fact]
    public async Task CleanupKeepsTheMovesBehindReplayScores()
    {
        var (original, _) = await WinOriginalAsync("alice", secondsPerMove: 5);
        var (replayGame, _) = await WinAsync("bob", secondsPerMove: 1, replayOf: original);

        await app.RunRecordCleanupAsync();

        var stored = await app.WithDbAsync(db => db.Games.SingleAsync(g => g.Id == replayGame));
        Assert.NotNull(stored.RecordJson);
    }

    [Fact]
    public async Task AForgedReplayIsRejected()
    {
        var (original, seed) = await WinOriginalAsync("alice", secondsPerMove: 5);
        var bob = app.ClientFor("bob");
        var replay = await StartAsync(bob, original);

        var response = await FinishAsync(bob, replay.GameId, Play(seed + 1, secondsPerMove: 1).Record!);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty((await ReplayInfoAsync(original)).Entries);
    }

    [Fact]
    public async Task UnknownReplayListsAreNotFound()
    {
        var response = await app.ClientFor(null).GetAsync($"api/replays/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>Starts, plays and finishes a winning game (or replay); returns its ID and the server's verdict.</summary>
    private async Task<(Guid GameId, FinishGameResponse Result)> WinAsync(string user, int secondsPerMove, Guid replayOf)
    {
        var client = app.ClientFor(user);
        var started = await StartAsync(client, replayOf);
        var response = await FinishAsync(client, started.GameId, Play(started.Seed, secondsPerMove).Record!);
        response.EnsureSuccessStatusCode();
        return (started.GameId, (await response.Content.ReadFromJsonAsync<FinishGameResponse>())!);
    }

    private async Task<(Guid GameId, long Seed)> WinOriginalAsync(string user, int secondsPerMove)
    {
        var client = app.ClientFor(user);
        var started = await StartAsync(client, replayOf: null);
        var response = await FinishAsync(client, started.GameId, Play(started.Seed, secondsPerMove).Record!);
        response.EnsureSuccessStatusCode();
        return (started.GameId, started.Seed);
    }

    private static async Task<StartGameResponse> StartAsync(HttpClient client, Guid? replayOf)
    {
        var response = await client.PostAsJsonAsync("api/games", new StartGameRequest(TestLayout, replayOf));
        response.EnsureSuccessStatusCode();
        var started = (await response.Content.ReadFromJsonAsync<StartGameResponse>())!;
        (await client.PostAsync($"api/games/{started.GameId}/resume", null)).EnsureSuccessStatusCode();
        return started;
    }

    private static Task<HttpResponseMessage> FinishAsync(HttpClient client, Guid gameId, GameRecord record) =>
        client.PostAsJsonAsync($"api/games/{gameId}/finish", new FinishGameRequest(record));

    private async Task<List<LeaderboardEntry>> LeaderboardAsync() =>
        (await app.ClientFor(null).GetFromJsonAsync<List<LeaderboardEntry>>($"api/leaderboards/{TestLayout}"))!;

    private async Task<ReplayInfo> ReplayInfoAsync(Guid original) =>
        (await app.ClientFor(null).GetFromJsonAsync<ReplayInfo>($"api/replays/{original}"))!;

    /// <summary>Plays the deal's solution, moving the real clock along with the game clock.</summary>
    private MahjongGame Play(long seed, int secondsPerMove)
    {
        var game = new MahjongGame(LayoutCatalog.Find(TestLayout)!, seed);
        foreach (var (first, second) in game.Board.LastDeal.Solution)
        {
            for (int s = 0; s < secondsPerMove; s++)
            {
                game.Tick();
            }

            app.Time.Advance(TimeSpan.FromSeconds(secondsPerMove));
            game.TryRemovePair(game.Board.At(first), game.Board.At(second));
        }

        game.Finish();
        return game;
    }
}
