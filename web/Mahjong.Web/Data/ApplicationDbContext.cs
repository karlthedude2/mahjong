using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mahjong.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    /// <summary>Identity's table layout (Version3 adds passkeys). Shared by the app and the EF tools.</summary>
    public static readonly Version IdentitySchemaVersion = IdentitySchemaVersions.Version3;

    public DbSet<GameEntity> Games => Set<GameEntity>();

    public DbSet<HighScoreEntity> HighScores => Set<HighScoreEntity>();

    public DbSet<ReplayScoreEntity> ReplayScores => Set<ReplayScoreEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<GameEntity>(game =>
        {
            game.ToTable("Games");
            game.HasIndex(g => new { g.UserId, g.Status });
            game.Property(g => g.Status).HasConversion<string>().HasMaxLength(16);
        });

        builder.Entity<HighScoreEntity>(score =>
        {
            score.ToTable("HighScores");
            score.HasIndex(s => new { s.LayoutName, s.Score });
        });

        builder.Entity<ReplayScoreEntity>(score =>
        {
            score.ToTable("ReplayScores");
            // One entry per player per deal: their best.
            score.HasIndex(s => new { s.OriginalGameId, s.UserId }).IsUnique();
            score.HasIndex(s => new { s.OriginalGameId, s.Score });
        });
    }
}
