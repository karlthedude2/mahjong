using System.Collections.Generic;
using System.Linq;

namespace Mahjong.Core
{
    /// <summary>A rectangle in board pixels.</summary>
    public readonly struct PixelRect
    {
        public PixelRect(int left, int top, int width, int height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        public int Left { get; }
        public int Top { get; }
        public int Width { get; }
        public int Height { get; }
        public int Right => Left + Width;
        public int Bottom => Top + Height;

        public bool Contains(int x, int y) => x >= Left && x < Right && y >= Top && y < Bottom;
    }

    /// <summary>
    /// Where tiles go on screen, shared by the desktop and web versions. Units are pixels at full
    /// tile size; each front end scales as needed.
    /// </summary>
    public static class BoardGeometry
    {
        // One board unit is half a tile. Tile images overlap their neighbours by a few pixels
        // (the 3D edge), and each layer is shifted up and left to look stacked.
        public const int XStep = 39;
        public const int YStep = 54;
        public const int LayerShift = 11;

        public const int TileWidth = 88;
        public const int TileHeight = 118;

        public static PixelRect Bounds(Position p)
        {
            return new PixelRect(p.X * XStep - p.Z * LayerShift, p.Y * YStep - p.Z * LayerShift, TileWidth, TileHeight);
        }

        /// <summary>The area covered by all of the given tiles.</summary>
        public static PixelRect Bounds(IEnumerable<Position> positions)
        {
            var all = positions.Select(Bounds).ToList();
            int left = all.Min(b => b.Left);
            int top = all.Min(b => b.Top);
            return new PixelRect(left, top, all.Max(b => b.Right) - left, all.Max(b => b.Bottom) - top);
        }

        /// <summary>
        /// The order to draw tiles in so each covers only its neighbours' edges, never their faces.
        /// </summary>
        /// <remarks>
        /// Each tile image overlaps its right and lower neighbours' space only with its side and
        /// bottom edges, so a tile must be drawn after the tiles to its left and above it. Sorting
        /// by row alone gets half-row offsets wrong; X + Y orders every overlapping neighbour
        /// correctly, including the half-tile offsets. Layers are drawn bottom to top.
        /// </remarks>
        public static IEnumerable<Tile> InDrawOrder(IEnumerable<Tile> tiles)
        {
            return tiles
                .OrderBy(t => t.Position.Z)
                .ThenBy(t => t.Position.X + t.Position.Y)
                .ThenBy(t => t.Position.X);
        }
    }
}
