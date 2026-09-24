using System.Net;
using System.Net.Http.Json;
using Mahjong.Core;
using Mahjong.Web.Client.Api;
using Mahjong.Web.Data;
using Mahjong.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Mahjong.Web.Tests;

public sealed class GameApiTests : IAsyncLifetime
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
    public async Task GuestsCannotStartRankedGames()
    {
        var response = await app.ClientFor(null).PostAsJsonAsync("api/games", new StartGameRequest(TestLayout));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LeaderboardsArePublic()
    {
        var entries = await app.ClientFor(null).GetFromJsonAsync<List<LeaderboardEntry>>($"api/leaderboards/{TestLayout}");
        Assert.NotNull(entries);
    }

    [Fact]
    public async Task AWinIsVerifiedScoredByTheServerAndRanked()
    {
        var client = app.ClientFor("alice");
        var started = await StartAsync(client);

        var game = Play(started.Seed, secondsPerMove: 2);
        var response = await FinishAsync(client, started.GameId, game.Record!);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = (await response.Content.ReadFromJsonAsync<FinishGameResponse>())!;
        Assert.True(result.Won);
        Assert.Equal(game.Scoring.Card.Score, result.Score);
        Assert.Equal(result.Score, result.Breakdown.Total);
        Assert.Equal(1, result.Rank);

        var profile = await client.GetFromJsonAsync<PlayerProfile>("api/me");
        Assert.Equal(1, profile!.GamesPlayed);
        Assert.Equal(1, profile.GamesWon);

        var board = await client.GetFromJsonAsync<List<LeaderboardEntry>>($"api/leaderboards/{TestLayout}");
        Assert.Equal("Alice", Assert.Single(board!).DisplayName);
    }

    [Fact]
    public async Task IllegalMovesAreRejectedAndTheGameCannotBeResubmitted()
    {
        var client = app.ClientFor("alice");
        var started = await StartAsync(client);
        var record = new GameRecord
        {
            LayoutName = TestLayout,
            Seed = started.Seed,
            Moves = [new RecordedMove { Kind = RecordedMoveKind.Remove, Second = 0, TileA = 0, TileB = 0 }],
        };

        var first = await FinishAsync(client, started.GameId, record);
        var second = await FinishAsync(client, started.GameId, record);

        Assert.Equal(HttpStatusCode.BadRequest, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal(0, (await client.GetFromJsonAsync<PlayerProfile>("api/me"))!.GamesWon);
    }

    [Fact]
    public async Task ARecordForADifferentSeedIsRejected()
    {
        var client = app.ClientFor("alice");
        var started = await StartAsync(client);

        var game = Play(started.Seed + 1, secondsPerMove: 1);
        var response = await FinishAsync(client, started.GameId, game.Record!);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AGameClockThatDoesNotMatchRealTimeIsRejected()
    {
        var client = app.ClientFor("alice");
        var started = await StartAsync(client);

        // The moves claim 12 seconds, but ten minutes really passed (e.g. a slowed-down clock).
        var game = Play(started.Seed, secondsPerMove: 1, advanceRealClock: false);
        app.Time.Advance(TimeSpan.FromMinutes(10));
        var response = await FinishAsync(client, started.GameId, game.Record!);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TimeBeforeTheFirstMoveIsNotCounted()
    {
        var client = app.ClientFor("alice");
        var started = await StartAsync(client, startClock: false);

        // Ten minutes studying the board, then the first move starts the clock.
        app.Time.Advance(TimeSpan.FromMinutes(10));
        (await client.PostAsync($"api/games/{started.GameId}/resume", null)).EnsureSuccessStatusCode();

        var game = Play(started.Seed, secondsPerMove: 2);
        var response = await FinishAsync(client, started.GameId, game.Record!);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PausedTimeIsLeftOutOfTheClockCheck()
    {
        var client = app.ClientFor("alice");
        var started = await StartAsync(client);

        // A ten-minute pause in the middle; the game clock doesn't move while paused.
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"api/games/{started.GameId}/pause", null)).StatusCode);
        app.Time.Advance(TimeSpan.FromMinutes(10));
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"api/games/{started.GameId}/resume", null)).StatusCode);

        var game = Play(started.Seed, secondsPerMove: 2);
        var response = await FinishAsync(client, started.GameId, game.Record!);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True((await response.Content.ReadFromJsonAsync<FinishGameResponse>())!.Won);
    }

    [Fact]
    public async Task RepeatedPauseOrResumeCallsAreHarmless()
    {
        var client = app.ClientFor("alice");
        var started = await StartAsync(client);

        await client.PostAsync($"api/games/{started.GameId}/resume", null); // already running: no effect
        await client.PostAsync($"api/games/{started.GameId}/pause", null);
        app.Time.Advance(TimeSpan.FromMinutes(5));
        await client.PostAsync($"api/games/{started.GameId}/pause", null); // already paused: keeps the first time
        app.Time.Advance(TimeSpan.FromMinutes(5));
        await client.PostAsync($"api/games/{started.GameId}/resume", null);

        double paused = await app.WithDbAsync(async db => (await db.Games.FindAsync(started.GameId))!.PausedSeconds);
        Assert.Equal(600, paused, precision: 3);
    }

    [Fact]
    public async Task OnlyTheOwnerCanPauseAnUnfinishedGame()
    {
        var alice = app.ClientFor("alice");
        var started = await StartAsync(alice);

        var byBob = await app.ClientFor("bob").PostAsync($"api/games/{started.GameId}/pause", null);
        await FinishAsync(alice, started.GameId, Play(started.Seed, secondsPerMove: 1).Record!);
        var afterFinish = await alice.PostAsync($"api/games/{started.GameId}/pause", null);

        Assert.Equal(HttpStatusCode.Forbidden, byBob.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, afterFinish.StatusCode);
    }

    [Fact]
    public async Task TileSetAndBackgroundChoicesAreSavedToTheProfile()
    {
        var client = app.ClientFor("alice");

        var before = await client.GetFromJsonAsync<PlayerProfile>("api/me");
        var saved = await client.PutAsJsonAsync("api/me", new UpdateProfileRequest(null, "simple", "jade-silk"));
        var after = await client.GetFromJsonAsync<PlayerProfile>("api/me");
        var rejected = await client.PutAsJsonAsync("api/me", new UpdateProfileRequest(null, null, "../../etc/passwd"));

        Assert.Equal("", before!.PreferredBackground); // not chosen yet: the site default
        Assert.Equal(HttpStatusCode.NoContent, saved.StatusCode);
        Assert.Equal(("simple", "jade-silk"), (after!.PreferredTileSet, after.PreferredBackground));
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
    }

    [Fact]
    public async Task PlayersCannotFinishSomeoneElsesGame()
    {
        var started = await StartAsync(app.ClientFor("alice"));
        var game = Play(started.Seed, secondsPerMove: 1);

        var response = await FinishAsync(app.ClientFor("bob"), started.GameId, game.Record!);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UnknownGamesAreNotFound()
    {
        var response = await FinishAsync(app.ClientFor("alice"), Guid.NewGuid(), new GameRecord { LayoutName = TestLayout });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task HiddenLayoutsCanOnlyBePlayedWhenEnabled()
    {
        using var strict = new MahjongAppFactory { ShowHiddenLayouts = false };
        await strict.AddUserAsync("carol", "Carol");

        var hidden = await strict.ClientFor("carol").PostAsJsonAsync("api/games", new StartGameRequest(TestLayout));
        var visible = await strict.ClientFor("carol").PostAsJsonAsync("api/games", new StartGameRequest("Twin Peaks"));

        Assert.Equal(HttpStatusCode.BadRequest, hidden.StatusCode);
        Assert.Equal(HttpStatusCode.OK, visible.StatusCode);
    }

    [Fact]
    public async Task OnlyTheTopTwentyScoresAreKept()
    {
        // Slower games score lower, so player i's score falls as i rises.
        var scores = new List<int>();
        for (int i = 0; i < GameService.LeaderboardSize + 3; i++)
        {
            string id = $"player{i}";
            await app.AddUserAsync(id, $"Player {i}");
            var client = app.ClientFor(id);
            var started = await StartAsync(client);
            var game = Play(started.Seed, secondsPerMove: 1 + i);
            var response = await FinishAsync(client, started.GameId, game.Record!);
            scores.Add((await response.Content.ReadFromJsonAsync<FinishGameResponse>())!.Score);
        }

        var board = await app.ClientFor(null).GetFromJsonAsync<List<LeaderboardEntry>>($"api/leaderboards/{TestLayout}");
        int stored = await app.WithDbAsync(db => db.HighScores.CountAsync(s => s.LayoutName == TestLayout));

        Assert.Equal(GameService.LeaderboardSize, stored);
        Assert.NotNull(board);
        Assert.Equal(scores.OrderByDescending(s => s).Take(GameService.LeaderboardSize), board.Select(e => e.Score));
        Assert.Equal(Enumerable.Range(1, GameService.LeaderboardSize), board.Select(e => e.Rank));
    }

    /// <summary>Starts a game and makes the first move's call that starts its clock, like the browser does.</summary>
    private static async Task<StartGameResponse> StartAsync(HttpClient client, bool startClock = true)
    {
        var response = await client.PostAsJsonAsync("api/games", new StartGameRequest(TestLayout));
        response.EnsureSuccessStatusCode();
        var started = (await response.Content.ReadFromJsonAsync<StartGameResponse>())!;
        if (startClock)
        {
            (await client.PostAsync($"api/games/{started.GameId}/resume", null)).EnsureSuccessStatusCode();
        }

        return started;
    }

    private static Task<HttpResponseMessage> FinishAsync(HttpClient client, Guid gameId, GameRecord record) =>
        client.PostAsJsonAsync($"api/games/{gameId}/finish", new FinishGameRequest(record));

    /// <summary>Plays the deal's solution, moving the real clock along with the game clock.</summary>
    private MahjongGame Play(long seed, int secondsPerMove, bool advanceRealClock = true)
    {
        var game = new MahjongGame(LayoutCatalog.Find(TestLayout)!, seed);
        foreach (var (first, second) in game.Board.LastDeal.Solution)
        {
            for (int s = 0; s < secondsPerMove; s++)
            {
                game.Tick();
            }

            if (advanceRealClock)
            {
                app.Time.Advance(TimeSpan.FromSeconds(secondsPerMove));
            }

            game.TryRemovePair(game.Board.At(first), game.Board.At(second));
        }

        game.Finish();
        return game;
    }
}
