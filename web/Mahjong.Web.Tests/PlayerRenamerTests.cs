using Mahjong.Web.Data;
using Mahjong.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mahjong.Web.Tests;

/// <summary>Renaming a player for support (the rename-player command).</summary>
public sealed class PlayerRenamerTests : IAsyncLifetime
{
    private readonly MahjongAppFactory app = new();

    public async Task InitializeAsync()
    {
        await app.AddUserAsync("alice", "Alise");
        await app.AddUserAsync("bob", "Bob");
        await app.WithDbAsync(async db =>
        {
            db.HighScores.Add(new HighScoreEntity { LayoutName = "Test", UserId = "alice", DisplayName = "Alise", Score = 900 });
            db.HighScores.Add(new HighScoreEntity { LayoutName = "Cube Small", UserId = "alice", DisplayName = "Alise", Score = 800 });
            db.HighScores.Add(new HighScoreEntity { LayoutName = "Test", UserId = "bob", DisplayName = "Bob", Score = 700 });
            db.ReplayScores.Add(new ReplayScoreEntity { OriginalGameId = Guid.NewGuid(), UserId = "alice", DisplayName = "Alise", Score = 600 });
            return await db.SaveChangesAsync();
        });
    }

    public Task DisposeAsync()
    {
        app.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ARenameChangesTheNameEverywhereItIsShown()
    {
        var result = await RenameAsync("Alise", "Alice", apply: true);

        Assert.Null(result.Error);
        Assert.True(result.Applied);
        var user = await app.WithDbAsync(db => db.Users.SingleAsync(u => u.Id == "alice"));
        Assert.Equal("Alice", user.DisplayName);
        Assert.Equal(DisplayNames.Key("Alice"), user.DisplayNameKey);
        Assert.All(await app.WithDbAsync(db => db.HighScores.Where(s => s.UserId == "alice").ToListAsync()), s => Assert.Equal("Alice", s.DisplayName));
        Assert.All(await app.WithDbAsync(db => db.ReplayScores.Where(s => s.UserId == "alice").ToListAsync()), s => Assert.Equal("Alice", s.DisplayName));
        Assert.Equal("Bob", (await app.WithDbAsync(db => db.HighScores.SingleAsync(s => s.UserId == "bob"))).DisplayName);
    }

    [Fact]
    public async Task ItReportsWhoItFoundAndWhatItChanges()
    {
        var result = await RenameAsync("alise", "Alice", apply: false);

        Assert.Equal(new RenameResult(null, "alice@example.com", "Alise", "Alice", LeaderboardRows: 2, ReplayRows: 1, Applied: false), result);
    }

    [Fact]
    public async Task WithoutApplyNothingChanges()
    {
        await RenameAsync("Alise", "Alice", apply: false);

        Assert.Equal("Alise", (await app.WithDbAsync(db => db.Users.SingleAsync(u => u.Id == "alice"))).DisplayName);
        Assert.All(await app.WithDbAsync(db => db.HighScores.Where(s => s.UserId == "alice").ToListAsync()), s => Assert.Equal("Alise", s.DisplayName));
    }

    [Theory]
    [InlineData("Bob")]
    [InlineData("BOB")]
    public async Task AnotherPlayersNameIsRefused(string taken)
    {
        var result = await RenameAsync("Alise", taken, apply: true);

        Assert.NotNull(result.Error);
        Assert.False(result.Applied);
        Assert.Equal("Alise", (await app.WithDbAsync(db => db.Users.SingleAsync(u => u.Id == "alice"))).DisplayName);
    }

    [Fact]
    public async Task APlayersOwnNameCanChangeItsCapitals()
    {
        var result = await RenameAsync("Alise", "ALISE", apply: true);

        Assert.Null(result.Error);
        Assert.Equal("ALISE", (await app.WithDbAsync(db => db.Users.SingleAsync(u => u.Id == "alice"))).DisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A name that is far too long to fit")]
    [InlineData("Ali​ce")]
    public async Task NamesSignUpWouldRefuseAreRefused(string bad)
    {
        var result = await RenameAsync("Alise", bad, apply: true);

        Assert.NotNull(result.Error);
        Assert.False(result.Applied);
    }

    [Fact]
    public async Task TheNewNameIsTidiedAsAtSignUp()
    {
        var result = await RenameAsync("Alise", "  Alice   Smith ", apply: true);

        Assert.Equal("Alice Smith", result.NewName);
        Assert.Equal("Alice Smith", (await app.WithDbAsync(db => db.Users.SingleAsync(u => u.Id == "alice"))).DisplayName);
    }

    [Fact]
    public async Task AnUnknownPlayerIsReported()
    {
        var result = await RenameAsync("Nobody", "Somebody", apply: true);

        Assert.NotNull(result.Error);
        Assert.False(result.Applied);
    }

    [Fact]
    public async Task TheCommandShowsWhatItWouldDoWithoutApply()
    {
        var (code, output) = await RunCommandAsync("rename-player", "Alise", "Alice");

        Assert.Equal(0, code);
        Assert.Contains("alice@example.com", output);
        Assert.Contains("Nothing was changed", output);
        Assert.Equal("Alise", (await app.WithDbAsync(db => db.Users.SingleAsync(u => u.Id == "alice"))).DisplayName);
    }

    [Fact]
    public async Task TheCommandRenamesWithApply()
    {
        var (code, output) = await RunCommandAsync("rename-player", "Alise", "Alice", "--apply");

        Assert.Equal(0, code);
        Assert.Contains("Renamed", output);
        Assert.Equal("Alice", (await app.WithDbAsync(db => db.Users.SingleAsync(u => u.Id == "alice"))).DisplayName);
    }

    [Fact]
    public async Task TheCommandFailsWhenTheRenameIsRefused()
    {
        var (code, output) = await RunCommandAsync("rename-player", "Alise", "Bob", "--apply");

        Assert.Equal(1, code);
        Assert.Contains("Another player", output);
    }

    [Theory]
    [InlineData("rename-player")]
    [InlineData("rename-player", "Alise")]
    [InlineData("rename-player", "Alise", "Alice", "--aply")]
    public async Task TheCommandExplainsItselfWhenUsedWrongly(params string[] args)
    {
        var (code, output) = await RunCommandAsync(args);

        Assert.Equal(2, code);
        Assert.Contains("Usage", output);
    }

    private async Task<(int Code, string Output)> RunCommandAsync(params string[] args)
    {
        var output = new StringWriter();
        int code = await SupportCommands.RunAsync(app.Services, args, output);
        return (code, output.ToString());
    }

    private async Task<RenameResult> RenameAsync(string current, string wanted, bool apply)
    {
        using var scope = app.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<PlayerRenamer>().RenameAsync(current, wanted, apply);
    }
}
