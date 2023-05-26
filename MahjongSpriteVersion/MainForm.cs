using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;

namespace MahjongSpriteVersion
{
    public partial class MainForm : Form
    {

        TileSprite selectedTile = null;
        //private const int yOffset = 40;
        Bitmap surface;
        Graphics device;
        SoundPlayer clickSound = new SoundPlayer("176893__mickmon__click-lighter.wav");
        private int score = 0;

        Timer timer = new Timer();
        int seconds = 0;
        int bonusSeconds = 300;
        int superBonusSeconds = 20;
        int tilesRemoved = 0;
        int superBonusTilesRemoved = 0;

        int times = 0;

        TileSprite undoTile1 = null;
        TileSprite undoTile2 = null;


        public event EventHandler OnTilesRemoved = null;
        public event EventHandler OnGameOver = null;

        public MainForm() {
            InitializeComponent();
            pbLayoutPanel.BackColor = Color.Transparent;
            lblClock.BackColor = Color.Transparent;
            lblScore.BackColor = Color.Transparent;
            lblSuper.BackColor = Color.Transparent;
            lblTimeBonus.BackColor = Color.Transparent;
            panel1.BackColor = Color.FromArgb(100, 255, 255, 255);

            //TileLayout = new FavoriteLayout();
            TileLayout = new RunnerUp();
            TileStyles = new StandardTiles();

            timer.Interval = 1000;
            timer.Tick += timer_Tick;
            timer.Start();
        }


        public TileStyles TileStyles { get; set; }
        public Layout TileLayout { get; set; }

        public void Render()
        {
            device.Clear(Color.Transparent);
            foreach (TileSprite tile in TileLayout.Tiles) {
                tile.Draw(device);
            }
            pbLayoutPanel.Image = surface;
        }

        private void LoadLayout() {

            //TileLayout.Locations.Reverse();
            int index = 0;
            foreach (Coordinate coordinate in TileLayout.Locations) {
                TileSprite tile = new TileSprite(TileLayout, coordinate, TileStyles.Styles[index++]);
                tile.Location = new Point(coordinate.GetPixelX(), coordinate.GetPixelY());
                TileLayout.Tiles.Add(tile);

                // Controls.SetChildIndex(tile, tile.Coordinate.Z);
                // tile.Click += tile_Click;
            }
        }

        private void btnShuffle_Click(object sender, EventArgs e) {
            Shuffle();
        }
        void timer_Tick(object sender, EventArgs e) {
            seconds++;

            if (--superBonusSeconds == 0) {
                superBonusSeconds = 30;
                txtTilesCollected.Text = "0";
                superBonusTilesRemoved = 0;
            }

            if (bonusSeconds != 0) {
                bonusSeconds--;
            }
            //StringBuilder sb = new StringBuilder();
            //Hours
            string hours = seconds / 3600 > 1 ? (seconds / 3600).ToString() + ":" : string.Empty;
            string minutes = (seconds / 60) % 60 > 1 ? ((seconds / 60) % 60).ToString() : "0" + ((seconds / 60) % 60).ToString();
            string secs = (seconds % 60).ToString().Length == 1 ? "0" + (seconds % 60).ToString() : (seconds % 60).ToString();
            //(seconds % 60).ToString();

            //sb.Append(seconds % 3600 > 1 ? (seconds % 3600).ToString() + ":" : string.Empty);
            //sb.Append(seconds % 60 > 1 ? (seconds % 60).ToString() : "0");
            //sb.Append(":");
            //sb.Append((seconds % 60).ToString().Length == 1 ? "0" : string.Empty);
            //sb.Append((seconds % 60).ToString());

            lblClock.Text = hours.ToString() + minutes.ToString() + ":" + secs.ToString();
            DisplayStatus();

            txtSuperTime.Text = superBonusSeconds.ToString();
            
            
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            surface = new Bitmap(this.Size.Width, this.Size.Height);
            pbLayoutPanel.Image = surface;
            device = Graphics.FromImage(surface);

            txtTilesCollected.Text = superBonusTilesRemoved.ToString();
            txtTilesCollected.Text = "0";

            LoadLayout();
            Render();
        }

        private void pbLayoutPanel_Click(object sender, EventArgs e) {
            //TileSprite tile = TileLayout.FindTile(Mouse
        }

