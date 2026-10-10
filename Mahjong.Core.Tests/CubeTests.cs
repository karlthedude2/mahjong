using System.Collections.Generic;
using System.Linq;
using Mahjong.Core;
using Xunit;

namespace Mahjong.Core.Tests
{
    public class CubeTests
    {
        private static readonly LayoutDefinition Small = LayoutCatalog.Find("Cube Small");

        private static HashSet<Position> Block(int size) => new HashSet<Position>(CubeLayouts.ForSize(size).Positions);

        private static void PlaySolution(MahjongGame game)
        {
            foreach (var (first, second) in game.Board.LastDeal.Solution)
            {
                game.Tick();
                Assert.True(game.TryRemovePair(game.Board.At(first), game.Board.At(second)), $"{first} + {second}");
            }
        }

        [Fact]
        public void TheBlocksAreSmallAndLargeCubes()
        {
            Assert.Equal(new[] { "Cube Small", "Cube Large" }, LayoutCatalog.Cube.Select(l => l.Name));
            Assert.Equal(new[] { 64, 216 }, LayoutCatalog.Cube.Select(l => l.Positions.Count));
            Assert.Equal(new[] { 4, 6 }, LayoutCatalog.Cube.Select(l => l.Size));
            Assert.All(LayoutCatalog.Cube, l => Assert.Equal(GameKind.Cube, l.Kind));
            Assert.Same(LayoutCatalog.Cube[1], LayoutCatalog.Find("cube large"));
            Assert.Same(LayoutCatalog.Cube[1], CubeLayouts.ForSize(6));
            Assert.Same(LayoutCatalog.Cube[0], CubeLayouts.ForSize(5));
            Assert.DoesNotContain(LayoutCatalog.All, l => l.Kind == GameKind.Cube);
        }

        [Fact]
        public void OnlyCornersAndEdgesOfAFullBlockAreFree()
        {
            var full = Block(4);

            // 8 corners and 12 edges of 2 tiles each; the middles of the faces have one open side only.
            Assert.Equal(8 + 12 * 2, full.Count(p => CubeRules.IsFree(p, full)));
            Assert.True(CubeRules.IsFree(new Position(0, 0, 0), full));
            Assert.True(CubeRules.IsFree(new Position(1, 0, 0), full));
            Assert.False(CubeRules.IsFree(new Position(0, 1, 1), full));
            Assert.False(CubeRules.IsFree(new Position(1, 1, 1), full));
        }

        [Fact]
        public void OpenSidesOnOneAxisOnlyDontFreeATile()
        {
            // (1,1,1) with its left and right neighbours gone: open on one axis, covered on the others.
            var block = Block(4);
            block.Remove(new Position(0, 1, 1));
            block.Remove(new Position(2, 1, 1));
            Assert.False(CubeRules.IsFree(new Position(1, 1, 1), block));

            // Open below as well: two sides that meet at an edge.
            block.Remove(new Position(1, 0, 1));
            Assert.True(CubeRules.IsFree(new Position(1, 1, 1), block));
        }

        [Fact]
        public void TheBoardUsesTheCubesFreeRule()
        {
            var game = new MahjongGame(Small, 5);

            Assert.Equal(64, game.Board.Count);
            Assert.Equal(8 + 12 * 2, game.Board.Tiles.Count(game.Board.IsFree));
            Assert.NotNull(game.Board.FindMove());
        }

        [Theory]
        [InlineData("Cube Small", 1)]
        [InlineData("Cube Small", 2)]
        [InlineData("Cube Small", 3)]
        [InlineData("Cube Large", 1)]
        [InlineData("Cube Large", 2)]
        public void AWinnableDealClearsByTheBoardsOwnRules(string name, int seed)
        {
            var game = new MahjongGame(LayoutCatalog.Find(name), seed);

            PlaySolution(game);

            Assert.True(game.Board.IsComplete, name);
        }

        [Fact]
        public void TheSameSeedDealsTheSameCube()
        {
            string Faces(MahjongGame g) => string.Join(",", g.Board.Tiles.OrderBy(t => t.Id).Select(t => t.Face.Name));

            Assert.Equal(Faces(new MahjongGame(Small, 42)), Faces(new MahjongGame(Small, 42)));
            Assert.NotEqual(Faces(new MahjongGame(Small, 42)), Faces(new MahjongGame(Small, 43)));
        }

        [Fact]
        public void AShuffleMidGameCanStillBeCleared()
        {
            var game = new MahjongGame(CubeLayouts.ForSize(6), 7);
            var solution = game.Board.LastDeal.Solution;
            for (int i = 0; i < 10; i++)
            {
                Assert.True(game.TryRemovePair(game.Board.At(solution[i].First), game.Board.At(solution[i].Second)));
            }

            game.Shuffle();
            Assert.Equal(216 - 20, game.Board.Count);
            PlaySolution(game);

            Assert.True(game.Board.IsComplete);
        }

        [Fact]
        public void ARandomDealFillsTheBlockWithPairs()
        {
            var game = new MahjongGame(Small, 3, winnable: false);

            Assert.Equal(64, game.Board.Count);
            Assert.Empty(game.Board.LastDeal.Solution);
            Assert.All(game.Board.Tiles.GroupBy(t => t.Face.Name), g => Assert.Equal(0, g.Count() % 2));
        }

        [Fact]
        public void ACubeGameWithAnUndoAHintAndAShuffleReplaysToTheSameScore()
        {
            var game = new MahjongGame(Small, 2026);
            var solution = game.Board.LastDeal.Solution;
            for (int i = 0; i < 4; i++)
            {
                game.Tick();
                game.Tick();
                Assert.True(game.TryRemovePair(game.Board.At(solution[i].First), game.Board.At(solution[i].Second)));
            }

            game.Tick();
            Assert.True(game.Undo());
            Assert.NotNull(game.Hint());
            game.Shuffle();
            PlaySolution(game);
            game.Finish();

            var result = GameReplayer.Replay(game.Record);

            Assert.True(result.Valid, result.Error);
            Assert.True(result.Complete);
            Assert.Equal(game.Scoring.Card.Score, result.Score);
        }
    }
}
