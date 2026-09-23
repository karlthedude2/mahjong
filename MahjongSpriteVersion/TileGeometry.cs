using System.Collections.Generic;
using System.Drawing;
using Mahjong.Core;

namespace MahjongSpriteVersion
{
    /// <summary>System.Drawing versions of the shared <see cref="BoardGeometry"/>.</summary>
    public static class TileGeometry
    {
        public static Point Location(Position p)
        {
            var bounds = BoardGeometry.Bounds(p);
            return new Point(bounds.Left, bounds.Top);
        }

        public static Rectangle Bounds(Position p) => ToRectangle(BoardGeometry.Bounds(p));

        /// <summary>The area covered by all of the given tiles.</summary>
        public static Rectangle Bounds(IEnumerable<Position> positions) => ToRectangle(BoardGeometry.Bounds(positions));

        private static Rectangle ToRectangle(PixelRect r) => new Rectangle(r.Left, r.Top, r.Width, r.Height);
    }
}
