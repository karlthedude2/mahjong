namespace Mahjong.Web.Client.Game;

public enum LeaderboardLinkKind
{
    /// <summary>A leaderboard ("leaderboards?layout=…").</summary>
    Leaderboard,

    /// <summary>A deal's replay list ("replays/{game}").</summary>
    Replays,

    /// <summary>Play a leaderboard game's deal ("?replay={game}").</summary>
    PlayDeal,
}

/// <summary>
/// A link to the leaderboards, a replay list or a replayable deal. On the game page these open the
/// leaderboard dialog (or start the deal) instead of leaving the game.
/// </summary>
public sealed record LeaderboardLink(LeaderboardLinkKind Kind, string? Layout = null, Guid GameId = default)
{
    /// <summary>Reads a link relative to the site's base ("replays/…"), or null if it's some other link.</summary>
    public static LeaderboardLink? Parse(string link)
    {
        int q = link.IndexOf('?');
        string path = q < 0 ? link : link[..q];
        string? Query(string name) => q < 0 ? null : link[(q + 1)..].Split('&')
            .Select(p => p.Split('=', 2))
            .Where(p => p.Length == 2 && p[0] == name)
            .Select(p => Uri.UnescapeDataString(p[1].Replace('+', ' ')))
            .FirstOrDefault();

        if (path == "leaderboards")
        {
            return new LeaderboardLink(LeaderboardLinkKind.Leaderboard, Layout: Query("layout"));
        }

        if (path.StartsWith("replays/", StringComparison.Ordinal) && Guid.TryParse(path["replays/".Length..], out var replays))
        {
            return new LeaderboardLink(LeaderboardLinkKind.Replays, GameId: replays);
        }

        if (path == "" && Guid.TryParse(Query("replay"), out var deal))
        {
            return new LeaderboardLink(LeaderboardLinkKind.PlayDeal, GameId: deal);
        }

        return null;
    }
}
