using System.Net;
using System.Net.Http.Json;
using Mahjong.Core;
using Mahjong.Web.Client.Api;
using Mahjong.Web.Services;
using Mahjong.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mahjong.Web.Tests;

public sealed class DisplayNameTests : IAsyncLifetime
{
    private readonly MahjongAppFactory app = new();

    public Task InitializeAsync() => app.AddUserAsync("alice", "Alice");

    public Task DisposeAsync()
    {
        app.Dispose();
        return Task.CompletedTask;
    }

    [Theory]
    [InlineData("Karl 🀄", "Karl 🀄")]
    [InlineData("  Dragon   Master  ", "Dragon Master")]
    [InlineData("👨‍👩‍👧 Family", "👨‍👩‍👧 Family")]
    [InlineData("🇯🇵 Tokyo", "🇯🇵 Tokyo")]
    [InlineData("🐉", "🐉")]
    [InlineData("Zoë & Łukasz", "Zoë & Łukasz")]
    [InlineData("玩家一号", "玩家一号")]
    public void NamesWithLettersAndEmojiAreFine(string input, string expected)
    {
        Assert.Null(DisplayNames.Check(input, out var name));
        Assert.Equal(expected, name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")]
    [InlineData("Bad‮emaN")]   // right-to-left override
    [InlineData("In​visible")] // zero-width space
    [InlineData("Tab\u0007bell")]   // control character
    public void EmptyInvisibleOrPunctuationOnlyNamesAreRefused(string input)
    {
        Assert.NotNull(DisplayNames.Check(input, out _));
    }

    [Theory]
    [InlineData("Fuuuck")]
    [InlineData("f.u.c.k this")]
    [InlineData("Sh1t Happens")]
    [InlineData("Big A$$")]
    [InlineData("Bïtch")]
    [InlineData("the admin")]
    [InlineData("Mahjong Haus Official")]
    [InlineData("KabKolor")]
    public void OffensiveOrReservedNamesAreRefused(string input)
    {
        Assert.NotNull(DisplayNames.Check(input, out _));
    }

    [Theory]
    [InlineData("Scunthorpe United")]
    [InlineData("Grape Ape")]
    [InlineData("Dickens Fan")]
    [InlineData("Class Act")]
    [InlineData("Peacock 🦚")]
    [InlineData("Sussex Tiles")]
    [InlineData("Nazir")]
    [InlineData("Modern Mahjong")]
    [InlineData("Happiness")]
    public void OrdinaryWordsThatContainBadOnesAreFine(string input)
    {
        Assert.Null(DisplayNames.Check(input, out _));
    }

    [Theory]
    [InlineData("Alice")]
    [InlineData("ALICE")]
    [InlineData("Ａｌｉｃｅ")]
    public async Task ANameAnotherPlayerHasIsTakenWhateverTheCapitals(string name)
    {
        using var scope = app.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var carol = new ApplicationUser { UserName = "carol", Email = "carol@example.com", DisplayName = name, DisplayNameKey = DisplayNames.Key(name) };

        var result = await users.CreateAsync(carol);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "DuplicateDisplayName");
    }

    [Fact]
    public async Task AFreeNameCanBeTakenAndTheOwnerCanStillSaveTheirAccount()
    {
        using var scope = app.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var carol = new ApplicationUser { UserName = "carol", Email = "carol@example.com", DisplayName = "Carol 🐼", DisplayNameKey = DisplayNames.Key("Carol 🐼") };

        Assert.True((await users.CreateAsync(carol)).Succeeded);
        carol.Avatar = "tiger";
        Assert.True((await users.UpdateAsync(carol)).Succeeded);
    }

    [Fact]
    public void TheLimitCountsWhatYouSeeSoEmojiCountAsOne()
    {
        Assert.Null(DisplayNames.Check(new string('a', DisplayNames.MaxCharacters), out _));
        Assert.NotNull(DisplayNames.Check(new string('a', DisplayNames.MaxCharacters + 1), out _));

        // An emoji takes two code units but counts as one character, so 24 of them are fine too.
        Assert.Null(DisplayNames.Check(string.Concat(Enumerable.Repeat("🐉", DisplayNames.MaxCharacters)), out _));
    }

    [Fact]
    public async Task DisplayNamesCannotBeChangedThroughTheApi()
    {
        var response = await app.ClientFor("alice").PutAsJsonAsync("api/me", new UpdateProfileRequest("Someone else", null, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Alice", (await app.ClientFor("alice").GetFromJsonAsync<PlayerProfile>("api/me"))!.DisplayName);
    }

    [Fact]
    public async Task LeaderboardsShowEachPlayersCurrentAvatar()
    {
        await app.WithDbAsync(async db =>
        {
            var alice = await db.Users.SingleAsync(u => u.Id == "alice");
            alice.Avatar = "dragon";
            return await db.SaveChangesAsync();
        });

        var client = app.ClientFor("alice");
        var start = await client.PostAsJsonAsync("api/games", new StartGameRequest("Test"));
        var started = (await start.Content.ReadFromJsonAsync<StartGameResponse>())!;
        (await client.PostAsync($"api/games/{started.GameId}/resume", null)).EnsureSuccessStatusCode();
        var game = new MahjongGame(LayoutCatalog.Find("Test")!, started.Seed);
        foreach (var (first, second) in game.Board.LastDeal.Solution)
        {
            game.Tick();
            app.Time.Advance(TimeSpan.FromSeconds(1));
            game.TryRemovePair(game.Board.At(first), game.Board.At(second));
        }

        (await client.PostAsJsonAsync($"api/games/{started.GameId}/finish", new FinishGameRequest(game.Record!))).EnsureSuccessStatusCode();
        var board = await client.GetFromJsonAsync<List<LeaderboardEntry>>("api/leaderboards/Test");

        Assert.Equal(("Alice", "dragon"), (board!.Single().DisplayName, board.Single().Avatar));
    }
}
