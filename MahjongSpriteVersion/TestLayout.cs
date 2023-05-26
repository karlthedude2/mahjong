using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahjongSpriteVersion
{
    public class TestLayout : Layout
    {


        protected override void Initialize()
        {
            Locations.Add(new Coordinate(0, 1, 0));
            Locations.Add(new Coordinate(4, 1, 0));
            Locations.Add(new Coordinate(8, 1, 0));
            Locations.Add(new Coordinate(12, 1, 0));
            Locations.Add(new Coordinate(16, 1, 0));
            Locations.Add(new Coordinate(20, 1, 0));
            Locations.Add(new Coordinate(0, 3, 0));
            Locations.Add(new Coordinate(4, 3, 0));
            Locations.Add(new Coordinate(8, 3, 0));
            Locations.Add(new Coordinate(12, 3, 0));
            Locations.Add(new Coordinate(16, 3, 0));
            Locations.Add(new Coordinate(20, 3, 0));
            Locations.Add(new Coordinate(0, 5, 0));
            Locations.Add(new Coordinate(4, 5, 0));
            Locations.Add(new Coordinate(8, 5, 0));
            Locations.Add(new Coordinate(12, 5, 0));
            Locations.Add(new Coordinate(16, 5, 0));
            Locations.Add(new Coordinate(20, 5, 0));
            Locations.Add(new Coordinate(0, 7, 0));
            Locations.Add(new Coordinate(4, 7, 0));
            Locations.Add(new Coordinate(8, 7, 0));
            Locations.Add(new Coordinate(12, 7, 0));
            Locations.Add(new Coordinate(16, 7, 0));
            Locations.Add(new Coordinate(20, 7, 0));
        }
    }
}
