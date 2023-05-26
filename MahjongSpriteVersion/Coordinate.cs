using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MahjongSpriteVersion
{
    public class Coordinate
    {
        //private const int xOffset = 33; //21;
        //private const int yOffset = 43; //27;
        //private const int zOffset = 8; //5;
        //private const int Width = 66; //42;
        //private const int Height = 86; //54;

        private const int xOffset = 42;
        private const int yOffset = 57;
        private const int zOffset = 11;
        private const int Width = 88;
        private const int Height = 118;

        private Rectangle bounds;

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public string bitmapName { get; set; }

        public Coordinate(int x, int y, int z) {
            X = x;
            Y = y;
            Z = z;

            bounds = new Rectangle(new Point(GetPixelX(), GetPixelY()), new Size(new Point(Width, Height)));
        }

        public Rectangle Bounds {
            get {
                return bounds;
            }
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
