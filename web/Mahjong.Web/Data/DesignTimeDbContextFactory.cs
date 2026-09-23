using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mahjong.Web.Data;

/// <summary>
/// Used by the EF Core tools (migrations and the deploy workflow's migration bundle), so they don't
/// have to start the whole app. The bundle's --connection argument supplies the real database.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string LocalDb = "Server=(localdb)\\MSSQLLocalDB;Database=Mahjong;Trusted_Connection=True;MultipleActiveResultSets=true";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // The Identity schema version shapes the model, so it must match Program.cs.
        var services = new ServiceCollection();
        services.Configure<IdentityOptions>(o => o.Stores.SchemaVersion = ApplicationDbContext.IdentitySchemaVersion);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(LocalDb, sql => sql.EnableRetryOnFailure())
            .UseApplicationServiceProvider(services.BuildServiceProvider())
            .Options;
        return new ApplicationDbContext(options);
    }
}
