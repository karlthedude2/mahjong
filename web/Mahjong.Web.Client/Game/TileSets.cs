namespace Mahjong.Web.Client.Game;

public enum TileSetKind
{
    /// <summary>One PNG per face in wwwroot/tilesets/{id}/{face}.png.</summary>
    Images,

    /// <summary>Drawn by the app as bold, colour-coded text; needs no image files.</summary>
    Glyphs,

    /// <summary>
    /// Stone blocks in 3D (the Board3D component), with the Mahjong Cube's stones and face art. Connect
    /// boards stay flat and draw these as glyphs.
    /// </summary>
    ThreeD
}

/// <param name="Stone">For a 3D set, the stone its tiles are made of (marble, darkMarble, onyx, ivory).</param>
public sealed record TileSetInfo(string Id, string Name, TileSetKind Kind, string? Stone = null);

/// <summary>A drawn tile face: a large main symbol, a small caption and a colour class.</summary>
public sealed record TileGlyph(string Main, string Caption, string ColorClass);

/// <summary>
/// The available tile sets. To add an image set, put {face}.png files (2bams.png, Cdragon.png, ...)
/// in wwwroot/tilesets/{id}/ and add a line to <see cref="All"/>.
/// </summary>
public static class TileSets
{
    public static readonly TileSetInfo Classic = new("classic", "Classic", TileSetKind.Images);
    public static readonly TileSetInfo Simple = new("simple", "Simple (high contrast)", TileSetKind.Glyphs);
    public static readonly TileSetInfo Marble3D = new("3d-marble", "3D Marble", TileSetKind.ThreeD, "marble");
    public static readonly TileSetInfo DarkMarble3D = new("3d-dark-marble", "3D Dark Marble", TileSetKind.ThreeD, "darkMarble");
    public static readonly TileSetInfo Onyx3D = new("3d-onyx", "3D Onyx", TileSetKind.ThreeD, "onyx");
    public static readonly TileSetInfo Ivory3D = new("3d-ivory", "3D Ivory", TileSetKind.ThreeD, "ivory");

    public static IReadOnlyList<TileSetInfo> All { get; } = [Classic, Simple, Marble3D, DarkMarble3D, Onyx3D, Ivory3D];

    public static TileSetInfo Default => Classic;

    public static TileSetInfo Find(string? id) => All.FirstOrDefault(s => s.Id == id) ?? Default;

    public static string ImageUrl(TileSetInfo set, string face) => $"tilesets/{set.Id}/{face}.png";

    /// <summary>
    /// How a face from <see cref="Mahjong.Core.TileSet.Standard"/> is drawn in the Simple set:
    /// big, bold and colour-coded so tiles are easy to tell apart at any size.
    /// </summary>
    public static TileGlyph Glyph(string face)
    {
        switch (face)
        {
            case "East": return new("東", "East", "wind");
            case "South": return new("南", "South", "wind");
            case "West": return new("西", "West", "wind");
            case "North": return new("北", "North", "wind");
            case "Cdragon": return new("中", "Red", "red");
            case "Fdragon": return new("發", "Green", "green");
            case "Bdragon": return new("白", "White", "blue");
            case "peacock": return new("✦", "Bird", "green");
        }

        string n = face.Substring(0, 1);
        return face.Substring(1) switch
        {
            "crack" => new(n, "Crak", "red"),
            "bams" => new(n, "Bam", "green"),
            "dots" => new(n, "Dot", "blue"),
            "flower" => new("✿" + n, "Flower", "flower"),
            "wizard" => new("☼" + n, "Season", "season"),
            _ => new(face, "", "wind"),
        };
    }
}
