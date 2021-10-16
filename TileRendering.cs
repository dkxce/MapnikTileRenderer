// --  read shape 1st record
// заменить tmpArea.points = new PointF[se-si-1];
// на tmpArea.points = new PointF[se-si];
// CHECK: int se = (i + 1) == this.Area.parts.Length ? this.Area.points.Length : this.Area.parts[i + 1];
// CEHCK: int se = (i+1) == this.Area.parts.Length ? this.Area.points.Length : this.Area.parts[i+1];

#define TileRenderer

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.InteropServices;

using BOOL = System.Boolean;
using DWORD = System.UInt32;
using LPWSTR = System.String;
using NET_API_STATUS = System.UInt32;


namespace TileRendering
{
    /// <summary>
    ///     Формат хранения тайлов в папках
    /// </summary>
    public enum TileRenderPathScheme
    {
        [Description(@"Хранить в формате Slippy map \Zoom\X\Y")]
        /// <summary>
        ///     Хранить в формате Slippy map \Zoom\X\Y
        ///     http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
        /// </summary>
        fsSlippyMap = 0x01,
        [Description(@"Хранить в формате ArcGIS conf.xml (\Zoom\Y\X)")]
        /// <summary>
        ///     Хранить в формате ArcGIS conf.xml (\Zoom\Y\X)
        /// </summary>
        fsArcGIS = 0x02
    }

    /// <summary>
    ///     Размер изображения для разделения участка на тайлы
    /// </summary>
    public enum TileRenderZoneSize
    {
        /// <summary>
        ///     256x256 (1 тайл)
        /// </summary>
        tileZone_1x1 = 0x01,
        /// <summary>
        ///     512x512 (4 тайла)
        /// </summary>
        tileZone_2x2 = 0x02,
        tileZone_3x3 = 0x03,
        tileZone_4x4 = 0x04,
        tileZone_5x5 = 0x05,
        tileZone_6x6 = 0x06,
        tileZone_7x7 = 0x07,
        tileZone_8x8 = 0x08,
        tileZone_9x9 = 0x09,
        tileZone_10x10 = 0x0A,
        tileZone_11x11 = 0x0B,
        tileZone_12x12 = 0x0C,
        tileZone_13x13 = 0x0D,
        tileZone_14x14 = 0x0E,
        tileZone_15x15 = 0x0F
    }

    /// <summary>
    ///     Сохранять ли пустые и необработанные тайлы
    /// </summary>
    public enum TileRenderEmptyData
    {
        /// <summary>
        ///     Сохранять все тайлы
        /// </summary>
        saveAnyTiles = 0x01,
        /// <summary>
        ///     Сохранять все тайлы в нарезаемом диапазоне (в том числе одноцветные и неотрисованные)
        /// </summary>
        saveOneColorTiles = 0x02,
        /// <summary>
        ///     Не сохранять одноцветные тайлы (сохраняются только несущую полезную информацию)
        /// </summary>
        saveActualTiles = 0x03,
        /// <summary>
        ///     Не сохранять одноцветные тайлы (сохраняются только несущую полезную информацию)    
        /// </summary>
        saveAnyAreaTiles = 0x04
    }

    /// <summary>
    ///     Сохранять запрашиваемый тайл или все тайлы зоны
    /// </summary>
    public enum TileRenderZoneMode
    {
        /// <summary>
        ///     Сохранять все тайлы зоны
        /// </summary>
        keepFullZone = 0x01,
        /// <summary>
        ///     Сохранять только запрашиваемый тайл
        /// </summary>
        keepOnlyCurrent = 0x02
    }

    /// <summary>
    ///     Оптимизировать ли тайлы
    /// </summary>
    public enum TileOptimizeMethod
    {
        none = 0x01,
        fastest = 0x02, // используется FreeImage.DLL/быстрое и Standard, сохраняется файл с меньшим размером	
        fastmax = 0x03, // используется FreeImage.DLL/максимальное и Standard, сохраняется файл с меньшим размером
        imageMagic = 0x04, // используется imageMagic и Standard, сохраняется файл с меньшим размером
        optimize = 0x05, // используется optimize.dll и Standard, сохраняется файл с меньшим размером
        fastMagic = 0x06, //используется imageMagic, FreeImage.DLL/максимальное и Standard, сохраняется файл с меньшим размером                
        maximim = 0x07 // используется imageMagic, optimize.dll и Standard, сохраняется файл с меньшим размером
    }

    /// <summary>
    ///     Если тайл с таким именем уже существует
    /// </summary>
    public enum TileFinalMerging
    {
        [Description("Не перезаписывать существующий файл")]
        /// <summary>
        ///     Не перезаписывать существующий файл
        /// </summary>
        keepExistingFile = 0x01,
        [Description("Перезаписывать существующий файл")]
        /// <summary>
        ///     Перезаписывать существующий файл
        /// </summary>
        overwriteExistingFile = 0x02,
        [Description("Объединять с существующим тайлом")]
        /// <summary>
        ///     Объединять с существующим тайлом
        /// </summary>
        mergeWithExistingFile = 0x03
    }

    /// <summary>
    ///     Конфигурация
    /// </summary>
    [Serializable]
    public class TileRendererConfig
    {
        /// <summary>
        ///     Число потоков
        /// </summary>
        public int multithreadCount = 1;

        /// <summary>
        ///     При ошибках отрисовки пробывать отризсовать зону заново, положив снова в очередь
        /// </summary>
        public bool addToQueueAgainIfError = true;

        /// <summary>
        ///     Перезагружатьфайл карты при каждой нарезке зоны
        /// </summary>
        public bool reloadMapEachZone = false;



        // <summary>
        ///     Размер перекрытия зоны за границей изображения
        /// </summary>
        public ushort tileZoneOverSize = 128;

        /// <summary>
        ///     Добавлять ли копирайт?
        /// </summary>
        public string addCopyrightsFile = null;

        /// <summary>
        ///     Обрабатывать тайлы только попадающие в полигон
        /// </summary>
        public string renderOnlyInPolygon = null;

        /// <summary>
        ///     Структура папок хранения тайлов
        /// </summary>
        public TileRenderPathScheme pathStructure = TileRenderPathScheme.fsSlippyMap;

        /// <summary>
        ///     Размер карты при отрисовки
        /// </summary>
        public TileRenderZoneSize tileZoneSize = TileRenderZoneSize.tileZone_8x8;

        /// <summary>
        ///     Сохранение тайлов при отрисовки
        /// </summary>
        public TileRenderZoneMode tileZoneMode = TileRenderZoneMode.keepFullZone;

        /// <summary>
        ///     Обрабатывать пустые тайлы
        /// </summary>
        public TileRenderEmptyData processEmptyTiles = TileRenderEmptyData.saveActualTiles;

        /// <summary>
        ///     Оптимизировать ли тайлы
        /// </summary>
        public TileOptimizeMethod tileOptimizeMethod = TileOptimizeMethod.none;

        /// <summary>
        ///     Формат PNG
        /// </summary>
        public byte tileOptimizeFormat = 0;

        /// <summary>
        ///     Перезаписывать ли существующий тайл
        /// </summary>
        public TileFinalMerging tileFinalMerging = TileFinalMerging.overwriteExistingFile;        
    }

    public class TileRenderingErrors
    {
        public class ErrorInfo
        {
            public DateTime dt;
            public uint ThreadID;
            public int x;
            public int y;
            public int z;
            public Exception ex;
            public string comment;
            public string parameter;
            public ErrorInfo(int x, int y, int z, string comment, Exception ex, string parameter)
            {
                this.dt = DateTime.Now;                
                this.ThreadID = MultithreadTileRenderer.GetCurrentThreadId();
                this.x = x;
                this.y = y;
                this.z = z;
                this.comment = comment;
                this.ex = ex;
                this.parameter = parameter;
            }
        }
        public static Mutex ErrorQueueMutex = new Mutex();
        public static ulong ErrorQueueTotalCount = 0;
        public static byte PauseThreadsIfErrorsCountIs = 0;
        public static List<uint> PausedThreads = new List<uint>();
        public static Mutex PausedThreadsMutex = new Mutex();
        public static Queue<ErrorInfo> ErrorQueue = new Queue<ErrorInfo>();

        public static bool PauseNormal = false;
        public static ulong PausedNormal = 0;
        public static bool WithDump = true;
        //public static string 
    }

    /// <summary>
    ///     Spherical Mercator Projection
    ///     http://spatialreference.org/ref/sr-org/7483/
    ///     EPSG:3857 / SR-ORG:7483 / EPSG:900913 / WGS84 Web Mercator (Auxiliary Sphere, Spherical Mercator)
    ///     Proj4: +proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +k=1.0 +x_0=0.0 +y_0=0.0 +units=m +no_defs +nadgrids=@null +over
    ///     [[ORIGINAL]]
    /// </summary>
    public class TilesProjection
    {
        public class PointD
        {
            private double x;
            private double y;
            public double X {get {return x;} set {x = value;}}
            public double Y {get {return y;} set {y = value;}}
            public double Lat {get {return y;} set {y = value;}}
            public double Lon{get {return x;} set {x = value;}}

            public PointD(){}
            public PointD(double X, double Y) {this.X = X; this.Y = Y;}
            public PointD(float X, float Y) { this.X = X; this.Y = Y; }
            public PointD(Point p) {this.X = p.X; this.Y = p.Y;}
            public PointD(PointF p) {this.X = p.X; this.Y = p.Y;}

            public static PointD FromXY(int X, int Y) { return new PointD(X, Y); }
            public static PointD FromXY(int[] xy) { return new PointD(xy[0], xy[1]); }
            public static PointD FromXY(double X, double Y) { return new PointD(X,Y);}
            public static PointD FromXY(double[] xy) { return new PointD(xy[0], xy[1]); }
            public static PointD FromXY(float X, float Y) { return new PointD(X, Y); }
            public static PointD FromXY(Point p) { return new PointD(p);}
            public static PointD FromXY(PointF p) { return new PointD(p);}
            public static PointD FromLL(double Lat, double Lon) { return new PointD(Lon, Lat); }

            public Point Point { get { return new Point((int)x, (int)y); } set { X = value.X; Y = value.Y; } }
            public PointF PointF {   get { return new PointF((float)x,(float)y); } set { X = value.X; Y = value.Y; }}

            public override string ToString() { return "{" + x.ToString() + ";" + y.ToString() + "}"; }

            public static explicit operator PointD(Point p) { return new PointD(p.X, p.Y); }
            public static explicit operator PointD(PointF p) { return new PointD(p.X, p.Y); }
            public static explicit operator PointD(double[] xy) { return new PointD(xy[0], xy[1]); }
            public static explicit operator PointD(int[] xy) { return new PointD(xy[0], xy[1]); }
            public static explicit operator Point(PointD p) { return new Point((int)p.X, (int)p.Y); }
            public static explicit operator PointF(PointD p) { return new PointF((float)p.X, (float)p.Y); }
            public static explicit operator double[](PointD p) { return new double[] { p.X, p.Y }; }            
            public static explicit operator int[](PointD p) { return new int[] { (int)p.X, (int)p.Y }; }            
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

        #region GetLengthMeters        
        public static uint GetLengthMeters(PointF a, PointF b)
        {
            return GetLengthMeters(a.Y, a.X, b.Y, b.X, false);
        }
        public static uint GetLengthMeters(PointD a, PointD b)
        {
            return GetLengthMeters(a.Lat, a.Lon, b.Lat, b.Lon, false);
        }
        public static uint GetLengthMeters(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            return GetLengthMetersC(StartLat, StartLong, EndLat, EndLong, radians);
        }
        public static uint GetLengthMetersA(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double D2R = Math.PI / 180;     // Преобразование градусов в радианы

            double a = 6378137.0000;     // WGS-84 Equatorial Radius (a)
            double f = 1 / 298.257223563;  // WGS-84 Flattening (f)
            double b = (1 - f) * a;      // WGS-84 Polar Radius
            double e2 = (2 - f) * f;      // WGS-84 Квадрат эксцентричности эллипсоида  // 1-(b/a)^2

            // Переменные, используемые для вычисления смещения и расстояния
            double fPhimean;                           // Средняя широта
            double fdLambda;                           // Разница между двумя значениями долготы
            double fdPhi;                           // Разница между двумя значениями широты
            double fAlpha;                           // Смещение
            double fRho;                           // Меридианский радиус кривизны
            double fNu;                           // Поперечный радиус кривизны
            double fR;                           // Радиус сферы Земли
            double fz;                           // Угловое расстояние от центра сфероида
            double fTemp;                           // Временная переменная, использующаяся в вычислениях

            // Вычисляем разницу между двумя долготами и широтами и получаем среднюю широту
            // предположительно что расстояние между точками << радиуса земли
            if (!radians)
            {
                fdLambda = (StartLong - EndLong) * D2R;
                fdPhi = (StartLat - EndLat) * D2R;
                fPhimean = ((StartLat + EndLat) / 2) * D2R;
            }
            else
            {
                fdLambda = StartLong - EndLong;
                fdPhi = StartLat - EndLat;
                fPhimean = (StartLat + EndLat) / 2;
            };

            // Вычисляем меридианные и поперечные радиусы кривизны средней широты
            fTemp = 1 - e2 * (sqr(Math.Sin(fPhimean)));
            fRho = (a * (1 - e2)) / Math.Pow(fTemp, 1.5);
            fNu = a / (Math.Sqrt(1 - e2 * (Math.Sin(fPhimean) * Math.Sin(fPhimean))));

            // Вычисляем угловое расстояние
            if (!radians)
            {
                fz = Math.Sqrt(sqr(Math.Sin(fdPhi / 2.0)) + Math.Cos(EndLat * D2R) * Math.Cos(StartLat * D2R) * sqr(Math.Sin(fdLambda / 2.0)));
            }
            else
            {
                fz = Math.Sqrt(sqr(Math.Sin(fdPhi / 2.0)) + Math.Cos(EndLat) * Math.Cos(StartLat) * sqr(Math.Sin(fdLambda / 2.0)));
            };
            fz = 2 * Math.Asin(fz);

            // Вычисляем смещение
            if (!radians)
            {
                fAlpha = Math.Cos(EndLat * D2R) * Math.Sin(fdLambda) * 1 / Math.Sin(fz);
            }
            else
            {
                fAlpha = Math.Cos(EndLat) * Math.Sin(fdLambda) * 1 / Math.Sin(fz);
            };
            fAlpha = Math.Asin(fAlpha);

            // Вычисляем радиус Земли
            fR = (fRho * fNu) / (fRho * sqr(Math.Sin(fAlpha)) + fNu * sqr(Math.Cos(fAlpha)));
            // Получаем расстояние
            return (uint)Math.Round(Math.Abs(fz * fR));
        }
        public static uint GetLengthMetersB(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double fPhimean, fdLambda, fdPhi, fAlpha, fRho, fNu, fR, fz, fTemp, Distance,
                D2R = Math.PI / 180,
                a = 6378137.0,
                e2 = 0.006739496742337;
            if (radians) D2R = 1;

            fdLambda = (StartLong - EndLong) * D2R;
            fdPhi = (StartLat - EndLat) * D2R;
            fPhimean = (StartLat + EndLat) / 2.0 * D2R;

            fTemp = 1 - e2 * Math.Pow(Math.Sin(fPhimean), 2);
            fRho = a * (1 - e2) / Math.Pow(fTemp, 1.5);
            fNu = a / Math.Sqrt(1 - e2 * Math.Sin(fPhimean) * Math.Sin(fPhimean));

            fz = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(fdPhi / 2.0), 2) +
              Math.Cos(EndLat * D2R) * Math.Cos(StartLat * D2R) * Math.Pow(Math.Sin(fdLambda / 2.0), 2)));
            fAlpha = Math.Asin(Math.Cos(EndLat * D2R) * Math.Sin(fdLambda) / Math.Sin(fz));
            fR = fRho * fNu / (fRho * Math.Pow(Math.Sin(fAlpha), 2) + fNu * Math.Pow(Math.Cos(fAlpha), 2));
            Distance = fz * fR;

