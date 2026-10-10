using System.Collections.Generic;
using System.Linq;

namespace Mahjong.Core
{
    /// <summary>Mahjong Cube's blocks: Small (4×4×4) and Large (6×6×6). Positions are whole cells.</summary>
    public static class CubeLayouts
    {
        public const string Prefix = "Cube";

        /// <summary>"Cube Small" and "Cube Large", in menu order.</summary>
        public static IReadOnlyList<LayoutDefinition> All { get; } = new[]
        {
            Create("Small", 4),
            Create("Large", 6),
        };

        /// <summary>The block with <paramref name="size"/> tiles along each edge; Small for any size it doesn't have.</summary>
        public static LayoutDefinition ForSize(int size) => All.FirstOrDefault(l => l.Size == size) ?? All[0];

        private static LayoutDefinition Create(string name, int size)
        {
            var cells = Enumerable.Range(0, size).SelectMany(x =>
                Enumerable.Range(0, size).SelectMany(y =>
                    Enumerable.Range(0, size).Select(z => new Position(x, y, z))));
            return new LayoutDefinition($"{Prefix} {name}", cells, size);
        }
    }

    /// <summary>Mahjong Cube's rule for which tiles can be taken.</summary>
    public static class CubeRules
    {
        private static readonly (int X, int Y, int Z)[] Axes = { (1, 0, 0), (0, 1, 0), (0, 0, 1) };

        /// <summary>
        /// Free when open along two different axes: one open side across (left or right) and one open
        /// side up or down, say, which is two sides meeting at an edge. Open sides on one axis only
        /// (left and right, but covered above, below, in front and behind) don't count.
        /// </summary>
        public static bool IsFree(Position p, ISet<Position> occupied)
        {
            int openAxes = 0;
            foreach (var (dx, dy, dz) in Axes)
            {
                if (!occupied.Contains(new Position(p.X + dx, p.Y + dy, p.Z + dz))
                    || !occupied.Contains(new Position(p.X - dx, p.Y - dy, p.Z - dz)))
                {
                    openAxes++;
                }
            }

            return openAxes >= 2;
        }
    }
}
