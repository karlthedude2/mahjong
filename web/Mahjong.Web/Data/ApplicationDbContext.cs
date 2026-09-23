using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mahjong.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<GameEntity> Games => Set<GameEntity>();

    public DbSet<HighScoreEntity> HighScores => Set<HighScoreEntity>();

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
    }
}
