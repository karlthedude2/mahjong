using System.Security.Cryptography;
using System.Text.Json;
using Mahjong.Core;
using Mahjong.Web.Client.Api;
using Mahjong.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Mahjong.Web.Services;

public enum GameActionOutcome
{
    Ok,
    NotFound,
    Forbidden,
    AlreadyFinished,
    Invalid
}

public sealed record FinishResult(GameActionOutcome Outcome, FinishGameResponse? Response = null, string? Error = null);

public sealed record ClaimResult(GameActionOutcome Outcome, ClaimGameResponse? Response = null, string? Error = null);

/// <summary>Who is playing: a signed-in player, or a guest holding the game's token.</summary>
public readonly record struct Player(string? UserId, string? GuestToken)
{
    public static Player Account(string userId) => new(userId, null);

    public static Player Guest(string? token) => new(null, token);

    public bool IsGuest => UserId == null;
}

/// <summary>
/// Starts ranked games and verifies finished ones. The server picks the seed, replays the
/// submitted moves with <see cref="GameReplayer"/>, and only ever stores its own score.
/// A game can also replay a leaderboard game's deal (same layout and seed); replays are ranked on
/// that deal's own replay list and never touch the leaderboard.
/// Guests' games are started, timed and verified the same way, then held for a day: if the guest
/// signs in, they can claim a win, which is scored as if they'd been signed in all along.
/// </summary>
public sealed class GameService(ApplicationDbContext db, TimeProvider time, ILogger<GameService> logger)
{
    public const int LeaderboardSize = 20;

    /// <summary>
    /// How far the game clock may drift from the real time played (time since the server started
    /// the game, less the pauses the server recorded), allowing for request latency.
    /// </summary>
    public static readonly TimeSpan ClockTolerance = TimeSpan.FromSeconds(30);

    /// <summary>How long a guest has to sign in and claim a win (and when unclaimed guest games are deleted).</summary>
    public static readonly TimeSpan GuestClaimWindow = TimeSpan.FromDays(1);

    public async Task<StartGameResponse?> StartAsync(Player player, string layoutName, bool allowHiddenLayouts)
    {
        var layout = LayoutCatalog.Find(layoutName);
        if (layout == null || (layout.Hidden && !allowHiddenLayouts))
        {
            return null;
        }

        return await AddGameAsync(player, layout.Name, NewSeed(), replayOf: null);
    }

    /// <summary>
    /// Starts a replay of a leaderboard game's deal. Returns null unless that game is on a
    /// leaderboard now (once it drops off, its replay list stays viewable but closes to new plays).
    /// </summary>
    public async Task<StartGameResponse?> StartReplayAsync(Player player, Guid originalGameId)
    {
        var original = await OriginalOnLeaderboardAsync(originalGameId);
        return original == null ? null : await AddGameAsync(player, original.LayoutName, original.Seed, original.Id);
    }

