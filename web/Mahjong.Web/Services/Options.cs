namespace Mahjong.Web.Services;

/// <summary>Google AdSense settings ("Ads" section). Ads show as placeholders until these are set.</summary>
public sealed class AdsOptions
{
    public const string Section = "Ads";

    /// <summary>The AdSense publisher ID, e.g. ca-pub-1234567890123456.</summary>
    public string? ClientId { get; set; }

    /// <summary>Tall unit beside the board.</summary>
    public string? RailSlot { get; set; }

    /// <summary>Wide unit under the board on narrow screens.</summary>
    public string? BannerSlot { get; set; }

    /// <summary>Unit in the guest results dialog after a win.</summary>
    public string? ResultsSlot { get; set; }

    /// <summary>Whether ads (and their placeholders) are shown at all. Off for now.</summary>
    public bool Show { get; set; }
}

/// <summary>Cloudflare Web Analytics settings ("Analytics" section). Off until the token is set.</summary>
public sealed class AnalyticsOptions
{
    public const string Section = "Analytics";

    /// <summary>The site token from Cloudflare (Web Analytics, JS snippet's "token" value).</summary>
    public string? CloudflareToken { get; set; }
}

/// <summary>Azure Communication Services email settings ("Email" section).</summary>
public sealed class EmailOptions
{
    public const string Section = "Email";

    public string? ConnectionString { get; set; }

    /// <summary>The verified sender, e.g. DoNotReply@xxxx.azurecomm.net.</summary>
    public string? SenderAddress { get; set; }
}

/// <summary>Mahjong Cube prototype settings ("Cube" section).</summary>
public sealed class CubeOptions
{
    public const string Section = "Cube";

    /// <summary>
    /// Which face art each stone uses: "light" (faces made for pale stone) or "dark" (faces made for
    /// dark stone, with pale symbols). Keyed by the stone's name (marble, onyx, ...) or, for a
    /// picture being tried in the cube-stones folder, its file name (dark-marble.png). A stone
    /// that isn't listed is judged by how dark its picture is.
    /// </summary>
    public Dictionary<string, string> StoneFaces { get; set; } = [];

    /// <summary>Development: where stone pictures to try are read from (default: cube-stones at the top of the repository).</summary>
    public string? StonesFolder { get; set; }

    /// <summary>Development: where face art to try is read from (default: cube-faces at the top of the repository).</summary>
    public string? FacesFolder { get; set; }

    /// <summary>The configured face sets, with names and values cleaned up and anything but light or dark left out.</summary>
    public IReadOnlyDictionary<string, string> CleanStoneFaces() => StoneFaces
        .Select(p => (Stone: p.Key.Trim(), Faces: p.Value?.Trim().ToLowerInvariant()))
        .Where(p => p.Stone.Length > 0 && p.Faces is "light" or "dark")
        .GroupBy(p => p.Stone, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(g => g.Key, g => g.Last().Faces!, StringComparer.OrdinalIgnoreCase);
}

/// <summary>Game settings ("Game" section).</summary>
public sealed class GameOptions
{
    public const string Section = "Game";

    /// <summary>Lists hidden layouts (such as Test) in the layout menu. Meant for development.</summary>
    public bool ShowHiddenLayouts { get; set; }
}
