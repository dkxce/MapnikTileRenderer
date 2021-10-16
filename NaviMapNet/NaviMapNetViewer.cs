using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace NaviMapNet
{
    public partial class NaviMapNetViewer : UserControl
    {
        private MouseEventHandler mv;
        private MouseEventHandler md;
        private MouseEventHandler mu;
        ~NaviMapNetViewer()
        {
            MouseMessageFilter.MouseMove -= mv;
            MouseMessageFilter.MouseDown -= md;
            MouseMessageFilter.MouseUp -= mu;
            Dispose();            
        }

        #region override
        public string Author { get { return "milokz@gmail.com"; } }
        public override bool AutoScroll { get { return false; } }
        public override Color ForeColor { get { return Color.Transparent; } }
        public override Color BackColor { get { return imagePanel.BackColor; } set { imagePanel.BackColor = value; } }
        public override Image BackgroundImage { get { return null; } }
        public override ImageLayout BackgroundImageLayout { get { return ImageLayout.None; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public override Cursor Cursor { get { return base.Cursor; } set { base.Cursor = value; } }
        #endregion

        #region events
        public delegate void MapEvent();
        /// <summary>
        ///     Событие, вызываемое после отрисовки карты
        /// </summary>
        public MapEvent OnMapUpdate = null;
        /// <summary>
        ///     Событие, вызываемое при смене типа карты
        /// </summary>
        public MapEvent OnMapChange = null;
        public delegate string GetTilePathCall(int x, int y, int z);
        public GetTilePathCall UserDefinedGetTileUrl = null;
        #endregion

        private int WebRequestTimeout_ = 100000;
        public int WebRequestTimeout { get { return WebRequestTimeout_; } set { WebRequestTimeout_ = value; } }
        
        private bool _invertBackground = false;
        public bool InvertBackground { get { return _invertBackground; } set { _invertBackground = value; this.ReloadMap(); } }

        private bool _useDiskCache = false;
        public bool UseDiskCache { get { return _useDiskCache; } set { _useDiskCache = value; ShowUseCacheBtn.Checked = _useDiskCache; } }

        private bool _useDefContMen = false;
        public bool UseDefaultContextMenu { get { return _useDefContMen; } set { _useDefContMen = value; } }

        private System.Globalization.CultureInfo stream_CultureInfo = System.Globalization.CultureInfo.InstalledUICulture;
        private System.Globalization.NumberFormatInfo ni;

        #region MapTool
        /// <summary>
        ///     Инструмент карты   
        /// </summary>
        public enum MapTools : uint
        {
            /// <summary>
            ///     Сдвиг
            /// </summary>
            mtShift = 0,
            /// <summary>
            ///     Приближение
            /// </summary>
            mtZoomIn = 1,
            /// <summary>
            ///     Отдаление
            /// </summary>
            mtZoomOut = 2
        }
        /// <summary>
        ///     Инструмент карты
        /// </summary>
        private MapTools MapTool_ = MapTools.mtShift;
        /// <summary>
        ///     Инструмент карты
        /// </summary>
        public MapTools MapTool
        {
            set
            {
                MapTool_ = value;
                if (value == MapTools.mtShift) { this.Cursor = DefaultMapCursor_; mttShift.Checked = true; mttZoomIn.Checked = false; mttZoomOut.Checked = false; };
                if (value == MapTools.mtZoomIn) { this.Cursor = MapCursors.ZoomIn; mttShift.Checked = false; mttZoomIn.Checked = true; mttZoomOut.Checked = false; };
                if (value == MapTools.mtZoomOut) { this.Cursor = MapCursors.ZoomOut; mttShift.Checked = false; mttZoomIn.Checked = false; mttZoomOut.Checked = true; };
            }
            get { return MapTool_; }
        }
        #endregion

        #region Map Services, Sources & Projections
        /// <summary>
        ///     Map Sources Projections
        /// </summary>
        public enum ImageSourceProjections
        {
            /// <summary>
            ///     EPSG:4326 (WGS84)
            /// </summary>
            EPSG4326 = 0x01,
            /// <summary>
            ///     EPSG:3857 (Spherical Mercator)
            /// </summary>
            EPSG3857 = 0x02
        }
        /// <summary>
        ///     Map Sources Projection
        /// </summary>
        private ImageSourceProjections ImageSourceProjection_ = ImageSourceProjections.EPSG4326;
        /// <summary>
        ///     Map Sources Projection
        /// </summary>
        public ImageSourceProjections ImageSourceProjection { get { return ImageSourceProjection_; } set { if (ImageSourceService_ != MapServices.Custom_UserDefined) return; ImageSourceProjection_ = value; ReloadMap(); } }
        /// <summary>
        ///     Источник данных карты
        /// </summary>
        public enum MapServices
        {
            Navicom_Tiles = 0x01,
            Navicom_WMS = 0x02,
            OSM_Mapnik = 0x03,
            OSM_Openvkarte = 0x04,
            OSM_Wikimapia = 0x05,
            
            OSM_MapSerfer = 0x07,
            OSM_Cyclemap = 0x08,
            OSM_MapQuest = 0x09,
            OSM_German = 0x10,
            OSM_Gray = 0x11,
            OSM_Aqua = 0x12,
            OSM_HOT = 0x13,
            
            Rosreestr = 0x06,
            Custom_LocalFiles = 0xFD,
            Custom_LocalHost_WMS7759 = 0x0FE,
            Custom_UserDefined = 0xFF
        }
        /// <summary>
        ///     Источник данных карты
        /// </summary>
        private MapServices ImageSourceService_ = MapServices.Custom_UserDefined;
        /// <summary>
        ///     Источник данных карты
        /// </summary>
        public MapServices ImageSourceService
        {
            get { return ImageSourceService_; }
            set
            {
                if (ImageSourceService_ == value) return;
                ImageSourceService_ = value;
                if(value == MapServices.Navicom_Tiles)
                {                    
                    ImageSourceType_ = ImageSourceTypes.tiles;
                    ImageSourceUrl_ = "http://maps.navicom.ru/nms/getTile.ashx?TEST;{x};{y};{z}";
                    ReloadMap();
                };
                if(value == MapServices.Navicom_WMS)
                {
                    ImageSourceType_ = ImageSourceTypes.single;
                    ImageSourceProjection_ = ImageSourceProjections.EPSG4326;
                    ImageSourceUrl_ = "http://maps.navicom.ru/nms/getMapWMS.ashx?key=TEST&{wms}";
                    ReloadMap();
                };
                if(value == MapServices.OSM_Mapnik)
                {
                    ImageSourceType_ = ImageSourceTypes.tiles;
                    ImageSourceUrl_ = "http://tile.openstreetmap.org/{z}/{x}/{y}.png";
                    ReloadMap();
                };
                if(value == MapServices.OSM_Openvkarte)
                {
                    ImageSourceType_ = ImageSourceTypes.tiles;
                    ImageSourceUrl_ = "http://tile.xn--pnvkarte-m4a.de/tilegen/{z}/{x}/{y}.png";
                    ReloadMap();
                };
                if (value == MapServices.OSM_Wikimapia)
                {
                    ImageSourceType_ = ImageSourceTypes.tiles;
                    ImageSourceUrl_ = "http://i{w}.wikimapia.org/?lng=1&x={x}&y={y}&zoom={z}";
                    ReloadMap();
                };
                if (value == MapServices.OSM_MapSerfer)
                {
                    ImageSourceType_ = ImageSourceTypes.tiles;
                    ImageSourceUrl_ = "http://129.206.74.245:8001/tms_r.ashx?x={x}&y={y}&z={z}";
                    ReloadMap();
                };
                if (value == MapServices.OSM_Cyclemap)
                {
                    ImageSourceType_ = ImageSourceTypes.tiles;
                    ImageSourceUrl_ = "http://{s}.tile.opencyclemap.org/cycle/{z}/{x}/{y}.png";
                    ReloadMap();
                };
                if (value == MapServices.OSM_MapQuest)
                {
                    ImageSourceType_ = ImageSourceTypes.tiles;
                    ImageSourceUrl_ = "http://otile{s}.mqcdn.com/tiles/1.0.0/map/{z}/{x}/{y}.jpg";
                    ReloadMap();
                };
                if (value == MapServices.OSM_German)
                {
                    ImageSourceType_ = ImageSourceTypes.tiles;
                    ImageSourceUrl_ = "http://{s}.tile.openstreetmap.de/tiles/osmde/{z}/{x}/{y}.png";
                    ReloadMap();
                };
                if (value == MapServices.OSM_Gray)
                {
                    ImageSourceType_ = ImageSourceTypes.tiles;
                    ImageSourceUrl_ = "http://{s}.tiles.wmflabs.org/bw-mapnik/{z}/{x}/{y}.png";
                    ReloadMap();
                };
                if (value == MapServices.OSM_Aqua)
                {
                    ImageSourceType_ = ImageSourceTypes.tiles;
                    ImageSourceUrl_ = "http://{s}.tile.stamen.com/watercolor/{z}/{x}/{y}.png";
                    ReloadMap();
                };
                if (value == MapServices.OSM_HOT)
                {
                    ImageSourceType_ = ImageSourceTypes.tiles;
                    ImageSourceUrl_ = "http://{s}.tile.openstreetmap.fr/hot/{z}/{x}/{y}.png";
                    ReloadMap();
                };             
                if(value == MapServices.Rosreestr)
                {
                    ImageSourceType_ = ImageSourceTypes.tiles;
                    ImageSourceUrl_ = "http://c.maps.rosreestr.ru/ArcGIS/rest/services/BaseMaps/BaseMap/MapServer/tile/{z}/{y}/{x}";
                    ReloadMap();
                };
                if (value == MapServices.Custom_LocalFiles)
                {
                    ImageSourceType_ = ImageSourceTypes.tiles;
                    if(ImageSourceUrl_.ToLower().IndexOf("http://") == 0)
                        ImageSourceUrl_ = NaviMapNet.NaviMapNetViewer.GetCurrentDir() + @"\CACHE\" + ((int)MapServices.OSM_Mapnik).ToString("X8") +  @"\L{l}\R{r}\C{c}.png";
                    ReloadMap();
                };
                if (value == MapServices.Custom_LocalHost_WMS7759)
                {
                    ImageSourceType_ = ImageSourceTypes.single;
                    ImageSourceUrl_ = "http://127.0.0.1:7759/?{wms}";
                    ReloadMap();
                };
                selMapType.SelectedItem = ImageSourceService_;
                if (OnMapChange != null) OnMapChange();
            }
        }

        public static string GetCurrentDir()
        {
            string fname = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.ToString();
            fname = fname.Replace("file:///", "");
            fname = fname.Replace("/", @"\");
            fname = fname.Substring(0, fname.LastIndexOf(@"\") + 1);
            return fname;
        }

        /// <summary>
        ///     Тип картинки
        /// </summary>
        public enum ImageSourceTypes
        {
            single = 0x01,
            tiles = 0x02
        }
        /// <summary>
        ///     Тип картинки
        /// </summary>
        private ImageSourceTypes ImageSourceType_ = ImageSourceTypes.tiles;
        /// <summary>
        ///     Тип источника картинки
        /// </summary>
        public ImageSourceTypes ImageSourceType { get { return ImageSourceType_; } set { if(ImageSourceService_ != MapServices.Custom_UserDefined) return; ImageSourceType_ = value; ReloadMap(); } }
        /// <summary>
        ///     Путь получения карты
        /// </summary>
        private string ImageSourceUrl_ = "";
        /// <summary>
        ///     Путь получения карты
        ///     {x} - x tile
        ///     {y} - y tile
        ///     {z} - zoom
        ///     {l} - arcgis level
        ///     {r} - arcgis row
        ///     {c} - arcgis column
        ///     {wms} - WMS request replacement
        ///     {w} - wikimapia server replacement
        ///     {s} - a/b/c replacement
        /// </summary>
        public string ImageSourceUrl { get { return ImageSourceUrl_; } set { 
            if ((ImageSourceService_ != MapServices.Custom_UserDefined) &&
                (ImageSourceService_ != MapServices.Custom_LocalFiles)
                )return; 
            ImageSourceUrl_ = value; ReloadMap();
        } }
        #endregion
        
        #region Map Center & Zoom Coordinates
        /// <summary>
        ///     Центр карты в градусах WGS84, Широта
        /// </summary>
        private double CenterLat_ = 52.59065;
        /// <summary>
        ///     Центр карты в градусах WGS84, Долгота
        /// </summary>
        private double CenterLon_ = 39.55;
        /// <summary>
        ///     Центр карты в градусах WGS84, Широта
        /// </summary>
        public double CenterDegreesLat
        {
            get
            {
               return CenterLat_;
            }
            set
            {
                if(value < -90) return;
                if(value > 90) return;
                CenterLat_ = value;
                ReloadMap();
            }
        }
        /// <summary>
        ///     Центр карты в градусах WGS84, Широта
        /// </summary>
        public double CenterDegreesY { get { return CenterLat_; } set { CenterDegreesLat = value; } }
        /// <summary>
        ///     Центр карты в градусах WGS84, Долгота
        /// </summary>
        public double CenterDegreesLon
        {
            get
            {
               return CenterLon_;
            }
            set
            {
                if(value < -180) return;
                if(value > 180) return;
                CenterLon_ = value;
                ReloadMap();
            }
        }
        /// <summary>
        ///     Центр карты в градусах WGS84, Долгота
        /// </summary>
        public double CenterDegreesX { get { return CenterLon_; } set { CenterDegreesLon = value; }}
        /// <summary>
        ///     Центр карты в метрах проекции, X
        /// </summary>
        public double CenterMetersX
        {
            get { return CenterMeters.X; }
            set
            {
                CenterDegreesLon = TilesProjection.x2lon_m(value);;
            }
        }
        /// <summary>
        ///     Центр карты в метрах проекции, Y
        /// </summary>
        public double CenterMetersY
        {
            get { return CenterMeters.Y; }
            set
            {
                CenterDegreesLat = TilesProjection.y2lat_m(value);
            }
        }
        /// <summary>
        ///     Центр карты в пикселях, X
        /// </summary>
        public int CenterPixelsX
        {
            get { return CenterPixels.X; }
            set
            {
                double[] ll = TilesProjection.fromPixelToLL(new double[] { value, CenterPixels.Y }, ZoomID);
                CenterDegreesLon = ll[0];
            }
        }
        /// <summary>
        ///     Центр карты в пикселях, Y
        /// </summary>
        public int CenterPixelsY
        {
            get { return CenterPixels.Y; }
            set
            {
                double[] ll = TilesProjection.fromPixelToLL(new double[] { CenterPixels.X, value }, ZoomID);
                CenterDegreesLat = ll[1];
            }
        }
        /// <summary>
        ///     Центр карты в пикселях
        /// </summary>
        public Point CenterPixels
        {
            get
            {
                double[] c = TilesProjection.fromLLtoPixel(new double[] { CenterLon_, CenterLat_ }, this.ZoomID_);
                return new Point((int)c[0], (int)c[1]);
            }
            set
            {
                double[] c = TilesProjection.fromPixelToLL(new double[] { value.X, value.Y }, ZoomID_);
                CenterLat_ = c[1];
                CenterLon_ = c[0];
                ReloadMap();
            }
        }
        /// <summary>
        ///     Центр карты в метрах проекции
        /// </summary>
        public PointF CenterMeters
        {
            get
            {
                return TilesProjection.fromLLtoMeter(new PointF((float)CenterLon_,(float)CenterLat_));
            }
            set
            {
                CenterLat_ = TilesProjection.y2lat_m(value.Y);
                CenterLon_ = TilesProjection.x2lon_m(value.X);
                ReloadMap();
            }
        }
        /// <summary>
        ///     Центр карты в градусах, WGS84
        /// </summary>
        public PointF CenterDegrees
        {
            get
            {
                return new PointF((float)CenterLon_, (float)CenterLat_);
            }
            set
            {
                CenterLat_ = value.Y;
                CenterLon_ = value.X;
                ReloadMap();
            }
        }        
        /// <summary>
        ///     Максимальный зум карты
        /// </summary>
        private byte MapMaxZoom_ = 18;
        /// <summary>
        ///     Максимальный зум карты
        /// </summary>
        public byte MapMaxZoom { get { return MapMaxZoom_; } set { if (value > 18) return; if (value < 2) return; MapMaxZoom_ = value; } }
        /// <summary>
        ///     Минимальный зум карты
        /// </summary>
        private byte MapMinZoom_ = 2;
        /// <summary>
        ///     Минимальный зум карты
        /// </summary>
        public byte MapMinZoom { get { return MapMinZoom_; } set { if (value > 18) return; if (value < 2) return; MapMinZoom_ = value; } }
        /// <summary>
        ///     Максимальный зум запрашиваемых тайлов
        /// </summary>
        private byte TilesMaxZoom_ = 21;
        /// <summary>
        ///     Максимальный зум запрашиваемых тайлов
        /// </summary>
        public byte TilesMaxZoom { get { return TilesMaxZoom_; } set { if (value > 21) return; if (value < 1) return; TilesMaxZoom_ = value; } }
        /// <summary>
        ///     Минимальный зум запрашиваемых тайлов
        /// </summary>
        private byte TilesMinZoom_ = 1;
        /// <summary>
        ///     Минимальный зум запрашиваемых тайлов
        /// </summary>
        public byte TilesMinZoom { get { return TilesMinZoom_; } set { if (value > 21) return; if (value < 1) return; TilesMinZoom_ = value; } }
        private double[] ZoomLevels_ = new double[22];
        [Browsable(false)]
        /// <summary>
        ///     Zooms Resolutions
        /// </summary>
        public double[] ZoomsResolutions { get { double[] res = new double[ZoomLevels_.Length]; for (int i = 0; i < ZoomLevels_.Length; i++) res[i] = ZoomLevels_[i]; return res; } }
        [Browsable(false)]
        /// <summary>
        ///     Zooms Scales
        /// </summary>
        public double[] ZoomsScales { get { double[] res = new double[ZoomLevels_.Length]; for (int i = 0; i < ZoomLevels_.Length; i++) res[i] = 559082264 / Math.Pow(2, i); return res; } }
        /// <summary>
        ///     Текущий зум карты
        /// </summary>
        private byte ZoomID_ = 10;
        /// <summary>
        ///     Текущий зум карты
        /// </summary>
        public byte ZoomID { get { return ZoomID_; } set { if (value < MapMinZoom_) return; if (value > MapMaxZoom_) return; ZoomID_ = value; ReloadMap(); } }
        /// <summary>
        ///     Текущее разрешение карты (mpp) метров на пиксель
        /// </summary>
        public double ZoomResolution { get { return ZoomLevels_[ZoomID_]; } }
        /// <summary>
        ///     Scale Denominator (масштаб)
        /// </summary>
        public double ZoomScale { get { return 559082264 / Math.Pow(2, ZoomID_); } }
        #endregion

        #region Map Image
        /// <summary>
        ///     Храним подложку карты без векторных слоев
        /// </summary>
        private Image ImageBackup_ = null;
        /// <summary>
        ///     Цвет подложки карты (используется для получения прозрачных изображений)
        /// </summary>
        public Color MapBackgroundColor { set { _backgroundcolor = value; } get { return _backgroundcolor; } }
        /// <summary>
        ///     Цвет для закрашивания ненайденных тайлов (закрашивается только при выводе либо границ тайлов, либо номеров)
        /// </summary>
        private Color NotFoundTileColor_ = Color.Transparent;
        /// <summary>
        ///     Цвет для закрашивания ненайденных тайлов (закрашивается только при выводе либо границ тайлов, либо номеров)
        /// </summary>
        public Color NotFoundTileColor { get { return NotFoundTileColor_; } set { NotFoundTileColor_ = value; } }
        /// <summary>
        ///     Изображение карты
        /// </summary>
        public Image MapImage
        {
            get
            {
                if (mapImage.Image == null) return null;
                Bitmap b = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(b);
                g.DrawImage(mapImage.Image, 0, 0, new Rectangle(128,128,this.Width,this.Height),GraphicsUnit.Pixel);
                g.Dispose();
                return (Image)b;
            }
        }
        /// <summary>
        ///     Получение изображение карты, где цвет подложки карты прозрачный
        /// </summary>
        [Browsable(false)]
        public Image ImageTransparent
        {
            get
            {
                Bitmap bmp = new Bitmap(MapImage);
                bmp.MakeTransparent(_backgroundcolor);
                return bmp;
            }
        }
        /// <summary>
        ///     Изображение карты
        /// </summary>
        [Browsable(false)]
        public Bitmap ImageGIF
        {
            get
            {
                Bitmap bmp = new Bitmap(MapImage);
                bmp.MakeTransparent(_backgroundcolor);
                System.IO.MemoryStream ms = new MemoryStream();
                bmp.Save(ms, ImageGIFCodec, new System.Drawing.Imaging.EncoderParameters(0));
                ms.Position = 0;
                return new Bitmap(ms);
            }
        }
        /// <summary>
        ///     Получение изображение карты, где цвет подложки карты прозрачный
        /// </summary>
        [Browsable(false)]
        public Bitmap ImageGIFTransparent
        {
            get 
            {
                return MakeTransparentGif(new Bitmap(ImageGIF), _backgroundcolor); 
            }
        }
        /// <summary>
        ///     Получение изображение карты
        /// </summary>
        [Browsable(false)]
        public Bitmap ImagePNG
        {
            get
            {
                Bitmap bmp = new Bitmap(MapImage);
                System.IO.MemoryStream ms = new MemoryStream();
                bmp.Save(ms, ImagePNGCodec, new System.Drawing.Imaging.EncoderParameters(0));
                ms.Position = 0;
                return new Bitmap(ms);
            }
        }
        /// <summary>
        ///     Получение изображение карты, где цвет подложки карты прозрачный
        /// </summary>
        [Browsable(false)]
        public Bitmap ImagePNGTransparent
        {
            get
            {
                Bitmap bmp = new Bitmap(MapImage);
                bmp.MakeTransparent(_backgroundcolor);
                // bmp = CopyToBpp(bmp, 8);
                // bmp = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);                
                System.IO.MemoryStream ms = new MemoryStream();
                bmp.Save(ms, ImagePNGCodec, new System.Drawing.Imaging.EncoderParameters(0));
                ms.Position = 0;
                return new Bitmap(ms);
            }
        }
        /// <summary>
        ///     Получение полного изображение карты (заэкранного), где цвет подложки карты прозрачный
        /// </summary>
        [Browsable(false)]
        public Image MapImageOversize
        {
            get
            {
                if (mapImage == null) return null;
                return (Image)mapImage.Image.Clone();
            }
        }
        /// <summary>
        ///     Получение подложки карты
        /// </summary>
        [Browsable(false)]
        public Image MapImageBackground
        {
            get
            {
                if (mapImage.Image == null) return null;
                Bitmap b = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(b);
                g.DrawImage(ImageBackup_, 0, 0, new Rectangle(128, 128, this.Width, this.Height), GraphicsUnit.Pixel);
                g.Dispose();
                return (Image)b;
            }
        }
        /// <summary>
        ///     Получение полного изображениеподложки карты (заэкранного)
        /// </summary>
        [Browsable(false)]
        public Image MapImageBackgroundOversize
        {
            get
            {
                if (mapImage == null) return null;
                return (Image)ImageBackup_.Clone();
            }
        }
        /// <summary>
        ///     Получение векторных слоев карты
        /// </summary>
        [Browsable(false)]
        public Image MapImageDataLayers
        {
            get
            {
                if (mapImage.Image == null) return null;
                Bitmap ovedata = new Bitmap(this.OversizeWidth, this.OversizeHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Graphics dv = Graphics.FromImage(ovedata);
                DrawOnMapData(dv);
                DrawSelection(dv);
                dv.Dispose();

                Bitmap b = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(b);
                g.DrawImage(ovedata, 0, 0, new Rectangle(128, 128, this.Width, this.Height), GraphicsUnit.Pixel);
                g.Dispose();
                ovedata.Dispose();
                return (Image)b;
            }
        }
        /// <summary>
        ///     Получение векторных слоев карты карты (заэкранного)
        /// </summary>
        [Browsable(false)]
        public Image MapImageDataLayersOversize
        {
            get
            {
                if (mapImage.Image == null) return null;
                Bitmap ovedata = new Bitmap(this.OversizeWidth, this.OversizeHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Graphics dv = Graphics.FromImage(ovedata);
                DrawOnMapData(dv);
                DrawSelection(dv);
                dv.Dispose();
                return (Image)ovedata;
            }
        }
        /// <summary>
        ///     Кодек PNG
        /// </summary>
        [Browsable(false)]
        public System.Drawing.Imaging.ImageCodecInfo ImagePNGCodec
        {
            get
            {
                foreach (System.Drawing.Imaging.ImageCodecInfo c in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()) if (c.MimeType == "image/png") return c;
                return null;
            }
        }
        /// <summary>
        ///     Кодек GIF
        /// </summary>
        [Browsable(false)]
        public System.Drawing.Imaging.ImageCodecInfo ImageGIFCodec
        {
            get
            {
                foreach (System.Drawing.Imaging.ImageCodecInfo c in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()) if (c.MimeType == "image/gif") return c;
                return null;
            }
        }
        /// <summary>   
        ///     Returns a transparent background GIF image from the specified Bitmap.   
        /// </summary>   
        /// <param name="bitmap">The Bitmap to make transparent.</param>   
        /// <param name="color">The Color to make transparent.</param>   
        /// <returns>New Bitmap containing a transparent background gif.</returns>   
        public Bitmap MakeTransparentGif(Bitmap bitmap, Color color)
        {
            byte R = color.R;
            byte G = color.G;
            byte B = color.B;
            MemoryStream fin = new MemoryStream();
            bitmap.Save(fin, System.Drawing.Imaging.ImageFormat.Gif);
            MemoryStream fout = new MemoryStream((int)fin.Length);
            int count = 0;
            byte[] buf = new byte[256];
            byte transparentIdx = 0;
            fin.Seek(0, SeekOrigin.Begin);
            //header 
            count = fin.Read(buf, 0, 13);
            if ((buf[0] != 71) || (buf[1] != 73) || (buf[2] != 70)) return null; //GIF 
            fout.Write(buf, 0, 13);
            int i = 0;
            if ((buf[10] & 0x80) > 0)
            {
                i = 1 << ((buf[10] & 7) + 1) == 256 ? 256 : 0;
            };
            for (; i != 0; i--)
            {
                fin.Read(buf, 0, 3);
                if ((buf[0] == R) && (buf[1] == G) && (buf[2] == B)) // -*=R=*-\|/-*=G=*-\|/-*=B=*- iddqd
                {
                    transparentIdx = (byte)(256 - i);
                };
                fout.Write(buf, 0, 3);
            };
            bool gcePresent = false;
            while (true)
            {
                fin.Read(buf, 0, 1);
                fout.Write(buf, 0, 1);
                if (buf[0] != 0x21) break;
                fin.Read(buf, 0, 1);
                fout.Write(buf, 0, 1);
                gcePresent = (buf[0] == 0xf9);
                while (true)
                {
                    fin.Read(buf, 0, 1);
                    fout.Write(buf, 0, 1);
                    if (buf[0] == 0) break;
                    count = buf[0];
                    if (fin.Read(buf, 0, count) != count) return null;
                    if (gcePresent)
                    {
                        if (count == 4)
                        {
                            buf[0] |= 0x01;
                            buf[3] = transparentIdx;
                        };
                    };
                    fout.Write(buf, 0, count);
                };
            };
            while (count > 0)
            {
                count = fin.Read(buf, 0, 1);
                fout.Write(buf, 0, 1);
            };
            fin.Close();
            fout.Flush();
            return new Bitmap(fout);
        }
        #endregion

        #region Map Tools & Options
        /// <summary>
        ///     Отображать выбор карты
        /// </summary>
        public bool ShowMapTypes { get { return selMapType.Visible; } set { selMapType.Visible = value; } }
        /// <summary>
        ///     Последний запрашиваемый файл карты
        /// </summary>
        private string LastRequestedFile_ = "";
        /// <summary>
        ///     Последний запрашиваемый файл карты
        /// </summary>
        public string LastRequestedFile { get { return LastRequestedFile_; } }
        /// <summary>
        ///     Курсор карты по умолчанию
        /// </summary>
        private Cursor DefaultMapCursor_ = MapCursors.Shift;
        /// <summary>
        ///     Курсор карты по умолчанию
        /// </summary>
        public Cursor DefaultMapCursor { get { return DefaultMapCursor_; } set { DefaultMapCursor_ = value; ChangeKeyedMapCursor(); } }
        /// <summary>
        ///     Отрисовывать карту
        /// </summary>
        private bool DrawMap_ = false;
        /// <summary>
        ///     Отрисовывать карту
        /// </summary>
        public bool DrawMap { get { return DrawMap_; } set { DrawMap_ = value; ReloadMap(); } }
        /// <summary>
        ///     Отображать на тайлы его XYZ
        /// </summary>
        private bool DrawTileXYZ_ = false;
        /// <summary>
        ///     Отображать на тайлы его XYZ
        /// </summary>
        public bool DrawTilesXYZ { get { return DrawTileXYZ_; } set { DrawTileXYZ_ = value; ShowXYZBtn.Checked = DrawTileXYZ_; } }
        /// <summary>
        ///     Отображать границы тайлов
        /// </summary>
        private bool DrawTileBorder_ = false;
        /// <summary>
        ///     Отображать границы тайлов
        /// </summary>
        public bool DrawTilesBorder { get { return DrawTileBorder_; } set { DrawTileBorder_ = value; ShowTileBorderBtn.Checked = DrawTileBorder_; } }
        /// <summary>
        ///     Масштаб зоны для нарезки тайлов X*X в тайлах, 0 или 1 - не выводить
        /// </summary>
        private short TilesRenderingZoneSize_ = 0;
        /// <summary>
        ///     Масштаб зоны для нарезки тайлов X*X в тайлах, 0 или 1 - не выводить
        /// </summary>
        public short TilesRenderingZoneSize { get { return TilesRenderingZoneSize_; } set { TilesRenderingZoneSize_ = value; } }
        /// <summary>
        ///     Отрисовывать векторные слои
        /// </summary>
        private bool DrawVector_ = true;
        /// <summary>
        ///     Отрисовывать векторные слои
        /// </summary>
        public bool DrawVector { get { return DrawVector_;  } set { DrawVector_ = value; DrawOnMapData(); } }
        /// <summary>
        ///     Отображать линейку зумов
        /// </summary>
        public bool ShowZooms { set { zoomLevels.Visible = value; ShowZoomsBtn.Checked = zoomLevels.Visible; } get { return zoomLevels.Visible; } }
        /// <summary>
        ///     Отображать перекрестие карты
        /// </summary>
        public bool ShowCross { set { crossImage.Visible = value; ShowCrossBtn.Checked = crossImage.Visible; } get { return crossImage.Visible; } }
        /// <summary>
        ///     Отображать шкалу масштаба
        /// </summary>
        public bool ShowScale { set { scaleImage.Visible = value; ShowZoomBtn.Checked = scaleImage.Visible; } get { return scaleImage.Visible; } }
        /// <summary>
        ///     Сдвиг на эту величину (в пикселях) считается случайным и не обрабатывается
        /// </summary>
        private byte MapMinMovement_ = 35;
        /// <summary>
        ///     Сдвиг на эту величину (в пикселях) считается случайным и не обрабатывается
        /// </summary>
        public byte MapMinMovement { get { return MapMinMovement_; } set { MapMinMovement_ = value; } }
        /// <summary>
        ///     Размер увеличинной карты (заэкранной), ширина
        /// </summary>
        public int OversizeWidth { get { return this.Width + 256; } }
        /// <summary>
        ///     Размер увеличинной карты (заэкранной), высота
        /// </summary>
        public int OversizeHeight { get { return this.Height + 256; } }
        #endregion

        #region Mouse Coordinates
        /// <summary>
        ///     Координаты курсора относительно центра карты в экранных пикселях
        /// </summary>
        private Point MousePositionScreen_ = new Point();
        /// <summary>
        ///     Координаты точки нажатия карты относительно центра карты в экранных пикселях
        /// </summary>
        private Point MousePositionDown_ = new Point();
        /// <summary>
        ///     Map is Moving By Mouse
        /// </summary>
        private bool MouseMapMove_ = false;
        private bool MouseMoveReal = false;
        private bool MouseMapZoom_ = false;
        private bool MouseMapSelection_ = false;
        private bool BreakNext = false;
        /// <summary>
        ///     Координаты курсора относительно центра карты в экранных пикселях
        /// </summary>
        public Point MousePositionScreen { get { return MousePositionScreen_; } }
        /// <summary>
        ///     Координаты курсора относительно карты в пикселях карты
        /// </summary>
        public Point MousePositionPixels
        {
            get
            {
                Point mp = MousePositionScreen_;
                Point mc = CenterPixels;
                return new Point(mc.X + mp.X, mc.Y + mp.Y);
            }
        }
        /// <summary>
        ///     Координаты курсора относительно карты в градусах
        /// </summary>
        public PointF MousePositionDegrees
        {
            get
            {
                Point px = MousePositionPixels;
                double[] ll = TilesProjection.fromPixelToLL(new double[] { px.X, px.Y }, ZoomID_);
                return new PointF((float)ll[0], (float)ll[1]);
            }
        }
        /// <summary>
        ///     Координаты курсора относительно карты в метрах проеции
        /// </summary>
        public PointF MousePositionMeters
        {
            get
            {
                return TilesProjection.fromLLtoMeter(MousePositionDegrees);                
            }
        }

        /// <summary>
        ///     Координаты последнего клика относительно центра карты в экранных пикселях
        /// </summary>
        public Point MouseDownScreen { get { return MousePositionDown_; } }

        /// <summary>
        ///     Координаты последнего клика относительно центра карты в пикселях карты
        /// </summary>
        public Point MouseDownPixels
        {
            get
            {
                Point mp = MousePositionDown_;
                Point mc = CenterPixels;
                return new Point(mc.X + mp.X, mc.Y + mp.Y);
            }
        }

        /// <summary>
        ///     Координаты последнего клика относительно карты в градусах
        /// </summary>
        public PointF MouseDownDegrees
        {
            get
            {
                Point px = MouseDownPixels;
                double[] ll = TilesProjection.fromPixelToLL(new double[] { px.X, px.Y }, ZoomID_);
                return new PointF((float)ll[0], (float)ll[1]);
            }
        }

        /// <summary>
        ///     Координаты последнего клика относительно карты в метрах проеции
        /// </summary>
        public PointF MouseDownMeters
        {
            get
            {
                return TilesProjection.fromLLtoMeter(MouseDownDegrees);
            }
        }
        #endregion 

        #region Map Vector Layers
        // <summary>
        ///     Слои карты
        /// </summary>
        private MapLayers _mapLayers = null;
        private MapLayers _selLayer = null;
        /// <summary>
        ///     Векторные слои карты
        /// </summary>
        [Browsable(false)]
        public MapLayers MapLayers { get { return _mapLayers; } }
        /// <summary>
        ///     Число векторных слоев карты
        /// </summary>
        public int MapLayersCount { get { return _mapLayers.Count; } }
        /// <summary>
        ///     Число видимых векторных слоев карты
        /// </summary>
        public int MapLayersVisibleCount { get { return _mapLayers.VisibleLayersCount; } }
        /// <summary>
        ///     Число скрытых векторных слоев карты
        /// </summary>
        public int MapLayersHiddenCount { get { return _mapLayers.HiddenLayersCount; } }
        /// <summary>
        ///     Список векторных слоев карты
        /// </summary>
        public string[] MapLayersNames 
        {
            get
            {
                if(_mapLayers.Count == 0) return new string[0];
                string[] res = new string[_mapLayers.Count];
                for (int i = 0; i < res.Length; i++) res[i] = _mapLayers[i].Name;
                return res;
            }
        }
        /// <summary>
        ///     Цвет подложки карты
        /// </summary>
        private Color _backgroundcolor = Color.White;
        #endregion

        /// <summary>
        ///     Constructor
        /// </summary>
        public NaviMapNetViewer()
        {
            InitializeComponent();

            ni = (System.Globalization.NumberFormatInfo)stream_CultureInfo.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            // Map Types
            selMapType.Items.Add(MapServices.Navicom_Tiles);
            selMapType.Items.Add(MapServices.Navicom_WMS);
            selMapType.Items.Add(MapServices.OSM_Mapnik);
            selMapType.Items.Add(MapServices.OSM_Openvkarte);
            selMapType.Items.Add(MapServices.OSM_Wikimapia);
            selMapType.Items.Add(MapServices.OSM_Cyclemap);
            selMapType.Items.Add(MapServices.OSM_MapQuest);
            selMapType.Items.Add(MapServices.OSM_MapSerfer);
            selMapType.Items.Add(MapServices.OSM_German);
            selMapType.Items.Add(MapServices.OSM_Gray);
            selMapType.Items.Add(MapServices.OSM_Aqua);
            selMapType.Items.Add(MapServices.OSM_HOT);
            selMapType.Items.Add(MapServices.Rosreestr);
            // selMapType.Items.Add(MapServices.Custom_LocalHost_WMS7759);
            selMapType.Items.Add(MapServices.Custom_LocalFiles);
            selMapType.Items.Add(MapServices.Custom_UserDefined);
            selMapType.Text = ImageSourceService_.ToString();

            // Map Bars
            zoomLevels.Image = global::NaviMapNet.Properties.Resources.ZoomLevels;
            zoomLevels.Height = 238;
            zoomLevels.Width = 26;
            crossImage.Image = new Bitmap(9, 9, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(crossImage.Image);
            g.Clear(Color.Transparent);
            g.DrawLine(Pens.Black, new Point(0, 4), new Point(8, 4));
            g.DrawLine(Pens.Black, new Point(4, 0), new Point(4, 8));
            g.DrawLine(Pens.Black, new Point(0, 0), new Point(0, 8));
            g.DrawLine(Pens.Black, new Point(0, 8), new Point(8, 8));
            g.DrawLine(Pens.Black, new Point(8, 8), new Point(8, 0));
            g.DrawLine(Pens.Black, new Point(8, 0), new Point(0, 0));
            g.Dispose();

            // Catch Maouse Events
            MouseMessageFilter mmf = new MouseMessageFilter();
            Application.AddMessageFilter(mmf);
            MouseMessageFilter.MouseMove += (mv = new MouseEventHandler(MouseMessageFilter_MouseMove));
            MouseMessageFilter.MouseDown += (md = new MouseEventHandler(MouseMessageFilter_MouseDown));
            MouseMessageFilter.MouseUp += (mu = new MouseEventHandler(MouseMessageFilter_MouseUp));
            mapImage.MouseWheel += new MouseEventHandler(NaviMapNetViewer_MouseWheel);

            (mapImage as Control).KeyDown += new KeyEventHandler(NaviMapNetViewer_KeyDown);
            (mapImage as Control).KeyUp += new KeyEventHandler(NaviMapNetViewer_KeyUp);

            // Map Defaults
            this.Cursor = DefaultMapCursor_;
            imagePanel.Left = -128;
            imagePanel.Top = -128;
            mapImage.Left = 0;
            mapImage.Top = 0;
            for (int i = 1; i < ZoomLevels_.Length; i++) ZoomLevels_[i] = 156543.0339 / Math.Pow(2, i);
            ImageSourceService = MapServices.OSM_Mapnik;
            selMapType.SelectedItem = ImageSourceService;
            _mapLayers = new MapLayers(this);
            {
                _selLayer = new MapLayers(this);
                MapPolygon mp = new MapPolygon();
                mp.BorderColor = Color.Black;
                mp.BodyColor = Color.FromArgb(75, _selBoxColor);
                MapLayer ml = new MapLayer("SELECTION");
                _selLayer.Add(ml);
                ml.Add(mp);
                ml.Visible = false;
                mp.Visible = false;
                mp.Points = new PointF[4];
            };


            // TestVectorLayers
            //TestVectorLayers();

            // Init Map Size
            ResizeMap();

            //this.DrawMap = true;
            //this.DrawSelectionBox = true;
            //this.UseDefaultContextMenu = true;
            //this.ShowInfoOnDblClick = true;
        }

        public bool DrawSelectionBox { get { return _selLayer[0].Visible; } set { _selLayer[0].Visible = value; if (_selLayer[0].Visible == false) ClearSelectionBox(); else this.DrawOnMapData(); } }
        public bool SelectionBoxIsVisible { get { return _selLayer[0].Visible && _selLayer[0][0].Visible; } }
        public void ClearSelectionBox() { _selLayer[0][0].Visible = false; HSB.Visible = false; hideSelBox.Visible = false; selBoxInfo.Visible = false; save2Shp.Visible = false; Zoom2Sel.Visible = false; this.DrawOnMapData(); }
        private Color _selBoxColor = Color.Black;
        public Color SelectionBoxColor { get { return _selBoxColor; } set { _selBoxColor = value; _selLayer[0][0].BodyColor = Color.FromArgb(75, value); this.DrawOnMapData(); } }

        private bool KeyDownAlt = false;
        private bool KeyDownControl = false;
        private bool KeyDownShift = false;

        private void NaviMapNetViewer_KeyUp(object sender, KeyEventArgs e)
        {            
            if (e.KeyValue == 18) KeyDownAlt = false;
            if (e.KeyValue == 17) KeyDownControl = false;
            if (e.KeyValue == 16) KeyDownShift = false;

            ChangeKeyedMapCursor();            
        }

        private void NaviMapNetViewer_KeyDown(object sender, KeyEventArgs e)
        {           
            if (e.KeyValue == 18) KeyDownAlt = true;
            if (e.KeyValue == 17) KeyDownControl = true;
            if (e.KeyValue == 16) KeyDownShift = true;

            ChangeKeyedMapCursor();
        }

        private void ChangeKeyedMapCursor()
        {
            if (KeyDownControl && (!KeyDownShift) && (!KeyDownAlt))
                this.Cursor = MapCursors.ZoomIn;
            else if ((!KeyDownControl) && KeyDownShift && (!KeyDownAlt) && (DrawSelectionBox))
                this.Cursor = Cursors.Cross;
            else
            {
                if (MapTool_ == MapTools.mtShift) this.Cursor = DefaultMapCursor_;
                if (MapTool_ == MapTools.mtZoomIn) this.Cursor = MapCursors.ZoomIn;
                if (MapTool_ == MapTools.mtZoomOut) this.Cursor = MapCursors.ZoomOut;
            };
        }

        #region MapTestVectorData
        /// <summary>
        ///     Отрисовка проверочного слоя векторных объектов
        /// </summary>
        private void TestVectorLayers()
        {
            MapLayer tl = new MapLayer("TestVectorLayers()");
            _mapLayers.Add(tl);

            MapPoint m0 = new MapPoint(new PointF((float)39.95084, (float)52.49743));
            m0.Color = Color.Purple;
            m0.SizePixels = new Size(14, 14);
            m0.Squared = true;
            tl.Add(m0);

            MapMultiPoint m12 = new MapMultiPoint(new PointF[]{
                new PointF((float)39.93084, (float)52.50743),
                new PointF((float)39.91084, (float)52.47743)
            });
            m12.Color = Color.Navy;
            m12.SizePixels = new Size(16, 16);
            m12.Squared = false;
            tl.Add(m12);


            MapEllipse me = new MapEllipse(new PointF((float)39.93084, (float)52.48743), 4000, 2000);
            me.BorderColor = Color.Red;
            me.BodyColor = Color.Transparent;
            me.LineWidth = 3;
            tl.Add(me);            

            MapLine ml = new MapLine(new PointF((float)39.93084, (float)52.48743), new PointF((float)39.93084, (float)52.49743));
            ml.Color = Color.DarkGreen;
            ml.Width = 5;
            tl.Add(ml);
            
            MapPolyLine mp = new MapPolyLine(new PointF[] { 
                new PointF((float)39.93084, (float)52.48743), 
                new PointF((float)39.95084, (float)52.49743), 
                new PointF((float)39.93084, (float)52.50743),
                new PointF((float)39.91084, (float)52.47743)
            });
            mp.Color = Color.Maroon;
            mp.Width = 3;
            tl.Add(mp);

            MapPolygon ma = new MapPolygon(new PointF[] {
                new PointF((float)39.84084, (float)52.48743), 
                new PointF((float)39.86084, (float)52.49743), 
                new PointF((float)39.84084, (float)52.50743),
                new PointF((float)39.82084, (float)52.47743)
            });
            ma.BodyColor = Color.Lime;
            ma.BorderColor = Color.Tan;
            ma.Width = 8;
            tl.Add(ma);
        }
        /// <summary>
        ///     Отрисовка проверочного слоя векторных объектов
        /// </summary>
        public bool DrawMapTestVectorData
        {
            get
            {
                return MapLayers.Exists("TestVectorLayers()");
            }
            set
            {
                if (value == true)
                {
                    if (MapLayers.Exists("TestVectorLayers()")) return;
                    TestVectorLayers();
                    DrawOnMapData();
                }
                else
                {
                    if (!MapLayers.Exists("TestVectorLayers()")) return;
                    MapLayers.Remove("TestVectorLayers()");
                    DrawOnMapData();
                };
            }
        }
        #endregion

        #region Resize Map
        private void onControlResize(object sender, EventArgs e) { ResizeMap(); }
        private void ResizeMap()
        {
            zoomLevels.Left = Width - zoomLevels.Width - 5;
            zoomLevels.Top = 5;
            crossImage.Left = Width / 2 - 5;
            crossImage.Top = Height / 2 - 5;
            scaleImage.Left = 5;
            scaleImage.Top = Height - 14;
            imagePanel.Width = Width + 256;
            imagePanel.Height = Height + 256;
            mapImage.Width = imagePanel.Width;
            mapImage.Height = imagePanel.Height;
            labelLoading.Top = this.Height - labelLoading.Height;
            labelLoading.Left = this.Width - labelLoading.Width;
            ReloadMap();
        }
        #endregion

        #region Mouse Events
        private bool MouseOverMap_ = false;
        private void NaviMapNetViewer_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((!MouseMapMove_) && MouseOverMap_)
            {
                int inc = e.Delta > 0 ? 1 : -1;
                int newZoom = ZoomID_ + inc;
                if (newZoom < MapMinZoom_) return;
                if (newZoom > MapMaxZoom_) return;

                PointF mp = MousePositionDegrees;
                PointF cp = CenterDegrees;
                double dx = mp.X - cp.X;
                double dy = mp.Y - cp.Y;
                cp.X += (float)(inc > 0 ? dx / 2.0 : -1 * dx);
                cp.Y += (float)(inc > 0 ? dy / 2.0 : -1 * dy);
                CenterLat_ = cp.Y;
                CenterLon_ = cp.X;
               
                ZoomID_ = (byte)newZoom;
                ReloadMap();
            };
        }
        private void MouseMessageFilter_MouseUp(object sender, MouseEventArgs e)
        {
            try // IF DISPOSED
            {
                ChangeKeyedMapCursor();

                if (MouseMapMove_)
                {
                    MouseMapMove_ = false;
                    if ((Math.Abs(mapImage.Top) < MapMinMovement_) && (Math.Abs(mapImage.Left) < MapMinMovement_)) // Случайный сдвиг
                    {
                        mapImage.Left = 0;
                        mapImage.Top = 0;
                        if (!MouseMoveReal) InternalPopup(sender, e);
                        MouseMoveReal = false;
                        return;
                    };

                    Point mc = CenterPixels;
                    mc.X -= mapImage.Left;
                    mc.Y -= mapImage.Top;
                    CenterPixels = mc;
                };
                if (MouseMapZoom_)
                {
                    MouseMapZoom_ = false;
                    Selabel.Visible = false;
                    ZoomBySelection();
                };
                if (MouseMapSelection_)
                {
                    MouseMapSelection_ = false;
                    if (Selabel.Visible)
                    {
                        Selabel.Visible = false;
                        SelShiftArea(sender, e);
                    }
                    else
                        Selabel.Visible = false;
                };
                if(!MouseMoveReal) InternalPopup(sender, e);
                MouseMoveReal = false;
            }
            catch { };
        }

        public void ZoomByArea(double[] minXminYmaxXmaxY)
        {
            ZoomByArea(minXminYmaxXmaxY, -1);
        }

        public void ZoomByArea(RectangleF rect)
        {
            ZoomByArea(new double[] { rect.Left, rect.Bottom, rect.Right, rect.Top }, -1);
        }

        public void ZoomByArea(RectangleF rect, int zoom)
        {
            ZoomByArea(new double[] { rect.Left, rect.Bottom, rect.Right, rect.Top }, zoom);
        }

        public void ZoomByArea(double[] minXminYmaxXmaxY, int zoom)
        {
            double areaWidth = Math.Abs((TilesProjection.lon2x_m(minXminYmaxXmaxY[2]) - TilesProjection.lon2x_m(minXminYmaxXmaxY[0]))) * 0.95;
            double areaHeight = Math.Abs((TilesProjection.lat2y_m(minXminYmaxXmaxY[3]) - TilesProjection.lat2y_m(minXminYmaxXmaxY[1]))) * 0.95;

            int newZoom = ZoomID;
            for (int i = 1; i < ZoomLevels_.Length; i++)
            {
                if ((Math.Min((double)(ZoomLevels_[i] * this.Width) / (double)areaWidth, ((double)(ZoomLevels_[i] * this.Height))/(double)areaHeight)) < 1) 
                    break;
                newZoom = i;
            };
            if (newZoom > MapMaxZoom_) newZoom = MapMaxZoom;

            CenterLat_ = (float)((minXminYmaxXmaxY[1] + minXminYmaxXmaxY[3]) / 2);
            CenterLon_ = (float)((minXminYmaxXmaxY[0] + minXminYmaxXmaxY[2]) / 2);
            ZoomID_ = (zoom < MapMinZoom_) || (zoom > MapMaxZoom_) ? (byte)newZoom : (byte)zoom;

            ReloadMap();
        }

        private void ZoomBySelection()
        {
            if ((Selabel.Width < 20) || (Selabel.Height < 20)) return;

            double currWidth = ZoomLevels_[ZoomID] * Selabel.Width;
            double needWidth = double.MaxValue;
            int newZoom = ZoomID + 1;
            for (int i = 1; i < ZoomLevels_.Length; i++)
            {
                double zoomWidth = Math.Abs(ZoomLevels_[i] * this.Width - currWidth);
                if (zoomWidth < needWidth)
                {
                    needWidth = zoomWidth;
                    newZoom = i;
                };
            };
            if (newZoom > MapMaxZoom_) newZoom = MapMaxZoom_;

            Point pCentSelScreen = this.PointToClient(mapImage.PointToScreen(new Point(Selabel.Left + Selabel.Width / 2, Selabel.Top + Selabel.Height / 2)));
            pCentSelScreen.X = pCentSelScreen.X - Width / 2;
            pCentSelScreen.Y = pCentSelScreen.Y - Height / 2;

            Point mc = CenterPixels;
            Point pCentSelPixels = new Point(mc.X + pCentSelScreen.X, mc.Y + pCentSelScreen.Y);

            double[] pCentSelGeo = TilesProjection.fromPixelToLL(new double[] { pCentSelPixels.X, pCentSelPixels.Y }, ZoomID_);
            CenterLat_ = (float)pCentSelGeo[1];
            CenterLon_ = (float)pCentSelGeo[0];

            ZoomID_ = (byte)newZoom;

            ReloadMap();
        }

        private void InternalPopup(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Right) && (_mouseIsOver))
            {
                if (KeyDownAlt) return;
                KeyDownControl = false;
                KeyDownShift = false;
                ChangeKeyedMapCursor();

                if (_useDefContMen)
                {
                    BreakNext = true;
                    defMenu.Show(e.X, e.Y);
                }
                else if (this.ContextMenuStrip != null)
                {
                    BreakNext = true;
                    this.ContextMenuStrip.Show(e.X, e.Y);
                };
            };
        }

        private void MouseMessageFilter_MouseDown(object sender, MouseEventArgs e)
        {            
            try // IF DISPOSED
            {
            }
            catch { };
        }
        private void MouseMessageFilter_MouseMove(object sender, MouseEventArgs e)
        {
            try // IF DISPOSED
            {
                Point mousePosition = this.PointToClient(e.Location);
                mousePosition.X = mousePosition.X - Width / 2;
                mousePosition.Y = mousePosition.Y - Height / 2;

                //if ((mousePosition.X >= 0) && (mousePosition.Y >= 0) && (mousePosition.X <= Width) && (mousePosition.Y <= Height))
                //   MouseOverMap_ = true;

                MousePositionScreen_ = mousePosition;
                if (MouseMapMove_)
                {
                    MouseMoveReal = true;
                    int _ms_x = MousePositionDown_.X - MousePositionScreen_.X;
                    int _ms_y = MousePositionDown_.Y - MousePositionScreen_.Y;
                    mapImage.Left = -1 * _ms_x;
                    mapImage.Top = -1 * _ms_y;
                    return;
                };
                if (MouseMapZoom_ || MouseMapSelection_)
                {
                    int _ms_x = MousePositionDown_.X - MousePositionScreen_.X;
                    int _ms_y = MousePositionDown_.Y - MousePositionScreen_.Y;
                    Point pFrom = mapImage.PointToClient(new Point(e.X + _ms_x, e.Y + _ms_y));
                    Point pTo = mapImage.PointToClient(new Point(e.X, e.Y));                    
                    Selabel.Left = (int)Math.Min(pFrom.X, pTo.X);
                    Selabel.Top = (int)Math.Min(pFrom.Y, pTo.Y);
                    Selabel.Width = (int)Math.Abs(_ms_x);
                    Selabel.Height = (int)Math.Abs(_ms_y);
                    if (!Selabel.Visible)
                    {
                        Selabel.Parent = mapImage;
                        Selabel.BackColor = Color.Transparent;
                        Selabel.Visible = true;
                    };
                };
            }
            catch { };
        }
        private void mapImage_MouseDown(object sender, MouseEventArgs e)
        {
            try // IF DISPOSED
            {
                mapImage.Select();
                MousePositionDown_ = MousePositionScreen_;

                if (BreakNext)
                {
                    BreakNext = false;
                    if (MouseOverMap_)
                        this.OnMouseDown(e);
                    return;
                };

                if (((e.Button == MouseButtons.Left) && (MapTool_ == MapTools.mtShift) && (KeyDownControl == false) && (KeyDownAlt == false) && (KeyDownShift == false)) || (e.Button == MouseButtons.Right))
                {
                    this.Cursor = MapCursors.Move;
                    MouseMapMove_ = true;
                };

                if ((e.Button == MouseButtons.Left) && (MapTool_ == MapTools.mtShift) && (KeyDownControl) && (KeyDownShift == false) && (KeyDownAlt == false))
                {
                    this.Cursor = MapCursors.ZoomIn;
                    MouseMapZoom_ = true;
                };

                if ((e.Button == MouseButtons.Left) && (MapTool_ == MapTools.mtShift) && (KeyDownShift) && (KeyDownControl == false) && (KeyDownControl == false))
                {
                    if (DrawSelectionBox)
                    {
                        MouseMapSelection_ = true;
                        ClearSelectionBox();
                    };
                };

                if (e.Button == MouseButtons.Left)
                {
                    if (MapTool_ == MapTools.mtZoomIn)
                    {
                        int newZoom = ZoomID_ + 1;
                        if (newZoom > MapMaxZoom_) return;

                        PointF mp = MousePositionDegrees;
                        PointF cp = CenterDegrees;
                        double dx = mp.X - cp.X;
                        double dy = mp.Y - cp.Y;
                        cp.X += (float)(dx / 2.0);
                        cp.Y += (float)(dy / 2.0);
                        CenterLat_ = cp.Y;
                        CenterLon_ = cp.X;

                        ZoomID_ = (byte)newZoom;
                        ReloadMap();
                    };
                    if (MapTool_ == MapTools.mtZoomOut)
                    {
                        int newZoom = ZoomID_ - 1;
                        if (newZoom < MapMinZoom_) return;

                        PointF mp = MousePositionDegrees;
                        PointF cp = CenterDegrees;
                        double dx = mp.X - cp.X;
                        double dy = mp.Y - cp.Y;
                        cp.X -= (float)dx;
                        cp.Y -= (float)dy;
                        CenterLat_ = cp.Y;
                        CenterLon_ = cp.X;

                        ZoomID_ = (byte)newZoom;
                        ReloadMap();
                    };
                };
            }
            catch { };

            if (MouseOverMap_)
                this.OnMouseDown(e);
        }
        private void mapImage_MouseEnter(object sender, EventArgs e)
        {
            MouseOverMap_ = true;
            mapImage.Select();
            this.OnMouseEnter(e);
        }
        private void mapImage_MouseLeave(object sender, EventArgs e)
        {
            MouseOverMap_ = false;
            this.OnMouseLeave(e);
        }
        #endregion                
                                        
        #region Draw Map & Vector Layers
        /// <summary>
        ///     Перерисовка карты
        /// </summary>
        public void ReloadMap()
        {
            zoomLevels.Refresh();
            DrawScaleBar();
            if (!DrawMap_) { mapImage.Image = null; return; };

            if (ImageSourceType_ == ImageSourceTypes.single) // WMS
            {
                string wmsPart = "&SERVICE=WMS&REQUEST=GetMap&VERSION=1.1.1&HEIGHT={he}&WIDTH={wi}&SRS={srs}&BBOX={bbox}";
                wmsPart = wmsPart.Replace("{he}", mapImage.Height.ToString());
                wmsPart = wmsPart.Replace("{wi}", mapImage.Width.ToString());
                if (this.ImageSourceProjection_ == ImageSourceProjections.EPSG4326)
                {
                    wmsPart = wmsPart.Replace("{srs}", "EPSG:4326");
                    double[] bbox = MapBoundsMinMaxOversizeDegrees;
                    wmsPart = wmsPart.Replace("{bbox}", String.Format(ni, "{0},{1},{2},{3}", new object[] { bbox[0], bbox[1], bbox[2], bbox[3] }));
                };
                if (this.ImageSourceProjection_ == ImageSourceProjections.EPSG3857)
                {
                    wmsPart = wmsPart.Replace("{srs}", "EPSG:3857");
                    double[] bbox = MapBoundsMinMaxOversizeMeters;
                    wmsPart = wmsPart.Replace("{bbox}", String.Format(ni, "{0},{1},{2},{3}", new object[] { bbox[0], bbox[1], bbox[2], bbox[3] }));
                };
                string url = ImageSourceUrl.Replace("{wms}", wmsPart);
                LastRequestedFile_ = url;
                mapImage.Image = GetImageFromURL(url, WebRequestTimeout_, Color.Transparent);
                mapImage.Left = 0;
                mapImage.Top = 0;
            };
            if (ImageSourceType_ == ImageSourceTypes.tiles) // TILES
            {
                MapMerger mm = new MapMerger();
                mm.InvertBackground = this.InvertBackground;                
                mm.CacheSubDirectory = String.IsNullOrEmpty(_UserDefinedMapName) ? ((int)ImageSourceService_).ToString("X8") : _UserDefinedMapName;
                mm.UseDiskCache = this.UseDiskCache && (ImageSourceService_ != MapServices.Custom_LocalFiles) && (mm.CacheSubDirectory != (0xFF).ToString("X8"));
                mm.MinZoom = this.TilesMinZoom_;
                mm.MaxZoom = this.TilesMaxZoom_;
                mm.WebRequestTimeout = WebRequestTimeout_;
                mm.NotFoundTileColor = DrawTileBorder_ || DrawTileXYZ_ ? NotFoundTileColor_ : Color.Transparent;
                mm.GetTilePath = this.GetTileUrl;
                mm.DrawTileBorder = DrawTileBorder_;
                mm.TilesRenderingZoneSize = TilesRenderingZoneSize_;
                mm.DrawTileXYZ = DrawTilesXYZ;
                double[] bounds = MapBoundsMinMaxOversizeDegrees;
                labelLoading.Visible = true;
                labelLoading.Refresh();
                mapImage.Image = mm.GetMap(bounds[1], bounds[0], bounds[3], bounds[2], true, this.OversizeWidth, this.OversizeHeight);
                labelLoading.Visible = false;
                mapImage.Left = 0;
                mapImage.Top = 0;
            };

            if (mapImage.Image != null)
                ImageBackup_ = (Image)mapImage.Image.Clone();
            else
                ImageBackup_ = null;
            DrawOnMapData();
            mapImage.Refresh();

            if (OnMapUpdate != null) OnMapUpdate();
        }

        private string _UserDefinedMapName = null;
        public string UserDefinedMapName { get { return _UserDefinedMapName; } set { _UserDefinedMapName = value; } }
        
        /// <summary>
        ///     Перерисовка векторных слоев карты
        /// </summary>
        public void DrawOnMapData()
        {
            if (mapImage.Image == null) return;

            mapImage.Image.Dispose();
            mapImage.Image = (Image)ImageBackup_.Clone();
            Graphics g = Graphics.FromImage(mapImage.Image);

            if (DrawVector_) DrawOnMapData(g);
            DrawSelection(g);
            try
            {
                if (AfterMapDraw != null) 
                    AfterMapDraw(g, new Rectangle(128,128,Width, Height));
            }
            catch { };                        
            g.Dispose();
        }
      
        /// <summary>
        ///     Draw Vectors Data
        /// </summary>
        /// <param name="g"></param>
        private void DrawOnMapData(Graphics g)
        {
            if (MapLayers == null) return;
            if (MapLayers.Count == 0) return;
            if (MapLayers.VisibleLayers == null) return;
            if (MapLayers.VisibleLayersCount == 0) return;

            foreach (MapLayer mapLay in MapLayers.VisibleLayers)
            {
                MapObject[] objs = mapLay.Select(MapBoundsRectOversizeDegrees, MapObjectType.mPoint | MapObjectType.mLine | MapObjectType.mPolygon | MapObjectType.mEllipse | MapObjectType.mPolyline, true, false);
                foreach (MapObject mo in objs)
                {
                    if (mo.ShowObject(ZoomID_))
                    {
                        if (mo.ObjectType == MapObjectType.mPoint)
                        {
                            bool Squared = false;
                            try { Squared = ((MapPoint)mo).Squared; } catch { };
                            try { Squared = ((MapMultiPoint)mo).Squared; } catch { };
                            foreach (PointF pnt in mo.Points) 
                                DrawMapPoint(g, pnt, mo.SizePixels, Squared, mo.BodyColor, mo.Img);
                        };
                        if (mo.ObjectType == MapObjectType.mEllipse)
                        {
                            MapEllipse ellipse = ((MapEllipse)mo);
                            ellipse.ZoomID = ZoomID_;
                            DrawMapEllipse(g, ellipse.Center, ellipse.SizePixels, ellipse.BodyColor, ellipse.BorderColor, ellipse.LineWidth, ellipse.Img);
                        };
                        if (mo.ObjectType == MapObjectType.mLine)
                        {
                            Rectangle bnds = mo.GetBoundsForZoom(ZoomID_);
                            if((mo.DrawEvenSizeIsTooSmall) || ((Math.Abs(bnds.Width) > 2) || (Math.Abs(bnds.Height) > 2)))
                                //for (int i = 1; i < mo.Points.Length; i++) DrawMapLine(g, mo.Points[i - 1], mo.Points[i], mo.BodyColor, mo.SizePixels.Width);
                                DrawMapLines(g, mo.Points, mo.BodyColor, mo.SizePixels.Width);
                        };
                        if (mo.ObjectType == MapObjectType.mPolyline)
                        {
                            Rectangle bnds = mo.GetBoundsForZoom(ZoomID_);
                            if ((mo.DrawEvenSizeIsTooSmall) || ((Math.Abs(bnds.Width) > 2) || (Math.Abs(bnds.Height) > 2)))
                                //for (int i = 1; i < mo.Points.Length; i++) DrawMapLine(g, mo.Points[i - 1], mo.Points[i], mo.BodyColor, mo.SizePixels.Width);
                                DrawMapLines(g, mo.Points, mo.BodyColor, mo.SizePixels.Width);
                        };
                        if (mo.ObjectType == MapObjectType.mPolygon)
                        {
                            Rectangle bnds = mo.GetBoundsForZoom(ZoomID_);
                            if ((mo.DrawEvenSizeIsTooSmall) || ((Math.Abs(bnds.Width) > 2) && (Math.Abs(bnds.Height) > 2)))
                                DrawMapPolygon(g, mo.Points, mo.BodyColor, mo.BorderColor, mo.SizePixels.Width);
                        };
                        if (mo.DrawText)
                        {
                            DrawMapText(g, mo.Points[0], mo.TextOffset, mo.TextFont, mo.TextBrush, mo.Text);
                        };
                    };
                };
            };

            //g.Dispose();
        }

        /// <summary>
        ///     Draw Vectors Data
        /// </summary>
        /// <param name="g"></param>
        private void DrawSelection(Graphics g)
        {
            if(_selLayer[0].Visible)
            {
                MapObject mo = _selLayer[0][0];                
                if (mo.Visible && mo.ShowObject(ZoomID_))
                {
                    Rectangle bnds = mo.GetBoundsForZoom(ZoomID_);
                    if ((mo.DrawEvenSizeIsTooSmall) || ((Math.Abs(bnds.Width) > 2) && (Math.Abs(bnds.Height) > 2)))
                    {
                        DrawMapPolygon(g, mo.Points, mo.BodyColor, mo.BorderColor, mo.SizePixels.Width);
                        for (int i = 0; i < 3; i++)
                        {
                            Point pointFrom = PixelsToScreen(DegreesToPixels(mo.Points[i]));
                            Point pointTo = PixelsToScreen(DegreesToPixels(mo.Points[i + 1]));
                            Pen pen = new Pen(new SolidBrush(Color.White));
                            pen.DashPattern = new float[] { 5, 5 };
                            g.DrawLine(pen, pointFrom.X, pointFrom.Y, pointTo.X, pointTo.Y);
                        };
                        {
                            Point pointFrom = PixelsToScreen(DegreesToPixels(mo.Points[3]));
                            Point pointTo = PixelsToScreen(DegreesToPixels(mo.Points[0]));
                            Pen pen = new Pen(new SolidBrush(Color.White));
                            pen.DashPattern = new float[] { 5, 5 };
                            g.DrawLine(pen, pointFrom.X, pointFrom.Y, pointTo.X, pointTo.Y);
                        };
                    };
                };
            };
        }

        public delegate void AfterDraw(Graphics g, Rectangle rect);
        public AfterDraw AfterMapDraw;        

        /// <summary>
        ///     Draw Map Vector Points
        /// </summary>
        /// <param name="g">Source Image</param>
        /// <param name="point">Point</param>
        /// <param name="size">Size in Pixels</param>
        /// <param name="squared">is Squared</param>
        /// <param name="color">Point Color</param>
        /// <param name="image">Point Image</param>
        private void DrawMapPoint(Graphics g, PointF point, Size size, bool squared, Color color, Image image)
        {
            PointF pxl = PixelsToScreen(DegreesToPixels(point));
            if (squared)
                g.FillRectangle(new System.Drawing.SolidBrush(color), pxl.X - size.Width / 2, pxl.Y - size.Height / 2, size.Width, size.Height);
            else
                g.FillEllipse(new System.Drawing.SolidBrush(color), pxl.X - size.Width / 2, pxl.Y - size.Height / 2, size.Width, size.Height);
            if (image != null && (!image.Size.IsEmpty))
                g.DrawImage(image, pxl.X - size.Width / 2, pxl.Y - size.Height / 2);
        }

        /// <summary>
        ///     Draw Map Text
        /// </summary>
        /// <param name="g">Source Image</param>
        /// <param name="point">Point</param>
        /// <param name="size">Size in Pixels</param>
        /// <param name="squared">is Squared</param>
        /// <param name="color">Point Color</param>
        /// <param name="image">Point Image</param>
        private void DrawMapText(Graphics g, PointF point, Point offset, Font font, Brush brush, string Text)
        {
            PointF pxl = PixelsToScreen(DegreesToPixels(point));
            pxl.X += offset.X;
            pxl.Y += offset.Y;
            g.DrawString(Text, font, brush, pxl);
            //g.FillRectangle(new System.Drawing.SolidBrush(color), pxl.X - size.Width / 2, pxl.Y - size.Height / 2, size.Width, size.Height);            
        }

        /// <summary>
        ///     Draw Vector Ellipse
        /// </summary>
        /// <param name="g">Source Image</param>
        /// <param name="point">Center of Ellipse</param>
        /// <param name="size">Size in Pixels</param>
        /// <param name="bodyColor">Fill Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="lineWidth">Line Width</param>
        /// <param name="image">Ellipse Image</param>
        private void DrawMapEllipse(Graphics g, PointF point, Size size, Color bodyColor, Color borderColor, int lineWidth, Image image)
        {
            PointF pxl = PixelsToScreen(DegreesToPixels(point));
            Pen pen = new Pen(new SolidBrush(borderColor), lineWidth);
            Rectangle rect = new Rectangle(new Point((int)(pxl.X - size.Width / 2), (int)(pxl.Y - size.Height / 2)), size);
            g.DrawEllipse(pen, rect);
            g.FillEllipse(new SolidBrush(bodyColor), rect);

            if (image != null) g.DrawImage(image, (int)pxl.X - image.Width / 2, pxl.Y - image.Height / 2);
        }
        /// <summary>
        ///     Draw Vector Line
        /// </summary>
        /// <param name="g">Source Image</param>
        /// <param name="pointFrom">point 0</param>
        /// <param name="pointTo">point 1</param>
        /// <param name="color">Line Color</param>
        /// <param name="width">Line Width</param>
        private void DrawMapLine(Graphics g, PointF pointFrom, PointF pointTo, Color color, int width)
        {
            pointFrom = PixelsToScreen(DegreesToPixels(pointFrom));
            pointTo = PixelsToScreen(DegreesToPixels(pointTo));
            Pen pen = new Pen(new SolidBrush(color));
            pen.Width = width;
            g.DrawLine(pen, pointFrom.X, pointFrom.Y, pointTo.X, pointTo.Y);
        }
        private void DrawMapLines(Graphics g, PointF[] points, Color color, int width)
        {
            PointF[] pointsLocal = new PointF[points.Length];
            for(int i=0;i<points.Length;i++)
                pointsLocal[i] = PixelsToScreen(DegreesToPixels(points[i]));
            Pen pen = new Pen(new SolidBrush(color));
            pen.Width = width;
            g.DrawLines(pen, pointsLocal);
        }

        // outroot keenrock woodplace

        /// <summary>
        ///     Draw Vector Polygon
        /// </summary>
        /// <param name="g">Source Image</param>
        /// <param name="points">Points</param>
        /// <param name="bodyColor">Fill Color</param>
        /// <param name="borderColor">Border Color</param>
        /// <param name="borderWidth">Border Line Width</param>
        private void DrawMapPolygon(Graphics g, PointF[] points, Color bodyColor, Color borderColor, int borderWidth)
        {
            Pen pen = new Pen(new SolidBrush(borderColor));
            pen.Width = borderWidth;
            PointF[] pp = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
                pp[i] = PixelsToScreen(DegreesToPixels(points[i]));
            g.DrawPolygon(pen, pp);
            g.FillPolygon(new SolidBrush(bodyColor), pp);
        }
        #endregion

        #region GetTileUrlFor XYZ
        private string GetTileUrl(int x, int y, int z)
        {
            // в первую очередь обрабатываем перекрытие
            string url = "";
            if (UserDefinedGetTileUrl != null)
                url = this.UserDefinedGetTileUrl(x, y, z);
            if (url != "")
            {
                LastRequestedFile_ = url;
                return url;
            };

            Random rnd = new Random();
            string[] abs = new string[]{"a","b","c"};
            // если перекрытие не состоялось - включаем алгоритм
            string path = ImageSourceUrl_;
            path = path.Replace("{x}", x.ToString()).Replace("{y}", y.ToString()).Replace("{z}", z.ToString());
            path = path.Replace("{l}", TwoZ(z < 10 ? z - 4 : z - 10).ToString()).Replace("{r}", ToHex8(y).ToString()).Replace("{c}", ToHex8(x).ToString());
            path = path.Replace("{w}", ((x % 4) + (y % 4) * 4).ToString());
            path = path.Replace("{s}", abs[rnd.Next(1,3)] );
            LastRequestedFile_ = path;
            return path;
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
        #endregion

        #region Selection Bounds
        /// <summary>
        ///     minx,miny,maxx,maxy
        /// </summary>
        public int[] SelectionBoundsMinMaxPixels
        {
            get
            {
                Point tl = TilesProjection.fromLLtoPixel(_selLayer[0][0].Points[0].Y, _selLayer[0][0].Points[0].X, ZoomID_);
                Point br = TilesProjection.fromLLtoPixel(_selLayer[0][0].Points[2].Y, _selLayer[0][0].Points[2].X, ZoomID_);

                return new int[] {tl.X, br.Y, br.X, tl.Y };
            }
        }
        /// <summary>
        ///     minx,miny,maxx,maxy
        /// </summary>
        public double[] SelectionBoundsMinMaxMeters
        {
            get
            {
                double[] d = SelectionBoundsMinMaxDegrees;
                return new double[] { TilesProjection.lon2x_m(d[0]), TilesProjection.lat2y_m(d[1]), TilesProjection.lon2x_m(d[2]), TilesProjection.lat2y_m(d[3]) };
            }
        }
        /// <summary>
        ///     minx,miny,maxx,maxy
        /// </summary>
        public double[] SelectionBoundsMinMaxDegrees
        {
            get
            {
                return new double[] { _selLayer[0][0].Points[0].X, _selLayer[0][0].Points[2].Y, _selLayer[0][0].Points[2].X, _selLayer[0][0].Points[0].Y };
            }
        }
        /// <summary>
        ///     minx,miny,width,height
        /// </summary>
        public Rectangle SelectionBoundsRectPixels
        {

            get
            {
                int[] b = SelectionBoundsMinMaxPixels;
                return new Rectangle(b[0], b[1], b[2] - b[0], b[3] - b[1]);
            }
        }
        /// <summary>
        ///     minx,miny,width,height
        /// </summary>
        public RectangleF SelectionBoundsRectMeters
        {

            get
            {
                double[] b = SelectionBoundsMinMaxDegrees;
                b[0] = TilesProjection.lon2x_m(b[0]);
                b[1] = TilesProjection.lat2y_m(b[1]);
                b[2] = TilesProjection.lon2x_m(b[2]);
                b[3] = TilesProjection.lat2y_m(b[3]);
                return new RectangleF((float)b[0], (float)b[1], (float)b[2] - (float)b[0], (float)b[3] - (float)b[1]);
            }
        }
        /// <summary>
        ///     minx,miny,width,height
        /// </summary>
        public RectangleF SelectionBoundsRectDegrees
        {

            get
            {
                double[] b = SelectionBoundsMinMaxDegrees;
                return new RectangleF((float)b[0], (float)b[1], (float)b[2] - (float)b[0], (float)b[3] - (float)b[1]);
            }
        }
        /// <summary>
        ///     left-top,bottom-right
        /// </summary>
        public Point[] SelectionBoundsAreaPixels
        {
            get
            {
                int[] b = SelectionBoundsMinMaxPixels;
                return new Point[] { new Point(b[0], b[3]), new Point(b[2], b[1]) };
            }
        }
        /// <summary>
        ///     left-top,bottom-right
        /// </summary>
        public PointF[] SelectionBoundsAreaMeters
        {
            get
            {
                double[] b = SelectionBoundsMinMaxDegrees;
                b[0] = TilesProjection.lon2x_m(b[0]);
                b[1] = TilesProjection.lat2y_m(b[1]);
                b[2] = TilesProjection.lon2x_m(b[2]);
                b[3] = TilesProjection.lat2y_m(b[3]);
                return new PointF[] { new PointF((float)b[0], (float)b[3]), new PointF((float)b[2], (float)b[1]) };
            }
        }
        /// <summary>
        ///     left-top,bottom-right
        /// </summary>
        public PointF[] SelectionBoundsAreaDegrees
        {
            get
            {
                double[] b = SelectionBoundsMinMaxDegrees;
                return new PointF[] { new PointF((float)b[0], (float)b[3]), new PointF((float)b[2], (float)b[1]) };
            }
        }
        #endregion

        #region MAP BOUNDS
        /// <summary>
        ///     minx,miny,maxx,maxy
        /// </summary>
        public int[] MapBoundsMinMaxPixels
        {
            get
            {
                Point c = CenterPixels;
                return new int[] { c.X - mapImage.Width / 2 + 128, c.Y - mapImage.Height / 2 + 128, c.X + mapImage.Width / 2 - 128, c.Y + mapImage.Height / 2 - 128 };
            }
        }
        /// <summary>
        ///     minx,miny,maxx,maxy
        /// </summary>
        public double[] MapBoundsMinMaxMeters
        {
            get
            {
                double[] d = MapBoundsMinMaxDegrees;
                return new double[] { TilesProjection.lon2x_m(d[0]), TilesProjection.lat2y_m(d[1]), TilesProjection.lon2x_m(d[2]), TilesProjection.lat2y_m(d[3]) };
            }
        }
        /// <summary>
        ///     minx,miny,maxx,maxy
        /// </summary>
        public double[] MapBoundsMinMaxDegrees
        {
            get
            {
                int[] b = MapBoundsMinMaxPixels;
                double[] llmin = TilesProjection.fromPixelToLL(new double[] { b[0], b[3] }, ZoomID_);
                double[] llmax = TilesProjection.fromPixelToLL(new double[] { b[2], b[1] }, ZoomID_);
                return new double[] { llmin[0], llmin[1], llmax[0], llmax[1] };
            }
        }
        /// <summary>
        ///     minx,miny,width,height
        /// </summary>
        public Rectangle MapBoundsRectPixels
        {

            get
            {
                int[] b = MapBoundsMinMaxPixels;
                return new Rectangle(b[0], b[1], b[2] - b[0], b[3] - b[1]);
            }
        }
        /// <summary>
        ///     minx,miny,width,height
        /// </summary>
        public RectangleF MapBoundsRectMeters
        {

            get
            {
                double[] b = MapBoundsMinMaxDegrees;
                b[0] = TilesProjection.lon2x_m(b[0]);
                b[1] = TilesProjection.lat2y_m(b[1]);
                b[2] = TilesProjection.lon2x_m(b[2]);
                b[3] = TilesProjection.lat2y_m(b[3]);
                return new RectangleF((float)b[0], (float)b[1], (float)b[2] - (float)b[0], (float)b[3] - (float)b[1]);
            }
        }
        /// <summary>
        ///     minx,miny,width,height
        /// </summary>
        public RectangleF MapBoundsRectDegrees
        {

            get
            {
                double[] b = MapBoundsMinMaxDegrees;
                return new RectangleF((float)b[0], (float)b[1], (float)b[2] - (float)b[0], (float)b[3] - (float)b[1]);
            }
        }
        /// <summary>
        ///     left-top,bottom-right
        /// </summary>
        public Point[] MapBoundsAreaPixels
        {
            get
            {
                int[] b = MapBoundsMinMaxPixels;
                return new Point[] { new Point(b[0],b[3]),new Point(b[2],b[1]) };
            }
        }
        /// <summary>
        ///     left-top,bottom-right
        /// </summary>
        public PointF[] MapBoundsAreaMeters
        {
            get
            {
                double[] b = MapBoundsMinMaxDegrees;
                b[0] = TilesProjection.lon2x_m(b[0]);
                b[1] = TilesProjection.lat2y_m(b[1]);
                b[2] = TilesProjection.lon2x_m(b[2]);
                b[3] = TilesProjection.lat2y_m(b[3]);
                return new PointF[] { new PointF((float)b[0], (float)b[3]), new PointF((float)b[2], (float)b[1]) };
            }
        }
        /// <summary>
        ///     left-top,bottom-right
        /// </summary>
        public PointF[] MapBoundsAreaDegrees
        {
            get
            {
                double[] b = MapBoundsMinMaxDegrees;
                return new PointF[] { new PointF((float)b[0], (float)b[3]), new PointF((float)b[2], (float)b[1]) };
            }
        }
        /// <summary>
        ///     minx,miny,maxx,maxy
        /// </summary>
        public int[] MapBoundsMinMaxOversizePixels
        {
            get
            {
                Point c = CenterPixels;
                return new int[] { c.X - mapImage.Width / 2, c.Y - mapImage.Height / 2, c.X + mapImage.Width / 2, c.Y + mapImage.Height / 2 };
            }
        }
        /// <summary>
        ///     minx,miny,maxx,maxy
        /// </summary>
        public double[] MapBoundsMinMaxOversizeMeters
        {
            get
            {
                double[] d = MapBoundsMinMaxOversizeDegrees;
                return new double[] { TilesProjection.lon2x_m(d[0]), TilesProjection.lat2y_m(d[1]), TilesProjection.lon2x_m(d[2]), TilesProjection.lat2y_m(d[3]) };
            }
        }
        /// <summary>
        ///     minx,miny,maxx,maxy
        /// </summary>
        public double[] MapBoundsMinMaxOversizeDegrees
        {
            get
            {
                int[] b = MapBoundsMinMaxOversizePixels;
                double[] llmin = TilesProjection.fromPixelToLL(new double[] { b[0], b[3] }, ZoomID_);
                double[] llmax = TilesProjection.fromPixelToLL(new double[] { b[2], b[1] }, ZoomID_);
                return new double[] { llmin[0], llmin[1], llmax[0], llmax[1] };
            }
        }
        /// <summary>
        ///     minx,miny,width,height
        /// </summary>
        public Rectangle MapBoundsRectOversizePixels
        {

            get
            {
                int[] b = MapBoundsMinMaxOversizePixels;
                return new Rectangle(b[0], b[1], b[2] - b[0], b[3] - b[1]);
            }
        }
        /// <summary>
        ///     minx,miny,width,height
        /// </summary>
        public RectangleF MapBoundsRectOversizeMeters
        {

            get
            {
                double[] b = MapBoundsMinMaxOversizeDegrees;
                b[0] = TilesProjection.lon2x_m(b[0]);
                b[1] = TilesProjection.lat2y_m(b[1]);
                b[2] = TilesProjection.lon2x_m(b[2]);
                b[3] = TilesProjection.lat2y_m(b[3]);
                return new RectangleF((float)b[0], (float)b[1], (float)b[2] - (float)b[0], (float)b[3] - (float)b[1]);
            }
        }
        /// <summary>
        ///     minx,miny,width,height
        /// </summary>
        public RectangleF MapBoundsRectOversizeDegrees
        {
            get
            {
                double[] b = MapBoundsMinMaxOversizeDegrees;
                return new RectangleF((float)b[0], (float)b[1], (float)b[2] - (float)b[0], (float)b[3] - (float)b[1]);
            }
        }
        /// <summary>
        ///     left-top,bottom-right
        /// </summary>
        public Point[] MapBoundsAreaOversizePixels
        {
            get
            {
                int[] b = MapBoundsMinMaxOversizePixels;
                return new Point[] { new Point(b[0], b[3]), new Point(b[2], b[1]) };
            }
        }
        /// <summary>
        ///     left-top,bottom-right
        /// </summary>
        public PointF[] MapBoundsAreaOversizeMeters
        {
            get
            {
                double[] b = MapBoundsMinMaxOversizeDegrees;
                b[0] = TilesProjection.lon2x_m(b[0]);
                b[1] = TilesProjection.lat2y_m(b[1]);
                b[2] = TilesProjection.lon2x_m(b[2]);
                b[3] = TilesProjection.lat2y_m(b[3]);
                return new PointF[] { new PointF((float)b[0], (float)b[3]), new PointF((float)b[2], (float)b[1]) };
            }
        }  
        /// <summary>
        ///     left-top,bottom-right
        /// </summary>
        public PointF[] MapBoundsAreaOversizeDegrees
        {
            get
            {
                double[] b = MapBoundsMinMaxOversizeDegrees;
                return new PointF[] { new PointF((float)b[0], (float)b[3]), new PointF((float)b[2], (float)b[1]) };
            }
        }
        #endregion                

        #region Converstions & Operations
        public Point DegreesToPixels(PointF degrees)
        {
            double[] pxl = TilesProjection.fromLLtoPixel(new double[] { degrees.X, degrees.Y }, ZoomID_);
            return new Point((int)pxl[0], (int)pxl[1]);
        }
        public PointF PixelsToDegrees(Point pixels)
        {
            double[] deg = TilesProjection.fromPixelToLL(new double[] { pixels.X, pixels.Y }, ZoomID_);
            return new PointF((float)deg[0], (float)deg[1]);
        }
        public static PointF DegreesToMeters(PointF degrees)
        {
            return TilesProjection.fromLLtoMeter(degrees);
        }
        public static PointF MetersToDegrees(PointF meters)
        {
            return new PointF((float)TilesProjection.x2lon_m(meters.X), (float)TilesProjection.y2lat_m(meters.Y));
        }
        public Point MetersToPixels(PointF meters)
        {
            return DegreesToPixels(MetersToDegrees(meters));
        }
        public PointF PixelsToMeters(Point pixels)
        {
            return DegreesToMeters(PixelsToDegrees(pixels));
        }
        public Point PixelsToScreen(Point pixels)
        {
            Point cp = CenterPixels;
            return new Point(pixels.X - cp.X + OversizeWidth / 2 ,pixels.Y - cp.Y + OversizeHeight / 2);
        }
        public Size MetersToPixels(SizeF meters)
        {
            return new Size((int)(meters.Width / ZoomResolution), (int)(meters.Height / ZoomResolution));
        }
        public SizeF PixelsToMeters(Size meters)
        {
            return new SizeF((float)(meters.Width * ZoomResolution), (float)(meters.Height * ZoomResolution));
        }
        public static Size MetersToPixels(SizeF meters, int zoomID)
        {
            double Resolution = 156543.0339 / Math.Pow(2, zoomID);
            return new Size((int)(meters.Width / Resolution), (int)(meters.Height / Resolution));
        }
        /// <summary>
        ///     Проверка на пересечение прямоугольников
        /// </summary>
        /// <param name="r1">Прямоугольник</param>
        /// <param name="r2">Прямоугольник</param>
        /// <returns></returns>
        private bool InRectangle(Rectangle r1, Rectangle r2)
        {
            return (r1.Left < r2.Right) && (r2.Left < r1.Right) && (r1.Top < r2.Bottom) && (r2.Top < r1.Bottom);
        }
        /// <summary>
        ///     Проверка на пересечение прямоугольников
        /// </summary>
        /// <param name="r1">Прямоугольник</param>
        /// <param name="r2">Прямоугольник</param>
        /// <returns></returns>
        private bool InRectangle(RectangleF r1, RectangleF r2)
        {
            return (r1.Left < r2.Right) && (r2.Left < r1.Right) && (r1.Top < r2.Bottom) && (r2.Top < r1.Bottom);
        }
        private static uint GetLengthMetersC(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double D2R = Math.PI / 180;
            if (radians) D2R = 1;
            double dDistance = Double.MinValue;
            double dLat1InRad = StartLat * D2R;
            double dLong1InRad = StartLong * D2R;
            double dLat2InRad = EndLat * D2R;
            double dLong2InRad = EndLong * D2R;

            double dLongitude = dLong2InRad - dLong1InRad;
            double dLatitude = dLat2InRad - dLat1InRad;

            // Intermediate result a.
            double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                       Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                       Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).
            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

            const double kEarthRadiusKms = 6378137.0000;
            dDistance = kEarthRadiusKms * c;

            return (uint)Math.Round(dDistance);
        }

        #endregion

        #region Zoom Levels Bar & Scale Bar

        private int zoomLevels_ZoomID = -1;
        private void zoomLevels_Paint(object sender, PaintEventArgs e)
        {
            if (zoomLevels.Visible)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(190, Color.Orange)), new Rectangle(5, 27 + (int)((18 - ZoomID_) * 11), 16, 8));
                e.Graphics.DrawString(ZoomID_.ToString(), new Font("Calibri", 8), new SolidBrush(Color.White), new PointF(ZoomID_ < 10 ? 8 : 5, 25 + (int)((18 - ZoomID_) * 11)));
            };

        }
        private void zoomLevels_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = DefaultMapCursor_;
        }
        private void zoomLevels_MouseLeave(object sender, EventArgs e)
        {
            ChangeKeyedMapCursor();
        }        
        private void zoomLevels_MouseMove(object sender, MouseEventArgs e)
        {
            zoomLevels_ZoomID = -1;
            this.Cursor = Cursors.Default;
            if (e.X > 4 && e.X < 22)
            {
                for (int z = 0; z < 18; z++)
                    if ((e.Y > (26 + z * 11)) && (e.Y < (36 + z * 11)))
                    {
                        zoomLevels_ZoomID = 18 - z;
                        this.Cursor = Cursors.Hand;
                    };
                if (e.Y < 20 && e.Y > 2)
                {
                    this.Cursor = Cursors.Hand;
                    zoomLevels_ZoomID = -2;
                };
                if (e.Y < zoomLevels.Height && e.Y > 220)
                {
                    this.Cursor = Cursors.Hand;
                    zoomLevels_ZoomID = -3;
                };
            };
        }
        private void zoomLevels_Click(object sender, EventArgs e)
        {
            if (zoomLevels_ZoomID == -1) return;

            if (zoomLevels_ZoomID == -2) ZoomID++;
            if (zoomLevels_ZoomID == -3) ZoomID--;
            if (zoomLevels_ZoomID >= 0) ZoomID = (byte)zoomLevels_ZoomID;
        }
        private void DrawScaleBar()
        {
            double tmp_x1 = ZoomLevels_[ZoomID_] * 90;
            double[] lllat = TilesProjection.fromPixelToLL(new double[] { (double)CenterPixelsX, (double)CenterPixelsY }, ZoomID_);
            double xmeter = TilesProjection.lon2x_m(lllat[0]);
            double lons = TilesProjection.x2lon_m((xmeter + 0) * ZoomLevels_[ZoomID_]);
            double lone = TilesProjection.x2lon_m((xmeter + 1000) * ZoomLevels_[ZoomID_]);
            tmp_x1 = GetLengthMetersC(lons, lllat[1], lone, lllat[1], false) / 1000 * 90;
            // MessageBox.Show(tmp_x1.ToString() + "      " + (mobj.zoom * 90).ToString() + "      " + p1.X + " " + p1.Y + " " + p2.X + " " + p2.Y + "    ==" + (mobj.center_x * mobj.zoom).ToString() + " " + ((mobj.center_x + 1000) * mobj.zoom).ToString());
            double tmp_new2 = 0;
            double[] tmp_x2 = new double[24] { 10, 20, 50, 100, 250, 400, 500, 1000, 2500, 4000, 5000, 10000, 25000, 40000, 50000, 100000, 250000, 400000, 500000, 1000000, 2500000, 4000000, 5000000, 10000000 };
            double[] tmp_x3 = new double[24] { 1, 2, 2, 4, 5, 4, 5, 4, 5, 4, 5, 4, 5, 4, 5, 4, 5, 5, 5, 4, 5, 4, 5, 4 };
            double tmp_zoom = 10000000;
            double tmp_new = 0;
            for (int i = 0; i < tmp_x2.Length; i++)
                if (Math.Abs(tmp_x1 - tmp_x2[i]) < tmp_zoom)
                {
                    tmp_zoom = Math.Abs(tmp_x1 - tmp_x2[i]);
                    tmp_new = tmp_x2[i];
                    tmp_new2 = tmp_x3[i];
                };
            tmp_x1 = Math.Round(tmp_new / ZoomLevels_[ZoomID_] - 1);
            Pen p = new Pen(Color.FromArgb(150, 0, 0, 0));
            scaleImage.Image = new Bitmap((int)tmp_x1 + 2, 9);
            Graphics g = Graphics.FromImage(scaleImage.Image);
            g.Clear(Color.Transparent);
            g.DrawRectangle(p, 0, 0, (int)tmp_x1 + 1, 8);
            g.FillRectangle(new SolidBrush(Color.White), new Rectangle(1, 0, (int)tmp_x1, 8));
            string ztext = (tmp_new < 1000 ? (((int)tmp_new).ToString() + " м") : ((int)(tmp_new / 1000)).ToString() + " км");
            SizeF sz = g.MeasureString(ztext, new Font("Calibri", 8));
            g.DrawString(ztext, new Font("Calibri", 8), new SolidBrush(Color.FromArgb(250, 0, 0, 0)), (float)(tmp_x1 / 2 - sz.Width / 2), -3);
            g.Dispose();
        }
        #endregion                

        public static Image GetEmptyTile(Color bgColor)
        {
            string t = "NOT FOUND\r\nOR\r\nERROR";
            Bitmap bmp = new Bitmap(256, 256);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(bgColor);
            Font f = new Font("Arial", 10);
            Brush b = new SolidBrush(Color.Silver);
            g.DrawString(t, f, b, new PointF(40,40));
            g.Dispose();
            return (Image)bmp;
        }

        /// <summary>
        ///     Загрузка картинки
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>Картинка</returns>
        public static Image GetImageFromURL(string url, int WebRequestTimeout, Color notFoundTileColor)
        {
            Uri Uri_url = new Uri(url);

            if (Uri_url.IsFile)
            {
                url = url.Replace(@"file:", @"\\");
                url = url.Replace("/", @"\");
                if (System.IO.File.Exists(url))
                    return new Bitmap(url);
                else
                    return notFoundTileColor == Color.Transparent ? null : GetEmptyTile(notFoundTileColor);
            };

            System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(Uri_url);
            wr.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:52.0) Gecko/20100101 Firefox/52.0";
            wr.Timeout = WebRequestTimeout;
            //wr.Referer = "http://127.0.0.1/";
            try
            {
                System.Net.WebResponse res = wr.GetResponse();
                System.IO.Stream sr = res.GetResponseStream();
                Image im = Image.FromStream(res.GetResponseStream());
                return im;
            }
            catch (System.Net.WebException err)
            {
                return notFoundTileColor == Color.Transparent ? null : GetEmptyTile(notFoundTileColor);
                //if (err.Status != System.Net.WebExceptionStatus.ProtocolError)
                //    throw new System.Net.WebException(err.Message, err);
            };
        }

        /// <summary>
        ///     Загрузка картинки
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>Картинка</returns>
        public static Image GetImageFromURL(string url, int WebRequestTimeout, Color notFoundTileColor, out bool isEmpty)
        {
            isEmpty = true;

            Uri Uri_url = new Uri(url);

            if (Uri_url.IsFile)
            {
                url = url.Replace(@"file:", @"\\");
                url = url.Replace("/", @"\");
                if (System.IO.File.Exists(url))
                {
                    isEmpty = false;
                    return new Bitmap(url);
                }
                else
                    return notFoundTileColor == Color.Transparent ? null : GetEmptyTile(notFoundTileColor);
            };

            System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(Uri_url);
            wr.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:52.0) Gecko/20100101 Firefox/52.0";
            wr.Timeout = WebRequestTimeout;
            //wr.Referer = "http://127.0.0.1/";
            try
            {
                System.Net.WebResponse res = wr.GetResponse();
                System.IO.Stream sr = res.GetResponseStream();
                Image im = Image.FromStream(res.GetResponseStream());
                isEmpty = false;
                return im;
            }
            catch (System.Net.WebException err)
            {
                return notFoundTileColor == Color.Transparent ? null : GetEmptyTile(notFoundTileColor);
                //if (err.Status != System.Net.WebExceptionStatus.ProtocolError)
                //    throw new System.Net.WebException(err.Message, err);
            };
        }  

        private void mapImage_MouseMove(object sender, MouseEventArgs e)
        {
            this.OnMouseMove(e);
        }

        private void mapImage_MouseClick(object sender, MouseEventArgs e)
        {
            if (MouseOverMap_)
                this.OnMouseClick(e);
        }

        private void mapImage_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (MouseOverMap_)
                this.OnMouseDoubleClick(e);

            //if ((!KeyDownAlt) && (!KeyDownControl) && (KeyDownShift) && (SelectionBoxIsVisible))
            //{
            //    Geometry.TPolygon p = new Geometry.TPolygon(_selLayer[0][0].Points);
            //    Geometry.TPoint d = new Geometry.TPoint(MousePositionDegrees);
            //    if (Geometry.PointInPolygon(d, p))
            //        ClearSelectionBox();
            //};

            if (_ShowInfoOnDblClick && (!KeyDownAlt) && (!KeyDownControl) && (!KeyDownShift))
            {
                if (!SelectionBoxIsVisible)
                    ShowAdditInfo();
                else
                {
                    Geometry.TPolygon p = new Geometry.TPolygon(_selLayer[0][0].Points);
                    Geometry.TPoint d = new Geometry.TPoint(MousePositionDegrees);
                    if(Geometry.PointInPolygon(d, p))
                        ShowSelectionInfo();
                    else
                        ShowAdditInfo();
                };
            };
        }

        private void mapImage_MouseHover(object sender, EventArgs e)
        {
            this.OnMouseHover(e);
        }

        private void mapImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (MouseOverMap_)
                this.OnMouseUp(e);
        }

        private void mapImage_Click(object sender, EventArgs e)
        {
            if (MouseOverMap_)
                this.OnClick(e);
        }

        private void selMapType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ImageSourceService = (MapServices)selMapType.Items[selMapType.SelectedIndex];
            }
            catch { };
        }
        
        private void btnCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            double lat = this.CenterDegreesLat;
            double lon = this.CenterDegreesLon;
            if (InputLatLon("Map Center", "Set Map Center to:", ref lat, ref lon) == DialogResult.OK)
                this.CenterDegrees = new PointF((float)lon, (float)lat);            
        }

        private void reloadTilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            this.ReloadMap();            
        }

        private void mttShift_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            this.MapTool = MapTools.mtShift;
        }

        private void mttZoomIn_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            this.MapTool = MapTools.mtZoomIn;
        }

        private void mttZoomOut_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            this.MapTool = MapTools.mtZoomOut;
        }

        private void getCursorLatLonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            PointF pf = MouseDownDegrees;
            PointF cf = CenterDegrees;
            float dx = cf.X - pf.X;
            float dy = cf.Y - pf.Y;
            double[] ll = new double[] {pf.X, pf.Y };
            if (InputLatLon("Map Click", "Map clicked at position:", ref ll[1], ref ll[0]) == DialogResult.OK)
                CenterDegrees = new PointF((float)(ll[0] + dx), (float)(ll[1] + dy));
        }

        private void showCross_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            this.ShowCross = !this.ShowCross;
        }

        private void ShowZoomBtn_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            this.ShowScale = !this.ShowScale;
        }

        private void showZoomsBtn_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            this.ShowZooms = !this.ShowZooms;
        }

        private void ShowTileBorder_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            this.DrawTilesBorder = !this.DrawTilesBorder;
            this.ReloadMap();
        }

        private void showXYZBtn_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            this.DrawTilesXYZ = !this.DrawTilesXYZ;
            this.ReloadMap();
        }

        private void UseCacheBtn_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            this.UseDiskCache = !this.UseDiskCache;
        }

        private string _additClickInfoText = "";
        public string AdditionalClickInfoText { get { return _additClickInfoText; } set { _additClickInfoText = value; } }
        private string _additSelectionInfoText = "";
        public string AdditionalSelectionInfoText { get { return _additSelectionInfoText; } set { _additSelectionInfoText = value; } }

        private void showAddit_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            ShowAdditInfo();
        }        

        private void defMenu_Opening(object sender, CancelEventArgs e)
        {
           if (DefaultMenuOpening != null) DefaultMenuOpening(sender, e);
        }

        public void AddItemToDefaultMenu(ToolStripItem item)
        {
            if (item != null)
            {
                DOP.DropDownItems.Add(item);
                DOP.Visible = true;
            };
        }

        private void SelShiftArea(object sender, EventArgs e)
        {                        
            //PointF[] sara = SelectionBoundsAreaDegrees;

            Point pCentSelScreen = this.PointToClient(mapImage.PointToScreen(new Point(Selabel.Left + Selabel.Width / 2, Selabel.Top + Selabel.Height / 2)));
            pCentSelScreen.X = pCentSelScreen.X - Width / 2;
            pCentSelScreen.Y = pCentSelScreen.Y - Height / 2;

            Point mc = CenterPixels;
            Point c = new Point(mc.X + pCentSelScreen.X, mc.Y + pCentSelScreen.Y);


            int[] b = new int[] { c.X - Selabel.Width / 2, c.Y - Selabel.Height / 2, c.X + Selabel.Width / 2, c.Y + Selabel.Height / 2 };
            double[] llmin = TilesProjection.fromPixelToLL(new double[] { b[0], b[3] }, ZoomID_);
            double[] llmax = TilesProjection.fromPixelToLL(new double[] { b[2], b[1] }, ZoomID_);
            PointF[] sara = new PointF[] { new PointF((float)llmin[0], (float)llmax[1]), new PointF((float)llmax[0], (float)llmin[1]) };

            _selLayer[0][0].Points =  new PointF[] { new PointF(sara[0].X, sara[0].Y), new PointF(sara[1].X, sara[0].Y), new PointF(sara[1].X, sara[1].Y), new PointF(sara[0].X, sara[1].Y) };
            _selLayer[0][0].Visible = true;
            HSB.Visible = true;
            hideSelBox.Visible = true;
            selBoxInfo.Visible = true;
            save2Shp.Visible = true;
            Zoom2Sel.Visible = true;
            this.DrawOnMapData();

            if (SelectingMapArea != null) SelectingMapArea(sender, e);            
        }

        [Browsable(true)]
        public EventHandler SelectingMapArea;
        [Browsable(true)]
        public EventHandler PreparingAdditionalPointInfo;
        [Browsable(true)]
        public EventHandler PreparingAdditionalSelectionInfo;
        [Browsable(true)]
        public System.ComponentModel.CancelEventHandler DefaultMenuOpening;    
    
        [Browsable(true)]
        public bool ShowInfoOnDblClick { get { return _ShowInfoOnDblClick; } set { _ShowInfoOnDblClick = value; } }
        private bool _ShowInfoOnDblClick = false;

        public static DialogResult InputLatLon(string title, string promptText, ref double Lat, ref double Lon)
        {
            Form form = new Form();
            Label label = new Label();
            Label labelat = new Label();
            Label labelon = new Label();
            TextBox textBoxLat = new TextBox();
            TextBox textBoxLon = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            labelat.Text = "Lat:";
            labelon.Text = "Lon:";
            textBoxLat.Text = Lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            textBoxLon.Text = Lon.ToString(System.Globalization.CultureInfo.InvariantCulture);
            textBoxLat.Validating += new CancelEventHandler(textBoxLat_Validating);
            textBoxLon.Validating += new CancelEventHandler(textBoxLon_Validating);


            buttonOk.Text = "OK";
            buttonCancel.Text = "Отмена";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(10, 10, 372, 13);
            labelat.SetBounds(10, 33, 40, 13);
            labelon.SetBounds(10, 53, 40, 13);
            textBoxLat.SetBounds(52, 30, 332, 20);
            textBoxLon.SetBounds(52, 52, 332, 20);
            buttonOk.SetBounds(228, 82, 75, 23);
            buttonCancel.SetBounds(309, 82, 75, 23);

            label.AutoSize = true;
            textBoxLat.BorderStyle = BorderStyle.FixedSingle;
            textBoxLat.Anchor = textBoxLat.Anchor | AnchorStyles.Right;
            textBoxLon.BorderStyle = BorderStyle.FixedSingle;
            textBoxLon.Anchor = textBoxLat.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 117);
            form.Controls.AddRange(new Control[] { label, labelat, labelon, textBoxLat, textBoxLon, buttonOk, buttonCancel });
            form.ClientSize = new Size(260, form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            double.TryParse(textBoxLat.Text.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out Lat);
            double.TryParse(textBoxLon.Text.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out Lon);
            return dialogResult;
        }

        private static void textBoxLon_Validating(object sender, CancelEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            tb.Text = LatLonParser.ToLon(tb.Text).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private static void textBoxLat_Validating(object sender, CancelEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            tb.Text = LatLonParser.ToLat(tb.Text).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public static DialogResult InputXYZ(string title, string promptText, ref int X, ref int Y, ref int Z)
        {
            Form form = new Form();
            Label label = new Label();
            Label labelX = new Label();
            Label labelY = new Label();
            Label labelZ = new Label();
            TextBox textBoxX = new TextBox();
            TextBox textBoxY = new TextBox();
            TextBox textBoxZ = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            labelX.Text = "Tile X:";
            labelY.Text = "Tile Y:";
            labelZ.Text = "Zoom:";
            textBoxX.Text = X.ToString();
            textBoxY.Text = Y.ToString();
            textBoxZ.Text = Z.ToString();


            buttonOk.Text = "OK";
            buttonCancel.Text = "Отмена";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(10, 10, 372, 13);
            labelX.SetBounds(10, 33, 40, 13);
            labelY.SetBounds(10, 53, 40, 13);
            labelZ.SetBounds(10, 73, 40, 13);
            textBoxX.SetBounds(52, 30, 332, 20);
            textBoxY.SetBounds(52, 52, 332, 20);
            textBoxZ.SetBounds(52, 74, 332, 20);
            buttonOk.SetBounds(228, 106, 75, 23);
            buttonCancel.SetBounds(309, 106, 75, 23);

            label.AutoSize = true;
            textBoxX.BorderStyle = BorderStyle.FixedSingle;
            textBoxX.Anchor = textBoxX.Anchor | AnchorStyles.Right;
            textBoxY.BorderStyle = BorderStyle.FixedSingle;
            textBoxY.Anchor = textBoxX.Anchor | AnchorStyles.Right;
            textBoxZ.BorderStyle = BorderStyle.FixedSingle;
            textBoxZ.Anchor = textBoxX.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 139);
            form.Controls.AddRange(new Control[] { label, labelX, labelY, labelZ, textBoxX, textBoxY, textBoxZ, buttonOk, buttonCancel });
            form.ClientSize = new Size(260, form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            int.TryParse(textBoxX.Text.Trim(), out X);
            int.TryParse(textBoxY.Text.Trim(), out Y);
            int.TryParse(textBoxZ.Text.Trim(), out Z);
            return dialogResult;
        }

        public static DialogResult ShowAdditionalInfoDialog(string title, string prompt, string text)
        {           
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();

            form.Text = title;
            label.Text = prompt;
            label.Font = new Font(label.Font, FontStyle.Bold);
            textBox.Multiline = true;
            textBox.Text = text;
            textBox.ScrollBars = ScrollBars.Both;
            textBox.ReadOnly = true;
            //textBox.BackColor = Color.White;
            textBox.BorderStyle = BorderStyle.None;

            buttonOk.Text = "OK";
            buttonOk.DialogResult = DialogResult.OK;

            label.SetBounds(8, 10, 372, 13);
            textBox.SetBounds(12, 30, 478, 284);
            buttonOk.SetBounds(218, 325, 75, 24);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(500, 360);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.ActiveControl = buttonOk;

            form.DialogResult = DialogResult.Cancel;
            DialogResult dialogResult = form.ShowDialog();
            return dialogResult;
        }

        private void Selabel_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, Selabel.DisplayRectangle, Color.White, ButtonBorderStyle.Solid);
        }

        private void hideSelBox_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            ClearSelectionBox();
        }

        private void selBoxInfo_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            ShowSelectionInfo();
        }

        private void ShowAdditInfo()
        {
            if (PreparingAdditionalPointInfo != null) PreparingAdditionalPointInfo(this, new EventArgs());

            PointF pcenter = this.CenterDegrees;
            PointF pclick = this.MouseDownDegrees;
            Point xyclick = this.MouseDownPixels; xyclick.X /= 256; xyclick.Y /= 256;

            string txt = "";
            txt += String.Format("Центр карты, широта: {0}\r\n", pcenter.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += String.Format("Центр карты, долгота: {0}\r\n", pcenter.X.ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += "\r\n";
            txt += String.Format("Точка клика, широта: {0}\r\n", pclick.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += String.Format("Точка клика, долгота: {0}\r\n", pclick.X.ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += "\r\n";
            txt += String.Format("Точка клика, X тайла: {0}\r\n", xyclick.X.ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += String.Format("Точка клика, Y тайла: {0}\r\n", xyclick.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += String.Format("Точка клика, Z тайла: {0}\r\n", this.ZoomID.ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += "\r\n";
            //////
            PointF pf = tile2location(xyclick.X, xyclick.Y, ZoomID_);
            txt += String.Format("Тайл клика, Top-Left: {0} {1}\r\n", new object[] { pf.Y.ToString(System.Globalization.CultureInfo.InvariantCulture), pf.X.ToString(System.Globalization.CultureInfo.InvariantCulture) });
            pf = tile2location(xyclick.X + 0.5, xyclick.Y + 0.5, ZoomID_);
            txt += String.Format("Тайл клика, Центр: {0} {1}\r\n", new object[] { pf.Y.ToString(System.Globalization.CultureInfo.InvariantCulture), pf.X.ToString(System.Globalization.CultureInfo.InvariantCulture) });
            //////
            txt += "\r\n";
            txt += String.Format("Точка клика, источник: {0}\r\n", GetTileUrl(xyclick.X, xyclick.Y, this.ZoomID));
            txt += "\r\n";

            if (PreparingAdditionalPointInfo != null) 
                if (!String.IsNullOrEmpty(_additClickInfoText)) 
                    txt += _additClickInfoText;

            ShowAdditionalInfoDialog("Информация", "Вспомогательная информация по точке:", txt);
        }

        private static System.Drawing.PointF tile2location(double x, double y, int zoom)
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


        private void ShowSelectionInfo()
        {
            if (PreparingAdditionalSelectionInfo != null) PreparingAdditionalSelectionInfo(this, new EventArgs());

            PointF pcenter = this.CenterDegrees;
            PointF[] area = SelectionBoundsAreaDegrees;
            
            string txt = "";
            txt += String.Format("Центр карты, широта: {0}\r\n", pcenter.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += String.Format("Центр карты, долгота: {0}\r\n", pcenter.X.ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += "\r\n";
            txt += String.Format("Зона выделения, центр: {0} {1}\r\n", ((area[0].Y+area[1].Y)/2).ToString(System.Globalization.CultureInfo.InvariantCulture), ((area[0].X+area[1].X)/2).ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += "\r\n";
            txt += String.Format("Зона выделения, лево: {0}\r\n", area[0].X.ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += String.Format("Зона выделения, верх: {0}\r\n", area[0].Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += String.Format("Зона выделения, право: {0}\r\n", area[1].X.ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += String.Format("Зона выделения, низ: {0}\r\n", area[1].Y.ToString(System.Globalization.CultureInfo.InvariantCulture));                        
            txt += "\r\n";
            txt += String.Format("Зона выделения, лево-верх: {0} {1}\r\n", area[0].Y.ToString(System.Globalization.CultureInfo.InvariantCulture), area[0].X.ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += String.Format("Зона выделения, право-низ: {0} {1}\r\n", area[1].Y.ToString(System.Globalization.CultureInfo.InvariantCulture), area[1].X.ToString(System.Globalization.CultureInfo.InvariantCulture));
            txt += "\r\n";                                    

            Int64 ttlx = 0;
            Int64 ttly = 0;
            Int64 ttla = 0;
            for (int i = MapMinZoom_; i <= MapMaxZoom_; i++)
            {
                Point ts = location2tile(area[0].Y, area[0].X, i);
                Point te = location2tile(area[1].Y, area[1].X, i);
                int tx = (int)(Math.Abs(te.X - ts.X) + 1);
                int ty = (int)(Math.Abs(te.Y - ts.Y) + 1);
                txt += String.Format("Зона выделения, Зум {0}, Тайлов по: X - {1}, Y - {2}, Всего - {3}\r\n", new object[] { i, tx, ty, tx * ty });
                ttlx += tx;
                ttly += ty;
                ttla += tx * ty;
            };
            txt += String.Format("Зона выделения, Все зумы, Тайлов по: X - {1}, Y - {2}, Всего - {3}\r\n", new object[] { 0, ttlx, ttly, ttla });
            txt += "\r\n";

            double WiMin = GetLengthMetersC(area[0].X, area[0].Y, area[1].X, area[0].Y, false) / 1000.0;
            double WiMax = GetLengthMetersC(area[0].X, area[1].Y, area[1].X, area[1].Y, false) / 1000.0;
            double He = GetLengthMetersC(area[0].X, area[0].Y, area[0].X, area[1].Y, false) / 1000.0;
            double Pe = WiMin + WiMax + 2 * He;
            double Sq = (WiMin + WiMin) * He / 2;
            txt += String.Format("Зона выделения, Ширина мин: {0} км\r\n", WiMin.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture));
            txt += String.Format("Зона выделения, Ширина макс: {0} км\r\n", WiMax.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture));
            txt += String.Format("Зона выделения, Высота: {0} км\r\n", He.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture));
            txt += String.Format("Зона выделения, Периметр: {0} км\r\n", Pe.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture));
            txt += String.Format("Зона выделения, Площадь: {0} кв км\r\n", Sq.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture));
            txt += "\r\n";

            if (PreparingAdditionalSelectionInfo != null) 
                if (!String.IsNullOrEmpty(_additSelectionInfoText)) 
                    txt += _additSelectionInfoText;
            
            ShowAdditionalInfoDialog("Информация", "Вспомогательная информация по области выделения:", txt);
        }

        public static System.Drawing.Point location2tile(double lat, double lon, int zoom)
        {
            if (System.Math.Abs(lat) > 85.0511287798066) return new System.Drawing.Point(0, 0);

            double sin_phi = System.Math.Sin(lat * System.Math.PI / 180);
            double norm_x = lon / 180;
            double norm_y = (0.5 * System.Math.Log((1 + sin_phi) / (1 - sin_phi))) / System.Math.PI;
            return new System.Drawing.Point((int)(System.Math.Pow(2, zoom) * ((norm_x + 1) / 2)), (int)(System.Math.Pow(2, zoom) * ((1 - norm_y) / 2)));
        }

        private void save2Shp_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".shp";
            sfd.Filter = "ESRI Shape Files (*.shp)|*.shp";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                PointF[] points = _selLayer[0][0].Points;
                RectToFile(sfd.FileName, points[0], points[2]);
            };
            sfd.Dispose();
        }

        public static byte[] Convert(byte[] ba, bool bigEndian)
        {
            if (BitConverter.IsLittleEndian != bigEndian) Array.Reverse(ba);
            return ba;
        }

        public static void RectToFile(string filename, PointF tleft, PointF bright)
        {
            double xmin = Math.Min(tleft.X, bright.X);
            double ymin = Math.Min(tleft.Y, bright.Y);
            double xmax = Math.Max(tleft.X, bright.X);
            double ymax = Math.Max(tleft.Y, bright.Y);

            List<byte> header = new List<byte>();
            header.AddRange(Convert(BitConverter.GetBytes((int)9994), false)); // File Code
            header.AddRange(new byte[20]);                                     // Not used
            header.AddRange(Convert(BitConverter.GetBytes((int)110), false));  // File_Length / 2
            header.AddRange(Convert(BitConverter.GetBytes((int)1000), true));  // Version 1000
            header.AddRange(Convert(BitConverter.GetBytes((int)5), true));     // Polygon Type
            header.AddRange(Convert(BitConverter.GetBytes((double)xmin), true));
            header.AddRange(Convert(BitConverter.GetBytes((double)ymin), true));
            header.AddRange(Convert(BitConverter.GetBytes((double)xmax), true));
            header.AddRange(Convert(BitConverter.GetBytes((double)ymax), true));
            header.AddRange(new byte[32]); // end of header

            header.AddRange(Convert(BitConverter.GetBytes((int)1), false)); // rec number
            header.AddRange(Convert(BitConverter.GetBytes((int)56), false));// rec_length / 2

            header.AddRange(Convert(BitConverter.GetBytes((int)5), true)); // rec type polygon
            header.AddRange(Convert(BitConverter.GetBytes((double)xmin), true));
            header.AddRange(Convert(BitConverter.GetBytes((double)ymin), true));
            header.AddRange(Convert(BitConverter.GetBytes((double)xmax), true));
            header.AddRange(Convert(BitConverter.GetBytes((double)ymax), true));
            header.AddRange(Convert(BitConverter.GetBytes((int)1), true)); // 1 part
            header.AddRange(Convert(BitConverter.GetBytes((int)4), true)); // 4 points
            header.AddRange(Convert(BitConverter.GetBytes((int)0), true)); // start at 0 point

            header.AddRange(Convert(BitConverter.GetBytes((double)tleft.X), true));  // point 0 x
            header.AddRange(Convert(BitConverter.GetBytes((double)tleft.Y), true));  // point 0 y
            header.AddRange(Convert(BitConverter.GetBytes((double)bright.X), true)); // point 1 x
            header.AddRange(Convert(BitConverter.GetBytes((double)tleft.Y), true));  // point 1 y
            header.AddRange(Convert(BitConverter.GetBytes((double)bright.X), true)); // point 2 x
            header.AddRange(Convert(BitConverter.GetBytes((double)bright.Y), true)); // point 2 y
            header.AddRange(Convert(BitConverter.GetBytes((double)tleft.X), true));  // point 3 x
            header.AddRange(Convert(BitConverter.GetBytes((double)bright.Y), true)); // point 3 y

            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            fs.Write(header.ToArray(), 0, header.Count);
            fs.Close();
        }

        private void saveBoundsShapeToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            BreakNext = false;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".shp";
            sfd.Filter = "ESRI Shape Files (*.shp)|*.shp";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                PointF[] points = MapBoundsAreaDegrees;
                RectToFile(sfd.FileName, points[0], points[1]);
            };
            sfd.Dispose();
        }

        private void saveMapImgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".png";
            sfd.Filter = "Portable Network Graphics (*.png)|*.png";
            if (sfd.ShowDialog() == DialogResult.OK)
                this.ImagePNG.Save(sfd.FileName);
            return;
        }

        private void openCacheFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            try
            {
                System.Diagnostics.Process.Start(NaviMapNet.NaviMapNetViewer.GetCurrentDir() + @"\CACHE\" + ((int)ImageSourceService_).ToString("X8"));
            }
            catch { };
        }

        private void setMapXYZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            int x = CenterPixelsX / 256;
            int y = CenterPixelsY / 256;
            int z = ZoomID_;
            if (InputXYZ("Map Center", "Set Map Center to:", ref x, ref y, ref z) == DialogResult.OK)
            {
                if ((z > 0) && (z < 22))
                {
                    PointF c = tile2location(x + 0.5, y + 0.5, z);
                    CenterLat_ = c.Y;
                    CenterLon_ = c.X;
                    ZoomID_ = (byte)z;
                    ReloadMap();
                };
            };
        }

        private void openExternalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BreakNext = false;

            PointF pcenter = this.CenterDegrees;
            PointF pclick = this.MouseDownDegrees;
            Point xyclick = this.MouseDownPixels; xyclick.X /= 256; xyclick.Y /= 256;

            string url = GetTileUrl(xyclick.X, xyclick.Y, this.ZoomID);

            BreakNext = false;
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch { };
        }

        private void Zoom2Sel_Click(object sender, EventArgs e)
        {
            BreakNext = false;
            ZoomByArea(SelectionBoundsMinMaxDegrees);
        }

        private void NaviMapNetViewer_Load(object sender, EventArgs e)
        {
            labelLoading.Parent = this;
        }


        private bool _mouseIsOver = false;
        private void NaviMapNetViewer_MouseEnter(object sender, EventArgs e)
        {
            _mouseIsOver = true;
        }

        private void NaviMapNetViewer_MouseLeave(object sender, EventArgs e)
        {
            _mouseIsOver = false;
            KeyDownAlt = false;
            KeyDownControl = false;
            KeyDownShift = false;
        }

        private void вернутьсяВНормальныйРежимToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BreakNext = false;
        }
    }

    internal class MouseMessageFilter : IMessageFilter
    {
        public static event MouseEventHandler MouseMove = delegate { };
        public static event MouseEventHandler MouseDown = delegate { };
        public static event MouseEventHandler MouseUp = delegate { };
        const int WM_MOUSEMOVE = 0x0200;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONUP = 0x0205;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_MOUSEMOVE)
            {
                Point mousePosition = Control.MousePosition;
                MouseMove(null, new MouseEventArgs(
                    MouseButtons.None, 0, mousePosition.X, mousePosition.Y, 0));
            };
            if ((m.Msg == WM_LBUTTONDOWN) || (m.Msg == WM_RBUTTONDOWN))
            {
                Point mousePosition = Control.MousePosition;
                MouseDown(null, new MouseEventArgs(
                    m.Msg == WM_LBUTTONUP ? MouseButtons.Left : MouseButtons.Right, 0, mousePosition.X, mousePosition.Y, 0));
            };
            if ((m.Msg == WM_LBUTTONUP) || (m.Msg == WM_RBUTTONUP))
            {
                Point mousePosition = Control.MousePosition;
                MouseUp(null, new MouseEventArgs(
                    m.Msg == WM_LBUTTONUP ? MouseButtons.Left : MouseButtons.Right, 0, mousePosition.X, mousePosition.Y, 0));
            };
            return false;
        }
    }

    internal static class MapCursors
    {
        /// <summary>
        ///     ZOOM IN
        /// </summary>
        public static Cursor ZoomIn
        {
            get
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(Properties.Resources.zoom_in);
                Cursor c = new Cursor(ms);
                ms.Close();
                return c;
            }
        }
        
        /// <summary>
        ///     ZOOM OUT
        /// </summary>
        public static Cursor ZoomOut
        {
            get
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(Properties.Resources.zoom_out);
                Cursor c = new Cursor(ms);
                ms.Close();
                return c;
            }
        }

        /// <summary>
        ///     SHIFT
        /// </summary>
        public static Cursor Shift
        {
            get
            {                
                return Cursors.Arrow;
            }
        }

        /// <summary>
        ///     SHIFT
        /// </summary>
        public static Cursor Move
        {
            get
            {
                return Cursors.Hand;
            }
        }
    }

    // ORIGINAL IN MapnikTileRenderer -> TileRendering.cs
    public class TilesProjection
    {
        public class PointD
        {
            private double x;
            private double y;
            public double X { get { return x; } set { x = value; } }
            public double Y { get { return y; } set { y = value; } }
            public double Lat { get { return y; } set { y = value; } }
            public double Lon { get { return x; } set { x = value; } }

            public PointD() { }
            public PointD(double X, double Y) { this.X = X; this.Y = Y; }
            public PointD(float X, float Y) { this.X = X; this.Y = Y; }
            public PointD(Point p) { this.X = p.X; this.Y = p.Y; }
            public PointD(PointF p) { this.X = p.X; this.Y = p.Y; }

            public static PointD FromXY(int X, int Y) { return new PointD(X, Y); }
            public static PointD FromXY(double X, double Y) { return new PointD(X, Y); }
            public static PointD FromXY(float X, float Y) { return new PointD(X, Y); }
            public static PointD FromXY(Point p) { return new PointD(p); }
            public static PointD FromXY(PointF p) { return new PointD(p); }
            public static PointD FromLL(double Lat, double Lon) { return new PointD(Lon, Lat); }

            public Point Point { get { return new Point((int)x, (int)y); } set { X = value.X; Y = value.Y; } }
            public PointF PointF { get { return new PointF((float)x, (float)y); } set { X = value.X; Y = value.Y; } }

            public override string ToString() { return "{" + x.ToString() + ";" + y.ToString() + "}"; }
        }

        #region private static methods and constants
        private const double DEG_TO_RAD = Math.PI / 180;
        private const double RAD_TO_DEG = 180 / Math.PI;
        private const double earth_radius = 6378137;
        private const double max_error = 1e-6;

        private static double[] Ac;
        private static double[] Bc;
        private static double[] Cc;
        private static double[][] zc;
        private static double c = 256;

        static TilesProjection()
        {
            Ac = new double[22];
            Bc = new double[22];
            Cc = new double[22];
            zc = new double[22][];
            for (int d = 0; d < 22; d++)
            {
                double e = c / 2;
                Bc[d] = c / 360;
                Cc[d] = c / (2 * Math.PI);
                zc[d] = new double[] { e, e };
                Ac[d] = c;
                c *= 2;
            };
        }

        private static double minmax(double a, double b, double c)
        {
            a = Math.Max(a, b);
            a = Math.Min(a, c);
            return a;
        }

        private static double deg2rad(double d) { return ((d) * Math.PI) / 180; }
        private static double rad2deg(double d) { return ((d) * 180) / Math.PI; }
        #endregion

        #region fromLLtoPixel
        public static double[] fromLLtoPixel(double[] ll, int zoom)
        {
            double[] d = zc[zoom];
            double e = Math.Round(d[0] + ll[0] * Bc[zoom]);
            double f = minmax(Math.Sin(DEG_TO_RAD * ll[1]), -0.9999, 0.9999);
            double g = Math.Round(d[1] + 0.5 * Math.Log((1 + f) / (1 - f)) * (-1 * Cc[zoom]));
            return new double[] { e, g };
        }
        public static Point fromLLtoPixel(double lat, double lon, int zoom)
        {
            double[] xy = fromLLtoPixel(new double[] { lon, lat }, zoom);
            return new Point((int)xy[0], (int)xy[1]);
        }
        public static Point fromLLtoPixel(PointF p, int zoom)
        {
            double[] xy = fromLLtoPixel(new double[] { p.X, p.Y }, zoom);
            return new Point((int)xy[0], (int)xy[1]);
        }
        public static PointD fromLLtoPixel(PointD p, int zoom)
        {
            double[] xy = fromLLtoPixel(new double[] { p.X, p.Y }, zoom);
            return new PointD(xy[0], xy[1]);
        }
        #endregion

        #region fromPixelToLL
        public static double[] fromPixelToLL(double[] px, int zoom)
        {
            double[] e = zc[zoom];
            double f = (px[0] - e[0]) / Bc[zoom];
            double g = (px[1] - e[1]) / (-1 * Cc[zoom]);
            double h = RAD_TO_DEG * (2 * Math.Atan(Math.Exp(g)) - 0.5 * Math.PI);
            return new double[] { f, h };
        }
        public static PointF fromPixelToLL(int x, int y, int zoom)
        {
            double[] ll = fromPixelToLL(new double[] { x, y }, zoom);
            return new PointF((float)ll[0], (float)ll[1]);
        }
        public static PointF fromPixelToLL(Point p, int zoom)
        {
            double[] ll = fromPixelToLL(new double[] { p.X, p.Y }, zoom);
            return new PointF((float)ll[0], (float)ll[1]);
        }
        public static PointF fromPixelToLL(PointF p, int zoom)
        {
            double[] ll = fromPixelToLL(new double[] { p.X, p.Y }, zoom);
            return new PointF((float)ll[0], (float)ll[1]);
        }
        public static PointD fromPixelToLL(PointD p, int zoom)
        {
            double[] ll = fromPixelToLL(new double[] { p.X, p.Y }, zoom);
            return new PointD(ll[0], ll[1]);
        }
        #endregion

        #region fromLLToTile
        public static void fromLLToTile(double lat, double lon, int zoom, out int x, out int y)
        {
            x = (int)((lon + 180.0) / 360.0 * Math.Pow(2.0, zoom));
            y = (int)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * Math.Pow(2.0, zoom));
        }
        public static void fromLLToTile(double lat, double lon, int zoom, out double x, out double y)
        {
            double sin_phi = System.Math.Sin(lat * System.Math.PI / 180);
            double norm_x = lon / 180;
            double norm_y = (0.5 * System.Math.Log((1 + sin_phi) / (1 - sin_phi))) / System.Math.PI;
            x = (System.Math.Pow(2, zoom) * ((norm_x + 1) / 2));
            y = (System.Math.Pow(2, zoom) * ((1 - norm_y) / 2));
        }
        public static Point fromLLToTile(double lat, double lon, int zoom)
        {
            int x, y;
            fromLLToTile(lat, lon, zoom, out x, out y);
            return new Point(x, y);
        }
        public static Point fromLLToTile(PointF p, int zoom)
        {
            int x, y;
            fromLLToTile(p.Y, p.X, zoom, out x, out y);
            return new Point(x, y);
        }
        public static PointD fromLLToTile(PointD p, int zoom)
        {
            double x, y;
            fromLLToTile(p.Y, p.X, zoom, out x, out y);
            return new PointD(x, y);
        }
        #endregion

        #region fromTileToLL
        public static void fromTileToLL(int x, int y, int zoom, out double lat, out double lon)
        {
            lon = ((x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
            double n = Math.PI - ((2.0 * Math.PI * y) / Math.Pow(2.0, zoom));
            lat = (180.0 / Math.PI * Math.Atan(Math.Sinh(n)));
        }
        public static void fromTileToLL(double x, double y, int zoom, out double Lat, out double Lng)
        {
            Lng = ((x * 256) - (256 * Math.Pow(2, zoom - 1))) / ((256 * Math.Pow(2, zoom)) / 360);
            while (Lng > 180) Lng -= 360;
            while (Lng < -180) Lng += 360;

            double exp = ((y * 256) - (256 * Math.Pow(2, zoom - 1))) / ((-256 * Math.Pow(2, zoom)) / (2 * Math.PI));
            Lat = ((2 * Math.Atan(Math.Exp(exp))) - (Math.PI / 2)) / (Math.PI / 180);
            if (Lat < -90) Lat = -90;
            if (Lat > 90) Lat = 90;
        }
        public static PointF fromTileToLL(double x, double y, int zoom)
        {
            double Lng = ((x * 256) - (256 * Math.Pow(2, zoom - 1))) / ((256 * Math.Pow(2, zoom)) / 360);
            while (Lng > 180) Lng -= 360;
            while (Lng < -180) Lng += 360;

            double exp = ((y * 256) - (256 * Math.Pow(2, zoom - 1))) / ((-256 * Math.Pow(2, zoom)) / (2 * Math.PI));
            double Lat = ((2 * Math.Atan(Math.Exp(exp))) - (Math.PI / 2)) / (Math.PI / 180);
            if (Lat < -90) Lat = -90;
            if (Lat > 90) Lat = 90;

            return new PointF((float)Lng, (float)Lat);
        }
        public static PointF fromTileToLL(int x, int y, int zoom)
        {
            double lat, lon;
            fromTileToLL(x, y, zoom, out lat, out lon);
            return new PointF((float)lon, (float)lat);
        }
        public static PointF fromTileToLL(Point p, int zoom)
        {
            return fromTileToLL(p.X, p.Y, zoom);
        }
        public static PointF fromTileToLL(PointF p, int zoom)
        {
            double x, y;
            fromTileToLL(p.X, p.Y, zoom, out y, out x);
            return new PointF((float)x, (float)y);
        }
        public static PointD fromTileToLL(PointD p, int zoom)
        {
            PointD res = new PointD();
            res.PointF = fromTileToLL(p.Point, zoom);
            return res;
        }
        #endregion

        #region fromLLtoMeter
        public static PointF fromLLtoMeter(double lat, double lon)
        {
            return new PointF((float)lon2x_m(lon), (float)lat2y_m(lat));
        }
        public static PointF fromLLtoMeter(PointF p)
        {
            return new PointF((float)lon2x_m(p.X), (float)lat2y_m(p.Y));
        }
        public static PointD fromLLtoMeter(PointD p)
        {
            return new PointD(lon2x_m(p.X), lat2y_m(p.Y));
        }
        #endregion

        #region fromMeterToLL
        public static PointF fromMeterToLL(double x, double y)
        {
            return new PointF((float)x2lon_m(x), (float)y2lat_m(y));
        }
        public static PointF fromMeterToLL(PointF p)
        {
            return new PointF((float)x2lon_m(p.X), (float)y2lat_m(p.Y));
        }
        public static PointD fromMeterToLL(PointD p)
        {
            return new PointD(x2lon_m(p.X), y2lat_m(p.Y));
        }
        #endregion

        #region fromPixelToMeter
        public static PointF fromPixelToMeter(int x, int y, int zoom)
        {
            return fromLLtoMeter(fromPixelToLL(x, y, zoom));
        }
        public static PointF fromPixelToMeter(Point p, int zoom)
        {
            return fromLLtoMeter(fromPixelToLL(p, zoom));
        }
        public static PointF fromPixelToMeter(PointF p, int zoom)
        {
            return fromLLtoMeter(fromPixelToLL(p, zoom));
        }
        public static PointD fromPixelToMeter(PointD p, int zoom)
        {
            return fromLLtoMeter(fromPixelToLL(p, zoom));
        }
        #endregion

        #region fromMeterToPixel
        public static Point fromMeterToPixel(double x, double y, int zoom)
        {
            return fromLLtoPixel(fromMeterToLL(x, y), zoom);
        }
        public static Point fromMeterToPixel(PointF p, int zoom)
        {
            return fromLLtoPixel(fromMeterToLL(p), zoom);
        }
        public static PointD fromMeterToPixel(PointD p, int zoom)
        {
            return fromLLtoPixel(fromMeterToLL(p), zoom);
        }
        #endregion

        #region fromTileToMeter
        public static PointF fromTileToMeter(int x, int y, int zoom)
        {
            return fromLLtoMeter(fromTileToLL(x, y, zoom));
        }
        public static PointF fromTileToMeter(double x, double y, int zoom)
        {
            return fromLLtoMeter(fromTileToLL(new PointD(x, y), zoom)).PointF;
        }
        public static PointF fromTileToMeter(Point p, int zoom)
        {
            return fromLLtoMeter(fromTileToLL(p, zoom));
        }
        public static PointF fromTileToMeter(PointF p, int zoom)
        {
            return fromLLtoMeter(fromTileToLL(p, zoom));
        }
        public static PointD fromTileToMeter(PointD p, int zoom)
        {
            return fromLLtoMeter(fromTileToLL(p, zoom));
        }
        #endregion

        #region fromMeterToTile
        public static Point fromMeterToTile(double x, double y, int zoom)
        {
            return fromLLToTile(fromMeterToLL(x, y), zoom);
        }
        public static Point fromMeterToTile(PointF p, int zoom)
        {
            return fromLLToTile(p, zoom);
        }
        public static PointD fromMeterToTile(PointD p, int zoom)
        {
            return fromLLToTile(p, zoom);
        }
        #endregion

        #region fromPixelToTile
        public static Point fromPixelToTile(int x, int y)
        {
            return new Point((int)((double)x / 256.0), (int)((double)y / 256.0));
        }
        public static Point fromPixelToTile(Point p)
        {
            return new Point((int)((double)p.X / 256.0), (int)((double)p.Y / 256.0));
        }
        public static PointF fromPixelToTile(PointF p)
        {
            return new PointF(p.X / 256, p.Y / 256);
        }
        public static PointD fromPixelToTile(PointD p)
        {
            return new PointD(p.X / 256.0, p.Y / 256.0);
        }
        #endregion

        #region fromTileToPixel
        public static Point fromTileToPixel(int x, int y)
        {
            return new Point((int)((double)x * 256.0), (int)((double)y * 256.0));
        }
        public static Point fromTileToPixel(Point p)
        {
            return new Point((int)((double)p.X * 256.0), (int)((double)p.Y * 256.0));
        }
        public static PointF fromTileToPixel(PointF p)
        {
            return new PointF(p.X * 256, p.Y * 256);
        }
        public static PointD fromTileToPixel(PointD p)
        {
            return new PointD(p.X * 256.0, p.Y * 256.0);
        }
        #endregion

        // /* The following functions take or return there results in degrees */

        //private static double y2lat_d(double y) { return rad2deg(2 * Math.Atan(Math.Exp(deg2rad(y))) - Math.PI / 2); }
        //private static double x2lon_d(double x) { return x; }
        //private static double lat2y_d(double lat) { return rad2deg(Math.Log(Math.Tan(Math.PI / 4 + deg2rad(lat) / 2))); }
        //private static double lon2x_d(double lon) { return lon; }

        /* The following functions take or return there results in something close to meters, along the equator */

        public static double y2lat_m(double y) { return rad2deg(2 * Math.Atan(Math.Exp((y / earth_radius))) - Math.PI / 2); }
        public static double x2lon_m(double x) { return rad2deg(x / earth_radius); }
        public static double lat2y_m(double lat) { return earth_radius * Math.Log(Math.Tan(Math.PI / 4 + deg2rad(lat) / 2)); }
        public static double lon2x_m(double lon) { return deg2rad(lon) * earth_radius; }
    }

    internal class MapMerger
    {
        public NaviMapNetViewer.GetTilePathCall GetTilePath = null;

        private int WebRequestTimeout_ = 100000;
        public int WebRequestTimeout { get { return WebRequestTimeout_; } set { WebRequestTimeout_ = value; } }
        private Color NotFoundTileColor_ = Color.Transparent;
        public Color NotFoundTileColor { get { return NotFoundTileColor_; } set { NotFoundTileColor_ = value; } }

        private int Width = 512;
        private int Height = 256;
        public int MinZoom = 2;
        public int MaxZoom = 18;

        private double mLat = 55.7250;
        private double mLon = 37.602;
        private int mZoom = 10;

        private int Center_TileX;
        private int Center_TileY;
        private int Center_Tile_XPos;
        private int Center_Tile_YPos;
        private double LonPerTile;
        private double LonPerPixel;

        public bool DrawTileXYZ = false;
        public bool DrawTileBorder = false;
        public short TilesRenderingZoneSize = 0;

        private bool _invertBackground = false;
        public bool InvertBackground { get { return _invertBackground; } set { _invertBackground = value; } }

        private bool _useDiskCache = false;
        public bool UseDiskCache { get { return _useDiskCache; } set { _useDiskCache = value; } }

        private string _CacheSubDirectory = "";
        public string CacheSubDirectory { get { return _CacheSubDirectory; } set { _CacheSubDirectory = value; if (_CacheSubDirectory == "") _CacheSubDirectory = ""; } }


        public MapMerger()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            CalcGeo();
        }        

        public void SetMap(double Lat, double Lon, int Zoom)
        {
            this.mLat = Lat;
            this.mLon = Lon;
            this.mZoom = Zoom;
            CalcGeo();
        }

        public int Zoom
        {
            get { return mZoom; }
            set
            {
                mZoom = value;
                CalcGeo();
            }
        }

        // http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
        // http://forum.kde.org/viewtopic.php?f=217&t=108975
        private static void GetTileXYFromLatLon(double lat, double lon, int zoom, out int x, out int y)
        {
            x = (int)((lon + 180.0) / 360.0 * Math.Pow(2.0, zoom));
            y = (int)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * Math.Pow(2.0, zoom));
        }
        // top left
        // http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
        // http://forum.kde.org/viewtopic.php?f=217&t=108975
        private static void GetLatLonFromTileXY(int x, int y, int zoom, out double lat, out double lon)
        {
            lon = ((x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
            double n = Math.PI - ((2.0 * Math.PI * y) / Math.Pow(2.0, zoom));
            lat = (180.0 / Math.PI * Math.Atan(Math.Sinh(n)));
        }
        private const double earth_radius = 6378137;
        private const double max_error = 1e-6;
        private static double deg2rad(double d) { return ((d) * Math.PI) / 180; }
        private static double rad2deg(double d) { return ((d) * 180) / Math.PI; }
        private static double y2lat_d(double y) { return rad2deg(2 * Math.Atan(Math.Exp(deg2rad(y))) - Math.PI / 2); }
        private static double x2lon_d(double x) { return x; }
        private static double lat2y_d(double lat) { return rad2deg(Math.Log(Math.Tan(Math.PI / 4 + deg2rad(lat) / 2))); }
        private static double lon2x_d(double lon) { return lon; }
        private static double y2lat_m(double y) { return rad2deg(2 * Math.Atan(Math.Exp((y / earth_radius))) - Math.PI / 2); }
        private static double x2lon_m(double x) { return rad2deg(x / earth_radius); }
        private static double lat2y_m(double lat) { return earth_radius * Math.Log(Math.Tan(Math.PI / 4 + deg2rad(lat) / 2)); }
        private static double lon2x_m(double lon) { return deg2rad(lon) * earth_radius; }

        public void GetImageBounds(out double lat1, out double lon1, out double lat2, out double lon2)
        {
            ImagePosToLatLon(0, 0, out lat1, out lon1);
            ImagePosToLatLon(Width - 1, Height - 1, out lat2, out lon2);
        }

        private void ImagePosToLatLon(int x, int y, out double lat, out double lon)
        {
            lon = mLon + LonPerPixel * (x - Width / 2);

            double offset = (double)(y - Center_Tile_YPos) / 256;
            double ty = Center_TileY + offset;
            double n = Math.PI - ((2.0 * Math.PI * ty) / Math.Pow(2.0, mZoom));
            lat = (180.0 / Math.PI * Math.Atan(Math.Sinh(n)));
        }

        private void LatLonToImagePos(double lat, double lon, out int x, out int y)
        {
            x = Width / 2 + (int)((lon - mLon) / LonPerPixel);
            double ty = ((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * Math.Pow(2.0, mZoom));
            y = Center_Tile_YPos + (int)((ty - Center_TileY) * 256);
        }

        private void CalcGeo()
        {
            GetTileXYFromLatLon(mLat, mLon, mZoom, out Center_TileX, out Center_TileY);

            double top, left;
            GetLatLonFromTileXY(Center_TileX, Center_TileY, mZoom, out top, out left);

            double bottom, right;
            GetLatLonFromTileXY(Center_TileX + 1, Center_TileY + 1, mZoom, out bottom, out right);

            LonPerTile = 360.0 / (Math.Pow(2, mZoom));
            LonPerPixel = LonPerTile / 256;

            Center_Tile_XPos = (int)(Width / 2 - (256.0 * ((mLon - left) / (right - left))));
            Center_Tile_YPos = (int)(Height / 2 - (256.0 * ((mLat - top) / (bottom - top))));
        }

        private string CacheDir = NaviMapNet.NaviMapNetViewer.GetCurrentDir() + @"\CACHE\";
        private Image GetTile(int x, int y, int z)
        {
            bool cached = false;
            return GetTile(x, y, z, out cached);
        }
        private Image GetTile(int x, int y, int z, out bool cached)
        {
            cached = false;
            string url = "";
            if (GetTilePath != null) url = GetTilePath(x, y, z);
            if (_useDiskCache)
            {
                string fn = CacheDir + _CacheSubDirectory + @"\" + z.ToString() + @"\" + y.ToString() + @"\" + x.ToString() + ".png";
                try
                {
                    string dir = Path.GetDirectoryName(fn);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                }
                catch { };
                if (!File.Exists(fn))
                {
                    Image im = null;
                    bool isEmpty = true;
                    try
                    {                        
                        im = NaviMapNetViewer.GetImageFromURL(url, WebRequestTimeout_, NotFoundTileColor_, out isEmpty);
                    }
                    catch { };
                    if ((im != null) && (!isEmpty)) try { im.Save(fn); }
                        catch { };
                    return im;
                }
                else
                {
                    try
                    {
                        cached = true;
                        return Image.FromFile(fn);
                    }
                    catch
                    {
                        return null;
                    };
                };
            };
            return NaviMapNetViewer.GetImageFromURL(url, WebRequestTimeout_, NotFoundTileColor_);
        }       

        private double[] ZoomsDLon = GetZoomsDLon();
        private static double[] GetZoomsDLon()
        {
            double lonmin = 0;
            double latmin = 0;
            double lonmax = 0;
            double latmax = 0;
            int tilex = 1; int tiley = 1;
            double[] res = new double[22];
            for (int i = 1; i < 22; i++)
            {
                GetLatLonFromTileXY(tilex, tiley, i, out latmin, out lonmin);
                GetLatLonFromTileXY(tilex + 1, tiley + 1, i, out latmax, out lonmax);
                res[i] = (lonmax - lonmin) / 256;
            };
            return res;
        }

        public Image GetMap(double Lat, double Lon, int z)
        {
            this.SetMap(Lat, Lon, z);
            return this.GetMap();
        }

        public Image GetMap(double LatMin, double LonMin, double LatMax, double LonMax, bool true_Degrees__false_meters, int Width, int Height)
        {
            if (!true_Degrees__false_meters)
            {
                LonMin = x2lon_m(LonMin);
                LatMin = y2lat_m(LatMin);
                LonMax = x2lon_m(LonMax);
                LatMax = y2lat_m(LatMax);
            };

            double latc = (LatMin + LatMax) / 2;
            double lonc = (LonMin + LonMax) / 2;
            double dlon = Math.Abs(LonMax - LonMin) / Width;
            double dlat = Math.Abs(LatMax - LatMin) / Height;

            int zoom = 0;
            for (int i = 1; i < 22; i++)
                if (Math.Abs(dlon) < (ZoomsDLon[i] + max_error)) zoom = i;
            if (zoom < MinZoom) zoom = MinZoom;
            if (zoom > MaxZoom) zoom = MaxZoom;
            double zoomscaleW = dlon / ZoomsDLon[zoom];

            this.Width = ((zoomscaleW == 1) && (Width == 256)) ? Width : (int)(Width * zoomscaleW + 5);
            if (this.Width < 256) this.Width = 256;
            this.Height = ((zoomscaleW == 1) && (Height == 256)) ? Height : (int)(Height * zoomscaleW + 5);
            if (this.Height < 256) this.Height = 256;
            this.SetMap(latc, lonc, zoom);
            this.CalcGeo();
            Image big = GetMap();
            if ((zoomscaleW == 1) && (Width == 256) && (Height == 256)) return big;

            int minx, miny, maxx, maxy;
            LatLonToImagePos(LatMin, LonMin, out minx, out maxy);
            LatLonToImagePos(LatMax, LonMax, out maxx, out miny);
            if (zoomscaleW <= 0.5)
                minx--; miny--; maxx--; maxy--;

            Image res = new Bitmap(Width, Height);
            Graphics g2 = Graphics.FromImage(res);
            g2.DrawImage(big, new RectangleF(0, 0, (float)Width, (float)Height), new RectangleF((float)minx, (float)miny, (float)(maxx - minx + 1), (float)(maxy - miny + 1)), GraphicsUnit.Pixel);
            g2.Dispose();
            return res;
        }

        private Point[][] DashedSquare(int sx, int sy)
        {
            List<Point[]> res = new List<Point[]>();
            for (int inc = 0; inc < 256; inc += 16)
            {
                res.Add(new Point[] { new Point(sx + inc, sy), new Point(sx + inc + 8, sy) });
                res.Add(new Point[] { new Point(sx, sy + inc), new Point(sx, sy + inc + 8) });
            };
            return res.ToArray();
        }

        private Point[][] DashedHorizontal(int sx, int sy)
        {
            List<Point[]> res = new List<Point[]>();
            for (int inc = 8; inc < 256; inc += 32)
            {
                res.Add(new Point[] { new Point(sx + inc, sy), new Point(sx + inc + 8, sy) });                
            };
            return res.ToArray();
        }

        private Point[][] DashedVertical(int sx, int sy)
        {
            List<Point[]> res = new List<Point[]>();
            for (int inc = 8; inc < 256; inc += 32)
            {
                res.Add(new Point[] { new Point(sx, sy + inc), new Point(sx, sy + inc + 8) });
            };
            return res.ToArray();
        }

        private Image InvertImage(Image im)
        {
            if (im == null) return null;
            Bitmap pic = new Bitmap(im);
            for (int y = 0; (y <= (pic.Height - 1)); y++)
            {
                for (int x = 0; (x <= (pic.Width - 1)); x++)
                {
                    Color inv = pic.GetPixel(x, y);
                    inv = Color.FromArgb(255, (255 - inv.R), (255 - inv.G), (255 - inv.B));
                    pic.SetPixel(x, y, inv);
                }
            }
            return pic;
        }

        public Image GetMap()
        {
            Image im = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage(im);

            Font f = new Font("Calibri", 9, FontStyle.Bold);
            Brush b = new SolidBrush(Color.FromArgb(200, Color.Black));
            Pen p = new Pen(b);
            Pen z = new Pen(new SolidBrush(Color.DarkViolet));

            try
            {
                // Find where to draw the centre tile...
                int drawx, drawy;
                drawx = Center_Tile_XPos;
                drawy = Center_Tile_YPos;

                int x = Center_TileX;
                int y = Center_TileY;

                // Move the position back to the top-leftmost tile we need to
                // draw...
                while (drawx >= 0) { drawx -= 256; x--; }
                while (drawy >= 0) { drawy -= 256; y--; }

                // Draw all the tiles...
                int curdrawx;
                int curx;
                while (drawy < Height)
                {
                    curdrawx = drawx;
                    curx = x;
                    while (curdrawx < Width)
                    {
                        try
                        {
                            bool cached = false;
                            Image img = GetTile(curx, y, mZoom, out cached);
                            if(InvertBackground) img = InvertImage(img);
                            if (img != null)
                                g.DrawImage(img, curdrawx, drawy, 256, 256);
                            if (DrawTileXYZ)
                            {
                                string txt = "x " + curx.ToString() + " y " + y.ToString() + " z " + mZoom.ToString();
                                g.DrawString(txt, f, b, curdrawx + 2, drawy + 2);
                                g.DrawLine(p, curdrawx, drawy, curdrawx + 8, drawy);
                                g.DrawLine(p, curdrawx, drawy, curdrawx, drawy + 8);
                                //
                                if (cached)
                                {
                                    txt = "cached";
                                    g.DrawString(txt, f, new SolidBrush(Color.Maroon), curdrawx + 2, drawy + f.Height + 4);
                                    g.DrawLine(p, curdrawx, drawy, curdrawx + 8, drawy);
                                    g.DrawLine(p, curdrawx, drawy, curdrawx, drawy + 8);
                                };
                            };
                            if (DrawTileBorder)
                            {
                                foreach (Point[] line in DashedSquare(curdrawx, drawy))
                                    g.DrawLine(p, line[0], line[1]);
                                if ((TilesRenderingZoneSize > 1) && ((curx % TilesRenderingZoneSize) == 0))
                                    g.DrawLine(z, curdrawx, drawy, curdrawx, drawy + 256);
                                    //foreach (Point[] line in DashedVertical(curdrawx, drawy))
                                    //    g.DrawLine(z, line[0], line[1]);
                                if ((TilesRenderingZoneSize > 1) && ((y % TilesRenderingZoneSize) == 0))
                                    g.DrawLine(z, curdrawx, drawy, curdrawx + 256, drawy);
                                    //foreach (Point[] line in DashedHorizontal(curdrawx, drawy))
                                    //    g.DrawLine(z, line[0], line[1]);
                            };
                        }
                        catch (Exception ex) { };
                        curdrawx += 256;
                        curx++;
                    }
                    drawy += 256;
                    y++;
                }

                //// Draw markers...
                if (Zoom > 14)
                {
                    int mx, my;
                    LatLonToImagePos(55.68461, 37.69849, out mx, out my);
                    Pen pen = new Pen(Color.Red);
                    g.DrawEllipse(pen, mx - 3, my - 3, 7, 7);
                    pen.Dispose();
                };
                ////
            }
            catch (Exception ex)
            {

            };

            return im;
        }        
    }
}