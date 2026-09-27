using Mahjong.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mahjong.Web.Services;

/// <summary>
/// Refuses to create (or save) an account whose display name another player already has, compared
/// by <see cref="DisplayNames.Key"/>. The unique index on DisplayNameKey backs this up.
/// </summary>
public sealed class UniqueDisplayNameValidator(ApplicationDbContext db) : IUserValidator<ApplicationUser>
{
    public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        if (user.DisplayNameKey == "")
        {
            return IdentityResult.Success;
        }

        bool taken = await db.Users.AnyAsync(u => u.DisplayNameKey == user.DisplayNameKey && u.Id != user.Id);
        return taken
            ? IdentityResult.Failed(new IdentityError { Code = "DuplicateDisplayName", Description = "That display name is taken. Please choose another." })
            : IdentityResult.Success;
    }
}
