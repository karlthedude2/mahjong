using Mahjong.Web.Client.Game;

namespace Mahjong.Web.Tests;

/// <summary>When the game bar pops up a bonus badge: only when a speed or pace bonus total goes up.</summary>
public sealed class BonusWatchTests
{
    [Fact]
    public void TheFirstLookOnlyNotesTheTotals()
    {
        var watch = new BonusWatch();

        Assert.Empty(watch.Update(speedTotal: 2300, paceTotal: 218));
    }

    [Fact]
    public void NothingNewMeansNoBadge()
    {
        var watch = new BonusWatch();
        watch.Update(0, 0);

        Assert.Empty(watch.Update(0, 0));
    }

    [Fact]
    public void ASpeedBonusIsReportedWithItsPoints()
    {
        var watch = new BonusWatch();
        watch.Update(0, 0);

        Assert.Equal([new EarnedBonus(BonusKind.Speed, 2300)], watch.Update(2300, 0));
    }

    [Fact]
    public void APaceBonusIsReportedWithItsPoints()
    {
        var watch = new BonusWatch();
        watch.Update(2300, 0);

        Assert.Equal([new EarnedBonus(BonusKind.Pace, 218)], watch.Update(2300, 218));
    }

    [Fact]
    public void BothOnOneMoveComeSpeedFirst()
    {
        var watch = new BonusWatch();
        watch.Update(0, 0);

        Assert.Equal([new EarnedBonus(BonusKind.Speed, 1900), new EarnedBonus(BonusKind.Pace, 280)], watch.Update(1900, 280));
    }

    [Fact]
    public void AnUndoNeverShowsABadgeButEarningItAgainDoes()
    {
        var watch = new BonusWatch();
        watch.Update(0, 0);
        watch.Update(2300, 0);

        Assert.Empty(watch.Update(0, 0));
        Assert.Equal([new EarnedBonus(BonusKind.Speed, 2100)], watch.Update(2100, 0));
    }
}
