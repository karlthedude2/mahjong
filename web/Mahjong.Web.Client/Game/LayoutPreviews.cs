using System.Text.RegularExpressions;

namespace Mahjong.Web.Client.Game;

/// <summary>
/// Preview pictures for the layout menu, in wwwroot/layouts/previews/{slug}.png. The slug is the
/// layout name in lower case with spaces and punctuation turned into hyphens ("Standard Turtle"
/// is standard-turtle.png). To add one, drop the PNG in that folder; layouts without a picture
/// show a placeholder.
/// </summary>
public static partial class LayoutPreviews
{
    public const string Folder = "layouts/previews";

    public static string Slug(string layoutName) => NonAlphanumeric().Replace(layoutName.ToLowerInvariant(), "-").Trim('-');

    public static string Url(string layoutName) => $"{Folder}/{Slug(layoutName)}.png";

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}

/// <summary>One card in a <see cref="Components.PreviewStrip"/>.</summary>
public sealed record PreviewItem(string Id, string Name, string ImageUrl, string? Badge = null);

/// <summary>What the player chose in the settings dialog.</summary>
public sealed record SettingsChoice(string LayoutName, string TileSetId, string BackgroundId);
