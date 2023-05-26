using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahjongSpriteVersion
{
    public class FavoriteLayout : Layout
    {

        public FavoriteLayout() {
        }
           
        protected override void Initialize()
        {

            //First Layer
            Locations.Add(new Coordinate(0, 1, 0));
            Locations.Add(new Coordinate(2, 1, 0));
            Locations.Add(new Coordinate(4, 1, 0));
            Locations.Add(new Coordinate(6, 1, 0));
            Locations.Add(new Coordinate(8, 1, 0));
            Locations.Add(new Coordinate(10, 1, 0));
            Locations.Add(new Coordinate(12, 1, 0));
            Locations.Add(new Coordinate(14, 1, 0));
            Locations.Add(new Coordinate(16, 1, 0));
            Locations.Add(new Coordinate(18, 1, 0));
            Locations.Add(new Coordinate(20, 1, 0));
            Locations.Add(new Coordinate(22, 1, 0));

            Locations.Add(new Coordinate(2, 3, 0));
            Locations.Add(new Coordinate(4, 3, 0));
            Locations.Add(new Coordinate(6, 3, 0));
            Locations.Add(new Coordinate(8, 3, 0));

            Locations.Add(new Coordinate(14, 3, 0));
            Locations.Add(new Coordinate(16, 3, 0));
            Locations.Add(new Coordinate(18, 3, 0));
            Locations.Add(new Coordinate(20, 3, 0));

            Locations.Add(new Coordinate(2, 5, 0));
            Locations.Add(new Coordinate(4, 5, 0));
            Locations.Add(new Coordinate(6, 5, 0));
            Locations.Add(new Coordinate(8, 5, 0));

            Locations.Add(new Coordinate(14, 5, 0));
            Locations.Add(new Coordinate(16, 5, 0));
            Locations.Add(new Coordinate(18, 5, 0));
            Locations.Add(new Coordinate(20, 5, 0));

            Locations.Add(new Coordinate(4, 7, 0));
            Locations.Add(new Coordinate(6, 7, 0));

            Locations.Add(new Coordinate(10, 7, 0));
            Locations.Add(new Coordinate(12, 7, 0));

            Locations.Add(new Coordinate(16, 7, 0));
            Locations.Add(new Coordinate(18, 7, 0));

            Locations.Add(new Coordinate(4, 9, 0));
            Locations.Add(new Coordinate(6, 9, 0));

            Locations.Add(new Coordinate(10, 9, 0));
            Locations.Add(new Coordinate(12, 9, 0));

            Locations.Add(new Coordinate(16, 9, 0));
            Locations.Add(new Coordinate(18, 9, 0));

            Locations.Add(new Coordinate(2, 11, 0));
            Locations.Add(new Coordinate(4, 11, 0));
            Locations.Add(new Coordinate(6, 11, 0));
            Locations.Add(new Coordinate(8, 11, 0));

            Locations.Add(new Coordinate(14, 11, 0));
            Locations.Add(new Coordinate(16, 11, 0));
            Locations.Add(new Coordinate(18, 11, 0));
            Locations.Add(new Coordinate(20, 11, 0));

            Locations.Add(new Coordinate(2, 13, 0));
            Locations.Add(new Coordinate(4, 13, 0));
            Locations.Add(new Coordinate(6, 13, 0));
            Locations.Add(new Coordinate(8, 13, 0));

            Locations.Add(new Coordinate(14, 13, 0));
            Locations.Add(new Coordinate(16, 13, 0));
            Locations.Add(new Coordinate(18, 13, 0));
            Locations.Add(new Coordinate(20, 13, 0));

            Locations.Add(new Coordinate(0, 15, 0));
            Locations.Add(new Coordinate(2, 15, 0));
            Locations.Add(new Coordinate(4, 15, 0));
            Locations.Add(new Coordinate(6, 15, 0));
            Locations.Add(new Coordinate(8, 15, 0));
            Locations.Add(new Coordinate(10, 15, 0));
            Locations.Add(new Coordinate(12, 15, 0));
            Locations.Add(new Coordinate(14, 15, 0));
            Locations.Add(new Coordinate(16, 15, 0));
            Locations.Add(new Coordinate(18, 15, 0));
            Locations.Add(new Coordinate(20, 15, 0));
            Locations.Add(new Coordinate(22, 15, 0));

            // 2nd Layer
            Locations.Add(new Coordinate(2, 1, 1));
            Locations.Add(new Coordinate(4, 1, 1));
            Locations.Add(new Coordinate(6, 1, 1));
            Locations.Add(new Coordinate(8, 1, 1));
            Locations.Add(new Coordinate(10, 1, 1));
            Locations.Add(new Coordinate(12, 1, 1));
            Locations.Add(new Coordinate(14, 1, 1));
            Locations.Add(new Coordinate(16, 1, 1));
            Locations.Add(new Coordinate(18, 1, 1));
            Locations.Add(new Coordinate(20, 1, 1));


            Locations.Add(new Coordinate(4, 3, 1));
            Locations.Add(new Coordinate(6, 3, 1));

            Locations.Add(new Coordinate(16, 3, 1));
            Locations.Add(new Coordinate(18, 3, 1));

            Locations.Add(new Coordinate(4, 5, 1));
            Locations.Add(new Coordinate(6, 5, 1));

            Locations.Add(new Coordinate(16, 5, 1));
            Locations.Add(new Coordinate(18, 5, 1));

            Locations.Add(new Coordinate(5, 7, 1));

            Locations.Add(new Coordinate(10, 7, 1));
            Locations.Add(new Coordinate(12, 7, 1));

            Locations.Add(new Coordinate(17, 7, 1));

            Locations.Add(new Coordinate(5, 9, 1));

            Locations.Add(new Coordinate(10, 9, 1));
            Locations.Add(new Coordinate(12, 9, 1));

            Locations.Add(new Coordinate(17, 9, 1));


            Locations.Add(new Coordinate(4, 11, 1));
            Locations.Add(new Coordinate(6, 11, 1));

            Locations.Add(new Coordinate(16, 11, 1));
            Locations.Add(new Coordinate(18, 11, 1));

            Locations.Add(new Coordinate(4, 13, 1));
            Locations.Add(new Coordinate(6, 13, 1));

            Locations.Add(new Coordinate(16, 13, 1));
            Locations.Add(new Coordinate(18, 13, 1));

            Locations.Add(new Coordinate(2, 15, 1));
            Locations.Add(new Coordinate(4, 15, 1));
            Locations.Add(new Coordinate(6, 15, 1));
            Locations.Add(new Coordinate(8, 15, 1));
            Locations.Add(new Coordinate(10, 15, 1));
            Locations.Add(new Coordinate(12, 15, 1));
            Locations.Add(new Coordinate(14, 15, 1));
            Locations.Add(new Coordinate(16, 15, 1));
            Locations.Add(new Coordinate(18, 15, 1));
            Locations.Add(new Coordinate(20, 15, 1));

            //3rd layer

            Locations.Add(new Coordinate(4, 1, 2));
            Locations.Add(new Coordinate(6, 1, 2));
            Locations.Add(new Coordinate(8, 1, 2));
            Locations.Add(new Coordinate(14, 1, 2));
            Locations.Add(new Coordinate(16, 1, 2));
            Locations.Add(new Coordinate(18, 1, 2));

            Locations.Add(new Coordinate(5, 3, 2));

            Locations.Add(new Coordinate(17, 3, 2));

            Locations.Add(new Coordinate(10, 7, 2));
            Locations.Add(new Coordinate(12, 7, 2));

            Locations.Add(new Coordinate(10, 9, 2));
            Locations.Add(new Coordinate(12, 9, 2));

            Locations.Add(new Coordinate(5, 13, 2));

            Locations.Add(new Coordinate(17, 13, 2));

            Locations.Add(new Coordinate(4, 15, 2));
            Locations.Add(new Coordinate(6, 15, 2));
            Locations.Add(new Coordinate(8, 15, 2));
            Locations.Add(new Coordinate(14, 15, 2));
            Locations.Add(new Coordinate(16, 15, 2));
            Locations.Add(new Coordinate(18, 15, 2));

            // 4th layer
            Locations.Add(new Coordinate(6, 1, 3));
            Locations.Add(new Coordinate(8, 1, 3));
            Locations.Add(new Coordinate(14, 1, 3));
            Locations.Add(new Coordinate(16, 1, 3));

            Locations.Add(new Coordinate(6, 15, 3));
            Locations.Add(new Coordinate(8, 15, 3));
            Locations.Add(new Coordinate(14, 15, 3));
            Locations.Add(new Coordinate(16, 15, 3));

            //5th layer

            Locations.Add(new Coordinate(8, 1, 4));
            Locations.Add(new Coordinate(14, 1, 4));

            Locations.Add(new Coordinate(8, 15, 4));
            Locations.Add(new Coordinate(14, 15, 4));


        }
    }
}
