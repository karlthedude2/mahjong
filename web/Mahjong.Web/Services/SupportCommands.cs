namespace Mahjong.Web.Services;

/// <summary>
/// Support jobs run from the command line instead of starting the site (see docs/deploy.md):
/// <c>dotnet run --project web/Mahjong.Web -- rename-player "Old name" "New name" [--apply]</c>.
/// </summary>
public static class SupportCommands
{
    private const string Usage = """
        Usage: rename-player "Current name" "New name" [--apply]
          Shows the player found and what would change. Add --apply to make the change.
        """;

    /// <summary>True if <paramref name="args"/> ask for a support command rather than the site.</summary>
    public static bool IsCommand(string[] args) => args.Length > 0 && args[0] == "rename-player";

    /// <summary>Runs the command; returns 0 if it worked, 1 if it was refused, 2 if it was used wrongly.</summary>
    public static async Task<int> RunAsync(IServiceProvider services, string[] args, TextWriter output)
    {
        bool apply = args.Length == 4 && args[3] == "--apply";
        if (args.Length is not (3 or 4) || (args.Length == 4 && !apply))
        {
            await output.WriteLineAsync(Usage);
            return 2;
        }

        using var scope = services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<PlayerRenamer>().RenameAsync(args[1], args[2], apply);
        if (result.Error != null)
        {
            await output.WriteLineAsync($"Refused: {result.Error} Nothing was changed.");
            return 1;
        }

        await output.WriteLineAsync($"Player:  \"{result.OldName}\" <{result.Email}>");
        await output.WriteLineAsync($"New name: \"{result.NewName}\"");
        await output.WriteLineAsync($"Also on: {result.LeaderboardRows} leaderboard score(s), {result.ReplayRows} replay score(s)");
        await output.WriteLineAsync(result.Applied
            ? $"Renamed \"{result.OldName}\" to \"{result.NewName}\" at {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC."
            : "Nothing was changed. Run again with --apply to rename.");
        return 0;
    }
}
