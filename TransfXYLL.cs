using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MapnikTileRenderer
{
    public partial class TransfXYLL : Form
    {
        public TransfXYLL()
        {
            InitializeComponent();
        }

        private void xx_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void mtx_KeyUp(object sender, KeyEventArgs e)
        {
            System.Globalization.CultureInfo stream_CultureInfo = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)stream_CultureInfo.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            byte skip = 0;

            if ((sender == pxx) || (sender == pxy))
            {                
                int x = 0;
                int.TryParse(pxx.Text, out x);
                int y = 0;
                int.TryParse(pxy.Text, out y);
                Point t = new Point();
                try
                {
                    t = TileRendering.TilesProjection.fromPixelToTile(x, y);
                }
                catch { };

                xx.Text = String.Format("{0:0}", t.X);
                yy.Text = String.Format("{0:0}", t.Y);

                skip = 2;
                sender = xx;
            };

            if ((sender == mtx) || (sender == mty))
            {
                int x = 0;
                int.TryParse(mtx.Text, out x);
                int y = 0;
                int.TryParse(mty.Text, out y);
                int z = 0;
                int.TryParse(zz.Text, out z);
                Point t = new Point();
                try
                {
                    t = TileRendering.TilesProjection.fromMeterToTile(x, y, z);
                }
                catch { };

                xx.Text = String.Format("{0:0}", t.X);
                yy.Text = String.Format("{0:0}", t.Y);

                skip = 3;
                sender = xx;
            };

            if ((sender == lat) || (sender == lon))
            {
                double x = 0;
                try { x = double.Parse(lon.Text, ni); } catch { };
                double y = 0;
                try { y = double.Parse(lat.Text, ni); } catch { };
                int z = 0;
                int.TryParse(zz.Text, out z);
                PointF t = new PointF();
                try
                {
                    t = TileRendering.TilesProjection.fromLLToTile(y, x, z);
                }
                catch { };

                xx.Text = String.Format("{0:0}", t.X);
                yy.Text = String.Format("{0:0}", t.Y);

                skip = 4;
                sender = xx;
            };

            if ((sender == latc) || (sender == lonc))
            {
                double x = 0;
                try { x = double.Parse(lonc.Text, ni); }
                catch { };
                double y = 0;
                try { y = double.Parse(latc.Text, ni); }
                catch { };
                int z = 0;
                int.TryParse(zz.Text, out z);
                PointF t = new PointF();
                try
                {
                    t = TileRendering.TilesProjection.fromLLToTile(y, x, z);
                }
                catch { };

                xx.Text = String.Format("{0:0}", t.X);
                yy.Text = String.Format("{0:0}", t.Y);

                skip = 5;
                sender = xx;
            };

            if ((sender == xx) || (sender == yy) || (sender == zz))
            {
                int x = 0;
                int.TryParse(xx.Text, out x);
                int y = 0;
                int.TryParse(yy.Text, out y);
                int z = 0;
                int.TryParse(zz.Text, out z);                                               

                if (skip != 2)
                {
                    PointF px = new PointF();
                    try
                    {
                        px = TileRendering.TilesProjection.fromTileToPixel(x, y);
                    }
                    catch { };
                    pxx.Text = String.Format("{0:0}", px.X);
                    pxy.Text = String.Format("{0:0}", px.Y);
                };

                if (skip != 3)
                {
                    PointF pmx = new PointF();
                    try
                    {
                        pmx = TileRendering.TilesProjection.fromTileToMeter(x, y, z);
                    }
                    catch { };
                    mtx.Text = String.Format("{0:0}", pmx.X);
                    mty.Text = String.Format("{0:0}", pmx.Y);
                };

                if (skip != 4)
                {
                    PointF ll = new PointF();
                    try
                    {
                        ll = TileRendering.TilesProjection.fromTileToLL(x, y, z);
                    }
                    catch { };

                    lat.Text = ll.Y.ToString().Replace(",", ".");
                    lon.Text = ll.X.ToString().Replace(",", ".");
                };

                if (skip != 5)
                {
                    PointF llc = new PointF();
                    try
                    {
                        llc = TileRendering.TilesProjection.fromTileToLL(x + 0.5, y + 0.5, z);
                    }
                    catch { };

                    latc.Text = llc.Y.ToString().Replace(",", ".");
                    lonc.Text = llc.X.ToString().Replace(",", ".");
                };
            };
        }

        private void TransfXYLL_Load(object sender, EventArgs e)
        {
            mtx_KeyUp(xx, null);
        }
    }
}