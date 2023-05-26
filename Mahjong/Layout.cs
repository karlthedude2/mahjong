using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahjong
{
    public class Layout
    {
        public Layout() {
            Locations = new List<Coordinate>();
        }

        public List<Coordinate> Locations { get; private set; }

        public bool IsBlocked(Coordinate coordinate) {

            var adjacentLeft1 = Locations.Find(t => t.X == coordinate.X - 2 && t.Y == coordinate.Y && t.Z == coordinate.Z);
            var adjacentLeft2 = Locations.Find(t => t.X == coordinate.X - 2 && t.Y == coordinate.Y - 1 && t.Z == coordinate.Z);
            var adjacentLeft3 = Locations.Find(t => t.X == coordinate.X - 2 && t.Y == coordinate.Y + 1 && t.Z == coordinate.Z);

            var adjacentRight1 = Locations.Find(t => t.X == coordinate.X + 2 && t.Y == coordinate.Y && t.Z == coordinate.Z);
            var adjacentRight2 = Locations.Find(t => t.X == coordinate.X + 2 && t.Y == coordinate.Y - 1 && t.Z == coordinate.Z);
            var adjacentRight3 = Locations.Find(t => t.X == coordinate.X + 2 && t.Y == coordinate.Y + 1 && t.Z == coordinate.Z);

            var atop1 = Locations.Find(t => t.X == coordinate.X     && t.Y == coordinate.Y     && t.Z == coordinate.Z + 1);
            var atop2 = Locations.Find(t => t.X == coordinate.X + 1 && t.Y == coordinate.Y     && t.Z == coordinate.Z + 1);
            var atop3 = Locations.Find(t => t.X == coordinate.X - 1 && t.Y == coordinate.Y     && t.Z == coordinate.Z + 1);
            var atop4 = Locations.Find(t => t.X == coordinate.X + 1 && t.Y == coordinate.Y + 1 && t.Z == coordinate.Z + 1);
            var atop5 = Locations.Find(t => t.X == coordinate.X + 1 && t.Y == coordinate.Y - 1 && t.Z == coordinate.Z + 1);
            var atop6 = Locations.Find(t => t.X == coordinate.X - 1 && t.Y == coordinate.Y + 1 && t.Z == coordinate.Z + 1);
            var atop7 = Locations.Find(t => t.X == coordinate.X - 1 && t.Y == coordinate.Y - 1 && t.Z == coordinate.Z + 1);

            bool sidesBlocked = (adjacentLeft1 != null || adjacentLeft2 != null || adjacentLeft3 != null) &&
                                (adjacentRight1 != null || adjacentRight2 != null || adjacentRight3 != null);

            bool topBlocked = atop1 != null || atop2 != null || atop3 != null || atop4 != null ||
                              atop5 != null || atop6 != null || atop7 != null;

            return sidesBlocked || topBlocked;
        }
    }
}
