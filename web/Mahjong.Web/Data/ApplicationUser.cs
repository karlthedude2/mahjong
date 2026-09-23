using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Mahjong.Web.Data;

public class ApplicationUser : IdentityUser
{
    public const int DisplayNameMaxLength = 30;

    /// <summary>The name shown on leaderboards.</summary>
    [MaxLength(DisplayNameMaxLength)]
    public string DisplayName { get; set; } = "";

    [MaxLength(32)]
    public string PreferredTileSet { get; set; } = "classic";

    [MaxLength(32)]
    public string PreferredBackground { get; set; } = "dragon-mountains";

    public int GamesPlayed { get; set; }

    public int GamesWon { get; set; }
}