    private async Task<StartGameResponse> AddGameAsync(Player player, string layoutName, long seed, Guid? replayOf)
    {
        string? guestToken = player.IsGuest ? NewGuestToken() : null;
        var now = time.GetUtcNow().UtcDateTime;
        var game = new GameEntity
        {
            Id = Guid.NewGuid(),
            UserId = player.UserId ?? "",
            GuestTokenHash = guestToken == null ? null : Hash(guestToken),
            LayoutName = layoutName,
            Seed = seed,
            ReplayOfGameId = replayOf,
            StartedUtc = now,
            Status = GameOutcome.InProgress,

            // The clock starts with the player's first move, which resumes the game; until then
            // the game counts as paused, so looking at the board first costs nothing.
            PausedAtUtc = now,
        };
        db.Games.Add(game);
        if (player.UserId is { } userId)
        {
            await db.Users.Where(u => u.Id == userId)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.GamesPlayed, x => x.GamesPlayed + 1));
        }

        await db.SaveChangesAsync();

        return new StartGameResponse(game.Id, game.Seed, guestToken);
    }

    public async Task<FinishResult> FinishAsync(Player player, Guid gameId, GameRecord? record)
    {
        var (game, problem) = await FindOwnGameInProgressAsync(player, gameId);
        if (game == null)
        {
            return new(problem);
        }

        var now = time.GetUtcNow().UtcDateTime;
        string? error = Verify(game, record, now, out var replay);

        game.FinishedUtc = now;
        string? recordJson = record == null ? null : JsonSerializer.Serialize(record);

        if (error != null)
        {
            // A rejected game can't be resubmitted. Its moves are kept for 30 days, to look into
            // if the player reports a problem.
            game.Status = GameOutcome.Rejected;
            game.RecordJson = recordJson;
            await db.SaveChangesAsync();
            logger.LogWarning("Rejected game {GameId} from {UserId}: {Error}", gameId, player.UserId ?? "a guest", error);
            return new(GameActionOutcome.Invalid, Error: error);
        }

        game.Status = replay!.Complete ? GameOutcome.Won : GameOutcome.Lost;
        game.Score = replay.Score;
        game.Seconds = replay.ElapsedSeconds;

        int? rank = null;
        int? replayRank = null;
        bool heldForClaim = false;
        if (replay.Complete)
        {
            if (player.UserId is { } userId)
            {
                if (await db.Users.FindAsync(userId) is { } user)
                {
                    user.GamesWon++;
                    (rank, replayRank) = await RankWinAsync(game, user);
                }
            }
            else
            {
                // A guest's win waits (with its moves) for them to sign in and claim it.
                heldForClaim = true;
            }
        }

        // The moves are the proof behind a listed score; other games only need their result.
        game.RecordJson = rank != null || replayRank != null || heldForClaim ? recordJson : null;

        await db.SaveChangesAsync();
        return new(GameActionOutcome.Ok, new FinishGameResponse(replay.Complete, replay.Score, BreakdownDto.From(replay.Breakdown), rank, replayRank));
    }

    /// <summary>
    /// Pauses or resumes a ranked game. The server times pauses itself, so paused time is
    /// left out of the clock check without trusting the browser. Repeating a call is harmless.
    /// </summary>
    public async Task<GameActionOutcome> SetPausedAsync(Player player, Guid gameId, bool paused)
    {
        var (game, problem) = await FindOwnGameInProgressAsync(player, gameId);
        if (game == null)
        {
            return problem;
        }

        var now = time.GetUtcNow().UtcDateTime;
        if (paused && game.PausedAtUtc == null)
        {
            game.PausedAtUtc = now;
        }
        else if (!paused && game.PausedAtUtc is { } pausedAt)
        {
            game.PausedSeconds += (now - pausedAt).TotalSeconds;
            game.PausedAtUtc = null;
        }

        await db.SaveChangesAsync();
        return GameActionOutcome.Ok;
    }

    public async Task<IReadOnlyList<LeaderboardEntry>> GetLeaderboardAsync(string layoutName)
    {
        var top = await db.HighScores
            .Where(s => s.LayoutName == layoutName)
            .OrderByDescending(s => s.Score).ThenBy(s => s.AchievedUtc)
            .Take(LeaderboardSize)
            .ToListAsync();

        var gameIds = top.Select(s => s.GameId).ToList();
        var replayCounts = await db.ReplayScores
            .Where(r => gameIds.Contains(r.OriginalGameId))
            .GroupBy(r => r.OriginalGameId)
            .Select(g => new { GameId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.GameId, g => g.Count);

        return top.Select((s, i) => new LeaderboardEntry(
            i + 1, s.DisplayName, s.Score, s.Seconds, s.AchievedUtc, s.GameId, replayCounts.GetValueOrDefault(s.GameId))).ToList();
    }

    /// <summary>
    /// A leaderboard game's deal and its replay list, or null if the game has never been on a
    /// leaderboard (or has dropped off with nobody having replayed it).
    /// </summary>
    public async Task<ReplayInfo?> GetReplayInfoAsync(Guid originalGameId)
    {
        var original = await db.Games.FindAsync(originalGameId);
        if (original?.Score is not { } originalScore || original.FinishedUtc is not { } finished)
        {
            return null;
        }

        var onLeaderboard = await db.HighScores.FirstOrDefaultAsync(s => s.GameId == originalGameId);
        var replays = await db.ReplayScores
            .Where(s => s.OriginalGameId == originalGameId)
            .OrderByDescending(s => s.Score).ThenBy(s => s.AchievedUtc)
            .Take(LeaderboardSize)
            .ToListAsync();
        if (onLeaderboard == null && replays.Count == 0)
        {
            return null;
        }

        string? player = onLeaderboard?.DisplayName
            ?? await db.Users.Where(u => u.Id == original.UserId).Select(u => u.DisplayName).FirstOrDefaultAsync();

        return new ReplayInfo(
            original.Id,
            original.LayoutName,
            original.Seed,
            string.IsNullOrWhiteSpace(player) ? "Player" : player,
            originalScore,
            onLeaderboard?.Seconds ?? original.Seconds ?? 0,
            finished,
            CanPlay: onLeaderboard != null,
            replays.Select((s, i) => new LeaderboardEntry(i + 1, s.DisplayName, s.Score, s.Seconds, s.AchievedUtc, s.GameId)).ToList());
    }

    /// <summary>
    /// A random seed below 2^53, so it survives a round trip through JavaScript numbers
    /// (handy for tools and tests that call the API from a browser).
    /// </summary>
    private static long NewSeed() => (long)(BitConverter.ToUInt64(RandomNumberGenerator.GetBytes(8)) >> 11);

    private static string? Verify(GameEntity game, GameRecord? record, DateTime now, out ReplayResult? replay)
    {
        replay = null;
        if (record == null || record.Seed != game.Seed || record.LayoutName != game.LayoutName)
        {
            return "The record doesn't match the game that was started.";
        }

        replay = GameReplayer.Replay(record);
        if (!replay.Valid)
        {
            return replay.Error;
        }

        var playTime = game.PlayTime(now);
        var gameClock = TimeSpan.FromSeconds(replay.ElapsedSeconds);
        if (gameClock > playTime + ClockTolerance || gameClock < playTime - ClockTolerance)
        {
            return $"Game clock ({gameClock.TotalSeconds:0}s) doesn't match the time played ({playTime.TotalSeconds:0}s).";
        }

        return null;
    }

    private async Task<(GameEntity? Game, GameActionOutcome Problem)> FindOwnGameInProgressAsync(Player player, Guid gameId)
    {
        var game = await db.Games.FindAsync(gameId);
        if (game == null)
        {
            return (null, GameActionOutcome.NotFound);
        }

        bool owns = player.UserId is { } userId
            ? game.UserId == userId
            : game.UserId == "" && TokenMatches(player.GuestToken, game.GuestTokenHash);
        if (!owns)
        {
            return (null, GameActionOutcome.Forbidden);
        }

        if (game.Status != GameOutcome.InProgress)
        {
            return (null, GameActionOutcome.AlreadyFinished);
        }

        return (game, GameActionOutcome.Ok);
    }

    /// <summary>
    /// Gives a guest's verified win to the player who has just signed in, and ranks it as if they'd
    /// been signed in when they played. Only the browser that played it (holding its token) can
    /// claim it, only once, and only within <see cref="GuestClaimWindow"/>.
    /// </summary>
    public async Task<ClaimResult> ClaimAsync(string userId, Guid gameId, string? guestToken)
    {
        var game = await db.Games.FindAsync(gameId);
        if (game == null)
        {
            return new(GameActionOutcome.NotFound);
        }

        if (game.UserId == userId)
        {
            return new(GameActionOutcome.AlreadyFinished, Error: "This game has already been saved.");
        }

        if (game.UserId != "" || !TokenMatches(guestToken, game.GuestTokenHash))
        {
            return new(GameActionOutcome.Forbidden);
        }

        if (game.Status != GameOutcome.Won)
        {
            return new(GameActionOutcome.Invalid, Error: "Only won games can be saved.");
        }

        if (time.GetUtcNow().UtcDateTime - game.FinishedUtc > GuestClaimWindow)
        {
            return new(GameActionOutcome.Invalid, Error: "This game is too old to save.");
        }

        if (await db.Users.FindAsync(userId) is not { } user)
        {
            return new(GameActionOutcome.NotFound);
        }

        game.UserId = userId;
        game.GuestTokenHash = null;
        user.GamesPlayed++;
        user.GamesWon++;
        var (rank, replayRank) = await RankWinAsync(game, user);
        if (rank == null && replayRank == null)
        {
            game.RecordJson = null;
        }

        await db.SaveChangesAsync();
        return new(GameActionOutcome.Ok, new ClaimGameResponse(game.LayoutName, game.Score!.Value, rank, replayRank, game.ReplayOfGameId));
    }

    /// <summary>Puts a verified win on its leaderboard, or on its deal's replay list if it's a replay.</summary>
    private async Task<(int? Rank, int? ReplayRank)> RankWinAsync(GameEntity game, ApplicationUser user)
    {
        int seconds = game.Seconds ?? 0;
        return game.ReplayOfGameId is { } originalGameId
            ? (null, await AddToReplayListAsync(game, user, originalGameId, seconds))
            : (await AddToLeaderboardAsync(game, user, seconds), null);
    }

    private static string NewGuestToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));

    private static bool TokenMatches(string? token, string? hash) =>
        !string.IsNullOrEmpty(token) && hash != null
        && CryptographicOperations.FixedTimeEquals(System.Text.Encoding.ASCII.GetBytes(Hash(token)), System.Text.Encoding.ASCII.GetBytes(hash));

    /// <summary>The original game if it's on a leaderboard now; only those deals can be replayed.</summary>
    private async Task<GameEntity?> OriginalOnLeaderboardAsync(Guid gameId) =>
        await db.HighScores.AnyAsync(s => s.GameId == gameId) ? await db.Games.FindAsync(gameId) : null;

    /// <summary>
    /// Puts a replay on its deal's replay list if it's the player's best there and makes the top 20.
    /// Returns the new rank, or null if the list didn't change.
    /// </summary>
    private async Task<int?> AddToReplayListAsync(GameEntity game, ApplicationUser user, Guid originalGameId, int seconds)
    {
        var list = await db.ReplayScores
            .Where(s => s.OriginalGameId == originalGameId)
            .OrderByDescending(s => s.Score).ThenBy(s => s.AchievedUtc)
            .ToListAsync();
        int score = game.Score!.Value;

        // Each player is listed once, with their best replay.
        var entry = list.FirstOrDefault(s => s.UserId == user.Id);
        if (entry != null ? score <= entry.Score : list.Count >= LeaderboardSize && score <= list[LeaderboardSize - 1].Score)
        {
            return null;
        }

        var noLongerProof = new List<Guid>();
        if (entry == null)
        {
            entry = new ReplayScoreEntity { OriginalGameId = originalGameId, UserId = user.Id };
            db.ReplayScores.Add(entry);
            list.Add(entry);
        }
        else
        {
            noLongerProof.Add(entry.GameId);
        }

        entry.DisplayName = string.IsNullOrWhiteSpace(user.DisplayName) ? "Player" : user.DisplayName;
        entry.Score = score;
        entry.Seconds = seconds;
        entry.AchievedUtc = game.FinishedUtc!.Value;
        entry.GameId = game.Id;

        // Ties keep the earlier score ahead; anything past the top 20 drops off.
        var ordered = list.OrderByDescending(s => s.Score).ThenBy(s => s.AchievedUtc).ToList();
        var dropped = ordered.Skip(LeaderboardSize).ToList();
        db.ReplayScores.RemoveRange(dropped);
        noLongerProof.AddRange(dropped.Select(s => s.GameId));
        foreach (var oldGame in await db.Games.Where(g => noLongerProof.Contains(g.Id)).ToListAsync())
        {
            oldGame.RecordJson = null;
        }

        return ordered.IndexOf(entry) + 1;
    }

    /// <summary>Adds the score if it makes the top 20, trims the list, and returns the new rank.</summary>
    private async Task<int?> AddToLeaderboardAsync(GameEntity game, ApplicationUser user, int seconds)
    {
        var top = await db.HighScores
            .Where(s => s.LayoutName == game.LayoutName)
            .OrderByDescending(s => s.Score).ThenBy(s => s.AchievedUtc)
            .ToListAsync();

        if (top.Count >= LeaderboardSize && game.Score <= top[LeaderboardSize - 1].Score)
        {
            return null;
        }

        var entry = new HighScoreEntity
        {
            LayoutName = game.LayoutName,
            UserId = user.Id,
            DisplayName = string.IsNullOrWhiteSpace(user.DisplayName) ? "Player" : user.DisplayName,
            Score = game.Score!.Value,
            Seconds = seconds,
            AchievedUtc = game.FinishedUtc!.Value,
            GameId = game.Id,
        };
        db.HighScores.Add(entry);

        // Ties keep the earlier score ahead, so a new entry goes after equal scores.
        int index = top.Count(s => s.Score >= entry.Score);
        // The new entry is in, so anything past 19 of the old list drops off, along with the
        // stored moves behind it.
        var dropped = top.Skip(LeaderboardSize - 1).ToList();
        db.HighScores.RemoveRange(dropped);
        var droppedGameIds = dropped.Select(s => s.GameId).ToList();
        foreach (var droppedGame in await db.Games.Where(g => droppedGameIds.Contains(g.Id)).ToListAsync())
        {
            droppedGame.RecordJson = null;
        }

        return index + 1;
    }
}
