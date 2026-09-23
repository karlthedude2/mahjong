using System.ComponentModel.DataAnnotations;

namespace Mahjong.Web.Data;

public enum GameOutcome
{
    InProgress,
    Won,
    Lost
}

/// <summary>A game started by a signed-in player. The seed is chosen by the server.</summary>
public class GameEntity
{
    public Guid Id { get; set; }

    [MaxLength(450)]
    public string UserId { get; set; } = "";

    [MaxLength(64)]
    public string LayoutName { get; set; } = "";

    public long Seed { get; set; }

    public DateTime StartedUtc { get; set; }

    public DateTime? FinishedUtc { get; set; }

    public GameOutcome Status { get; set; }

    /// <summary>The score the server computed by replaying the game.</summary>
    public int? Score { get; set; }

    /// <summary>The submitted moves, kept for auditing.</summary>
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

    public DateTime AchievedUtc { get; set; }

    public Guid GameId { get; set; }
}
