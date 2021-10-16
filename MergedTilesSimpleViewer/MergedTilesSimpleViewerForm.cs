using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MergedTilesSimpleViewer
{
    public partial class MergedTilesSimpleViewerForm : Form
    {
        public MergedTilesSimpleViewerForm()
        {
            InitializeComponent();
            map.NotFoundTileColor = Color.LightYellow;
            map.OnMapUpdate += new NaviMapNet.NaviMapNetViewer.MapEvent(MapUpdate);
        }

        private void MapUpdate()
        {
            string lreq = map.LastRequestedFile;
            if (lreq.Length > 50) lreq = "... " + lreq.Substring(lreq.Length - 50);
            toolStripStatusLabel1.Text = "Last Requested File: " + lreq;
            toolStripStatusLabel2.Text = map.CenterDegreesLat.ToString().Replace(",", ".");
            toolStripStatusLabel3.Text = map.CenterDegreesLon.ToString().Replace(",", ".");
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            PointF m = map.MousePositionDegrees;
            toolStripStatusLabel4.Text = m.Y.ToString().Replace(",", ".");
            toolStripStatusLabel5.Text = m.X.ToString().Replace(",", ".");
        }

        private void MergedTilesSimpleViewerForm_Load(object sender, EventArgs e)
        {
            LoadMapLocal(true);
        }

        private void LoadMapLocal(bool firstRun)
        {
            string cd = GetCurrentDir(); // @"E:\_RENDER_MAP_TILES\";// 
            Text += " - " + cd;

            map.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.Custom_LocalFiles;
            map.ImageSourceUrl = cd + @"\_alllayers\L{l}\R{r}\C{c}.png";

            if (firstRun)
            {
                // OPEN MAP AT CENTER
                // TRY OPEN SHAPEFILE
                bool shapeOk = false;
                {
                    string shpfile = cd + @"\bounds.shp";
                    if (System.IO.File.Exists(shpfile))
                    {
                        Polygon Area = readShapeFile(shpfile);
                        double cx = (Area.box[0] + Area.box[2]) / 2.0;
                        double cy = (Area.box[3] + Area.box[1]) / 2.0;
                        map.CenterDegrees = new PointF((float)cx, (float)cy);
                        shapeOk = true;
                        //

                        NaviMapNet.MapLayer borderLayer = new NaviMapNet.MapLayer("borderLayer");
                        map.MapLayers.Clear();
                        map.MapLayers.Add(borderLayer);

                        int[] pFrom = new int[Area.numParts + 1];
                        pFrom[pFrom.Length - 1] = Area.numPoints;
                        for (int i = 0; i < Area.numParts; i++) pFrom[i] = Area.parts[i];
                        for (int i = 0; i < Area.numParts; i++)
                        {
                            List<PointF> pts = new List<PointF>();
                            for (int l = pFrom[i]; l < pFrom[i + 1]; l++) pts.Add(Area.points[l]);
                            NaviMapNet.MapPolyLine mpl = new NaviMapNet.MapPolyLine(pts.ToArray());
                            mpl.Width = 3;
                            mpl.Color = defBColor;
                            mpl.DrawEvenSizeIsTooSmall = true;
                            borderLayer.Add(mpl);
                        };

                        cbcds.Visible = true;
                        cbcd.Visible = true;
                    };
                };
                // TRY OPEN FIRST TILE
                if ((!shapeOk) && (System.IO.Directory.Exists(cd + @"\_alllayers\L00\")))
                {
                    string[] dirs = System.IO.Directory.GetDirectories(cd + @"\_alllayers\L00\");
                    if (dirs.Length > 0)
                    {
                        string r = dirs[0].Substring(dirs[0].LastIndexOf("\\") + 2);
                        int y = int.Parse(r, System.Globalization.NumberStyles.HexNumber);
                        dirs = System.IO.Directory.GetFiles(dirs[0] + @"\", "*.png");
                        if (dirs.Length > 0)
                        {
                            string c = dirs[0].Substring(dirs[0].LastIndexOf("\\") + 2);
                            c = c.Remove(c.IndexOf("."));
                            int x = int.Parse(c, System.Globalization.NumberStyles.HexNumber);

                            if ((x <= 16) && (y <= 16))
                                map.ZoomID = 4;
                            else
                            {
                                map.ZoomID = 10;
                                map.CenterPixels = new Point(x * 256 + 128, y * 256 + 128);
                            };
                        };
                    };
                };

                map.DrawMap = true;
            }
            else
                map.ReloadMap();

            toolStripMenuItem1.Checked = true;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem2.Checked = false;
        }

        private void LoadMapOSM()
        {
            map.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.OSM_Mapnik;
            toolStripMenuItem1.Checked = false;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem2.Checked = true;            
        }

        private void LoadMapNavicom()
        {
            map.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.Navicom_Tiles;
            toolStripMenuItem1.Checked = false;
            toolStripMenuItem3.Checked = true;
            toolStripMenuItem2.Checked = false;

        }

        #region read shape 1st record
        private Polygon readShapeFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            long fileLength = fs.Length;
            Byte[] data = new Byte[fileLength];
            fs.Read(data, 0, (int)fileLength);
            fs.Close();
            int shapetype = readIntLittle(data, 32);
            if (shapetype != 5) return new Polygon();
            int currentPosition = 100;

            int recordStart = currentPosition;
            int recordNumber = readIntBig(data, recordStart);
            int contentLength = readIntBig(data, recordStart + 4);
            int recordContentStart = recordStart + 8;

            Polygon Area = new Polygon();
            int recordShapeType = readIntLittle(data, recordContentStart);
            Area.box = new Double[4];
            // NORMAL //

            Area.box[0] = readDoubleLittle(data, recordContentStart + 4);
            Area.box[1] = readDoubleLittle(data, recordContentStart + 12);
            Area.box[2] = readDoubleLittle(data, recordContentStart + 20);
            Area.box[3] = readDoubleLittle(data, recordContentStart + 28);

            // TRUE
            /*
            Area.box[0] = double.MaxValue;
            Area.box[1] = double.MaxValue;
            Area.box[2] = double.MinValue;
            Area.box[3] = double.MinValue;
             */
            //
            Area.numParts = readIntLittle(data, recordContentStart + 36);
            Area.parts = new int[Area.numParts];
            Area.numPoints = readIntLittle(data, recordContentStart + 40);
            Area.points = new PointF[Area.numPoints];
            int partStart = recordContentStart + 44;
            for (int i = 0; i < Area.numParts; i++)
            {
                Area.parts[i] = readIntLittle(data, partStart + i * 4);
            };
            int pointStart = recordContentStart + 44 + 4 * Area.numParts;
            for (int i = 0; i < Area.numPoints; i++)
            {
                Area.points[i].X = (float)readDoubleLittle(data, pointStart + (i * 16));
                Area.points[i].Y = (float)readDoubleLittle(data, pointStart + (i * 16) + 8);
                // TRUE
                /*
                if (Area.points[i].X < Area.box[0]) Area.box[0] = Area.points[i].X;
                if (Area.points[i].Y < Area.box[1]) Area.box[1] = Area.points[i].Y;
                if (Area.points[i].X > Area.box[2]) Area.box[2] = Area.points[i].X;
                if (Area.points[i].Y > Area.box[3]) Area.box[3] = Area.points[i].Y;
                 */
                //
            };

            return Area;
        }

        private int readIntBig(byte[] data, int pos)
        {
            byte[] bytes = new byte[4];
            bytes[0] = data[pos];
            bytes[1] = data[pos + 1];
            bytes[2] = data[pos + 2];
            bytes[3] = data[pos + 3];
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        private int readIntLittle(byte[] data, int pos)
        {
            byte[] bytes = new byte[4];
            bytes[0] = data[pos];
            bytes[1] = data[pos + 1];
            bytes[2] = data[pos + 2];
            bytes[3] = data[pos + 3];
            return BitConverter.ToInt32(bytes, 0);
        }

        private double readDoubleLittle(byte[] data, int pos)
        {
            byte[] bytes = new byte[8];
            bytes[0] = data[pos];
            bytes[1] = data[pos + 1];
            bytes[2] = data[pos + 2];
            bytes[3] = data[pos + 3];
            bytes[4] = data[pos + 4];
            bytes[5] = data[pos + 5];
            bytes[6] = data[pos + 6];
            bytes[7] = data[pos + 7];
            return BitConverter.ToDouble(bytes, 0);
        }

        public struct Polygon
        {
            public double[] box;
            public int numParts;
            public int numPoints;
            public int[] parts;
            public PointF[] points;
        }
        #endregion            


        public static string GetCurrentDir()
        {
            string fname = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.ToString();
            fname = fname.Replace("file:///", "");
            fname = fname.Replace("/", @"\");
            fname = fname.Substring(0, fname.LastIndexOf(@"\") + 1);
            return fname;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            LoadMapLocal(false);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            LoadMapOSM();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            LoadMapNavicom();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            map.DrawTilesBorder = !map.DrawTilesBorder;
            toolStripMenuItem4.Checked = map.DrawTilesBorder;
            map.ReloadMap();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            map.DrawTilesXYZ = !map.DrawTilesXYZ;
            toolStripMenuItem5.Checked = map.DrawTilesXYZ;
            map.ReloadMap();
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            map.TilesRenderingZoneSize = ((short)(toolStripComboBox1.SelectedIndex + 1));
            if (map.DrawTilesBorder) map.ReloadMap();
        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            double lat = map.CenterDegreesLat;
            double lon = map.CenterDegreesLon;
            if (NaviMapNet.NaviMapNetViewer.InputLatLon("Навигация по карте", "Введите координаты центра карты:", ref lat, ref lon) == DialogResult.OK)
                map.CenterDegrees = new PointF((float)lon, (float)lat);
        }

        private static string TwoZ(int val)
        {
            return val.ToString().Length > 1 ? val.ToString() : "0" + val.ToString();
        }
        private static string ToHex8(int val)
        {
            string res = val.ToString("X");
            while (res.Length < 8) res = "0" + res;
            return res;
        }

        private void toolStripDropDownButton1_DropDownOpening(object sender, EventArgs e)
        {
            toolStripMenuItem4.Checked = map.DrawTilesBorder;
            toolStripMenuItem5.Checked = map.DrawTilesXYZ;
        }

        private void map_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private Color defBColor = Color.Purple;
        private void cbcd_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = defBColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                defBColor = cd.Color;
                NaviMapNet.MapLayer ml = map.MapLayers[0];
                for (int i = 0; i < ml.ObjectsCount; i++)
                    ml[i].BodyColor = defBColor;
                map.DrawOnMapData();
            };
            cd.Dispose();
        }
    }
}