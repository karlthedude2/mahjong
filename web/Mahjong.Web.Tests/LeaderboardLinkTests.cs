using Mahjong.Web.Client.Game;

namespace Mahjong.Web.Tests;

/// <summary>Which links on the game page open the leaderboard dialog instead of leaving the game.</summary>
public sealed class LeaderboardLinkTests
{
    private static readonly Guid Game = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e");

    [Fact]
    public void ALeaderboardLinkKeepsItsLayout()
    {
        Assert.Equal(new LeaderboardLink(LeaderboardLinkKind.Leaderboard, Layout: "Cube Small"), LeaderboardLink.Parse("leaderboards?layout=Cube%20Small"));
    }

    [Fact]
    public void ALeaderboardLinkWithoutALayoutHasNone()
    {
        Assert.Equal(new LeaderboardLink(LeaderboardLinkKind.Leaderboard), LeaderboardLink.Parse("leaderboards"));
    }

    [Fact]
    public void AReplayListLinkHasItsGame()
    {
        Assert.Equal(new LeaderboardLink(LeaderboardLinkKind.Replays, GameId: Game), LeaderboardLink.Parse($"replays/{Game}"));
    }

    [Fact]
    public void APlayThisDealLinkHasItsGame()
    {
        Assert.Equal(new LeaderboardLink(LeaderboardLinkKind.PlayDeal, GameId: Game), LeaderboardLink.Parse($"?replay={Game}"));
    }

    [Theory]
    [InlineData("replays/not-a-game")]
    [InlineData("?replay=not-a-game")]
    [InlineData("Account/Login")]
    [InlineData("privacy")]
    [InlineData("")]
    public void OtherLinksAreLeftAlone(string link)
    {
        Assert.Null(LeaderboardLink.Parse(link));
    }
}
