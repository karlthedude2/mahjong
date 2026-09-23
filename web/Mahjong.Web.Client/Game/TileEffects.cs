using Mahjong.Core;

namespace Mahjong.Web.Client.Game;

/// <summary>
/// Hook for feedback when tiles are picked. Today it plays a click; tile animations can be added
/// here later (the board draws each tile as its own SVG element, so CSS animations apply directly).
/// </summary>
public interface ITileEffects
{
    ValueTask OnSelectedAsync(Tile tile);

    ValueTask OnPairRemovedAsync(Tile first, Tile second);
}

public sealed class SoundTileEffects(BrowserInterop browser, PlayerPreferences preferences) : ITileEffects
{
    private const string ClickSound = "sounds/click.wav";

    public ValueTask OnSelectedAsync(Tile tile) => PlayAsync();

    public ValueTask OnPairRemovedAsync(Tile first, Tile second) => PlayAsync();

    private async ValueTask PlayAsync()
    {
        if (preferences.SoundOn)
        {
            await browser.PlaySoundAsync(ClickSound);
        }
    }
}
