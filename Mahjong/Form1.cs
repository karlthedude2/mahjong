using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mahjong
{
    public partial class Form1 : Form
    {
        Timer timer = new Timer();
        int seconds = 0;

        public Form1() {
            InitializeComponent();

            layoutPanel.TileLayout = new FavoriteLayout();
            layoutPanel.TileStyles = new StandardTiles();
            layoutPanel.Render();

            txtLayoutCount.Text = layoutPanel.TileLayout.Locations.Count.ToString();
            txtStyleCount.Text = layoutPanel.TileStyles.Styles.Count.ToString();

            layoutPanel.OnTilesRemoved += layoutPanel_OnTilesRemoved;
            layoutPanel.OnGameOver += layoutPanel_OnGameOver;

            //label3.
            timer.Interval = 1000;
            timer.Tick += timer_Tick;
            timer.Start();


        }

        void layoutPanel_OnGameOver(object sender, EventArgs e) {
            timer.Stop();
        }


        void timer_Tick(object sender, EventArgs e) {
            seconds++;
            StringBuilder sb = new StringBuilder();
            //Hours
            string hours = seconds / 3600 > 1 ? (seconds / 3600).ToString() + ":" : string.Empty;
            string minutes = seconds / 60 > 1 ? (seconds / 60).ToString() : "0" + (seconds / 60).ToString();
            string secs = (seconds % 60).ToString().Length == 1 ? "0" + (seconds % 60).ToString() : (seconds % 60).ToString();
                //(seconds % 60).ToString();

            sb.Append(seconds % 3600 > 1 ? (seconds % 3600).ToString() + ":" : string.Empty);
            sb.Append(seconds % 60 > 1 ? (seconds % 60).ToString() : "0");
            sb.Append(":");
            sb.Append((seconds % 60).ToString().Length == 1 ? "0" : string.Empty);
            sb.Append((seconds % 60).ToString());

            lblClock.Text = hours.ToString() + minutes.ToString() + ":" + secs.ToString();
            //label3.Text = sb.ToString();

        }

        void layoutPanel_OnTilesRemoved(object sender, EventArgs e) {
            txtLayoutCount.Text = layoutPanel.TileLayout.Locations.Count.ToString();
            txtStyleCount.Text = layoutPanel.TileStyles.Styles.Count.ToString();
        }

        private void btnShuffle_Click(object sender, EventArgs e) {
            layoutPanel.Shuffle();
        }

        private void btnNew_Click(object sender, EventArgs e) {
            timer.Stop();
            layoutPanel.Controls.Clear();
            layoutPanel.TileLayout = new FavoriteLayout();
            layoutPanel.TileStyles = new StandardTiles();
            layoutPanel.Render();
            seconds = 0;
            timer.Start();
        }

        private void btnPause_Click(object sender, EventArgs e) {
            if (btnPause.Text == "Pause") {
                timer.Stop();
                layoutPanel.Enabled = false;
                btnPause.Text = "Play";
            }
            else {
                timer.Start();
                layoutPanel.Enabled = true;
                btnPause.Text = "Pause";
            }
        }
        
    }
}
