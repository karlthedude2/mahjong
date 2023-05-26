using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahjongSpriteVersion
{
    public class LayoutFactory
    {
        public static Layout CreateLayout(string layoutName)
        {
            Layout layout = null;

            switch(layoutName)
            {
                case "Number One":
                    layout = new FavoriteLayout();
                    break;

                case "The Runner Up":
                    layout = new RunnerUp();
                    break;

                case "Test":
                    layout = new TestLayout();
                    break;
            }

            return layout;
        }
    }
}
