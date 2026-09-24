using Mahjong.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Mahjong.Web.Services;

/// <summary>
/// Deletes stored moves that are no longer worth keeping. Moves stay only for games on a
/// leaderboard, and for rejected games until they're 30 days old.
/// </summary>
public sealed class GameRecordCleanup(ApplicationDbContext db, TimeProvider time)
{
    public static readonly TimeSpan RejectedRetention = TimeSpan.FromDays(30);

    /// <summary>Returns how many games had their moves deleted.</summary>
    public Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = time.GetUtcNow().UtcDateTime - RejectedRetention;
        var onLeaderboard = db.HighScores.Select(s => s.GameId);

        return db.Games
            .Where(g => g.RecordJson != null
                && !onLeaderboard.Contains(g.Id)
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
