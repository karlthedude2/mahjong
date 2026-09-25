using System.Net;
using System.Net.Http.Json;
using Mahjong.Core;
using Mahjong.Web.Api;
using Mahjong.Web.Client.Api;
using Mahjong.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Mahjong.Web.Tests;

/// <summary>Guests' games are verified by the server, and a win can be claimed after signing in.</summary>
public sealed class GuestClaimTests : IAsyncLifetime
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
    public async Task AGuestWinIsVerifiedButNotRankedUntilClaimed()
    {
        var (game, result) = await GuestWinAsync(secondsPerMove: 2);

        Assert.True(result.Won);
        Assert.Null(result.Rank);
        Assert.Empty(await LeaderboardAsync());

        var stored = await app.WithDbAsync(db => db.Games.SingleAsync(g => g.Id == game.GameId));
        Assert.Equal("", stored.UserId);
        Assert.NotNull(stored.RecordJson);
    }

    [Fact]
    public async Task SigningInAndClaimingPutsTheWinOnTheLeaderboard()
    {
        var (game, result) = await GuestWinAsync(secondsPerMove: 2);

        var response = await ClaimAsync("alice", game);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var claimed = (await response.Content.ReadFromJsonAsync<ClaimGameResponse>())!;
        Assert.Equal((TestLayout, result.Score, 1, (int?)null), (claimed.Layout, claimed.Score, claimed.Rank, claimed.ReplayRank));

        var entry = Assert.Single(await LeaderboardAsync());
        Assert.Equal(("Alice", result.Score, 24), (entry.DisplayName, entry.Score, entry.Seconds));

        var profile = await app.ClientFor("alice").GetFromJsonAsync<PlayerProfile>("api/me");
        Assert.Equal((1, 1), (profile!.GamesPlayed, profile.GamesWon));
    }

    [Fact]
    public async Task AGuestReplayIsClaimedOntoTheReplayList()
    {
        var original = await StartAsync(app.ClientFor("bob"), "api/games", replayOf: null);
        await FinishAsync(app.ClientFor("bob"), "api/games", original, secondsPerMove: 5);

        var guest = app.ClientFor(null);
        var replay = await StartAsync(guest, "api/guest-games", original.GameId);
        await FinishAsync(guest, "api/guest-games", replay, secondsPerMove: 1);
        var claimed = (await (await ClaimAsync("alice", replay)).Content.ReadFromJsonAsync<ClaimGameResponse>())!;

        Assert.Equal((null, 1, original.GameId), (claimed.Rank, claimed.ReplayRank, claimed.ReplayOf));
        Assert.Equal("Bob", Assert.Single(await LeaderboardAsync()).DisplayName);
        var info = await guest.GetFromJsonAsync<ReplayInfo>($"api/replays/{original.GameId}");
        Assert.Equal("Alice", Assert.Single(info!.Entries).DisplayName);
    }

    [Fact]
    public async Task AWinCanOnlyBeClaimedOnceAndWithItsToken()
    {
        var (game, _) = await GuestWinAsync(secondsPerMove: 2);

        var wrongToken = await ClaimAsync("alice", game with { GuestToken = "not-the-token" });
        var first = await ClaimAsync("alice", game);
        var again = await ClaimAsync("alice", game);
        var someoneElse = await ClaimAsync("bob", game);

        Assert.Equal(HttpStatusCode.Forbidden, wrongToken.StatusCode);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, someoneElse.StatusCode);
        Assert.Single(await LeaderboardAsync());
    }

    [Fact]
    public async Task AGuestGameNeedsItsTokenToBePlayed()
    {
        var guest = app.ClientFor(null);
        var response = await guest.PostAsJsonAsync("api/guest-games", new StartGameRequest(TestLayout));
        var game = (await response.Content.ReadFromJsonAsync<StartGameResponse>())!;
        Assert.False(string.IsNullOrEmpty(game.GuestToken));

        var withoutToken = await guest.PostAsync($"api/guest-games/{game.GameId}/resume", null);

        Assert.Equal(HttpStatusCode.Forbidden, withoutToken.StatusCode);
    }

    [Fact]
    public async Task OnlyWinsCanBeClaimedAndOnlyForADay()
    {
        var guest = app.ClientFor(null);
        var unfinished = await StartAsync(guest, "api/guest-games", replayOf: null);
        var (won, _) = await GuestWinAsync(secondsPerMove: 2);

        var notWon = await ClaimAsync("alice", unfinished);
        app.Time.Advance(GameService.GuestClaimWindow + TimeSpan.FromMinutes(1));
        var tooLate = await ClaimAsync("alice", won);

        Assert.Equal(HttpStatusCode.BadRequest, notWon.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, tooLate.StatusCode);
        Assert.Empty(await LeaderboardAsync());
    }

    [Fact]
    public async Task UnclaimedGuestGamesAreDeletedAfterADay()
    {
        var (unclaimed, _) = await GuestWinAsync(secondsPerMove: 2);
        var (claimed, _) = await GuestWinAsync(secondsPerMove: 2);
        await ClaimAsync("alice", claimed);

        app.Time.Advance(GameService.GuestClaimWindow + TimeSpan.FromMinutes(1));
        await app.RunRecordCleanupAsync();

        var remaining = await app.WithDbAsync(db => db.Games.Select(g => g.Id).ToListAsync());
        Assert.Equal([claimed.GameId], remaining);
    }

    private async Task<(StartGameResponse Game, FinishGameResponse Result)> GuestWinAsync(int secondsPerMove)
    {
        var guest = app.ClientFor(null);
        var game = await StartAsync(guest, "api/guest-games", replayOf: null);
        return (game, await FinishAsync(guest, "api/guest-games", game, secondsPerMove));
    }

    private static async Task<StartGameResponse> StartAsync(HttpClient client, string route, Guid? replayOf)
    {
        var response = await client.PostAsJsonAsync(route, new StartGameRequest(TestLayout, replayOf));
        response.EnsureSuccessStatusCode();
        var game = (await response.Content.ReadFromJsonAsync<StartGameResponse>())!;
        (await client.SendAsync(WithToken(HttpMethod.Post, $"{route}/{game.GameId}/resume", game))).EnsureSuccessStatusCode();
        return game;
    }

    private async Task<FinishGameResponse> FinishAsync(HttpClient client, string route, StartGameResponse game, int secondsPerMove)
    {
        var request = WithToken(HttpMethod.Post, $"{route}/{game.GameId}/finish", game);
        request.Content = JsonContent.Create(new FinishGameRequest(Play(game.Seed, secondsPerMove).Record!));
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<FinishGameResponse>())!;
    }

    private Task<HttpResponseMessage> ClaimAsync(string user, StartGameResponse game) =>
        app.ClientFor(user).PostAsJsonAsync($"api/games/{game.GameId}/claim", new ClaimGameRequest(game.GuestToken!));

    private static HttpRequestMessage WithToken(HttpMethod method, string uri, StartGameResponse game)
    {
        var request = new HttpRequestMessage(method, uri);
        if (game.GuestToken != null)
        {
            request.Headers.Add(ApiEndpoints.GuestTokenHeader, game.GuestToken);
        }

        return request;
    }

    private async Task<List<LeaderboardEntry>> LeaderboardAsync() =>
        (await app.ClientFor(null).GetFromJsonAsync<List<LeaderboardEntry>>($"api/leaderboards/{TestLayout}"))!;

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
