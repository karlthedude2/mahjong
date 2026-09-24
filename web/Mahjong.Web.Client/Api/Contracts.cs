using Mahjong.Core;

namespace Mahjong.Web.Client.Api;

// Request and response shapes shared by the browser client and the server API.

public sealed record StartGameRequest(string Layout);

public sealed record StartGameResponse(Guid GameId, long Seed);

public sealed record FinishGameRequest(GameRecord Record);

/// <summary>The server's verdict after replaying a game. The score is always the server's own.</summary>
public sealed record FinishGameResponse(bool Won, int Score, BreakdownDto Breakdown, int? Rank);

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

/// <summary>A leaderboard row. Seconds is how long the game took (0 for scores recorded before times were kept).</summary>
public sealed record LeaderboardEntry(int Rank, string DisplayName, int Score, int Seconds, DateTime AchievedUtc)
{
    /// <summary>The time as m:ss (or h:mm:ss), or an empty string if it wasn't recorded.</summary>
    public string TimeText => Seconds <= 0 ? "" : TimeSpan.FromSeconds(Seconds).ToString(Seconds >= 3600 ? @"h\:mm\:ss" : @"m\:ss");
}

public sealed record PlayerProfile(string DisplayName, string PreferredTileSet, string PreferredBackground, int GamesPlayed, int GamesWon);

public sealed record UpdateProfileRequest(string? DisplayName, string? PreferredTileSet, string? PreferredBackground);

/// <summary>Settings the browser needs from the server's configuration.</summary>
public sealed record ClientConfig(string? AdsClientId, string? AdSlotRail, string? AdSlotBanner, string? AdSlotResults, bool ShowHiddenLayouts);
