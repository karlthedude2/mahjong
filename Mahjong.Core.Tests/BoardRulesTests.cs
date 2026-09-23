using System.Collections.Generic;
using Mahjong.Core;
using Xunit;

namespace Mahjong.Core.Tests
{
    public class BoardRulesTests
    {
        private static bool IsFree(Position p, params Position[] others)
        {
            var occupied = new HashSet<Position>(others) { p };
            return BoardRules.IsFree(p, occupied);
        }

        [Fact]
        public void LoneTileIsFree()
        {
            Assert.True(IsFree(new Position(4, 4, 0)));
        }

        [Fact]
        public void TileWithNeighbourOnOneSideIsFree()
        {
            Assert.True(IsFree(new Position(4, 4, 0), new Position(2, 4, 0)));
            Assert.True(IsFree(new Position(4, 4, 0), new Position(6, 4, 0)));
        }

        [Fact]
        public void TileWithNeighboursOnBothSidesIsBlocked()
        {
            Assert.False(IsFree(new Position(4, 4, 0), new Position(2, 4, 0), new Position(6, 4, 0)));
        }

        [Fact]
        public void HalfOffsetSideNeighboursBlock()
        {
            Assert.False(IsFree(new Position(4, 4, 0), new Position(2, 3, 0), new Position(6, 5, 0)));
        }

        [Fact]
        public void NeighboursOnOtherRowsDoNotBlock()
        {
            Assert.True(IsFree(new Position(4, 4, 0), new Position(2, 2, 0), new Position(6, 6, 0)));
        }

        [Theory]
        [InlineData(-1, -1)]
        [InlineData(0, -1)] // the old code never checked this one
        [InlineData(1, -1)]
        [InlineData(-1, 0)]
        [InlineData(0, 0)]
        [InlineData(1, 0)]
        [InlineData(-1, 1)]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        public void AnyOverlappingTileAboveBlocks(int dx, int dy)
        {
            Assert.False(IsFree(new Position(4, 4, 0), new Position(4 + dx, 4 + dy, 1)));
        }

        [Fact]
        public void TileAboveButNotOverlappingDoesNotBlock()
        {
            Assert.True(IsFree(new Position(4, 4, 0), new Position(6, 4, 1)));
            Assert.True(IsFree(new Position(4, 4, 0), new Position(4, 6, 1)));
        }

        [Fact]
        public void TileTwoLayersUpDoesNotBlockDirectly()
        {
            Assert.True(IsFree(new Position(4, 4, 0), new Position(4, 4, 2)));
        }
    }
}
