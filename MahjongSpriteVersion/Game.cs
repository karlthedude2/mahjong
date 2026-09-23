using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Media;
using System.Reflection;
using Mahjong.Core;

namespace MahjongSpriteVersion
{
    /// <summary>Connects a <see cref="MahjongGame"/> to the screen: selection, clicking, drawing and sound.</summary>
    public sealed class Game : IDisposable
    {
        private readonly TileImageCache images = new TileImageCache();
        private SoundPlayer clickSound;
        private Graphics device;
        private Tile selectedTile;

        // Where the layout is drawn: centred in the play area, and shrunk only if it can't fit
        // (the form sizes the play area for the largest layout, so normally scale is 1).
        private float scale = 1f;
        private PointF offset;

        public event EventHandler TilesRemoved;

        public MahjongGame Current { get; private set; }
        public Scoring Scoring => Current.Scoring;
        public Bitmap Surface { get; private set; }
        public bool Sound { get; set; }
        public HighScoreManager HighScoreManager { get; } = new HighScoreManager();

        /// <param name="size">The size of the area the board is drawn in.</param>
        public void Initialize(Size size)
        {
            Surface = new Bitmap(size.Width, size.Height);
            device = Graphics.FromImage(Surface);

            // The .wav must be an EmbeddedResource in the csproj. A SoundPlayer with no stream plays
            // the Windows default beep, so stay silent if the resource is missing.
            var soundStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("MahjongSpriteVersion.192277__lebcraftlp__click.wav");
            if (soundStream != null)
            {
                clickSound = new SoundPlayer(soundStream);
                clickSound.Load();
            }
        }

        public void New(LayoutDefinition layout)
        {
            Current = new MahjongGame(layout);
            selectedTile = null;
            FitToSurface(layout);
        }

        public void Tick() => Current.Tick();

        public Bitmap Render()
        {
            device.ResetTransform();
            device.Clear(Color.Transparent);
            device.TranslateTransform(offset.X, offset.Y);
            device.ScaleTransform(scale, scale);
            device.InterpolationMode = scale < 1f ? InterpolationMode.HighQualityBicubic : InterpolationMode.Default;
            foreach (var tile in InDrawOrder())
            {
                var image = images.Get(tile.Face.Name, tile == selectedTile);
                var location = TileGeometry.Location(tile.Position);
                device.DrawImage(image, location.X, location.Y);
            }

            return Surface;
        }

        public void HandleClick(Point point)
        {
            // Undo the centring and scaling so the point is in the same units as the tile bounds.
            point = new Point((int)((point.X - offset.X) / scale), (int)((point.Y - offset.Y) / scale));

            // The tile drawn last is the one on top, so hit-test in reverse draw order.
            var clicked = InDrawOrder().LastOrDefault(t => TileGeometry.Bounds(t.Position).Contains(point));
            if (clicked == null || !Current.Board.IsFree(clicked))
            {
                return;
            }

            if (Sound)
            {
                clickSound?.Play();
            }

            if (selectedTile == null)
            {
                selectedTile = clicked;
            }
            else if (selectedTile == clicked)
            {
                selectedTile = null;
            }
            else if (Current.TryRemovePair(selectedTile, clicked))
            {
                selectedTile = null;
                TilesRemoved?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                selectedTile = clicked;
            }
        }

        public void Shuffle()
        {
            selectedTile = null;
            Current.Shuffle();
        }

        public void Undo()
        {
            selectedTile = null;
            Current.Undo();
        }

        public void Redo()
        {
            selectedTile = null;
            Current.Redo();
        }

        public void Dispose()
        {
            images.Dispose();
            clickSound?.Dispose();
            device?.Dispose();
            Surface?.Dispose();
        }

        private void FitToSurface(LayoutDefinition layout)
        {
            var bounds = TileGeometry.Bounds(layout.Positions);
            scale = Math.Min(1f, Math.Min((float)Surface.Width / bounds.Width, (float)Surface.Height / bounds.Height));
            offset = new PointF(
                (Surface.Width - bounds.Width * scale) / 2 - bounds.Left * scale,
                (Surface.Height - bounds.Height * scale) / 2 - bounds.Top * scale);
        }

        private IEnumerable<Tile> InDrawOrder()
        {
            // Each tile image overlaps its right and lower neighbours' space only with its side and
            // bottom edges, so a tile must be drawn after the tiles to its left and above it.
            // Sorting by row alone gets half-row offsets wrong: a tile half a row higher but to the
            // right would be drawn first and have its face covered. X + Y orders every overlapping
            // neighbour correctly, including the half-tile offsets.
            return Current.Board.Tiles
                .OrderBy(t => t.Position.Z)
                .ThenBy(t => t.Position.X + t.Position.Y)
                .ThenBy(t => t.Position.X);
        }
    }
}
