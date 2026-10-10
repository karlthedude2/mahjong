namespace Mahjong.Web.Client.Game;

/// <summary>How the cube view catches up with the game after a move.</summary>
public static class CubeRedraw
{
    /// <summary>
    /// Compares the tiles drawn (id → face) with the tiles on the board now. Tiles that are only gone
    /// (a pair taken) shrink away. Any tile that's new or has a new face (an undo, a redo, a shuffle)
    /// means drawing the block again.
    /// </summary>
    public static (bool Redraw, int[] Removed) Compare(IReadOnlyDictionary<int, string> drawn, IReadOnlyDictionary<int, string> now)
    {
        bool redraw = now.Any(t => !drawn.TryGetValue(t.Key, out var face) || face != t.Value);
        return (redraw, redraw ? [] : drawn.Keys.Where(id => !now.ContainsKey(id)).ToArray());
    }
}
