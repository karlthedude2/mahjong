using Mahjong.Web.Client.Game;

namespace Mahjong.Web.Tests;

/// <summary>Flat tile sets for Mahjong Solitaire and Connect, stones for Mahjong 3D (and the cube).</summary>
public sealed class TileSetsTests
{
    [Fact]
    public void FlatSetsAndStonesAreSeparateLists()
    {
        Assert.Equal(["classic", "simple"], TileSets.Flat.Select(s => s.Id));
        Assert.Equal(["ivory", "onyx", "beige", "squareMarble"], TileSets.Stones.Select(s => s.Stone));
        Assert.All(TileSets.Flat, s => Assert.NotEqual(TileSetKind.ThreeD, s.Kind));
        Assert.All(TileSets.Stones, s => Assert.Equal(TileSetKind.ThreeD, s.Kind));
        Assert.Equal(TileSets.Flat.Concat(TileSets.Stones), TileSets.All);
    }

    [Fact]
    public void ARetiredStoneFallsBackToTheDefaultStone()
    {
        Assert.Same(TileSets.DefaultStone, TileSets.Find("3d-dark-marble"));
        Assert.Equal(TileSetKind.ThreeD, TileSets.DefaultStone.Kind);
    }

    [Fact]
    public void AnUnknownSetFallsBackToTheDefaultFlatSet()
    {
        Assert.Same(TileSets.Default, TileSets.Find("no-such-set"));
        Assert.Same(TileSets.Default, TileSets.Find(null));
    }
}
