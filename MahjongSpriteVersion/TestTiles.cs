using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahjongSpriteVersion
{
    public class TestTiles : TileStyles
    {
        protected override void InitializeStyles()
        {

            for (int i = 0; i < 2; i++)
            {

                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.North.png", 40));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.South.png", 40));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.East.png", 40));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.West.png", 40));

                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.1flower.png", 40));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.2flower.png", 40));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.3flower.png", 40));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.4flower.png", 40));

                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.1wizard.png", 40));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.2wizard.png", 40));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.3wizard.png", 40));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.4wizard.png", 40));
            }

            Shuffle();
            base.InitializeStyles();
        }
    }
}
