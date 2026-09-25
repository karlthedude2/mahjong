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

    /// <summary>The chosen page background, or empty for the site default.</summary>
    [MaxLength(32)]
    public string PreferredBackground { get; set; } = "";

    /// <summary>True if the player turned off the settings dialog that opens for each new game.</summary>
    public bool SkipSettingsOnNewGame { get; set; }

    public int GamesPlayed { get; set; }

    public int GamesWon { get; set; }
}
