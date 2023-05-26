using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace MahjongSpriteVersion
{
    public class TileSprite
    {
        Bitmap m_bmp;
        Bitmap m_OriginalBmp;
        Bitmap m_SelectedBmp;
        bool selected = false;

        public Coordinate Coordinate { get; set; }
        public Guid Id { get; private set; }
        public TileStyle Style { get; set; }
        public int ShuffleIndex { get; set; }
        public bool Shuffled { get; set; }
        public Point Location { get; set; }
        public Layout TileLayout { get; set; }

        public TileSprite(Layout layout, Coordinate coordinate, TileStyle style) {

            TileLayout = layout;
            this.Coordinate = coordinate;
            this.Coordinate.bitmapName = style.Name;
            Style = style;
            CreateMemoryBitmap(style.Name);
            Id = Guid.NewGuid();
            
        }

        public void Select()
        {

            if (!selected)
            {
                m_bmp.Dispose();
                m_bmp = new Bitmap(m_SelectedBmp);
                selected = true;
            }
        }

        public void Deselect() {

            if (selected) {
                m_bmp.Dispose();
                m_bmp = new Bitmap(m_OriginalBmp);
                selected = false;
            }
        }

        public void Draw(Graphics g) {
            if (Shuffled) {
                Shuffled = false;
                selected = false;
                CreateMemoryBitmap(Style.Name);
            }

            //m_bmp.SetResolution(96, 96);
            m_bmp.SetResolution(72, 72);
            g.DrawImage(m_bmp, Location.X, Location.Y);

        }

        public bool IsToRightOrToBottomOf(TileSprite tile)
        {
            bool x = tile.Coordinate.X < this.Coordinate.X;
            bool y = tile.Coordinate.Y < this.Coordinate.Y;

            return x  ||  y;
        }



        private void CreateMemoryBitmap(string rsrcName) {
            m_bmp = new Bitmap(
                System.Reflection.Assembly.GetEntryAssembly().
                GetManifestResourceStream(rsrcName));
            m_OriginalBmp = new Bitmap(m_bmp);
            m_SelectedBmp = new Bitmap(m_bmp);
            DarkenSelectedBitmap();
        }

        //private void SetBrightness(int brightness) {
        //    Bitmap temp = m_bmp;
        //    Bitmap bmap = (Bitmap)temp.Clone();
        //    if (brightness < -255) brightness = -255;
        //    if (brightness > 255) brightness = 255;
        //    Color c;
        //    for (int i = 0; i < bmap.Width; i++) {
        //        for (int j = 0; j < bmap.Height; j++) {
        //            c = bmap.GetPixel(i, j);

        //            if (c.A != 0)  {
        //                int cR = c.R + brightness;
        //                int cG = c.G + brightness;
        //                int cB = c.B + brightness;

        //                if (cR < 0) cR = 1;
        //                if (cR > 255) cR = 255;

        //                if (cG < 0) cG = 1;
        //                if (cG > 255) cG = 255;

        //                if (cB < 0) cB = 1;
        //                if (cB > 255) cB = 255;

        //                bmap.SetPixel(i, j, Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
        //            }
        //        }
        //    }
        //    m_bmp = (Bitmap)bmap.Clone();
        //}

        private void DarkenSelectedBitmap()
        {
            Color c;
            for (int i = 0; i < m_SelectedBmp.Width; i++)
            {
                for (int j = 0; j < m_SelectedBmp.Height; j++)
                {
                    c = m_SelectedBmp.GetPixel(i, j);

                    if (c.A != 0)
                    {
                        int cR = c.R - 50;
                        int cG = c.G - 50;
                        int cB = c.B - 50;

                        if (cR < 0) cR = 1;
                        if (cR > 255) cR = 255;

                        if (cG < 0) cG = 1;
                        if (cG > 255) cG = 255;

                        if (cB < 0) cB = 1;
                        if (cB > 255) cB = 255;

                        m_SelectedBmp.SetPixel(i, j, Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
                    }
                }
            }
        }

        public void OnClick() {
            if (!TileLayout.IsBlocked(Coordinate)) {
                if (selected) {
                    Deselect();
                }
                else {
                    Select();
                }


            }
        }
    }
}
