using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Mahjong.Web.Data;

public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Room for a display name in the database, in UTF-16 code units. Names are limited to 24 visible
    /// characters (see DisplayNames), but an emoji can take several code units.
    /// </summary>
    public const int DisplayNameMaxLength = 64;

    /// <summary>The name shown on leaderboards, chosen when the account is created. It can't be changed.</summary>
    [MaxLength(DisplayNameMaxLength)]
    public string DisplayName { get; set; } = "";

    [MaxLength(32)]
    public string PreferredTileSet { get; set; } = "classic";

    /// <summary>The chosen page background, or empty for the site default.</summary>
    [MaxLength(32)]
    public string PreferredBackground { get; set; } = "";

    /// <summary>True if the player turned off the settings dialog that opens for each new game.</summary>
    public bool SkipSettingsOnNewGame { get; set; }

    /// <summary>True if the player turned off "Guaranteed winnable path" and wants random deals.</summary>
    public bool PreferRandomDeals { get; set; }

    /// <summary>The chosen avatar's id (a Chinese zodiac animal, e.g. "dragon"), or empty for none.</summary>
    [MaxLength(16)]
    public string Avatar { get; set; } = "";

    public int GamesPlayed { get; set; }

    public int GamesWon { get; set; }
}
