using Mahjong.Core;

namespace Mahjong.Web.Client.Api;

// Request and response shapes shared by the browser client and the server API.

/// <summary>Starts a ranked game. With ReplayOf, the game replays that leaderboard game's deal.</summary>
public sealed record StartGameRequest(string Layout, Guid? ReplayOf = null);

public sealed record StartGameResponse(Guid GameId, long Seed);

public sealed record FinishGameRequest(GameRecord Record);

/// <summary>
/// The server's verdict after replaying a game. The score is always the server's own. Rank is the
/// place on the layout's leaderboard; ReplayRank the place on a replayed deal's replay list.
/// </summary>
public sealed record FinishGameResponse(bool Won, int Score, BreakdownDto Breakdown, int? Rank, int? ReplayRank = null);

public sealed record BreakdownDto(
    int TilePoints,
    int SpeedBonusCount,
    int SpeedBonusTotal,
    int PaceBonusTotal,
    int TimeBonus,
    int QuickFinishBonus,
    int NoShuffleBonus,
    int Total)
{
    public static BreakdownDto From(ScoreBreakdown b) => new(
        b.TilePoints, b.SpeedBonusCount, b.SpeedBonusTotal, b.PaceBonusTotal,
        b.TimeBonus, b.QuickFinishBonus, b.NoShuffleBonus, b.Total);
}

/// <summary>
/// A leaderboard or replay-list row. Seconds is how long the game took (0 for scores recorded
/// before times were kept). GameId is the game that set the score; ReplayCount is how many
/// players are on its replay list (leaderboard rows only).
/// </summary>
public sealed record LeaderboardEntry(int Rank, string DisplayName, int Score, int Seconds, DateTime AchievedUtc, Guid GameId = default, int ReplayCount = 0)
{
    /// <summary>The time as m:ss (or h:mm:ss), or an empty string if it wasn't recorded.</summary>
    public string TimeText => Seconds <= 0 ? "" : TimeSpan.FromSeconds(Seconds).ToString(Seconds >= 3600 ? @"h\:mm\:ss" : @"m\:ss");
}

/// <summary>
/// A leaderboard game's deal and its replay list. The original score was a first play and stays
/// on the leaderboard; replays are ranked separately. CanPlay is false once the original has
/// dropped off the leaderboard (the list stays viewable).
/// </summary>
public sealed record ReplayInfo(
    Guid OriginalGameId,
    string Layout,
    long Seed,
    string OriginalPlayer,
    int OriginalScore,
    int OriginalSeconds,
    DateTime OriginalAchievedUtc,
    bool CanPlay,
    IReadOnlyList<LeaderboardEntry> Entries);

public sealed record PlayerProfile(string DisplayName, string PreferredTileSet, string PreferredBackground, int GamesPlayed, int GamesWon);

public sealed record UpdateProfileRequest(string? DisplayName, string? PreferredTileSet, string? PreferredBackground);

/// <summary>Settings the browser needs from the server's configuration.</summary>
public sealed record ClientConfig(string? AdsClientId, string? AdSlotRail, string? AdSlotBanner, string? AdSlotResults, bool ShowHiddenLayouts);
