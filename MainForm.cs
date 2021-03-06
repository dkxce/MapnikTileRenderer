//#define HID_01

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MapnikTileRenderer
{
    public partial class MainForm : Form
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        static extern bool TerminateThread(IntPtr hThread, uint dwExitCode);

        public MainForm()
        {
            InitializeComponent();

#if HID_01
            runFHoleBtn.Visible = false;
            runFHolesSvrToolStripMenuItem.Visible = false;
            imDiskCPL.Visible = false;
            vdMenu.Visible = false;
            saveMatrixDumbBtn.Visible = false;
            iStartMtx.Visible = false;
#endif


            pauseIfErrorSet(true, false);
        }

        private int rGrpZoom = -1;
        
        private RecentSet<string> mruList = new RecentSet<string>();
        private System.Globalization.CultureInfo stream_CultureInfo = System.Globalization.CultureInfo.InstalledUICulture;
        private System.Globalization.NumberFormatInfo ni;

        private void MapnicCreateMapExample(object sender, EventArgs e)
        {
            string cd = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.ToString();
            cd = cd.Replace("file:///", "");
            cd = cd.Replace("/", @"\");
            cd = cd.Substring(0, cd.LastIndexOf(@"\") + 1);

            MapnikCs.DatasourceCache.RegisterDatasources(cd + @"\data_sources\");
            MapnikCs.FontEngine.RegisterFont(cd + @"\fonts\DejaVuSans.ttf");
            MapnikCs.FontEngine.RegisterFont(cd + @"\fonts\comicbd.ttf");
            //string f1 = MapnikCs.FontEngine.FaceNames[0];
            //string f2 = MapnikCs.FontEngine.FaceNames[1];

            // Create Map
            // +proj=longlat +ellps=WGS84 +datum=WGS84 +no_defs
            // +proj=longlat +datum=WGS84
            MapnikCs.Map m = new MapnikCs.Map(800, 600, "+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs");
            m.BackgroundColor = Color.White;

            //MapnikCs.FontSet fontset = new MapnikCs.FontSet("fontset");
            //fontset.AddFaceName("DejaVu Sans Book");            
            //m.FontSets.Add("fontset", fontset);

            // Layer 1
            MapnikCs.Layer provpoly_lyr = new MapnikCs.Layer("Provinces");
            provpoly_lyr.SpatialReference = "+proj=lcc +ellps=GRS80 +lat_0=49 +lon_0=-95 +lat+1=49 +lat_2=77 +datum=NAD83 +units=m +no_defs";
            provpoly_lyr.Datasource = new MapnikCs.ShapeFileDatasource(cd + @"data\boundaries");

            MapnikCs.FeatureTypeStyle provpoly_style = new MapnikCs.FeatureTypeStyle();

            MapnikCs.Rule provpoly_rule_on = new MapnikCs.Rule();
            provpoly_rule_on.Filter = MapnikCs.ExpressionNodeBase.Parse("[NAME_EN] = 'Ontario'");
            provpoly_rule_on.Symbolizers.Add(new MapnikCs.PolygonSolidSymbolizer(Color.FromArgb(250, 190, 183)));
            provpoly_style.Rules.Add(provpoly_rule_on);

            MapnikCs.Rule provpoly_rule_qc = new MapnikCs.Rule();
            provpoly_rule_qc.Filter = MapnikCs.ExpressionNodeBase.Parse("[NOM_FR] = 'Québec'");
            provpoly_rule_qc.Symbolizers.Add(new MapnikCs.PolygonSolidSymbolizer(Color.FromArgb(217, 235, 203)));
            provpoly_style.Rules.Add(provpoly_rule_qc);

            m.Styles.Add("provinces", provpoly_style);
            provpoly_lyr.Styles.Add("provinces");
            m.Layers.Add(provpoly_lyr);

            // Layer 2

            MapnikCs.Layer qcdrain_lyr = new MapnikCs.Layer("Quebec Hydrography");
            qcdrain_lyr.SpatialReference = "+proj=lcc +ellps=GRS80 +lat_0=49 +lon_0=-95 +lat+1=49 +lat_2=77 +datum=NAD83 +units=m +no_defs";
            qcdrain_lyr.Datasource = new MapnikCs.ShapeFileDatasource(cd + @"data\qcdrainage");

            MapnikCs.FeatureTypeStyle qcdrain_style = new MapnikCs.FeatureTypeStyle();

            MapnikCs.Rule qcdrain_rule = new MapnikCs.Rule();
            qcdrain_rule.Filter = MapnikCs.ExpressionNodeBase.Parse("[HYC] = 8");
            MapnikCs.PolygonSolidSymbolizer sym = new MapnikCs.PolygonSolidSymbolizer(Color.FromArgb(153, 204, 255));
            sym.Opacity = 1.0;
            qcdrain_rule.Symbolizers.Add(sym);
            qcdrain_style.Rules.Add(qcdrain_rule);

            m.Styles.Add("drainage", qcdrain_style);
            qcdrain_lyr.Styles.Add("drainage");
            m.Layers.Add(qcdrain_lyr);

            // Layer 3

            MapnikCs.Layer ondrain_lyr = new MapnikCs.Layer("Ontario Hydrography");
            ondrain_lyr.SpatialReference = "+proj=lcc +ellps=GRS80 +lat_0=49 +lon_0=-95 +lat+1=49 +lat_2=77 +datum=NAD83 +units=m +no_defs";
            ondrain_lyr.Datasource = new MapnikCs.ShapeFileDatasource(cd + @"data\ontdrainage");

            ondrain_lyr.Styles.Add("drainage");
            m.Layers.Add(ondrain_lyr);

            // Layer 4

            MapnikCs.Layer provlines_lyr = new MapnikCs.Layer("Provincial borders");
            provlines_lyr.SpatialReference = "+proj=lcc +ellps=GRS80 +lat_0=49 +lon_0=-95 +lat+1=49 +lat_2=77 +datum=NAD83 +units=m +no_defs";
            provlines_lyr.Datasource = new MapnikCs.ShapeFileDatasource(cd + @"data\boundaries_l");

            MapnikCs.Stroke provlines_stk = new MapnikCs.Stroke(Color.Black, 1);
            provlines_stk.Dashes.Add(new MapnikCs.Stroke.Dash(8, 4));
            provlines_stk.Dashes.Add(new MapnikCs.Stroke.Dash(2, 2));
            provlines_stk.Dashes.Add(new MapnikCs.Stroke.Dash(2, 2));

            MapnikCs.FeatureTypeStyle provlines_style = new MapnikCs.FeatureTypeStyle();

            MapnikCs.Rule provlines_rule = new MapnikCs.Rule();
            provlines_rule.Symbolizers.Add(new MapnikCs.LineSymbolizer(provlines_stk));
            provlines_style.Rules.Add(provlines_rule);

            m.Styles.Add("provlines", provlines_style);
            provlines_lyr.Styles.Add("provlines");
            m.Layers.Add(provlines_lyr);

            // Layer 5

            MapnikCs.Layer roads34_lyr = new MapnikCs.Layer("Roads");
            roads34_lyr.SpatialReference = "+proj=lcc +ellps=GRS80 +lat_0=49 +lon_0=-95 +lat+1=49 +lat_2=77 +datum=NAD83 +units=m +no_defs";
            roads34_lyr.Datasource = new MapnikCs.ShapeFileDatasource(cd + @"data\roads");

            MapnikCs.FeatureTypeStyle roads34_style = new MapnikCs.FeatureTypeStyle();

            MapnikCs.Rule roads34_rule = new MapnikCs.Rule();
            roads34_rule.Filter = MapnikCs.ExpressionNodeBase.Parse("([CLASS] = 3) or ([CLASS] = 4)");

            MapnikCs.Stroke roads34_rule_stk = new MapnikCs.Stroke(Color.FromArgb(171, 158, 137), 2);
            roads34_rule_stk.LineCap = MapnikCs.LineCap.Round;

            roads34_rule.Symbolizers.Add(new MapnikCs.LineSymbolizer(roads34_rule_stk));
            roads34_style.Rules.Add(roads34_rule);

            m.Styles.Add("smallroads", roads34_style);
            roads34_lyr.Styles.Add("smallroads");
            m.Layers.Add(roads34_lyr);

            // Layer 6

            MapnikCs.Layer roads2_lyr = new MapnikCs.Layer("Roads");
            roads2_lyr.SpatialReference = "+proj=lcc +ellps=GRS80 +lat_0=49 +lon_0=-95 +lat+1=49 +lat_2=77 +datum=NAD83 +units=m +no_defs";
            roads2_lyr.Datasource = roads34_lyr.Datasource;

            MapnikCs.FeatureTypeStyle roads2_style_1 = new MapnikCs.FeatureTypeStyle();
            MapnikCs.Rule roads2_rule_1 = new MapnikCs.Rule();
            roads2_rule_1.Filter = MapnikCs.ExpressionNodeBase.Parse("[CLASS] = 2");

            MapnikCs.Stroke roads2_rule_stk_1 = new MapnikCs.Stroke(Color.FromArgb(171, 158, 137), 4);
            roads2_rule_stk_1.LineCap = MapnikCs.LineCap.Round;

            roads2_rule_1.Symbolizers.Add(new MapnikCs.LineSymbolizer(roads2_rule_stk_1));
            roads2_style_1.Rules.Add(roads2_rule_1);

            m.Styles.Add("road-border", roads2_style_1);

            MapnikCs.FeatureTypeStyle roads2_style_2 = new MapnikCs.FeatureTypeStyle();
            MapnikCs.Rule roads2_rule_2 = new MapnikCs.Rule();
            roads2_rule_2.Filter = MapnikCs.ExpressionNodeBase.Parse("[CLASS] = 2");

            MapnikCs.Stroke roads2_rule_stk_2 = new MapnikCs.Stroke(Color.FromArgb(255, 250, 115), 2);
            roads2_rule_stk_2.LineCap = MapnikCs.LineCap.Round;

            roads2_rule_2.Symbolizers.Add(new MapnikCs.LineSymbolizer(roads2_rule_stk_2));
            roads2_style_2.Rules.Add(roads2_rule_2);

            m.Styles.Add("road-fill", roads2_style_2);

            roads2_lyr.Styles.Add("road-border");
            roads2_lyr.Styles.Add("road-fill");

            m.Layers.Add(roads2_lyr);

            // Layer 7

            MapnikCs.Layer roads1_lyr = new MapnikCs.Layer("Roads");
            roads1_lyr.SpatialReference = "+proj=lcc +ellps=GRS80 +lat_0=49 +lon_0=-95 +lat+1=49 +lat_2=77 +datum=NAD83 +units=m +no_defs";
            roads1_lyr.Datasource = roads34_lyr.Datasource;

            MapnikCs.FeatureTypeStyle roads1_style_1 = new MapnikCs.FeatureTypeStyle();
            MapnikCs.Rule roads1_rule_1 = new MapnikCs.Rule();
            roads1_rule_1.Filter = MapnikCs.ExpressionNodeBase.Parse("[CLASS] = 1");
            MapnikCs.Stroke roads1_rule_stk_1 = new MapnikCs.Stroke(Color.FromArgb(188, 149, 28), 7);
            roads1_rule_stk_1.LineCap = MapnikCs.LineCap.Round;
            roads1_rule_1.Symbolizers.Add(new MapnikCs.LineSymbolizer(roads1_rule_stk_1));
            roads1_style_1.Rules.Add(roads1_rule_1);
            m.Styles.Add("highway-border", roads1_style_1);

            MapnikCs.FeatureTypeStyle roads1_style_2 = new MapnikCs.FeatureTypeStyle();
            MapnikCs.Rule roads1_rule_2 = new MapnikCs.Rule();
            roads1_rule_2.Filter = MapnikCs.ExpressionNodeBase.Parse("[CLASS] = 1");
            MapnikCs.Stroke roads1_rule_stk_2 = new MapnikCs.Stroke(Color.FromArgb(242, 191, 36), 5);
            roads1_rule_stk_2.LineCap = MapnikCs.LineCap.Round;
            roads1_rule_2.Symbolizers.Add(new MapnikCs.LineSymbolizer(roads1_rule_stk_2));
            roads1_style_2.Rules.Add(roads1_rule_2);
            m.Styles.Add("highway-fill", roads1_style_2);

            roads1_lyr.Styles.Add("highway-border");
            roads1_lyr.Styles.Add("highway-fill");
            m.Layers.Add(roads1_lyr);

            // Layer 8

            MapnikCs.Layer popplaces_lyr = new MapnikCs.Layer("Populated Places");
            popplaces_lyr.SpatialReference = "+proj=lcc +ellps=GRS80 +lat_0=49 +lon_0=-95 +lat+1=49 +lat_2=77 +datum=NAD83 +units=m +no_defs";
            popplaces_lyr.Datasource = new MapnikCs.ShapeFileDatasource(cd + @"data\popplaces");

            MapnikCs.FeatureTypeStyle popplaces_style = new MapnikCs.FeatureTypeStyle();
            MapnikCs.Rule popplaces_rule = new MapnikCs.Rule();
            MapnikCs.TextSymbolizer popplaces_text_symbolizer = new MapnikCs.TextSymbolizer(MapnikCs.ExpressionNodeBase.Parse("[GEONAME]"), "Comic Sans MS Bold", 10, Color.Black);
            popplaces_text_symbolizer.LabelPlacement = MapnikCs.LabelPlacement.Point;
            popplaces_text_symbolizer.HaloFill = Color.FromArgb(255, 255, 200);
            popplaces_text_symbolizer.HaloRadius = 1;
            popplaces_text_symbolizer.AvoidEdges = true;

            popplaces_rule.Symbolizers.Add(popplaces_text_symbolizer);
            popplaces_style.Rules.Add(popplaces_rule);

            m.Styles.Add("popplaces", popplaces_style);
            popplaces_lyr.Styles.Add("popplaces");
            m.Layers.Add(popplaces_lyr);

            // Draw Map

            m.ZoomToBox(new MapnikCs.Envelope(-8024477.28459, 5445190.38849, -7381388.20071, 5662941.44855));
            MapnikCs.Image im = new MapnikCs.Image(m.Width, m.Height);
            m.Render(im);
            im.Save(cd + @"\maps\demo_cs.png", System.Drawing.Imaging.ImageFormat.Png);
            m.SaveFile(cd + @"\maps\demo_cs_map.xml");
        }

        private void MapnicLoadMapExample(object sender, EventArgs e)
        {
            string cd = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.ToString();
            cd = cd.Replace("file:///", "");
            cd = cd.Replace("/", @"\");
            cd = cd.Substring(0, cd.LastIndexOf(@"\") + 1);

            MapnikCs.DatasourceCache.RegisterDatasources(cd + @"\data_sources\");
            MapnikCs.FontEngine.RegisterFont(cd + @"\fonts\DejaVuSans.ttf");
            MapnikCs.FontEngine.RegisterFont(cd + @"\fonts\comicbd.ttf");

            MapnikCs.Map m = new MapnikCs.Map(800, 600, "+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs");
            m.ZoomToBox(new MapnikCs.Envelope(-8024477.28459, 5445190.38849, -7381388.20071, 5662941.44855));
            m.LoadFile(cd + @"\maps\demo_cs_map-6.xml");
            MapnikCs.Image im = new MapnikCs.Image(m.Width, m.Height);
            m.Render(im);
            im.Save(cd + @"\maps\demo_csII.png", System.Drawing.Imaging.ImageFormat.Png);
            m.SaveFile(cd + @"\maps\demo_cs_mapII.xml");
        }

        private void MapnicRenderTilesExample(object sender, EventArgs e)
        {
            string cd = TileRendering.TileRenderer.GetCurrentDir();

            double[] px = TileRendering.TilesProjection.fromLLtoPixel(new double[] { 37.39, 55.45 }, 5);
            double[] tl = new double[] { px[0] / 256, px[1] / 256 };
            double[] ll = TileRendering.TilesProjection.fromPixelToLL(px, 10);

            double lat = 52.86;
            double lon = 39.33;
            double[] xyt = TileRendering.TilesProjection.fromLLtoPixel(new double[] { lon, lat }, 5);
            int y_t = (int)(xyt[1] / 256.0);
            int x_t = (int)(xyt[0] / 256.0);
            double y_m = TileRendering.TilesProjection.lat2y_m(lat);
            double x_m = TileRendering.TilesProjection.lon2x_m(lon);
            

            //TileRendering.TileRenderer.RENDER_SAMPLE(80, -150, 0, 40,//70,20,10,80,
            //    cd + @"\maps\demo_cs_map-6.xml",
            //    cd + @"\TILES\",
            //    7);

            TileRendering.MultithreadTileRenderer mtr = new TileRendering.MultithreadTileRenderer(
                cd + @"\maps\demo_cs_map-6.xml",
                cd + @"\TILES\");
            mtr.RenderTiles(80.0, -150.0, 0.0, 40.0, 7);    
        }

        private void NavicomTilesExample(object sender, EventArgs e)
        {
            string cd = TileRendering.TileRenderer.GetCurrentDir();

            // Draw Map
            MapnikCs.DatasourceCache.RegisterDatasources(cd + @"\data_sources\");
            MapnikCs.FontEngine.RegisterFont(cd + @"\fonts\DejaVuSans.ttf");
            MapnikCs.FontEngine.RegisterFont(cd + @"\fonts\comicbd.ttf");

            MapnikCs.Map m = new MapnikCs.Map(1600, 1600, "+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs");
            m.LoadFile(cd + @"\maps\nav_maps.xml");
            MapnikCs.Projection map_proj = new MapnikCs.Projection(m.SpatialReference);
            MapnikCs.Coord c0 = map_proj.Forward(new MapnikCs.Coord(39.33, 52.86));
            MapnikCs.Coord c1 = map_proj.Forward(new MapnikCs.Coord(39.83, 52.38));
            MapnikCs.Envelope bbox = new MapnikCs.Envelope(c0.X, c0.Y, c1.X, c1.Y);
            m.ZoomToBox(bbox);
            MapnikCs.Image im = new MapnikCs.Image(m.Width, m.Height);
            m.Render(im);
            im.Save(cd + @"\maps\nav_maps.png", System.Drawing.Imaging.ImageFormat.Png);  

            //TileRendering.MultithreadTileRenderer mtr = new TileRendering.MultithreadTileRenderer(
            //    cd + @"\maps\nav_maps.xml",
            //    cd + @"\TILES\");
            ////mtr.RenderTiles(2498, 1341, 2498, 1341, 12);
            //mtr.RenderTiles(53.5, 38, 51.5, 42, 12);

            //TileRendering.TileRenderer str = new TileRendering.TileRenderer(
            //    cd + @"\maps\nav_maps.xml",
            //    cd + @"\TILES\");
            //str.render_tile(2498, 1341, 12);
        }

        private string _project_filename = "";
        private System.Threading.Thread _runtime_thread = null;
        private DateTime[] _runtime_started = new DateTime[22];
        private DateTime[] _runtime_stopped = new DateTime[22];
        private TileRendering.MultithreadTileRenderer[] _runtime_renderers = new TileRendering.MultithreadTileRenderer[22];
        private int _runtime_current_zoom = 0;
        private int _runtime_zooms_done = 0;
        private bool[] _runtime_zooms_todo = new bool[22];
        private ulong _runtime_total_to_render = 0;

        private void MainForm_Load(object sender, EventArgs e)
        {
            string mruFile = TileRendering.TileRenderer.GetCurrentDir() + @"\MapnikTileRenderer.mru";
            if (File.Exists(mruFile))
            {
                List<string> list = new List<string>();
                System.IO.FileStream fs = new FileStream(mruFile, FileMode.Open, FileAccess.Read);
                System.IO.StreamReader sr = new StreamReader(fs);
                while (!sr.EndOfStream) list.Add(sr.ReadLine());
                sr.Close();
                fs.Close();
                mruList = new RecentSet<string>(list);
                UpdateMRU(false);
            };

            ni = (System.Globalization.NumberFormatInfo)stream_CultureInfo.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            updateFontList(sender, e);
            update_data_sources(sender, e);
            label23.Text = String.Format("(Число ядер процессора: {0})", System.Environment.ProcessorCount);
            coresCount.Text = System.Environment.ProcessorCount.ToString();
            NewTProject();

            if ((AppStart.arguments != null) && (AppStart.arguments.Length > 0))
            {
                if(File.Exists(AppStart.arguments[0]))
                    WriteProjectToForm(AppStart.arguments[0]);
            };            
        }

        private void Write2Reg()
        {
            System.IO.FileStream fs = new FileStream(TileRendering.TileRenderer.GetCurrentDir() + @"\MapnikTileRenderer.trg", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            string txt = sr.ReadToEnd();
            sr.Close();
            fs.Close();

            string cd = TileRendering.TileRenderer.GetCurrentDir();
            if (cd.Substring(cd.Length - 1) == @"\") cd = cd.Remove(cd.Length - 1);
            cd = cd.Replace(@"\", @"\\");
            txt = txt.Replace("{APPDIR}", cd);

            string tmpfn = TileRendering.TileRenderer.GetCurrentDir() + @"\register_proj_type.reg";
            fs = new FileStream(tmpfn, FileMode.Create, FileAccess.Write);
            byte[] b2w = System.Text.Encoding.GetEncoding(1251).GetBytes(txt);
            fs.Write(b2w, 0, b2w.Length);
            fs.Close();

            System.Diagnostics.Process.Start("regedit.exe", "\"" + tmpfn + "\"");
        }

        public class ToolStripMenuFileItem : ToolStripMenuItem
        {
            public string FileName = "";
            public ToolStripMenuFileItem(string fileName)
            {
                this.FileName = fileName;
                string path = Path.GetDirectoryName(this.FileName);
                if (path.Length > 25)
                {
                    string lp = path.Substring(path.Length - 25);
                    lp = lp.Substring(lp.IndexOf(@"\") >= 0 ? lp.IndexOf(@"\") : lp.Length);
                    path = Path.GetPathRoot(path) + " ... " + lp;
                };
                this.Text = path + "\\" + Path.GetFileName(this.FileName);
            }
        }

        private void UpdateMRU(bool saveToFile)
        {
            if (saveToFile)
            {
                string mruFile = TileRendering.TileRenderer.GetCurrentDir() + @"\MapnikTileRenderer.mru";
                System.IO.FileStream fs = new FileStream(mruFile, FileMode.Create, FileAccess.Write);
                System.IO.StreamWriter sw = new StreamWriter(fs);
                if (mruList.Count > 0)
                    foreach (string f in mruList)
                        sw.WriteLine(f);
                sw.Close();
                fs.Close();
            };
            MRU.DropDownItems.Clear();
            MRU.Visible = mruList.Count > 0;
            if (mruList.Count > 0)
                foreach (string f in mruList)
                {
                    System.Windows.Forms.Keys num = System.Windows.Forms.Keys.D1;
                    num += MRU.DropDownItems.Count;
                    ToolStripMenuFileItem itm = new ToolStripMenuFileItem(f);
                    itm.Click += new EventHandler(mruItemClick);
                    itm.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | num)));
                    MRU.DropDownItems.Add(itm);
                };
        }

        private void mruItemClick(object sender, EventArgs e)
        {
            ToolStripMenuFileItem itm = null;
            try
            {
                itm = (ToolStripMenuFileItem)sender;
            }
            catch { };
            if (itm == null) return;
            if (File.Exists(itm.FileName))
            {
                WriteProjectToForm(itm.FileName);                
            }
            else
            {
                MessageBox.Show("Файл проекта " + itm.FileName + " не найден!","Загрузка проекта",MessageBoxButtons.OK,MessageBoxIcon.Error);
                mruList.Delete(itm.FileName);
                UpdateMRU(true);
            };
        }

        private void NewTProject()
        {
            iTileRenderPathScheme.SelectedIndex = 1;
            iTileRenderZoneSize.SelectedIndex = 7;
            iTileRenderEmptyData.SelectedIndex = 3;
            iTileRenderZoneMode.SelectedIndex = 0;
            iTileOptimizeMethod.SelectedIndex = 3;
            iPNGFormat.SelectedIndex = 0;
            iTileFinalMerging.SelectedIndex = 1;
            itileZoneOverSize.Value = 128;
            imultithreadCount.Value = System.Environment.ProcessorCount > 4 ? System.Environment.ProcessorCount - 2 : 2;
            iRerender.Checked = true;
            iRenderReload.Checked = false;
            iaddCopyrightsConfig.Text = "";
            irPolygon.Text = "";
            iMapFileName.Text = "";
            iTilesDir.Text = "";
            isX.Text = "-180.0";
            isY.Text = "90.0";
            ieX.Text = "180.0";
            ieY.Text = "-90.0";
            iBoxFormat.SelectedIndex = 0;
            iBoxZoom.SelectedIndex = 9;
            for (int i = 0; i < iZooms.Items.Count; i++) iZooms.SetItemChecked(i, false);
            UpdateMenu("");
        }


        private void newProjMenuItem_Click(object sender, EventArgs e)
        {            
            NewTProject();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            iaddCopyrightsConfig.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Выберите PNG файл водяного знака";
            ofd.DefaultExt = ".png";
            ofd.Filter = "PNG Files (*.png)|*.png|All types (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Image im = Image.FromFile(ofd.FileName);
                if ((im.Height != 256) || (im.Width != 256))
                {
                    im.Dispose();
                    MessageBox.Show("Размер изображения водяного знака должен быть 256 х 256 пикселей!", "Выбор водяного знака", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ofd.Dispose();
                    return;                    
                };
                im.Dispose();
                iaddCopyrightsConfig.Text = ofd.FileName;
            };
            ofd.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            iMapFileName.Text = "";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".xml";
            //ofd.Multiselect = true;
            ofd.Filter = "XML Files (*.xml)|*.xml|All types (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                iMapFileName.Text = ofd.FileName;
                //for (int i = 1; i < ofd.FileNames.Length; i++)
                //    iMapFileName.Text += "?" + ofd.FileNames[i];
            };
            ofd.Dispose();
        }

        private void updateFontList(object sender, EventArgs e)
        {
            iFonts.Items.Clear();
            System.IO.FileStream fs = new FileStream(TileRendering.TileRenderer.GetCurrentDir() + @"\fonts\_fontlist.txt", FileMode.Create, FileAccess.Write);
            for (int i = 0; i < MapnikCs.FontEngine.FaceNames.Count; i++)
            {
                iFonts.Items.Add(MapnikCs.FontEngine.FaceNames[i]);
                byte[] ba = System.Text.Encoding.GetEncoding(1251).GetBytes(MapnikCs.FontEngine.FaceNames[i] + "\r\n");
                fs.Write(ba, 0, ba.Length);
            };
            fs.Close();
        }

        private void update_data_sources(object sender, EventArgs e)
        {
            idata_sources.Items.Clear();
            foreach (string f in System.IO.Directory.GetFiles(TileRendering.TileRenderer.GetCurrentDir() + @"\data_sources", "*.input"))
                idata_sources.Items.Add(System.IO.Path.GetFileName(f));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            iTilesDir.Text = "";
        }

        private void button10_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
                iTilesDir.Text = fbd.SelectedPath;
            fbd.Dispose();
        }

        private void iBoxFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            iBoxZoom.Enabled = iBoxFormat.SelectedIndex > 1;
        }

        private void UpdateMenu(string filename)
        {
            _project_filename = filename;
            spMnu.Enabled = _project_filename != "";
            Text = "Mapnik Tile Renderer v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + (_project_filename == "" ? "" : " : " + System.IO.Path.GetFileName(_project_filename));            

            string errLogFileName = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\NO_NAME_PROJECT";
            if (_project_filename != "") errLogFileName = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\main_[" + System.IO.Path.GetFileName(_project_filename)+"]";
            errLogFileName += ".errorlog";
            groupBox2.Text = "Последняя ошибка (остальные сохраняются в файл `\\LOGS\\" + System.IO.Path.GetFileName(errLogFileName) + "`)";
        }

        private MapnikTileRendererProject ReadFormToProject()
        {
            MapnikTileRendererProject prj = new MapnikTileRendererProject();
            prj.MapFile = iMapFileName.Text;
            prj.TilesPath = iTilesDir.Text;
            prj.RendererConfig = new TileRendering.TileRendererConfig();
            prj.RendererConfig.multithreadCount = (int)imultithreadCount.Value;
            prj.RendererConfig.addToQueueAgainIfError = iRerender.Checked;
            prj.RendererConfig.reloadMapEachZone = iRenderReload.Checked;
            prj.RendererConfig.pathStructure = (TileRendering.TileRenderPathScheme)(iTileRenderPathScheme.SelectedIndex + 1);
            prj.RendererConfig.processEmptyTiles = (TileRendering.TileRenderEmptyData)(iTileRenderEmptyData.SelectedIndex + 1);
            prj.RendererConfig.tileFinalMerging = (TileRendering.TileFinalMerging)(iTileFinalMerging.SelectedIndex + 1);
            prj.RendererConfig.tileOptimizeMethod = (TileRendering.TileOptimizeMethod)(iTileOptimizeMethod.SelectedIndex + 1);
            prj.RendererConfig.tileOptimizeFormat = (byte)(iPNGFormat.SelectedIndex);
            prj.RendererConfig.tileZoneMode = (TileRendering.TileRenderZoneMode)(iTileRenderZoneMode.SelectedIndex + 1);
            prj.RendererConfig.tileZoneOverSize = (ushort)itileZoneOverSize.Value;
            prj.RendererConfig.tileZoneSize = (TileRendering.TileRenderZoneSize)((int)(iTileRenderZoneSize.SelectedIndex)+1);
            prj.RendererConfig.addCopyrightsFile = iaddCopyrightsConfig.Text;
            prj.RendererConfig.renderOnlyInPolygon = irPolygon.Text;
            double sx; double sy; double ex; double ey;
            double.TryParse(isX.Text, System.Globalization.NumberStyles.Float, ni, out sx);
            double.TryParse(isY.Text, System.Globalization.NumberStyles.Float, ni, out sy);
            double.TryParse(ieX.Text, System.Globalization.NumberStyles.Float, ni, out ex);
            double.TryParse(ieY.Text, System.Globalization.NumberStyles.Float, ni, out ey);
            prj.Zone = new double[] { iBoxFormat.SelectedIndex, sx, sy, ex, ey, iBoxZoom.SelectedIndex };
            for (int i = 0; i < iZooms.Items.Count; i++)
                prj.zooms[i + 1] = iZooms.GetItemChecked(i);
            return prj;
        }

        private void SaveProject(string filename)
        {
            MapnikTileRendererProject prj = ReadFormToProject();

            if (filename == "")
            {
                SaveFileDialog sfd = new SaveFileDialog();
                if (_project_filename != null) sfd.FileName = _project_filename;
                sfd.DefaultExt = ".mtr_project";
                sfd.Filter = "MTR Projects (*.mtr_project)|*.mtr_project";
                if (sfd.ShowDialog() == DialogResult.OK)
                    prj.Save(filename = sfd.FileName);
                sfd.Dispose();
                if (filename == "") return;
            }
            else
                prj.Save(filename);

            mruList.Add(filename);
            UpdateMRU(true);

            UpdateMenu(filename);
        }

        private void saveAsProj(object sender, EventArgs e)
        {
            SaveProject("");
        }

        private void saveBtn(object sender, EventArgs e)
        {
            SaveProject(_project_filename);
        }

        private void WriteProjectToForm(string fileName)
        {
            MapnikTileRendererProject prj = MapnikTileRendererProject.FromFile(fileName);
            iMapFileName.Text = prj.MapFile;
            iTilesDir.Text = prj.TilesPath;
            imultithreadCount.Value = prj.RendererConfig.multithreadCount;
            iRerender.Checked = prj.RendererConfig.addToQueueAgainIfError;
            iRenderReload.Checked = prj.RendererConfig.reloadMapEachZone;
            iTileRenderPathScheme.SelectedIndex = ((int)(prj.RendererConfig.pathStructure)) - 1;
            iTileRenderEmptyData.SelectedIndex = ((int)(prj.RendererConfig.processEmptyTiles)) - 1;
            iTileFinalMerging.SelectedIndex = ((int)(prj.RendererConfig.tileFinalMerging)) - 1;
            iTileOptimizeMethod.SelectedIndex = ((int)(prj.RendererConfig.tileOptimizeMethod)) - 1;
            iPNGFormat.SelectedIndex = prj.RendererConfig.tileOptimizeFormat;
            iTileRenderZoneMode.SelectedIndex = ((int)(prj.RendererConfig.tileZoneMode)) - 1;
            itileZoneOverSize.Value = prj.RendererConfig.tileZoneOverSize;
            iTileRenderZoneSize.SelectedIndex = ((int)prj.RendererConfig.tileZoneSize) - 1;
            iaddCopyrightsConfig.Text = prj.RendererConfig.addCopyrightsFile;
            irPolygon.Text = prj.RendererConfig.renderOnlyInPolygon;
            isX.Text = prj.Zone[1].ToString(ni);
            isY.Text = prj.Zone[2].ToString(ni);
            ieX.Text = prj.Zone[3].ToString(ni);
            ieY.Text = prj.Zone[4].ToString(ni);
            iBoxFormat.SelectedIndex = (int)prj.Zone[0];
            iBoxZoom.SelectedIndex = (int)prj.Zone[5];
            for (int i = 0; i < iZooms.Items.Count; i++)
                iZooms.SetItemChecked(i, prj.zooms[i + 1]);

            mruList.Add(fileName);
            UpdateMRU(true);

            UpdateMenu(fileName);
        }

        private void openBtn(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".mtr_project";
            ofd.Filter = "MTR Projects (*.mtr_project)|*.mtr_project";
            if (ofd.ShowDialog() == DialogResult.OK)
                WriteProjectToForm(ofd.FileName);
            ofd.Dispose();
        }

        private void exitBtn(object sender, EventArgs e)
        {
            this.Close();
        }
     
        private void saveMapExample(object sender, EventArgs e)
        {
            if ((iMapFileName.Text == "") || (!System.IO.File.Exists(iMapFileName.Text))) { MessageBox.Show("Не настроены параметры карты!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information); return; };
            
            WHZ whz = new WHZ();
            if (whz.ShowDialog() != DialogResult.OK)
            {
                whz.Dispose();
                return;
            };
            uint wi = (uint)whz.iWidth.Value;
            uint he = (uint)whz.iHeight.Value;
            whz.Dispose();

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".png";
            sfd.Filter = "PNG Files (*.png)|*.png";
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                sfd.Dispose();
                return;
            };
            string pngFileName = sfd.FileName;
            sfd.Dispose();

            //градусы
            double sx; double sy; double ex; double ey;
            double.TryParse(isX.Text, System.Globalization.NumberStyles.Float, ni, out sx);
            double.TryParse(isY.Text, System.Globalization.NumberStyles.Float, ni, out sy);
            double.TryParse(ieX.Text, System.Globalization.NumberStyles.Float, ni, out ex);
            double.TryParse(ieY.Text, System.Globalization.NumberStyles.Float, ni, out ey);
            
            //метры проекции
            if (iBoxFormat.SelectedIndex == 1)
            {
                sx = TileRendering.TilesProjection.x2lon_m(sx);
                sy = TileRendering.TilesProjection.y2lat_m(sy);
                ex = TileRendering.TilesProjection.x2lon_m(ex);
                ey = TileRendering.TilesProjection.y2lat_m(ey);
            };
            //номера тайлов
            if (iBoxFormat.SelectedIndex == 2)
            {
                double tmp_lat; double tmp_lon;
                TileRendering.TilesProjection.fromTileToLL((int)sx, (int)sy, iBoxZoom.SelectedIndex + 1, out tmp_lat, out tmp_lon);
                sx = tmp_lon; sy = tmp_lat;
                TileRendering.TilesProjection.fromTileToLL((int)ex, (int)ey, iBoxZoom.SelectedIndex + 1, out tmp_lat, out tmp_lon);
                ex = tmp_lon; ey = tmp_lat;
            };
            //координаты в пикселях
            if (iBoxFormat.SelectedIndex == 3)
            {
                double[] ll = TileRendering.TilesProjection.fromPixelToLL(new double[] { sy, sx }, iBoxZoom.SelectedIndex + 1);
                sx = ll[0]; sy = ll[1];
                ll = TileRendering.TilesProjection.fromPixelToLL(new double[] { ey, ex }, iBoxZoom.SelectedIndex + 1);
                ex = ll[0]; ey = ll[1];
            };

            MapnikCs.Map m = new MapnikCs.Map(wi, he, "+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs");
            MapnikCs.Projection map_proj = new MapnikCs.Projection(m.SpatialReference);
            MapnikCs.Coord c0 = map_proj.Forward(new MapnikCs.Coord(sx, sy));
            MapnikCs.Coord c1 = map_proj.Forward(new MapnikCs.Coord(ex, ey));
            MapnikCs.Envelope bbox = new MapnikCs.Envelope(c0.X, c0.Y, c1.X, c1.Y);            
            m.LoadFile(iMapFileName.Text);
            m.ZoomToBox(bbox);
            MapnikCs.Image im = new MapnikCs.Image(m.Width, m.Height);
            m.Render(im);
            im.Save(pngFileName, System.Drawing.Imaging.ImageFormat.Png);
            System.Diagnostics.Process.Start(pngFileName);
        }

        private void iStartBtn_Click(object sender, EventArgs e)
        {
            if ((iMapFileName.Text == "") 
                || (!System.IO.File.Exists(iMapFileName.Text.Split(new string[]{"?"},StringSplitOptions.RemoveEmptyEntries)[0])) 
                || (iTilesDir.Text.Trim().Length == 0)
                || (iZooms.CheckedItems.Count == 0)) { MessageBox.Show("Не настроены параметры карты!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information); return; };
            
            if (iTileRenderPathScheme.SelectedIndex == 1)
            {
                bool baseZooms = false;
                bool detailedZooms = false;
                for (int i = 1; i < 10; i++) if (iZooms.GetItemChecked(i - 1)) baseZooms = true;
                for (int i = 10; i <= iZooms.Items.Count; i++) if (iZooms.GetItemChecked(i - 1)) detailedZooms = true;
                if (baseZooms && detailedZooms)
                {
                    if (MessageBox.Show(
                        "При нарезке одновременно и базовой карты (1-9 уровни),\r\n" +
                        "и региональной карты (10-21 уровни) может возникать перекрытие\r\n" +
                        "уровней т.к. файлы некоторых масштабных уровней лежат в одной папке:\r\n\r\n" +
                        "zoom 1 -> данные отсутствуют\r\n" +
                        "zoom 2 -> данные отсутствуют\r\n" +
                        "zoom 3 -> данные отсутствуют\r\n\r\n" +
                        "zoom 4, zoom 10 - > Папка \\_alllayers\\L00\\ " + (iZooms.GetItemChecked(3) && iZooms.GetItemChecked(9) ? " - перекрытие, совпадение имен файлов" : "") + "\r\n" +
                        "zoom 5, zoom 11 - > Папка \\_alllayers\\L01\\ " + (iZooms.GetItemChecked(4) && iZooms.GetItemChecked(10) ? " - перекрытие, совпадение имен файлов" : "") + "\r\n" +
                        "zoom 6, zoom 12 - > Папка \\_alllayers\\L02\\ " + (iZooms.GetItemChecked(5) && iZooms.GetItemChecked(11) ? " - перекрытие, совпадение имен файлов" : "") + "\r\n" +
                        "zoom 7, zoom 13 - > Папка \\_alllayers\\L03\\ " + (iZooms.GetItemChecked(6) && iZooms.GetItemChecked(12) ? " - перекрытие, совпадение имен файлов" : "") + "\r\n" +
                        "zoom 8, zoom 14 - > Папка \\_alllayers\\L04\\ " + (iZooms.GetItemChecked(7) && iZooms.GetItemChecked(13) ? " - перекрытие, совпадение имен файлов" : "") + "\r\n" +
                        "zoom 9, zoom 15 - > Папка \\_alllayers\\L05\\ " + (iZooms.GetItemChecked(8) && iZooms.GetItemChecked(14) ? " - перекрытие, совпадение имен файлов" : "") + "\r\n\r\n" +
                        "zoom 16 -> Папка \\_alllayers\\L06\\ \r\n" +
                        "zoom 17 -> Папка \\_alllayers\\L07\\ \r\n" +
                        "zoom 18 -> Папка \\_alllayers\\L08\\ \r\n" +
                        "zoom 19 -> Папка \\_alllayers\\L09\\ \r\n" +
                        "zoom 20 -> Папка \\_alllayers\\L10\\ \r\n" +
                        "zoom 21 -> Папка \\_alllayers\\L11\\ \r\n\r\n" +
                        "Рекомендуется нарезать базовую карту и региональные карту в разные папки.\r\n" +
                        "Вы действительно хотите начать нарезку тайлов для выбранных уровней?",
                        "Перекрытие уровней", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No) return;
                }
                else
                {
                    if (MessageBox.Show("Вы действительно хотите начать нарезку тайлов для выбранных уровней?", "Нарезка тайлов", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No) return;
                };
            }
            else
            {
                if (MessageBox.Show("Вы действительно хотите начать нарезку тайлов для выбранных уровней?", "Нарезка тайлов", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No) return;
            };

            StartRendering();
        }

        private void StartRendering()
        {
            StartRendering(null, null);
        }

        private static void BackUpLogs()
        {
            try
            {
                string ld = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\";
                string[] allf = Directory.GetFiles(ld, "*.*", SearchOption.TopDirectoryOnly);
                if (allf.Length > 0)
                {
                    string bd = ld + "Autobackup " + DateTime.Now.ToString("yyyy-MM-dd HHmmss fff") + @"\";
                    Directory.CreateDirectory(bd);
                    foreach (string f in allf)
                        File.Copy(f, bd + Path.GetFileName(f));
                };
            }
            catch { };
        }

        private void StartRendering(TileRendering.FHoles holes, string loadDumpFile)
        {
            _runtime_renderers = new TileRendering.MultithreadTileRenderer[22];

            //CHECK INDEX FILES
            {
                string cd = TileRendering.TileRenderer.GetCurrentDir();
                
                System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
                try
                {
                    xd.Load(iMapFileName.Text);
                    XmlNodeList nl = xd.SelectNodes("Map/Layer/Datasource/Parameter[@name='type']");
                    int shapeFC = 0;
                    int shapeIC = 0;
                    foreach (XmlNode xn in nl)
                    {
                        string tp = xn.ChildNodes[0].Value;
                        if (tp.ToLower() != "shape") continue;
                        shapeFC++;
                        XmlNode fn = xn.ParentNode.SelectSingleNode("Parameter[@name='file']");
                        string gn = fn.ChildNodes[0].Value;
                        if (File.Exists(gn + ".index")) shapeIC++;
                    };
                    if (shapeFC != shapeIC)
                    {
                        MessageBox.Show("Для "+(shapeFC-shapeIC).ToString()+" shape файлов не найдены индексные файлы!\r\nПрежде чем запускать нарезку необхоимо проиндексировать данные!", "Нарезка тайлов", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    };
                }
                catch (Exception ex) {};
                xd = null;
            };

            logErrors.Text = "Ошибок нет";
            TileRendering.TileRenderingErrors.PausedThreads.Clear();
            TileRendering.TileRenderingErrors.ErrorQueueTotalCount = 0;
            TileRendering.TileRenderingErrors.ErrorQueue.Clear();

            TileRendering.TileRenderingErrors.PauseNormal = false;
            TileRendering.TileRenderingErrors.PausedNormal = 0;

            BackUpLogs();

            // DELETE ERROR LOG FILE
            string errLogFileName = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\NO_NAME_PROJECT";
            if (_project_filename != "") errLogFileName = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\main_[" + System.IO.Path.GetFileName(_project_filename) + "]";
            errLogFileName += ".errorlog";
            if (System.IO.File.Exists(errLogFileName)) System.IO.File.Delete(errLogFileName);

            TileRendering.MultithreadTileRenderer.holes_fileName = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\main_[" + System.IO.Path.GetFileName(_project_filename) + "].holes";
            if (System.IO.File.Exists(TileRendering.MultithreadTileRenderer.holes_fileName)) System.IO.File.Delete(TileRendering.MultithreadTileRenderer.holes_fileName);

            tabControl1.Enabled = false;
            tabControl1.Visible = false;
            tabControl2.Height = 713;
            tabControl2.Top = tabControl1.Top;
            visualizeBtn.Enabled = false;
            vslviewBtn.Enabled = visualizeBtn.Checked;
            saveMatrixDumbBtn.Enabled = visualizeBtn.Checked;
            iStartBtn.Enabled = false;
            iStartMtx.Enabled = false;
            runFHoleBtn.Enabled = false;
            getThreadsInfoToolStripMenuItem.Enabled = true;
            consoleRun.Enabled = false;
            iStopBtn.Enabled = true;
            saveExBtn.Enabled = false;
            opBtn.Enabled = false;
            npBtn.Enabled = false;
            exBtn.Enabled = false;

            MapnikTileRendererProject proj = ReadFormToProject();

            rGrpZoom = -1;

            if (holes == null)
            {
                _runtime_thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(RunThread));
                _runtime_thread.Start(new object[] { proj, loadDumpFile });
            }
            else
            {
                saveMatrixDumbBtn.Enabled = false;
                _runtime_thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(RunThreadHoles));
                _runtime_thread.Start(new object[] { proj, holes });
            };            
        }
       
        private void StopRendering(bool terminate)
        {
            TileRendering.TileRenderingErrors.PauseNormal = false;

            tabControl1.Enabled = false;
            tabControl1.Visible = true;
            tabControl2.Height = 418;
            tabControl2.Top = 322;
            visualizeBtn.Enabled = false;
            saveMatrixDumbBtn.Enabled = false;
            iStartBtn.Enabled = false;
            iStartMtx.Enabled = false;
            runFHoleBtn.Enabled = false;
            getThreadsInfoToolStripMenuItem.Enabled = true;
            consoleRun.Enabled = false;
            iStopBtn.Enabled = false;
            saveExBtn.Enabled = false;
            opBtn.Enabled = false;
            npBtn.Enabled = false;
            exBtn.Enabled = false;

            // Stop Main Zoom Loop & Queue Zoom Thread
            _runtime_zooms_todo[0] = false;

            // Call Stop Threads
            for (int i = 0; i < _runtime_renderers.Length; i++)
                if (_runtime_renderers[i] != null)
                    _runtime_renderers[i].BreakThreads(terminate, 10);

            // Позволяем потоку зумов завершиться нормально
            //if(_runtime_thread != null)
            //    _runtime_thread.Join(1500);

            //StopRenderingOk();
        }

        private void StopRenderingOk()
        {
            timerOnTickUpdateFile = 0;
            _runtime_stopped[0] = DateTime.Now;

            tabControl1.Enabled = true;
            tabControl1.Visible = true;
            tabControl1.Visible = true;
            tabControl2.Height = 418;
            tabControl2.Top = 322;
            visualizeBtn.Enabled = true;
            saveMatrixDumbBtn.Enabled = false;
            iStartBtn.Enabled = true;
            iStartMtx.Enabled = true;
            runFHoleBtn.Enabled = true;
            getThreadsInfoToolStripMenuItem.Enabled = false;
            consoleRun.Enabled = true;
            iStopBtn.Enabled = false;
            saveExBtn.Enabled = true;
            opBtn.Enabled = true;
            npBtn.Enabled = true;
            exBtn.Enabled = true;            
        }

        private void iStopBtn_Click(object sender, EventArgs e)
        {
            StopProcessDialog spd = new StopProcessDialog();
            if (spd.ShowDialog() == DialogResult.Cancel)
            {
                spd.Dispose();
                return;
            };
            bool kill = spd.stopKill.Checked;
            spd.Dispose();

            StopRendering(kill);
        }

        private void RunThreadHoles(object data)
        {
            MapnikTileRendererProject proj = (MapnikTileRendererProject)((object[])data)[0];
            TileRendering.FHoles fh = (TileRendering.FHoles)((object[])data)[1];

            int tzs = (int)(proj.RendererConfig.tileZoneSize);
            tzs = tzs * tzs;

            _runtime_zooms_todo[0] = true; // started
            _runtime_total_to_render = 0;
            _runtime_zooms_done = 0;
            _runtime_started[0] = DateTime.Now;
            _runtime_stopped[0] = DateTime.MinValue;
            Queue<int> zooms2run = new Queue<int>();
            for (int i = 1; i < proj.zooms.Length; i++)
            {
                _runtime_renderers[i] = null;
                _runtime_started[i] = DateTime.MinValue;
                _runtime_stopped[i] = DateTime.MinValue;                
                ulong inz = fh.CountIn(i) * (ulong)tzs;
                if(inz > 0)
                {
                    _runtime_zooms_todo[i] = true;
                    zooms2run.Enqueue(i);
                    _runtime_total_to_render += inz;
                };
            };
            
            while ((zooms2run.Count > 0) && (_runtime_zooms_todo[0]))
            {
                int currentZoom = zooms2run.Dequeue();
                _runtime_current_zoom = currentZoom;
                _runtime_started[currentZoom] = DateTime.Now;

                string tilesPath = proj.TilesPath + @"\";
                if (proj.RendererConfig.pathStructure == TileRendering.TileRenderPathScheme.fsArcGIS)
                {
                    tilesPath = proj.TilesPath + @"\_alllayers\";
                    try
                    {
                        System.IO.Directory.CreateDirectory(tilesPath);
                        System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() +
                            (currentZoom < 10 ? @"\ArcGIS_conf[04..09].xml" : @"\ArcGIS_conf[10..21].xml"),
                            proj.TilesPath + @"\conf.xml", true);
                        if (!System.IO.File.Exists(proj.TilesPath + @"\ReadMeFirst.txt"))
                            System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() + "ArcGIS_RMF.txt", proj.TilesPath + @"\ReadMeFirst.txt", false);
                        if (!System.IO.File.Exists(proj.TilesPath + @"\MergedTilesSimpleViewer.exe"))
                            System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() + "MergedTilesSimpleViewer.bin", proj.TilesPath + @"\MergedTilesSimpleViewer.exe", false);
                        if (!System.IO.File.Exists(proj.TilesPath + @"\MakeTRZ.cmd"))
                            System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() + "MakeTRZ.txt", proj.TilesPath + @"\MakeTRZ.cmd", false);
                    }
                    catch { };
                };

                try
                {
                    _runtime_renderers[currentZoom] = new TileRendering.MultithreadTileRenderer(proj.MapFile, tilesPath, proj.RendererConfig);
                    _runtime_renderers[currentZoom].RenderTilesList(fh.ZonesIn(currentZoom), currentZoom);
                    _runtime_stopped[currentZoom] = DateTime.Now;
                    _runtime_zooms_done++;
                }
                catch { _runtime_zooms_todo[0] = false; };
                System.Threading.Thread.Sleep(1);
            };

            //  Wait All Threads If They Still Running
            if (_runtime_renderers.Length > 0)
                for (int i = 0; i < _runtime_renderers.Length; i++)
                    if (_runtime_renderers[i] != null)
                        while (_runtime_renderers[i].GetMultitreadThreadIDs().Length > 0)
                            System.Threading.Thread.Sleep(250);

            this.Invoke(new System.Threading.ThreadStart(StopRenderingOk));

            System.Threading.Thread.Sleep(2500);
            _runtime_thread = null;
        }

        // Save Dump Every 1 hour //
        private DateTime lastDUMPauto = DateTime.MinValue;
        private void AutoSaveDump()
        {
            if (!TileRendering.TileRenderingErrors.WithDump) return;
            if (DateTime.UtcNow.Subtract(lastDUMPauto).TotalHours < 3) return;
            lastDUMPauto = DateTime.UtcNow;

            List<TileRendering.RenderView> data = new List<TileRendering.RenderView>();
            if ((_runtime_renderers != null) && (_runtime_renderers.Length > 0))
                for (int i = 0; i < _runtime_renderers.Length; i++)
                    if (_runtime_renderers[i] != null)
                        if (_runtime_renderers[i].VIEW != null)
                            data.Add(_runtime_renderers[i].VIEW);

            if (data.Count > 0)
            {
                try
                {
                    System.IO.FileStream fs = new System.IO.FileStream(TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\main_[" + System.IO.Path.GetFileName(_project_filename) + "].dump", System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    fs.Write(TileRendering.RenderView.fileheader, 0, TileRendering.RenderView.fileheader.Length);
                    foreach (TileRendering.RenderView d in data)
                        for (int i = 0; i < d.Size; i++)
                        {
                            byte b = d.GetByte(i);
                            fs.WriteByte(b);
                        };
                    fs.Close();
                }
                catch { };
            };
        }

        private void RunThread(object data)
        {
            MapnikTileRendererProject proj = (MapnikTileRendererProject)((object[])data)[0];
            string loadDumpFile = (string)((object[])data)[1];
            
            // определеям координаты зоны
            double sx = proj.Zone[1];
            double sy = proj.Zone[2];
            double ex = proj.Zone[3];
            double ey = proj.Zone[4]; 
            {
                //градусы                
                //if (proj.Zone[0] == 0) { };
                
                //метры проекции
                if (proj.Zone[0] == 1)
                {
                    sx = TileRendering.TilesProjection.x2lon_m(sx);
                    sy = TileRendering.TilesProjection.y2lat_m(sy);
                    ex = TileRendering.TilesProjection.x2lon_m(ex);
                    ey = TileRendering.TilesProjection.y2lat_m(ey);
                };
                //номера тайлов
                if (proj.Zone[0] == 2)
                {
                    double tmp_lat; double tmp_lon;
                    TileRendering.TilesProjection.fromTileToLL((int)sx, (int)sy, (int)proj.Zone[5] + 1, out tmp_lat, out tmp_lon);
                    sx = tmp_lon; sy = tmp_lat;
                    TileRendering.TilesProjection.fromTileToLL((int)ex, (int)ey, (int)proj.Zone[5] + 1, out tmp_lat, out tmp_lon);
                    ex = tmp_lon; ey = tmp_lat;
                };
                //координаты в пикселях
                if (proj.Zone[0] == 3)
                {
                    double[] ll = TileRendering.TilesProjection.fromPixelToLL(new double[] { sy, sx }, (int)proj.Zone[5] + 1);
                    sx = ll[0]; sy = ll[1];
                    ll = TileRendering.TilesProjection.fromPixelToLL(new double[] { ey, ex }, (int)proj.Zone[5] + 1);
                    ex = ll[0]; ey = ll[1];
                };
            };

            _runtime_zooms_todo[0] = true; // started
            _runtime_total_to_render = 0;
            _runtime_zooms_done = 0;
            _runtime_started[0] = DateTime.Now;
            _runtime_stopped[0] = DateTime.MinValue;
            Queue<int> zooms2run = new Queue<int>();
            for (int i = 1; i < proj.zooms.Length; i++)
            {
                _runtime_renderers[i] = null;
                _runtime_started[i] = DateTime.MinValue;
                _runtime_stopped[i] = DateTime.MinValue;
                _runtime_zooms_todo[i] = proj.zooms[i];
                if (proj.zooms[i])
                {
                    zooms2run.Enqueue(i);
                    _runtime_total_to_render += TileRendering.MultithreadTileRenderer.RenderTilesCount(sy, sx, ey, ex, i, proj.RendererConfig.tileZoneSize);
                    //_runtime_total_to_render = 10; //!!!
                    ulong fSize = _runtime_total_to_render / 8 + 1;
                };
            };

            while ((zooms2run.Count > 0) && (_runtime_zooms_todo[0]))
            {
                int currentZoom = zooms2run.Dequeue();
                _runtime_current_zoom = currentZoom;
                _runtime_started[currentZoom] = DateTime.Now;

                string tilesPath = proj.TilesPath + @"\";
                if (proj.RendererConfig.pathStructure == TileRendering.TileRenderPathScheme.fsArcGIS)
                {
                    tilesPath = proj.TilesPath + @"\_alllayers\";
                    try
                    {
                        System.IO.Directory.CreateDirectory(tilesPath);
                        System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() +
                            (currentZoom < 10 ? @"\ArcGIS_conf[04..09].xml" : @"\ArcGIS_conf[10..21].xml"),
                            proj.TilesPath + @"\conf.xml",true);
                        if (!System.IO.File.Exists(proj.TilesPath + @"\ReadMeFirst.txt"))
                            System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() + "ArcGIS_RMF.txt", proj.TilesPath + @"\ReadMeFirst.txt", false);
                        if (!System.IO.File.Exists(proj.TilesPath + @"\MergedTilesSimpleViewer.exe"))
                            System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() + "MergedTilesSimpleViewer.bin", proj.TilesPath + @"\MergedTilesSimpleViewer.exe", false);
                        if (!System.IO.File.Exists(proj.TilesPath + @"\MakeTRZ.cmd"))
                            System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() + "MakeTRZ.txt", proj.TilesPath + @"\MakeTRZ.cmd", false);                        
                    }
                    catch { };
                };

                try
                {
                    _runtime_renderers[currentZoom] = new TileRendering.MultithreadTileRenderer(proj.MapFile, tilesPath, proj.RendererConfig, loadDumpFile);
                    _runtime_renderers[currentZoom].RenderTiles(sy, sx, ey, ex, currentZoom);
                    _runtime_stopped[currentZoom] = DateTime.Now;
                    _runtime_zooms_done++;
                }
                catch { _runtime_zooms_todo[0] = false; };                
                System.Threading.Thread.Sleep(1);
            };

            //  Wait All Threads If They Still Running
            if(_runtime_renderers.Length > 0)
                for (int i = 0; i < _runtime_renderers.Length; i++)
                    if(_runtime_renderers[i] != null)
                        while(_runtime_renderers[i].GetMultitreadThreadIDs().Length > 0)
                            System.Threading.Thread.Sleep(250);
            
            this.Invoke(new System.Threading.ThreadStart(StopRenderingOk));

            System.Threading.Thread.Sleep(2500);
            _runtime_thread = null;
        }

        private bool timerOnTickIsRunning = false;
        private byte timerOnTickUpdateFile = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {            
            if (_runtime_thread == null) return;
            if (timerOnTickIsRunning) return;
            timerOnTickIsRunning = true;

            AutoSaveDump();

            ulong total_created = 0;
            ulong total_passed = 0;
            ulong total_skipped = 0;
            ulong total_witherr = 0;
            ulong total_zoneerr = 0;
            ulong total_to_render = 0;
            double total_size = 0;
            int total_zooms = 0;
            List<int> checkedZooms = new List<int>();
            string chZooms = "";
            for (int i = 1; i < _runtime_zooms_todo.Length; i++)
            {
                if (_runtime_zooms_todo[i])
                {
                    total_zooms++;
                    checkedZooms.Add(i);
                    chZooms += chZooms.Length > 0 ? ", " +i.ToString() : i.ToString() ;
                };
            };

            string status = "";
            status += String.Format("Состояние: {0}, нарезано {1} зумов из {2}\r\n", (_runtime_stopped[0] <= DateTime.MinValue) ? (TileRendering.TileRenderingErrors.PauseNormal ? "Приостановлена" : "Идет") + " нарезка " + _runtime_current_zoom.ToString() + " зума" : (_runtime_zooms_todo[0] ? "Нарезка завершена" : "Нарезка прервана"), _runtime_zooms_done, total_zooms);
            status += String.Format("Выбранные зумы для нарезки: {0}\r\n", chZooms);
            status += String.Format("Зона для нарезки: ( {0} ; {1} ) .. ( {2} ; {3} ) - {4}\r\n", new object[] { isX.Text, isY.Text, ieX.Text, ieY.Text, iBoxFormat.Items[iBoxFormat.SelectedIndex].ToString() });
            status += String.Format("Размер изображения при отрисовки карты (зоны): {0}\r\n", iTileRenderZoneSize.Text);
            status += String.Format("При ошибках отрисовки пробывать нарезать зону повторно: {0}\r\n", iRerender.Checked ? "да" : "нет");
            status += String.Format("Если сохряняемый тайл уже существует: {0}\r\n", iTileFinalMerging.Text.ToLower());
            status += String.Format("Обрабатывать тайлы только попадающие в полигон/полилиную: {0}\r\n", irPolygon.Text != String.Empty ? "`" + System.IO.Path.GetFileName(irPolygon.Text) + "`" : "нет");
            status += String.Format("Накладывать водяной знак из файла: {0}\r\n", iaddCopyrightsConfig.Text != String.Empty ? "`"+System.IO.Path.GetFileName(iaddCopyrightsConfig.Text)+"`" : "нет");
            status += String.Format("Число потоков: {0}, ошибок: {1}\r\n", imultithreadCount.Value, TileRendering.TileRenderingErrors.ErrorQueueTotalCount);
            status += String.Format("\r\nНачато: {0}\r\n", _runtime_started[0]);
            for (int i = 0; i < _runtime_zooms_todo.Length; i++)
                if((checkedZooms.IndexOf(i+1)>=0) && ((_runtime_zooms_done == total_zooms) || (_runtime_current_zoom >= (i+1))))
                {
                    status += "\r\n";
                    //status += String.Format("Зум {0}\r\n", i+1);
                    status += String.Format(ni, "Зум {0}, выполнено {1:0.0}%\r\n", i + 1, ((double)_runtime_renderers[i + 1].status_total_passed) / ((double)_runtime_renderers[i + 1].status_total_to_render) * 100.0);
                    status += String.Format("  - Начато: {0}\r\n", _runtime_started[i + 1]);
                    if (_runtime_renderers[i + 1] != null)
                    {
                        status += String.Format("  - Тайлов обработано: {0} из {1}, создано {2}, пропущено {3}, дыр {4} в {5} квдр\r\n", new object[] { _runtime_renderers[i + 1].status_total_passed, _runtime_renderers[i + 1].status_total_to_render, _runtime_renderers[i + 1].status_total_created, _runtime_renderers[i + 1].status_total_skipped, _runtime_renderers[i + 1].status_total_witherr, _runtime_renderers[i+1].status_total_zoneerr });
                        status += String.Format(ni, "  - Размер созданных тайлов: {0}\r\n", TileRendering.TileRenderer.FileSizeSuffix(_runtime_renderers[i + 1].status_total_size));
                        status += String.Format("  - Номера тайлов: x {0} из  {1}..{2}, y {3} из  {4}..{5}\r\n", new object[] { _runtime_renderers[i + 1].status_x_current, _runtime_renderers[i + 1].status_x_start, _runtime_renderers[i + 1].status_x_end, _runtime_renderers[i + 1].status_y_current, _runtime_renderers[i + 1].status_y_start, _runtime_renderers[i + 1].status_y_end });

                        total_created += _runtime_renderers[i + 1].status_total_created;
                        total_passed += _runtime_renderers[i + 1].status_total_passed;
                        total_skipped += _runtime_renderers[i + 1].status_total_skipped;
                        total_witherr += _runtime_renderers[i + 1].status_total_witherr;
                        total_zoneerr += _runtime_renderers[i + 1].status_total_zoneerr;
                        total_to_render += _runtime_renderers[i + 1].status_total_to_render;
                        total_size += _runtime_renderers[i + 1].status_total_size;
                    };
                    if ((_runtime_stopped[i + 1] > DateTime.MinValue) && (_runtime_renderers[i + 1].status_total_passed == _runtime_renderers[i + 1].status_total_to_render))
                    {
                        status += String.Format("  - Закончено: {0}\r\n", _runtime_stopped[i + 1]);
                        TimeSpan ts = _runtime_stopped[i + 1].Subtract(_runtime_started[i + 1]);
                        status += String.Format("  - Выполнялось: {0} дн {1} ч {2} м {3} c\r\n", new object[] { (int)ts.TotalDays, ts.Hours, ts.Minutes, ts.Seconds });
                    }
                    else if (_runtime_stopped[0] > DateTime.MinValue)
                    {
                        status += String.Format("  - Прервано: {0}\r\n", _runtime_stopped[0]);
                        TimeSpan ts = _runtime_stopped[0].Subtract(_runtime_started[i + 1]);
                        status += String.Format("  - Выполнялось: {0} дн {1} ч {2} м {3} c\r\n", new object[] { (int)ts.TotalDays, ts.Hours, ts.Minutes, ts.Seconds });
                    }
                    else
                    {
                        TimeSpan ts = DateTime.Now.Subtract(_runtime_started[i + 1]);
                        //status += String.Format("  - Выполнено: {0}%\r\n", (int)(((double)_runtime_renderers[i + 1].status_total_passed) / ((double)_runtime_renderers[i + 1].status_total_to_render) * 100.0));
                        status += String.Format("  - Выполняется: {0} дн {1} ч {2} м\r\n", new object[] { (int)ts.TotalDays, ts.Hours, ts.Minutes, ts.Seconds });
                    };
                };

            status += String.Format("\r\nНарезано {0} зумов из {1}\r\n", _runtime_zooms_done, total_zooms);
            status += String.Format("Тайлов обработано: {0} из {1}/{4}, создано {2}, пропущено {3}, дыр {5} в {6} квдр\r\n", new object[] { total_passed, total_to_render, total_created, total_skipped, _runtime_total_to_render, total_witherr, total_zoneerr });
            status += String.Format(ni, "Размер созданных тайлов: {0}\r\n", TileRendering.TileRenderer.FileSizeSuffix(total_size));

            if (_runtime_stopped[0] > DateTime.MinValue)
            {
                if (!_runtime_zooms_todo[0])
                    status += String.Format("Прервано: {0}\r\n", _runtime_stopped[0]);
                else
                    status += String.Format("Законечно: {0}\r\n", _runtime_stopped[0]);
                TimeSpan ts = _runtime_stopped[0].Subtract(_runtime_started[0]);
                status += String.Format("Выполнялось: {0} дн {1} ч {2} м {3} c\r\n", new object[] { (int)ts.TotalDays, ts.Hours, ts.Minutes, ts.Seconds });
            }
            else
            {
                if (!_runtime_zooms_todo[0])
                    status += "Нарезка в процессе остановки... Подождите пока нарежутся текущие квадраты.\r\n";
                TimeSpan ts = DateTime.Now.Subtract(_runtime_started[0]);
                //status += String.Format("Выполнено: {0}%\r\n", (int)(((double)total_passed) / ((double)_runtime_total_to_render) * 100.0));
                status += String.Format("Выполняется: {0} дн {1} ч {2} м\r\n", new object[] { (int)ts.TotalDays, ts.Hours, ts.Minutes, ts.Seconds });                
            };


            status += String.Format(ni, "Выполнено {0:0.0}%\r\n", ((double)total_passed) / ((double)_runtime_total_to_render) * 100.0);

            ipc.Text = String.Format("{0}%", (int)(((double)total_passed) / ((double)_runtime_total_to_render) * 100.0));

            if (iStatus.Text != status)
            {
                iStatus.Text = status;
                iStatus.SelectionStart = iStatus.Text.Length - 1;
                iStatus.SelectionLength = 0;
                iStatus.ScrollToCaret();                
            };

            if (timerOnTickUpdateFile++ % 8 == 0) // 1 time per 8 seconds
            {
                try
                {
                    string fileName = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\NO_NAME_PROJECT";
                    if (_project_filename != "") fileName = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\main_[" + System.IO.Path.GetFileName(_project_filename) + "]";
                    System.IO.FileStream fs = new System.IO.FileStream(fileName + ".log", System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    byte[] data = System.Text.Encoding.GetEncoding(1251).GetBytes(status);
                    fs.Write(data, 0, data.Length);
                    fs.Close();
                }
                catch { };
            };


            int cval = (int)(((double)total_passed) / ((double)_runtime_total_to_render) * 10000.0);
            iProgress.Value = cval >= 0 ? cval : 0;

            if (_runtime_zooms_todo[0])
            {
                if ((iProgress.Value < iProgress.Maximum) && (iProgress.ForeColor != SystemColors.Highlight))
                    iProgress.ForeColor = SystemColors.Highlight;
            }
            else
            {
                if ((iProgress.Value < iProgress.Maximum) && (iProgress.ForeColor != Color.Maroon))
                    iProgress.ForeColor = Color.Maroon;
            };
            if ((iProgress.Value == iProgress.Maximum) && (iProgress.ForeColor != Color.Green))
                iProgress.ForeColor = Color.Green;

            // THread State
            if (tabControl2.SelectedIndex == 1)
            {
                string txt = "";
                if ((_runtime_renderers != null) && (_runtime_renderers.Length > 0))
                {
                    if (TileRendering.TileRenderingErrors.PauseNormal)
                    {
                        int ttl_c = 0;
                        for (int ri = 0; ri < _runtime_renderers.Length; ri++)
                            if (_runtime_renderers[ri] != null)
                            {
                                uint[] tuids = _runtime_renderers[ri].GetMultitreadThreadIDs();
                                if (tuids != null) ttl_c += tuids.Length;
                            };
                        if (((ulong)ttl_c) > TileRendering.TileRenderingErrors.PausedNormal)
                        {
                            txt += "Нарезка приостановлена, для полной пристановки дождитесь завершения каждым из потоков текущего задания.\r\n";
                            txt += String.Format("На текащий момент приостановлено {0} потоков из {1}\r\n\r\n", TileRendering.TileRenderingErrors.PausedNormal, ttl_c);
                        }
                        else
                        {
                            txt += "Нарезка приостановлена, для возобновления нарезки воспользуйтесь меню.\r\n";
                            txt += String.Format("На текащий момент приостановлено {0} потоков из {1}\r\n\r\n", TileRendering.TileRenderingErrors.PausedNormal, ttl_c);
                        }
                    }
                    else
                    {
                        txt += "Здесь выводится общая информация о работе потоков.\r\n";
                        txt += "Вы можете приостановить нарезку, если вам необходимо высвобождение процессорного времени на некоторое время\r\n\r\n";
                    };
                    for (int ri = 0; ri < _runtime_renderers.Length; ri++)
                        if (_runtime_renderers[ri] != null)
                        {
                            try
                            {
                                System.Threading.Thread[] tids = _runtime_renderers[ri].GetMultitreadThreads();
                                if ((tids == null) || (tids.Length == 0)) continue;
                                TileRendering.MultithreadTileRenderer.ThreadRenderObject[] inf = _runtime_renderers[ri].GetMultitreadThreadInfo();

                                for (int x = 0; x < tids.Length; x++)
                                {                                    
                                    txt += "THREAD " + inf[x].threadID.ToString();
                                    txt += " ТАЙЛ " + inf[x].nowData[0].ToString() + "x " + inf[x].nowData[1].ToString() + "y " + inf[x].nowData[2].ToString() + "z";
                                    //txt += "\t\t[" + tids[x].Name + "] ";
                                    txt += "\r\n";                                    
                                    txt += "Тайл обр: " + inf[x].tilesPassed.ToString();
                                    txt += ", сзд : " + inf[x].tilesCreated.ToString();
                                    txt += ", ппщ : " + inf[x].tilesSkipped.ToString();
                                    txt += ", дыр : " + inf[x].tilesWithErr.ToString();
                                    txt += " в " + inf[x].tilesZoneErr.ToString() + " квдр\r\n";
                                    txt += "Зон обр: " + inf[x].zonesPassed.ToString();
                                    if ((inf[x].lastData != null) && (inf[x].lastData.Length > 2))
                                        txt += ", последняя: " + inf[x].lastData[0].ToString() + "x " + inf[x].lastData[1].ToString() + "y " + inf[x].lastData[2].ToString() + "z";
                                    txt += "\r\nОбщий размер созданных тайлов: ";
                                    txt += TileRendering.TileRenderer.FileSizeSuffix(inf[x].tilesSize) + "\r\n";                                    
                                    txt += "Приоритет: " + tids[x].Priority.ToString();
                                    txt += ", Состояние: " + tids[x].ThreadState.ToString() + " \r\n\r\n";
                                };
                            }
                            catch { };
                        };
                };
                if(threadState.Text != txt) threadState.Text = txt;
            };


            //
            // LOG ERRORS //
            //

            // read stack errors
            List<TileRendering.TileRenderingErrors.ErrorInfo> errors = new List<TileRendering.TileRenderingErrors.ErrorInfo>();            
            TileRendering.TileRenderingErrors.ErrorQueueMutex.WaitOne();
            ulong errCnt = TileRendering.TileRenderingErrors.ErrorQueueTotalCount;
            while (TileRendering.TileRenderingErrors.ErrorQueue.Count > 0)
                errors.Add(TileRendering.TileRenderingErrors.ErrorQueue.Dequeue());
            TileRendering.TileRenderingErrors.ErrorQueueMutex.ReleaseMutex();

            errCount.Text = " "+errCnt.ToString(); 
            if (errors.Count > 0)
                for (int i = 0; i < errors.Count; i++)
                {
                    TileRendering.TileRenderingErrors.ErrorInfo err = errors[i];
                    string txt = String.Format(
                        "{0}\r\nTHREAD {4} TILE {1}z {2}y {3}z\r\n{5}: {6}\r\nParameters: {7}"
                        , new object[]{
                        err.dt, err.x, err.y, err.z, err.ThreadID, err.comment, err.ex.Message, err.parameter
                    }); 
                   
                    // update only last
                    if(i == (errors.Count -1))
                        logErrors.Text = txt;

                    txt = "\r\n" + txt + "\r\n";

                    // Add Errors To Log
                    try
                    {
                        string fileName = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\NO_NAME_PROJECT";
                        if (_project_filename != "") fileName = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\main_[" + System.IO.Path.GetFileName(_project_filename) + "]";
                        System.IO.FileStream fs = new System.IO.FileStream(fileName+".errorlog", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                        byte[] data = System.Text.Encoding.GetEncoding(1251).GetBytes(txt);
                        fs.Position = fs.Length;
                        fs.Write(data, 0, data.Length);
                        fs.Close();
                    }
                    catch { };

                    if (err.comment == "Error Tile Optimization") continue;

                    // check paused threads
                    bool paused = TileRendering.TileRenderingErrors.PausedThreads.Count > 0;
                    if (paused)
                    {
                        TileRendering.TileRenderingErrors.PausedThreadsMutex.WaitOne();
                        bool ex = TileRendering.TileRenderingErrors.PausedThreads.IndexOf(err.ThreadID) >= 0;
                        TileRendering.TileRenderingErrors.PausedThreadsMutex.ReleaseMutex();
                        if (ex)
                        {
                            DialogResult dr = MessageBox.Show("При выполнении нарезки в одном из потоков возникла ошибка: \r\n" + txt + "\r\nПрервать выполнение всех операций?\r\nДа - Прервать нарезку\r\nНет - Не прерывать, продолжить нарезку\r\nОтмена - Продолжить нарезку и больше не выводить сообщение об ошибке", "ОШИБКА", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
                            if (dr == DialogResult.Yes)
                            {
                                if (TileRendering.TileRenderingErrors.WithDump)
                                {
                                    TileRendering.TileRenderingErrors.PauseNormal = true;
                                    if(MessageBox.Show("Хотите ли сохранить текущий дамп нарезки в файл?","Сохранение дампа",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes)
                                        saveMatrixDumbBtm_Click(sender, e);
                                    TileRendering.TileRenderingErrors.PauseNormal = false;
                                };
                                StopRendering(true);
                                return;
                            }
                            else
                            {
                                if (dr == DialogResult.Cancel)
                                {
                                    pauseIfErrorSet(false, false);
                                    TileRendering.TileRenderingErrors.PausedThreadsMutex.WaitOne();
                                    TileRendering.TileRenderingErrors.PausedThreads.Clear();
                                    TileRendering.TileRenderingErrors.PausedThreadsMutex.ReleaseMutex();
                                }
                                else
                                {
                                    TileRendering.TileRenderingErrors.PausedThreadsMutex.WaitOne();
                                    TileRendering.TileRenderingErrors.PausedThreads.Remove(err.ThreadID);
                                    TileRendering.TileRenderingErrors.PausedThreadsMutex.ReleaseMutex();
                                };
                            };
                        };
                    };
                };
            
            timerOnTickIsRunning = false;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!iStartBtn.Enabled)
            {
                MessageBox.Show("Нельзя закрыть приложение, пока идет нарезка", "Закрыть приложение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            };
        }

        private void consoleRun_Click(object sender, EventArgs e)
        {
            if (_project_filename == "")
            {
                MessageBox.Show("Необходимо cперва сохранить файл!");
                return;
            };

            if (MessageBox.Show("Вы действительно хотите запустить нарезку тайлов для выбранных уровней из-под консоли?\r\nP.S: Перед запуском все изменения настроек необходимо сохранить в файл проекта!", "Нарезка тайлов", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No) return;

            System.Diagnostics.Process.Start(TileRendering.TileRenderer.GetCurrentDir() + @"\mtrc.exe", "\"" + _project_filename + "\" -wait");
        }        

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // почему то на некоторых машинах подвисает, поэтому принудительно
            // убиваем из другого приложения
            System.Diagnostics.Process.Start(TileRendering.TileRenderer.GetCurrentDir() + @"\nircmd.exe","cmdwait 1500 killprocess MapnikTileRenderer.exe");

            // Убиваем все незавершенные потоки на случай зависания
            System.Diagnostics.ProcessThreadCollection ttk = System.Diagnostics.Process.GetCurrentProcess().Threads;
            foreach (System.Diagnostics.ProcessThread pc in ttk)
            {
                IntPtr ptrThread = OpenThread(1, false, (uint)pc.Id);
                if (AppDomain.GetCurrentThreadId() != pc.Id)
                    try { TerminateThread(ptrThread, 1); } catch { };
            };
            
            uint current = TileRendering.MultithreadTileRenderer.GetCurrentThreadId();
            try { TerminateThread(OpenThread(1, false, current), 1); }
            catch { };            
        }

        private void savelogBtn(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (_project_filename != null) sfd.FileName = _project_filename + ".log.txt";
            sfd.DefaultExt = ".txt";
            sfd.Filter = "Text Files (*.txt)|*.txt";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                byte[] txt = System.Text.Encoding.GetEncoding(1251).GetBytes(iStatus.Text);
                fs.Write(txt, 0, txt.Length);
                fs.Close();
            };
            sfd.Dispose();
        }

        private void проверитьКорректностьРазметкиXMLФайлаКартыToolStripMenuItem_Click(object sender, EventArgs e)
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
                MessageBox.Show("Ошибок синтаксиса не выявлено", "Разбора XML файла карт",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка разбора XML файла карты", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            };
            xd = null;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string shpfile = "";
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Загрузить границы из shape файла";
            ofd.FileName = "bounds.shp";
            ofd.DefaultExt = ".shp";
            ofd.Filter = "Shape Files (*.shp)|*.shp|All types (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
                shpfile = ofd.FileName;
            ofd.Dispose();
            if (shpfile == "") return;

            Polygon Area = readShapeFile(shpfile);
            isY.Text = Area.box[3].ToString(ni);
            isX.Text = Area.box[0].ToString(ni);
            ieY.Text = Area.box[1].ToString(ni);
            ieX.Text = Area.box[2].ToString(ni);
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
               
        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            browseBtn.Enabled = System.IO.Directory.Exists(iTilesDir.Text);

            bool v = Directory.Exists(@"V:\");
            vdCreateOnly.Enabled = !v;
            vdCreate.Enabled = !v && File.Exists(iMapFileName.Text);
            vdOpenCreate.Enabled = !v;
            vdProxy.Enabled = !v;
            vdResize.Enabled = v;
            vdDelete.Enabled = v;
            vdOpen.Enabled = v;
            vdCopy.Enabled = v && File.Exists(iMapFileName.Text);
            vdSave.Enabled = v;

            getThreadsInfoToolStripMenuItem.Text = (TileRendering.TileRenderingErrors.PauseNormal ? "Возобновить нарезку" : "Приостановить нарезку");
        }

        private void browseBtn_Click(object sender, EventArgs e)
        {
            BrowseTileFolder btf = new BrowseTileFolder();            
            btf.map.NotFoundTileColor = Color.LightYellow;
            btf.map.TilesRenderingZoneSize = ((short)(iTileRenderZoneSize.SelectedIndex+1));
            btf.toolStripComboBox1.SelectedIndex = iTileRenderZoneSize.SelectedIndex;
            btf.Show();
            if(iTileRenderPathScheme.SelectedIndex == 1)
                btf.map.ImageSourceUrl = btf.current_project_dir = iTilesDir.Text + @"\_alllayers\L{l}\R{r}\C{c}.png";
            else
                btf.map.ImageSourceUrl = btf.current_project_dir = iTilesDir.Text + @"\{z}\{x}\{y}.png";

            double sx; double sy; double ex; double ey; double cx; double cy;
            double.TryParse(isX.Text, System.Globalization.NumberStyles.Float, ni, out sx);
            double.TryParse(isY.Text, System.Globalization.NumberStyles.Float, ni, out sy);
            double.TryParse(ieX.Text, System.Globalization.NumberStyles.Float, ni, out ex);
            double.TryParse(ieY.Text, System.Globalization.NumberStyles.Float, ni, out ey);
            cx = (sx + ex) / 2.0;
            cy = (sy + ey) / 2.0;

            //градусы
            if (iBoxFormat.SelectedIndex == 0)
                btf.map.CenterDegrees = new PointF((float)cx, (float)cy);
            
            //метры проекции
            if (iBoxFormat.SelectedIndex == 1)
                btf.map.CenterMeters = new PointF((float)cx, (float)cy);
            //номера тайлов
            if (iBoxFormat.SelectedIndex == 2)
            {
                btf.map.ZoomID = ((byte)(iBoxZoom.SelectedIndex + 1));
                btf.map.CenterPixels = new Point((int)cx * 256, (int)cy * 256);
            };
            //координаты в пикселях
            if (iBoxFormat.SelectedIndex == 3)
            {
                btf.map.ZoomID = ((byte)(iBoxZoom.SelectedIndex + 1));
                btf.map.CenterPixels = new Point((int)cx, (int)cy);
            };

            btf.map.DrawMap = true;
        }

        private void pauseIfErrorSet(bool pause, bool p100)
        {
            if (p100) pause = false;
            pauseIfError.Checked = pause;            
            pauseIf100.Checked = p100;

            TileRendering.TileRenderingErrors.PauseThreadsIfErrorsCountIs = 0;
            if (pause)
                TileRendering.TileRenderingErrors.PauseThreadsIfErrorsCountIs = 1;
            if (p100)
                TileRendering.TileRenderingErrors.PauseThreadsIfErrorsCountIs = 100;            
        }

        private void pauseIfError_Click(object sender, EventArgs e)
        {
            pauseIfErrorSet(!pauseIfError.Checked, false);
        }

        private void pauseIf100_Click(object sender, EventArgs e)
        {
            pauseIfErrorSet(false, !pauseIf100.Checked);
        }

        private void iTileOptimizeMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = iTileOptimizeMethod.SelectedIndex;
            iPNGFormat.Enabled = (i == 3) || (i == 5) || (i == 6);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            irPolygon.Text = "";
        }

        private void button13_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Загрузить полигон в проекции EPSG:3857 из shape файла";
            ofd.FileName = "bounds.shp";
            ofd.DefaultExt = ".shp";
            ofd.Filter = "Shape Files (*.shp)|*.shp|All types (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
                irPolygon.Text = ofd.FileName;
            ofd.Dispose();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (!File.Exists(irPolygon.Text)) return;

            byte type = 0;
            {
                FileStream fs = new FileStream(irPolygon.Text, FileMode.Open, FileAccess.Read);
                fs.Position = 32;
                type = (byte)fs.ReadByte();
                fs.Close();
            };

            if ((type != 3) && (type != 5))
            {
                MessageBox.Show("Данный тип shape файла не поддерживается!\r\nПоддерживаются только shape файлы полилиний и полигонов", "Shape файл", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            };

            string tp = type == 5 ? "полигон" : "полилиния";
            string tn = type != 5 ? "полигон" : "полилиния";

            if (MessageBox.Show("Тип shape файла: " + tp + "\r\n\r\nВы желаете сменить тип на: " + tn + "\r\n", "Shape файл", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                FileStream fs = new FileStream(irPolygon.Text, FileMode.Open, FileAccess.Write);
                fs.Position = 32;
                fs.WriteByte(type == 5 ? (byte)3 : (byte)5);
                fs.Close();
                MessageBox.Show("Тип shape файла успешно изменен на: " + tn, "Shape файл", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }        

        private void getThreadsInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TileRendering.TileRenderingErrors.PauseNormal =! TileRendering.TileRenderingErrors.PauseNormal;
        }

        private void openMapExt_Click(object sender, EventArgs e)
        {
            if ((iMapFileName.Text == "") || (!System.IO.File.Exists(iMapFileName.Text))) { MessageBox.Show("Не настроены параметры карты!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information); return; };

            //градусы
            double sx; double sy; double ex; double ey;
            double.TryParse(isX.Text, System.Globalization.NumberStyles.Float, ni, out sx);
            double.TryParse(isY.Text, System.Globalization.NumberStyles.Float, ni, out sy);
            double.TryParse(ieX.Text, System.Globalization.NumberStyles.Float, ni, out ex);
            double.TryParse(ieY.Text, System.Globalization.NumberStyles.Float, ni, out ey);

            //метры проекции
            if (iBoxFormat.SelectedIndex == 1)
            {
                sx = TileRendering.TilesProjection.x2lon_m(sx);
                sy = TileRendering.TilesProjection.y2lat_m(sy);
                ex = TileRendering.TilesProjection.x2lon_m(ex);
                ey = TileRendering.TilesProjection.y2lat_m(ey);
            };
            //номера тайлов
            if (iBoxFormat.SelectedIndex == 2)
            {
                double tmp_lat; double tmp_lon;
                TileRendering.TilesProjection.fromTileToLL((int)sx, (int)sy, iBoxZoom.SelectedIndex + 1, out tmp_lat, out tmp_lon);
                sx = tmp_lon; sy = tmp_lat;
                TileRendering.TilesProjection.fromTileToLL((int)ex, (int)ey, iBoxZoom.SelectedIndex + 1, out tmp_lat, out tmp_lon);
                ex = tmp_lon; ey = tmp_lat;
            };
            //координаты в пикселях
            if (iBoxFormat.SelectedIndex == 3)
            {
                double[] ll = TileRendering.TilesProjection.fromPixelToLL(new double[] { sy, sx }, iBoxZoom.SelectedIndex + 1);
                sx = ll[0]; sy = ll[1];
                ll = TileRendering.TilesProjection.fromPixelToLL(new double[] { ey, ex }, iBoxZoom.SelectedIndex + 1);
                ex = ll[0]; ey = ll[1];
            };

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><app_texts_data><center><string>{0}</string><string>{1}</string></center><map>{2}</map><zoom>{3}</zoom></app_texts_data>";
            xml = String.Format(xml, new object[] { ((sx + ex) / 2).ToString().Replace(",", "."), ((sy + ey) / 2).ToString().Replace(",", "."), iMapFileName.Text, 10 });

            FileStream fs = new FileStream(TileRendering.TileRenderer.GetCurrentDir() + @"\Viewer.cfg", FileMode.Create, FileAccess.Write);
            byte[] ba = System.Text.Encoding.GetEncoding(1251).GetBytes(xml);
            fs.Write(ba, 0, ba.Length);
            fs.Close();

            System.Diagnostics.Process.Start(TileRendering.TileRenderer.GetCurrentDir() + @"\MapnikSimpleMapViewer.exe", "");
        }

        private void mergedTilesSimpleViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(iTilesDir.Text + @"\MergedTilesSimpleViewer.exe", "");
            }
            catch { };
        }

        private void indexShapeClick(object sender, EventArgs e)
        {
            if (iMapFileName.Text == "") return;
            if (!System.IO.File.Exists(iMapFileName.Text))
            {
                MessageBox.Show("Файл карты не найден!");
                return;
            };

            string cd = TileRendering.TileRenderer.GetCurrentDir();
            System.IO.FileStream fs = new FileStream(cd + @"\shapeindexer\shapeindex.cmd", FileMode.Create, FileAccess.Write);

            System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
            try
            {                
                xd.Load(iMapFileName.Text);
                XmlNodeList nl = xd.SelectNodes("Map/Layer/Datasource/Parameter[@name='type']");
                foreach (XmlNode xn in nl)
                {
                    string tp = xn.ChildNodes[0].Value;
                    if (tp.ToLower() != "shape") continue;
                    XmlNode fn = xn.ParentNode.SelectSingleNode("Parameter[@name='file']");
                    string gn = fn.ChildNodes[0].Value;
                    byte[] ba = System.Text.Encoding.GetEncoding(1251).GetBytes("shapeindex.exe \"" + gn + "\"\r\n");
                    fs.Write(ba, 0, ba.Length);
                };
                //byte[] ps = System.Text.Encoding.GetEncoding(1251).GetBytes("pause\r\n");
                //fs.Write(ps, 0, ps.Length);
                fs.Close();
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(cd + @"\shapeindexer\shapeindex.cmd");
                psi.WorkingDirectory = cd + @"\shapeindexer\";
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(psi);
                p.WaitForExit();
                MessageBox.Show("Индексация завершена", "Индексация shape файлов", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                fs.Close();
                MessageBox.Show(ex.ToString(), "Ошибка разбора XML файла карты", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            };            
            xd = null;
        }

        private void OpenImDiskCPL(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("imdisk.cpl");
        }

        private void vdCreate_Click(object sender, EventArgs e)
        {
            if (!File.Exists(iMapFileName.Text)) return;

            uint dev = 0;
            string[] files;
            long shapeFileSize = GetShapeFileSize(out files)/1024/1024 + 1;
            long size = shapeFileSize + 10 < 50 ? 50 : shapeFileSize + 10;
            if (InputNumericBox("Создание виртуального диска в памяти", "Укажите размер виртуального диска в мегабайтах:", "Размер Shape файлов: " + shapeFileSize.ToString() + " МБ", shapeFileSize + 5, 2147483648, ref size) == DialogResult.Cancel)
                return;

            //LTR.IO.ImDisk.ImDiskAPI.CreateDevice(size * 1024 * 1024, null, LTR.IO.ImDisk.ImDiskAPI.MemoryType.PhysicalMemory, "V:", ref dev);
            string cmd = String.Format("-a -s {0}M -o awe -m V: -p \"/fs:fat32 /q /y\"", size);
            System.Diagnostics.Process.Start("imdisk", cmd).WaitForExit();
            try
            {
                // Check exists
                LTR.IO.ImDisk.ImDiskDevice d = new LTR.IO.ImDisk.ImDiskDevice("V:", FileAccess.Read);
                d.Close();
                
                if (MessageBox.Show("Скопировать Shape файлы и файлы проекта на виртуальный диск?", "Создание виртуального диска в памяти", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

                // copy files
                CopyFiles.CopyFiles FileCopier = new CopyFiles.CopyFiles(new List<string>(files), @"V:\");
                CopyFiles.DIA_CopyFiles Dialog = new CopyFiles.DIA_CopyFiles(this);
                Dialog.SynchronizationObject = this;
                FileCopier.CopyAsync(Dialog);
                while (FileCopier.IsRunning)
                {
                    Application.DoEvents();
                    Dialog.Focus();
                };
                Dialog.Dispose();
                UpdateShapeFilesPathInXml(@"V:\" + Path.GetFileName(iMapFileName.Text), @"V:\");

                if (MessageBox.Show("Уставновить файл описания карты в проекте, файл полигона и файл водяного знака на файлы с виртуального диска?", "Создание виртуального диска в памяти", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
                
                // update cfg
                iMapFileName.Text = @"V:\" + Path.GetFileName(iMapFileName.Text);
                if (File.Exists(irPolygon.Text)) irPolygon.Text = @"V:\_render_in_shape.shp";
                if (File.Exists(iaddCopyrightsConfig.Text)) iaddCopyrightsConfig.Text = @"V:\_render_copyright.png";

                if (MessageBox.Show("Сохранить файл проекта на виртуальный диск?", "Создание виртуального диска в памяти", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

                // save to disk
                SaveProject(@"V:\_render_project.mtr_project");
            }
            catch{};
            
        }

        public long GetShapeFileSize(out string[] files)
        {
            files = new string[0];
            if (!File.Exists(iMapFileName.Text)) return 0;
            List<string> flist = new List<string>();

            long res = 0;

            if (File.Exists(iMapFileName.Text))
            {
                res += (new FileInfo(iMapFileName.Text)).Length;
                flist.Add(iMapFileName.Text);
            };

            if (File.Exists(irPolygon.Text))
            {
                res += (new FileInfo(irPolygon.Text)).Length;
                flist.Add(irPolygon.Text+"?_render_in_shape.shp");
            };

            if (File.Exists(iaddCopyrightsConfig.Text))
            {
                res += (new FileInfo(iaddCopyrightsConfig.Text)).Length;
                flist.Add(iaddCopyrightsConfig.Text+"?_render_copyright.png");
            };

            System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
            try
            {
                xd.Load(iMapFileName.Text);
                XmlNodeList nl = xd.SelectNodes("Map/Layer/Datasource/Parameter[@name='type']");
                foreach (XmlNode xn in nl)
                {
                    string tp = xn.ChildNodes[0].Value;
                    if (tp.ToLower() != "shape") continue;
                    XmlNode fn = xn.ParentNode.SelectSingleNode("Parameter[@name='file']");
                    string gn = fn.ChildNodes[0].Value;

                    string[] filesIn = Directory.GetFiles(Path.GetDirectoryName(gn),Path.GetFileNameWithoutExtension(gn)+".*");
                    foreach (string file in filesIn)
                    {
                        FileInfo fi = new FileInfo(file);
                        res += fi.Length;
                        flist.Add(file);
                    };
                };
            }
            catch (Exception ex)
            {
            };
            files = flist.ToArray();
            return res;
        }

        public void UpdateShapeFilesPathInXml(string xmlfile, string newPath)
        {
            System.Xml.XmlDocument xd = new System.Xml.XmlDocument();
            try
            {
                xd.Load(xmlfile);
                XmlNodeList nl = xd.SelectNodes("Map/Layer/Datasource/Parameter[@name='type']");
                foreach (XmlNode xn in nl)
                {
                    string tp = xn.ChildNodes[0].Value;
                    if (tp.ToLower() != "shape") continue;
                    XmlNode fn = xn.ParentNode.SelectSingleNode("Parameter[@name='file']");
                    string gn = fn.ChildNodes[0].Value;
                    gn = newPath + Path.GetFileName(gn);
                    fn.ChildNodes[0].Value = gn;
                };
                xd.Save(xmlfile);
            }
            catch (Exception ex)
            {
            };
        }

        public static DialogResult InputNumericBox(string title, string promptText, string commentText, long minvalue, long maxvalue, ref long value)
        {
            Form form = new Form();
            Label label = new Label();
            Label label2 = new Label();
            NumericUpDown sizeBox = new NumericUpDown();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            label2.Text = commentText;            
            sizeBox.Minimum = minvalue;
            sizeBox.Maximum = maxvalue;
            sizeBox.Value = value;
            sizeBox.TextAlign = HorizontalAlignment.Right;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Отмена";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(10, 12, 372, 13);
            label2.SetBounds(10, 54, 372, 13);
            sizeBox.SetBounds(12, 30, 372, 20);
            buttonOk.SetBounds(228, 76, 75, 23);
            buttonCancel.SetBounds(309, 76, 75, 23);

            label.AutoSize = true;
            label2.AutoSize = true;
            sizeBox.Anchor = sizeBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, sizeBox, label2, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = (long)sizeBox.Value;
            return dialogResult;
        }

        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            return InputBox(title, promptText, 300, ref value);
        }

        public static DialogResult InputBox(string title, string promptText, int maxWidth, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Отмена";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(10, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(maxWidth, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

       
        private void vdResize_Click(object sender, EventArgs e)
        {
            List<int> devices = LTR.IO.ImDisk.ImDiskAPI.GetDeviceList();
            for(int i=0;i<devices.Count;i++)
            {
                LTR.IO.ImDisk.DLL.ImDiskCreateData dcd = LTR.IO.ImDisk.ImDiskAPI.QueryDevice((uint)devices[i]);
                if(dcd.DriveLetter != 'V') continue;
                //LTR.IO.ImDisk.ImDiskAPI.ExtendDevice(

                long oldsize = 0;
                long size = oldsize = dcd.DiskSize / 1024 / 1024;
                if (InputNumericBox("Изменение виртуального диска в памяти", "Укажите размер виртуального диска в мегабайтах:", "Текущий размер диска: " + size.ToString() + " МБ", size, 2147483648, ref size) == DialogResult.Cancel)
                    return;

                long newDiskSize = size - oldsize;
                if (newDiskSize > 0)
                {
                    LTR.IO.ImDisk.ImDiskAPI.ExtendDevice((uint)dcd.DeviceNumber, newDiskSize * 1024 * 1024);
                    MessageBox.Show("Размер виртуального диска успешно изменен!", "Изменение виртуального диска в памяти", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                return;
            };                                 
        }

        private void vdDelete_Click(object sender, EventArgs e)
        {
            LTR.IO.ImDisk.ImDiskAPI.RemoveDevice(@"V:");
            MessageBox.Show("Вирутальный диск V: успешно удален!", "Удаление виртуального диска", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void vdOpen_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"V:\");
        }

        private void vdSave_Click(object sender, EventArgs e)
        {            
            string vdname = "";
            SaveFileDialog sfd = new SaveFileDialog();
            if (_project_filename != null) sfd.FileName = _project_filename + ".img";
            sfd.DefaultExt = ".img";
            sfd.Filter = "Virtual Disk Image File (*.img)|*.img";
            if (sfd.ShowDialog() == DialogResult.OK)
                vdname = sfd.FileName;
            sfd.Dispose();
            if(vdname == "") return;

            if (_project_filename.IndexOf("V:") == 0)
            {
                DialogResult dr = MessageBox.Show("Сохранить проект на виртуальном диске перед сохранением самого диска?", "Сохранение виртуального диска", MessageBoxButtons.YesNoCancel);
                if (dr == DialogResult.Cancel) return;
                if (dr == DialogResult.Yes) SaveProject(_project_filename);
            };

            LTR.IO.ImDisk.ImDiskDevice d = new LTR.IO.ImDisk.ImDiskDevice("V:", FileAccess.Read);
            d.SaveImageFile(vdname);
            d.Close();

            MessageBox.Show("Содержимое виртуального диска V: сохранено в файл:\r\n"+vdname+"\r\nРекомендуется сжать файл диска для уменьшения занимаемого им места!", "Сохранение виртуального диска", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void vdOpenCreate_Click(object sender, EventArgs e)
        {
            string vdname = "";
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".img";
            ofd.Filter = "Virtual Disk Image File (*.img)|*.img";
            if (ofd.ShowDialog() == DialogResult.OK)
                vdname = ofd.FileName;
            ofd.Dispose();
            if (vdname == "") return;

            FileInfo fi = new FileInfo(vdname);
            uint dev = 0;
            LTR.IO.ImDisk.ImDiskAPI.CreateDevice(fi.Length,vdname, LTR.IO.ImDisk.ImDiskAPI.MemoryType.PhysicalMemory, "V:", ref dev);

            string fn = @"V:\_render_project.mtr_project";
            if (File.Exists(fn))
            {
                if (MessageBox.Show("Виртуальный диск V: успешно создан!\r\nНа виртуальном диске обнаружен сохраненный проект нарезки!\r\nЗагрузить проект нарезки с виртуального диска?", "Создание виртуального диска в памяти", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    WriteProjectToForm(fn);
            }
            else
                MessageBox.Show("Виртуальный диск V: успешно создан!", "Создание виртуального диска в памяти", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void vdCreateOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            uint dev = 0;
            string[] files;
            long shapeFileSize = GetShapeFileSize(out files)/1024/1024 + 1;
            long size = shapeFileSize + 10 < 50 ? 50 : shapeFileSize + 10;
            if (InputNumericBox("Создание виртуального диска в памяти", "Укажите размер виртуального диска в мегабайтах:", "Размер Shape файлов: " + shapeFileSize.ToString() + " МБ", shapeFileSize + 5, 2147483648, ref size) == DialogResult.Cancel)
                return;

            //LTR.IO.ImDisk.ImDiskAPI.CreateDevice(size * 1024 * 1024, null, LTR.IO.ImDisk.ImDiskAPI.MemoryType.PhysicalMemory, "V:", ref dev);
            string cmd = String.Format("-a -s {0}M -o awe -m V: -p \"/fs:fat32 /q /y\"", size);
            System.Diagnostics.Process.Start("imdisk", cmd).WaitForExit();
            try
            {
                // Check exists
                LTR.IO.ImDisk.ImDiskDevice d = new LTR.IO.ImDisk.ImDiskDevice("V:", FileAccess.Read);
                d.Close();

                MessageBox.Show("Виртуальный диск V: успешно создан", "Создание виртуального диска в памяти", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Не удалось создать виртуальный диск V!", "Создание виртуального диска в памяти", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
        }

        private void vdCopy_Click(object sender, EventArgs e)
        {
            if (!File.Exists(iMapFileName.Text)) return;

            long diskSize = 0;
            try
            {
                LTR.IO.ImDisk.ImDiskDevice d = new LTR.IO.ImDisk.ImDiskDevice("V:", FileAccess.Read);
                diskSize = d.DiskSize;
                d.Close();
            }
            catch
            {
                MessageBox.Show("Виртуальный диск V: не найден!","Копирование проекта на виртуальный диск",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            };

            try
            {
                uint dev = 0;
                string[] files;
                long shapeFileSize = GetShapeFileSize(out files) + 1024 * 1024;

                if (diskSize < shapeFileSize)
                {
                    shapeFileSize = shapeFileSize / 1024 / 1024;
                    MessageBox.Show("Размер виртуального диска недостаточен для размещения на нем файлов проекта!\r\nРазмер диска должен быть не менее " + shapeFileSize.ToString()+ "МБ", "Копирование проекта на виртуальный диск", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                };

                if (MessageBox.Show("Скопировать Shape файлы и файлы проекта на виртуальный диск?", "Создание виртуального диска в памяти", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

                // copy files
                CopyFiles.CopyFiles FileCopier = new CopyFiles.CopyFiles(new List<string>(files), @"V:\");
                CopyFiles.DIA_CopyFiles Dialog = new CopyFiles.DIA_CopyFiles(this);
                Dialog.SynchronizationObject = this;
                FileCopier.CopyAsync(Dialog);
                while (FileCopier.IsRunning)
                {
                    Application.DoEvents();
                    Dialog.Focus();
                };
                Dialog.Dispose();
                UpdateShapeFilesPathInXml(@"V:\" + Path.GetFileName(iMapFileName.Text), @"V:\");

                if (MessageBox.Show("Уставновить файл описания карты в проекте, файл полигона и файл водяного знака на файлы с виртуального диска?", "Создание виртуального диска в памяти", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

                // update cfg
                iMapFileName.Text = @"V:\" + Path.GetFileName(iMapFileName.Text);
                if (File.Exists(irPolygon.Text)) irPolygon.Text = @"V:\_render_in_shape.shp";
                if (File.Exists(iaddCopyrightsConfig.Text)) iaddCopyrightsConfig.Text = @"V:\_render_copyright.png";

                if (MessageBox.Show("Сохранить файл проекта на виртуальный диск?", "Создание виртуального диска в памяти", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

                // save to disk
                SaveProject(@"V:\_render_project.mtr_project");
            }
            catch { };
        }

        private void softPerfectRAMDiskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // C:\Program Files\SoftPerfect RAM Disk\ramdiskws.exe            
            string prog = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles) + @"\SoftPerfect RAM Disk\ramdiskws.exe";
            System.Diagnostics.Process.Start(prog);
        }

        private void vdProxy_Click(object sender, EventArgs e)
        {
            string sp = "127.0.0.1:9000";
            if (InputBox("Подключение виртуального диска", "Укажите удаленный сервер и порт:", ref sp) == DialogResult.Cancel) return;

            string cmd = String.Format("-a -t proxy -o ip -f {0} -m V:",sp);
            System.Diagnostics.Process.Start("imdisk", cmd).WaitForExit();

            try
            {
                LTR.IO.ImDisk.ImDiskDevice d = new LTR.IO.ImDisk.ImDiskDevice("V:", FileAccess.Read);
                d.Close();
                MessageBox.Show("Виртуальный диск V: подключен!", "Подключение виртуального диска", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подключить виртуальный диск V:!\r\n"+ex.Message, "Подключение виртуального диска", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };
        }

        private void serverRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_project_filename == "")
            {
                MessageBox.Show("Необходимо cперва сохранить файл!");
                return;
            };

            CreateCMD ccmd = new CreateCMD(_project_filename, true);
            ccmd.ShowDialog();
            ccmd.Dispose();            
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            threadState.Text = "";
            if ((_runtime_stopped[0] <= DateTime.MinValue) && (tabControl2.SelectedIndex == 1))
            {
                threadState.Text = "Подождите, идет получение информации...";
            };
        }

        private void преобразоватьКоординатыXYLatLonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TransfXYLL tf = new TransfXYLL();
            tf.ShowDialog();
            tf.Dispose();
        }

        private void nZip_Click(object sender, EventArgs e)
        {            
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images Types: (*.png;*.gif;*.jpg)|*.png;*.gif;*.jpg;*.jpeg|All types (*.*)|*.*";
            if (ofd.ShowDialog() != DialogResult.OK) return;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".png";
            sfd.FileName = ofd.FileName;
            ofd.Filter = "Portable Network Graphics (*.png)|*.png|All types (*.*)|*.*";
            if (sfd.ShowDialog() != DialogResult.OK) return;
            TileRendering.TileOptimizeMethod tom = (TileRendering.TileOptimizeMethod)(iTileOptimizeMethod.SelectedIndex + 1);
            byte tileOptimizeFormat = (byte)(iPNGFormat.SelectedIndex);
            OptiSave(ofd.FileName, sfd.FileName, tom, tileOptimizeFormat);
            sfd.Dispose();
            ofd.Dispose();
        }

        /// <summary>
        ///     Оптимизируем и сохраняем файл
        /// </summary>
        /// <param name="img"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool OptiSave(string inFile, string outFile, TileRendering.TileOptimizeMethod tileOptimizeMethod, byte tileOptimizeFormat)
        {
            System.IO.MemoryStream STD = new MemoryStream();
            STD.Position = 0;

            try
            {
                Image src = Image.FromFile(inFile);
                Image img = (Image)src.Clone();
                src.Dispose();
                img.Save(STD, System.Drawing.Imaging.ImageFormat.Png);
                long fileSize = STD.Length;

                // fastest = 0x02, // используется FreeImage.DLL/быстрое и Standard, сохраняется файл с меньшим размером	
                if (tileOptimizeMethod == TileRendering.TileOptimizeMethod.fastest)
                {

                    FreeImageAPI.FIBITMAP dib = FreeImageAPI.FreeImage.LoadFromStream(STD);
                    FreeImageAPI.FreeImage.SaveEx(
                        ref dib,
                        outFile, FreeImageAPI.FREE_IMAGE_FORMAT.FIF_PNG,
                        FreeImageAPI.FREE_IMAGE_SAVE_FLAGS.PNG_Z_DEFAULT_COMPRESSION,
                        FreeImageAPI.FREE_IMAGE_COLOR_DEPTH.FICD_AUTO,
                        true);
                    dib.SetNull();

                    if ((fileSize = (new FileInfo(outFile)).Length) > STD.Length)
                        SaveStreamToFile(STD, outFile);

                    STD.Close();
                    return true;
                };

                // fastmax = 0x03, // используется FreeImage.DLL/максимальное и Standard, сохраняется файл с меньшим размером
                if (tileOptimizeMethod == TileRendering.TileOptimizeMethod.fastmax)
                {
                    FreeImageAPI.FIBITMAP dib = FreeImageAPI.FreeImage.LoadFromStream(STD);
                    FreeImageAPI.FreeImage.SaveEx(
                        ref dib,
                        outFile, FreeImageAPI.FREE_IMAGE_FORMAT.FIF_PNG,
                        FreeImageAPI.FREE_IMAGE_SAVE_FLAGS.PNG_Z_BEST_COMPRESSION,
                        FreeImageAPI.FREE_IMAGE_COLOR_DEPTH.FICD_AUTO,
                        true);
                    dib.SetNull();

                    if ((fileSize = (new FileInfo(outFile)).Length) > STD.Length)
                        SaveStreamToFile(STD, outFile);

                    STD.Close();
                    return true;
                };

                // fastMagic = 0x04, //используется imageMagic, FreeImage.DLL/максимальное и Standard, сохраняется файл с меньшим размером
                if (tileOptimizeMethod == TileRendering.TileOptimizeMethod.fastMagic)
                {
                    ImageMagick.MagickImage mi = new ImageMagick.MagickImage(STD.ToArray());
                    mi.Format = ImageMagick.MagickFormat.Png + tileOptimizeFormat;// sfsf 
                    System.IO.MemoryStream ms = new MemoryStream();
                    mi.Write(ms);

                    FreeImageAPI.FIBITMAP dib = FreeImageAPI.FreeImage.LoadFromStream(STD);
                    FreeImageAPI.FreeImage.SaveEx(
                        ref dib,
                        outFile, FreeImageAPI.FREE_IMAGE_FORMAT.FIF_PNG,
                        FreeImageAPI.FREE_IMAGE_SAVE_FLAGS.PNG_Z_BEST_COMPRESSION,
                        FreeImageAPI.FREE_IMAGE_COLOR_DEPTH.FICD_AUTO,
                        true);
                    dib.SetNull();

                    long[] sa = new long[] { STD.Length, ms.Length, (new FileInfo(outFile)).Length };
                    Array.Sort(sa);

                    if (sa[0] == STD.Length) SaveStreamToFile(STD, outFile);
                    if (sa[0] == ms.Length) SaveStreamToFile(ms, outFile);
                    ms.Close();
                    mi = null;

                    STD.Close();
                    return true;
                };

                // imageMagic = 0x05, // используется imageMagic и Standard, сохраняется файл с меньшим размером
                if (tileOptimizeMethod == TileRendering.TileOptimizeMethod.imageMagic)
                {
                    ImageMagick.MagickImage mi = new ImageMagick.MagickImage(STD.ToArray());
                    mi.Format = ImageMagick.MagickFormat.Png + tileOptimizeFormat;
                    System.IO.MemoryStream ms = new MemoryStream();
                    mi.Write(ms);
                    if (ms.Length > STD.Length)
                        SaveStreamToFile(STD, outFile);
                    else
                    {
                        SaveStreamToFile(ms, outFile);
                        fileSize = ms.Length;
                    };
                    ms.Close();
                    mi = null;

                    STD.Close();
                    return true;
                };

                // optimize = 0x06, // используется optimize.dll и Standard, сохраняется файл с меньшим размером
                if (tileOptimizeMethod == TileRendering.TileOptimizeMethod.optimize)
                {
                    SaveStreamToFile(STD, outFile);
                    Optimize_OptimizeDLL("-file:\"" + outFile + "\" -KeepInterlacing -KeepBackgroundColor -AvoidGreyWithSimpleTransparency");
                    if ((fileSize = (new FileInfo(outFile)).Length) > STD.Length)
                        SaveStreamToFile(STD, outFile);
                    return true;
                };

                // maximim = 0x07 // используется imageMagic, optimize.dll и Standard, сохраняется файл с меньшим размером
                if (tileOptimizeMethod == TileRendering.TileOptimizeMethod.maximim)
                {
                    ImageMagick.MagickImage mi = new ImageMagick.MagickImage(STD.ToArray());
                    mi.Format = ImageMagick.MagickFormat.Png + tileOptimizeFormat;
                    System.IO.MemoryStream ms = new MemoryStream();
                    mi.Write(ms);

                    SaveStreamToFile(STD, outFile);
                    Optimize_OptimizeDLL("-file:\"" + outFile + "\" -KeepInterlacing -KeepBackgroundColor -AvoidGreyWithSimpleTransparency");

                    long[] sa = new long[] { STD.Length, ms.Length, (new FileInfo(outFile)).Length };
                    Array.Sort(sa);

                    if (sa[0] == STD.Length) SaveStreamToFile(STD, outFile);
                    if (sa[0] == ms.Length) SaveStreamToFile(ms, outFile);
                    ms.Close();
                    mi = null;

                    return true;
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Tile Optimization: " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            STD.Close();
            return false;
        }

        [DllImport("optimize.dll", SetLastError = true, EntryPoint = "optimizefiles")]
        private static extern int Optimize_OptimizeDLL(string fileName);

        private static void SaveStreamToFile(Stream ms, string fileName)
        {
            ms.Position = 0;
            byte[] buff = new byte[1024];
            System.IO.FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            int read = 0;
            while ((read = ms.Read(buff, 0, buff.Length)) > 0)
                fs.Write(buff, 0, read);
            fs.Close();
        }

        private void runFHoleBtn_Click(object sender, EventArgs e)
        {
            if ((iMapFileName.Text == "")
                            || (!System.IO.File.Exists(iMapFileName.Text.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries)[0]))
                            || (iTilesDir.Text.Trim().Length == 0)) { MessageBox.Show("Не настроены параметры карты!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information); return; };

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "*.holes";
            ofd.Filter = "Holes & Error files (*.holes;*.errorlog)|*.holes;*.errorlog|Holes files (*.holes)|*.holes|Error Log files (*.errorlog)|*.errorlog|All Types (*.*)|*.*";
            ofd.Title = "Выберите файл с дырами";

            string fn = "";
            if (ofd.ShowDialog() == DialogResult.OK)
                fn = ofd.FileName;
            ofd.Dispose();
            if (fn == "") return;
            if (!File.Exists(fn)) return;

            FileInfo fi = new FileInfo(fn);
            if(MessageBox.Show(String.Format("Файл с дырами: {1}\r\nРазмер файла {0}! Хотите ли вы просканировать файл?",TileRendering.TileRenderer.FileSizeSuffix(fi.Length),fn),"Анализ файла с дырами",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.No)
                return;

            TileRendering.FHoles fh;
            try
            {
                fh = new TileRendering.FHoles(fn, ((int)(iTileRenderZoneSize.SelectedIndex) + 1));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удается открыть файл дыр!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };

            //int[][] zn11 = fh.ZonesIn(11);

            if (fh.Count > 0)
            {
                if (MessageBox.Show(String.Format("Найдены дыры в {0} квадратах в {1} зумах!\r\nНачать нарезку квадратов с дырами?", fh.Count, fh.Zooms), "Нарезка дыр", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }
            else
            {
                MessageBox.Show("В выбранном файле не найдено информации о дырах!", "Нарезка дыр", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            };

            StartRendering(fh, null);
        }

        private void lsopnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cd = TileRendering.TileRenderer.GetCurrentDir();
            string fn = cd + @"\LOGS\main_[" + System.IO.Path.GetFileName(_project_filename) + "].log";
            if (File.Exists(fn))
            {
                string akelpad = cd + @"\plugins\AkelPad.exe";
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = akelpad;
                psi.Arguments = fn;
                System.Diagnostics.Process.Start(psi);
            };
        }

        private void leopnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cd = TileRendering.TileRenderer.GetCurrentDir();
            string fn = cd + @"\LOGS\main_[" + System.IO.Path.GetFileName(_project_filename) + "].errorlog";
            if (File.Exists(fn))
            {
                string akelpad = cd + @"\plugins\AkelPad.exe";
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = akelpad;
                psi.Arguments = fn;
                System.Diagnostics.Process.Start(psi);
            };
        }

        private void lhopnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cd = TileRendering.TileRenderer.GetCurrentDir();
            string fn = cd + @"\LOGS\main_[" + System.IO.Path.GetFileName(_project_filename) + "].holes";
            if (File.Exists(fn))
            {
                string akelpad = cd + @"\plugins\AkelPad.exe";
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = akelpad;
                psi.Arguments = fn;
                System.Diagnostics.Process.Start(psi);
            };
        }

        private void lfopnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cd = TileRendering.TileRenderer.GetCurrentDir()+@"\LOGS\";
            System.Diagnostics.Process.Start(cd);
        }
        
        private void logggsToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            string cd = TileRendering.TileRenderer.GetCurrentDir();
            string fp = cd + @"\LOGS\main_[" + System.IO.Path.GetFileName(_project_filename) + "].";
            lsopnToolStripMenuItem.Enabled = File.Exists(fp + "log");
            leopnToolStripMenuItem.Enabled = File.Exists(fp + "errorlog");
            lhopnToolStripMenuItem.Enabled = File.Exists(fp + "holes");
        }

        private void visualizeBtn_Click(object sender, EventArgs e)
        {
            visualizeBtn.Checked = !visualizeBtn.Checked;
            iStartMtx.Visible = visualizeBtn.Checked;
            #if HID_01
            iStartMtx.Visible = false;
            #endif
            TileRendering.TileRenderingErrors.WithDump = visualizeBtn.Checked;
        }

        private void vslviewBtn_Click(object sender, EventArgs e)
        {
            VisualizeMap cm = new VisualizeMap(this);
            cm.renderers = _runtime_renderers;
            cm.ShowModal();
        }

        private void saveMatrixDumbBtm_Click(object sender, EventArgs e)
        {
            if ((_runtime_renderers != null) && (_runtime_renderers.Length > 0))
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = ".dump";
                sfd.FileName = System.IO.Path.GetFileNameWithoutExtension(_project_filename) + ".dump";
                sfd.Filter = "Dump files (*.dump)|*.dump";
                if (sfd.ShowDialog() != DialogResult.OK) return;
                string fn = sfd.FileName;
                sfd.Dispose();

                string ttlzs = "";

                FileStream fs = new FileStream(fn, FileMode.Create, FileAccess.Write);
                {
                    // HEADER 18 bytes [0..17]
                    fs.Write(TileRendering.RenderView.fileheader, 0, TileRendering.RenderView.fileheader.Length);
                    // EACH DUMP
                    for(int r =0;r<_runtime_renderers.Length;r++)
                        if ((_runtime_renderers[r] != null) && (_runtime_renderers[r].VIEW != null))
                        {
                            if (ttlzs.Length > 0) ttlzs += ", ";
                            ttlzs += r.ToString();

                            TileRendering.RenderView rv = _runtime_renderers[r].VIEW;                            
                            for (int i = 0; i < rv.Size; i++)
                                fs.WriteByte(rv.GetByte(i));
                        };                    
                };
                fs.Close();

                if (ttlzs.Length > 0)
                    MessageBox.Show(String.Format("Дамп успешно сохранен для {0} зума(ов)!\r\nФайл дампа может быть использован для переноски нарезки на другой компьютер.", ttlzs), "Сохранение дампа нарезки", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        private void iStartMtx_Click(object sender, EventArgs e)
        {
            #if HID_01
            return;
            #endif

            if ((iMapFileName.Text == "")
                || (!System.IO.File.Exists(iMapFileName.Text.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries)[0]))
                || (iTilesDir.Text.Trim().Length == 0)
                || (iZooms.CheckedItems.Count == 0)) { MessageBox.Show("Не настроены параметры карты!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information); return; };
            
            OpenFileDialog sfd = new OpenFileDialog();
            sfd.Title = "Выберите файл дампа нарезки";
            sfd.DefaultExt = ".dump";
            sfd.Filter = "Dump files (*.dump)|*.dump|All types (*.*)|*.*";
            if (sfd.ShowDialog() != DialogResult.OK) return;
            string fn = sfd.FileName;
            sfd.Dispose();


            string smlist = "";
            string smwill = "";
            FileStream fs = new FileStream(fn, FileMode.Open, FileAccess.Read);
            if (fs.Length > 18)
            {
                // HEADER 18 bytes [0..17]                        
                byte[] bheader = new byte[18];
                fs.Read(bheader, 0, bheader.Length);
                bool headerOk = true;
                for (int i = 0; i < bheader.Length; i++)
                    if (bheader[i] != TileRendering.RenderView.fileheader[i])
                        headerOk = false;
                if (headerOk)
                {
                    while (fs.Position < fs.Length)
                    {
                        // ZOOM NO byte, zoneSize byte, xmin int, xmax int, ymin int, ymax int, ZOOM data_size int, DATA byte[]
                        byte zm = (byte)fs.ReadByte();

                        if (zm < 23)
                        {
                            if (smlist.Length > 0) smlist += ", ";
                            smlist += zm.ToString();
                            if (iZooms.GetItemChecked(zm-1))
                            {
                                if (smwill.Length > 0) smwill += ", ";
                                smwill += zm.ToString();
                            };
                        }
                        else
                        {
                            fs.Close();
                            MessageBox.Show("Информация в файле повреждена!", "Сбой нарезки", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                            return;
                        };

                        fs.Position += 17;
                        byte[] zms = new byte[4];
                        fs.Read(zms, 0, zms.Length);
                        if (BitConverter.IsLittleEndian) Array.Reverse(zms);
                        int zlength = BitConverter.ToInt32(zms, 0);
                        fs.Position += zlength;
                    };

                };
            };
            fs.Close();

            if (smlist.Length == 0)
            {
                MessageBox.Show("В выбранном файле не найдено информации о нарезаемых зумах!", "Сбой нарезки", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                return;
            }
            else
            {
                if (smwill.Length > 0)
                {
                    if (MessageBox.Show("Вы должны быть уверены, что все настройки параметров карты совпадают с настройками при котором был сделан дамп! Особенно это касается границ нарезаемой области и размера зоны при отрисовке. Если границы нарезаемой области не будут совпадать, нарезка может быть выполнена с ошибками или вовсе прерваться!\r\n\r\n" +
                        "В выбранном файле обнаружена информация о " + smlist + " зуме(ах)!\r\n" +
                        "В нарезке будет использована "+(smwill == smlist ? "вся информация!" : "информация только о " + smwill + " зуме(ах)!")+"\r\n" +
                        "\r\n" +
                        "Вы действительно хотите продолжить и использовать информацию из файла дампа?", "Старт нарезки", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                        return;
                }
                else
                {
                    MessageBox.Show(
                        "В выбранном файле обнаружена информация о " + smlist + " зуме(ах)!\r\n" +
                        "К сожалению этой информции не достаточно для нарезки выбранных масштабных уровней!\r\n"+
                        "Для осуществления нарезки необходимо выбрать уровни, информация хотя бы об одном из которых есть в файле дампа!\r\n"
                        , "Старт нарезки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                };
            };

            StartRendering(null, fn);
        }

        private void makeCMDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_project_filename == "")
            {
                MessageBox.Show("Необходимо cперва сохранить файл!");
                return;
            };

            CreateCMD ccmd = new CreateCMD(_project_filename, false);
            ccmd.ShowDialog();
            ccmd.Dispose();
        }

        private void aboutBth_Click(object sender, EventArgs e)
        {
            string txt = "Mapnik Tile Renderer\r\n";
            txt += "Mapnic Web Spherical Mercator Tiles Creator\r\n";            
            txt += "Текущая версия: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\r\n";
            txt += "\r\n";
            txt += "Программа предназначена для нарезки тайлов на основе заранее подготовленной карты,\r\n";
            txt += "которая описана в виде XML файла легенды в формате Mapnic\r\n\r\n";
            txt += "Описание файлов и папок можно найти в файле - `About.txt`\r\n";
            txt += "Описание работы с консольной версией в файле - `mtrc.ReadMe.txt`\r\n";
            txt += "Описание форматов сжатия картинок в файле - `Описание форматов сжатия.txt`\r\n";
            txt += "Описание проекции используемой для нарезки - `PROJECTIONS.txt`\r\n";
            MessageBox.Show(txt,"О программе",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private void substvAddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmd = @"SUBST M: \\192.168.0.22\Shared_mxd2\NMS\MultiMapnik";
            DialogResult dr = InputBox("Создание (подключение) виртуального диска:", "Параметры командной строки (можно указать как локальный, так и сетевой ресурс:", 500, ref cmd);
            if (dr != DialogResult.OK) return;
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/K " + cmd);
            psi.UseShellExecute = true;
            System.Diagnostics.Process.Start(psi);
        }

        private void substvRemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmd = @"SUBST M: /D";
            DialogResult dr = InputBox("Удаление (отключение) виртуального диска:", "Параметры командной строки:", 500, ref cmd);
            if (dr != DialogResult.OK) return;
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/K " + cmd);
            psi.UseShellExecute = true;
            System.Diagnostics.Process.Start(psi);
        }

        private void ndAddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmd = @"net use R: \\192.168.0.22\data_hp\arcgisserver\arcgiscache";
            DialogResult dr = InputBox("Подключение сетевого диска:", "Параметры командной строки:", 500, ref cmd);
            if (dr != DialogResult.OK) return;
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/K " + cmd);
            psi.UseShellExecute = true;
            System.Diagnostics.Process.Start(psi);
        }

        private void ndDelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmd = @"net use R: /DELETE";
            DialogResult dr = InputBox("Отключение сетевого диска:", "Параметры командной строки:", 500, ref cmd);
            if (dr != DialogResult.OK) return;
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/K " + cmd);
            psi.UseShellExecute = true;
            System.Diagnostics.Process.Start(psi);
        }

        private void nfShareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmd = @"net share Mapnik=C:\Mapnik";
            DialogResult dr = InputBox("Открыть доступ к сетевой папке:", "Параметры командной строки:", 500, ref cmd);
            if (dr != DialogResult.OK) return;
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/K " + cmd);
            psi.UseShellExecute = true;
            System.Diagnostics.Process.Start(psi);
        }

        private void openAkelPadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cd = TileRendering.TileRenderer.GetCurrentDir();
            string akelpad = cd + @"\plugins\AkelPad.exe";
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = akelpad;
            System.Diagnostics.Process.Start(psi);
        }

        private void iPConfigallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/K \"ipconfig /all\"");
            psi.UseShellExecute = true;
            System.Diagnostics.Process.Start(psi);
        }

        private void reg2regToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Write2Reg();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            KMZRebuilder.PolyCreator pc = new KMZRebuilder.PolyCreator();
            pc.ShowDialog();
            pc.Dispose();
        }

        private void tvOpen_Click(object sender, EventArgs e)
        {
            try
            {
                string cd = TileRendering.TileRenderer.GetCurrentDir();
                System.Diagnostics.Process.Start(cd + @"\TileViewer\TileViewer.exe", "");
            }
            catch { };
        }
    }

    [Serializable]
    public class MapnikTileRendererProject
    {
        public string MapFile;
        public string TilesPath;
        public TileRendering.TileRendererConfig RendererConfig;
        public double[] Zone = new double[6];
        public bool[] zooms = new bool[22];

        /// <summary>
        ///     Загружаем из файла
        /// </summary>
        /// <param name="file">файл</param>
        /// <returns></returns>
        public static MapnikTileRendererProject FromFile(string fileName)
        {
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(MapnikTileRendererProject));
            System.IO.StreamReader reader = System.IO.File.OpenText(fileName);
            MapnikTileRendererProject c = (MapnikTileRendererProject)xs.Deserialize(reader);
            reader.Close();
            return c;
        }

        /// <summary>
        ///     Сохранить в файл
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(MapnikTileRendererProject));
            System.IO.StreamWriter writer = System.IO.File.CreateText(fileName);
            xs.Serialize(writer, this);
            writer.Flush();
            writer.Close();
        }
    }

    /// <summary>
    /// Stores a set of items with the most recently added at item the front and the 
    /// least recently added item at the end
    /// </summary>
    public class RecentSet<T> : IEnumerable<T>
    {
        private List<T> _list;
        private int _size = 9;

        /// <summary>
        /// Creates a new RecentSet object.
        /// </summary>
        public RecentSet()
        {
            _list = new List<T>();
        }
        /// <summary>
        /// Creates a new RecentSet object with a fixed size. The return set may be smaller than
        /// the specified size but it will never be larger
        /// </summary>
        /// <param name="size">The maximum size of the set</param>
        public RecentSet(int size)
        {
            _list = new List<T>();
            _size = size;
        }

        /// <summary>
        /// Creates a new RecentSet object initializing it with the indicated items. Note: 
        /// the initialized RecentSet will be in the order of parameter items.  If items are {1, 2, 3, 4},
        /// iterating through RecentSet will result in a list of {1, 2, 3, 4} not {4, 3, 2, 1}        
        /// </summary>
        public RecentSet(List<T> items)
        {
            _list = items;
        }

        /// <summary>
        /// Creates a new RecentSet object with a fixed size initializing it with the indicated items. Note: 
        /// the initialized RecentSet will be in the order of parameter items.  If items are {1, 2, 3, 4},
        /// iterating through RecentSet will result in a list of {1, 2, 3, 4} not {4, 3, 2, 1}        
        /// </summary>
        public RecentSet(int size, List<T> items)
        {
            _list = items;
            _size = size;

            TrimList();
        }

        /// <summary>
        /// Adds an item to the RecentSet
        /// </summary>
        public void Add(T item)
        {
            // If the item is already in the set, remove it
            int i = _list.IndexOf(item);
            if (i > -1)
                _list.RemoveAt(i);

            // Add the item to the front of the list.
            _list.Insert(0, item);

            TrimList();
        }

        public void Delete(T item)
        {
            // If the item is already in the set, remove it
            int i = _list.IndexOf(item);
            if (i > -1)
                _list.RemoveAt(i);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        private void TrimList()
        {
            // If there is a set size, make sure the set only contains that many elements
            if (_size != -1)
                while (_list.Count > _size)
                    _list.RemoveAt(_list.Count - 1);
        }

        /// <summary>
        /// Returns the set in the form of a List
        /// </summary>
        public List<T> ToList()
        {
            return _list;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion
    }
}