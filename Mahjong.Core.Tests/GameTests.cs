using System;
using System.Linq;
using Mahjong.Core;
using Xunit;

namespace Mahjong.Core.Tests
{
    public class GameTests
    {
        private static MahjongGame NewGame(int seed = 1) =>
            new MahjongGame(LayoutCatalog.Find("Twin Peaks"), random: new Random(seed));

        private static (Tile, Tile) FirstRemovablePair(MahjongGame game)
        {
            var free = game.Board.Tiles.Where(game.Board.IsFree).ToList();
            return free.SelectMany(a => free.Where(b => game.Board.CanRemove(a, b)).Select(b => (a, b))).First();
        }

        [Fact]
        public void DealUsesExactlyOnePairPerTwoPositions()
        {
            var game = NewGame();
            Assert.Equal(144, game.Board.Count);
            Assert.All(game.Board.Tiles.GroupBy(t => t.Face.Name), g => Assert.True(g.Count() % 2 == 0));
        }

        [Fact]
        public void RemovingAPairScoresAndUndoRedoRoundTrips()
        {
            var game = NewGame();
            var (a, b) = FirstRemovablePair(game);

            Assert.True(game.TryRemovePair(a, b));
            int scoreAfter = game.Scoring.Card.Score;
            Assert.Equal(a.Face.Value + b.Face.Value, scoreAfter);
            Assert.Equal(142, game.Board.Count);

            Assert.True(game.Undo());
            Assert.Equal(0, game.Scoring.Card.Score);
            Assert.Equal(0, game.Scoring.Card.TilesTowardPaceBonus);
            Assert.Equal(144, game.Board.Count);
            Assert.True(game.Board.Contains(a) && game.Board.Contains(b));

            Assert.True(game.Redo());
            Assert.Equal(scoreAfter, game.Scoring.Card.Score);
            Assert.Equal(142, game.Board.Count);
            Assert.False(game.Redo());
        }

        [Fact]
        public void CannotRemoveBlockedOrMismatchedTiles()
        {
            var game = NewGame();
            var blocked = game.Board.Tiles.First(t => !game.Board.IsFree(t));
            var twin = game.Board.Tiles.First(t => t != blocked && t.Face.Matches(blocked.Face));
            Assert.False(game.TryRemovePair(blocked, twin));

            var free = game.Board.Tiles.Where(game.Board.IsFree).ToList();
            var a = free[0];
            var mismatch = free.FirstOrDefault(t => !t.Face.Matches(a.Face));
            if (mismatch != null)
            {
                Assert.False(game.TryRemovePair(a, mismatch));
            }

            Assert.False(game.TryRemovePair(a, a));
        }

        [Fact]
        public void ShuffleKeepsTheBoardSolvableAndCanBeUndone()
        {
            for (int seed = 0; seed < 200; seed++)
            {
                var game = NewGame(seed);
                for (int i = 0; i < 10; i++)
                {
                    var (a, b) = FirstRemovablePair(game);
                    game.TryRemovePair(a, b);
                }

                var facesBefore = game.Board.Tiles.ToDictionary(t => t, t => t.Face.Name);
                var countsBefore = facesBefore.Values.GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());

                game.Shuffle();

                var countsAfter = game.Board.Tiles.GroupBy(t => t.Face.Name).ToDictionary(g => g.Key, g => g.Count());
                Assert.Equal(countsBefore.OrderBy(kv => kv.Key), countsAfter.OrderBy(kv => kv.Key));
                Assert.Equal(2000, game.Scoring.Card.NoShuffleBonus);

                game.Undo();
                Assert.All(game.Board.Tiles, t => Assert.Equal(facesBefore[t], t.Face.Name));
                Assert.Equal(3000, game.Scoring.Card.NoShuffleBonus);

                game.Redo();
                LayoutTests.Solve(game);
                Assert.Equal(GameStatus.Complete, game.Status);
            }
        }

        [Fact]
        public void SpeedBonusIsPaidForTwentyTilesInsideTheWindow()
        {
            var game = NewGame();
            for (int i = 0; i < 10; i++)
            {
                var (a, b) = FirstRemovablePair(game);
                game.TryRemovePair(a, b);
            }

            Assert.Equal(1, game.Scoring.Card.SpeedBonusCount);
            Assert.Equal(3000, game.Scoring.Card.SpeedBonusTotal);

            game.Undo();
            Assert.Equal(0, game.Scoring.Card.SpeedBonusCount);
            game.Redo();
            Assert.Equal(1, game.Scoring.Card.SpeedBonusCount);
        }

        [Fact]
        public void ClockTextFormatsHours()
        {
            var game = NewGame();
            for (int i = 0; i < 3725; i++)
            {
                game.Tick();
            }

            Assert.Equal("1:02:05", game.Scoring.Clock.ClockText);
        }
    }
}
