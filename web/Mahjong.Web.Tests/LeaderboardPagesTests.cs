using Mahjong.Web.Data;

namespace Mahjong.Web.Tests;

/// <summary>
/// The leaderboards, a deal's replays and My stats in the dark style: scores in a light list whose
/// rows don't wrap (two lines on a phone, one row on a computer), with icon links to play a deal and
/// to see its replays.
/// </summary>
public sealed class LeaderboardPagesTests : IDisposable
{
    private static readonly Guid Game = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e");

    private readonly MahjongAppFactory app = new();

    public void Dispose() => app.Dispose();

    private async Task SeedAsync()
    {
        await app.AddUserAsync("u1", "Jade Fox");
        await app.WithDbAsync(async db =>
        {
            db.HighScores.Add(new HighScoreEntity
            {
                LayoutName = "Twin Peaks", UserId = "u1", DisplayName = "Jade Fox", Score = 15410, Seconds = 220,
                AchievedUtc = new DateTime(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc), GameId = Game, RandomDeal = true,
            });
            return await db.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task TheLeaderboardIsALightListInTheDarkPage()
    {
        await SeedAsync();

        string html = await app.CreateClient().GetStringAsync("/leaderboards");

        Assert.Contains("class=\"dark-page", html);
        Assert.Contains("<ol class=\"scores", html);
        Assert.Contains(">Jade Fox</span>", html);
        Assert.Contains(">15,410</span>", html);
        Assert.Contains(">3:40</span>", html);
        Assert.Contains("class=\"tag\"", html);
    }

    [Fact]
    public async Task EachScoreHasIconLinksToItsDealAndReplays()
    {
        await SeedAsync();

        string html = await app.CreateClient().GetStringAsync("/leaderboards");

        Assert.Matches($"<a class=\"icon-action\" href=\"\\?replay={Game}\"[^>]*aria-label=\"Play this deal\"", html);
        Assert.Matches($"<a class=\"icon-action\" href=\"replays/{Game}\"[^>]*aria-label=\"Replays \\(0\\)\"", html);
    }

    [Fact]
    public async Task TheIntroIsFoldedBehindAnInfoButton()
    {
        string html = await app.CreateClient().GetStringAsync("/leaderboards");

        Assert.Matches("<details class=\"about\"", html);
    }

    [Fact]
    public async Task MyStatsIsInTheDarkPageWithTiles()
    {
        await SeedAsync();
        await app.WithDbAsync(async db =>
        {
            db.Games.Add(new GameEntity
            {
                Id = Game, UserId = "u1", LayoutName = "Twin Peaks", Status = GameOutcome.Won, Score = 15410, Seconds = 220,
                StartedUtc = new DateTime(2026, 10, 2, 11, 56, 0, DateTimeKind.Utc), FinishedUtc = new DateTime(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc),
            });
            return await db.SaveChangesAsync();
        });
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Add(MahjongAppFactory.UserHeader, "u1");

        string html = await client.GetStringAsync("/stats");

        Assert.Contains("class=\"dark-page", html);
        Assert.Contains("class=\"stat-tiles\"", html);
        Assert.Contains("<ol class=\"bests\"", html);
        Assert.Contains(">15,410</span>", html);
    }
}
