using Mahjong.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Mahjong.Web.Services;

/// <summary>
/// Deletes stored moves that are no longer worth keeping. Moves stay only for games on a
/// leaderboard or a replay list, and for rejected games until they're 30 days old. Guests' games
/// that weren't claimed within a day are deleted altogether.
/// </summary>
public sealed class GameRecordCleanup(ApplicationDbContext db, TimeProvider time)
{
    public static readonly TimeSpan RejectedRetention = TimeSpan.FromDays(30);

    /// <summary>Returns how many games had their moves (or, for unclaimed guest games, everything) deleted.</summary>
    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var guestCutoff = now - GameService.GuestClaimWindow;
        int deleted = await db.Games
            .Where(g => g.UserId == "" && g.StartedUtc < guestCutoff)
            .ExecuteDeleteAsync(cancellationToken);

        var cutoff = now - RejectedRetention;
        var onLeaderboard = db.HighScores.Select(s => s.GameId);
        var onReplayList = db.ReplayScores.Select(s => s.GameId);

        // Guests' wins keep their moves until claimed (or deleted above).
        return deleted + await db.Games
            .Where(g => g.RecordJson != null
                && g.UserId != ""
                && !onLeaderboard.Contains(g.Id)
                && !onReplayList.Contains(g.Id)
                && (g.Status != GameOutcome.Rejected || g.FinishedUtc < cutoff))
            .ExecuteUpdateAsync(s => s.SetProperty(g => g.RecordJson, (string?)null), cancellationToken);
    }
}

/// <summary>Runs <see cref="GameRecordCleanup"/> a minute after start-up, then once a day.</summary>
public sealed class GameRecordCleanupService(IServiceScopeFactory scopes, TimeProvider time, ILogger<GameRecordCleanupService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = TimeSpan.FromMinutes(1);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(delay, time, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            delay = TimeSpan.FromDays(1);
            try
            {
                using var scope = scopes.CreateScope();
                int cleared = await scope.ServiceProvider.GetRequiredService<GameRecordCleanup>().RunAsync(stoppingToken);
                if (cleared > 0)
                {
                    logger.LogInformation("Deleted stored moves for {Count} games", cleared);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Try again tomorrow; a failed cleanup mustn't take the site down.
                logger.LogError(ex, "Cleaning up stored game moves failed");
            }
        }
    }
}
