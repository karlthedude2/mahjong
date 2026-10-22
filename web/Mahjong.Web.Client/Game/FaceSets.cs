namespace Mahjong.Web.Client.Game;

public sealed record FaceSetInfo(string Id, string Name);

/// <summary>
/// The face art sets for Mahjong 3D and the cube, which the player chooses from. Each set is a folder,
/// wwwroot/faces/{id}/: a picture per face ({face}.webp) for light stones and a dark/ folder with the
/// ones for dark stones. Which of the two a stone uses is the server's configuration (Cube:StoneFaces).
/// To add a set, put its folders in place (tools/ship-faces.py {id} makes them) and add a line to
/// <see cref="All"/>.
/// </summary>
public static class FaceSets
{
    public const string Folder = "faces";

    public const string Extension = ".webp";

    public static readonly FaceSetInfo Standard = new("standard", "Standard");

    public static IReadOnlyList<FaceSetInfo> All { get; } = [Standard];

    public static FaceSetInfo Default => Standard;

    /// <summary>A set by its id; one no longer offered becomes the default.</summary>
    public static FaceSetInfo Find(string? id) => All.FirstOrDefault(s => s.Id == id) ?? Default;

    /// <summary>A face's shipped picture in a set, for a light stone or (with <paramref name="dark"/>) a dark one.</summary>
    public static string PictureUrl(FaceSetInfo set, string face, bool dark) =>
        $"{Folder}/{set.Id}/{(dark ? "dark/" : "")}{Uri.EscapeDataString(face)}{Extension}";
}
