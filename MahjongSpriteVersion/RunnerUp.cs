using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahjongSpriteVersion
{
    public class RunnerUp: Layout
    {

        public RunnerUp() {
        }

        protected override void Initialize()
        {

            // First Layer
            Locations.Add(new Coordinate(1, 1, 0));
            Locations.Add(new Coordinate(3, 1, 0));
            Locations.Add(new Coordinate(5, 1, 0));
            Locations.Add(new Coordinate(9, 1, 0));
            Locations.Add(new Coordinate(11, 1, 0));
            Locations.Add(new Coordinate(13, 1, 0));
            Locations.Add(new Coordinate(15, 1, 0));
            Locations.Add(new Coordinate(19, 1, 0));
            Locations.Add(new Coordinate(21, 1, 0));
            Locations.Add(new Coordinate(23, 1, 0));

            Locations.Add(new Coordinate(1, 3, 0));
            Locations.Add(new Coordinate(7, 3, 0));
            Locations.Add(new Coordinate(9, 3, 0));
            Locations.Add(new Coordinate(11, 3, 0));
            Locations.Add(new Coordinate(13, 3, 0));
            Locations.Add(new Coordinate(15, 3, 0));
            Locations.Add(new Coordinate(17, 3, 0));
            Locations.Add(new Coordinate(23, 3, 0));

            Locations.Add(new Coordinate(1, 5, 0));
            Locations.Add(new Coordinate(5, 5, 0));
            Locations.Add(new Coordinate(7, 5, 0));
            Locations.Add(new Coordinate(9, 5, 0));
            Locations.Add(new Coordinate(11, 5, 0));
            Locations.Add(new Coordinate(13, 5, 0));
            Locations.Add(new Coordinate(15, 5, 0));
            Locations.Add(new Coordinate(17, 5, 0));
            Locations.Add(new Coordinate(19, 5, 0));
            Locations.Add(new Coordinate(23, 5, 0));

            Locations.Add(new Coordinate(3, 7, 0));
            Locations.Add(new Coordinate(5, 7, 0));
            Locations.Add(new Coordinate(7, 7, 0));
            Locations.Add(new Coordinate(9, 7, 0));
            Locations.Add(new Coordinate(11, 7, 0));
            Locations.Add(new Coordinate(13, 7, 0));
            Locations.Add(new Coordinate(15, 7, 0));
            Locations.Add(new Coordinate(17, 7, 0));
            Locations.Add(new Coordinate(19, 7, 0));
            Locations.Add(new Coordinate(21, 7, 0));

            Locations.Add(new Coordinate(3, 9, 0));
            Locations.Add(new Coordinate(5, 9, 0));
            Locations.Add(new Coordinate(7, 9, 0));
            Locations.Add(new Coordinate(9, 9, 0));
            Locations.Add(new Coordinate(11, 9, 0));
            Locations.Add(new Coordinate(13, 9, 0));
            Locations.Add(new Coordinate(15, 9, 0));
            Locations.Add(new Coordinate(17, 9, 0));
            Locations.Add(new Coordinate(19, 9, 0));
            Locations.Add(new Coordinate(21, 9, 0));

            Locations.Add(new Coordinate(1, 11, 0));
            Locations.Add(new Coordinate(5, 11, 0));
            Locations.Add(new Coordinate(7, 11, 0));
            Locations.Add(new Coordinate(9, 11, 0));
            Locations.Add(new Coordinate(11, 11, 0));
            Locations.Add(new Coordinate(13, 11, 0));
            Locations.Add(new Coordinate(15, 11, 0));
            Locations.Add(new Coordinate(17, 11, 0));
            Locations.Add(new Coordinate(19, 11, 0));
            Locations.Add(new Coordinate(23, 11, 0));

            Locations.Add(new Coordinate(1, 13, 0));
            Locations.Add(new Coordinate(7, 13, 0));
            Locations.Add(new Coordinate(9, 13, 0));
            Locations.Add(new Coordinate(11, 13, 0));
            Locations.Add(new Coordinate(13, 13, 0));
            Locations.Add(new Coordinate(15, 13, 0));
            Locations.Add(new Coordinate(17, 13, 0));
            Locations.Add(new Coordinate(23, 13, 0));

            Locations.Add(new Coordinate(1, 15, 0));
            Locations.Add(new Coordinate(3, 15, 0));
            Locations.Add(new Coordinate(5, 15, 0));
            Locations.Add(new Coordinate(9, 15, 0));
            Locations.Add(new Coordinate(11, 15, 0));
            Locations.Add(new Coordinate(13, 15, 0));
            Locations.Add(new Coordinate(15, 15, 0));
            Locations.Add(new Coordinate(19, 15, 0));
            Locations.Add(new Coordinate(21, 15, 0));
            Locations.Add(new Coordinate(23, 15, 0));

            //Second Layer
            Locations.Add(new Coordinate(1, 1, 1));
            Locations.Add(new Coordinate(3, 1, 1));
            Locations.Add(new Coordinate(12, 1, 1));
            Locations.Add(new Coordinate(21, 1, 1));
            Locations.Add(new Coordinate(23, 1, 1));

            Locations.Add(new Coordinate(1, 3, 1));
            Locations.Add(new Coordinate(11, 3, 1));
            Locations.Add(new Coordinate(13, 3, 1));
            Locations.Add(new Coordinate(23, 3, 1));

            Locations.Add(new Coordinate(9, 5, 1));
            Locations.Add(new Coordinate(11, 5, 1));
            Locations.Add(new Coordinate(13, 5, 1));
            Locations.Add(new Coordinate(15, 5, 1));

            Locations.Add(new Coordinate(5, 7, 1));
            Locations.Add(new Coordinate(7, 7, 1));
            Locations.Add(new Coordinate(9, 7, 1));
            Locations.Add(new Coordinate(11, 7, 1));
            Locations.Add(new Coordinate(13, 7, 1));
            Locations.Add(new Coordinate(15, 7, 1));
            Locations.Add(new Coordinate(17, 7, 1));
            Locations.Add(new Coordinate(19, 7, 1));

            Locations.Add(new Coordinate(5, 9, 1));
            Locations.Add(new Coordinate(7, 9, 1));
            Locations.Add(new Coordinate(9, 9, 1));
            Locations.Add(new Coordinate(11, 9, 1));
            Locations.Add(new Coordinate(13, 9, 1));
            Locations.Add(new Coordinate(15, 9, 1));
            Locations.Add(new Coordinate(17, 9, 1));
            Locations.Add(new Coordinate(19, 9, 1));

            Locations.Add(new Coordinate(9, 11, 1));
            Locations.Add(new Coordinate(11, 11, 1));
            Locations.Add(new Coordinate(13, 11, 1));
            Locations.Add(new Coordinate(15, 11, 1));

            Locations.Add(new Coordinate(1, 13, 1));
            Locations.Add(new Coordinate(11, 13, 1));
            Locations.Add(new Coordinate(13, 13, 1));
            Locations.Add(new Coordinate(23, 13, 1));

            Locations.Add(new Coordinate(1, 15, 1));
            Locations.Add(new Coordinate(3, 15, 1));
            Locations.Add(new Coordinate(12, 15, 1));
            Locations.Add(new Coordinate(21, 15, 1));
            Locations.Add(new Coordinate(23, 15, 1));

            // Third Layer
            Locations.Add(new Coordinate(1, 1, 2));
            Locations.Add(new Coordinate(23, 1, 2));

            Locations.Add(new Coordinate(11, 5, 2));
            Locations.Add(new Coordinate(13, 5, 2));

            Locations.Add(new Coordinate(7, 8, 2));

            Locations.Add(new Coordinate(9, 7, 2));
            Locations.Add(new Coordinate(11, 7, 2));
            Locations.Add(new Coordinate(13, 7, 2));
            Locations.Add(new Coordinate(15, 7, 2));

            Locations.Add(new Coordinate(9, 9, 2));
            Locations.Add(new Coordinate(11, 9, 2));
            Locations.Add(new Coordinate(13, 9, 2));
            Locations.Add(new Coordinate(15, 9, 2));

            Locations.Add(new Coordinate(17, 8, 2));

            Locations.Add(new Coordinate(11, 11, 2));
            Locations.Add(new Coordinate(13, 11, 2));

            Locations.Add(new Coordinate(1, 15, 2));
            Locations.Add(new Coordinate(23, 15, 2));

            //Fourth Layer
            Locations.Add(new Coordinate(9, 8, 3));

            Locations.Add(new Coordinate(11, 7, 3));
            Locations.Add(new Coordinate(13, 7, 3));

            Locations.Add(new Coordinate(11, 9, 3));
            Locations.Add(new Coordinate(13, 9, 3));

            Locations.Add(new Coordinate(15, 8, 3));

            //Fifth & Sixth
            Locations.Add(new Coordinate(12, 8, 4));
            Locations.Add(new Coordinate(12, 8, 5));


        }
    }
}
