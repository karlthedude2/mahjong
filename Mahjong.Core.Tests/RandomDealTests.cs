using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Mahjong.Core;
using Xunit;

namespace Mahjong.Core.Tests
{
    public class RandomDealTests
    {
        private static readonly LayoutDefinition TwinPeaks = LayoutCatalog.Find("Twin Peaks");
        private static readonly LayoutDefinition Test = LayoutCatalog.Find("Test");

        private static string Faces(MahjongGame game) => string.Join(",", game.Board.Tiles.OrderBy(t => t.Id).Select(t => t.Face.Name));

        [Fact]
        public void ARandomDealIsRepeatableFromItsSeedAndDiffersFromTheWinnableDeal()
        {
            var a = new MahjongGame(TwinPeaks, 777, winnable: false);
            var b = new MahjongGame(TwinPeaks, 777, winnable: false);
            var winnable = new MahjongGame(TwinPeaks, 777);

            Assert.Equal(Faces(a), Faces(b));
            Assert.NotEqual(Faces(a), Faces(winnable));
            Assert.False(a.Winnable);
            Assert.False(a.Record.Winnable);
            Assert.Empty(a.Board.LastDeal.Solution);
        }

        [Fact]
        public void ARandomDealUsesTheSameTilesAsAWinnableOne()
        {
            var random = new MahjongGame(TwinPeaks, 42, winnable: false);

            Assert.Equal(TwinPeaks.Positions.Count, random.Board.Count);
            Assert.All(random.Board.Tiles.GroupBy(t => t.Face.Name), group => Assert.Equal(0, group.Count() % 2));
        }

        [Fact]
        public void ARandomGameReplaysExactlyIncludingShuffles()
        {
            var (game, _) = FindSolvableRandomGame();

            var replay = GameReplayer.Replay(game.Record);

            Assert.True(replay.Valid, replay.Error);
            Assert.True(replay.Complete);
            Assert.Equal(game.Scoring.Card.Score, replay.Score);
        }

        [Fact]
        public void ReplayingARandomRecordAsWinnableFails()
        {
            var (game, _) = FindSolvableRandomGame();
            game.Record.Winnable = true;

            var replay = GameReplayer.Replay(game.Record);

            Assert.False(replay.Valid && replay.Complete);
        }

        [Fact]
        public void RecordsFromBeforeRandomDealsCountAsWinnable()
        {
            var record = JsonSerializer.Deserialize<GameRecord>("{\"LayoutName\":\"Test\",\"Seed\":5,\"Moves\":[]}");
            Assert.True(record.Winnable);
        }

        /// <summary>
        /// A random deal of the small Test layout that can be cleared, played to the end: a shuffle
        /// first (to check reshuffles replay too), then a search for a way to clear the board.
        /// </summary>
        private static (MahjongGame Game, long Seed) FindSolvableRandomGame()
        {
            for (long seed = 1; seed < 500; seed++)
            {
                var game = new MahjongGame(Test, seed, winnable: false);
                game.Tick();
                game.Shuffle();
                if (Solve(game, new HashSet<string>()))
                {
                    game.Finish();
                    return (game, seed);
                }
            }

            throw new Xunit.Sdk.XunitException("No solvable random deal found.");
        }

        private static bool Solve(MahjongGame game, HashSet<string> deadEnds)
        {
            if (game.Board.IsComplete)
            {
                return true;
            }

            string state = string.Join(",", game.Board.Tiles.Select(t => t.Id).OrderBy(id => id));
            if (deadEnds.Contains(state))
            {
                return false;
            }

            var free = game.Board.Tiles.Where(game.Board.IsFree).ToList();
            for (int i = 0; i < free.Count; i++)
            {
                for (int j = i + 1; j < free.Count; j++)
                {
                    game.Tick();
                    if (game.TryRemovePair(free[i], free[j]))
                    {
                        if (Solve(game, deadEnds))
                        {
                            return true;
                        }

                        game.Undo();
                    }
                }
            }

            deadEnds.Add(state);
            return false;
        }
    }
}
