namespace Mahjong.Web.Client.Game;

public sealed record BackgroundInfo(string Id, string Name)
{
    /// <summary>Backgrounds live in wwwroot/backgrounds/{id}.svg (or .jpg/.png/.webp; see <see cref="Backgrounds"/>).</summary>
    public string Url { get; init; } = $"backgrounds/{Id}.svg";
}

/// <summary>
/// The page backgrounds a player can choose from. To add one, put the image in
/// wwwroot/backgrounds/ and add a line to <see cref="All"/> (set Url for a non-SVG file).
/// A wide image (about 16:9) works best; it's scaled to cover the window.
/// </summary>
public static class Backgrounds
{
    public static readonly BackgroundInfo DragonValley = new("dragon-valley", "Dragon Valley") { Url = "backgrounds/dragon-valley.jpg" };
    public static readonly BackgroundInfo DragonMountains = new("dragon-mountains", "Dragon Mountains");
    public static readonly BackgroundInfo JadeSilk = new("jade-silk", "Jade Silk");

    public static IReadOnlyList<BackgroundInfo> All { get; } = [DragonValley, DragonMountains, JadeSilk];

    public static BackgroundInfo Default => DragonValley;

    public static BackgroundInfo Find(string? id) => All.FirstOrDefault(b => b.Id == id) ?? Default;
}
