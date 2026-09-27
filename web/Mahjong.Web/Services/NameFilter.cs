using System.Globalization;
using System.Text;

namespace Mahjong.Web.Services;

/// <summary>
/// Refuses offensive display names, and names that would pass for the site's own staff. Names are
/// checked after simplifying them: lower case, no accents, the usual number-for-letter swaps undone
/// ("sh1t", "a$$"), and punctuation and spaces dropped ("f.u.c.k"). Words that are also parts of
/// ordinary words ("Scunthorpe", "grape", "Dickens") are only refused on their own.
/// </summary>
/// <remarks>
/// No filter is perfect; this catches the common cases. To refuse another word, add it to the
/// right list below (and a test).
/// </remarks>
public static class NameFilter
{
    // Refused anywhere in the name, even with letters repeated ("fuuuck").
    private static readonly string[] Anywhere =
    [
        "fuck", "shit", "bitch", "whore", "slut", "porn", "penis", "dildo", "twat", "wank",
        "hitler", "asshole", "bollock", "bastard", "motherf", "jizz", "cumshot",
    ];

    // Refused anywhere in the name, spelled as written (squeezing repeats would catch ordinary words).
    private static readonly string[] AnywhereExact =
    [
        "nigger", "nigga", "faggot", "boobs", "pussy", "vagina", "rapist", "molest", "incest",
        "blowjob", "handjob", "wetback", "tranny", "kkk",
    ];

    // Refused only as a whole word, because they're also parts of ordinary words.
    private static readonly HashSet<string> WholeWords =
    [
        "ass", "arse", "cunt", "cock", "dick", "rape", "sex", "sexy", "cum", "anal", "fag", "tits",
        "titty", "kike", "chink", "spic", "gook", "retard", "pedo", "horny", "nude", "nudes", "piss", "nazi", "nazis",
    ];

    // Names that could pass for the site or its staff.
    private static readonly HashSet<string> ReservedWords =
    [
        "admin", "administrator", "moderator", "mod", "official", "staff", "support", "system", "owner",
    ];

    private static readonly string[] ReservedAnywhere = ["mahjonghaus", "kabkolor"];

    /// <summary>Null if the name is fine; otherwise why it isn't.</summary>
    public static string? Check(string name)
    {
        string folded = Fold(name, keepSpaces: false);
        string squeezed = Squeeze(folded);
        var words = Fold(name, keepSpaces: true).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (ReservedAnywhere.Any(folded.Contains) || words.Any(ReservedWords.Contains))
        {
            return "That name is reserved. Please choose another.";
        }

        bool offensive = Anywhere.Any(term => folded.Contains(term) || squeezed.Contains(Squeeze(term)))
            || AnywhereExact.Any(folded.Contains)
            || words.Any(WholeWords.Contains);
        return offensive ? "That name isn't allowed. Please choose another." : null;
    }

    // Lower case, no accents, look-alike digits and symbols as letters, and only letters kept
    // (with single spaces between words if keepSpaces).
    private static string Fold(string name, bool keepSpaces)
    {
        var text = new StringBuilder();
        foreach (char c in name.Normalize(NormalizationForm.FormD).ToLowerInvariant())
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            char letter = c switch
            {
                '0' => 'o', '1' or '!' or '|' => 'i', '3' => 'e', '4' or '@' => 'a', '5' or '$' => 's',
                '7' or '+' => 't', '8' => 'b', '9' => 'g',
                _ => c,
            };

            if (letter is >= 'a' and <= 'z')
            {
                text.Append(letter);
            }
            else if (keepSpaces && char.IsWhiteSpace(letter) && text.Length > 0 && text[^1] != ' ')
            {
                text.Append(' ');
            }
        }

        return text.ToString().Trim();
    }

    // Runs of the same letter as one ("fuuuck" → "fuck").
    private static string Squeeze(string text)
    {
        var squeezed = new StringBuilder(text.Length);
        foreach (char c in text)
        {
            if (squeezed.Length == 0 || squeezed[^1] != c)
            {
                squeezed.Append(c);
            }
        }

        return squeezed.ToString();
    }
}
