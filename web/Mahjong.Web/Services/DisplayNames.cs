using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using Mahjong.Web.Client;
using Mahjong.Web.Data;
using Microsoft.Extensions.Localization;

namespace Mahjong.Web.Services;

/// <summary>
/// The rules for display names, which are chosen once, when the account is created, and shown on
/// the leaderboards. Letters, numbers, spaces, punctuation and emoji are all fine; invisible
/// control and direction characters aren't, nor are offensive or reserved names (<see cref="NameFilter"/>).
/// Names are unique, compared by <see cref="Key"/>.
/// </summary>
public static class DisplayNames
{
    /// <summary>The most visible characters (an emoji, even one made of several code points, counts as one).</summary>
    public const int MaxCharacters = 24;

    /// <summary>
    /// Tidies <paramref name="input"/> (trims it and collapses runs of spaces) and checks it. Returns
    /// null and the tidied name if it's fine, or a message saying what's wrong.
    /// </summary>
    public static string? Check(string? input, out string name)
    {
        name = string.Join(' ', (input ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (name.Length == 0)
        {
            return "Please choose a display name.";
        }

        int visible = new StringInfo(name).LengthInTextElements;
        if (visible > MaxCharacters || name.Length > ApplicationUser.DisplayNameMaxLength)
        {
            return "Display names can be up to 24 characters.";
        }

        bool hasSubstance = false;
        foreach (var rune in name.EnumerateRunes())
        {
            var category = Rune.GetUnicodeCategory(rune);
            if (!Allowed(rune, category))
            {
                return "Display names can't contain invisible or control characters.";
            }

            hasSubstance |= category is UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter
                or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter or UnicodeCategory.OtherLetter
                or UnicodeCategory.DecimalDigitNumber or UnicodeCategory.LetterNumber or UnicodeCategory.OtherNumber
                or UnicodeCategory.OtherSymbol;
        }

        return hasSubstance ? NameFilter.Check(name) : "Display names need at least one letter, number or emoji.";
    }

    /// <summary>
    /// What makes two names "the same" for uniqueness: compatibility-normalised (so full-width and
    /// styled letters match their plain forms) and upper-cased. "Karl", "KARL" and "Ｋａｒｌ" share a key.
    /// </summary>
    public static string Key(string name) => name.Normalize(NormalizationForm.FormKC).ToUpperInvariant();

    private static bool Allowed(Rune rune, UnicodeCategory category) => category switch
    {
        UnicodeCategory.Control or UnicodeCategory.PrivateUse or UnicodeCategory.OtherNotAssigned
            or UnicodeCategory.Surrogate or UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator => false,

        // Format characters are invisible (and include text-direction overrides); only the joiner
        // that builds emoji like 👨‍👩‍👧 and the emoji tag characters (as in some flags) are needed.
        UnicodeCategory.Format => rune.Value == 0x200D || rune.Value is >= 0xE0020 and <= 0xE007F,

        // The only spaces left after tidying are ordinary ones.
        UnicodeCategory.SpaceSeparator => rune.Value == ' ',
        _ => true,
    };
}

/// <summary>
/// Checks a display name with <see cref="DisplayNames.Check"/> (for registration forms), in the page's
/// language: its messages are keys in Strings.es.resx.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ValidDisplayNameAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
        DisplayNames.Check(value as string, out _) is { } error
            ? new ValidationResult((validationContext.GetService(typeof(IStringLocalizer<Strings>)) as IStringLocalizer<Strings>)?[error] ?? error, [validationContext.MemberName!])
            : ValidationResult.Success;
}
