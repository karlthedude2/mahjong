namespace Mahjong.Web.Client.Game;

/// <summary>An avatar: one of the twelve animals of the Chinese zodiac.</summary>
public sealed record AvatarInfo(string Id, string Name, string Chinese)
{
    /// <summary>The picture: avatars/{id}.png on the server. Until it's there, the "no image" avatar shows instead.</summary>
    public string Url => $"avatars/{Id}.png";
}

/// <summary>
/// The avatars players can pick. To add a picture, save it as web/Mahjong.Web/wwwroot/avatars/{id}.png
/// (square, about 256×256); until then <see cref="NoImageUrl"/> stands in for it.
/// </summary>
public static class Avatars
{
    /// <summary>The "no image" avatar, for players who haven't chosen one and for missing pictures.</summary>
    public const string NoImageUrl = "avatars/none.svg";

    /// <summary>For an img's onerror attribute: swaps a missing picture for the "no image" avatar.</summary>
    public const string FallbackScript = "this.onerror=null;this.src='" + NoImageUrl + "'";

    public static IReadOnlyList<AvatarInfo> All { get; } =
    [
        new("rat", "Rat", "鼠"),
        new("ox", "Ox", "牛"),
        new("tiger", "Tiger", "虎"),
        new("rabbit", "Rabbit", "兔"),
        new("dragon", "Dragon", "龍"),
        new("snake", "Snake", "蛇"),
        new("horse", "Horse", "馬"),
        new("goat", "Goat", "羊"),
        new("monkey", "Monkey", "猴"),
        new("rooster", "Rooster", "雞"),
        new("dog", "Dog", "狗"),
        new("pig", "Pig", "豬"),
    ];

    public static AvatarInfo? Find(string? id) => All.FirstOrDefault(a => a.Id == id);

    /// <summary>The picture for an avatar id, or the "no image" avatar for none (or an unknown id).</summary>
    public static string UrlFor(string? id) => Find(id)?.Url ?? NoImageUrl;
}
