namespace Mahjong.Web.Client.Game;

/// <summary>
/// The small pictures on the settings screen's Tiles and Stone cards, until tile sets have
/// pictures of their own.
/// </summary>
public static class SettingsArt
{
    /// <summary>The three faces the Tiles card shows from the chosen flat set.</summary>
    public static IReadOnlyList<string> SampleFaces { get; } = ["Cdragon", "Fdragon", "East"];

    /// <summary>The CSS background of a blank tile in the given stone, for the Stone card.</summary>
    public static string StoneCss(TileSetInfo stone) => stone.Stone switch
    {
        "onyx" => "#1E1C1F",
        "beige" => "#D8C3A0",
        "squareMarble" => "url('textures/square-marble.jpg') center / cover",
        _ => "#EFE6D0",
    };

    /// <summary>
    /// Whether a stone shows a face set's dark pictures: its configured face set (Cube:StoneFaces) if it
    /// has one, otherwise only onyx, the one dark stone offered.
    /// </summary>
    public static bool IsDark(TileSetInfo stone, string? configured) => configured switch
    {
        "dark" => true,
        "light" => false,
        _ => stone.Stone == "onyx",
    };

    /// <summary>A face's picture in the chosen set, for the Stone and Face card.</summary>
    public static string FaceUrl(FaceSetInfo set, string face, bool darkStone) => FaceSets.PictureUrl(set, face, darkStone);
}
