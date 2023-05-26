using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahjongSpriteVersion
{
    public class StyleFactory
    {
        public static TileStyles CreateStyle(string layoutName)
        {
            TileStyles style = null;

            switch(layoutName)
            {
                case "Number One":
                    style = new StandardTiles();
                    break;

                case "The Runner Up":
                    style = new StandardTiles();
                    break;

                case "Test":
                    style = new TestTiles();
                    break;
            }

            return style;
        }
    }
}