            return (uint)Math.Round(Distance);
        }
        public static uint GetLengthMetersC(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
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
        public static double GetLengthMetersD(double sLat, double sLon, double eLat, double eLon, bool radians)
        {
            double EarthRadius = 6378137.0;

            double lon1 = radians ? sLon : deg2rad(sLon);
            double lon2 = radians ? eLon : deg2rad(eLon);
            double lat1 = radians ? sLat : deg2rad(sLat);
            double lat2 = radians ? eLat : deg2rad(eLat);

            return EarthRadius * (Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2)));
        }
        public static double GetLengthMetersE(double sLat, double sLon, double eLat, double eLon, bool radians)
        {
            double EarthRadius = 6378137.0;

            double lon1 = radians ? sLon : deg2rad(sLon);
            double lon2 = radians ? eLon : deg2rad(eLon);
            double lat1 = radians ? sLat : deg2rad(sLat);
            double lat2 = radians ? eLat : deg2rad(eLat);

            /* This algorithm is called Sinnott's Formula */
            double dlon = (lon2) - (lon1);
            double dlat = (lat2) - (lat1);
            double a = Math.Pow(Math.Sin(dlat / 2), 2.0) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dlon / 2), 2.0);
            double c = 2 * Math.Asin(Math.Sqrt(a));
            return EarthRadius * c;
        }
        private static double sqr(double val)
        {
            return val * val;
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

    /// <summary>
    ///     Нарезка тайлов в несколько потоков
    /// </summary>
    public class MultithreadTileRenderer // : MarshalByRefObject // for Lifetime Services
    {        
        /// <summary>
        ///     Число потоков
        /// </summary>
        public int multithreadCount = 8;

        /// <summary>
        ///     При ошибках отрисовки пробывать отрисовать зону повторно
        /// </summary>
        public bool addToQueueAgainIfError = true;

        /// <summary>
        ///     Перезагружатьфайл карты при каждой нарезке зоны
        /// </summary>
        public bool reloadMapEachZone = false;

        /// <summary>
        ///     Размер перекрытия зоны за границей изображения
        /// </summary>
        public ushort tileZoneOverSize = 128;

        /// <summary>
        ///     Добавлять ли копирайт?
        /// </summary>
        public string addCopyrightsFile = null;

        /// <summary>
        ///     Обрабатывать тайлы только попадающие в полигон
        /// </summary>
        public string renderOnlyInPolygon = null;


        /// <summary>
        ///     Структура папок хранения тайлов
        /// </summary>
        public TileRenderPathScheme pathStructure = TileRenderPathScheme.fsSlippyMap;

        /// <summary>
        ///     Размер карты при отрисовке
        /// </summary>
        public TileRenderZoneSize tileZoneSize = TileRenderZoneSize.tileZone_8x8;
        private int tileZoneSizeDiv = 8;
        private int tileZoneSizeCount = 8 * 8;

        /// <summary>
        ///     Сохранение тайлов при отрисовке
        /// </summary>
        public TileRenderZoneMode tileZoneMode = TileRenderZoneMode.keepFullZone;

        /// <summary>
        ///     Обрабатывать пустые тайлы
        /// </summary>
        public TileRenderEmptyData processEmptyTiles = TileRenderEmptyData.saveActualTiles;

        /// <summary>
        ///     Оптимизировать ли тайлы
        /// </summary>
        public TileOptimizeMethod tileOptimizeMethod = TileOptimizeMethod.none;

        /// <summary>
        ///     Формат PNG
        /// </summary>
        public byte tileOptimizeFormat = 0;

        /// <summary>
        ///     Перезаписывать ли существующий тайл
        /// </summary>
        public TileFinalMerging tileFinalMerging = TileFinalMerging.overwriteExistingFile;

        /// <summary>
        ///     Число тайлов для нарезки
        /// </summary>
        public ulong status_total_to_render = 0;

        /// <summary>
        ///     Число нарезанных и сохраненных тайлов
        /// </summary>
        public ulong status_total_created = 0;

        /// <summary>
        ///     Размер нарезанных тайлов
        /// </summary>
        public double status_total_size = 0;

        /// <summary>
        ///     Число нарезанных и несохраненных тайлов
        /// </summary>
        public ulong status_total_skipped = 0;
        /// <summary>
        ///     Число тайлов, которые не удалось сохранить
        /// </summary>
        public ulong status_total_witherr = 0;
        /// <summary>
        ///     Число зон, в которых есть дыры
        /// </summary>
        public ulong status_total_zoneerr = 0;

        /// <summary>
        ///     Число нарезннаых тайлов (в т.ч. пустых)
        /// </summary>
        public ulong status_total_passed = 0;

        /// <summary>
        ///     X Tile Started to Render
        /// </summary>
        public int status_x_start = 0;
        /// <summary>
        ///     Y Tile Started to Render
        /// </summary>
        public int status_x_current = 0;
        /// <summary>
        ///     X Tile Current to Render
        /// </summary>
        public int status_x_end = 0;
        /// <summary>
        ///     Y Tile Current to Render
        /// </summary>
        public int status_y_start = 0;
        /// <summary>
        ///     X Tile Finish to Render
        /// </summary>
        public int status_y_current = 0;
        /// <summary>
        ///     Y Tile Finish to Render
        /// </summary>
        public int status_y_end = 0;

        public int[] last_enqueued = null;
        public int[] last_dequeued = null;

        [XmlIgnore]
        public RenderView VIEW = null;

        public static string holes_fileName = "";

        /// <summary>
        ///     Объекты потоков
        /// </summary>
        private ThreadRenderObject[] rdata = null;

        /// <summary>
        ///     Потоки
        /// </summary>
        private System.Threading.Thread[] threads = null;

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        public static extern bool TerminateThread(IntPtr hThread, uint dwExitCode);

        /// <summary>
        ///     Остановить все потоки рендеринга
        /// </summary>
        public void BreakThreads()
        {
            BreakThreads(false, 0);
        }

        /// <summary>
        ///     Остановить все потоки рендеринга
        /// </summary>
        /// <param name="terminate">Убивать поток принудительно?</param>
        public void BreakThreads(bool terminate, int millisecondsTimeout)
        {
            queueBreaked = terminate ? (byte)2 : (byte)1;            
        }

        /// <summary>
        ///     Получаем список системных ThreadID 
        ///     для всех потоков нарезки, кроме того, который добавляет в очередь
        /// </summary>
        /// <returns></returns>
        public uint[] GetMultitreadThreadIDs()
        {
            if (threads == null) return new uint[0];
            if (threads.Length == 0) return new uint[0];

            List<uint> threadIDs = new List<uint>();
            for (int i = 0; i < threads.Length; i++)
                if (threads[i] != null)
                    threadIDs.Add(rdata[i].threadID);

            return threadIDs.ToArray();
        }

        /// <summary>
        ///     Получаем список системных ThreadID 
        ///     для всех потоков нарезки, кроме того, который добавляет в очередь
        /// </summary>
        /// <returns></returns>
        public Thread[] GetMultitreadThreads()
        {            
            if (threads == null) return new Thread[0];
            if (threads.Length == 0) return new Thread[0];
            return threads;
        }

        /// <summary>
        ///     Получаем список системных ThreadID 
        ///     для всех потоков нарезки, кроме того, который добавляет в очередь
        /// </summary>
        /// <returns></returns>
        internal ThreadRenderObject[] GetMultitreadThreadInfo()
        {
            if (rdata == null) return new ThreadRenderObject[0];
            if (rdata.Length == 0) return new ThreadRenderObject[0];
            return rdata;
        }

        /// <summary>
        ///     Очередь
        /// </summary>
        private Queue<int[]> queue = new Queue<int[]>();
        private ulong queueTotal = 0;
        private byte queueBreaked = 0; // not breaked // 1 - wait // 2 - terminate
        private System.Threading.Mutex queueMtx = new System.Threading.Mutex();

        /// <summary>
        ///     Папка с тайлами
        /// </summary>
        private string tiles_dir;

        /// <summary>
        ///     Папка с тайлами
        /// </summary>
        public string tilesDirectory { get { return tiles_dir; } set { tiles_dir = value; } }

        /// <summary>
        ///     Файл карты
        /// </summary>
        private string mapfile;

        /// <summary>
        ///     Файл дампа нарезки
        /// </summary>
        public string loadDumpFile = null;

        /// <summary>
        ///     Файл карты
        /// </summary>
        public string mapFileName { get { return mapfile; } }

        /// <summary>
        ///     Нарезка тайлов в несколько потоков
        /// </summary>
        /// <param name="mapfile">XML-файл карты</param>
        /// <param name="tiles_dir">Путь к папке хранения тайлов</param>
        public MultithreadTileRenderer(string mapfile, string tiles_dir)
        {
            this.mapfile = mapfile;
            this.tiles_dir = tiles_dir;
        }

        /// <summary>
        ///     Нарезка тайлов в несколько потоков
        /// </summary>
        /// <param name="mapfile">XML-файл карты</param>
        /// <param name="tiles_dir">Путь к папке хранения тайлов</param>
        /// <param name="config">Конфигурация</param>
        public MultithreadTileRenderer(string mapfile, string tiles_dir, TileRendererConfig config)
        {
            this.mapfile = mapfile;
            this.tiles_dir = tiles_dir;
            this.SetConfig(config);
        }

        /// <summary>
        ///     Нарезка тайлов в несколько потоков
        /// </summary>
        /// <param name="mapfile">XML-файл карты</param>
        /// <param name="tiles_dir">Путь к папке хранения тайлов</param>
        /// <param name="config">Конфигурация</param>
        public MultithreadTileRenderer(string mapfile, string tiles_dir, TileRendererConfig config, string loadDumpFile)
        {
            this.mapfile = mapfile;
            this.tiles_dir = tiles_dir;
            this.SetConfig(config);
            this.loadDumpFile = loadDumpFile;
        }

        /// <summary>
        ///     Установка параметров
        /// </summary>
        /// <param name="config"></param>
        public void SetConfig(TileRendererConfig config)
        {
            multithreadCount = config.multithreadCount;
            addToQueueAgainIfError = config.addToQueueAgainIfError;
            reloadMapEachZone = config.reloadMapEachZone;
            tileZoneOverSize = config.tileZoneOverSize;
            addCopyrightsFile = config.addCopyrightsFile;
            renderOnlyInPolygon = config.renderOnlyInPolygon;
            pathStructure = config.pathStructure;
            tileZoneSize = config.tileZoneSize;
            tileZoneSizeDiv = (int)config.tileZoneSize;
            tileZoneSizeCount = tileZoneSizeDiv * tileZoneSizeDiv;
            tileZoneMode = config.tileZoneMode;
            processEmptyTiles = config.processEmptyTiles;
            tileOptimizeMethod = config.tileOptimizeMethod;
            tileOptimizeFormat = config.tileOptimizeFormat;
            tileFinalMerging = config.tileFinalMerging;
        }

        /// <summary>
        ///     Параметры
        /// </summary>
        public TileRendererConfig GetConfig()
        {
            TileRendererConfig config = new TileRendererConfig();
            config.multithreadCount = multithreadCount;
            config.addToQueueAgainIfError = addToQueueAgainIfError;
            config.reloadMapEachZone = reloadMapEachZone;
            config.tileZoneOverSize = tileZoneOverSize;
            config.addCopyrightsFile = addCopyrightsFile;
            config.renderOnlyInPolygon = renderOnlyInPolygon;
            config.pathStructure = pathStructure;
            config.tileZoneSize = tileZoneSize;
            config.tileZoneMode = tileZoneMode;
            config.processEmptyTiles = processEmptyTiles;
            config.tileOptimizeMethod = tileOptimizeMethod;
            config.tileOptimizeFormat = tileOptimizeFormat;
            config.tileFinalMerging = tileFinalMerging;
            return config;
        }

        /// <summary>
        ///     Нарезка тайлов для определенного уровня
        /// </summary>
        /// <param name="startLat">начальная широта (сверху)</param>
        /// <param name="startLon">начальная долгота (слева)</param>
        /// <param name="endLat">конечная широта (снизу)</param>
        /// <param name="endLon">конечная долгота (справа)</param>
        /// <param name="zoom">Масштабный уровень</param>
        public void RenderTiles(double startLat, double startLon, double endLat, double endLon, int zoom)
        {
            if ((startLat > 90) || (startLat < -90)) return;
            if ((startLon < -180) || (startLon > 180)) return;
            if ((endLat < -90) || (endLat > 90)) return;
            if ((endLon > 180) || (endLon < -180)) return;

            double[] px0 = TilesProjection.fromLLtoPixel(new double[] { startLon, startLat }, zoom);
            double[] px1 = TilesProjection.fromLLtoPixel(new double[] { endLon, endLat }, zoom);

            RenderTiles((int)(px0[0] / 256.0), (int)(px0[1] / 256.0), (int)(px1[0] / 256.0), (int)(px1[1] / 256.0), zoom);
        }

        /// <summary>
        ///     Нарезка тайлов для определенного уровня    
        /// </summary>
        /// <param name="startX">start tile X</param>
        /// <param name="startY">start tile Y</param>
        /// <param name="endX">end tile X</param>
        /// <param name="endY">end tile Y</param>
        /// <param name="zoom">zoom level</param>
        public void RenderTiles(int startX, int startY, int endX, int endY, int zoom)
        {
            if ((zoom < TileRenderer.minZoom) || (zoom > TileRenderer.maxZoom)) return;

            // Prepare
            queue.Clear();
            queueTotal = 0;
            queueBreaked = 0;

            // Calculate MinMax
            int maxtilesinline = (int)Math.Pow(2, zoom);
            int min_x = status_x_start = Math.Max(0, startX);
            int max_x = status_x_end = Math.Min(maxtilesinline, endX - (endX % tileZoneSizeDiv) + tileZoneSizeDiv - 1);
            int min_y = status_y_start = Math.Max(0, startY);
            int max_y = status_y_end = Math.Min(maxtilesinline, endY - (endY % tileZoneSizeDiv) + tileZoneSizeDiv - 1);

            if (TileRenderingErrors.WithDump)
            {
                // SAVING REDERER MATRIX IN-MEM
                VIEW = new RenderView(min_x, max_x, min_y, max_y, tileZoneSizeDiv, zoom);

                // LOAD RENDERER MATRIX FROM FILE
                if ((!String.IsNullOrEmpty(this.loadDumpFile)) && (File.Exists(this.loadDumpFile)))
                {
                    FileStream fs = new FileStream(this.loadDumpFile, FileMode.Open, FileAccess.Read);
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
                                // ZOOM NO byte, zoneSize byte, zone  xmin int, xmax int, ymin int, ymax int, ZOOM size int, DATA byte[]
                                byte zm = (byte)fs.ReadByte();
                                byte sqsz = (byte)fs.ReadByte();
                                byte[] zms = new byte[4];

                                fs.Read(zms, 0, zms.Length);
                                if (BitConverter.IsLittleEndian) Array.Reverse(zms);
                                int xf = BitConverter.ToInt32(zms, 0);

                                fs.Read(zms, 0, zms.Length);
                                if (BitConverter.IsLittleEndian) Array.Reverse(zms);
                                int xt = BitConverter.ToInt32(zms, 0);

                                fs.Read(zms, 0, zms.Length);
                                if (BitConverter.IsLittleEndian) Array.Reverse(zms);
                                int yf = BitConverter.ToInt32(zms, 0);

                                fs.Read(zms, 0, zms.Length);
                                if (BitConverter.IsLittleEndian) Array.Reverse(zms);
                                int yt = BitConverter.ToInt32(zms, 0);

                                fs.Read(zms, 0, zms.Length);
                                if (BitConverter.IsLittleEndian) Array.Reverse(zms);
                                int zlength = BitConverter.ToInt32(zms, 0);

                                if (zm != zoom)
                                    fs.Position += zlength;
                                else if ((VIEW.Size == (zlength + 22)) && (sqsz == tileZoneSizeDiv) && (status_x_start == xf) && (status_x_end == xt) && (status_y_start == yf) && (status_y_end == yt))
                                {
                                    for (int i = 22; i < VIEW.Size; i++)
                                    {
                                        int b = fs.ReadByte();
                                        VIEW.SetByte(i, (byte)b);
                                    };
                                };
                            };

                        };
                    };
                    fs.Close();
                };
            };

            // prepare status
            status_total_to_render = (ulong)(tileZoneSizeCount * (((int)(max_x / tileZoneSizeDiv) - (int)(min_x / tileZoneSizeDiv) + 1) * ((int)(max_y / tileZoneSizeDiv) - (int)(min_y / tileZoneSizeDiv) + 1)));
            status_total_created = 0;
            status_total_passed = 0;
            status_total_size = 0;

            //// old calculate status_total_to_render
            //for (int x = min_x; x <= max_x; x += tileZoneDiv)
            //{
            //    if ((x >= maxtilesinline)) continue; // Validate x tile number
            //    for (int y = min_y; y <= max_y; y += tileZoneDiv)
            //    {
            //        if ((y >= maxtilesinline)) continue; // Validate x tile number
            //        status_total_to_render += (ulong)(tileZoneDiv * tileZoneDiv);
            //    };
            //};

            // Create and Start Threads
            rdata = new ThreadRenderObject[multithreadCount];
            threads = new System.Threading.Thread[multithreadCount];
            int queueMax = multithreadCount * 3;
            TileRendererConfig cfg = this.GetConfig();
            for (int i = 0; i < multithreadCount; i++)
            {
                string[] mfs = this.mapfile.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries);
                rdata[i] = new ThreadRenderObject();
                rdata[i].renderer = new TileRenderer(mfs[i % mfs.Length], this.tiles_dir, cfg);
                rdata[i].Active = true;
                threads[i] = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(thread_render));
                threads[i].Priority = ThreadPriority.AboveNormal;
                threads[i].Name = "TileRendererThread_Zoom_"+zoom.ToString()+"_No_" + i.ToString();
                threads[i].Start(rdata[i]);
            };            

            //ADD TO QUEUE            
            for (int x = min_x; x <= max_x; x += tileZoneSizeDiv)
                for (int y = min_y; y <= max_y; y += tileZoneSizeDiv)
                {
                    // добавляем в очередь
                    bool added = false;
                    while (!added)
                    {
                        if (queueBreaked > 0) // прерываем нарезку
                        {
                            WaitForRenderThreads();
                            return;
                        };                        
                        if (queue.Count < queueMax)
                        {
                            int[] q = new int[] { x, y, zoom };
                            // SKIP IF DONE
                            byte czs = 0;
                            if((VIEW != null) && ((czs = VIEW.Get(x,y)) > 3) && ((czs == 4) || (czs == 5) || (czs == 8)))
                            { // квадрат нарезан без дыр и ошибок
                                // обновляем статистику
                                ulong passed = (ulong)tileZoneSizeCount;
                                status_total_skipped += passed;
                                status_total_passed += passed;
                                queueTotal--;
                            }
                            else
                            {
                                queueMtx.WaitOne();
                                queue.Enqueue(q);
                                queueMtx.ReleaseMutex();
                                last_enqueued = q;
                                if (VIEW != null) VIEW.Set(x, y, 1);
                            };                            
                            added = true;
                            queueTotal++;
                        };                        
                        if (!added)
                            System.Threading.Thread.Sleep(5);
                    };
                };

            // Ждем пока все потоки не разберут объекты из очереди и не закончат над ними работу
            bool isZero = false;
            while (!isZero)
            {                
                queueMtx.WaitOne();
                if (queueBreaked > 0) // прерываем нарезку
                {
                    queue.Clear();
                    queueTotal = 0;
                };
                if ((queue.Count == 0) && (queueTotal == 0))
                    isZero = true;
                queueMtx.ReleaseMutex();
                System.Threading.Thread.Sleep(5);
            };

            WaitForRenderThreads();
        }

        /// <summary>
        ///     Нарезка тайлов для определенного уровня    
        /// </summary>
        public void RenderTilesList(int[][] tilelist, int zoom)
        {
            if ((zoom < TileRenderer.minZoom) || (zoom > TileRenderer.maxZoom)) return;

            // Prepare
            queue.Clear();
            queueTotal = 0;
            queueBreaked = 0;

            status_x_start = tilelist[0][0];
            status_x_end = tilelist[0][0];
            status_y_start = tilelist[0][1];
            status_y_end = tilelist[0][1];
            for (int i = 0; i < tilelist.Length; i++)
            {
                status_x_start = Math.Min(status_x_start,tilelist[i][0]);
                status_x_end = Math.Max(status_x_start,tilelist[i][0]);
                status_y_start = Math.Min(status_y_start,tilelist[i][1]);
                status_y_end = Math.Max(status_y_start,tilelist[i][1]);
            };

            VIEW = new RenderView(status_x_start, status_x_end, status_y_start, status_y_end, (int)tileZoneSize, zoom);

            // prepare status
            int tzs = (int)tileZoneSize;
            tzs = tzs * tzs;
            status_total_to_render = (ulong)tilelist.Length * (ulong)tzs;
            status_total_created = 0;
            status_total_passed = 0;
            status_total_size = 0;

            // Create and Start Threads
            rdata = new ThreadRenderObject[multithreadCount];
            threads = new System.Threading.Thread[multithreadCount];
            int queueMax = multithreadCount * 3;
            TileRendererConfig cfg = this.GetConfig();
            for (int i = 0; i < multithreadCount; i++)
            {
                string[] mfs = this.mapfile.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries);
                rdata[i] = new ThreadRenderObject();
                rdata[i].renderer = new TileRenderer(mfs[i % mfs.Length], this.tiles_dir, cfg);
                rdata[i].Active = true;
                threads[i] = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(thread_render));
                threads[i].Priority = ThreadPriority.AboveNormal;
                threads[i].Name = "TileRendererThread_Zoom_" + zoom.ToString() + "_No_" + i.ToString();
                threads[i].Start(rdata[i]);
            };

            //ADD TO QUEUE
            for (int i = 0; i < tilelist.Length; i++ )
            {
                // добавляем в очередь
                bool added = false;
                while (!added)
                {
                    if (queueBreaked > 0) // прерываем нарезку
                    {
                        WaitForRenderThreads();
                        return;
                    };
                    queueMtx.WaitOne();
                    if (queue.Count < queueMax)
                    {
                        queue.Enqueue(tilelist[i]);
                        last_enqueued = tilelist[i];
                        if (VIEW != null) VIEW.Set(tilelist[i][0], tilelist[i][1], 1);
                        added = true;
                        queueTotal++;
                    };
                    queueMtx.ReleaseMutex();
                    System.Threading.Thread.Sleep(5);
                };
            };

            // Ждем пока все потоки не разберут объекты из очереди и не закончат над ними работу
            bool isZero = false;
            while (!isZero)
            {
                queueMtx.WaitOne();
                if (queueBreaked > 0) // прерываем нарезку
                {
                    queue.Clear();
                    queueTotal = 0;
                };
                if ((queue.Count == 0) && (queueTotal == 0))
                    isZero = true;
                queueMtx.ReleaseMutex();
                System.Threading.Thread.Sleep(5);
            };

            WaitForRenderThreads();
        }

        /// <summary>
        ///   Ждем пока все потоки не разберут объекты из очереди и не закончат над ними работу
        /// </summary>
        private void WaitForRenderThreads()
        {
            //
            //  queueBreaked == 2 - Terminate Threads
            //

            if (queueBreaked > 1)
            {
                // Завершаем потоки и Освобождаем ресурсы 
                for (int i = 0; i < multithreadCount; i++)
                {
                    // позволяем потоку нормально завершиться
                    if (rdata[i] != null)
                    {
                        rdata[i].Active = false;
                        if (rdata[i].renderer.map != null)
                            rdata[i].renderer.map.Dispose();
                        rdata[i].renderer.map = null;
                        if (rdata[i].renderer.map_proj != null)
                            rdata[i].renderer.map_proj.Dispose();
                        rdata[i].renderer.map_proj = null;
                    };
                    if ((threads != null) && (threads[i] != null))
                    {
                        threads[i].Join(50); // Ждем пока поток самостоятельно завершится
                        if ((rdata[i] != null) && (rdata[i].threadID != 0))
                            try { TerminateThread(OpenThread(1, false, rdata[i].threadID), 1); }
                            catch { };
                        threads[i] = null;
                    };
                    if (rdata[i] != null)
                    {
                        rdata[i] = null;
                    };
                };
                threads = null;
                return;
            };

            //
            // queueBreaked == 0 || queueBreaked == 1 - wait normal
            //            

            if(true)
            {
                // Завершаем потоки и Освобождаем ресурсы 
                for (int i = 0; i < multithreadCount; i++)
                {
                    // позволяем потоку нормально завершиться
                    if (rdata[i] != null) rdata[i].Active = false;
                    if ((threads != null) && (threads[i] != null))
                    {
                        threads[i].Join(); // Ждем пока поток самостоятельно завершится
                        threads[i] = null;
                    };
                    if (rdata[i] != null)
                    {
                        if (rdata[i].renderer.map != null)
                            rdata[i].renderer.map.Dispose();
                        rdata[i].renderer.map = null;
                        if (rdata[i].renderer.map_proj != null)
                            rdata[i].renderer.map_proj.Dispose();
                        rdata[i].renderer.map_proj = null;
                        rdata[i].renderer = null;
                        rdata[i] = null;
                    };
                };
                threads = null;
            };
        }

        /// <summary>
        ///     Число тайлов для нарезки определенного уровня
        /// </summary>
        /// <param name="startLat">начальная широта (сверху)</param>
        /// <param name="startLon">начальная долгота (слева)</param>
        /// <param name="endLat">конечная широта (снизу)</param>
        /// <param name="endLon">конечная долгота (справа)</param>
        /// <param name="zoom">Масштабный уровень</param>
        public static ulong RenderTilesCount(double startLat, double startLon, double endLat, double endLon, int zoom, TileRenderZoneSize tileZoneSize)
        {
            if ((startLat > 90) || (startLat < -90)) return 0;
            if ((startLon < -180) || (startLon > 180)) return 0;
            if ((endLat < -90) || (endLat > 90)) return 0;
            if ((endLon > 180) || (endLon < -180)) return 0;

            double[] px0 = TilesProjection.fromLLtoPixel(new double[] { startLon, startLat }, zoom);
            double[] px1 = TilesProjection.fromLLtoPixel(new double[] { endLon, endLat }, zoom);

            return RenderTilesCount((int)(px0[0] / 256.0), (int)(px0[1] / 256.0), (int)(px1[0] / 256.0), (int)(px1[1] / 256.0), zoom, tileZoneSize);
        }

        /// <summary>
        ///     Число тайлов для нарезки определенного уровня
        /// </summary>
        /// <param name="startX">start tile X</param>
        /// <param name="startY">start tile Y</param>
        /// <param name="endX">end tile X</param>
        /// <param name="endY">end tile Y</param>
        /// <param name="zoom">zoom level</param>
        public static ulong RenderTilesCount(int startX, int startY, int endX, int endY, int zoom, TileRenderZoneSize tileZoneSize)
        {
            if ((zoom < TileRenderer.minZoom) || (zoom > TileRenderer.maxZoom)) return 0;

            int tileZoneDiv = (int)(tileZoneSize);
            int maxtilesinline = (int)Math.Pow(2, zoom);
            int min_x = Math.Max(0, startX);
            int max_x = Math.Min(maxtilesinline, endX - (endX % tileZoneDiv) + tileZoneDiv - 1);
            int min_y = Math.Max(0, startY);
            int max_y = Math.Min(maxtilesinline, endY - (endY % tileZoneDiv) + tileZoneDiv - 1);

            return (ulong)(tileZoneDiv * tileZoneDiv * (((int)(max_x / tileZoneDiv) - (int)(min_x / tileZoneDiv) + 1) * ((int)(max_y / tileZoneDiv) - (int)(min_y / tileZoneDiv) + 1)));
            
            //// old calculate status_total_to_render
            //ulong total_to_render = 0;
            //for (int x = min_x; x <= max_x; x += tileZoneDiv)
            //{
            //    for (int y = min_y; y <= max_y; y += tileZoneDiv)
            //    {
            //        total_to_render += (ulong)(tileZoneDiv * tileZoneDiv);
            //    };
            //};
            //return total_to_render;
        }

        //// MarshalByRefObject
        //public override object InitializeLifetimeService()
        //{
        //    return null;
        //}        

        internal class ThreadRenderObject
        {
            [XmlIgnore]
            /// <summary>
            ///     Single Renderer
            /// </summary>
            public TileRenderer renderer;
            /// <summary>
            ///     Активный ли поток (если нет, то поток завершается)
            /// </summary>
            public bool Active;
            /// <summary>
            ///     Сколько тайлов сохранено
            /// </summary>
            public ulong tilesCreated = 0;
            /// <summary>
            ///     Сколько тайлов пропущено (т.к. уже существует такой файл)
            /// </summary>
            public ulong tilesSkipped = 0;
            /// <summary>
            ///     Сколько тайлов не удалось сохранить
            /// </summary>
            public ulong tilesWithErr = 0;
            /// <summary>
            ///     В скольких зонах дыры
            /// </summary>
            public ulong tilesZoneErr = 0;
            /// <summary>
            ///     Сколько файлов оработано (в т.ч. пустых)
            /// </summary>
            public ulong tilesPassed = 0;
            /// <summary>
            ///     Сколько зон обработано
            /// </summary>
            public ulong zonesPassed = 0;
            /// <summary>
            ///     Последние отрисованные тайлы {x,y,z};
            /// </summary>
            public int[] lastData;
            public int[] nowData;
            /// <summary>
            ///     ThreadID потока в системе
            /// </summary>
            public uint threadID = 0;
            /// <summary>
            ///     Общий размер тайлов
            /// </summary>
            public double tilesSize = 0;
        }

        /// <summary>
        ///     Основной код потока
        /// </summary>
        /// <param name="tr">ThreadRenderObject объект потока</param>
        private void thread_render(object tr)
        {            
            ThreadRenderObject data = (ThreadRenderObject)tr;
            data.threadID = GetCurrentThreadId(); // запоминаем ThreadID потока в системе

            // пока поток рабочий
            while (data.Active)
            {
                // получаем параметры тайлов из очереди
                int[] p = null;
                queueMtx.WaitOne();
                if (queue.Count > 0)
                {
                    p = queue.Dequeue();
                    last_dequeued = p;
                    if (VIEW != null) VIEW.Set(p[0], p[1], 2);
                };
                queueMtx.ReleaseMutex();

                // если парамеры получены, рисуем
                if (p != null)
                {
                    // обновляем статус
                    data.nowData = p;
                    status_x_current = p[0];
                    status_y_current = p[1];

                    // отрисовываем тайлы
                    // [tiles_created, skipped, with_errors, tiles_size, error_code 1 or 2]
                    int[] rResult = new int[]{0,0,0,0,4};


                    try
                    {
                        rResult = data.renderer.render_tile(p[0], p[1], p[2]);
                    }
                    catch (Exception ex)
                    {

                    };
                    
                    // обработка ошибок RENDER MAP ERROR и Error MapnicCS.Image.ToImage
                    // только в оконном и stand_alone
                    // если хотя бы одна дырка в зоне
                    if (((rResult[2] > 0) || (rResult[4] > 0)) && this.addToQueueAgainIfError)
                    {
                        if (VIEW != null) VIEW.Set(p[0], p[1], 3);
                        // При ошибках отрисовки пробывать отрисовать зону повторно
                        // [tiles_created, skipped, with_errors, tiles_size, error_code 1 or 2]
                        rResult = data.renderer.render_tile(p[0], p[1], p[2]);

                        // добавляем зону в очередь заново
                        //queueMtx.WaitOne(); 
                        //queue.Enqueue(p);sf
                        //queueMtx.ReleaseMutex();
                    };

                    if (VIEW != null)
                    {
                        VIEW.Set(p[0], p[1], 10);
                        if ((rResult[0] > 0) && (rResult[1] == 0) && (rResult[2] == 0) && (rResult[4] == 0)) VIEW.Set(p[0], p[1], 4);
                        if ((rResult[0] > 0) && (rResult[1] > 0) && (rResult[2] == 0) && (rResult[4] == 0)) VIEW.Set(p[0], p[1], 5);
                        if ((rResult[0] > 0) && (rResult[1] == 0) && (rResult[2] > 0)) VIEW.Set(p[0], p[1], 6);
                        if ((rResult[0] > 0) && (rResult[1] > 0) && (rResult[2] > 0)) VIEW.Set(p[0], p[1], 7);
                        if ((rResult[0] == 0) && (rResult[1] > 0) && (rResult[2] == 0)) VIEW.Set(p[0], p[1], 8);
                        if ((rResult[0] == 0) && (rResult[1] == 0) && (rResult[2] > 0)) VIEW.Set(p[0], p[1], 9);
                    };

                    // обновляем статистику
                    ulong created = (ulong)rResult[0];
                    ulong skipped = (ulong)rResult[1];
                    ulong witherr = (ulong)rResult[2];
                    ulong crfsize = (ulong)rResult[3];
                    data.tilesCreated += created;                    
                    data.tilesSkipped += skipped;
                    data.tilesWithErr += witherr;
                    if(witherr > 0) data.tilesZoneErr++;
                    data.tilesSize += crfsize;
                    status_total_created += created;
                    status_total_skipped += skipped;
                    status_total_witherr += witherr;
                    status_total_zoneerr += witherr > 0 ? (ulong)1 : (ulong)0;
                    status_total_size += crfsize;
                    ulong passed = (ulong)tileZoneSizeCount;
                    data.tilesPassed += passed;
                    data.zonesPassed++;
                    status_total_passed += passed;
                    data.lastData = p;
                    queueTotal--;

                    if (witherr > 0) Add2HolesLog(p);
                };

                System.Threading.Thread.Sleep(1);

                //PAUSE NORMAL
                if (TileRendering.TileRenderingErrors.PauseNormal)
                {
                    TileRendering.TileRenderingErrors.PausedNormal++;
                    while (TileRendering.TileRenderingErrors.PauseNormal) Thread.Sleep(100);
                    TileRendering.TileRenderingErrors.PausedNormal--;
                };
            };
        }

        private static Mutex Add2HolesLogMtx = new Mutex();
        private static void Add2HolesLog(int[] p)
        {
            Add2HolesLogMtx.WaitOne();
            try
            {
                string txt = String.Format("ZN:{0},{1},{2};\r\n", new object[] { p[0], p[1], p[2] });
                System.IO.FileStream fs = new System.IO.FileStream(TileRendering.MultithreadTileRenderer.holes_fileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                byte[] data = System.Text.Encoding.GetEncoding(1251).GetBytes(txt);
                fs.Position = fs.Length;
                fs.Write(data, 0, data.Length);
                fs.Close();
            }
            catch { };
            Add2HolesLogMtx.ReleaseMutex();
        }
    }

    /// <summary>
    ///     Нарезка тайлов в один поток
    /// </summary>
    public class TileRenderer
    {
        private string mapfile = "";

        /// <summary>
        ///     Перезагружатьфайл карты при каждой нарезке зоны
        /// </summary>
        public bool reloadMapEachZone = false;

        // <summary>
        ///     Размер перекрытия зоны за границей изображения
        /// </summary>
        public ushort tileZoneOverSize = 128;

        /// <summary>
        ///     Добавлять ли копирайт?
        /// </summary>
        public string addCopyrightsFile = null;

        /// <summary>
        ///     Обрабатывать тайлы только попадающие в полигон
        /// </summary>
        public string renderOnlyInPolygon = null;

        /// <summary>
        ///     Структура папок хранения тайлов
        /// </summary>
        public TileRenderPathScheme pathStructure = TileRenderPathScheme.fsSlippyMap;

        /// <summary>
        ///     Размер карты при отрисовки
        /// </summary>
        public TileRenderZoneSize tileZoneSize = TileRenderZoneSize.tileZone_8x8;
        private int tileZoneSizeDiv = 8;
        private int tileZoneSizeCount = 8 * 8;

        /// <summary>
        ///     Сохранение тайлов при отрисовки
        /// </summary>
        public TileRenderZoneMode tileZoneMode = TileRenderZoneMode.keepFullZone;

        /// <summary>
        ///     Обрабатывать пустые тайлы
        /// </summary>
        public TileRenderEmptyData processEmptyTiles = TileRenderEmptyData.saveActualTiles;

        /// <summary>
        ///     Оптимизировать ли тайлы
        /// </summary>
        public TileOptimizeMethod tileOptimizeMethod = TileOptimizeMethod.none;

        /// <summary>
        ///     Формат PNG
        /// </summary>
        public byte tileOptimizeFormat = 0;

        /// <summary>
        ///     Перезаписывать ли существующий тайл
        /// </summary>
        public TileFinalMerging tileFinalMerging = TileFinalMerging.overwriteExistingFile;

        /// <summary>
        ///     Папка с тайлами
        /// </summary>
        public string tiles_dir;

        public MapnikCs.Map map = null;
        public MapnikCs.Projection map_proj = null;
        private List<int> dir_list = new List<int>();
        private System.Threading.Mutex dir_list_mtx = new System.Threading.Mutex();

        public const byte minZoom = 2;
        public const byte maxZoom = 19;

        public Image CopyrightWatermarkImage = null;
        public TileRenderShapeLimiter RenderInShape = null;

        /// <summary>
        ///     Инициализация папок и шрифтов mapnik
        /// </summary>
        static TileRenderer()
        {
            string cd = GetCurrentDir();
            MapnikCs.DatasourceCache.RegisterDatasources(cd + @"data_sources\");
            MapnikCs.FontEngine.RegisterFonts(cd + @"\fonts\", false);

            if (!System.IO.Directory.Exists(cd + @"\LOGS"))
                System.IO.Directory.CreateDirectory(cd + @"\LOGS");
        }

        /// <summary>
        ///     Добавляем шрифт в mapnik
        /// </summary>
        /// <param name="fontFileName"></param>
        public static void RegisterFont(string fontFileName)
        {
            MapnikCs.FontEngine.RegisterFont(fontFileName);
        }

        public TileRenderer(string mapfile, string tiles_dir)
        {
            this.mapfile = mapfile;
            this.tiles_dir = tiles_dir;
            this.map = new MapnikCs.Map(256 * (uint)tileZoneSizeDiv, 256 * (uint)tileZoneSizeDiv);
            this.map.BufferSize = 128;

            try
            {
                this.map.LoadFile(this.mapfile, true);                 
            }
            catch (Exception ex)
            {
                TileRenderingErrors.ErrorQueueMutex.WaitOne();
                TileRenderingErrors.ErrorQueue.Enqueue(new TileRenderingErrors.ErrorInfo(0,0,0,"Error Opening Map File",ex,mapfile));
                TileRenderingErrors.ErrorQueueTotalCount++;
                TileRenderingErrors.ErrorQueueMutex.ReleaseMutex();
                throw ex;
            };            

            this.map_proj = new MapnikCs.Projection(this.map.SpatialReference);
        }

        public TileRenderer(string mapfile, string tiles_dir, TileRendererConfig config)
        {
            this.mapfile = mapfile;
            this.SetConfig(config);
            this.tiles_dir = tiles_dir;
            if (!this.reloadMapEachZone)
            {
                this.map = new MapnikCs.Map(256 * (uint)tileZoneSize, 256 * (uint)tileZoneSize);
                this.map.BufferSize = this.tileZoneOverSize;
            };

            try
            {
                if (!this.reloadMapEachZone)
                    this.map.LoadFile(this.mapfile, true);
            }
            catch (Exception ex)
            {
                TileRenderingErrors.ErrorQueueMutex.WaitOne();
                TileRenderingErrors.ErrorQueue.Enqueue(new TileRenderingErrors.ErrorInfo(0, 0, 0, "Error Opening Map File", ex,mapfile));
                TileRenderingErrors.ErrorQueueTotalCount++;
                TileRenderingErrors.ErrorQueueMutex.ReleaseMutex();
                throw ex;
            };

            if (addCopyrightsFile != String.Empty)
            {
                try
                {
                    CopyrightWatermarkImage = Image.FromFile(addCopyrightsFile);
                }
                catch (Exception ex)
                {
                    TileRenderingErrors.ErrorQueueMutex.WaitOne();
                    TileRenderingErrors.ErrorQueue.Enqueue(new TileRenderingErrors.ErrorInfo(0, 0, 0, "Error Opening Watermark File", ex, addCopyrightsFile));
                    TileRenderingErrors.ErrorQueueTotalCount++;
                    TileRenderingErrors.ErrorQueueMutex.ReleaseMutex();
                    throw ex;
                };
            };

            if (renderOnlyInPolygon != String.Empty)
            {
                try
                {
                    RenderInShape = new TileRenderShapeLimiter(renderOnlyInPolygon);
                }
                catch (Exception ex)
                {
                    TileRenderingErrors.ErrorQueueMutex.WaitOne();
                    TileRenderingErrors.ErrorQueue.Enqueue(new TileRenderingErrors.ErrorInfo(0, 0, 0, "Error Opening Shape File", ex, renderOnlyInPolygon));
                    TileRenderingErrors.ErrorQueueTotalCount++;
                    TileRenderingErrors.ErrorQueueMutex.ReleaseMutex();
                    throw ex;
                };
            };

            if (!this.reloadMapEachZone)
                this.map_proj = new MapnikCs.Projection(this.map.SpatialReference);
        }

        /// <summary>
        ///     Установка параметров
        /// </summary>
        /// <param name="config"></param>
        public void SetConfig(TileRendererConfig config)
        {
            reloadMapEachZone = config.reloadMapEachZone;
            tileZoneOverSize = config.tileZoneOverSize;
            addCopyrightsFile = config.addCopyrightsFile;
            renderOnlyInPolygon = config.renderOnlyInPolygon;
            pathStructure = config.pathStructure;
            tileZoneSize = config.tileZoneSize;
            tileZoneSizeDiv = (int)config.tileZoneSize;
            tileZoneSizeCount = tileZoneSizeDiv * tileZoneSizeDiv;
            tileZoneMode = config.tileZoneMode;
            processEmptyTiles = config.processEmptyTiles;
            tileOptimizeMethod = config.tileOptimizeMethod;
            tileOptimizeFormat = config.tileOptimizeFormat;
            tileFinalMerging = config.tileFinalMerging;
        }

        /// <summary>
        ///     Параметры
        /// </summary>
        /// <returns></returns>
        public TileRendererConfig GetConfig()
        {
            TileRendererConfig config = new TileRendererConfig();
            config.tileZoneOverSize = tileZoneOverSize;
            config.addCopyrightsFile = addCopyrightsFile;
            config.renderOnlyInPolygon = renderOnlyInPolygon;
            config.pathStructure = pathStructure;
            config.tileZoneSize = tileZoneSize;
            config.tileZoneMode = tileZoneMode;
            config.processEmptyTiles = processEmptyTiles;
            config.tileOptimizeMethod = tileOptimizeMethod;
            config.tileOptimizeFormat = tileOptimizeFormat;
            config.tileFinalMerging = tileFinalMerging;
            return config;
        }

        /// <summary>
        ///     Отрисовка тайлов для указанного зума
        /// </summary>
        /// <param name="startLat">начальная широта (сверху)</param>
        /// <param name="startLon">начальная долгота (слева)</param>
        /// <param name="endLat">конечная широта (снизу)</param>
        /// <param name="endLon">конечная долгота (справа)</param>    
        /// <param name="zoom">зум</param>
        public void render_zoom(double startLat, double startLon, double endLat, double endLon, int zoom)
        {
            if ((startLat > 90) || (startLat < -90)) return;
            if ((startLon < -180) || (startLon > 180)) return;
            if ((endLat < -90) || (endLat > 90)) return;
            if ((endLon > 180) || (endLon < -180)) return;
            if ((zoom < minZoom) || (zoom > maxZoom)) return;   

            double[] px0 = TilesProjection.fromLLtoPixel(new double[] { startLon, startLat }, zoom);
            double[] px1 = TilesProjection.fromLLtoPixel(new double[] { endLon, endLat }, zoom);
            px0[0] /= 256.0; px0[1] /= 256.0; px1[0] /= 256.0; px1[1] /= 256.0;

            int maxtilesinline = (int)Math.Pow(2, zoom);
            int min_x = Math.Max(0,(int)(px0[0]));
            int max_x = Math.Min(maxtilesinline, (int)(px1[0]) - ((int)(px1[0]) % tileZoneSizeDiv) + tileZoneSizeDiv - 1);
            int min_y = Math.Max(0,(int)(px0[1]));
            int max_y = Math.Min(maxtilesinline, (int)(px1[1]) - ((int)(px1[1]) % tileZoneSizeDiv) + tileZoneSizeDiv - 1);


            ulong total_to_render = (ulong)(tileZoneSizeCount * (((int)(max_x / tileZoneSizeDiv) - (int)(min_x / tileZoneSizeDiv) + 1) * ((int)(max_y / tileZoneSizeDiv) - (int)(min_y / tileZoneSizeDiv) + 1)));
            // // OLD calculate total_to_render
            //for (int x = min_x; x <= max_x; x += tileZoneDiv)
            //    for (int y = min_y; y <= max_y; y += tileZoneDiv)
            //        total_to_render += (ulong)(tileZoneDiv * tileZoneDiv);

            //START
            ulong total_created = 0;
            ulong total_skipped = 0;
            ulong total_notsavd = 0;
            ulong total_created_size = 0;
            for (int x = min_x; x <= max_x; x += tileZoneSizeDiv)
                for (int y = min_y; y <= max_y; y += tileZoneSizeDiv)
                {
                    // [tiles_created, skipped, with_errors, tiles_size, error_code 1 or 2]
                    int[] cas = this.render_tile(x, y, zoom);
                    total_created += (ulong)cas[0];
                    total_skipped += (ulong)cas[1];
                    total_notsavd += (ulong)cas[2];
                    total_created_size += (ulong)cas[3];

                    //PAUSE NORMAL
                    if (TileRendering.TileRenderingErrors.PauseNormal)
                    {
                        TileRendering.TileRenderingErrors.PausedNormal++;
                        while (TileRendering.TileRenderingErrors.PauseNormal) Thread.Sleep(100);
                        TileRendering.TileRenderingErrors.PausedNormal--;
                    };
                };
        }

        /// <summary>
        ///     Отрисовка тайла (зоны тайлов)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>[tiles_created,skipped,with_error,tiles_size,error_code 1 or 2 or 3]</returns>
        public int[] render_tile(int x, int y, int z)
        {
            if (this.reloadMapEachZone)
            {
                if (this.map != null) this.map.Dispose();
                this.map = new MapnikCs.Map(256 * (uint)tileZoneSize, 256 * (uint)tileZoneSize);
                this.map.BufferSize = this.tileZoneOverSize;
                this.map.LoadFile(this.mapfile, true);
                if(this.map_proj == null) this.map_proj = new MapnikCs.Projection(this.map.SpatialReference);
            };


            int xx = (int)(x / tileZoneSizeDiv);
            int yy = (int)(y / tileZoneSizeDiv);

            // Calculate pixel positions of bottom-left & top-right
            double[] p0 = new double[] { xx * 256 * tileZoneSizeDiv, (yy + 1) * 256 * tileZoneSizeDiv };
            double[] p1 = new double[] { (xx + 1) * 256 * tileZoneSizeDiv, yy * 256 * tileZoneSizeDiv };
           
            // Convert to LatLong (EPSG:4326 / SR-ORG:14)
            double[] l0 = TilesProjection.fromPixelToLL(p0, z);
            double[] l1 = TilesProjection.fromPixelToLL(p1, z);

            // if render & save tiles only in polygon or inline
            if(RenderInShape != null)
                if(!RenderInShape.SquareIn(new PointF((float)l0[0], (float)l0[1]), new PointF((float)l1[0], (float)l1[1])))
                    return new int[] { 0, tileZoneSizeCount, 0, 0, 0 };

            // Convert to map projection (spherical mercator co-ords EPSG:3857 / SR-ORG:7483 / EPSG:900913)            
            MapnikCs.Coord c0 = this.map_proj.Forward(new MapnikCs.Coord(l0[0], l0[1]));
            MapnikCs.Coord c1 = this.map_proj.Forward(new MapnikCs.Coord(l1[0], l1[1]));

            // Bounding box for the tile
            this.map.ZoomToBox(new MapnikCs.Envelope(c0.X, c0.Y, c1.X, c1.Y));

            // Render Tile
            int tilesToSave = 0;
            int tilesToSkip = 0;
            int tilesSaveErr = 0;
            int tilesToSaveSize = 0;
            MapnikCs.Image im = new MapnikCs.Image(map.Width, map.Height);
            try
            {
                this.map.Render(im);
            }
            catch (AccessViolationException ave)
            {
                Excepted(x, y, z, "Render Map Error", ave, "");
                return new int[] { 0, 0, tileZoneSizeCount, 0, 1 };
            }
            catch (Exception ex)
            {
                Excepted(x, y, z, "Unhandled Map Error", ex, "");
                return new int[] { 0, 0, tileZoneSizeCount, 0, 1 };
            };

            if ((processEmptyTiles == TileRenderEmptyData.saveAnyTiles) || (im.WasPainted))
            {
                //im.Save("...", System.Drawing.Imaging.ImageFormat.Png);
                Bitmap img;
                try
                {
                    img = (Bitmap)im.ToImage();
                }
                catch (Exception ex)
                {
                    Excepted(x, y, z, "Error MapnicCS.Image.ToImage()", ex, "");
                    im.Dispose();
                    return new int[] { 0, 0, tileZoneSizeCount, 0, 2 };
                };

                if (tileZoneSize == TileRenderZoneSize.tileZone_1x1)
                {
                    // если размер зоны 1х1 тайл, то сохраняем его
                    double dev = 1;
                    // если в настройках указано не сохранять одноцветные, то определяем одноцветный ли тайл
                    if (processEmptyTiles == TileRenderEmptyData.saveActualTiles)
                        dev = GetImageColorDeviation(img, this.map.BackgroundColor, false);
                    if (dev > 0)
                    {
                        //string fileName = tiles_dir + GetTileSubPath(x, y, z);
                        //FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                        //long fSize = fs.Length;
                        //fs.Close();
                        int ts = 0;
                        byte svCode = SaveTile(img, x, y, z, out ts);
                        if (ts == 0)
                        {
                            tilesToSave++;
                            tilesToSaveSize += ts;
                        }
                        else if (ts == 1)
                            tilesToSkip++;
                        else
                            tilesSaveErr++;
                    }
                    else
                        tilesToSkip++;
                }
                else
                {
                    // если размер зоны больше чем 1х1 тайл, то делим зону на тайлы
                    for (int xi = 0; xi < tileZoneSizeDiv; xi++)
                        for (int yi = 0; yi < tileZoneSizeDiv; yi++)
                        {
                            int stx = xx * tileZoneSizeDiv + xi;
                            int sty = yy * tileZoneSizeDiv + yi;

                            // если в настройках указано сохранять только запрашиваемый тайл, а это не он - пропускаем
                            if ((tileZoneMode == TileRenderZoneMode.keepOnlyCurrent) && ((stx != x) || (sty != y)))
                                continue;

                            // if render & save tiles only in polygon or inline
                            if (RenderInShape != null)
                                if (!RenderInShape.TileIn(stx, sty, z))
                                {
                                    tilesToSkip++;
                                    continue;
                                };

                            Rectangle srcRect = new Rectangle(xi * 256, yi * 256, 256, 256);
                            Bitmap tile = (Bitmap)img.Clone(srcRect, img.PixelFormat);
                            double dev = 1;
                            // Так как Mapnik отрисовывает карту размером более чем 1х1 тайл, то возможно какие-либо из
                            // тайлов на изображении не содержат полезную информацию.
                            // если в настройках указано не сохранять любые файлы, то определяем есть ли на тайле данные
                            if ((processEmptyTiles != TileRenderEmptyData.saveAnyTiles) && (processEmptyTiles != TileRenderEmptyData.saveAnyAreaTiles))
                                dev = GetImageColorDeviation(tile, this.map.BackgroundColor, processEmptyTiles == TileRenderEmptyData.saveOneColorTiles);
                            if (dev > 0)
                            {
                                //string fileName = tiles_dir + GetTileSubPath(x, y, z);
                                //FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                                //long fSize = fs.Length;
                                //fs.Close();
                                int ts = 0;
                                byte svCode = SaveTile(tile, stx, sty, z, out ts);
                                if (svCode == 0)
                                {
                                    tilesToSave++;
                                    tilesToSaveSize += ts;
                                }
                                else if (svCode == 1)
                                    tilesToSkip++;
                                else
                                    tilesSaveErr++;
                            }
                            else
                                tilesToSkip++;
                            tile.Dispose();
                        };
                };
                img.Dispose();
            }
            else
            {
                im.Dispose();
                return new int[] { 0, tileZoneSizeCount, 0, 0, 0 };
            };

            im.Dispose();
            return new int[] { tilesToSave, tilesToSkip, tilesSaveErr, tilesToSaveSize, (tilesToSave == 0) && (tilesSaveErr > 0) ? 3 : 0 };
        }

        public void Excepted(int x, int y, int z, string comment, Exception ex, string parameter)
        {
            uint thrID = MultithreadTileRenderer.GetCurrentThreadId();

            //PAUSE NORMAL
            if (TileRendering.TileRenderingErrors.PauseNormal)
            {
                TileRendering.TileRenderingErrors.PausedNormal++;
                while (TileRendering.TileRenderingErrors.PauseNormal) Thread.Sleep(100);
                TileRendering.TileRenderingErrors.PausedNormal--;
            };

            // check if pause to show message
            bool paused = (TileRenderingErrors.PauseThreadsIfErrorsCountIs > 0) && ((TileRenderingErrors.ErrorQueueTotalCount+1) >= TileRenderingErrors.PauseThreadsIfErrorsCountIs);                        

            // add error to list
            TileRenderingErrors.ErrorQueueMutex.WaitOne();
            TileRenderingErrors.ErrorQueue.Enqueue(new TileRenderingErrors.ErrorInfo(x, y, z, comment, ex, parameter));
            TileRenderingErrors.ErrorQueueTotalCount++;
            TileRenderingErrors.ErrorQueueMutex.ReleaseMutex();

            if (comment == "Error Tile Optimization") return;
            
            // adding thread to paused list
            if (paused)
            {                
                TileRenderingErrors.PausedThreadsMutex.WaitOne();
                if(TileRenderingErrors.PausedThreads.IndexOf(thrID) < 0)
                    TileRenderingErrors.PausedThreads.Add(thrID);
                TileRenderingErrors.PausedThreadsMutex.ReleaseMutex();
            };

            // if thread is paused, wait until error message accepted 
            while (paused)
            {
                TileRenderingErrors.PausedThreadsMutex.WaitOne();
                paused = TileRenderingErrors.PausedThreads.IndexOf(thrID) >= 0;
                TileRenderingErrors.PausedThreadsMutex.ReleaseMutex();
                Thread.Sleep(250);
            };
        }

        /// <summary>
        ///     Сохраняем конечный тайл
        /// </summary>
        /// <param name="tile">Bitmap</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <returns>0 - saved, 1 - skipped, 2 - error</returns>
        public byte SaveTile(Bitmap tile, int x, int y, int z, out int fileSize)
        {            
            string fileName = tiles_dir + GetTileSubPath(x, y, z);
            fileSize = 0;
            
            // проверяем структуру папок и создаем отсутствующие
            int dir_index = pathStructure == TileRenderPathScheme.fsSlippyMap ? x : y;
            dir_list_mtx.WaitOne();
            if (dir_list.IndexOf(dir_index) < 0)
            {
                dir_list.Add(dir_index);
                dir_list_mtx.ReleaseMutex();

                try
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileName));
                }
                catch (Exception ex)
                {
                    Excepted(x, y, z, "Error Creating Directory", ex, System.IO.Path.GetDirectoryName(fileName));
                };
            }
            else
                dir_list_mtx.ReleaseMutex();

            // если в настройках указано сохранять существующий файл то тайл не сохраняем
            if ((tileFinalMerging == TileFinalMerging.keepExistingFile) && System.IO.File.Exists(fileName))
                return 1;

            // если в настройках указано объединять существующий файл с новым
            if ((tileFinalMerging == TileFinalMerging.mergeWithExistingFile) && System.IO.File.Exists(fileName))
            {
                FileStream df = null;
                try
                {
                    df = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    tile = MergeTiles(new Bitmap[] { (Bitmap)Image.FromStream(df), tile }, map.BackgroundColor, true);
                }
                catch (Exception ex)
                {
                    Excepted(x, y, z, "Error Merging Tiles", ex, fileName);
                };
                if (df != null) df.Dispose();
            };

            // добавляем водяные знаки
            if (CopyrightWatermarkImage != null)
            {
                try
                {
                    WatermarkFile(ref tile, CopyrightWatermarkImage);
                }
                catch (Exception ex)
                {
                    Excepted(x, y, z, "Error Adding Watermark", ex, "");
                };
            };


            // сжимаем и сохраняем конечный тайл
            if (tileOptimizeMethod != TileOptimizeMethod.none)
                if (OptimizeAndSave(tile, fileName, x, y, z, out fileSize)) 
                    return 0;

            try 
            {
                tile.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                fileSize = (int)(new FileInfo(fileName)).Length;
                return 0;
            }
            catch (Exception ex)
            {
                Excepted(x, y, z, "Error Saving Tile", ex, fileName);
                return 2;
            };
            // RETURN //  
        }

        /// <summary>
        ///     Изображение с водяным знаком
        /// </summary>
        public System.Drawing.Image _wmImage = null;
        
        /// <summary>
        ///     Наносим на тайл водяной знак
        /// </summary>
        /// <param name="bmp">Конечный тайл</param>
        /// <param name="cfg">Параметры наложения</param>
        private static void WatermarkFile(ref Bitmap bmp, string copyrightFile)
        {
            System.Drawing.Image img = new System.Drawing.Bitmap(256, 256, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(img);
            g.DrawImage(bmp, 0, 0, 256, 256);

            System.Drawing.Image _wmImage = System.Drawing.Image.FromFile(copyrightFile);
            g.DrawImage(_wmImage, 0, 0, 256, 256);            
            g.Dispose();
            bmp.Dispose();
            _wmImage.Dispose();
            bmp = (Bitmap)img;
        }

        /// <summary>
        ///     Наносим на тайл водяной знак
        /// </summary>
        /// <param name="bmp">Конечный тайл</param>
        /// <param name="cfg">Параметры наложения</param>
        private static void WatermarkFile(ref Bitmap bmp, Image watermark)
        {
            System.Drawing.Image img = new System.Drawing.Bitmap(256, 256, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(img);
            g.DrawImage(bmp, 0, 0, 256, 256);
            g.DrawImage(watermark, 0, 0, 256, 256);
            g.Dispose();
            bmp.Dispose();
            bmp = (Bitmap)img;
        }
     

        /// <summary>
        ///     Накладываем тайлы один на другой
        /// </summary>
        /// <param name="bitmaps">Тайлы (0-нижний слой, 1 - верхний)</param>
        /// <param name="dispose">закрывать тайлы после наложения</param>
        /// <returns></returns>
        private static Bitmap MergeTiles(Bitmap[] bitmaps, Color BackgroundColor, bool dispose)
        {
            Rectangle r256 = new Rectangle(0, 0, 256, 256);
            System.Drawing.Image res = new System.Drawing.Bitmap(256, 256, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(res);
            for (int i = 0; i < bitmaps.Length; i++)
            {
                // считаем что в палитре отрендеренного файла есть прозрачный цвет 
                // если цвет фона карты не прозрачный, то считаем его таковым
                if ((i > 0) && (BackgroundColor != Color.Transparent))
                    for (int x = 0; x < bitmaps[i].Width; x++)
                        for (int y = 0; y < bitmaps[i].Height; y++)
                            if (bitmaps[i].GetPixel(x, y) == BackgroundColor)
                                bitmaps[i].SetPixel(x, y, Color.Transparent);
                g.DrawImage(bitmaps[i], r256, r256, GraphicsUnit.Pixel);
                if (dispose) bitmaps[i].Dispose();
            };
            g.Dispose();
            return (Bitmap)res;
        }


        /// <summary>
        ///     Пример построения карты
        /// </summary>
        /// <param name="startLat">начальная широта (сверху)</param>
        /// <param name="startLon">начальная долгота (слева)</param>
        /// <param name="endLat">конечная широта (снизу)</param>
        /// <param name="endLon">конечная долгота (справа)</param>        
        /// <param name="mapfile">XML-файл карты</param>
        /// <param name="tiles_dir">папка для хранения тайлов</param>
        /// <param name="zoom">Масштабный уровень</param>
        public static void RENDER_SAMPLE(double startLat, double startLon, double endLat, double endLon,
            string mapfile, string tiles_dir,
            int zoom)
        {
            TileRenderer renderer = new TileRenderer(mapfile, tiles_dir);
            renderer.render_zoom(startLat, startLon, endLat, endLon, zoom);
            System.Windows.Forms.MessageBox.Show("DONE");
        }

        #region Path Structure
        /// <summary>
        ///     Получаем путь к тайлу
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public string GetTileSubPath(int x, int y, int z)
        {
            if (pathStructure == TileRenderPathScheme.fsSlippyMap)
                return String.Format(@"\{0}\{1}\{2}.png", z, x, y);
            else
                return String.Format(@"\L{0}\R{1}\C{2}.png", TwoZ(z < 10 ? z - 4 : z - 10), ToHex8(y), ToHex8(x));
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


        #region Optimze DLLs
        #region Optimize.dll
        // Optimize Image // dkxce ver 0.1 // Optimize.dll
        /// <summary>
        ///     Optimize Image with Optimize.dll
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [DllImport("optimize.dll", SetLastError = true, EntryPoint = "optimizefiles")]
        private static extern int Optimize_OptimizeDLL(string fileName);
        #endregion

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
        
        /// <summary>
        ///     Оптимизируем и сохраняем файл
        /// </summary>
        /// <param name="img"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool OptimizeAndSave(Bitmap img, string fileName, int x, int y, int z, out int outFileSize)
        {
            System.IO.MemoryStream STD = new MemoryStream();
            STD.Position = 0;
            outFileSize = 0;

            try
            {
                img.Save(STD, System.Drawing.Imaging.ImageFormat.Png);
                long fileSize = STD.Length;
                outFileSize = (int)(STD.Length);
                
                // fastest = 0x02, // используется FreeImage.DLL/быстрое и Standard, сохраняется файл с меньшим размером	
                if (tileOptimizeMethod == TileOptimizeMethod.fastest)
                {
                    FreeImageAPI.FIBITMAP dib = FreeImageAPI.FreeImage.LoadFromStream(STD);
                    FreeImageAPI.FreeImage.SaveEx(
                        ref dib,
                        fileName, FreeImageAPI.FREE_IMAGE_FORMAT.FIF_PNG,
                        FreeImageAPI.FREE_IMAGE_SAVE_FLAGS.PNG_Z_DEFAULT_COMPRESSION,
                        FreeImageAPI.FREE_IMAGE_COLOR_DEPTH.FICD_AUTO,
                        true);
                    dib.SetNull();

                    if ((fileSize = (new FileInfo(fileName)).Length) > STD.Length)
                        SaveStreamToFile(STD, fileName);

                    STD.Close();
                    outFileSize = (int)Math.Min(fileSize, STD.Length);
                    return true;
                };

                // fastmax = 0x03, // используется FreeImage.DLL/максимальное и Standard, сохраняется файл с меньшим размером
                if (tileOptimizeMethod == TileOptimizeMethod.fastmax)
                {
                    FreeImageAPI.FIBITMAP dib = FreeImageAPI.FreeImage.LoadFromStream(STD);
                    FreeImageAPI.FreeImage.SaveEx(
                        ref dib,
                        fileName, FreeImageAPI.FREE_IMAGE_FORMAT.FIF_PNG,
                        FreeImageAPI.FREE_IMAGE_SAVE_FLAGS.PNG_Z_BEST_COMPRESSION,
                        FreeImageAPI.FREE_IMAGE_COLOR_DEPTH.FICD_AUTO,
                        true);
                    dib.SetNull();

                    if ((fileSize = (new FileInfo(fileName)).Length) > STD.Length)
                        SaveStreamToFile(STD, fileName);

                    STD.Close();
                    outFileSize = (int)(Math.Min(fileSize, STD.Length));
                    return true;
                };

                // fastMagic = 0x04, //используется imageMagic, FreeImage.DLL/максимальное и Standard, сохраняется файл с меньшим размером
                if (tileOptimizeMethod == TileOptimizeMethod.fastMagic)
                {
                    ImageMagick.MagickImage mi = new ImageMagick.MagickImage(STD.ToArray());
                    mi.Format = ImageMagick.MagickFormat.Png + tileOptimizeFormat;// sfsf 
                    System.IO.MemoryStream ms = new MemoryStream();
                    mi.Write(ms);

                    FreeImageAPI.FIBITMAP dib = FreeImageAPI.FreeImage.LoadFromStream(STD);
                    FreeImageAPI.FreeImage.SaveEx(
                        ref dib,
                        fileName, FreeImageAPI.FREE_IMAGE_FORMAT.FIF_PNG,
                        FreeImageAPI.FREE_IMAGE_SAVE_FLAGS.PNG_Z_BEST_COMPRESSION,
                        FreeImageAPI.FREE_IMAGE_COLOR_DEPTH.FICD_AUTO,
                        true);
                    dib.SetNull();

                    long[] sa = new long[] { STD.Length, ms.Length, (new FileInfo(fileName)).Length };
                    Array.Sort(sa);

                    if (sa[0] == STD.Length) SaveStreamToFile(STD, fileName);
                    if (sa[0] == ms.Length) SaveStreamToFile(ms, fileName);
                    ms.Close();
                    mi = null;

                    STD.Close();
                    outFileSize = (int)(sa[0]);
                    return true;
                };

                // imageMagic = 0x05, // используется imageMagic и Standard, сохраняется файл с меньшим размером
                if (tileOptimizeMethod == TileOptimizeMethod.imageMagic)
                {
                    ImageMagick.MagickImage mi = new ImageMagick.MagickImage(STD.ToArray());
                    mi.Format = ImageMagick.MagickFormat.Png + tileOptimizeFormat;
                    System.IO.MemoryStream ms = new MemoryStream();
                    mi.Write(ms);
                    if (ms.Length > STD.Length)
                    {
                        SaveStreamToFile(STD, fileName);
                        outFileSize = (int)STD.Length;
                    }
                    else
                    {
                        SaveStreamToFile(ms, fileName);
                        outFileSize = (int)ms.Length;
                    };
                    ms.Close();
                    mi = null;

                    STD.Close();
                    return true;
                };

                // optimize = 0x06, // используется optimize.dll и Standard, сохраняется файл с меньшим размером
                if (tileOptimizeMethod == TileOptimizeMethod.optimize)
                {
                    SaveStreamToFile(STD, fileName);
                    Optimize_OptimizeDLL("-file:\"" + fileName + "\" -KeepInterlacing -KeepBackgroundColor -AvoidGreyWithSimpleTransparency");
                    if ((fileSize = (new FileInfo(fileName)).Length) > STD.Length)
                        SaveStreamToFile(STD, fileName);
                    outFileSize = (int)(Math.Min(fileSize, STD.Length));
                    return true;
                };

                // maximim = 0x07 // используется imageMagic, optimize.dll и Standard, сохраняется файл с меньшим размером
                if (tileOptimizeMethod == TileOptimizeMethod.maximim)
                {
                    ImageMagick.MagickImage mi = new ImageMagick.MagickImage(STD.ToArray());
                    mi.Format = ImageMagick.MagickFormat.Png + tileOptimizeFormat;
                    System.IO.MemoryStream ms = new MemoryStream();
                    mi.Write(ms);

                    SaveStreamToFile(STD, fileName);
                    Optimize_OptimizeDLL("-file:\"" + fileName + "\" -KeepInterlacing -KeepBackgroundColor -AvoidGreyWithSimpleTransparency");

                    long[] sa = new long[] { STD.Length, ms.Length, (new FileInfo(fileName)).Length };
                    Array.Sort(sa);

                    if (sa[0] == STD.Length) SaveStreamToFile(STD, fileName);
                    if (sa[0] == ms.Length) SaveStreamToFile(ms, fileName);
                    ms.Close();
                    mi = null;
                    outFileSize = (int)(sa[0]);
                    return true;
                };                
            }
            catch (Exception ex)
            {
                Excepted(x, y, z, "Error Tile Optimization", ex, fileName);                
            };

            try
            {
                STD.Close();
            }
            catch { };
            return false;
        }
        
        #endregion

        /// <summary>
        ///     Определяем пустой ли тайл или нет и если пустой отличается ли он от заданного цвета 
        ///     (0 - пустой; > (+) - несущий полезную информацию; -1 - одноцветный тайл отличного от подложки цвета)
        /// </summary>
        /// <param name="b">Bitmap</param>
        /// <param name="bgColor">Map Background Color</param>
        /// <returns></returns>
        public static double GetImageColorDeviation(Bitmap b, Color bgColor, bool returnWhenFirstPixelColorIsDifferrent)
        {
            double total = 0, totalVariance = 0;
            int count = 0;
            double stdDev = 0;
            Color pxlColor = Color.Transparent;
            
            for (int y = 0; y < b.Height; ++y)
            {
                for (int x = 0; x < b.Width; ++x)
                {
                    count++;
                    pxlColor = b.GetPixel(x, y);

                    if ((returnWhenFirstPixelColorIsDifferrent) && (pxlColor != bgColor))
                        return double.MaxValue;
                
                    int pixelValue = Color.FromArgb(0, pxlColor).ToArgb();
                    total += pixelValue;
                    double avg = total / count;
                    totalVariance += Math.Pow(pixelValue - avg, 2);
                    stdDev = Math.Sqrt(totalVariance / count);
                }
            };

            // если тайл покрашен в один цвет и этот цвет отличается от заданного, то возвращаем -1
            if ((stdDev == 0.0) && (pxlColor != bgColor)) return -1.0; // одноцветный тайл отличного от подложки цвета

            return stdDev;
        }

        /// <summary>
        ///     Определяем пустой ли тайл или нет (0-пустой)
        /// </summary>
        /// <param name="b">Bitmap</param>
        /// <returns></returns>
        public static double GetImageColorDeviation(Bitmap b)
        {
            double total = 0, totalVariance = 0;
            int count = 0;
            double stdDev = 0;
            Color pxlColor = Color.Transparent;

            for (int y = 0; y < b.Height; ++y)
            {
                for (int x = 0; x < b.Width; ++x)
                {
                    count++;
                    pxlColor = b.GetPixel(x, y);
                    int pixelValue = Color.FromArgb(0, pxlColor).ToArgb();
                    total += pixelValue;
                    double avg = total / count;
                    totalVariance += Math.Pow(pixelValue - avg, 2);
                    stdDev = Math.Sqrt(totalVariance / count);
                }
            };

            return stdDev;
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

        static readonly string[] FileSizeSuffixes = { "байт", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string FileSizeSuffix(double value)
        {
            if (value < 0) { return "-" + FileSizeSuffix(-value); }
            if (value == 0) { return "0 байт"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:0.00} {1}", adjustedSize, FileSizeSuffixes[mag]).Replace(",",".");
        }
    }

    public class RenderView
    {
        private class Img8Converter
        {
            public Bitmap ConvertTo8bppFormat(Bitmap bmpSource, System.Drawing.Imaging.ColorPalette palette, int colors)
            {
                int imWi = bmpSource.Width;
                int imHi = bmpSource.Height;

                Bitmap bmpDest = null;
                System.Drawing.Imaging.BitmapData dstImg = null;
                System.Drawing.Imaging.BitmapData srcImg = null;

                try
                {
                    bmpDest = new Bitmap(imWi, imHi, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    dstImg = bmpDest.LockBits(new Rectangle(0, 0, imWi, imHi), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmpDest.PixelFormat);
                    srcImg = bmpSource.LockBits(new Rectangle(0, 0, imWi, imHi), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmpSource.PixelFormat);

                    int pixelSize = GetPixelInfoSize(srcImg.PixelFormat);
                    byte[] srcBuff = new byte[imWi * imHi * pixelSize];
                    byte[] dstBuffer = new byte[imWi * imHi];

                    ReadBmpData(srcImg, srcBuff, pixelSize, imWi, imHi);
                    bmpDest.Palette = palette;
                    MatchColors(srcBuff, dstBuffer, pixelSize, bmpDest.Palette, colors);
                    WriteBmpData(dstImg, dstBuffer, imWi, imHi);
                }
                finally
                {
                    if (bmpDest != null) bmpDest.UnlockBits(dstImg);
                    if (bmpSource != null) bmpSource.UnlockBits(srcImg);
                };

                return bmpDest;
            }

            public static byte[] LoadPalette(string fileName)
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                sr.ReadLine();
                sr.ReadLine();
                int colors = int.Parse(sr.ReadLine().Trim());
                byte[] pal = new byte[colors * 3];
                for (int i = 0; i < colors; i++)
                {
                    string[] col = sr.ReadLine().Trim().Split(new string[] { " " }, StringSplitOptions.None);
                    pal[i * 3 + 0] = byte.Parse(col[0]); //R
                    pal[i * 3 + 1] = byte.Parse(col[1]); //G
                    pal[i * 3 + 2] = byte.Parse(col[2]); //B
                };
                sr.Close();
                fs.Close();

                return pal;
            }

            public static System.Drawing.Imaging.ColorPalette LoadPalette(string filename, out int size)
            {
                byte[] cpal = LoadPalette(filename);
                Bitmap tmp = new Bitmap(16, 16, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                System.Drawing.Imaging.ColorPalette palette = tmp.Palette;
                tmp.Dispose();

                size = (int)(cpal.Length / 3.0);
                for (int i = 0; i <= 255; i++)
                {
                    if (i < size)
                        palette.Entries[i] = Color.FromArgb(cpal[i * 3 + 0], cpal[i * 3 + 1], cpal[i * 3 + 2]);
                    else
                        palette.Entries[i] = Color.FromArgb(0, 0, 0);
                };

                return palette;
            }

            private void MatchColors(byte[] buffer, byte[] destBuffer, int pixelSize, System.Drawing.Imaging.ColorPalette pallete, int colors)
            {
                int length = destBuffer.Length;
                byte[] temp = new byte[pixelSize];
                for (int i = 0; i < length; i++)
                {
                    Array.Copy(buffer, i * pixelSize, temp, 0, pixelSize);
                    destBuffer[i] = GetSimilarColor(pallete, temp, colors);
                };
            }

            private int GetPixelInfoSize(System.Drawing.Imaging.PixelFormat format)
            {
                switch (format)
                {
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb: { return 4; break; }
                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb: { return 3; break; }
                    default: { throw new ApplicationException("Only 24bit and 32bits colors supported now"); }
                };
            }

            private void ReadBmpData(System.Drawing.Imaging.BitmapData bmpDataSource, byte[] buffer, int pixelSize, int width, int height)
            {
                int addrStart = bmpDataSource.Scan0.ToInt32();
                for (int i = 0; i < height; i++)
                {
                    IntPtr realByteAddr = new IntPtr(addrStart + System.Convert.ToInt32(i * bmpDataSource.Stride));
                    Marshal.Copy(realByteAddr, buffer, (int)(i * width * pixelSize), (int)(width * pixelSize));
                };
            }

            private void WriteBmpData(System.Drawing.Imaging.BitmapData bmpDataDest, byte[] destBuffer, int imageWidth, int imageHeight)
            {
                int addrStart = bmpDataDest.Scan0.ToInt32();
                for (int i = 0; i < imageHeight; i++)
                {
                    IntPtr realByteAddr = new IntPtr(addrStart + System.Convert.ToInt32(i * bmpDataDest.Stride));
                    Marshal.Copy(destBuffer, i * imageWidth, realByteAddr, imageWidth);
                };
            }

            private byte GetSimilarColor(System.Drawing.Imaging.ColorPalette palette, byte[] color, int palleteSize)
            {
                byte index = 0;
                int md = int.MaxValue;
                for (int i = 0; i < palleteSize; i++)
                {
                    int cd = GetMaxDiff(color, palette.Entries[i]);
                    if (cd < md)
                    {
                        md = cd;
                        index = (byte)i;
                    };
                };
                return index;
            }

            private static int GetMaxDiff(byte[] a, Color b)
            {
                byte rd = (byte)Math.Abs(a[2] - b.R);
                byte gd = (byte)Math.Abs(a[1] - b.G);
                byte bd = (byte)Math.Abs(a[0] - b.B);

                //return bd + gd + rd;
                return Math.Max(rd, Math.Max(bd, gd));
            }

            public static void TEST()
            {
                Bitmap colorBmp = new Bitmap(@"C:\Downloads\CD-REC\KMZRebuilder[Sources]\bin\Debug\Images\dot[red].png");
                int pcols = 0;
                System.Drawing.Imaging.ColorPalette cp = Img8Converter.LoadPalette(@"C:\Downloads\CD-REC\_DSC_0156.pal", out pcols);
                Img8Converter c2 = new Img8Converter();
                Bitmap res = c2.ConvertTo8bppFormat(colorBmp, cp, pcols);
                res.Save(@"C:\Downloads\CD-REC\_DSC_0156.GIF", System.Drawing.Imaging.ImageFormat.Gif);
            }
        }

        public static byte[] fileheader = new byte[] { 0x52, 0x45, 0x4e, 0x44, 0x45, 0x52, 0x45, 0x52, 0x4d, 0x41, 0x54, 0x52, 0x49, 0x58, 0x44, 0x55, 0x4d, 0x50 };

        private int minx;
        private int maxx;
        private int miny;
        private int maxy;
        private int imwi;
        private int imhe;
        private int zsize;
        private int fsize;
        private int zoom;
        private IntPtr _map;
        private IntPtr _data;

        public RenderView(int minx, int maxx, int miny, int maxy, int zsize, int zoom)
        {
            this.minx = minx;
            this.maxx = maxx;
            this.miny = miny;
            this.maxy = maxy;
            this.zsize = zsize;
            this.zoom = zoom;

            imwi = ((maxx - minx + 1) / zsize) + 1;
            imhe = ((maxy - miny + 1) / zsize) + 1;
            fsize = imwi * imhe + 22;

            // ZOOM NO byte, zoneSize byte, xmin int, xmax int, ymin int, ymax int, ZOOM data_size int, DATA byte[]
            _map = Marshal.AllocHGlobal(fsize);
            _data = (IntPtr)((int)_map + 22);
            unsafe
            {
                byte* st = (byte*)_map;
                // zoomNo
                *st = (byte)zoom;
                st++;
                // zoneSize
                *st = (byte)zsize;
                st++;
                // xmin
                byte[] inval = BitConverter.GetBytes(minx);
                if (BitConverter.IsLittleEndian) Array.Reverse(inval);
                for (int i = 0; i < inval.Length; i++, st++) *st = inval[i];
                // xmax
                inval = BitConverter.GetBytes(maxx);
                if (BitConverter.IsLittleEndian) Array.Reverse(inval);
                for (int i = 0; i < inval.Length; i++, st++) *st = inval[i];
                // ymin
                inval = BitConverter.GetBytes(miny);
                if (BitConverter.IsLittleEndian) Array.Reverse(inval);
                for (int i = 0; i < inval.Length; i++, st++) *st = inval[i];
                // ymax
                inval = BitConverter.GetBytes(maxy);
                if (BitConverter.IsLittleEndian) Array.Reverse(inval);
                for (int i = 0; i < inval.Length; i++, st++) *st = inval[i];
                // data_size
                inval = BitConverter.GetBytes(imwi * imhe);
                if (BitConverter.IsLittleEndian) Array.Reverse(inval);
                for (int i = 0; i < inval.Length; i++, st++) *st = inval[i];
                // data
                for (int x = 0; x < imwi; x++)
                    for (int y = 0; y < imhe; y++, st++)
                        *st = 0;
            };
        }

        // 0 - no info // White
        // 1 - in queue // Aqua
        // 2 - in work // Yellow
        // 3 - in work, error, repeat // Orange
        // 4 - all done well // Green
        // 5 - done with skipped // LightGreen
        // 6 - done with holes // Orchid
        // 7 - done with skipped and holes // Pink
        // 8 - all skipped // LimeGreen
        // 9 - all holes // Red
        // 10 - ? // Black
        public void Set(int x, int y, byte status)
        {
            int cx = (x - minx) / zsize;
            int cy = (y - miny) / zsize;
            int offset = cy + cx * imhe;
            unsafe
            {
                byte* st = (byte*)(((int)_data) + offset);
                *st = status;
            };
        }

        public byte Get(int x, int y)
        {
            int cx = (x - minx) / zsize;
            int cy = (y - miny) / zsize;
            int offset = cy + cx * imhe;
            unsafe
            {
                byte* st = (byte*)(((int)_data) + offset);
                return *st;
            };
        }

        public Bitmap GetView()
        {
            unsafe
            {
                byte* st = (byte*)_data;
                Bitmap bmp = new Bitmap(imwi, imhe);
                for (int x = 0; x < imwi; x++)
                    for (int y = 0; y < imhe; y++)
                    {
                        if (*st == 0)
                            bmp.SetPixel(x, y, Color.White);
                        if(*st == 1)
                            bmp.SetPixel(x, y, Color.Aqua);
                        if (*st == 2)
                            bmp.SetPixel(x, y, Color.Yellow);
                        if (*st == 3)
                            bmp.SetPixel(x, y, Color.Orange);
                        if (*st == 4)
                            bmp.SetPixel(x, y, Color.Green);
                        if (*st == 5)
                            bmp.SetPixel(x, y, Color.LightGreen);
                        if (*st == 6)
                            bmp.SetPixel(x, y, Color.Orchid);
                        if (*st == 7)
                            bmp.SetPixel(x, y, Color.Pink);
                        if (*st == 8)
                            bmp.SetPixel(x, y, Color.LimeGreen);
                        if (*st == 9)
                            bmp.SetPixel(x, y, Color.Red);
                        if (*st == 10)
                            bmp.SetPixel(x, y, Color.Black);
                        st++;
                    };                
                return bmp;
            };
            //return null;
        }

        public Bitmap Resize(Bitmap bmp, int wi, int he)
        {            
            int dw = (int)((double)wi / (double)bmp.Width);
            int dh = (int)((double)he / (double)bmp.Height);
            Bitmap b = new Bitmap(bmp.Width * dw, bmp.Height * dh);

            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    for (int yy = 0; yy < dh; yy++)
                        for (int xx = 0; xx < dw; xx++)
                            b.SetPixel(x * dw + xx, y * dh + yy, c);
                };
            return b;
            //Graphics g = Graphics.FromImage((Image)b);
            //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            //g.DrawImage(bmp, 0, 0, wi, he);
            //g.Dispose();
            //return b;
        }

        public Bitmap Compress(Bitmap bmp)
        {
            Img8Converter conv = new Img8Converter();
            Bitmap tmp = new Bitmap(16, 16, System.Drawing.Imaging.PixelFormat.Format4bppIndexed);
            System.Drawing.Imaging.ColorPalette palette = tmp.Palette;
            tmp.Dispose();

            palette.Entries[0] = Color.White;
            palette.Entries[1] = Color.Aqua;
            palette.Entries[2] = Color.Yellow;
            palette.Entries[3] = Color.Orange;
            palette.Entries[4] = Color.Green;
            palette.Entries[5] = Color.LightGreen;
            palette.Entries[6] = Color.Orchid;
            palette.Entries[7] = Color.Pink;
            palette.Entries[8] = Color.LimeGreen;
            palette.Entries[9] = Color.Red;
            palette.Entries[10] = Color.Black;

            return conv.ConvertTo8bppFormat(bmp, palette, 11);
        }

        public int Width
        {
            get
            {
                return imwi;
            }
        }

        public int Height
        {
            get
            {
                return imhe;
            }
        }

        public int Size
        {
            get
            {
                return imwi * imhe + 22;
            }
        }

        public byte GetByte(int byteNo)
        {
            unsafe
            {
                byte* st = (byte*)(((int)_map) + byteNo);
                return *st;
            };
        }

        public void SetByte(int byteNo, byte value)
        {
            unsafe
            {
                byte* st = (byte*)(((int)_map) + byteNo);
                *st = value;
            }
        }

        public IntPtr Pointer
        {
            get { return _map; }
        }

        ~RenderView()
        {
            Marshal.FreeHGlobal(_map);
        }
    }

    /// <summary>
    ///     ShapeFile Reader Class for 1-polygon-in-file or 1-polyline-in-file
    ///     Geometry Functions, Intersect Polygons
    ///     [[ORIGINAL]]
    /// </summary>
    public class TileRenderShapeLimiter
    {
        private static double MaxError = 1E-09;

        public ShapeFileType ShapeType;
        public Polygon Area;

        public TileRenderShapeLimiter(string shapeFile)
        {
            if (!SambaPathExists(shapeFile))
                throw new System.IO.IOException("Shape file not found: " + shapeFile);
            this.readShapeFile(shapeFile);
            if ((this.Area.box == null) || (this.Area.points == null) || (this.Area.points.Length == 0))
                throw new System.IO.IOException("Error reading shape file: " + shapeFile);
            if ((Math.Abs(this.Area.points[0].X) > 360) || (Math.Abs(this.Area.points[0].Y) > 180))
                throw new System.OverflowException("Wrong shape file projection: " + shapeFile + "\r\nX: " + this.Area.points[0].X.ToString() + " Y: " + this.Area.points[0].Y.ToString());
        }

        public TileRenderShapeLimiter(ShapeFileType ShapeType, double[] box, int numParts, int numPoints, int[] parts, PointF[] points)
        {
            this.ShapeType = ShapeType;
            this.Area = new Polygon();
            this.Area.box = box;
            this.Area.numParts = numParts;
            this.Area.numPoints = numPoints;
            this.Area.parts = parts;
            this.Area.points = points;

            this.Area.Bounds = new List<double[]>();

            int[] startAt = new int[Area.numParts];
            int[] endBefore = new int[Area.numParts];
            for (int i = 0; i < Area.numParts; i++)
            {
                startAt[i] = Area.parts[i];
                endBefore[i] = (i + 1) == Area.parts.Length ? Area.points.Length : Area.parts[i + 1];

                if (i == 0)
                    Area.Bounds.Add(Area.box);
                else
                    Area.Bounds.Add(new double[] { double.MaxValue, double.MaxValue, double.MinValue, double.MinValue });
            };
            for (int i = 0; i < Area.numPoints; i++)
            {
                for (int p = 1; p < Area.numParts; p++)
                    if ((i >= startAt[p]) && (i < endBefore[p]))
                    {
                        Area.Bounds[p][0] = Math.Min(Area.Bounds[p][0], Area.points[i].X);
                        Area.Bounds[p][1] = Math.Min(Area.Bounds[p][1], Area.points[i].Y);
                        Area.Bounds[p][2] = Math.Max(Area.Bounds[p][2], Area.points[i].X);
                        Area.Bounds[p][3] = Math.Max(Area.Bounds[p][3], Area.points[i].Y);
                    };
            };
        }

        #region SambaPathExists
        public static bool SambaFileExists(string fileName)
        {
            bool exists = true;
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(delegate()
            {
                exists = System.IO.File.Exists(fileName);
            }));
            t.Start();
            bool completed = t.Join(1000); //half a sec of timeout
            if (!completed)
            { 
                exists = false; 
                t.Abort();
            };
            return exists;
        }

        /// <summary>
        ///     Check File Exist With Max Wait File
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool SambaPathExists(string path)
        {
            return SambaFileExists(path);
        }
        #endregion
        
        /// <summary>
        ///     Попадает ли точка в полигон
        ///     (поддерживает Polygon Rings)
        /// </summary>
        /// <param name="LonAsXLatAsY"></param>
        /// <returns></returns>
        public bool PointIn(PointF LonAsXLatAsY)
        {
            if (LonAsXLatAsY.X < this.Area.box[0]) return false; // Xmin
            if (LonAsXLatAsY.Y < this.Area.box[1]) return false; // Ymin
            if (LonAsXLatAsY.X > this.Area.box[2]) return false; // Xmax
            if (LonAsXLatAsY.Y > this.Area.box[3]) return false; // Ymax

            if (ShapeType == ShapeFileType.tPolygon)
            {
                if (!PointInPolygon(LonAsXLatAsY, this.Area[0], MaxError)) return false;
                for (int i = 1; i < this.Area.parts.Length; i++)
                    if (PointInPolygon(LonAsXLatAsY, this.Area[i], MaxError)) return false;
                return true;
            }
            else
            {
                // если точка лежит на линии
                if (Area.numParts == 1)
                    return PointInLineSegment(LonAsXLatAsY, this.Area.points);
                else
                    for (int i = 0; i < this.Area.parts.Length; i++)
                    {
                        int si = this.Area.parts[i];
                        int se = (i + 1) == this.Area.parts.Length ? this.Area.points.Length : this.Area.parts[i + 1];
                        PointF[] subline = new PointF[se - si];
                        Array.Copy(this.Area.points, si, subline, 0, subline.Length);

                        if (PointInLineSegment(LonAsXLatAsY, subline)) return true;
                    };
                return false;
            };
        }

        /// <summary>
        ///     Попадает ли тайл в полигон
        ///     (поддерживает Polygon Rings)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool TileIn(int x, int y, int z)
        {
            PointF[] tile = new PointF[4];
            PointF sp = tile2location(x, y, z);
            PointF ep = tile2location(x + 1, y + 1, z);
            tile[0] = (new PointF((float)sp.X, (float)sp.Y));
            tile[1] = (new PointF((float)sp.X, (float)ep.Y));
            tile[2] = (new PointF((float)ep.X, (float)ep.Y));
            tile[3] = (new PointF((float)ep.X, (float)sp.Y));

            if (ShapeType == ShapeFileType.tPolygon)
            {
                return PolygonIn(tile);
            }
            else
            {
                if (Area.numParts == 1)
                {
                    if ((this.Area.box[0] >= Math.Min(sp.X, ep.X)) &&
                        (this.Area.box[1] >= Math.Min(sp.Y, ep.Y)) &&
                        (this.Area.box[2] <= Math.Max(sp.Y, ep.Y)) &&
                        (this.Area.box[3] <= Math.Max(sp.Y, ep.Y))) return true;
                    return IntersectLineWithPolygon(this.Area.points, tile);
                }
                else
                {
                    for (int i = 0; i < this.Area.parts.Length; i++)
                    {
                        PointF[] subline = this.Area[i];                        

                        if (LineInsideSquare(subline, sp, ep)) return true;
                        if (IntersectLineWithPolygon(subline, tile)) return true;
                    };
                };
                return false;
            };
        }

        /// <summary>
        ///     Попадает ли тайл в полигон
        ///     (поддерживает Polygon Rings)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool TileInsidePolygon(int x, int y, int z)
        {
            if (ShapeType != ShapeFileType.tPolygon) throw new Exception("Shape type must be polygon");

            return TileIn(x, y, z);
        }

        /// <summary>
        ///     Лежит ли тайл в полигоне и не заступает ли за границы
        ///     (поддерживает Polygon Rings)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool TileInsidePolygonBorder(int x, int y, int z) 
        {
            if (ShapeType != ShapeFileType.tPolygon) throw new Exception("Shape type must be polygon");

            if (!PointIn(tile2location(((double)x) + 0.5, ((double)y) + 0.5, z)))
                return false;

            PointF[] tile = new PointF[5];
            PointF sp = tile2location(x, y, z);
            PointF ep = tile2location(x + 1, y + 1, z);
            tile[0] = (new PointF((float)sp.X, (float)sp.Y));
            tile[1] = (new PointF((float)sp.X, (float)ep.Y));
            tile[2] = (new PointF((float)ep.X, (float)ep.Y));
            tile[3] = (new PointF((float)ep.X, (float)sp.Y));
            tile[4] = (new PointF((float)sp.X, (float)sp.Y));
            
            for (int i = 0; i < this.Area.parts.Length; i++)
                if (IntersectLineWithPolygon(tile, this.Area[i]))
                    return false;
            return true;
        }

        /// <summary>
        ///     Лежит ли тайл за границами полигона но может пересекать его границы
        ///     (поддерживает Polygon Rings)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool TileOutsidePolygon(int x, int y, int z)
        {
            if (ShapeType != ShapeFileType.tPolygon) throw new Exception("Shape type must be polygon");
            return !TileInsidePolygonBorder(x, y, z);
        }

        /// <summary>
        ///     Лежит ли тайл за границами полигона и не задевает его
        ///     (поддерживает Polygon Rings)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool TileOutsidePolygonBorder(int x, int y, int z) 
        {
            if (ShapeType != ShapeFileType.tPolygon) throw new Exception("Shape type must be polygon");
            return !TileIn(x, y, z);
        }

        /// <summary>
        ///     Лежит ли тайл на границе полигона
        ///     (поддерживает Polygon Rings)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool TileOnPolygonBorder(int x, int y, int z) 
        {
            PointF[] tile = new PointF[5];
            PointF sp = tile2location(x, y, z);
            PointF ep = tile2location(x + 1, y + 1, z);
            tile[0] = (new PointF((float)sp.X, (float)sp.Y));
            tile[1] = (new PointF((float)sp.X, (float)ep.Y));
            tile[2] = (new PointF((float)ep.X, (float)ep.Y));
            tile[3] = (new PointF((float)ep.X, (float)sp.Y));
            tile[4] = (new PointF((float)sp.X, (float)sp.Y));

            for (int i = 0; i < this.Area.parts.Length; i++)
                if(IntersectLineWithPolygon(tile, this.Area[i])) 
                    return true;
            return false;
        }

        /// <summary>
        ///     Попадает ли зона тайлов в полигон
        ///     (поддерживает Polygon Rings)
        /// </summary>
        /// <param name="xmin"></param>
        /// <param name="ymin"></param>
        /// <param name="xmax"></param>
        /// <param name="ymax"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool TileZoneIn(int xmin, int ymin, int xmax, int ymax, int z)
        {
            PointF[] tile = new PointF[4];
            PointF sp = tile2location(xmin, ymin, z);
            PointF ep = tile2location(xmax + 1, ymax + 1, z);
            tile[0] = (new PointF((float)sp.X, (float)sp.Y));
            tile[1] = (new PointF((float)sp.X, (float)ep.Y));
            tile[2] = (new PointF((float)ep.X, (float)ep.Y));
            tile[3] = (new PointF((float)ep.X, (float)sp.Y));

            if (ShapeType == ShapeFileType.tPolygon)
            {
                return PolygonIn(tile);
            }
            else
            {
                if (Area.numParts == 1)
                {
                    if ((this.Area.box[0] >= Math.Min(sp.X, ep.X)) &&
                        (this.Area.box[1] >= Math.Min(sp.Y, ep.Y)) &&
                        (this.Area.box[2] <= Math.Max(sp.Y, ep.Y)) &&
                        (this.Area.box[3] <= Math.Max(sp.Y, ep.Y))) return true;
                    return IntersectLineWithPolygon(this.Area.points, tile);
                }
                else
                {
                    for (int i = 0; i < this.Area.parts.Length; i++)
                    {
                        PointF[] subline = this.Area[i];
                        if (LineInsideSquare(subline, sp, ep)) return true;
                        if (IntersectLineWithPolygon(subline, tile)) return true;
                    };
                };
                return false;
            };
        }

        /// <summary>
        ///     Пересекаются ли полигоны
        ///     (поддерживает Polygon Rings)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool PolygonIn(PointF[] p)
        {
            if (ShapeType == ShapeFileType.tPolygon)
            {
                if (!IntersectPolygons(this.Area[0], p, MaxError)) return false;
                for (int i = 1; i < this.Area.parts.Length; i++)
                {
                    if ((PointInPolygon(p[0], this.Area[i], MaxError))
                            && (!IntersectLineWithPolygon(p, this.Area[i]))) return false;
                };
                return true;
            }
            else
            {
                for (int i = 0; i < this.Area.parts.Length; i++)
                {
                    if (PointInPolygon(p[0], this.Area[i], MaxError)) return true;
                    if (IntersectLineWithPolygon(this.Area[i], p)) return true;
                };
                return false;
            };
        }

        /// <summary>
        ///     Попадает ли прямоугольная область или квадрат в полигон
        ///     (поддерживает Polygon Rings)
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <returns></returns>
        public bool SquareIn(PointF p1, PointF p2)
        {
            List<PointF> pts = new List<PointF>();
            pts.Add(p1);
            pts.Add(new PointF(p1.X, p2.Y));
            pts.Add(p2);
            pts.Add(new PointF(p2.X, p1.Y));
            
            if (ShapeType == ShapeFileType.tPolygon)
            {
                return PolygonIn(pts.ToArray());
            }
            else
            {
                if (Area.numParts == 1)
                {
                    if ((this.Area.box[0] >= Math.Min(p1.X, p2.X)) &&
                        (this.Area.box[1] >= Math.Min(p1.Y, p2.Y)) &&
                        (this.Area.box[2] <= Math.Max(p1.Y, p2.Y)) &&
                        (this.Area.box[3] <= Math.Max(p1.Y, p2.Y))) return true;
                    return IntersectLineWithPolygon(this.Area.points, pts.ToArray());
                }
                else
                {
                    for (int i = 0; i < this.Area.parts.Length; i++)
                    {
                        PointF[] subline = this.Area[i];                        
                        if (LineInsideSquare(subline, p1, p2)) return true;
                        if (IntersectLineWithPolygon(subline, pts.ToArray())) return true;
                    };
                };
                return false;
            };
        }
        
        #region read shape 1st record
        private void readShapeFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            long fileLength = fs.Length;
            Byte[] data = new Byte[fileLength];
            fs.Read(data, 0, (int)fileLength);
            fs.Close();
            int shapetype = readIntLittle(data, 32);
#if TileRenderer
            if ((shapetype != 5) && (shapetype != 3))
                throw new System.IO.IOException("Type of shape must be polyline or polygon: " + filename);
#endif
#if TileCopier
            if (shapetype != 5)
                throw new System.IO.IOException("Type of shape must be polygone");
#endif
            int currentPosition = 100;

            int recordStart = currentPosition;
            int recordNumber = readIntBig(data, recordStart);
            int contentLength = readIntBig(data, recordStart + 4);
            int recordContentStart = recordStart + 8;

#if TileRenderer
            if (shapetype == 3) ShapeType = ShapeFileType.tPolyline;
#endif
            if (shapetype == 5) ShapeType = ShapeFileType.tPolygon;

            Area = new Polygon();
            
            int recordShapeType = readIntLittle(data, recordContentStart);
            Area.box = new Double[4];
            // NORMAL //

            Area.box[0] = readDoubleLittle(data, recordContentStart + 4);
            Area.box[1] = readDoubleLittle(data, recordContentStart + 12);
            Area.box[2] = readDoubleLittle(data, recordContentStart + 20);
            Area.box[3] = readDoubleLittle(data, recordContentStart + 28);

            Area.numParts = readIntLittle(data, recordContentStart + 36);
            Area.parts = new int[Area.numParts];
            Area.numPoints = readIntLittle(data, recordContentStart + 40);
            Area.points = new PointF[Area.numPoints];
            int partStart = recordContentStart + 44;

            Area.Bounds = new List<double[]>();
            int[] startAt = new int[Area.numParts];
            int[] endBefore = new int[Area.numParts];
            for (int i = 0; i < Area.numParts; i++)
            {
                Area.parts[i] = readIntLittle(data, partStart + i * 4);
                if (i == 0)
                    Area.Bounds.Add(Area.box);
                else
                    Area.Bounds.Add(new double[] { double.MaxValue, double.MaxValue, double.MinValue, double.MinValue });
            };
            for (int i = 0; i < Area.numParts; i++)
            {
                startAt[i] = Area.parts[i];
                endBefore[i] = (i + 1) == Area.parts.Length ? Area.points.Length : Area.parts[i + 1];
            };
            int pointStart = recordContentStart + 44 + 4 * Area.numParts;
            for (int i = 0; i < Area.numPoints; i++)
            {
                Area.points[i].X = (float)readDoubleLittle(data, pointStart + (i * 16));
                Area.points[i].Y = (float)readDoubleLittle(data, pointStart + (i * 16) + 8);

                for (int p = 1; p < Area.numParts; p++)
                    if ((i >= startAt[p]) && (i < endBefore[p]))
                    {
                        Area.Bounds[p][0] = Math.Min(Area.Bounds[p][0], Area.points[i].X);
                        Area.Bounds[p][1] = Math.Min(Area.Bounds[p][1], Area.points[i].Y);
                        Area.Bounds[p][2] = Math.Max(Area.Bounds[p][2], Area.points[i].X);
                        Area.Bounds[p][3] = Math.Max(Area.Bounds[p][3], Area.points[i].Y);
                    };

            };
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

            public List<double[]> Bounds;

            /// <summary>
            ///     How many points in a polygon part
            /// </summary>
            /// <param name="part"></param>
            /// <returns></returns>
            public int GetPartCount(int part)
            {
                int startAt = this.parts[part];
                int endBefore = (part + 1) == this.parts.Length ? this.points.Length : this.parts[part + 1];
                return endBefore - startAt;
            }

            /// <summary>
            ///     Returns Polygon Part
            /// </summary>
            /// <param name="part"></param>
            /// <returns></returns>
            public PointF[] this[int part]
            {
                get
                {
                    if (this.numParts == 1) return this.points;

                    int startAt = this.parts[part];
                    int endBefore = (part + 1) == this.parts.Length ? this.points.Length : this.parts[part + 1];
                    PointF[] res = new PointF[endBefore - startAt];
                    Array.Copy(this.points, startAt, res, 0, res.Length);
                    return res;
                }
            }

            /// <summary>
            ///     Returns point of a polygon part    
            /// </summary>
            /// <param name="part"></param>
            /// <param name="point"></param>
            /// <returns></returns>
            public PointF this[int part, int point]
            {
                get
                {
                    int startIndex = this.parts[part];
                    return this.points[startIndex + point];
                }
            }                

            public void ReCalculateBounds()
            {
                if (points.Length == 0) return;

                float xl = points[0].X;
                float xr = points[0].X;
                float yt = points[0].Y;
                float yb = points[0].Y;
                if (points.Length == 1)
                {
                    box = new double[]{xl, yb, xl, yb};
                    return;
                };
                foreach (PointF pnt in points)
                {
                    if (pnt.X < xl) xl = pnt.X;
                    if (pnt.X > xr) xr = pnt.X;
                    if (pnt.Y > yt) yt = pnt.Y;
                    if (pnt.Y < yb) yb = pnt.Y;
                };
                box = new double[]{xl, yb, xr, yt};
            }
        }

        public enum ShapeFileType
        {
            tPolygon = 0x01,
            tPolyline = 0x02
        }
        #endregion            

        #region Проверка на пересечения, наложение и вхождение в полигон                
        private static int CRS(PointF P, PointF A1, PointF A2, double EPS)
        {
            double x;
            int res = 0;
            if (Math.Abs(A1.Y - A2.Y) < EPS)
            {
                if ((Math.Abs(P.Y - A1.Y) < EPS) && ((P.X - A1.X) * (P.X - A2.X) < 0.0)) res = -1;
                return res;
            };
            if ((A1.Y - P.Y) * (A2.Y - P.Y) > 0.0) return res;
            x = A2.X - (A2.Y - P.Y) / (A2.Y - A1.Y) * (A2.X - A1.X);
            if (Math.Abs(x - P.X) < EPS)
            {
                res = -1;
            }
            else
            {
                if (x < P.X)
                {
                    res = 1;
                    if ((Math.Abs(A1.Y - P.Y) < EPS) && (A1.Y < A2.Y)) res = 0;
                    else
                        if ((Math.Abs(A2.Y - P.Y) < EPS) && (A2.Y < A1.Y)) res = 0;
                };
            };
            return res;
        }

        // Попадает ли точка в полигон
        private static bool PointInPolygon(PointF point, PointF[] polygon, double EPS)
        {
            int count, up;
            count = 0;
            for (int i = 0; i < polygon.Length - 1; i++)
            {
                up = CRS(point, polygon[i], polygon[i + 1], EPS);
                if (up >= 0)
                    count += up;
                else
                    break;
            };
            up = CRS(point, polygon[polygon.Length - 1], polygon[0], EPS);
            if (up >= 0)
                return Convert.ToBoolean((count + up) & 1);
            else
                return false;
        }

        // Пересекаются ли полигоны
        private static bool IntersectPolygons(PointF[] pol1, PointF[] pol2, double EPS)
        {
            for (int i = 0; i < pol1.Length; i++) if (PointInPolygon(pol1[i], pol2, EPS)) return true;
            for (int i = 0; i < pol2.Length; i++) if (PointInPolygon(pol2[i], pol1, EPS)) return true;
            return false;
        }

        // лежит ли точка на отрезке линии
        public static bool PointInLineSegment(PointF point, PointF[] line)
        {            
            for (int i = 1; i < line.Length; i++)
            {
                double dx = (line[i].Y - line[i - 1].Y) / (line[i].X - line[i - 1].X);
                if ((Math.Abs((point.X - line[i - 1].X) * dx - (point.Y - line[i - 1].Y)) <= MaxError) && // удовлетворяет ли уравнению линии
                    (point.X >= Math.Min(line[i-1].X, line[i].X)) && // попадает ли в bounds отрезка
                    (point.X <= Math.Max(line[i-1].X, line[i].X)) && 
                    (point.Y >= Math.Min(line[i-1].Y, line[i].Y)) && 
                    (point.Y <= Math.Max(line[i-1].Y, line[i].Y))) return true;
            };
            return false;
        }


        private static bool IntersectLines(PointF line1start, PointF line1end, PointF line2start, PointF line2end)
        {
            PointF cr = GetTwoLinesCrossPoint(line1start, line1end, line2start, line2end);
            if ((cr.X == float.MaxValue) || (cr.Y == float.MaxValue)) return false;
            if ((cr.X < Math.Min(line1start.X, line1end.X)) || (cr.Y < Math.Min(line1start.Y, line1end.Y)) || (cr.X > Math.Max(line1start.X, line1end.X)) || (cr.Y > Math.Max(line1start.Y, line1end.Y)) || (cr.X < Math.Min(line2start.X, line2end.X)) || (cr.Y < Math.Min(line2start.Y, line2end.Y)) || (cr.X > Math.Max(line2start.X, line2end.X)) || (cr.Y > Math.Max(line2start.Y, line2end.Y)))
                return false;
            else
                return true;
        }

        private static PointF GetTwoLinesCrossPoint(PointF line1start, PointF line1end, PointF line2start, PointF line2end)
        {
            // Get A,B,C of first line - points : ps1 to pe1
            double A1 = line1end.Y - line1start.Y;
            double B1 = line1start.X - line1end.X;
            double C1 = A1 * line1start.X + B1 * line1start.Y;

            // Get A,B,C of second line - points : ps2 to pe2
            double A2 = line2end.Y - line2start.Y;
            double B2 = line2start.X - line2end.X;
            double C2 = A2 * line2start.X + B2 * line2start.Y;

            // Get delta and check if the lines are parallel
            double delta = A1 * B2 - A2 * B1;
            if (delta == 0) return new PointF(float.MaxValue, float.MaxValue);

            // now return the Vector2 intersection point
            PointF res = new PointF(
                (float)((B2 * C1 - B1 * C2) / delta),
                (float)((A1 * C2 - A2 * C1) / delta)
            );
            return res;
        }

        private static bool IntersectLineWithPolygon(PointF[] polyline, PointF[] polygone)
        {
            for (int l = 1; l < polyline.Length; l++)
                for (int p = 0; p < polygone.Length; p++)
                {
                    if (p != polygone.Length - 1)
                        if (IntersectLines(polyline[l-1], polyline[l], polygone[p], polygone[p + 1]))
                            return true;
                    if (p == polygone.Length - 1)
                        if (IntersectLines(polyline[l-1], polyline[l], polygone[p], polygone[0]))
                            return true;
                };
            return false;
        }

        private static bool LineInsideSquare(PointF[] polyline, PointF squareLT, PointF squareBR)
        {
            double[] line = new double[] { polyline[0].X, polyline[0].Y, polyline[0].X, polyline[0].Y };
            for (int i = 1; i < polyline.Length; i++)
            {
                line[0] = Math.Min(line[0], polyline[i].X);
                line[1] = Math.Min(line[1], polyline[i].Y);
                line[2] = Math.Max(line[2], polyline[i].X);
                line[3] = Math.Max(line[3], polyline[i].Y);
            };
            double[] square = new double[] { Math.Min(squareLT.X, squareBR.X), Math.Min(squareLT.Y, squareBR.Y), Math.Max(squareLT.X, squareBR.X), Math.Max(squareLT.Y, squareBR.Y) };
            return 
                (line[0] >= square[0]) &&
                (line[1] >= square[1]) && 
                (line[2] <= square[2]) &&                 
                (line[3] <= square[3]);
        }

        #endregion            

        #region TileLatLon
#if TileRenderer
        public static System.Drawing.Point location2tile(double lat, double lon, int zoom)
        {
            if (System.Math.Abs(lat) > 85.0511287798066) return new System.Drawing.Point(0, 0);

            double x, y;
            TilesProjection.fromLLToTile(lat, lon, zoom, out x, out y);
            return new System.Drawing.Point((int)x, (int)y);
        }
#endif

        // return X - lon
        // return Y - lat
        public static System.Drawing.PointF tile2location(double x, double y, int zoom)
        {
            double Lng, Lat;
#if TileCopier
            fromTileToLL(x, y, zoom, out Lat, out Lng);
#endif
#if TileRenderer
            TilesProjection.fromTileToLL(x, y, zoom, out Lat, out Lng);
#endif
            return new System.Drawing.PointF((float)Lng, (float)Lat);
        }

#if TileCopier
        private static void fromTileToLL(double x, double y, int zoom, out double Lat, out double Lng)
        {
            Lng = ((x * 256) - (256 * Math.Pow(2, zoom - 1))) / ((256 * Math.Pow(2, zoom)) / 360);
            while (Lng > 180) Lng -= 360;
            while (Lng < -180) Lng += 360;

            double exp = ((y * 256) - (256 * Math.Pow(2, zoom - 1))) / ((-256 * Math.Pow(2, zoom)) / (2 * Math.PI));
            Lat = ((2 * Math.Atan(Math.Exp(exp))) - (Math.PI / 2)) / (Math.PI / 180);
            if (Lat < -90) Lat = -90;
            if (Lat > 90) Lat = 90;
        } 
#endif
        #endregion
    }

    public class NetworkConnection : IDisposable
    {
        string _networkName = "";
        bool _connected = false;

        public bool IsConnected { get { return _connected; } }

        public NetworkConnection(string networkName, string user, string pass)
        {
            _networkName = networkName;

            NetResource netResource = new NetResource();
            netResource.Scope = ResourceScope.GlobalNetwork;
            netResource.ResourceType = ResourceType.Disk;
            netResource.DisplayType = ResourceDisplaytype.Share;
            netResource.RemoteName = networkName;

            int result = WNetAddConnection2(netResource, pass, user, 0);
            _connected = result == 0;
        }

        ~NetworkConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(_connected)
                WNetCancelConnection2(_networkName, 0, true);
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource,
            string password, string username, int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags,
            bool force);

        [StructLayout(LayoutKind.Sequential)]
        public class NetResource
        {
            public ResourceScope Scope;
            public ResourceType ResourceType;
            public ResourceDisplaytype DisplayType;
            public int Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }

        public enum ResourceScope : int
        {
            Connected = 1,
            GlobalNetwork,
            Remembered,
            Recent,
            Context
        };

        public enum ResourceType : int
        {
            Any = 0,
            Disk = 1,
            Print = 2,
            Reserved = 8,
        }

        public enum ResourceDisplaytype : int
        {
            Generic = 0x0,
            Domain = 0x01,
            Server = 0x02,
            Share = 0x03,
            File = 0x04,
            Group = 0x05,
            Network = 0x06,
            Root = 0x07,
            Shareadmin = 0x08,
            Directory = 0x09,
            Tree = 0x0a,
            Ndscontainer = 0x0b
        }
    }

    public class FHoles
    {
        private int _zooms = 0;
        private ulong _total = 0;
        private ulong[] _inzooms = new ulong[25];
        private List<int[]>[] _byzooms = new List<int[]>[25];

        //public FHoles()
        //{
        //    for (int i = 0; i < _byzooms.Length; i++) _byzooms[i] = new List<int[]>();

        //    // TEST

        //    _total = 18;

        //    _inzooms[10] = 4;
        //    for (int i = 0; i < 4; i++)
        //        _byzooms[10].Add(new int[] { 611, 313 + i, 10 });

        //    _inzooms[11] = 14;                
        //    for (int i = 0; i < 14; i++)
        //        _byzooms[11].Add(new int[] { 1223, 627+i, 11 });
        //}

        public FHoles(string fileName, int tileZoneSize)
        {
            for (int i = 0; i < _byzooms.Length; i++) _byzooms[i] = new List<int[]>();

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            bool isH = System.IO.Path.GetExtension(fileName).ToLower() == ".holes";
            StreamReader sr = new StreamReader(fs);
            string prev_l = "";
            string curr_l = "";
            while (!sr.EndOfStream)
            {
                string line = curr_l = sr.ReadLine();
                //
                if (isH) // ZN:1223,627,11;TRIED3
                {
                    if (line.IndexOf("ZN") == 0)
                    {
                        line = line.Substring(3);
                        line = line.Substring(0, line.IndexOf(";"));
                        string[] xyzs = line.Split(new string[] { "," }, StringSplitOptions.None);
                        int x = int.Parse(xyzs[0]);
                        int y = int.Parse(xyzs[1]);
                        int z = int.Parse(xyzs[2]);
                        if (_inzooms[z] == 0) _zooms++;
                        bool ex = false;
                        foreach (int[] xyz in _byzooms[z])
                            if ((xyz[0] == x) && (xyz[1] == y))
                            {
                                ex = true;
                                break;
                            };
                        if (!ex)
                        {
                            _byzooms[z].Add(new int[] { x, y, z });
                            _inzooms[z]++;
                            _total++;
                        };
                    };
                }
                else
                {
                    //THREAD 47304 TILE 1216x 632y 11z
                    //Error Saving Tile: A generic error occurred in GDI+.
                    if ((line.IndexOf("Error Saving Tile") == 0) && (prev_l.IndexOf("THREAD") == 0))
                    {
                        line = prev_l;
                        line = line.Substring(line.IndexOf("TILE") + 4).Trim().Replace(" ", "");
                        string[] xyzs = line.Split(new string[] { "x", "y", "z" }, StringSplitOptions.None);
                        int x = NormalizeXY(int.Parse(xyzs[0]), tileZoneSize);
                        int y = NormalizeXY(int.Parse(xyzs[1]), tileZoneSize);
                        int z = int.Parse(xyzs[2]);
                        if (_inzooms[z] == 0) _zooms++;
                        bool ex = false;
                        foreach (int[] xyz in _byzooms[z])
                            if ((xyz[0] == x) && (xyz[1] == y))
                            {
                                ex = true;
                                break;
                            };
                        if (!ex)
                        {
                            _byzooms[z].Add(new int[] { x, y, z });
                            _inzooms[z]++;
                            _total++;
                        };
                    };
                };
                //
                prev_l = curr_l;
            };
            fs.Close();
        }

        private int NormalizeXY(int val, int tileZoneSizeDiv)
        {
            int v = (int)(val / tileZoneSizeDiv);
            return v * tileZoneSizeDiv;
        }

        public int Zooms { get { return _zooms; } }

        public ulong Count { get { return _total; } }

        public ulong CountIn(int zoom)
        {
            return _inzooms[zoom];
        }

        public int[][] ZonesIn(int zoom)
        {
            return _byzooms[zoom].ToArray();
        }
    }


    public class ClientServerUtils
    {
        // http://www.pinvoke.net/default.aspx/kernel32.setthreadexecutionstate
        // https://msdn.microsoft.com/en-us/library/aa373208(VS.85).aspx
        // https://msdn.microsoft.com/ru-ru/library/windows/desktop/aa373208(v=vs.85).aspx
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040, // Enables away mode. This value must be specified with ES_CONTINUOUS.
            ES_CONTINUOUS = 0x80000000, // Informs the system that the state being set should remain in effect until the next call that uses ES_CONTINUOUS and one of the other state flags is cleared.
            ES_DISPLAY_REQUIRED = 0x00000002, // Forces the display to be on by resetting the display idle timer.
            ES_SYSTEM_REQUIRED = 0x00000001 // Forces the system to be in the working state by resetting the system idle timer.
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        /// <summary>
        ///     Prevent Only Monitor Power Down
        /// </summary>
        public static void KeepDisplayAwake()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
        }

        /// <summary>
        ///     Prevent Monitor Power Down And System
        /// </summary>
        public static void KeepDisplayAndSystemAwake()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED);
        }

        /// <summary>
        ///     Prevent system to sleep and reset idle timer
        /// </summary>
        public static void KeepSystemAwake()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED);
        }

        /// <summary>
        ///     Prevent system to sleep until AllowMonitorPowerdown will call
        /// </summary>
        public static void PreventSleep()
        {
            // Prevent Idle-to-Sleep (monitor not affected) (see note above)
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
        }

        /// <summary>
        ///     
        /// </summary>
        public static void AllowSleep()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }        
    }
}

