using Mahjong.Core;

namespace Mahjong.Web.Client.Game;

/// <summary>
/// Hook for feedback when tiles are picked. Today it plays a click; tile animations can be added
/// here later (the board draws each tile as its own SVG element, so CSS animations apply directly).
/// </summary>
public interface ITileEffects
{
    /// <summary>Called once when the game page opens, to load sounds (and later, other assets).</summary>
    ValueTask PreloadAsync();

    /// <summary>Called as soon as a free tile is clicked, before anything else happens.</summary>
    ValueTask OnClickedAsync(Tile tile);

    /// <summary>Called after a matching pair has been removed.</summary>
    ValueTask OnPairRemovedAsync(Tile first, Tile second);
}

/// <summary>Plays a click; the browser skips it when the header's speaker button has muted sound.</summary>
public sealed class SoundTileEffects(BrowserInterop browser) : ITileEffects
{
    private const string ClickSound = "sounds/click.wav";

    public ValueTask PreloadAsync() => browser.PreloadSoundAsync(ClickSound);

    public ValueTask OnClickedAsync(Tile tile) => browser.PlaySoundAsync(ClickSound);

    // The click already sounded; a matched pair has no extra effect yet.
    public ValueTask OnPairRemovedAsync(Tile first, Tile second) => ValueTask.CompletedTask;
}
