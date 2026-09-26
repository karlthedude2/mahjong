using System.Net;
using System.Net.Http.Json;
using Mahjong.Core;
using Mahjong.Web.Client.Api;
using Microsoft.EntityFrameworkCore;

namespace Mahjong.Web.Tests;

/// <summary>Random deals (Guaranteed winnable path off): verified and ranked, marked on the leaderboard.</summary>
public sealed class RandomDealApiTests : IAsyncLifetime
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
    public async Task ARandomDealIsStartedAndStored()
    {
        var started = await StartAsync(app.ClientFor("alice"), new StartGameRequest(TestLayout, RandomDeal: true));

        Assert.True(started.RandomDeal);
        var stored = await app.WithDbAsync(db => db.Games.SingleAsync(g => g.Id == started.GameId));
        Assert.True(stored.RandomDeal);
    }

    [Fact]
    public async Task ARandomDealWinIsRankedAndMarked()
    {
        var alice = app.ClientFor("alice");
        var (started, game) = await StartSolvableAsync(alice);

        var response = await FinishAsync(alice, started, game.Record!);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = (await response.Content.ReadFromJsonAsync<FinishGameResponse>())!;
        Assert.Equal((true, game.Scoring.Card.Score, 1), (result.Won, result.Score, result.Rank));
        var entry = Assert.Single((await alice.GetFromJsonAsync<List<LeaderboardEntry>>($"api/leaderboards/{TestLayout}"))!);
        Assert.True(entry.RandomDeal);
    }

    [Fact]
    public async Task ARecordForTheOtherKindOfDealIsRejected()
    {
        var alice = app.ClientFor("alice");
        var started = await StartAsync(alice, new StartGameRequest(TestLayout, RandomDeal: true));

        // A winnable deal from the same seed: legal moves, but not the game the server dealt.
        var winnable = new MahjongGame(LayoutCatalog.Find(TestLayout)!, started.Seed);
        foreach (var (first, second) in winnable.Board.LastDeal.Solution)
        {
            winnable.Tick();
            app.Time.Advance(TimeSpan.FromSeconds(1));
            winnable.TryRemovePair(winnable.Board.At(first), winnable.Board.At(second));
        }

        var response = await FinishAsync(alice, started, winnable.Record!);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AReplayOfARandomDealIsARandomDealToo()
    {
        var alice = app.ClientFor("alice");
        var (original, game) = await StartSolvableAsync(alice);
        (await FinishAsync(alice, original, game.Record!)).EnsureSuccessStatusCode();

        // Bob asks for a winnable game, but a replay always takes the original's kind of deal.
        var replay = await StartAsync(app.ClientFor("bob"), new StartGameRequest(TestLayout, original.GameId, RandomDeal: false));
        var info = await alice.GetFromJsonAsync<ReplayInfo>($"api/replays/{original.GameId}");

        Assert.True(replay.RandomDeal);
        Assert.Equal(original.Seed, replay.Seed);
        Assert.True(info!.RandomDeal);
    }

    private static async Task<StartGameResponse> StartAsync(HttpClient client, StartGameRequest request)
    {
        var response = await client.PostAsJsonAsync("api/games", request);
        response.EnsureSuccessStatusCode();
        var started = (await response.Content.ReadFromJsonAsync<StartGameResponse>())!;
        (await client.PostAsync($"api/games/{started.GameId}/resume", null)).EnsureSuccessStatusCode();
        return started;
    }

    private static Task<HttpResponseMessage> FinishAsync(HttpClient client, StartGameResponse started, GameRecord record) =>
        client.PostAsJsonAsync($"api/games/{started.GameId}/finish", new FinishGameRequest(record));

    /// <summary>Starts random deals until one can be cleared, and clears it (moving the real clock along).</summary>
    private async Task<(StartGameResponse Started, MahjongGame Game)> StartSolvableAsync(HttpClient client)
    {
        for (int attempt = 0; attempt < 200; attempt++)
        {
            var started = await StartAsync(client, new StartGameRequest(TestLayout, RandomDeal: true));
            var game = new MahjongGame(LayoutCatalog.Find(TestLayout)!, started.Seed, winnable: false);
            if (Solve(game, []))
            {
                game.Finish();
                return (started, game);
            }
        }

        throw new Xunit.Sdk.XunitException("No solvable random deal found.");
    }

    private bool Solve(MahjongGame game, HashSet<string> deadEnds)
    {
        if (game.Board.IsComplete)
        {
            return true;
        }

        string state = string.Join(",", game.Board.Tiles.Select(t => t.Id).Order());
        if (!deadEnds.Add(state))
        {
            return false;
        }

        var free = game.Board.Tiles.Where(game.Board.IsFree).ToList();
        for (int i = 0; i < free.Count; i++)
        {
            for (int j = i + 1; j < free.Count; j++)
            {
                if (!free[i].Face.Matches(free[j].Face))
                {
                    continue;
                }

                game.Tick();
                app.Time.Advance(TimeSpan.FromSeconds(1));
                if (game.TryRemovePair(free[i], free[j]))
                {
                    if (Solve(game, deadEnds))
                    {
                        return true;
                    }

                    game.Undo();
                }
            }
        }

        return false;
    }
}
