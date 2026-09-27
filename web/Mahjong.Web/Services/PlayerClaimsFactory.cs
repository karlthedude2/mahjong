using System.Security.Claims;
using Mahjong.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Mahjong.Web.Services;

/// <summary>
/// Adds the player's display name and avatar to their sign-in, so the header can show them without
/// a database lookup on every page. Changing the avatar refreshes the sign-in to pick it up.
/// </summary>
public sealed class PlayerClaimsFactory(UserManager<ApplicationUser> users, IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser>(users, options)
{
    public const string DisplayNameClaim = "mahjong:display_name";
    public const string AvatarClaim = "mahjong:avatar";

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(DisplayNameClaim, user.DisplayName));
        identity.AddClaim(new Claim(AvatarClaim, user.Avatar));
        return identity;
    }
}
