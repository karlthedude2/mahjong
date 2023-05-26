using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahjongSpriteVersion
{
    public class StandardTiles : TileStyles
    {
        protected override void InitializeStyles() {

            for (int i = 0; i < 4; i++) {

                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.2bams.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.3bams.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.4bams.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.5bams.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.6bams.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.7bams.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.8bams.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.9bams.png", 10));

                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.1dots.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.2dots.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.3dots.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.4dots.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.5dots.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.6dots.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.7dots.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.8dots.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.9dots.png", 10));

                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.1crack.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.2crack.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.3crack.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.4crack.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.5crack.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.6crack.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.7crack.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.8crack.png", 10));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.9crack.png", 10));

                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.Bdragon.png", 20));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.Cdragon.png", 20));
                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.Fdragon.png", 20));

                Styles.Add(new TileStyle("MahjongSpriteVersion.Graphics.peacock.png", 20));
            }

            for (int i = 0; i < 2; i++) {

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
