using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mahjong
{
    public partial class LayoutPanel : UserControl
    {

        Tile selectedTile = null;
        private const int yOffset = 40;

        public LayoutPanel() {
            InitializeComponent();
        }

        public event EventHandler OnTilesRemoved = null;
        public event EventHandler OnGameOver = null;

        [Browsable(true)]
        public Layout TileLayout { get; set; }

        [Browsable(true)]
        public TileStyles TileStyles { get; set; }

        public void Render() {
            TileLayout.Locations.Reverse();
            int index = 0;
            foreach (Coordinate coordinate in TileLayout.Locations) {
                Tile tile = new Tile(TileLayout, coordinate, TileStyles.Styles[index++]);
                tile.Location = new Point(coordinate.GetPixelX(), coordinate.GetPixelY() + yOffset);
                this.Controls.Add(tile);

               // Controls.SetChildIndex(tile, tile.Coordinate.Z);
                tile.Click += tile_Click;
            }

        }

        public void Shuffle() {

            foreach (Tile tile in Controls) {
                tile.ShuffleIndex = TileStyles.Styles.IndexOf(tile.Style);
            }

            TileStyles.Shuffle();

            foreach (Tile tile in Controls) {
                tile.Style = TileStyles.Styles[tile.ShuffleIndex];
                tile.Shuffled = true;
                tile.Invalidate();
            }

           // while (GetStatus() !=) ;
            Update();

        }

        void tile_Click(object sender, EventArgs e) {
            Tile clickedTile = sender as Tile;
            //Controls.SetChildIndex(clickedTile, clickedTile.Coordinate.Z);
            if (selectedTile == null) {
                selectedTile = clickedTile;
            }
            else if (selectedTile.Style.Name == clickedTile.Style.Name &&
                selectedTile.Id != clickedTile.Id) {
                RemoveTile(selectedTile);
                RemoveTile(clickedTile);
                selectedTile = null;

                TileStatus status = GetStatus();
                if (status == TileStatus.Blocked)
                    MessageBox.Show("There are no more solvable pairs");
                else if (status == TileStatus.GameComplete) {
                    if (OnGameOver != null) {
                        OnGameOver(this, new EventArgs());
                    }
                    MessageBox.Show("You Win!!");
                }

                if (OnTilesRemoved != null) {
                    OnTilesRemoved(this, new EventArgs());
                }
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

        private void RemoveTile(Tile tile) {
            try {

                this.Controls.Remove(tile);
                tile.Dispose();
                this.TileLayout.Locations.Remove(tile.Coordinate);
                this.TileStyles.Styles.Remove(tile.Style);
            }
            catch { }
        }

        private void LayoutPanel_Click(object sender, EventArgs e) {

            if (selectedTile != null) {
                selectedTile.Deselect();
                selectedTile = null;
            }
        }

        private TileStatus GetStatus() {

            List<TileStyle> styles = new List<TileStyle>();
            foreach (Tile tile in Controls) {
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
    }
}
