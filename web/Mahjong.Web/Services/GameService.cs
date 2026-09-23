using System.Security.Cryptography;
using System.Text.Json;
using Mahjong.Core;
using Mahjong.Web.Client.Api;
using Mahjong.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Mahjong.Web.Services;

public enum FinishOutcome
{
    Ok,
    NotFound,
    Forbidden,
    AlreadyFinished,
    Invalid
}

public sealed record FinishResult(FinishOutcome Outcome, FinishGameResponse? Response = null, string? Error = null);

/// <summary>
/// Starts ranked games and verifies finished ones. The server picks the seed, replays the
/// submitted moves with <see cref="GameReplayer"/>, and only ever stores its own score.
/// </summary>
public sealed class GameService(ApplicationDbContext db, TimeProvider time, ILogger<GameService> logger)
{
    public const int LeaderboardSize = 20;

    /// <summary>
    /// How far the game clock may drift from real time. Ranked games can't be paused, so the
    /// game clock should match the time since the server started the game, less request latency.
    /// </summary>
    public static readonly TimeSpan ClockTolerance = TimeSpan.FromSeconds(30);

    public async Task<StartGameResponse?> StartAsync(string userId, string layoutName, bool allowHiddenLayouts)
    {
        var layout = LayoutCatalog.Find(layoutName);
        if (layout == null || (layout.Hidden && !allowHiddenLayouts))
        {
            return null;
        }

        var game = new GameEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LayoutName = layout.Name,
            Seed = NewSeed(),
            StartedUtc = time.GetUtcNow().UtcDateTime,
            Status = GameOutcome.InProgress,
        };
        db.Games.Add(game);
        await db.Users.Where(u => u.Id == userId)
            .ExecuteUpdateAsync(u => u.SetProperty(x => x.GamesPlayed, x => x.GamesPlayed + 1));
        await db.SaveChangesAsync();

        return new StartGameResponse(game.Id, game.Seed);
    }

    public async Task<FinishResult> FinishAsync(string userId, Guid gameId, GameRecord? record)
    {
        var game = await db.Games.FindAsync(gameId);
        if (game == null)
        {
            return new(FinishOutcome.NotFound);
        }

        if (game.UserId != userId)
        {
            return new(FinishOutcome.Forbidden);
        }

        if (game.Status != GameOutcome.InProgress)
        {
            return new(FinishOutcome.AlreadyFinished);
        }

        var now = time.GetUtcNow().UtcDateTime;
        string? error = Verify(game, record, now, out var replay);

        game.FinishedUtc = now;
        game.RecordJson = record == null ? null : JsonSerializer.Serialize(record);

        if (error != null)
        {
            // A rejected game can't be resubmitted.
            game.Status = GameOutcome.Lost;
            await db.SaveChangesAsync();
            logger.LogWarning("Rejected game {GameId} from {UserId}: {Error}", gameId, userId, error);
            return new(FinishOutcome.Invalid, Error: error);
        }

        game.Status = replay!.Complete ? GameOutcome.Won : GameOutcome.Lost;
        game.Score = replay.Score;

        int? rank = null;
        if (replay.Complete)
        {
            var user = await db.Users.FindAsync(userId);
            if (user != null)
            {
                user.GamesWon++;
                rank = await AddToLeaderboardAsync(game, user);
            }
        }

        await db.SaveChangesAsync();
        return new(FinishOutcome.Ok, new FinishGameResponse(replay.Complete, replay.Score, BreakdownDto.From(replay.Breakdown), rank));
    }

    public async Task<IReadOnlyList<LeaderboardEntry>> GetLeaderboardAsync(string layoutName)
    {
        var top = await db.HighScores
            .Where(s => s.LayoutName == layoutName)
            .OrderByDescending(s => s.Score).ThenBy(s => s.AchievedUtc)
            .Take(LeaderboardSize)
            .ToListAsync();

        return top.Select((s, i) => new LeaderboardEntry(i + 1, s.DisplayName, s.Score, s.AchievedUtc)).ToList();
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

        var wallClock = now - game.StartedUtc;
        var gameClock = TimeSpan.FromSeconds(replay.ElapsedSeconds);
        if (gameClock > wallClock + ClockTolerance || gameClock < wallClock - ClockTolerance)
        {
            return $"Game clock ({gameClock.TotalSeconds:0}s) doesn't match the time since the game started ({wallClock.TotalSeconds:0}s).";
        }

        return null;
    }

    /// <summary>Adds the score if it makes the top 20, trims the list, and returns the new rank.</summary>
    private async Task<int?> AddToLeaderboardAsync(GameEntity game, ApplicationUser user)
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
            AchievedUtc = game.FinishedUtc!.Value,
            GameId = game.Id,
        };
        db.HighScores.Add(entry);

        // Ties keep the earlier score ahead, so a new entry goes after equal scores.
        int index = top.Count(s => s.Score >= entry.Score);
        // The new entry is in, so anything past 19 of the old list drops off.
        db.HighScores.RemoveRange(top.Skip(LeaderboardSize - 1));
        return index + 1;
    }
}
