using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahjong
{
    public class SimpleLayout : Layout
    {
        public SimpleLayout() {
            Initialize();
        }

        private void Initialize() {

            Locations.Add(new Coordinate(0, 0, 0));
            Locations.Add(new Coordinate(2, 0, 0));
            Locations.Add(new Coordinate(4, 0, 0));
            Locations.Add(new Coordinate(6, 0, 0));
            Locations.Add(new Coordinate(8, 0, 0));
            Locations.Add(new Coordinate(10, 0, 0));
            Locations.Add(new Coordinate(12, 0, 0));
            Locations.Add(new Coordinate(14, 0, 0));
            Locations.Add(new Coordinate(16, 0, 0));
            Locations.Add(new Coordinate(18, 0, 0));
            Locations.Add(new Coordinate(20, 0, 0));
            Locations.Add(new Coordinate(22, 0, 0));
            Locations.Add(new Coordinate(0, 2, 0));
            Locations.Add(new Coordinate(2, 2, 0));
            Locations.Add(new Coordinate(4, 2, 0));
            Locations.Add(new Coordinate(6, 2, 0));
            Locations.Add(new Coordinate(8, 2, 0));
            Locations.Add(new Coordinate(10, 2, 0));
            Locations.Add(new Coordinate(12, 2, 0));
            Locations.Add(new Coordinate(14, 2, 0));
            Locations.Add(new Coordinate(16, 2, 0));
            Locations.Add(new Coordinate(18, 2, 0));
            Locations.Add(new Coordinate(20, 2, 0));
            Locations.Add(new Coordinate(22, 2, 0));
            Locations.Add(new Coordinate(0, 4, 0));
            Locations.Add(new Coordinate(2, 4, 0));
            Locations.Add(new Coordinate(4, 4, 0));
            Locations.Add(new Coordinate(6, 4, 0));
            Locations.Add(new Coordinate(8, 4, 0));
            Locations.Add(new Coordinate(10, 4, 0));
            Locations.Add(new Coordinate(12, 4, 0));
            Locations.Add(new Coordinate(14, 4, 0));
            Locations.Add(new Coordinate(16, 4, 0));
            Locations.Add(new Coordinate(18, 4, 0));
            Locations.Add(new Coordinate(20, 4, 0));
            Locations.Add(new Coordinate(22, 4, 0));
            Locations.Add(new Coordinate(0, 6, 0));
            Locations.Add(new Coordinate(2, 6, 0));
            Locations.Add(new Coordinate(4, 6, 0));
            Locations.Add(new Coordinate(6, 6, 0));
            Locations.Add(new Coordinate(8, 6, 0));
            Locations.Add(new Coordinate(10, 6, 0));
            Locations.Add(new Coordinate(12, 6, 0));
            Locations.Add(new Coordinate(14, 6, 0));
            Locations.Add(new Coordinate(16, 6, 0));
            Locations.Add(new Coordinate(18, 6, 0));
            Locations.Add(new Coordinate(20, 6, 0));
            Locations.Add(new Coordinate(22, 6, 0));
            //Locations.Add(new Coordinate(1, 0, 1));

        }
    }
}
