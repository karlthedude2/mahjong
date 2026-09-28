using System.ComponentModel.DataAnnotations;
using Mahjong.Web.Client;
using Microsoft.Extensions.Localization;

namespace Mahjong.Web.Services;

/// <summary>
/// The form checks the account pages use, with their messages (and the field names in them)
/// translated into the page's language: the English message is the key in Strings.es.resx, as
/// is each field's [Display(Name)].
/// </summary>
internal static class LocalizedValidation
{
    public static ValidationResult? Localize(ValidationResult? result, ValidationContext context, string template, params object[] extra)
    {
        if (result == ValidationResult.Success || result == null)
        {
            return result;
        }

        var localizer = context.GetService(typeof(IStringLocalizer<Strings>)) as IStringLocalizer<Strings>;
        string field = localizer?[context.DisplayName] ?? context.DisplayName;
        string message = localizer?[template, [field, .. extra]] ?? string.Format(template, [field, .. extra]);
        return new ValidationResult(message, context.MemberName is { } member ? [member] : []);
    }
}

public sealed class LocalizedRequiredAttribute : RequiredAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context) =>
        LocalizedValidation.Localize(base.IsValid(value, context), context, "The {0} field is required.");
}

public sealed class LocalizedEmailAddressAttribute : ValidationAttribute
{
    private static readonly EmailAddressAttribute Email = new();

    protected override ValidationResult? IsValid(object? value, ValidationContext context) =>
        LocalizedValidation.Localize(Email.IsValid(value) ? ValidationResult.Success : new ValidationResult(""), context, "The {0} field is not a valid email address.");
}

public sealed class LocalizedStringLengthAttribute(int maximumLength) : StringLengthAttribute(maximumLength)
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context) =>
        LocalizedValidation.Localize(base.IsValid(value, context), context, ErrorMessage ?? "The {0} must be at most {1} characters long.", MaximumLength, MinimumLength);
}

public sealed class LocalizedCompareAttribute(string otherProperty) : CompareAttribute(otherProperty)
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context) =>
        LocalizedValidation.Localize(base.IsValid(value, context), context, ErrorMessage ?? "The {0} field doesn't match.");
}
