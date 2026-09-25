using System.ComponentModel.DataAnnotations;

namespace Mahjong.Web.Data;

public enum GameOutcome
{
    InProgress,
    Won,
    Lost,

    /// <summary>The submitted moves failed the server's check (impossible moves or a faked clock).</summary>
    Rejected
}

/// <summary>
/// A game the server started (and so can verify). The seed is chosen by the server. A guest's game
/// has no user until they sign in and claim it; until then only the holder of the guest token can
/// play it, and unclaimed guest games are deleted after a day.
/// </summary>
public class GameEntity
{
    public Guid Id { get; set; }

    /// <summary>The player, or empty for a guest's game that hasn't been claimed.</summary>
    [MaxLength(450)]
    public string UserId { get; set; } = "";

    /// <summary>SHA-256 (hex) of a guest game's token; null for signed-in players' games and once claimed.</summary>
    [MaxLength(64)]
    public string? GuestTokenHash { get; set; }

    [MaxLength(64)]
    public string LayoutName { get; set; } = "";

    public long Seed { get; set; }

    /// <summary>For a replay, the leaderboard game whose deal (layout and seed) this game replays.</summary>
    public Guid? ReplayOfGameId { get; set; }

    public DateTime StartedUtc { get; set; }

    public DateTime? FinishedUtc { get; set; }

    public GameOutcome Status { get; set; }

    /// <summary>Total time the game spent paused, measured by the server.</summary>
    public double PausedSeconds { get; set; }

    /// <summary>When the current pause began, if the game is paused now.</summary>
    public DateTime? PausedAtUtc { get; set; }

    /// <summary>Real time spent playing: time since the start, less time spent paused.</summary>
    public TimeSpan PlayTime(DateTime now)
    {
        var paused = TimeSpan.FromSeconds(PausedSeconds);
        if (PausedAtUtc is { } pausedAt)
        {
            paused += now - pausedAt;
        }

        return now - StartedUtc - paused;
    }

    /// <summary>The score the server computed by replaying the game.</summary>
    public int? Score { get; set; }

    /// <summary>How long the game took by the game clock (paused time excluded), once verified.</summary>
    public int? Seconds { get; set; }

    /// <summary>
    /// The submitted moves, kept only where they're worth auditing: while the game is on a
    /// leaderboard or a replay list, and for 30 days after a game is rejected (see <c>GameRecordCleanup</c>).
    /// </summary>
    public string? RecordJson { get; set; }
}

/// <summary>One entry on a layout's top-20 leaderboard.</summary>
public class HighScoreEntity
{
    public int Id { get; set; }

    [MaxLength(64)]
    public string LayoutName { get; set; } = "";

    [MaxLength(450)]
    public string UserId { get; set; } = "";

    [MaxLength(ApplicationUser.DisplayNameMaxLength)]
    public string DisplayName { get; set; } = "";

    public int Score { get; set; }

    /// <summary>How long the game took, in game-clock seconds (paused time excluded), as verified by the server.</summary>
    public int Seconds { get; set; }

    public DateTime AchievedUtc { get; set; }

    public Guid GameId { get; set; }
}

/// <summary>
/// A player's best score replaying a leaderboard game's deal. Kept apart from the leaderboard:
/// a replay can beat the original, but the original (a first play) never changes.
/// </summary>
public class ReplayScoreEntity
{
    public int Id { get; set; }

    /// <summary>The leaderboard game whose deal was replayed.</summary>
    public Guid OriginalGameId { get; set; }

    [MaxLength(450)]
    public string UserId { get; set; } = "";

    [MaxLength(ApplicationUser.DisplayNameMaxLength)]
    public string DisplayName { get; set; } = "";

    public int Score { get; set; }

    public int Seconds { get; set; }

    public DateTime AchievedUtc { get; set; }

    /// <summary>The replay game that set this score.</summary>
    public Guid GameId { get; set; }
}
