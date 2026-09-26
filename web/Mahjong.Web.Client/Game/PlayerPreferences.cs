using Mahjong.Web.Client.Api;

namespace Mahjong.Web.Client.Game;

/// <summary>
/// Tile set, background and last layout. Kept in the browser's local storage; the tile set
/// and background are also saved to the player's profile when signed in, so they follow them to
/// other devices.
/// </summary>
public sealed class PlayerPreferences(BrowserInterop browser, GameApi api)
{
    private const string TileSetKey = "tileSet";
    private const string BackgroundKey = "background";
    private const string LayoutKey = "layout";
    private const string SkipSettingsKey = "skipSettings";
    private const string WinnableKey = "winnable";

    private bool signedIn;

    public TileSetInfo TileSet { get; private set; } = TileSets.Default;

    public BackgroundInfo Background { get; private set; } = Backgrounds.Default;

    public string? LastLayout { get; private set; }

    /// <summary>True if the settings dialog shouldn't open for each new game ("Don't show settings for every new game").</summary>
    public bool SkipSettings { get; private set; }

    /// <summary>"Guaranteed winnable path": true (the default) for winnable deals, false for random ones.</summary>
    public bool GuaranteedWinnable { get; private set; } = true;

    public async Task LoadAsync(bool isSignedIn)
    {
        signedIn = isSignedIn;
        TileSet = TileSets.Find(await browser.GetSettingAsync(TileSetKey));
        Background = Backgrounds.Find(await browser.GetSettingAsync(BackgroundKey));
        LastLayout = await browser.GetSettingAsync(LayoutKey);
        SkipSettings = await browser.GetSettingAsync(SkipSettingsKey) == "on";
        GuaranteedWinnable = await browser.GetSettingAsync(WinnableKey) != "off";

        if (signedIn)
        {
            var profile = await api.GetProfileAsync();
            if (profile != null)
            {
                TileSet = TileSets.Find(profile.PreferredTileSet);
                SkipSettings = profile.SkipSettingsOnNewGame;
                GuaranteedWinnable = !profile.PreferRandomDeals;

                // An empty profile value means they haven't chosen yet, so keep this browser's choice.
                if (!string.IsNullOrEmpty(profile.PreferredBackground))
                {
                    Background = Backgrounds.Find(profile.PreferredBackground);
                }
            }
        }

        // Keep the page (and the copy the page head reads on the next visit) in step.
        await browser.SetSettingAsync(BackgroundKey, Background.Id);
        await browser.SetBackgroundAsync(Background.Url);
    }

    public async Task SetTileSetAsync(TileSetInfo set)
    {
        TileSet = set;
        await browser.SetSettingAsync(TileSetKey, set.Id);
        if (signedIn)
        {
            await api.UpdateProfileAsync(new UpdateProfileRequest(null, set.Id, null));
        }
    }

    public async Task SetBackgroundAsync(BackgroundInfo background)
    {
        Background = background;
        await browser.SetSettingAsync(BackgroundKey, background.Id);
        await browser.SetBackgroundAsync(background.Url);
        if (signedIn)
        {
            await api.UpdateProfileAsync(new UpdateProfileRequest(null, null, background.Id));
        }
    }

    public async Task SetSkipSettingsAsync(bool skip)
    {
        SkipSettings = skip;
        await browser.SetSettingAsync(SkipSettingsKey, skip ? "on" : "off");
        if (signedIn)
        {
            await api.UpdateProfileAsync(new UpdateProfileRequest(null, null, null, skip));
        }
    }

    public async Task SetGuaranteedWinnableAsync(bool winnable)
    {
        GuaranteedWinnable = winnable;
        await browser.SetSettingAsync(WinnableKey, winnable ? "on" : "off");
        if (signedIn)
        {
            await api.UpdateProfileAsync(new UpdateProfileRequest(null, null, null, PreferRandomDeals: !winnable));
        }
    }

    public async Task SetLastLayoutAsync(string layout)
    {
        LastLayout = layout;
        await browser.SetSettingAsync(LayoutKey, layout);
    }
}
