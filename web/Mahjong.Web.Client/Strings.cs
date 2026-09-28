using System.Globalization;
using System.Text.Encodings.Web;
using Mahjong.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Mahjong.Web.Client;

/// <summary>
/// The site's text, for <c>IStringLocalizer&lt;Strings&gt;</c> (injected as <c>L</c> in every page and
/// component). The English text is the key; translations live in Resources/Strings.{culture}.resx
/// (Spanish: Strings.es.resx). Text without a translation shows in English. A test checks that every
/// text passed to L in the code has a Spanish translation.
/// </summary>
public sealed class Strings
{
    /// <summary>The languages the site speaks: English (the default) and Spanish.</summary>
    public static readonly string[] Cultures = ["en", "es"];

    public static bool IsSpanish => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "es";

    /// <summary>
    /// How numbers and dates are written for a language. Spanish text is formatted the Mexican way
    /// (3,000 and 1.5, with Spanish month names): plain "es" writes decimals with a comma, which
    /// would also break the numbers Blazor writes into SVG and style attributes ("86,92").
    /// </summary>
    public static CultureInfo FormattingCulture(CultureInfo language) =>
        language.TwoLetterISOLanguageName == "es" ? CultureInfo.GetCultureInfo("es-MX") : language;
}

public static class LocalizerExtensions
{
    /// <summary>
    /// Text that contains markup (links, bold): the translation is trusted HTML from the resource file,
    /// and each argument is HTML-encoded before it's placed into it, as {0}, {1}...
    /// </summary>
    public static MarkupString Html(this IStringLocalizer localizer, string key, params object?[] args)
    {
        string template = localizer[key];
        var encoded = args.Select(a => a is MarkupString markup ? markup.Value : HtmlEncoder.Default.Encode(Convert.ToString(a, CultureInfo.CurrentCulture) ?? ""));
        return new MarkupString(args.Length == 0 ? template : string.Format(CultureInfo.CurrentCulture, template, encoded.ToArray()));
    }

    /// <summary>
    /// A layout's name as players see it. Layout names stay English inside the game and on the
    /// server (they're what leaderboards are keyed by); only the displayed name is translated.
    /// </summary>
    public static string LayoutName(this IStringLocalizer localizer, LayoutDefinition? layout)
    {
        if (layout == null)
        {
            return "";
        }

        if (layout.Kind != GameKind.Connect)
        {
            return localizer[layout.Name];
        }

        string size = layout.PreviewName[(ConnectLayouts.Prefix.Length + 1)..];
        string board = localizer["Connect {0}", localizer[size]];
        return layout.Gravity == Gravity.None ? board : localizer[$"{{0}} (Gravity {layout.Gravity})", board];
    }

    /// <summary>A layout's displayed name from its (English) name.</summary>
    public static string LayoutName(this IStringLocalizer localizer, string? name) =>
        name == null ? "" : LayoutCatalog.Find(name) is { } layout ? localizer.LayoutName(layout) : name;
}
