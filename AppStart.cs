using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MapnikTileRenderer
{
    public static class AppStart
    {
        /*
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);
        public static IntPtr ML;
         */

        public static string[] arguments;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            arguments = args;

            /* // TEST TileRenderShapeLimiter
            TileRendering.TileRenderShapeLimiter cfo = new TileRendering.TileRenderShapeLimiter(@"E:\_RENDER_MAP_TILES\LIPETS_20151029_BY_IGOR\bounds_cfo.shp");
            bool pin = cfo.PointIn(new System.Drawing.PointF((float)37.39, (float)55.45));
            bool lin = cfo.PointIn(new System.Drawing.PointF((float)39.97925, (float)53.23563));
            
            System.Drawing.Point tile = TileRendering.TileRenderShapeLimiter.location2tile(55.45, 37.39, 10);
            System.Drawing.PointF p2 = TileRendering.TileRenderShapeLimiter.tile2location(tile.X + 1, tile.Y + 1, 10);
            pin = cfo.SquareIn(new System.Drawing.PointF((float)37.39, (float)55.45), p2);
            pin = cfo.TileZoneIn(tile.X, tile.Y, tile.X + 1, tile.Y + 1, 10);
            pin = cfo.TileOnPolygonBorder(tile.X, tile.Y, 10);

            tile = TileRendering.TileRenderShapeLimiter.location2tile(53.23563, 39.97925, 10);
            p2 = TileRendering.TileRenderShapeLimiter.tile2location(tile.X + 1, tile.Y + 1, 10);
            lin = cfo.SquareIn(new System.Drawing.PointF((float)39.97925, (float)53.23563), p2);
            lin = cfo.TileZoneIn(tile.X, tile.Y, tile.X + 1, tile.Y + 1, 10);
            lin = cfo.TileOnPolygonBorder(tile.X, tile.Y, 10);

            bool gin = cfo.TileOnPolygonBorder(79, 42, 7);
            gin = cfo.TileOnPolygonBorder(77, 41, 7);
            gin = cfo.TileOnPolygonBorder(77, 40, 7);

            gin = cfo.TileZoneIn(79, 42, 79, 42, 7);
            gin = cfo.TileZoneIn(77, 41, 77, 41, 7);
            gin = cfo.TileZoneIn(77, 40, 77, 40, 7);

            gin = cfo.TileOutsidePolygonBorder(80, 41, 7);
            gin = cfo.TileOutsidePolygonBorder(79, 42, 7);
            gin = cfo.TileOutsidePolygonBorder(77, 41, 7);
            gin = cfo.TileOutsidePolygonBorder(77, 40, 7);
            gin = cfo.TileOutsidePolygonBorder(309, 160, 9);

            gin = cfo.TileOutsidePolygon(80, 41, 7);
            gin = cfo.TileOutsidePolygon(79, 42, 7);
            gin = cfo.TileOutsidePolygon(77, 41, 7);
            gin = cfo.TileOutsidePolygon(77, 40, 7);
            gin = cfo.TileOutsidePolygon(309, 160, 9);

            gin = cfo.TileInsidePolygonBorder(80, 41, 7);
            gin = cfo.TileInsidePolygonBorder(79, 42, 7);
            gin = cfo.TileInsidePolygonBorder(77, 41, 7);
            gin = cfo.TileInsidePolygonBorder(77, 40, 7);
            gin = cfo.TileInsidePolygonBorder(309, 160, 9);

            gin = cfo.TileInsidePolygon(80, 41, 7);
            gin = cfo.TileInsidePolygon(79, 42, 7);
            gin = cfo.TileInsidePolygon(77, 41, 7);
            gin = cfo.TileInsidePolygon(77, 40, 7);
            gin = cfo.TileInsidePolygon(309, 160, 9);
            */


            /* // TEST GEOMETRY
            TileRendering.TileRenderPolygon lip = new TileRendering.TileRenderPolygon(@"C:\Downloads\CD-REC\lipetsk.shp");
            TileRendering.TileRenderPolygon sib = new TileRendering.TileRenderPolygon(@"C:\Downloads\CD-REC\sibir.shp");
            NaviMapNet.Geometry.TPolygon plip = (NaviMapNet.Geometry.TPolygon)lip.Area.points;
            NaviMapNet.Geometry.TPolygon psib = (NaviMapNet.Geometry.TPolygon)sib.Area.points;
            NaviMapNet.Geometry.TPoint pnt = new NaviMapNet.Geometry.TPoint(39.51994, 52.58065);

            NaviMapNet.Geometry.TCircle c = new NaviMapNet.Geometry.TCircle(new NaviMapNet.Geometry.TPoint(37.39,55.45),0.5);
            NaviMapNet.Geometry.TPolygon p = new NaviMapNet.Geometry.TPolygon(new NaviMapNet.Geometry.TPoint[] { new NaviMapNet.Geometry.TPoint(37.39, 55.45), new NaviMapNet.Geometry.TPoint(37.89, 55.25), new NaviMapNet.Geometry.TPoint(37.79, 55.05) });

            //NaviMapNet.Geometry.TPolyline
            bool pinlip = NaviMapNet.Geometry.PointInPolygon(pnt, plip);
            bool pinsib = NaviMapNet.Geometry.PointInPolygon(pnt, psib);
            bool pinlipX = lip.TileIn(1246, 671, 11);
            bool pinsibX = sib.TileIn(1246, 671, 11);
            bool oneIntwo = lip.PolygonIn(lip.Area.points);
            long t1 = DateTime.Now.Ticks;
            bool crossA = NaviMapNet.Geometry.IntersectPolygons(plip, psib);
            long t2 = DateTime.Now.Ticks;
            bool crossB = NaviMapNet.Geometry.IntersectPolygonsB(plip, psib);
            long t3 = DateTime.Now.Ticks;
            long tA = t2 - t1;
            long tB = t3 - t2;
            */

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledError);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            /*
            string cd = GetCurrentDir();
            SetDllDirectory(cd);
            ML = LoadLibrary(cd + @"mapnik_c.dll");
             */

            Application.Run(new MainForm());
        }

        static void UnhandledError(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            AddErr2SysLog(e.ToString());
        }

        /// <summary>
        ///     Добавляем ошибку в системный лог
        /// </summary>
        /// <param name="msg"></param>
        public static void AddErr2SysLog(string msg)
        {
            try
            {
                string sSource = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                if (!System.Diagnostics.EventLog.SourceExists(sSource))
                    System.Diagnostics.EventLog.CreateEventSource(sSource, "Application");
                System.Diagnostics.EventLog.WriteEntry(sSource, msg, System.Diagnostics.EventLogEntryType.Error);
            }
            catch { };

            try
            {
                System.IO.FileStream fs = new System.IO.FileStream(GetCurrentDir() + @"\Error.log", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                fs.Position = fs.Length;
                byte[] data = System.Text.Encoding.GetEncoding(1251).GetBytes("\r\n\r\n" + DateTime.Now.ToString() + ": " + msg);
                fs.Write(data, 0, data.Length);
                fs.Close();
            }
            catch { };
        }

        /// <summary>
        ///     Добавляем в системный лог
        /// </summary>
        /// <param name="msg"></param>
        public static void Add2SysLog(string msg)
        {
            try
            {
                string sSource = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                if (!System.Diagnostics.EventLog.SourceExists(sSource))
                    System.Diagnostics.EventLog.CreateEventSource(sSource, "Application");
                System.Diagnostics.EventLog.WriteEntry(sSource, msg, System.Diagnostics.EventLogEntryType.Information);
            }
            catch { };                       
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
    }
}