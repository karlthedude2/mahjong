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
    private const string FlatTileSetKey = "flatTileSet";
    private const string StoneKey = "stone";
    private const string FaceSetKey = "faceSet";
    private const string BackgroundKey = "background";
    private const string LayoutKey = "layout";
    private const string SkipSettingsKey = "skipSettings";
    private const string WinnableKey = "winnable";
    private const string SkipCubeIntroKey = "skipCubeIntro";

    private bool signedIn;

    public TileSetInfo TileSet { get; private set; } = TileSets.Default;

    /// <summary>The last flat set chosen (Mahjong Solitaire, Connect), kept while playing in 3D.</summary>
    public TileSetInfo LastFlat { get; private set; } = TileSets.Default;

    /// <summary>The last stone chosen (Mahjong 3D), kept while playing a flat board.</summary>
    public TileSetInfo LastStone { get; private set; } = TileSets.DefaultStone;

    /// <summary>The face art set for Mahjong 3D and the cube.</summary>
    public FaceSetInfo FaceSet { get; private set; } = FaceSets.Default;

    public BackgroundInfo Background { get; private set; } = Backgrounds.Default;

    public string? LastLayout { get; private set; }

    /// <summary>True if the settings dialog shouldn't open for each new game ("Skip this screen next time").</summary>
    public bool SkipSettings { get; private set; }

    /// <summary>"Guaranteed winnable path": true (the default) for winnable deals, false for random ones.</summary>
    public bool GuaranteedWinnable { get; private set; } = true;

    /// <summary>True if the player checked "Don't show again" on the Mahjong Cube intro.</summary>
    public bool SkipCubeIntro { get; private set; }

    public async Task LoadAsync(bool isSignedIn)
    {
        signedIn = isSignedIn;
        TileSet = TileSets.Find(await browser.GetSettingAsync(TileSetKey));
        Background = Backgrounds.Find(await browser.GetSettingAsync(BackgroundKey));
        LastLayout = await browser.GetSettingAsync(LayoutKey);
        SkipSettings = await browser.GetSettingAsync(SkipSettingsKey) == "on";
        GuaranteedWinnable = await browser.GetSettingAsync(WinnableKey) != "off";
        SkipCubeIntro = await browser.GetSettingAsync(SkipCubeIntroKey) == "on";

        if (signedIn)
        {
            var profile = await api.GetProfileAsync();
            if (profile != null)
            {
                TileSet = TileSets.Find(profile.PreferredTileSet);
                SkipSettings = profile.SkipSettingsOnNewGame;
                GuaranteedWinnable = !profile.PreferRandomDeals;
                SkipCubeIntro = profile.SkipCubeIntro;

                // An empty profile value means they haven't chosen yet, so keep this browser's choice.
                if (!string.IsNullOrEmpty(profile.PreferredBackground))
                {
                    Background = Backgrounds.Find(profile.PreferredBackground);
                }
            }
        }

        // The other kind's last choice is kept in this browser only (the profile has one tile set).
        LastFlat = TileSet.Kind == TileSetKind.ThreeD ? FlatOrDefault(await browser.GetSettingAsync(FlatTileSetKey)) : TileSet;
        LastStone = TileSet.Kind == TileSetKind.ThreeD ? TileSet : StoneOrDefault(await browser.GetSettingAsync(StoneKey));
        FaceSet = FaceSets.Find(await browser.GetSettingAsync(FaceSetKey));

        // Keep the page (and the copy the page head reads on the next visit) in step.
        await browser.SetSettingAsync(BackgroundKey, Background.Id);
        await browser.SetBackgroundAsync(Background.Url);
    }

    public async Task SetTileSetAsync(TileSetInfo set)
    {
        TileSet = set;
        await browser.SetSettingAsync(TileSetKey, set.Id);
        if (set.Kind == TileSetKind.ThreeD)
        {
            LastStone = set;
            await browser.SetSettingAsync(StoneKey, set.Id);
        }
        else
        {
            LastFlat = set;
            await browser.SetSettingAsync(FlatTileSetKey, set.Id);
        }

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

    public async Task SetSkipCubeIntroAsync(bool skip)
    {
        SkipCubeIntro = skip;
        await browser.SetSettingAsync(SkipCubeIntroKey, skip ? "on" : "off");
        if (signedIn)
        {
            await api.UpdateProfileAsync(new UpdateProfileRequest(null, null, null, SkipCubeIntro: skip));
        }
    }

    /// <summary>The stone for Mahjong 3D and the cube, leaving the tile set of the flat games as it is.</summary>
    public async Task SetStoneAsync(TileSetInfo stone)
    {
        LastStone = stone;
        await browser.SetSettingAsync(StoneKey, stone.Id);
    }

    /// <summary>The face art set for Mahjong 3D and the cube, kept in this browser like the stone.</summary>
    public async Task SetFaceSetAsync(FaceSetInfo faceSet)
    {
        FaceSet = faceSet;
        await browser.SetSettingAsync(FaceSetKey, faceSet.Id);
    }

    private static TileSetInfo FlatOrDefault(string? id) => TileSets.Flat.FirstOrDefault(s => s.Id == id) ?? TileSets.Default;

    private static TileSetInfo StoneOrDefault(string? id) => TileSets.Stones.FirstOrDefault(s => s.Id == id) ?? TileSets.DefaultStone;

    public async Task SetLastLayoutAsync(string layout)
    {
        LastLayout = layout;
        await browser.SetSettingAsync(LayoutKey, layout);
    }
}
