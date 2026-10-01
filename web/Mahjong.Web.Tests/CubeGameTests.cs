using Mahjong.Web.Client.Game;

namespace Mahjong.Web.Tests;

/// <summary>The Mahjong Cube prototype's rules.</summary>
public sealed class CubeGameTests
{
    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    public void DealsAFullBlockOfPairs(int size)
    {
        var game = CubeGame.Deal(size, new Random(size));

        Assert.Equal(size * size * size, game.Tiles.Count);
        Assert.All(game.Tiles.GroupBy(t => t.Face.Name), faces => Assert.True(faces.Count() % 2 == 0));
    }

    [Fact]
    public void OnlyCornersAndEdgesStartFree()
    {
        var game = CubeGame.Deal(4, new Random(1));

        // 8 corners and 12 edges of 2 tiles each; the middles of the faces have one open side only.
        var free = game.Tiles.Where(game.IsFree).ToList();
        Assert.Equal(8 + 12 * 2, free.Count);
        Assert.All(free, t => Assert.True(new[] { t.X, t.Y, t.Z }.Count(c => c is 0 or 3) >= 2));
    }

    [Fact]
    public void RemovesOnlyFreeMatchingPairs()
    {
        var game = CubeGame.Deal(4, new Random(2));
        var (first, second) = game.FindPair()!.Value;
        var blocked = game.Tiles.First(t => !game.IsFree(t));

        Assert.False(game.TryRemovePair(first, blocked));
        Assert.True(game.TryRemovePair(first, second));
        Assert.False(game.TryRemovePair(first, second));
        Assert.Equal(62, game.Tiles.Count);
    }
}
