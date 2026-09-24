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

/// <summary>Game settings ("Game" section).</summary>
public sealed class GameOptions
{
    public const string Section = "Game";

    /// <summary>Lists hidden layouts (such as Test) in the layout menu. Meant for development.</summary>
    public bool ShowHiddenLayouts { get; set; }
}
