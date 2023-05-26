using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace Mahjong
{
    public class Tile : Control
    {
        Bitmap m_bmp;
        bool selected = false;
        Layout tileLayout;
        
        public Coordinate Coordinate { get; set; }
        public Guid Id { get; private set; }
        public TileStyle Style { get; set; }
        public int ShuffleIndex { get; set; }
        public bool Shuffled { get; set; }

        public Tile(Layout layout, Coordinate coordinate, TileStyle style) {
            Width = 42;
            Height = 54;

            tileLayout = layout;
            this.Coordinate = coordinate;
            Style = style;
            CreateMemoryBitmap(style.Name);
            Id = Guid.NewGuid();
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, true);
            this.BackColor = Color.Transparent;
            
        }

        public void Deselect() {

            if (selected) {
                SetBrightness(50);
                selected = false;
                Invalidate();
                Update();
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            if (Shuffled) {
                Shuffled = false;
                selected = false;
                CreateMemoryBitmap(Style.Name);
            }

            m_bmp.MakeTransparent();
            m_bmp.SetResolution(96, 96);
            e.Graphics.DrawImage(m_bmp, 0, 0);

            base.OnPaint(e);
        }

        private void CreateMemoryBitmap(string rsrcName) {
            m_bmp = new Bitmap(
                System.Reflection.Assembly.GetEntryAssembly().
                GetManifestResourceStream(rsrcName));
            this.Width = m_bmp.Width;
            this.Height = m_bmp.Height;
        }

        private void SetBrightness(int brightness) {
            Bitmap temp = m_bmp;
            Bitmap bmap = (Bitmap)temp.Clone();
            if (brightness < -255) brightness = -255;
            if (brightness > 255) brightness = 255;
            Color c;
            for (int i = 0; i < bmap.Width; i++) {
                for (int j = 0; j < bmap.Height; j++) {
                    c = bmap.GetPixel(i, j);
                    int cR = c.R + brightness;
                    int cG = c.G + brightness;
                    int cB = c.B + brightness;

                    if (cR < 0) cR = 1;
                    if (cR > 255) cR = 255;

                    if (cG < 0) cG = 1;
                    if (cG > 255) cG = 255;

                    if (cB < 0) cB = 1;
                    if (cB > 255) cB = 255;

                    bmap.SetPixel(i, j,
        Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
                }
            }
            m_bmp = (Bitmap)bmap.Clone();
        }

        protected override void OnClick(EventArgs e) {
            if (!tileLayout.IsBlocked(Coordinate)) {
                if (selected) {
                    SetBrightness(50);
                    selected = false;
                }
                else {
                    SetBrightness(-50);
                    selected = true;
                }

                Invalidate();
                Update();

                base.OnClick(e);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent) {
            base.OnPaintBackground(pevent);
        }

        //protected override void OnBackColorChanged(EventArgs e) {
        //    if (this.Parent != null) {
        //        Parent.Invalidate(this.Bounds, true);
        //    }
        //    base.OnBackColorChanged(e);
        //}
        
	


        //public int Opacity
        //{
        //    get
        //    {
        //        if (m_opacity > 100)
        //        {
        //            m_opacity = 100;
        //        }
        //        else if (m_opacity < 1)
        //        {
        //            m_opacity = 1;
        //        }
        //        return this.m_opacity;
        //    }
        //    set
        //    {
        //        this.m_opacity = value;
        //        if (this.Parent != null)
        //        {
        //            Parent.Invalidate(this.Bounds, true);
        //        }
        //    }
        //}

        //protected override CreateParams CreateParams {
        //    get {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle = cp.ExStyle | 0x20;
        //        return cp;
        //    }
        //}
    }
}
