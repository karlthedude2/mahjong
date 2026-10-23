using Microsoft.AspNetCore.Localization;

namespace Mahjong.Web.Components.HowToPlay;

/// <summary>One game's how-to-play page: its address and its name in each language.</summary>
public sealed record HelpGame(string Slug, string English, string Spanish);

/// <summary>
/// The how-to-play pages: a hub at /how-to-play and a page per game under it, in English, and the
/// same addresses under /es/ in Spanish. On these pages the address decides the language (see
/// <see cref="HelpCultureProvider"/>), so search engines see one language per address.
/// </summary>
public static class HelpPages
{
    public const string Hub = "how-to-play";

    /// <summary>Address prefixes of the other languages; English has none.</summary>
    public static readonly string[] Prefixes = ["es"];

    public static readonly HelpGame Solitaire = new("mahjong-solitaire", "Mahjong Solitaire", "Mahjong Solitario");
    public static readonly HelpGame ThreeD = new("mahjong-3d", "Mahjong 3D", "Mahjong 3D");
    public static readonly HelpGame Cube = new("mahjong-cube", "Mahjong Cube", "Cubo Mahjong");
    public static readonly HelpGame Connect = new("connect", "Connect (Shisen-Sho)", "Conectar (Shisen-Sho)");

    public static IReadOnlyList<HelpGame> Games { get; } = [Solitaire, ThreeD, Cube, Connect];

    public static HelpGame? Find(string? slug) => Games.FirstOrDefault(g => g.Slug == slug);

    /// <summary>Every help page, relative to the site's root, in every language (for the sitemap).</summary>
    public static IEnumerable<string> All() =>
        new[] { "en" }.Concat(Prefixes).SelectMany(language =>
            new[] { Hub }.Concat(Games.Select(g => $"{Hub}/{g.Slug}")).Select(page => Path(page, language)));

    /// <summary>A page's address (e.g. "how-to-play/connect") in a language.</summary>
    public static string Path(string page, string language) => language == "en" ? page : $"{language}/{page}";

    /// <summary>
    /// For a help page's path (relative to the root, no query), the page without its language prefix
    /// and the language it's in; null for any other page.
    /// </summary>
    public static (string Page, string Language)? Parse(string relativePath)
    {
        // The router matches addresses in any letter case, so this does too.
        string path = relativePath.Trim('/').ToLowerInvariant();
        string language = "en";
        foreach (var prefix in Prefixes)
        {
            if (path == prefix || path.StartsWith(prefix + "/", StringComparison.Ordinal))
            {
                language = prefix;
                path = path.Length == prefix.Length ? "" : path[(prefix.Length + 1)..];
            }
        }

        return path == Hub || path.StartsWith(Hub + "/", StringComparison.Ordinal) ? (path, language) : null;
    }
}

/// <summary>
/// One of the Connect diagram's examples, on a board 6 cells wide and 4 high: the blocking tiles, the
/// pair, the line drawn between them (cells, corners only; -1 and 6 or 4 are outside the board) and
/// whether the rule allows it. HowToPlayTests checks each against ConnectRules.
/// </summary>
public sealed record ConnectExample((int Col, int Row)[] Blocks, (int Col, int Row) A, (int Col, int Row) B, (int Col, int Row)[] Line, bool Allowed, string English, string Spanish);

public static class ConnectExamples
{
    public const int Columns = 6;
    public const int Rows = 4;

    public static readonly ConnectExample[] All =
    [
        new([(0, 3), (1, 3), (5, 3)], (0, 1), (5, 1), [(0, 1), (5, 1)], true, "Straight: no turns", "Recta: sin vueltas"),
        new([(1, 1), (2, 1), (4, 3)], (0, 0), (3, 3), [(0, 0), (3, 0), (3, 3)], true, "One turn", "Una vuelta"),
        new([(2, 3), (3, 3), (1, 2), (4, 2)], (1, 3), (4, 3), [(1, 3), (1, 4), (4, 4), (4, 3)], true, "Two turns, round the edge", "Dos vueltas, por fuera"),
        // Every way out of A (up, left) and into B (from the right, from below) needs a third turn.
        new([(1, 0), (0, 1), (5, 2), (4, 3), (2, 2), (3, 1)], (0, 0), (5, 3), [(0, 0), (0, -1), (6, -1), (6, 3), (5, 3)], false, "Three turns: no", "Tres vueltas: no"),
    ];
}

/// <summary>On a help page, the address's language comes first, ahead of the cookie and the browser.</summary>
public sealed class HelpCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext) =>
        Task.FromResult(HelpPages.Parse(httpContext.Request.Path.Value ?? "") is { } page
            ? new ProviderCultureResult(page.Language)
            : null);
}
