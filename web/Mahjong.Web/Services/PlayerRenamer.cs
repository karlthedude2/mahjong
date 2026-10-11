using Mahjong.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Mahjong.Web.Services;

/// <summary>
/// What a rename found and did: an error (nothing changed), or the player's email (to check it's
/// theirs), the old and new names, and how many leaderboard and replay rows carry the name.
/// </summary>
public sealed record RenameResult(
    string? Error,
    string? Email = null,
    string? OldName = null,
    string? NewName = null,
    int LeaderboardRows = 0,
    int ReplayRows = 0,
    bool Applied = false);

/// <summary>
/// Changes a player's display name for support (players choose it once, at sign-up). The name is
/// checked as at sign-up, and changed everywhere it's kept: the account (with its uniqueness key)
/// and the copies on the player's leaderboard and replay rows, all in one save.
/// </summary>
public sealed class PlayerRenamer(ApplicationDbContext db)
{
    /// <summary>Finds the player called <paramref name="currentName"/> (any capitals); renames them only if <paramref name="apply"/>.</summary>
    public async Task<RenameResult> RenameAsync(string currentName, string newName, bool apply)
    {
        string currentKey = DisplayNames.Key(currentName.Trim());
        var user = await db.Users.SingleOrDefaultAsync(u => u.DisplayNameKey == currentKey);
        if (user == null)
        {
            return new RenameResult($"No player is called \"{currentName}\".");
        }

        if (DisplayNames.Check(newName, out string name) is { } problem)
        {
            return new RenameResult(problem);
        }

        string key = DisplayNames.Key(name);
        if (await db.Users.AnyAsync(u => u.DisplayNameKey == key && u.Id != user.Id))
        {
            return new RenameResult($"Another player is already called \"{name}\".");
        }

        var highScores = await db.HighScores.Where(s => s.UserId == user.Id).ToListAsync();
        var replayScores = await db.ReplayScores.Where(s => s.UserId == user.Id).ToListAsync();
        var result = new RenameResult(null, user.Email, user.DisplayName, name, highScores.Count, replayScores.Count);
        if (!apply)
        {
            return result;
        }

        user.DisplayName = name;
        user.DisplayNameKey = key;
        highScores.ForEach(s => s.DisplayName = name);
        replayScores.ForEach(s => s.DisplayName = name);
        await db.SaveChangesAsync();
        return result with { Applied = true };
    }
}
