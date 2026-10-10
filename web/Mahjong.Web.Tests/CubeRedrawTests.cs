using Mahjong.Web.Client.Game;

namespace Mahjong.Web.Tests;

/// <summary>How the cube view catches up with the game: shrink away a taken pair, or draw the block again.</summary>
public sealed class CubeRedrawTests
{
    private static readonly Dictionary<int, string> Drawn = new() { [1] = "1dot", [2] = "1dot", [3] = "2bam", [4] = "2bam" };

    [Fact]
    public void NothingChangedNeedsNothing()
    {
        var (redraw, removed) = CubeRedraw.Compare(Drawn, new Dictionary<int, string>(Drawn));

        Assert.False(redraw);
        Assert.Empty(removed);
    }

    [Fact]
    public void ATakenPairShrinksAway()
    {
        var (redraw, removed) = CubeRedraw.Compare(Drawn, new Dictionary<int, string> { [3] = "2bam", [4] = "2bam" });

        Assert.False(redraw);
        Assert.Equal(new[] { 1, 2 }, removed.Order());
    }

    [Fact]
    public void AnUndoBringsTilesBackSoTheBlockIsRedrawn()
    {
        var afterTaking = new Dictionary<int, string> { [3] = "2bam", [4] = "2bam" };
        var (redraw, removed) = CubeRedraw.Compare(afterTaking, Drawn);

        Assert.True(redraw);
        Assert.Empty(removed);
    }

    [Fact]
    public void AShuffleChangesFacesSoTheBlockIsRedrawn()
    {
        var shuffled = new Dictionary<int, string> { [1] = "2bam", [2] = "1dot", [3] = "1dot", [4] = "2bam" };
        var (redraw, removed) = CubeRedraw.Compare(Drawn, shuffled);

        Assert.True(redraw);
        Assert.Empty(removed);
    }

    [Fact]
    public void TheFirstDrawIsARedraw()
    {
        var (redraw, removed) = CubeRedraw.Compare(new Dictionary<int, string>(), Drawn);

        Assert.True(redraw);
        Assert.Empty(removed);
    }
}
