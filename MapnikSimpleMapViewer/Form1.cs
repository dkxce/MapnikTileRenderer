#define _IGOR

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;
using System.Drawing.Text;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Web;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace MapnikSimpleMapCreator
{
    public partial class Form1 : Form
    {
        static string title = "Mapnik Simple Map Viewer v0.3.1.7.0b";

        private System.Globalization.CultureInfo stream_CultureInfo = System.Globalization.CultureInfo.InstalledUICulture;
        private System.Globalization.NumberFormatInfo ni;

        private string MapnikFontList = "";

        bool GlobalMouseImageMove = false;
        bool GlobalMouseOverImage = false;
        Point GlobalMousePos = new Point();
        Point GlobalMouseDown = new Point();
        Point GlobalMouseLoc = new Point();

        MapnikCs.Projection GlobalMapProj = new MapnikCs.Projection("+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs");
        MapnikCs.Map GlobalMap = null;
        string GlobalMapFile = "";

        public Form1()
        {            
            InitializeComponent();
            MouseMessageFilter mmf = new MouseMessageFilter();
            Application.AddMessageFilter(mmf);
            MouseMessageFilter.MouseMove += new MouseEventHandler(OnGlobalMouseMove);
            MouseMessageFilter.MouseDown += new MouseEventHandler(OnGlobalMouseDown);
            MouseMessageFilter.MouseUp += new MouseEventHandler(OnGlobalMouseUp);
            this.MouseWheel += new MouseEventHandler(onImageMouseWheel);

            #if IGOR
            toolStripMenuItem3.Visible = false;
            svrMnu.Visible = false;
            smm.Visible = false;
            ssm.Visible = false;
            #endif

            Text = title;
            iZooms.SelectedIndex = 9;            

            ni = (System.Globalization.NumberFormatInfo)stream_CultureInfo.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            GlobalMouseWheelRunning = true;
            GlobalMouseWheelDelay = new Thread(new ThreadStart(DelayMouseWheel));
            GlobalMouseWheelDelay.Start();
        }               

        private void OnGlobalMouseMove(object sender, MouseEventArgs e) 
        {
            GlobalMousePos = e.Location;
            Point cp = panel1.PointToClient(e.Location);
            if (GlobalMouseImageMove)
            {
                int _ms_x = GlobalMouseDown.X - GlobalMousePos.X;
                int _ms_y = GlobalMouseDown.Y - GlobalMousePos.Y;
                pictureBox1.Left = -1 * _ms_x + GlobalMouseLoc.X;
                pictureBox1.Top = -1 * _ms_y + GlobalMouseLoc.Y;
                return;
            };

            if ((_current_map_resolution != 0) && (cp.X > 0) && (cp.Y > 0) && (cp.X < panel1.Width) && (cp.Y < panel1.Height))
            {
                int _ms_x = cp.X - pictureBox1.Width / 2;
                int _ms_y = pictureBox1.Height / 2 - cp.Y;
                double dX = _current_map_bounds.Width / (double)pictureBox1.Width;
                double dY = _current_map_bounds.Height / (double)pictureBox1.Height;
                MapnikCs.Coord center = _current_map_bounds.Center;
                center.X -= dX * _ms_x;
                center.Y += dY * _ms_y;

                MapnikCs.Projection proj = new MapnikCs.Projection("+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs");
                MapnikCs.Coord cursorOver = proj.Inverse(center);

                Text = String.Format("{0}  -  {2:0.000000}  {1:0.000000}", new object[] { title, cursorOver.X, cursorOver.Y }).Replace(",", ".");
            };
        }

        private void OnGlobalMouseDown(object sender, MouseEventArgs e) {  }

        private void OnGlobalMouseUp(object sender, MouseEventArgs e)
        {
            if (GlobalMouseImageMove)
            {
                GlobalMouseImageMove = false;
                if ((Math.Abs(pictureBox1.Top) < 35) && (Math.Abs(pictureBox1.Left) < 35)) // Случайный
                {
                    pictureBox1.Left = 0;
                    pictureBox1.Top = 0;
                    return;
                };

                double dX = _current_map_bounds.Width / (double)pictureBox1.Width;
                double dY = _current_map_bounds.Height / (double)pictureBox1.Height;
                MapnikCs.Coord center = _current_map_bounds.Center;
                center.X -= dX * pictureBox1.Location.X;
                center.Y += dY * pictureBox1.Location.Y;

                MapnikCs.Projection proj = new MapnikCs.Projection("+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs");
                MapnikCs.Coord moveTo = proj.Inverse(center);

                iStat.Text = String.Format("{0}  -  {2:0.000000}  {1:0.000000}", new object[] { title, moveTo.X, moveTo.Y }).Replace(",", ".");

                isX.Text = moveTo.X.ToString().Replace(",", ".");
                isY.Text = moveTo.Y.ToString().Replace(",", ".");
                iZooms.SelectedIndex = _current_map_zoom - 1;
                isX.Refresh();
                isY.Refresh();
                iZooms.Refresh();

                button2_Click(sender, null);                
            };
        }

        private DateTime GlobalMouseWheeled = DateTime.MaxValue;
        private Thread GlobalMouseWheelDelay = null;
        private bool GlobalMouseWheelRunning = false;
        private int GlobalMouseWheelIncrement = 0;
        private void onImageMouseWheel(object sender, MouseEventArgs e)
        {
            Point cp = panel1.PointToClient(GlobalMousePos);
            if ((GlobalMouseWheelIncrement < 3) && (_current_map_resolution != 0) && (!GlobalMouseImageMove) && GlobalMouseOverImage)
            {
                int inc = e.Delta > 0 ? 1 : -1;
                int nzli = (_current_map_zoom - 1 + inc);
                if (nzli < 0) return;
                if (nzli >= iZooms.Items.Count) return;

                double current_res = 40075000.0 / Math.Pow(2, _current_map_zoom - 1 + 9);
                double new_res = 40075000.0 / Math.Pow(2, nzli + 9);

                int _ms_x = cp.X - pictureBox1.Width / 2;
                int _ms_y = pictureBox1.Height / 2 - cp.Y;
                MapnikCs.Coord centerPoint = _current_map_bounds.Center;
                centerPoint.X += (inc > 0) ? current_res * _ms_x - new_res * _ms_x : current_res * _ms_x - new_res * _ms_x;
                centerPoint.Y += (inc > 0) ? current_res * _ms_y - new_res * _ms_y : current_res * _ms_y - new_res * _ms_y;
                _current_map_bounds.Center = centerPoint;

                MapnikCs.Projection proj = new MapnikCs.Projection("+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs");
                MapnikCs.Coord newZoomCenter = proj.Inverse(centerPoint);

                isX.Text = newZoomCenter.X.ToString().Replace(",", ".");
                isY.Text = newZoomCenter.Y.ToString().Replace(",", ".");
                iZooms.SelectedIndex = nzli;
                _current_map_zoom = nzli + 1;
                isX.Refresh();
                isY.Refresh();
                iZooms.Refresh();

                GlobalMouseWheeled = DateTime.UtcNow;
                GlobalMouseWheelIncrement++;               
            };
        }
        private void DelayMouseWheel()
        {
            while (GlobalMouseWheelRunning)
            {
                TimeSpan ts = DateTime.UtcNow.Subtract(GlobalMouseWheeled);
                if (ts.TotalMilliseconds > 250)
                {
                    GlobalMouseWheelIncrement = 0;
                    GlobalMouseWheeled = DateTime.MaxValue;
                    this.Invoke(new ThreadStart(callWheelThread), null);
                };
                Thread.Sleep(25);
            };
        }
        private void callWheelThread()
        {
            button2_Click(this, null);
        }
        

        private void Form1_Load(object sender, EventArgs e)
        {
            string cd = GetCurrentDir();
            MapnikCs.DatasourceCache.RegisterDatasources(cd + @"data_sources\");
            MapnikCs.FontEngine.RegisterFonts(cd + @"\fonts\",false);
            MapnikFontList = SaveFontList();

            app_texts_data d = app_texts_data.Load();

            isX.Text = d.center[0];
            isY.Text = d.center[1];

            iMapFileName.Text = d.map;

            iZooms.SelectedIndex = d.zoom - 1;
            iZooms_SelectedIndexChanged(sender, e);
        }

        /// <summary>
        ///     Exe path
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDir()
        {
            string cd = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.ToString();
            cd = cd.Replace("file:///", "");
            cd = cd.Replace("/", @"\");
            cd = cd.Substring(0, cd.LastIndexOf(@"\") + 1);
            return cd;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".xml";
            ofd.Filter = "XML Files (*.xml)|*.xml|All types (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                iMapFileName.Text = ofd.FileName;
                _current_map_resolution = 0;
                iMapFileName.Enabled = true;
                pictureBox1.Image = null;
            };
            ofd.Dispose();            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            iMapFileName.Text = "";
            iMapFileName.Enabled = true;
        }

        double _current_map_resolution = 0.0;
        MapnikCs.Envelope _current_map_bounds = new MapnikCs.Envelope();
        int _current_map_zoom = 10;
        private void button2_Click(object sender, EventArgs e)
        {
            if ((iMapFileName.Text == "") || (!System.IO.File.Exists(iMapFileName.Text))) { MessageBox.Show("Не настроены параметры карты!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information); return; };
            iMapFileName.Enabled = false;
            iMapFileName.Refresh();

            pictureBox1.Cursor = Cursors.Hand;

            rLabel.Visible = true;
            rLabel.Refresh();
           
            if ((GlobalMap == null) || (GlobalMapFile != iMapFileName.Text))
            {
                GlobalMap = new MapnikCs.Map(800, 700, "+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs");
                GlobalMap.LoadFile(GlobalMapFile = iMapFileName.Text);
            };

            double sx; double sy;
            double.TryParse(isX.Text, System.Globalization.NumberStyles.Float, ni, out sx);
            double.TryParse(isY.Text, System.Globalization.NumberStyles.Float, ni, out sy);

            int zoom = iZooms.SelectedIndex + 1;
            double res = 40075000.0 / Math.Pow(2, zoom + 8);
            double scale = 559082264 / Math.Pow(2, zoom);

            MapnikCs.Coord center = GlobalMapProj.Forward(new MapnikCs.Coord(sx, sy));
            MapnikCs.Coord[] box = calcBounds(center, res, 800, 700);
            MapnikCs.Coord start = GlobalMapProj.Inverse(box[0]);
            MapnikCs.Coord end = GlobalMapProj.Inverse(box[1]);

            MapnikCs.Coord c0 = GlobalMapProj.Forward(start);
            MapnikCs.Coord c1 = GlobalMapProj.Forward(end);
            MapnikCs.Envelope bbox = new MapnikCs.Envelope(c0.X, c0.Y, c1.X, c1.Y);                
            GlobalMap.ZoomToBox(bbox);

            MapnikCs.Image im = new MapnikCs.Image(GlobalMap.Width, GlobalMap.Height);
            GlobalMap.Render(im);            

            rLabel.Visible = false;
            pictureBox1.Image = im.ToImage();
            pictureBox1.Left = 0;
            pictureBox1.Top = 0;

            _current_map_resolution = GlobalMap.Scale;
            _current_map_bounds = GlobalMap.Extent;
            _current_map_zoom = zoom;

            iStat.Text = String.Format("Rendered Map >> ScaleDenominator: {0} Resolution: {1:0.0000} Zoom: {2}", (int)GlobalMap.ScaleDenominator, GlobalMap.Scale, _current_map_zoom).Replace(",", ".");
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            string pngFileName = GetCurrentDir() + @"\rendered_file.png";
            System.Diagnostics.Process.Start(pngFileName);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (svrMnu.Checked)
            {
                if (MessageBox.Show("Нельзя закрыть приложение, пока запущен Simple WMS HTTP Server\r\nОстановить сервер и закрыть приложение?", "Закрыть приложение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    svrMnu_Click(null, null);

                    app_texts_data d = new app_texts_data(new string[] { isX.Text, isY.Text }, iMapFileName.Text, iZooms.SelectedIndex + 1);
                    d.Save();

                    return;
                }
                e.Cancel = true;
                return;
            };

            {
                app_texts_data d = new app_texts_data(new string[] { isX.Text, isY.Text }, iMapFileName.Text, iZooms.SelectedIndex + 1);
                d.Save();
            };
        }

        private string SaveFontList()
        {
            string s = "";
            string sl = "";
            string pb = "";
            for (int i = 0; i < MapnikCs.FontEngine.FaceNames.Count; i++)
            {
                string cb = MapnikCs.FontEngine.FaceNames[i].Substring(0, 1);
                if (cb != pb)
                    s += "\r\n";
                else if (i > 0)
                    s += ", ";
                pb = cb;
                s += MapnikCs.FontEngine.FaceNames[i];
                sl += MapnikCs.FontEngine.FaceNames[i] + "\r\n";
            };
            System.IO.FileStream fs = new FileStream(GetCurrentDir() + @"\fonts\_fontlist.txt", FileMode.Create, FileAccess.Write);
            byte[] bb = System.Text.Encoding.GetEncoding(1251).GetBytes(sl);
            fs.Write(bb, 0, bb.Length);
            fs.Close();
            return s;
        }

        WMSHTTPListener http_server = null;
        private void загрузитьНастройкиИзФайлаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "._msmc";
            ofd.Filter = "MSMC Config (*._msmc)|*._msmc";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                app_texts_data d = app_texts_data.Load(ofd.FileName);

                isX.Text = d.center[0];
                isY.Text = d.center[1];

                iMapFileName.Text = d.map;

                iZooms.SelectedIndex = d.zoom - 1;
                _current_map_resolution = 0;
                iMapFileName.Enabled = true;
                pictureBox1.Image = null;
            };
            ofd.Dispose();
        }

        private void сохранитьНайтсройкиВФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = "._msmc";
            sfd.Filter = "MSMC Config (*._msmc)|*._msmc";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                app_texts_data d = new app_texts_data(new string[] { isX.Text, isY.Text }, iMapFileName.Text, iZooms.SelectedIndex + 1);
                d.Save(sfd.FileName);
            };
            sfd.Dispose();
        }        

        private MapnikCs.Coord[] calcBounds(MapnikCs.Coord center, double res, int wi, int he) 
        {       
            double dx = ((double)wi * res) / 2;
            double dy = ((double)he * res) / 2;
            return new MapnikCs.Coord[]{new MapnikCs.Coord(center.X-dx,center.Y+dy), new MapnikCs.Coord(center.X+dx,center.Y-dy)};
        }

        private void svrMnu_Click(object sender, EventArgs e)
        {
            if (!svrMnu.Checked)
            {
                if ((iMapFileName.Text == "") || (!System.IO.File.Exists(iMapFileName.Text))) { MessageBox.Show("Не настроены параметры карты!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information); return; };

                http_server = new WMSHTTPListener(7759, 2);
                http_server.map = new MapnikCs.Map(256, 256, "+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs");
                //http_server.map.BufferSize = 128;
                http_server.map_proj = new MapnikCs.Projection(http_server.map.SpatialReference);
                http_server.map.LoadFile(iMapFileName.Text);
                http_server.Start();

                svrMnu.Checked = true;
                this.Icon = global::MapnikSimpleMapViewer.Properties.Resources.objectsIcon;
            }
            else
            {
                if (http_server != null)
                {
                    http_server.mapMtx.WaitOne();
                    http_server.map.Dispose();
                    http_server.map = null;
                    http_server.map_proj = null;
                    http_server.mapMtx.ReleaseMutex();
                    http_server.Stop();
                    http_server.Dispose();
                    http_server = null;

                    try
                    {
                        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
                    }
                    catch { };
                };
                svrMnu.Checked = false;
            };

            smm.Enabled = svrMnu.Checked;
            ssm.Enabled = svrMnu.Checked;
            sisr.Visible = svrMnu.Checked;            
        }

        private void iZooms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!iZooms.Visible) return;

            int zoom = iZooms.SelectedIndex + 1;
            double res = 40075000.0 / Math.Pow(2, zoom + 8);
            double scale = 559082264 / Math.Pow(2, zoom);

            iStat.Text = String.Format("Changed to >> ScaleDenominator: {0} Resolution: {1:0.0000} Zoom {2}", (int)scale, res, zoom).Replace(",",".");
        }

        private string GetDefaultBrowserPath()
        {
            string key = @"HTTP\shell\open\command";
            using (RegistryKey registrykey = Registry.ClassesRoot.OpenSubKey(key, false))
                return ((string)registrykey.GetValue(null, null)).Split('"')[1];
        }

        string DefBrowserPath = null;
        private void Navigate(string url)
        {
            if (DefBrowserPath == null) DefBrowserPath = GetDefaultBrowserPath();
            System.Diagnostics.Process.Start(DefBrowserPath, "file:///" + url);
        }

        private void smm_Click(object sender, EventArgs e)
        {
            Navigate(GetCurrentDir() + @"\httpClient\multi_tiles_map.html?{lat:" + isY.Text.Trim() + ",lon:" + isX.Text.Trim() + ",zoom:" + (iZooms.SelectedIndex + 1).ToString() + "}");
        }

        private void ssm_Click(object sender, EventArgs e)
        {
            Navigate(GetCurrentDir() + @"\httpClient\single_tile_map.html#map=1&lat=" + isY.Text.Trim() + "&lon=" + isX.Text.Trim() + "&zoom=" + (iZooms.SelectedIndex + 1).ToString() + "");
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (_current_map_resolution == 0) return;
            if(e.Button == MouseButtons.Left)
            {
                panel1.Select();
                GlobalMouseDown = GlobalMousePos;
                GlobalMouseLoc = pictureBox1.Location;
                GlobalMouseImageMove = true;
            };
        }

        private void viewFonts(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(GetCurrentDir() + @"\fonts\_fontlist.txt");
        }

        private void проверитьСинтаксисXMLФайлаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (iMapFileName.Text == "") return;
            if (!System.IO.File.Exists(iMapFileName.Text))
            {
                MessageBox.Show("Файл карты не найден!");
                return;
            };

            System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
            try
            {
                xd.Load(iMapFileName.Text);
                MessageBox.Show("Ошибок синтаксиса не выявлено", "Разбора XML файла карт", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка разбора XML файла карты", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            };
            xd = null;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            GlobalMouseOverImage = true;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            GlobalMouseOverImage = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalMouseWheelRunning = false;
            GlobalMouseWheelDelay.Join();
            GlobalMouseWheelDelay.Abort();
        }

        private void saveImage(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".png";
            sfd.Filter = "PNG Files (*.png)|*.png";
            if (sfd.ShowDialog() == DialogResult.OK)
                pictureBox1.Image.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
            sfd.Dispose();                        
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            savImg.Enabled = pictureBox1.Image != null;
        }

    }

    public class MouseMessageFilter : IMessageFilter
    {
        public static event MouseEventHandler MouseMove = delegate { };
        public static event MouseEventHandler MouseDown= delegate { };
        public static event MouseEventHandler MouseUp = delegate { };
        const int WM_MOUSEMOVE = 0x0200;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_MOUSEMOVE)
            {
                Point mousePosition = Control.MousePosition;
                MouseMove(null, new MouseEventArgs(
                    MouseButtons.None, 0, mousePosition.X, mousePosition.Y, 0));
            };
            if (m.Msg == WM_LBUTTONDOWN)
            {
                Point mousePosition = Control.MousePosition;
                MouseDown(null, new MouseEventArgs(
                    MouseButtons.None, 0, mousePosition.X, mousePosition.Y, 0));
            };
            if (m.Msg == WM_LBUTTONUP)
            {
                Point mousePosition = Control.MousePosition;
                MouseUp(null, new MouseEventArgs(
                    MouseButtons.None, 0, mousePosition.X, mousePosition.Y, 0));
            };
            return false;
        }
    }

    [Serializable]
    public class app_texts_data
    {
        public string[] center = new string[2];
        public string map = "";
        public int zoom = 10;

        [XmlIgnore]
        static string fileName = "";

        public app_texts_data() { }

        public app_texts_data(string[] center, string map, int zoom)
        {
            this.center = center;
            this.map = map;
            this.zoom = zoom;
        }

        static app_texts_data()
        {
            fileName = Form1.GetCurrentDir() + @"\Viewer.cfg";
        }

        /// <summary>
        ///     Загружаем из файла
        /// </summary>
        /// <returns></returns>
        public static app_texts_data Load()
        {
            if (!System.IO.File.Exists(fileName)) return new app_texts_data(new string[] { "39.55", "52.59065" }, "", 10);

            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(app_texts_data));
            System.IO.StreamReader reader = System.IO.File.OpenText(fileName);
            app_texts_data c = (app_texts_data)xs.Deserialize(reader);
            reader.Close();
            return c;
        }

        /// <summary>
        ///     Загружаем из файла
        /// </summary>
        /// <param name="filename">файл</param>
        /// <returns></returns>
        public static app_texts_data Load(string filename)
        {
            if (!System.IO.File.Exists(filename)) return new app_texts_data(new string[] { "39.55", "52.59065" }, "", 10);

            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(app_texts_data));
            System.IO.StreamReader reader = System.IO.File.OpenText(filename);
            app_texts_data c = (app_texts_data)xs.Deserialize(reader);
            reader.Close();
            return c;
        }

        /// <summary>
        ///     Сохранить в файл
        /// </summary>
        public void Save()
        {
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(app_texts_data));
            System.IO.StreamWriter writer = System.IO.File.CreateText(fileName);
            xs.Serialize(writer, this);
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        ///     Сохранить в файл
        /// </summary>
        /// <param name="filename">Имя файла</param>
        public void Save(string filename)
        {
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(app_texts_data));
            System.IO.StreamWriter writer = System.IO.File.CreateText(filename);
            xs.Serialize(writer, this);
            writer.Flush();
            writer.Close();
        }
    }


    public class WMSHTTPListener //: MarshalByRefObject
    {
        //public override object InitializeLifetimeService()
        //{
        //    //return base.InitializeLifetimeService();
        //    return null;
        //}

        private Thread mainThread = null;
        private TcpListener mainListener = null;
        private IPAddress ListenIP = IPAddress.Any;
        private int ListenPort = 80;
        private bool isRunning = false;
        private int MaxThreads = 1;
        private int ThreadsCount = 0;

        public Mutex mapMtx = new Mutex();
        public MapnikCs.Map map = null;
        public MapnikCs.Projection map_proj = null;

        private System.Globalization.CultureInfo stream_CultureInfo = System.Globalization.CultureInfo.InstalledUICulture;
        private System.Globalization.NumberFormatInfo ni;

        public WMSHTTPListener(int Port, int MaxThreads)
        {
            this.ListenPort = Port;
            this.MaxThreads = MaxThreads;
            this.ni = (System.Globalization.NumberFormatInfo)stream_CultureInfo.NumberFormat.Clone();
            this.ni.NumberDecimalSeparator = ".";
        }

        public bool Running { get { return isRunning; } }
        public IPAddress ServerIP { get { return ListenIP; } }
        public int ServerPort { get { return ListenPort; } }

        public void Dispose() { Stop(); }
        ~WMSHTTPListener() { Dispose(); }

        public virtual void Start()
        {
            if (isRunning) throw new Exception("Server Already Running!");

            isRunning = true;
            mainThread = new Thread(MainThread);
            mainThread.Start();
        }

        private void MainThread()
        {
            mainListener = new TcpListener(this.ListenIP, this.ListenPort);
            mainListener.Start();
            while (isRunning)
            {
                try
                {
                    TcpClient client = mainListener.AcceptTcpClient();

                    if (this.MaxThreads > 1) // multithread or multiclient
                    {
                        while ((this.ThreadsCount >= this.MaxThreads) && isRunning) // wait for any closed thread
                            System.Threading.Thread.Sleep(5);
                        if (!isRunning) break; // break if stopped
                        Thread thr = new Thread(GetClient); // create new thread for new client
                        thr.Start(client);
                    }
                    else // single thread
                        GetClient(client);
                }
                catch { Thread.Sleep(1); };
            };
        }

        public virtual void Stop()
        {
            if (!isRunning) return;

            isRunning = false;

            if (mainListener != null) mainListener.Stop();
            mainListener = null;

            mainThread.Join();
            mainThread = null;
        }

        public void GetClient(object data)
        {
            this.MaxThreads++;
            TcpClient Client = (TcpClient)data;

            string Request = "";
            byte[] Buffer = new byte[4096];
            int Count = 0;

            try
            {
                while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
                {
                    Request += Encoding.ASCII.GetString(Buffer, 0, Count);
                    if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 4096) { break; };
                };
            }
            catch { }; // READ REQUEST ERROR

            if (Count > 0) // PARSE REQUEST ERROR
                try { ReceiveData(Client, Request); }
                catch (Exception ex)
                {
                    
                };

            Client.Close();
            this.MaxThreads--;
        }

        #region Private ReceiveData Methods
        private void ReceiveData_HTTPDescription(Stream outTo)
        {
            byte[] Buffer = Encoding.ASCII.GetBytes(
                         "HTTP/1.1 200 " + ((HttpStatusCode)200).ToString() + "\r\n" +
                         "Connection: close\r\n" +
                         "Content-Type: text/html; charset=windows-1251\r\n" +
                         "\r\n"); // charset=utf-8  
            outTo.Write(Buffer, 0, Buffer.Length);
        }

        private void ReceiveDate_GetFile(Stream outTo, string fileName, string contentType)
        {
            byte[] Buffer = Encoding.ASCII.GetBytes(
                         "HTTP/1.1 200 " + ((HttpStatusCode)200).ToString() + "\r\n" +
                         "Connection: close\r\n" +
                         "Content-Type: " + contentType + "\r\n" +
                         "\r\n"); // charset=utf-8  
            outTo.Write(Buffer, 0, Buffer.Length);

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] buff = new byte[fs.Length];
            fs.Read(buff, 0, buff.Length);
            fs.Close();

            outTo.Write(buff, 0, buff.Length);
        }
        #endregion

        // BEGIN METHOD RECEIVE DATE
        public void ReceiveData(TcpClient Client, string Request)
        {
            if (map == null)
            {
                HttpClientSendError(Client, 503);
                return;
            };

            // PARSE REQUEST
            int x1 = Request.IndexOf("GET");
            int x2 = Request.IndexOf("HTTP");
            string query = "";
            if ((x1 >= 0) && (x2 >= 0) && (x2 > x1)) query = Request.Substring(x1, x2 - x1).Trim();

            //http://192.168.0.18/nms/getMapWMS.ashx?key=TEST&SERVICE=WMS&REQUEST=GetMap&VERSION=1.1.1
            //&HEIGHT=256&WIDTH=256&SRS=EPSG:3857&BBOX=4182634.1877648444,7492051.764399837,4185080.1726699704,7494497.749304961
            
            // or

            //http://127.0.0.1:7759/?SRS=TILE&CLEAR=1&XYZ={x},{y},{z}

            // ONLY ALLOWED METHODS
            string[] qpc = query.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries);
            if (qpc.Length == 0)
            {
                HttpClientSendError(Client, 501);
                return;
            };
            System.Collections.Specialized.NameValueCollection nvc = HttpUtility.ParseQueryString(qpc[1]);

            uint wi = 256, he = 256;
            try
            {
                wi = (uint)int.Parse(nvc["WIDTH"].ToString());
                he = (uint)int.Parse(nvc["HEIGHT"].ToString());
            }
            catch { };

            string srs = nvc["SRS"];

            double sx = 0; double sy = 0; double ex = 0; double ey = 0;

            if (srs == "TILE")
            {
                string[] xyz = nvc["XYZ"].Split(new string[] { "," }, StringSplitOptions.None);
                int zm = 0;
                double.TryParse(xyz[0], System.Globalization.NumberStyles.Float, ni, out sx);
                double.TryParse(xyz[1], System.Globalization.NumberStyles.Float, ni, out sy);
                int.TryParse(xyz[2], System.Globalization.NumberStyles.Float, ni, out zm);
                PointF lt = tile2location(sx, sy, zm);
                PointF br = tile2location(sx + 1, sy + 1, zm);
                sx = lt.X;
                sy = lt.Y;
                ex = br.X;
                ey = br.Y;
            }
            else
            {
                try
                {
                    // left,bottom,right,top
                    // minx,miny,maxx,maxy
                    string[] bbox = nvc["BBOX"].Split(new string[] { "," }, StringSplitOptions.None);
                    double.TryParse(bbox[0], System.Globalization.NumberStyles.Float, ni, out sx);
                    double.TryParse(bbox[3], System.Globalization.NumberStyles.Float, ni, out sy);
                    double.TryParse(bbox[2], System.Globalization.NumberStyles.Float, ni, out ex);
                    double.TryParse(bbox[1], System.Globalization.NumberStyles.Float, ni, out ey);
                }
                catch { };

                if (srs == "EPSG:3857") //meters
                {
                    sx = x2lon_m(sx);
                    sy = y2lat_m(sy);
                    ex = x2lon_m(ex);
                    ey = y2lat_m(ey);
                };

                if (srs == "EPSG:3395") // Yandex Projection, EPSG:3395
                {
                    // Yandex Projection, EPSG:3395
                    MapnikCs.Projection proj = new MapnikCs.Projection("+proj=merc +lon_0=0 +k=1 +x_0=0 +y_0=0 +ellps=WGS84 +datum=WGS84 +units=m +no_defs");
                    MapnikCs.Coord xys = proj.Inverse(new MapnikCs.Coord(sx, sy));
                    MapnikCs.Coord xye = proj.Inverse(new MapnikCs.Coord(ex, ey));
                    sx = xys.X;
                    sy = xys.Y;
                    ex = xye.X;
                    ey = xye.Y;
                    // xys = proj.Forward(new MapnikCs.Coord(39.33, 52.86));
                };
            };

            Image img = new Bitmap((int)wi, (int)he);
            mapMtx.WaitOne();
            if (map != null)
            {
                if((map.Width != wi) || (map.Height != he))
                    map.Resize(wi, he);

                MapnikCs.Coord c0 = map_proj.Forward(new MapnikCs.Coord(sx, sy));
                MapnikCs.Coord c1 = map_proj.Forward(new MapnikCs.Coord(ex, ey));
                MapnikCs.Envelope area = new MapnikCs.Envelope(c0.X, c0.Y, c1.X, c1.Y);
                map.ZoomToBox(area);                
                MapnikCs.Image im = new MapnikCs.Image(map.Width, map.Height);
                map.Render(im);
                img = im.ToImage();
                mapMtx.ReleaseMutex();
            }
            else
            {
                HttpClientSendError(Client, 503);
                mapMtx.ReleaseMutex();
                return;
            };

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            img.Dispose();

            byte[] res = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(res, 0, res.Length);
            ms.Close();

            byte[] Buffer = Encoding.ASCII.GetBytes(
                         "HTTP/1.1 200 " + ((HttpStatusCode)200).ToString() + "\r\n" +
                         "Connection: close\r\n" +
                         "Content-Type: image/png;\r\n" +
                         "Content-Length: "+res.Length.ToString()+"\r\n"+
                         "\r\n");
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            Client.GetStream().Write(res, 0, res.Length);
            Client.Close();
        }
        // END METHOD RECEIVE DATE

        public static System.Drawing.PointF tile2location(double x, double y, int zoom)
        {
            double Lng = ((x * 256) - (256 * Math.Pow(2, zoom - 1))) / ((256 * Math.Pow(2, zoom)) / 360);
            while (Lng > 180) Lng -= 360;
            while (Lng < -180) Lng += 360;

            double exp = ((y * 256) - (256 * Math.Pow(2, zoom - 1))) / ((-256 * Math.Pow(2, zoom)) / (2 * Math.PI));
            double Lat = ((2 * Math.Atan(Math.Exp(exp))) - (Math.PI / 2)) / (Math.PI / 180);
            if (Lat < -90) Lat = -90;
            if (Lat > 90) Lat = 90;

            return new System.Drawing.PointF((float)Lng, (float)Lat);
        }

        public virtual void HttpClientSendError(TcpClient Client, int Code)
        {
            HttpClientSendError(Client, Code, "");
        }

        public virtual void HttpClientSendError(TcpClient Client, int Code, string data)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
            // Код простой HTML-странички
            string Html = "<html><body><h1>" + CodeStr + "</h1>" + data + "</body></html>";
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string Str =
                "HTTP/1.1 " + CodeStr + "\r\n" +
                "Connection: close\r\n" +
                "Content-type: text/html\r\n" +
                "Content-Length:" + Html.Length.ToString() + "\r\n\r\n" + Html;
            // Приведем строку к виду массива байт
            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
            // Отправим его клиенту
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            // Закроем соединение
            Client.Close();
        }

        private const double earth_radius = 6378137;
        private const double max_error = 1e-6;
        private static double deg2rad(double d) { return ((d) * Math.PI) / 180; }
        private static double rad2deg(double d) { return ((d) * 180) / Math.PI; }
        public static double y2lat_m(double y) { return rad2deg(2 * Math.Atan(Math.Exp((y / earth_radius))) - Math.PI / 2); }
        public static double x2lon_m(double x) { return rad2deg(x / earth_radius); }
        public static double lat2y_m(double lat) { return earth_radius * Math.Log(Math.Tan(Math.PI / 4 + deg2rad(lat) / 2)); }
        public static double lon2x_m(double lon) { return deg2rad(lon) * earth_radius; }
    }

}