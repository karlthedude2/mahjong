using Mahjong.Web.Client.Api;

namespace Mahjong.Web.Client.Game;

/// <summary>
/// Tile set, sound and last layout. Kept in the browser's local storage; the tile set is also
/// saved to the player's profile when signed in, so it follows them to other devices.
/// </summary>
public sealed class PlayerPreferences(BrowserInterop browser, GameApi api)
{
    private const string TileSetKey = "tileSet";
    private const string SoundKey = "sound";
    private const string LayoutKey = "layout";

    private bool signedIn;

    public TileSetInfo TileSet { get; private set; } = TileSets.Default;

    public bool SoundOn { get; private set; } = true;

    public string? LastLayout { get; private set; }

    public async Task LoadAsync(bool isSignedIn)
    {
        signedIn = isSignedIn;
        TileSet = TileSets.Find(await browser.GetSettingAsync(TileSetKey));
        SoundOn = await browser.GetSettingAsync(SoundKey) != "off";
        LastLayout = await browser.GetSettingAsync(LayoutKey);

        if (signedIn)
        {
            var profile = await api.GetProfileAsync();
            if (profile != null)
            {
                TileSet = TileSets.Find(profile.PreferredTileSet);
            }
        }
    }

    public async Task SetTileSetAsync(TileSetInfo set)
    {
        TileSet = set;
        await browser.SetSettingAsync(TileSetKey, set.Id);
        if (signedIn)
        {
            await api.UpdateProfileAsync(new UpdateProfileRequest(null, set.Id));
        }
    }

    public async Task SetSoundAsync(bool on)
    {
        SoundOn = on;
        await browser.SetSettingAsync(SoundKey, on ? "on" : "off");
    }

    public async Task SetLastLayoutAsync(string layout)
    {
        LastLayout = layout;
        await browser.SetSettingAsync(LayoutKey, layout);
    }
}
