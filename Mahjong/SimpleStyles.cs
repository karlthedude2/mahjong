using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahjong
{
    class SimpleStyles : TileStyles 
    {
        protected override void InitializeStyles() {

            Styles = new List<TileStyle>();
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
            }

            Shuffle();
            base.InitializeStyles();
        }
    }
}
