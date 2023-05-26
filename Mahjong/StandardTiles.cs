using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahjong
{
    public class StandardTiles : TileStyles
    {
        protected override void InitializeStyles() {
            Styles = new List<TileStyle>();

            for (int i = 0; i < 4; i++) {

                Styles.Add(new TileStyle("Mahjong.Graphics.2bams.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.3bams.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.4bams.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.5bams.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.6bams.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.7bams.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.8bams.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.9bams.png"));

                Styles.Add(new TileStyle("Mahjong.Graphics.1dots.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.2dots.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.3dots.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.4dots.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.5dots.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.6dots.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.7dots.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.8dots.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.9dots.png"));

                Styles.Add(new TileStyle("Mahjong.Graphics.1crack.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.2crack.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.3crack.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.4crack.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.5crack.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.6crack.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.7crack.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.8crack.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.9crack.png"));

                Styles.Add(new TileStyle("Mahjong.Graphics.Bdragon.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.Cdragon.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.Fdragon.png"));

                Styles.Add(new TileStyle("Mahjong.Graphics.peacock.png"));
            }

            for (int i = 0; i < 2; i++) {

                Styles.Add(new TileStyle("Mahjong.Graphics.North.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.South.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.East.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.West.png"));

                Styles.Add(new TileStyle("Mahjong.Graphics.1flower.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.2flower.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.3flower.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.4flower.png"));

                Styles.Add(new TileStyle("Mahjong.Graphics.1wizard.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.2wizard.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.3wizard.png"));
                Styles.Add(new TileStyle("Mahjong.Graphics.4wizard.png"));
            }

            Shuffle();
            base.InitializeStyles();
        }
    }
}
