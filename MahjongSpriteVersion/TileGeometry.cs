using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Mahjong.Core;

namespace MahjongSpriteVersion
{
    /// <summary>Converts board positions to screen pixels.</summary>
    public static class TileGeometry
    {
        // One board unit is half a tile. The tile images overlap their neighbours by a few pixels
        // (the 3D edge), and each layer is shifted up and left to look stacked.
        private const int XStep = 42 - 3;
        private const int YStep = 57 - 3;
        private const int LayerShift = 11;

        public const int Width = 88;
        public const int Height = 118;

        public static Point Location(Position p)
        {
            return new Point(p.X * XStep - p.Z * LayerShift, p.Y * YStep - p.Z * LayerShift);
        }

        public static Rectangle Bounds(Position p)
        {
            return new Rectangle(Location(p), new Size(Width, Height));
        }

        /// <summary>The area covered by all of the given tiles.</summary>
        public static Rectangle Bounds(IEnumerable<Position> positions)
        {
            return positions.Select(Bounds).Aggregate(Rectangle.Union);
        }
    }
}