        private void pbLayoutPanel_MouseClick(object sender, MouseEventArgs e) {
            TileSprite clickedTile = TileLayout.FindTile(e.Location);
            if (clickedTile != null) {
                if (clickSound != null) {
                    clickSound.Play();
                }
                clickedTile.OnClick();

                if (selectedTile == null) {
                    selectedTile = clickedTile;
                }
                else if (selectedTile.Style.Name == clickedTile.Style.Name &&
                    selectedTile.Id != clickedTile.Id) {
                    RemoveTile(selectedTile);
                    RemoveTile(clickedTile);
                    undoTile1 = selectedTile;
                    undoTile2 = clickedTile;
                    selectedTile = null;

                    if (OnTilesRemoved != null) {
                        OnTilesRemoved(this, new EventArgs());
                    }

                    times = 1;
                }
                else if (selectedTile != clickedTile) {
                    selectedTile.Deselect();
                    selectedTile = clickedTile;
                }
                else {
                    selectedTile.Deselect();
                    selectedTile = null;
                }
            }
            Render();
        }

        private void RemoveTile(TileSprite tile) {
            try {
                score += tile.Style.Value;
                if (++tilesRemoved == 20) {
                    //tilesRemoved = 0;
                    score += bonusSeconds;

                    //superBonusSeconds = 20;
                    tilesRemoved = 0;
                }

                if (superBonusSeconds > 0 && ++superBonusTilesRemoved == 20) {

                    //if (superBonusSeconds == 20)
                    //    MessageBox.Show("superBonusSeconds is 20");

                    score += 100 * superBonusSeconds;

                    UpdateScore(txtSuper, 100 * superBonusSeconds);
                    UpdateScore(txtSuperQty, 1);
                    superBonusSeconds = 30;
                    superBonusTilesRemoved = 0;
                } 

                lblScore.Text = score.ToString();
                txtTilesCollected.Text = superBonusTilesRemoved.ToString();

                TileLayout.Tiles.Remove(tile);
                this.TileLayout.Locations.Remove(tile.Coordinate);
                this.TileStyles.Styles.Remove(tile.Style);
            }
            catch(Exception ex) { 
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateScore(TextBox txt, int addedAmount) {
            int origValue;
            int.TryParse(txt.Text, out origValue);
            txt.Text = (origValue + addedAmount).ToString();
        }

        private TileStatus GetStatus() {

            List<TileStyle> styles = new List<TileStyle>();
            foreach (TileSprite tile in TileLayout.Tiles) {
                if (!TileLayout.IsBlocked(tile.Coordinate)) {
                    styles.Add(tile.Style);
                }
            }

            foreach (TileStyle style in styles) {
                var items = styles.FindAll(s => s.Name == style.Name);
                if (items.Count > 1) {
                    return TileStatus.Available;
                }
            }

            if (this.TileStyles.Styles.Count == 0)
                return TileStatus.GameComplete;

            return TileStatus.Blocked;
        }

        private void DisplayStatus() {
            if (times == 0)
                return;

            if (times == 1)
                times++;

            if (times == 2) {
                times = 0;
                TileStatus status = GetStatus();
                if (status == TileStatus.Blocked)
                    MessageBox.Show("There are no more solvable pairs");
                else if (status == TileStatus.GameComplete) {
                    if (OnGameOver != null) {
                        OnGameOver(this, new EventArgs());
                    }
                    if (bonusSeconds > 0) {
                        score += bonusSeconds * 30;
                        lblScore.Text = score.ToString();
                        txtTimeBonus.Text = (bonusSeconds * 30).ToString();
                    }
                    timer.Stop();
                    MessageBox.Show("You Win!!");
                }
            }
        }

        public void Shuffle() {

            foreach (TileSprite tile in TileLayout.Tiles) {
                tile.ShuffleIndex = TileStyles.Styles.IndexOf(tile.Style);
            }

            TileStyles.Shuffle();

            foreach (TileSprite tile in TileLayout.Tiles) {
                tile.Style = TileStyles.Styles[tile.ShuffleIndex];
                tile.Shuffled = true;
            }
            Render();

        }

        private void btnNew_Click(object sender, EventArgs e) {
            timer.Stop();
            score = 0;
            bonusSeconds = 300;
            txtTimeBonus.Text = string.Empty;
            txtSuper.Text = string.Empty;
            txtSuperQty.Text = string.Empty;
            TileLayout = new FavoriteLayout();
            TileStyles = new StandardTiles();
            LoadLayout();
            seconds = 0;
            timer.Start();
            Render();
        }

        private void btnPause_Click(object sender, EventArgs e) {
            
            if (btnPause.Text == "Pause") {
                timer.Stop();
                pbLayoutPanel.Enabled = false;
                btnPause.Text = "Play";
            }
            else {
                timer.Start();
                pbLayoutPanel.Enabled = true;
                btnPause.Text = "Pause";
            }
        }

        private void cboLayout_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void btnUndo_Click(object sender, EventArgs e) {
            undoTile1.Deselect();
            undoTile2.Deselect();
            TileLayout.Tiles.Add(undoTile1);
            TileLayout.Tiles.Add(undoTile2);
            Render();
        }
        
    }
}
