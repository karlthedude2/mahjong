using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

namespace MahjongSpriteVersion
{
    /// <summary>Loads each tile face image once, along with a darker copy used to show selection.</summary>
    public sealed class TileImageCache : IDisposable
    {
        private const float SelectedDarkening = 50f / 255f;

        private readonly Dictionary<string, Bitmap> normal = new Dictionary<string, Bitmap>();
        private readonly Dictionary<string, Bitmap> selected = new Dictionary<string, Bitmap>();

        public Bitmap Get(string faceName, bool isSelected)
        {
            if (!normal.ContainsKey(faceName))
            {
                Load(faceName);
            }

            return isSelected ? selected[faceName] : normal[faceName];
        }

        public void Dispose()
        {
            foreach (var bitmap in normal.Values)
            {
                bitmap.Dispose();
            }

            foreach (var bitmap in selected.Values)
            {
                bitmap.Dispose();
            }

            normal.Clear();
            selected.Clear();
        }

        private void Load(string faceName)
        {
            string resource = "MahjongSpriteVersion.Graphics." + faceName + ".png";
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            if (stream == null)
            {
                throw new InvalidOperationException("Missing tile image " + resource);
            }

            Bitmap image;
            using (stream)
            using (var loaded = new Bitmap(stream))
            {
                image = new Bitmap(loaded);
            }

            // The images are drawn at 72 dpi, which scales them up to the tile size on screen.
            image.SetResolution(72, 72);
            normal[faceName] = image;
            selected[faceName] = Darken(image);
        }

        private static Bitmap Darken(Bitmap source)
        {
            var result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            result.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            var matrix = new ColorMatrix(new[]
            {
                new float[] { 1, 0, 0, 0, 0 },
                new float[] { 0, 1, 0, 0, 0 },
                new float[] { 0, 0, 1, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { -SelectedDarkening, -SelectedDarkening, -SelectedDarkening, 0, 1 },
            });

            using (var attributes = new ImageAttributes())
            using (var g = Graphics.FromImage(result))
            {
                attributes.SetColorMatrix(matrix);
                g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                    0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
            }

            return result;
        }
    }
}
