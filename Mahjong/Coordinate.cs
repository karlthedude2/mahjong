using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahjong
{
    public class Coordinate
    {
        private const int xOffset = 21;
        private const int yOffset = 27;
        private const int zOffset = 5;

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public Coordinate(int x, int y, int z) {
            X = x;
            Y = y;
            Z = z;
        }

        public int GetPixelX() {
            int x = X * xOffset - Z * zOffset - (X == 0 ? 0 : 3 * X );
            return x;
        }

        public int GetPixelY() {
            return Y * yOffset - Z * zOffset -(Y == 0 ? 0 : 3 * Y);
        }
    }
}
