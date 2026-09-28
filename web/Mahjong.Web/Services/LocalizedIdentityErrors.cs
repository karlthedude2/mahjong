using Mahjong.Web.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace Mahjong.Web.Services;

/// <summary>
/// ASP.NET Identity's error messages (password rules, "email already taken" and so on) in the page's
/// language. The English messages are the keys in Strings.es.resx.
/// </summary>
public sealed class LocalizedIdentityErrors(IStringLocalizer<Strings> L) : IdentityErrorDescriber
{
    private IdentityError Error(IdentityError original, string message, params object[] args) =>
        new() { Code = original.Code, Description = L[message, args] };

    public override IdentityError DefaultError() => Error(base.DefaultError(), "An unknown failure has occurred.");
    public override IdentityError ConcurrencyFailure() => Error(base.ConcurrencyFailure(), "Optimistic concurrency failure, object has been modified.");
    public override IdentityError PasswordMismatch() => Error(base.PasswordMismatch(), "Incorrect password.");
    public override IdentityError InvalidToken() => Error(base.InvalidToken(), "Invalid token.");
    public override IdentityError RecoveryCodeRedemptionFailed() => Error(base.RecoveryCodeRedemptionFailed(), "Recovery code redemption failed.");
    public override IdentityError LoginAlreadyAssociated() => Error(base.LoginAlreadyAssociated(), "A user with this login already exists.");
    public override IdentityError InvalidUserName(string? userName) => Error(base.InvalidUserName(userName), "Username '{0}' is invalid, can only contain letters or digits.", userName ?? "");
    public override IdentityError InvalidEmail(string? email) => Error(base.InvalidEmail(email), "Email '{0}' is invalid.", email ?? "");
    public override IdentityError DuplicateUserName(string userName) => Error(base.DuplicateUserName(userName), "Email '{0}' is already taken.", userName);
    public override IdentityError DuplicateEmail(string email) => Error(base.DuplicateEmail(email), "Email '{0}' is already taken.", email);
    public override IdentityError UserAlreadyHasPassword() => Error(base.UserAlreadyHasPassword(), "User already has a password set.");
    public override IdentityError UserLockoutNotEnabled() => Error(base.UserLockoutNotEnabled(), "Lockout is not enabled for this user.");
    public override IdentityError PasswordTooShort(int length) => Error(base.PasswordTooShort(length), "Passwords must be at least {0} characters.", length);
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Error(base.PasswordRequiresUniqueChars(uniqueChars), "Passwords must use at least {0} different characters.", uniqueChars);
    public override IdentityError PasswordRequiresNonAlphanumeric() => Error(base.PasswordRequiresNonAlphanumeric(), "Passwords must have at least one non alphanumeric character.");
    public override IdentityError PasswordRequiresDigit() => Error(base.PasswordRequiresDigit(), "Passwords must have at least one digit ('0'-'9').");
    public override IdentityError PasswordRequiresLower() => Error(base.PasswordRequiresLower(), "Passwords must have at least one lowercase ('a'-'z').");
    public override IdentityError PasswordRequiresUpper() => Error(base.PasswordRequiresUpper(), "Passwords must have at least one uppercase ('A'-'Z').");
}
