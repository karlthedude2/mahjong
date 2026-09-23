using System.Collections.Generic;

namespace Mahjong.Core
{
    /// <summary>
    /// The single definition of which tiles can be played. Everything else (clicking, dealing,
    /// shuffling, dead-end detection) goes through here.
    /// </summary>
    public static class BoardRules
    {
        /// <summary>
        /// A tile is free when nothing on the layer above overlaps it and at least one of its
        /// long sides (left or right) is open.
        /// </summary>
        public static bool IsFree(Position p, ISet<Position> occupied)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (occupied.Contains(new Position(p.X + dx, p.Y + dy, p.Z + 1)))
                    {
                        return false;
                    }
                }
            }

            return !HasNeighbour(p, -2, occupied) || !HasNeighbour(p, 2, occupied);
        }

        /// <summary>True when two tiles on the same layer would occupy the same space.</summary>
        public static bool Overlaps(Position a, Position b)
        {
            return a.Z == b.Z && System.Math.Abs(a.X - b.X) < 2 && System.Math.Abs(a.Y - b.Y) < 2;
        }

        private static bool HasNeighbour(Position p, int dx, ISet<Position> occupied)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (occupied.Contains(new Position(p.X + dx, p.Y + dy, p.Z)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
