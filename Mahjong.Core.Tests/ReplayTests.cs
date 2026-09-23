using System.Linq;
using Mahjong.Core;
using Xunit;

namespace Mahjong.Core.Tests
{
    public class ReplayTests
    {
        private static readonly LayoutDefinition TwinPeaks = LayoutCatalog.Find("Twin Peaks");

        /// <summary>Plays the dealt solution, ticking the clock a little before each move.</summary>
        private static void PlaySolution(MahjongGame game, int secondsPerMove = 2)
        {
            foreach (var (first, second) in game.Board.LastDeal.Solution)
            {
                for (int i = 0; i < secondsPerMove; i++)
                {
                    game.Tick();
                }

                Assert.True(game.TryRemovePair(game.Board.At(first), game.Board.At(second)));
            }

            game.Finish();
        }

        [Fact]
        public void DeterministicRandomMatchesSplitMix64()
        {
            // Reference outputs of SplitMix64 for seed 0, shifted to 31 bits by Next().
            var random = new DeterministicRandom(0);
            Assert.Equal(new[] { 1896895516, 926699317, 56766092 }, new[] { random.Next(), random.Next(), random.Next() });
        }

        [Fact]
        public void SameSeedDealsTheSameBoard()
        {
            var a = new MahjongGame(TwinPeaks, 12345);
            var b = new MahjongGame(TwinPeaks, 12345);
            var c = new MahjongGame(TwinPeaks, 54321);

            string Faces(MahjongGame g) => string.Join(",", g.Board.Tiles.OrderBy(t => t.Id).Select(t => t.Face.Name));

            Assert.Equal(Faces(a), Faces(b));
            Assert.NotEqual(Faces(a), Faces(c));
        }

        [Fact]
        public void ReplayReproducesTheScore()
        {
            var game = new MahjongGame(TwinPeaks, 777);
            PlaySolution(game);

            var result = GameReplayer.Replay(game.Record);

            Assert.True(result.Valid, result.Error);
            Assert.True(result.Complete);
            Assert.Equal(game.Scoring.Card.Score, result.Score);
            Assert.Equal(game.Scoring.Clock.Seconds, result.ElapsedSeconds);
        }

        [Fact]
        public void ReplayHandlesShuffleUndoAndRedo()
        {
            var game = new MahjongGame(TwinPeaks, 2024);
            var solution = game.Board.LastDeal.Solution;
            for (int i = 0; i < 5; i++)
            {
                game.Tick();
                game.TryRemovePair(game.Board.At(solution[i].First), game.Board.At(solution[i].Second));
            }

            game.Tick();
            game.Shuffle();
            game.Shuffle();
            game.Undo();
            game.Redo();
            PlaySolution(game, secondsPerMove: 1);

            var result = GameReplayer.Replay(game.Record);

            Assert.True(result.Valid, result.Error);
            Assert.True(result.Complete);
            Assert.Equal(game.Scoring.Card.Score, result.Score);
            Assert.Equal(1000, result.Breakdown.NoShuffleBonus);
        }

        [Fact]
        public void BreakdownAddsUpToTheTotal()
        {
            foreach (int secondsPerMove in new[] { 0, 1, 3, 6 })
            {
                var game = new MahjongGame(TwinPeaks, 99 + secondsPerMove);
                PlaySolution(game, secondsPerMove);
                var b = game.Scoring.Card.Breakdown;

                Assert.Equal(b.Total,
                    b.TilePoints + b.SpeedBonusTotal + b.PaceBonusTotal + b.TimeBonus + b.QuickFinishBonus + b.NoShuffleBonus);
            }
        }

        [Fact]
        public void SlowGamesEarnNoFinalBonuses()
        {
            var game = new MahjongGame(TwinPeaks, 5);
            PlaySolution(game, secondsPerMove: 6); // 72 moves x 6s = 432s, past the 5-minute bonus clock

            var b = game.Scoring.Card.Breakdown;
            Assert.Equal(0, b.TimeBonus + b.QuickFinishBonus + b.NoShuffleBonus);
        }

        [Fact]
        public void FinishIsAppliedOnlyOnce()
        {
            var game = new MahjongGame(TwinPeaks, 8);
            PlaySolution(game);
            int score = game.Scoring.Card.Score;

            game.Finish();

            Assert.Equal(score, game.Scoring.Card.Score);
        }

        [Fact]
        public void TamperedRecordsAreRejected()
        {
            var game = new MahjongGame(TwinPeaks, 31337);
            PlaySolution(game);
            var record = game.Record;

            // A blocked tile removed first.
            var fresh = new MahjongGame(TwinPeaks, 31337);
            var blocked = fresh.Board.Tiles.First(t => !fresh.Board.IsFree(t));
            var illegal = Copy(record);
            illegal.Moves[0].TileA = blocked.Id;
            Assert.False(GameReplayer.Replay(illegal).Valid);

            // Moves that go back in time.
            var outOfOrder = Copy(record);
            outOfOrder.Moves[10].Second = 0;
            Assert.False(GameReplayer.Replay(outOfOrder).Valid);

            // A different seed deals a different board, so the recorded pairs don't match.
            var wrongSeed = Copy(record);
            wrongSeed.Seed = 1;
            Assert.False(GameReplayer.Replay(wrongSeed).Valid);

            // Unknown layout.
            var wrongLayout = Copy(record);
            wrongLayout.LayoutName = "Nope";
            Assert.False(GameReplayer.Replay(wrongLayout).Valid);

            // Extra moves after the board was cleared.
            var extra = Copy(record);
            extra.Moves.Add(new RecordedMove { Kind = RecordedMoveKind.Shuffle, Second = extra.Moves.Last().Second });
            Assert.False(GameReplayer.Replay(extra).Valid);
        }

        [Fact]
        public void UnfinishedGamesReplayButAreNotComplete()
        {
            var game = new MahjongGame(TwinPeaks, 3);
            var (first, second) = game.Board.LastDeal.Solution[0];
            game.TryRemovePair(game.Board.At(first), game.Board.At(second));

            var result = GameReplayer.Replay(game.Record);

            Assert.True(result.Valid);
            Assert.False(result.Complete);
        }

        private static GameRecord Copy(GameRecord record) => new GameRecord
        {
            LayoutName = record.LayoutName,
            Seed = record.Seed,
            Moves = record.Moves
                .Select(m => new RecordedMove { Kind = m.Kind, Second = m.Second, TileA = m.TileA, TileB = m.TileB })
                .ToList(),
        };
    }
}
