using System.Collections.Generic;
using System.Linq;
using Mahjong.Core;
using Xunit;

namespace Mahjong.Core.Tests
{
    public class ConnectTests
    {
        private static readonly LayoutDefinition Small = LayoutCatalog.Find("Connect Small");

        private static Position P(int column, int row) => new Cell(column, row).ToPosition();

        private static IReadOnlyList<Cell> Path(IEnumerable<Position> occupied, Position a, Position b) =>
            ConnectRules.FindPath(Small, new HashSet<Position>(occupied), a, b);

        [Fact]
        public void TheBoardsAreThreeSizesEachWithThreeGravitySettings()
        {
            Assert.Equal(9, LayoutCatalog.Connect.Count);
            Assert.Equal(new[] { 72, 112, 144 }, ConnectLayouts.BaseBoards.Select(l => l.Positions.Count));
            Assert.Equal("Connect Classic (Gravity Down)", LayoutCatalog.Find("Connect Classic (Gravity Down)").Name);
            Assert.All(LayoutCatalog.Connect, l => Assert.Equal(GameKind.Connect, l.Kind));
            Assert.DoesNotContain(LayoutCatalog.All, l => l.Kind == GameKind.Connect);
        }

        [Fact]
        public void NeighboursAndClearLinesConnect()
        {
            Assert.Equal(2, Path(new[] { P(3, 2), P(4, 2) }, P(3, 2), P(4, 2)).Count);
            Assert.Equal(2, Path(new[] { P(3, 2), P(8, 2) }, P(3, 2), P(8, 2)).Count);
        }

        [Fact]
        public void OneOrTwoTurnsConnectButNotThree()
        {
            // One turn: (2,1) to (5,4) around the empty corner.
            Assert.Equal(3, Path(new[] { P(2, 1), P(5, 4) }, P(2, 1), P(5, 4)).Count);

            // Two turns: a wall in between forces a detour.
            var wall = Enumerable.Range(0, 6).Select(r => P(4, r)).Where(p => p != P(4, 0)).ToList();
            var twoTurns = Path(wall.Concat(new[] { P(2, 3), P(6, 3) }), P(2, 3), P(6, 3));
            Assert.Equal(4, twoTurns.Count);
            Assert.Equal(new Cell(4, 0).Row, twoTurns[1].Row);

            // Boxed in on every side: no line at all.
            var boxed = new[] { P(5, 2), P(4, 2), P(6, 2), P(5, 1), P(5, 3), P(9, 4) };
            Assert.Null(Path(boxed, P(5, 2), P(9, 4)));
        }

        [Fact]
        public void TheLineMayRunAroundTheOutsideOfTheBoard()
        {
            // A full board: two tiles on the top edge connect through the lane above it.
            var full = Small.Positions;
            var path = Path(full, P(0, 0), P(11, 0));
            Assert.NotNull(path);
            Assert.Equal(-1, path[1].Row);

            // ...but two tiles in the middle of a full board don't.
            Assert.Null(Path(full, P(3, 2), P(7, 3)));
        }

        [Fact]
        public void EveryConnectBoardDealsAWinnableGame()
        {
            foreach (var layout in LayoutCatalog.Connect)
            {
                var game = new MahjongGame(layout, 99);
                Assert.Equal(layout.Positions.Count, game.Board.Count);

                foreach (var (first, second) in game.Board.LastDeal.Solution)
                {
                    Assert.True(game.TryRemovePair(game.Board.At(first), game.Board.At(second)), $"{layout.Name}: {first} + {second}");
                }

                Assert.True(game.Board.IsComplete, layout.Name);
            }
        }

        [Fact]
        public void GravityDownDropsTilesIntoTheGapAndUndoPutsThemBack()
        {
            var layout = LayoutCatalog.Find("Connect Small (Gravity Down)");
            var game = new MahjongGame(layout, 7);
            var (first, second) = game.Board.LastDeal.Solution[0];
            var a = game.Board.At(first);
            var b = game.Board.At(second);
            var before = game.Board.Tiles.ToDictionary(t => t.Id, t => t.Position);

            Assert.True(game.TryRemovePair(a, b));

            // Every column is still packed to the bottom.
            foreach (var column in game.Board.Tiles.GroupBy(t => Cell.Of(t.Position).Column))
            {
                var rows = column.Select(t => Cell.Of(t.Position).Row).OrderByDescending(r => r).ToList();
                Assert.Equal(Enumerable.Range(0, rows.Count).Select(i => layout.Rows - 1 - i), rows);
            }

            Assert.True(game.Undo());
            Assert.Equal(before, game.Board.Tiles.ToDictionary(t => t.Id, t => t.Position));
        }

        [Fact]
        public void AHintFindsAMoveAndCostsPartOfTheNoShuffleBonusEvenAfterUndo()
        {
            var game = new MahjongGame(Small, 3);
            var (first, second) = game.Board.LastDeal.Solution[0];
            game.TryRemovePair(game.Board.At(first), game.Board.At(second));

            var hint = game.Hint();

            Assert.NotNull(hint);
            Assert.True(game.Board.CanRemove(hint.Value.First, hint.Value.Second));
            Assert.Equal(ScoreCard.NoShuffleBonusStart - Scoring.HintPenalty, game.Scoring.NoShuffleBonusNow);

            game.Undo();
            Assert.Equal(ScoreCard.NoShuffleBonusStart - Scoring.HintPenalty, game.Scoring.NoShuffleBonusNow);
        }

        [Fact]
        public void AConnectGameWithGravityShufflesAndHintsReplaysExactly()
        {
            var layout = LayoutCatalog.Find("Connect Medium (Gravity Left)");
            var game = new MahjongGame(layout, 2024);
            game.Tick();
            game.Shuffle();
            game.Hint();
            foreach (var (first, second) in game.Board.LastDeal.Solution)
            {
                game.Tick();
                game.TryRemovePair(game.Board.At(first), game.Board.At(second));
            }

            game.Finish();
            var replay = GameReplayer.Replay(game.Record);

            Assert.True(game.Board.IsComplete);
            Assert.True(replay.Valid, replay.Error);
            Assert.True(replay.Complete);
            Assert.Equal(game.Scoring.Card.Score, replay.Score);
            Assert.Equal(ScoreCard.NoShuffleBonusStart - Scoring.ShufflePenalty - Scoring.HintPenalty, replay.Breakdown.NoShuffleBonus);
        }

        [Fact]
        public void ARandomConnectDealIsJustShuffledTiles()
        {
            var game = new MahjongGame(Small, 5, winnable: false);

            Assert.Equal(72, game.Board.Count);
            Assert.Empty(game.Board.LastDeal.Solution);
            Assert.All(game.Board.Tiles.GroupBy(t => t.Face.Name), g => Assert.Equal(0, g.Count() % 2));
        }
    }
}
