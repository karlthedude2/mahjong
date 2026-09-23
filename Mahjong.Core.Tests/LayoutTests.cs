using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mahjong.Core;
using Xunit;

namespace Mahjong.Core.Tests
{
    public class LayoutTests
    {
        public static IEnumerable<object[]> LayoutNames => LayoutCatalog.All.Select(l => new object[] { l.Name });

        [Fact]
        public void BuiltInLayoutsAreLoaded()
        {
            Assert.Equal(144, LayoutCatalog.Find("Twin Peaks").Positions.Count);
            Assert.Equal(144, LayoutCatalog.Find("Temple").Positions.Count);
            Assert.Equal(24, LayoutCatalog.Find("Test").Positions.Count);
            Assert.Equal(144, LayoutCatalog.Find("Standard Turtle").Positions.Count);
        }

        [Theory]
        [MemberData(nameof(LayoutNames))]
        public void LayoutIsValid(string name)
        {
            Assert.Empty(LayoutCatalog.Find(name).Validate());
        }

        [Theory]
        [MemberData(nameof(LayoutNames))]
        public void EveryDealCanBeCleared(string name)
        {
            var layout = LayoutCatalog.Find(name);
            for (int seed = 0; seed < 1000; seed++)
            {
                var game = new MahjongGame(layout, random: new Random(seed));
                Solve(game);
                Assert.Equal(GameStatus.Complete, game.Status);
            }
        }

        [Fact]
        public void ParserReadsNameCommentsAndPositions()
        {
            var layout = LayoutParser.Parse("# a comment\nname: Tiny\n\n0 0 0\n2,0,0\n  4\t0 0  \n");
            Assert.Equal("Tiny", layout.Name);
            Assert.Equal(new[] { new Position(0, 0, 0), new Position(2, 0, 0), new Position(4, 0, 0) }, layout.Positions);
        }

        [Fact]
        public void ParserReadsHiddenFlag()
        {
            Assert.True(LayoutParser.Parse("name: Secret\nhidden: true\n0 0 0\n2 0 0\n").Hidden);
            Assert.False(LayoutParser.Parse("name: Plain\n0 0 0\n2 0 0\n").Hidden);
            Assert.Throws<FormatException>(() => LayoutParser.Parse("name: Bad\nhidden: maybe\n"));
        }

        [Fact]
        public void TestLayoutIsHiddenFromTheMenu()
        {
            Assert.True(LayoutCatalog.Find("Test").Hidden);
            Assert.DoesNotContain(LayoutCatalog.Visible, l => l.Name == "Test");
            Assert.Contains(LayoutCatalog.Visible, l => l.Name == "Twin Peaks");
        }

        [Fact]
        public void ParserRejectsBadLines()
        {
            var ex = Assert.Throws<FormatException>(() => LayoutParser.Parse("name: Bad\n0 0\n"));
            Assert.Contains("line 2", ex.Message);
        }

        [Fact]
        public void ParserRequiresName()
        {
            Assert.Throws<FormatException>(() => LayoutParser.Parse("0 0 0\n2 0 0\n"));
        }

        [Fact]
        public void ValidateReportsOddCountDuplicatesAndOverlaps()
        {
            var errors = LayoutParser.Parse("name: Broken\n0 0 0\n0 0 0\n1 0 0\n").Validate();
            Assert.Contains(errors, e => e.Contains("even"));
            Assert.Contains(errors, e => e.Contains("listed 2 times"));
            Assert.Contains(errors, e => e.Contains("overlap"));
        }

        [Fact]
        public void ValidateReportsUnclearableLayout()
        {
            // A single stack: only the top tile is ever free, so no pair can be removed.
            var errors = LayoutParser.Parse("name: Tower\n0 0 0\n0 0 1\n").Validate();
            Assert.Contains(errors, e => e.Contains("No way to clear"));
        }

        [Fact]
        public void LayoutsCanBeAddedFromADirectory()
        {
            string dir = Path.Combine(Path.GetTempPath(), "mahjong-layouts-" + Guid.NewGuid());
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "row.layout"), "name: Zz Row\n0 0 0\n2 0 0\n4 0 0\n6 0 0\n");
                File.WriteAllText(Path.Combine(dir, "bad.layout"), "0 0 0\n");

                var problems = LayoutCatalog.AddFromDirectory(dir);

                Assert.Single(problems);
                Assert.Equal(4, LayoutCatalog.Find("Zz Row").Positions.Count);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        internal static void Solve(MahjongGame game)
        {
            foreach (var (first, second) in game.Board.LastDeal.Solution)
            {
                var a = game.Board.At(first);
                var b = game.Board.At(second);
                Assert.True(game.TryRemovePair(a, b), $"Could not remove {a} and {b}.");
            }
        }
    }
}
