using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Web;

namespace MapnikTileRendererConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if ((args == null) || (args.Length == 0))
            {
                Console.WriteLine("Mapnik Tile Renderer Console\r\n");
                Console.WriteLine("Синтаксис:");
                Console.WriteLine();
                Console.WriteLine("Standalone (режим командной строки)");
                Console.WriteLine("  mtrc.exe \"proj.mtr_project\" [options]");
                Console.WriteLine("    Options:");
                Console.WriteLine("       -wait - по завершении не закрывать окно, а ждать подтверждение");
                Console.WriteLine("       -errors - приостанавливать нарезку при возникновении ошибки");
                Console.WriteLine("       -err100 - приостанавливать нарезку при возникновении сотой ошибки");
                Console.WriteLine();
                Console.WriteLine("Server (режим сервера)");
                Console.WriteLine("  mtrc.exe \"proj.mtr_project\" /server 9666 [options]");
                Console.WriteLine("  mtrc.exe \"proj.mtr_project\" /dumpserver 9666 inputfile.dump [options]");
                Console.WriteLine("  mtrc.exe \"proj.mtr_project\" /holeserver 9666 inputfile.holes [options]");
                Console.WriteLine("    Options:");
                Console.WriteLine("      -task_## - идентификатор задания для выполнения (для разных заданий)");
                Console.WriteLine("      -cs - закрывать приложения сервера после окончания нарезки");
                Console.WriteLine("      -cc - закрывать приложения клиента после окончания нарезки");
                Console.WriteLine("      -dump - создавать и хранить информационную матрицу нарезки");
                Console.WriteLine();
                Console.WriteLine("Client (режим клиента)");
                Console.WriteLine("  mtrc.exe \"proj.mtr_project\" /client 127.0.0.1:9666 [options]");
                Console.WriteLine("    Option:");
                Console.WriteLine("      -task_## - идентификатор задания для выполнения (для разных заданий)");
                Console.WriteLine("      -threads_cn - число потоков нарезки = число ядер процессора(cores number)");
                Console.WriteLine("      -threads_## - число потоков нарезки (01..32)");
                Console.WriteLine();
                Console.WriteLine("...для более подробной информации см. файл \"mtrc.ReadMe.txt\"");
                System.Threading.Thread.Sleep(15000);
                return;
            };

            // by default is true, but save dump function awailable only in desktop application
            TileRendering.TileRenderingErrors.WithDump = false;

            string user = "";
            string pass = "";
            for (int i = 2; i < args.Length; i++)
            {
                string al = args[i].Trim().ToLower();
                if (al.IndexOf("-user:") == 0) user = al.Substring(6);
                if (al.IndexOf("-pass:") == 0) pass = al.Substring(6);
            };
            if ((user != String.Empty) && (pass != String.Empty))
            {
                string[] du = user.Split(new string[] { @"\" }, StringSplitOptions.None);
                string domain = (du.Length == 2) ? du[0] : "";
                user = (du.Length == 2) ? du[1] : user;
                SambaNetwork.Impersonator imp = new SambaNetwork.Impersonator(user, domain, pass);
            };

            string project = args[0];

            // Server
            if ((args.Length > 1) && (args[1].Trim().ToLower() == "/server"))
            {
                int port = 9666;
                try { port = int.Parse(args[2]); } catch { }
                bool closeServerWhenDone = false;
                bool closeClientsWhenDone = false;
                string task = "";
                for (int i = 2; i < args.Length; i++)
                {
                    string al = args[i].Trim().ToLower();
                    if (al == "-cs") closeServerWhenDone = true;
                    if (al == "-cc") closeClientsWhenDone = true;
                    if (al == "-dump") TileRendering.TileRenderingErrors.WithDump = true;
                    if (al.IndexOf("-task_") == 0) task = al;
                };                
                //HoleServer.StartHoles(project, port, closeClientsWhenDone, closeServerWhenDone, task);
                Server.Start(project, port, closeClientsWhenDone, closeServerWhenDone, task, null);
                return;
            };

            // Dump Server
            if ((args.Length > 1) && (args[1].Trim().ToLower() == "/dumpserver"))
            {
                TileRendering.TileRenderingErrors.WithDump = true; // dumpserver is only with dump

                string dumpFile = "";

                int port = 9666;
                try { port = int.Parse(args[2]); }
                catch
                {
                    try
                    {
                        dumpFile = args[2];
                    }
                    catch { };
                }
                bool closeServerWhenDone = false;
                bool closeClientsWhenDone = false;
                string task = "";
                for (int i = 2; i < args.Length; i++)
                {
                    string al = args[i].Trim().ToLower();
                    if (al == "-cs") closeServerWhenDone = true;
                    if (al == "-cc") closeClientsWhenDone = true;
                    
                    if (al.IndexOf("-task_") == 0) task = al;
                };
                if (!File.Exists(dumpFile))
                {
                    try
                    {
                        dumpFile = args[3].Trim().ToLower();
                    }
                    catch
                    {
                        Console.WriteLine("Ошибка передачи аргументов запуска");
                        Console.WriteLine("Файл дампа не указан");
                        return;
                    };
                    if (!File.Exists(dumpFile))
                    {
                        Console.WriteLine("Указанный файл дампа не найден!");
                        return;
                    };
                };
                Server.Start(project, port, closeClientsWhenDone, closeServerWhenDone, task, dumpFile);
                return;
            };

            // Hole Server
            if ((args.Length > 1) && (args[1].Trim().ToLower() == "/holeserver"))
            {
                string holesFile = "";

                int port = 9666;
                try { port = int.Parse(args[2]); }
                catch 
                {
                    try
                    {
                        holesFile = args[2];
                    }
                    catch { };
                }
                bool closeServerWhenDone = false;
                bool closeClientsWhenDone = false;
                string task = "";
                for (int i = 2; i < args.Length; i++)
                {
                    string al = args[i].Trim().ToLower();
                    if (al == "-cs") closeServerWhenDone = true;
                    if (al == "-cc") closeClientsWhenDone = true;
                    if (al.IndexOf("-task_") == 0) task = al;
                };
                if (!File.Exists(holesFile))
                {
                    try
                    {
                        holesFile = args[3].Trim().ToLower();
                    }
                    catch
                    {
                        Console.WriteLine("Ошибка передачи аргументов запуска");
                        Console.WriteLine("Файл с дырами не указан");
                        return;
                    };
                    if (!File.Exists(holesFile))
                    {
                        Console.WriteLine("Указанный файл с дырами не найден!");
                        return;
                    };
                };
                HoleServer.StartHoles(project, port, closeClientsWhenDone, closeServerWhenDone, task, holesFile);               
                return;
            };

            // Client
            if ((args.Length > 1) && (args[1].Trim().ToLower() == "/client"))
            {
                string remoteAddress = args[2];
                string task = "";
                for (int i = 2; i < args.Length; i++)
                {
                    string al = args[i].Trim().ToLower();
                    if (al.IndexOf("-task_") == 0) task = al;
                };
                int threads = 0;
                for (int i = 2; i < args.Length; i++)
                {
                    string al = args[i].Trim().ToLower();
                    if (al.IndexOf("-threads_") == 0)
                    {                        
                        al = al.Remove(0, 9);
                        if (al.ToLower() == "cn") // cores number
                            threads = System.Environment.ProcessorCount;
                        else
                        {
                            int.TryParse(al, out threads);
                            if (threads > 32) threads = 32;
                        };
                    };
                };
                Client.Start(project, remoteAddress, task, threads);
                return;
            };

            // StandAlone
            {
                StandAlone.start(args);
                return;
            };
        }
    }

    class StandAlone
    {
        static string fileName = "";
        static bool wait = false;
        static MapnikTileRenderer.MapnikTileRendererProject proj;
        static Thread _runtime_thread;
        static DateTime[] _runtime_started = new DateTime[22];
        static DateTime[] _runtime_stopped = new DateTime[22];
        static TileRendering.MultithreadTileRenderer[] _runtime_renderers = new TileRendering.MultithreadTileRenderer[22];
        static int _runtime_current_zoom = 0;
        static int _runtime_zooms_done = 0;
        static bool[] _runtime_zooms_todo = new bool[22];
        static ulong _runtime_total_to_render = 0;

        static byte timerOnTickUpdateFile = 0;
        static string LastError = "Ошибок нет";

        public static void start(string[] args)
        {
            for (int i = 1; i < args.Length; i++)
            {
                string al = args[i].Trim().ToLower();
                if (al == "-wait") wait = true;
                if (al == "-errors") TileRendering.TileRenderingErrors.PauseThreadsIfErrorsCountIs = 1;
                if (al == "-err100") TileRendering.TileRenderingErrors.PauseThreadsIfErrorsCountIs = 101;                
            };

            Console.Clear();
            Console.WriteLine("Mapnik Tile Renderer Console");
            fileName = System.IO.Path.GetFileName(args[0]);
            Console.WriteLine("Loading file: " + fileName);
            _runtime_stopped[0] = DateTime.MinValue;
            proj = MapnikTileRenderer.MapnikTileRendererProject.FromFile(args[0]);            
            _runtime_thread = new System.Threading.Thread(new System.Threading.ThreadStart(RunThread));
            _runtime_thread.Start();

            Console.WriteLine("Starting...\r\n");
            System.Threading.Thread.Sleep(1000);
            while (_runtime_stopped[0] == DateTime.MinValue)
            {                
                UpdateStatus();
                System.Threading.Thread.Sleep(1000);
            };

            // finally save
            _runtime_total_to_render = 0;
            UpdateStatus(true);
        }

        static void BackUpLogs()
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

        static void RunThread()
        {
            BackUpLogs();

            string errLogFileName = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\console_[" + fileName + "].errorlog";
            if (System.IO.File.Exists(errLogFileName)) System.IO.File.Delete(errLogFileName);

            TileRendering.MultithreadTileRenderer.holes_fileName = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\console_[" + fileName + "].holes";
            if (System.IO.File.Exists(TileRendering.MultithreadTileRenderer.holes_fileName)) System.IO.File.Delete(TileRendering.MultithreadTileRenderer.holes_fileName);

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
                        if(!System.IO.File.Exists(proj.TilesPath + @"\ReadMeFirst.txt"))
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
                    _runtime_renderers[currentZoom].RenderTiles(sy, sx, ey, ex, currentZoom);
                    _runtime_stopped[currentZoom] = DateTime.Now;
                    _runtime_zooms_done++;
                }
                catch { _runtime_zooms_todo[0] = false; };
                System.Threading.Thread.Sleep(1);
            };

            _runtime_stopped[0] = DateTime.Now;
            UpdateStatus(true);

            Console.WriteLine();
            Console.WriteLine("DONE");
            Console.WriteLine("Log saved into file: LOGS\\console_["+fileName+"].log");
            if (TileRendering.TileRenderingErrors.ErrorQueueTotalCount > 0)
                Console.WriteLine("Error log saved into file: LOGS\\console_[" + fileName + "].errorlog");
            if (wait)
            {
                Console.WriteLine("Press Enter To Continue");
                Console.ReadLine();
            };
        }

        static void UpdateStatus() { UpdateStatus(false); }

        static void UpdateStatus(bool saveToFileNow)
        {
            System.Globalization.CultureInfo stream_CultureInfo = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)stream_CultureInfo.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

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
                    chZooms += chZooms.Length > 0 ? ", " + i.ToString() : i.ToString();
                };
            };

            string status = "";
            status += String.Format("Состояние: {0}, нарезано {1} зумов из {2}\r\n", _runtime_thread.IsAlive ? "Идет нарезка " + _runtime_current_zoom.ToString() + " зума" : (_runtime_zooms_todo[0] ? "Нарезка завершена" : "Нарезка прервана"), _runtime_zooms_done, total_zooms);
            status += String.Format("Выбранные зумы для нарезки: {0}\r\n", chZooms);
            status += String.Format("Зона для нарезки: ( {0} ; {1} ) .. ( {2} ; {3} ) - {4}\r\n", new object[] { proj.Zone[1], proj.Zone[2], proj.Zone[3], proj.Zone[4], proj.Zone[0] });
            status += String.Format("Размер изображения при отрисовки карты (зоны): {0}\r\n", proj.RendererConfig.tileZoneSize.ToString());
            status += String.Format("При ошибках отрисовки пробывать нарезать зону повторно: {0}\r\n", proj.RendererConfig.addToQueueAgainIfError ? "да" : "нет");
            status += String.Format("Если сохряняемый тайл уже существует: {0}\r\n", GetDescription(proj.RendererConfig.tileFinalMerging).ToLower());
            status += String.Format("Обрабатывать тайлы только попадающие в полигон/полилиную: {0}\r\n", proj.RendererConfig.renderOnlyInPolygon != String.Empty ? "`" + System.IO.Path.GetFileName(proj.RendererConfig.renderOnlyInPolygon) + "`" : "нет");
            status += String.Format("Накладывать водяной знак из файла: {0}\r\n", proj.RendererConfig.addCopyrightsFile != String.Empty ? "`"+System.IO.Path.GetFileName(proj.RendererConfig.addCopyrightsFile) + "`" : "нет");
            status += String.Format("Число потоков: {0}, ошибок: {1}\r\n", proj.RendererConfig.multithreadCount, TileRendering.TileRenderingErrors.ErrorQueueTotalCount);
            status += String.Format("\r\nНачато: {0}\r\n", _runtime_started[0]);
            for (int i = 0; i < _runtime_zooms_todo.Length; i++)
                if ((checkedZooms.IndexOf(i + 1) >= 0) && ((_runtime_zooms_done == total_zooms) || (_runtime_current_zoom >= (i + 1))))
                {
                    status += "\r\n";
                    status += String.Format("Зум {0}\r\n", i + 1);
                    status += String.Format("  - Начато: {0}\r\n", _runtime_started[i + 1]);
                    if (_runtime_renderers[i + 1] != null)
                    {
                        status += String.Format("  - Тайл обр: {0} из {1}, сзд {2}, ппщ {3}, дыр {4} в {5} квдр\r\n", new object[] { _runtime_renderers[i + 1].status_total_passed, _runtime_renderers[i + 1].status_total_to_render, _runtime_renderers[i + 1].status_total_created, _runtime_renderers[i + 1].status_total_skipped, _runtime_renderers[i+1].status_total_witherr, _runtime_renderers[i+1].status_total_zoneerr });
                        status += String.Format("  - Номера тайлов: x {0} из {1}..{2}, y {3} из {4}..{5}\r\n", new object[] { _runtime_renderers[i + 1].status_x_current, _runtime_renderers[i + 1].status_x_start, _runtime_renderers[i + 1].status_x_end, _runtime_renderers[i + 1].status_y_current, _runtime_renderers[i + 1].status_y_start, _runtime_renderers[i + 1].status_y_end });
                        status += String.Format(ni, "  - Размер созданных тайлов: {0}\r\n", TileRendering.TileRenderer.FileSizeSuffix(_runtime_renderers[i + 1].status_total_size));
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
            status += String.Format("Тайл обр: {0} из {1}/{4}, сзд {2}, ппщ {3}, дыр {5} в {6} квдр\r\n", new object[] { total_passed, total_to_render, total_created, total_skipped, _runtime_total_to_render, total_witherr, total_zoneerr });
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
                TimeSpan ts = DateTime.Now.Subtract(_runtime_started[0]);
                //status += String.Format("Выполнено: {0}%\r\n", (int)(((double)total_passed) / ((double)_runtime_total_to_render) * 100.0));
                status += String.Format("Выполняется: {0} дн {1} ч {2} м\r\n", new object[] { (int)ts.TotalDays, ts.Hours, ts.Minutes, ts.Seconds });
            };

            status += String.Format("Выполнено {0}%\r\n", (int)(((double)total_passed) / ((double)_runtime_total_to_render) * 100.0));


            Console.Clear();
            Console.Write(status);
            Console.WriteLine("Ошибок: "+TileRendering.TileRenderingErrors.ErrorQueueTotalCount.ToString());

            if (saveToFileNow || (timerOnTickUpdateFile++ % 8 == 0)) // 1 time per 8 seconds
            {
                try
                {
                    System.IO.FileStream fs = new System.IO.FileStream(TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\console_[" + fileName + "].log", System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    byte[] data = System.Text.Encoding.GetEncoding(1251).GetBytes(status);
                    fs.Write(data, 0, data.Length);
                    fs.Close();
                }
                catch { };
            };

            //
            // LOG ERRORS
            //

            // read stack errors
            List<TileRendering.TileRenderingErrors.ErrorInfo> errors = new List<TileRendering.TileRenderingErrors.ErrorInfo>();
            TileRendering.TileRenderingErrors.ErrorQueueMutex.WaitOne();
            while (TileRendering.TileRenderingErrors.ErrorQueue.Count > 0)
                errors.Add(TileRendering.TileRenderingErrors.ErrorQueue.Dequeue());
            TileRendering.TileRenderingErrors.ErrorQueueMutex.ReleaseMutex();

            bool skip_out_err = false;
            if (errors.Count > 0)
                for (int i = 0; i < errors.Count; i++)
                {
                    TileRendering.TileRenderingErrors.ErrorInfo err = errors[i];
                    string txt = String.Format(
                        "\r\n{0}\r\nTHREAD {4} TILE {1}x {2}y {3}z\r\n{5}: {6}\r\nParameters: {7}\r\n"
                        , new object[]{
                        err.dt, err.x, err.y, err.z, err.ThreadID, err.comment, err.ex.Message, err.parameter
                    });

                    LastError = txt;

                    try
                    {
                        System.IO.FileStream fs = new System.IO.FileStream(TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\console_[" + fileName + "].errorlog", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
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
                            skip_out_err = true;
                            Console.Write("\r\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\r\n"+
                                     "При выполнении нарезки в одном из потоков возникла ошибка: \r\n" + 
                                     "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\r\n" + txt + "\r\n");
                            Console.WriteLine("Прервать выполнение всех операций?\r\nY - Прервать нарезку\r\nN - Не прерывать, продолжить нарезку\r\nI - Продолжить нарезку и больше не выводить сообщение об ошибке");
                            Console.Write("Ваш выбор: ");
                            string ych = Console.ReadLine().ToUpper();
                            if (ych == "I")
                            {
                                TileRendering.TileRenderingErrors.PauseThreadsIfErrorsCountIs = 0;
                                TileRendering.TileRenderingErrors.PausedThreadsMutex.WaitOne();
                                TileRendering.TileRenderingErrors.PausedThreads.Remove(err.ThreadID);
                                TileRendering.TileRenderingErrors.PausedThreadsMutex.ReleaseMutex();
                            };
                            if ((ych == "Y") || (ych == "A"))
                            {
                                Console.WriteLine("You can shut down this window");
                                throw new Exception("EXIT");
                            };
                            TileRendering.TileRenderingErrors.PausedThreadsMutex.WaitOne();
                            TileRendering.TileRenderingErrors.PausedThreads.Remove(err.ThreadID);
                            TileRendering.TileRenderingErrors.PausedThreadsMutex.ReleaseMutex();
                        };
                    };
                };

            if(!skip_out_err)
                Console.WriteLine("Последняя ошибка:\r\n" + LastError);
        }

        public static string GetDescription(Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }
    }

    class HoleServer : Server
    {
        private static TileRendering.FHoles curr_holes = null;

        static void BackUpLogs()
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

        public static void StartHoles(string projFile, int port, bool closeClientsWhenDone, bool closeServerWhenDone, string task, string holesFile)
        {            
            curr_task = task;

            Console.Clear();
            Console.WriteLine("Mapnik Tile Renderer Server");
            Console.WriteLine("Loading file: " + System.IO.Path.GetFileName(projFile));

            localServerPort = port;
            localServerTask = task.Replace("-", "");
            projectFileName = System.IO.Path.GetFileName(projFile);
            project = MapnikTileRenderer.MapnikTileRendererProject.FromFile(projFile);
            _runtime_stopped[0] = DateTime.MinValue;
            closeAllClientsWhenDone = closeClientsWhenDone;

            string tilesPath = project.TilesPath + @"\";
            if (project.RendererConfig.pathStructure == TileRendering.TileRenderPathScheme.fsArcGIS)
                tilesPath = project.TilesPath + @"\_alllayers\";

            if (!CheckDistinationFolderAccess(tilesPath))
            {
                Console.WriteLine();
                Console.WriteLine("Возможно на клиентах тоже нет доступа к папке назначения!");
                Console.WriteLine("Если вы хотите продолжить независимо от предупреждения, нажмите `Enter`");
                Console.ReadLine();
            };
            Console.WriteLine();

            Console.WriteLine("Загрузка дыр из файла '" + System.IO.Path.GetFileName(holesFile) + "'...");
            try
            {
                //E:\__MAPNIK\app\LOGS\--sample.holes            
                curr_holes = new TileRendering.FHoles(holesFile, (int)project.RendererConfig.tileZoneSize);
                Console.WriteLine(String.Format("Найдены дыры в {0} квадратах в {1} зумах", curr_holes.Count, curr_holes.Zooms));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удается открыть файл дыр!");
                return;
            };

            Console.WriteLine("Запуск...");
            Console.WriteLine();
            text_specified_by_child = String.Format("Нарезка {0} квадратов c дырами в {1} зумах из '{2}'\r\n", new object[] { curr_holes.Count, curr_holes.Zooms, System.IO.Path.GetFileName(holesFile) });
            System.Threading.Thread.Sleep(1000);

            BackUpLogs();

            string holesLogFileName = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\server_[" + projectFileName + "]" + curr_task + ".holes";
            if (System.IO.File.Exists(holesLogFileName)) System.IO.File.Delete(holesLogFileName);

            (_thr_Run = new System.Threading.Thread(new System.Threading.ThreadStart(RunThreadHoles))).Start();
            (_thr_Svr = new HTTPListener(localServerPort, 5, task)).Start();
            (_thr_Fld = new System.Threading.Thread(new ThreadStart(RunThreadCheckBrokenClient))).Start();
            (_thr_Stt = new System.Threading.Thread(new System.Threading.ThreadStart(StatusThread))).Start();

            while (_thr_Run.IsAlive)
            {
                Thread.Sleep(15000);
            };

            // wait until all client will get exit command
            if (closeAllClientsWhenDone)
                Thread.Sleep(15000);

            if (closeServerWhenDone)
            {
                _thr_Svr.Stop();
                _thr_Fld.Abort();
            };

            stateAlive = false;
        }

        protected static void RunThreadHoles()
        {            
            int tzs = (int)(project.RendererConfig.tileZoneSize);
            tzs = tzs * tzs;

            _runtime_zooms_todo[0] = true; // started
            _runtime_total_to_render = 0;
            _runtime_zooms_done = 0;
            _runtime_started[0] = DateTime.Now;
            _runtime_stopped[0] = DateTime.MinValue;
            Queue<int> zooms2run = new Queue<int>();
            for (int i = 1; i < project.zooms.Length; i++)
            {
                _runtime_renderers[i] = null;
                _runtime_started[i] = DateTime.MinValue;
                _runtime_stopped[i] = DateTime.MinValue;
                ulong inz = curr_holes.CountIn(i) * (ulong)tzs;
                if (inz > 0)
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

                string tilesPath = project.TilesPath + @"\";
                if (project.RendererConfig.pathStructure == TileRendering.TileRenderPathScheme.fsArcGIS)
                {
                    tilesPath = project.TilesPath + @"\_alllayers\";
                    try
                    {
                        System.IO.Directory.CreateDirectory(tilesPath);
                        System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() +
                            (currentZoom < 10 ? @"\ArcGIS_conf[04..09].xml" : @"\ArcGIS_conf[10..21].xml"),
                            project.TilesPath + @"\conf.xml", true);
                        if (!System.IO.File.Exists(project.TilesPath + @"\ReadMeFirst.txt"))
                            System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() + "ArcGIS_RMF.txt", project.TilesPath + @"\ReadMeFirst.txt", false);
                        if (!System.IO.File.Exists(project.TilesPath + @"\MergedTilesSimpleViewer.exe"))
                            System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() + "MergedTilesSimpleViewer.bin", project.TilesPath + @"\MergedTilesSimpleViewer.exe", false);
                        if (!System.IO.File.Exists(project.TilesPath + @"\MakeTRZ.cmd"))
                            System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() + "MakeTRZ.txt", project.TilesPath + @"\MakeTRZ.cmd", false);

                    }
                    catch { };
                };

                try
                {
                    _runtime_renderers[currentZoom] = new TileRendering.MultithreadTileRenderer("", "");
                    RenderTilesList(curr_holes.ZonesIn(currentZoom), currentZoom);
                }
                catch (Exception ex)
                { 
                    _runtime_zooms_todo[0] = false; 
                };
                System.Threading.Thread.Sleep(1);
            };

            while ((queueTotal > 0) || (queueRunTotal > 0))
                Thread.Sleep(1000);

            hasEnded = true;
            _runtime_stopped[0] = DateTime.Now;
        }

        protected static void RenderTilesList(int[][] tilelist, int zoom)
        {
            if ((zoom < TileRendering.TileRenderer.minZoom) || (zoom > TileRendering.TileRenderer.maxZoom)) return;

            // Prepare
            tileZoneSizeDiv = (int)project.RendererConfig.tileZoneSize;
            tileZoneSizeCount = tileZoneSizeDiv * tileZoneSizeDiv;

            _runtime_renderers[zoom].status_x_start = tilelist[0][0];
            _runtime_renderers[zoom].status_x_end = tilelist[0][0];
            _runtime_renderers[zoom].status_y_start = tilelist[0][1];
            _runtime_renderers[zoom].status_y_end = tilelist[0][1];
            for (int i = 0; i < tilelist.Length; i++)
            {
                _runtime_renderers[zoom].status_x_start = Math.Min(_runtime_renderers[zoom].status_x_start, tilelist[i][0]);
                _runtime_renderers[zoom].status_x_end = Math.Max(_runtime_renderers[zoom].status_x_start, tilelist[i][0]);
                _runtime_renderers[zoom].status_y_start = Math.Min(_runtime_renderers[zoom].status_y_start, tilelist[i][1]);
                _runtime_renderers[zoom].status_y_end = Math.Max(_runtime_renderers[zoom].status_y_start, tilelist[i][1]);
            };

            // prepare status
            // prepare status
            int tzs = (int)tileZoneSizeDiv;
            tzs = tzs * tzs;
            _runtime_renderers[zoom].status_total_to_render = (ulong)tilelist.Length * (ulong)tzs;
            _runtime_renderers[zoom].status_total_created = 0;
            _runtime_renderers[zoom].status_total_passed = 0;
            _runtime_renderers[zoom].status_total_size = 0;

            //ADD TO QUEUE            
            for (int i = 0; i < tilelist.Length; i++)
            {
                // добавляем в очередь
                bool added = false;
                while (!added)
                {
                    queueMtx.WaitOne();
                    if (queue.Count < queueMax)
                    {
                        queue.Enqueue(tilelist[i]);
                        queueTotal++;
                        added = true;
                    };
                    queueMtx.ReleaseMutex();
                    System.Threading.Thread.Sleep(5);
                };
            };

            // done
        }
    }

    class Server
    {
        public static string sversion = "2019-01-14-B";

        protected static string text_specified_by_child = String.Empty;

        public class SClientList : List<SClient>
        {
            public int IndexOf(string ip, string id)
            {
                if (this.Count == 0) return -1;
                for (int i = 0; i < this.Count; i++)
                    if ((this[i].ip == ip) && (this[i].id == id)) return i;
                return -1;
            }
        }

        public class SClient
        {
            public byte clrwError = 0;

            public string ip = "";
            public string id = "";
            public string th = "1";
            public DateTime la = DateTime.UtcNow;

            public ulong queued = 0;
            public ulong created = 0;
            public ulong skipped = 0;
            public ulong witherr = 0;
            public ulong itec = 0; // число последовательных ошибок
            public long alive = 0;

            public SClient() { }
            public SClient(string ip) { this.ip = ip; }
            public SClient(string ip, string id) { this.ip = ip; this.id = id; }
            public SClient(string ip, string id, string th) { this.ip = ip; this.id = id; this.th = th; }
            public override string ToString()
            {
                if ((clrwError == 1) && (queued == 0))
                    return ip + " (ОШБК ЗАП: Не удается произвести запись в папку с тайлами)";
                else
                    return ip + " [пот:" + th + ",зад:" + queued.ToString() + ",врб:" + alive.ToString() + ";сзд:" + created.ToString() + ",ппщ:" + skipped + ",дыр:" + witherr.ToString() + (itec > 0 ? ",чпо+" + itec.ToString() + (itec > 5 ? "!" : "") : "") + "]";
            }
        }

        // Query Tile Zone Mow
        public struct QeuryTileZoneByClientNow { public int x; public int y; public int z; public DateTime started; public QeuryTileZoneByClientNow(int x, int y, int z) { this.x = x; this.y = y; this.z = z; this.started = DateTime.Now; } }

        protected static int localServerPort = 9666;
        protected static string localServerTask = "";
        protected static MapnikTileRenderer.MapnikTileRendererProject project;
        protected static string projectFileName = "";
        protected static SClientList ClientList = new SClientList();
        protected static Mutex clmtx = new Mutex();
        protected static string curr_task = "";

        protected static DateTime[] _runtime_started = new DateTime[22];
        protected static DateTime[] _runtime_stopped = new DateTime[22];
        protected static TileRendering.MultithreadTileRenderer[] _runtime_renderers = new TileRendering.MultithreadTileRenderer[22];
        protected static int _runtime_current_zoom = 0;
        protected static int _runtime_zooms_done = 0;
        protected static bool[] _runtime_zooms_todo = new bool[22];
        protected static ulong _runtime_total_to_render = 0;

        protected static bool closeAllClientsWhenDone = false;
        protected static Mutex queueMtx = new Mutex();
        protected static Queue<int[]> queue = new Queue<int[]>();
        protected static List<int[]> queueErr = new List<int[]>();
        protected static List<QeuryTileZoneByClientNow> queueRun = new List<QeuryTileZoneByClientNow>();
        protected static ulong queueTotal = 0;
        protected static ulong queueRunTotal = 0;
        protected static int queueMax = 32;
        protected static byte errorRepeatTries = 3;
        protected static bool hasEnded = false;

        protected static int tileZoneSizeDiv = 8;
        protected static int tileZoneSizeCount = 8 * 8;

        protected static byte timerOnTickUpdateFile = 0;

        protected static Thread _thr_Run;
        protected static Thread _thr_Stt;
        protected static Thread _thr_Fld;
        protected static HTTPListener _thr_Svr;

        protected static bool CheckDistinationFolderAccess(string tilesPath)
        {
            Console.WriteLine();
            Console.WriteLine("Проверка доступа к папке с тайлами:");
            Console.WriteLine("  " + tilesPath);

            bool de = false;
            try
            {
                de = Directory.Exists(tilesPath);
                Console.WriteLine("Папка " + (de ? "найдена" : "не найдена"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: Не удается найти и получить доступ к папке!");
            };

            if (!de)
            {
                try
                {
                    de = Directory.CreateDirectory(tilesPath).Exists;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: Не удается создать папку!");
                };
            };

            string fn = tilesPath + @"\" + curr_task + ".txt";
            bool ok = false;
            try
            {
                FileStream fs = new FileStream(fn, FileMode.Create, FileAccess.Write);
                fs.WriteByte(0x30);
                fs.WriteByte(0x53);
                fs.Close();
                ok = true;
                Console.WriteLine("Наличие прав на запись в папку с тайлами подтверждено!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: Отсутствует папка или права на запись!");
            };
            return ok;
        }

        public static string loadDumpFile = null;

        static void BackUpLogs()
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

        public static void Start(string projFile, int port, bool closeClientsWhenDone, bool closeServerWhenDone, string task, string dumpFile)
        {
            curr_task = task;
            
            Console.Clear();
            Console.WriteLine("Mapnik Tile Renderer Server");
            Console.WriteLine("Loading file: " + System.IO.Path.GetFileName(projFile));
            
            localServerPort = port;
            localServerTask = task.Replace("-","");
            projectFileName = System.IO.Path.GetFileName(projFile);
            project = MapnikTileRenderer.MapnikTileRendererProject.FromFile(projFile);
            _runtime_stopped[0] = DateTime.MinValue;
            closeAllClientsWhenDone = closeClientsWhenDone;

            string tilesPath = project.TilesPath + @"\";
            if (project.RendererConfig.pathStructure == TileRendering.TileRenderPathScheme.fsArcGIS)
                tilesPath = project.TilesPath + @"\_alllayers\";

            if (!CheckDistinationFolderAccess(tilesPath))
            {
                Console.WriteLine();
                Console.WriteLine("Возможно на клиентах тоже нет доступа к папке назначения!");
                Console.WriteLine("Если вы хотите продолжить независимо от предупреждения, нажмите `Enter`");
                Console.ReadLine();
            };
            Console.WriteLine("Запуск...");
            Console.WriteLine();
            if((dumpFile != null) &&  (dumpFile != String.Empty))
            {
                text_specified_by_child = String.Format("Нарезка с учетом дампа из '{0}'\r\n", new object[] { System.IO.Path.GetFileName(dumpFile) });
                loadDumpFile = dumpFile;
            };

            BackUpLogs();

            string holesLogFileName = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\server_[" + projectFileName + "]" + curr_task + ".holes";
            if (System.IO.File.Exists(holesLogFileName)) System.IO.File.Delete(holesLogFileName);

            (_thr_Run = new System.Threading.Thread(new System.Threading.ThreadStart(RunThread))).Start();            
            (_thr_Svr = new HTTPListener(localServerPort, 5, task)).Start();
            (_thr_Fld = new System.Threading.Thread(new ThreadStart(RunThreadCheckBrokenClient))).Start();
            (_thr_Stt = new System.Threading.Thread(new System.Threading.ThreadStart(StatusThread))).Start();

            while (_thr_Run.IsAlive)
            {                
                Thread.Sleep(15000);
            };

            // wait until all client will get exit command
            if (closeAllClientsWhenDone)
                Thread.Sleep(15000);

            AutoSaveDump();

            if (closeServerWhenDone)
            {
                _thr_Svr.Stop();
                _thr_Fld.Abort();
            };            

            stateAlive = false;

            // finally save to file
            timerOnTickUpdateFile = 0;
            UpdateStatus(true);
        }

        protected static bool stateAlive = true;
        protected static void StatusThread()
        {
            while (stateAlive)
            {
                UpdateStatus(true);
                Thread.Sleep(2000);
            };
        }

        // Save Dump Every 1 hour //
        private static DateTime lastDUMPauto = DateTime.MinValue;
        private static void AutoSaveDump()
        {
            if (!TileRendering.TileRenderingErrors.WithDump) return;
            if (DateTime.UtcNow.Subtract(lastDUMPauto).TotalHours < 3) return;
            lastDUMPauto = DateTime.UtcNow;
            
            List<TileRendering.RenderView> data = new List<TileRendering.RenderView>();
            if (_runtime_renderers != null)
                for (int i = 0; i < _runtime_renderers.Length; i++)
                    if (_runtime_renderers[i] != null)
                        if (_runtime_renderers[i].VIEW != null)
                            data.Add(_runtime_renderers[i].VIEW);
            
            if (data.Count > 0)
            {
                try
                {
                    System.IO.FileStream fs = new System.IO.FileStream(TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\server_[" + projectFileName + "]" + curr_task + ".dump", System.IO.FileMode.Create, System.IO.FileAccess.Write);
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

        static void RunThread()
        {
            // определеям координаты зоны
            double sx = project.Zone[1];
            double sy = project.Zone[2];
            double ex = project.Zone[3];
            double ey = project.Zone[4];
            {
                //градусы                
                //if (proj.Zone[0] == 0) { };

                //метры проекции
                if (project.Zone[0] == 1)
                {
                    sx = TileRendering.TilesProjection.x2lon_m(sx);
                    sy = TileRendering.TilesProjection.y2lat_m(sy);
                    ex = TileRendering.TilesProjection.x2lon_m(ex);
                    ey = TileRendering.TilesProjection.y2lat_m(ey);
                };
                //номера тайлов
                if (project.Zone[0] == 2)
                {
                    double tmp_lat; double tmp_lon;
                    TileRendering.TilesProjection.fromTileToLL((int)sx, (int)sy, (int)project.Zone[5] + 1, out tmp_lat, out tmp_lon);
                    sx = tmp_lon; sy = tmp_lat;
                    TileRendering.TilesProjection.fromTileToLL((int)ex, (int)ey, (int)project.Zone[5] + 1, out tmp_lat, out tmp_lon);
                    ex = tmp_lon; ey = tmp_lat;
                };
                //координаты в пикселях
                if (project.Zone[0] == 3)
                {
                    double[] ll = TileRendering.TilesProjection.fromPixelToLL(new double[] { sy, sx }, (int)project.Zone[5] + 1);
                    sx = ll[0]; sy = ll[1];
                    ll = TileRendering.TilesProjection.fromPixelToLL(new double[] { ey, ex }, (int)project.Zone[5] + 1);
                    ex = ll[0]; ey = ll[1];
                };
            };

            _runtime_zooms_todo[0] = true; // started
            _runtime_total_to_render = 0;
            _runtime_zooms_done = 0;
            _runtime_started[0] = DateTime.Now;
            _runtime_stopped[0] = DateTime.MinValue;
            Queue<int> zooms2run = new Queue<int>();
            for (int i = 1; i < project.zooms.Length; i++)
            {
                _runtime_renderers[i] = new TileRendering.MultithreadTileRenderer("", "");
                _runtime_started[i] = DateTime.MinValue;
                _runtime_stopped[i] = DateTime.MinValue;
                _runtime_zooms_todo[i] = project.zooms[i];
                if (project.zooms[i])
                {
                    zooms2run.Enqueue(i);
                    _runtime_total_to_render += TileRendering.MultithreadTileRenderer.RenderTilesCount(sy, sx, ey, ex, i, project.RendererConfig.tileZoneSize);
                };
            };

            while ((zooms2run.Count > 0) && (_runtime_zooms_todo[0]))
            {
                int currentZoom = zooms2run.Dequeue();
                _runtime_current_zoom = currentZoom;
                _runtime_started[currentZoom] = DateTime.Now;

                string tilesPath = project.TilesPath + @"\";
                if (project.RendererConfig.pathStructure == TileRendering.TileRenderPathScheme.fsArcGIS)
                {
                    tilesPath = project.TilesPath + @"\_alllayers\";
                    try
                    {
                        System.IO.Directory.CreateDirectory(tilesPath);
                        System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() +
                            (currentZoom < 10 ? @"\ArcGIS_conf[04..09].xml" : @"\ArcGIS_conf[10..21].xml"),
                            project.TilesPath + @"\conf.xml", true);
                        if (!System.IO.File.Exists(project.TilesPath + @"\ReadMeFirst.txt"))
                            System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() + "ArcGIS_RMF.txt", project.TilesPath + @"\ReadMeFirst.txt", false);
                        if (!System.IO.File.Exists(project.TilesPath + @"\MergedTilesSimpleViewer.exe"))
                            System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() + "MergedTilesSimpleViewer.bin", project.TilesPath + @"\MergedTilesSimpleViewer.exe", false);
                        if (!System.IO.File.Exists(project.TilesPath + @"\MakeTRZ.cmd"))
                            System.IO.File.Copy(TileRendering.TileRenderer.GetCurrentDir() + "MakeTRZ.txt", project.TilesPath + @"\MakeTRZ.cmd", false);                        

                    }
                    catch { };
                };

                try
                {
                    RenderTiles(sy, sx, ey, ex, currentZoom);
                }
                catch { _runtime_zooms_todo[0] = false; };
                System.Threading.Thread.Sleep(1);
            };

            while ((queueTotal > 0) || (queueRunTotal > 0))
                Thread.Sleep(1000);
            
            hasEnded = true;
            _runtime_stopped[0] = DateTime.Now;
        }

        protected static void RunThreadCheckBrokenClient()
        {
            while (true)
            {
                // LOST QUEUE - NO STATUS UPDATE
                bool removed = false;
                if (queueRunTotal > 0)
                {
                    queueMtx.WaitOne();
                    for (int i = queueRun.Count - 1; i >= 0; i--)
                        if (DateTime.Now.Subtract(queueRun[i].started).TotalMinutes >= 6)
                        {
                            queue.Enqueue(new int[] { queueRun[i].x, queueRun[i].y, queueRun[i].z });
                            if (_runtime_renderers[queueRun[i].z].VIEW != null) _runtime_renderers[queueRun[i].z].VIEW.Set(queueRun[i].x, queueRun[i].y, 1);
                            queueTotal++;
                            queueRun.RemoveAt(i);
                            queueRunTotal--;
                            removed = true;
                        };
                    queueMtx.ReleaseMutex();
                };
                // LOST CLIENT
                bool deleted = false;
                if (ClientList.Count > 0)
                {
                    clmtx.WaitOne();
                    for(int i = ClientList.Count - 1; i >= 0; i--)
                        if (DateTime.UtcNow.Subtract(ClientList[i].la).TotalMinutes >= 6)
                        {
                            ClientList.RemoveAt(i);
                            deleted = true;
                        };
                    clmtx.ReleaseMutex();
                };
                ///////////////////
                Thread.Sleep(30000);
            };
        }

        static void UpdateStatus(bool saveToFileNow)
        {
            string status = GetStatus();

            if (saveToFileNow || (timerOnTickUpdateFile++ % 8 == 0)) // 1 time per 16 seconds
            {
                try
                {
                    System.IO.FileStream fs = new System.IO.FileStream(TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\server_[" + projectFileName + "]" + curr_task + ".log", System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    byte[] data = System.Text.Encoding.GetEncoding(1251).GetBytes(status);
                    fs.Write(data, 0, data.Length);
                    fs.Close();
                }
                catch { };
            };

            Console.Clear();
            Console.Write(status);
        }

        public static string GetStatus()
        {
            return GetStatus(false, "127.0.0.1");
        }

        public static string GetStatus(bool addHTMLtags)
        {
            return GetStatus(false, "127.0.0.1");
        }

        public static string GetStatus(bool addHTMLtags, string Host)
        {            
            System.Globalization.CultureInfo stream_CultureInfo = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)stream_CultureInfo.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

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
                    chZooms += chZooms.Length > 0 ? ", " + i.ToString() : i.ToString();
                };
            };

            string clist = "";
            string cchar = Char.ConvertFromUtf32(65);
            clmtx.WaitOne();
            if (ClientList.Count > 0) 
            for (int i = 0; i < ClientList.Count; i++)
            {
                cchar = Char.ConvertFromUtf32(65+i);
                clist += cchar + " " + ClientList[i].ToString() + "\r\n";
            };
            clmtx.ReleaseMutex();

            string IP = "127.0.0.1";
            if (!String.IsNullOrEmpty(Host)) IP = Host.Trim();
            string status = "";
            status += String.Format("Mapnik Tile Renderer Console Server, version {0}\r\n", sversion);
            status += String.Format("Файл проекта: {0}\r\n", projectFileName);
            status += String.Format("Сервер запущен в: {0}\r\n\r", _runtime_started[0]);
            status += String.Format("Подключения клиентов: tcp://{2}:{0}/{1}\r\n", localServerPort, localServerTask, IP);
            status += String.Format("Статус в браузере: http://{1}:{0}/getstatus\r\n", localServerPort, IP);
            if (TileRendering.TileRenderingErrors.WithDump)
            {
                if (addHTMLtags)
                {
                    string ztxt = "";
                    if (_runtime_renderers != null)
                        for (int i = 0; i < _runtime_renderers.Length; i++)
                            if (_runtime_renderers[i] != null)
                                if (_runtime_renderers[i].VIEW != null)
                                    ztxt += (ztxt.Length > 0 ? ", " : " ") + "<a href=\"/image?zoom=" + i.ToString() + "\">" + i.ToString() + "</a>";
                    status += String.Format("Открыть визуализацию нарезки для "+ztxt+" зума; <a href=\"/dump\">скачать текущий дамп нарезки</a>\r\n", localServerPort);
                };
            };
            if (!String.IsNullOrEmpty(text_specified_by_child))
                status += text_specified_by_child + "\r\n";
            else
                status += "\r\n";
            status += String.Format("Клиенты {0}:\r\n{1}", ClientList.Count, clist);
            status += "\r\n";
            status += String.Format("Нарезано {0} зумов из {1}\r\n", _runtime_zooms_done, total_zooms);
            status += String.Format("Выбранные зумы для нарезки: {0}\r\n", chZooms);
            status += String.Format("Зона для нарезки: ( {0} ; {1} ) .. ( {2} ; {3} ) - {4}\r\n", new object[] { project.Zone[1], project.Zone[2], project.Zone[3], project.Zone[4], project.Zone[0] });
            status += "\r\n";
            status += String.Format("Сейчас в работе: {0}\r\n", queueRunTotal);            
            for (int i = 0; i < _runtime_zooms_todo.Length; i++)
                if ((checkedZooms.IndexOf(i + 1) >= 0) && ((_runtime_zooms_done == total_zooms) || (_runtime_current_zoom >= (i + 1))))
                {
                    status += "\r\n";
                    status += String.Format(ni, "Зум {0}, выполнено {1:0.0}%\r\n", i + 1, ((double)_runtime_renderers[i + 1].status_total_passed) / ((double)_runtime_renderers[i + 1].status_total_to_render) * 100.0);
                    status += String.Format("  - Начато: {0}\r\n", _runtime_started[i + 1]);
                    if (_runtime_renderers[i + 1] != null)
                    {
                        status += String.Format("  - Тайл обр: {0} из {1}, сзд {2}, ппщ {3}, дыр {4} в {5} квдр\r\n", new object[] { _runtime_renderers[i + 1].status_total_passed, _runtime_renderers[i + 1].status_total_to_render, _runtime_renderers[i + 1].status_total_created, _runtime_renderers[i + 1].status_total_skipped, _runtime_renderers[i+1].status_total_witherr, _runtime_renderers[i+1].status_total_zoneerr });
                        status += String.Format("  - Номера тайлов: x {0} из {1}..{2}, y {3} из {4}..{5}\r\n", new object[] { _runtime_renderers[i + 1].status_x_current, _runtime_renderers[i + 1].status_x_start, _runtime_renderers[i + 1].status_x_end, _runtime_renderers[i + 1].status_y_current, _runtime_renderers[i + 1].status_y_start, _runtime_renderers[i + 1].status_y_end });
                        status += String.Format(ni, "  - Размер созданных тайлов: {0}\r\n", TileRendering.TileRenderer.FileSizeSuffix(_runtime_renderers[i + 1].status_total_size));
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
                        status += String.Format("  - Выполняется: {0} дн {1} ч {2} м {3} c\r\n", new object[] { (int)ts.TotalDays, ts.Hours, ts.Minutes, ts.Seconds });
                    };
                };

            status += String.Format("\r\nНарезано {0} зумов из {1}\r\n", _runtime_zooms_done, total_zooms);
            status += String.Format("Тайл обр: {0} из {1}/{4}, сзд {2}, ппщ {3}, дыр {5} в {6} квдр\r\n", new object[] { total_passed, total_to_render, total_created, total_skipped, _runtime_total_to_render, total_witherr, total_zoneerr });
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
                TimeSpan ts = DateTime.Now.Subtract(_runtime_started[0]);
                //status += String.Format("Выполнено: {0}%\r\n", (int)(((double)total_passed) / ((double)_runtime_total_to_render) * 100.0));
                status += String.Format("Выполняется: {0} дн {1} ч {2} м {3} c\r\n", new object[] { (int)ts.TotalDays, ts.Hours, ts.Minutes, ts.Seconds });
            };
            status += String.Format(ni, "Выполнено {0:0.0}%\r\n", ((double)total_passed) / ((double)_runtime_total_to_render) * 100.0);

            if (addHTMLtags) status = status.Replace("\r\n", "<br/>");
            return status;
        }

        static void RenderTiles(double startLat, double startLon, double endLat, double endLon, int zoom)
        {
            if ((startLat > 90) || (startLat < -90)) return;
            if ((startLon < -180) || (startLon > 180)) return;
            if ((endLat < -90) || (endLat > 90)) return;
            if ((endLon > 180) || (endLon < -180)) return;

            double[] px0 = TileRendering.TilesProjection.fromLLtoPixel(new double[] { startLon, startLat }, zoom);
            double[] px1 = TileRendering.TilesProjection.fromLLtoPixel(new double[] { endLon, endLat }, zoom);

            RenderTiles((int)(px0[0] / 256.0), (int)(px0[1] / 256.0), (int)(px1[0] / 256.0), (int)(px1[1] / 256.0), zoom);
        }

        static void RenderTiles(int startX, int startY, int endX, int endY, int zoom)
        {
            if ((zoom < TileRendering.TileRenderer.minZoom) || (zoom > TileRendering.TileRenderer.maxZoom)) return;

            // Prepare
            tileZoneSizeDiv = (int)project.RendererConfig.tileZoneSize;
            tileZoneSizeCount = tileZoneSizeDiv * tileZoneSizeDiv;

            // Calculate MinMax
            int maxtilesinline = (int)Math.Pow(2, zoom);
            int min_x = _runtime_renderers[zoom].status_x_start = Math.Max(0, startX);
            int max_x = _runtime_renderers[zoom].status_x_end = Math.Min(maxtilesinline, endX - (endX % tileZoneSizeDiv) + tileZoneSizeDiv - 1);
            int min_y = _runtime_renderers[zoom].status_y_start = Math.Max(0, startY);
            int max_y = _runtime_renderers[zoom].status_y_end = Math.Min(maxtilesinline, endY - (endY % tileZoneSizeDiv) + tileZoneSizeDiv - 1);

            if (TileRendering.TileRenderingErrors.WithDump)
            {
                // SAVING REDERER MATRIX IN-MEM
               _runtime_renderers[zoom].VIEW = new TileRendering.RenderView(min_x, max_x, min_y, max_y, tileZoneSizeDiv, zoom);

               // LOAD RENDERER MATRIX FROM FILE
               if ((!String.IsNullOrEmpty(loadDumpFile)) && (File.Exists(loadDumpFile)))
               {
                   FileStream fs = new FileStream(loadDumpFile, FileMode.Open, FileAccess.Read);
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
                               else if ((_runtime_renderers[zoom].VIEW.Size == (zlength + 22)) && (sqsz == tileZoneSizeDiv) && (_runtime_renderers[zoom].status_x_start == xf) && (_runtime_renderers[zoom].status_x_end == xt) && (_runtime_renderers[zoom].status_y_start == yf) && (_runtime_renderers[zoom].status_y_end == yt))
                               {
                                   for (int i = 22; i < _runtime_renderers[zoom].VIEW.Size; i++)
                                   {
                                       int b = fs.ReadByte();
                                       _runtime_renderers[zoom].VIEW.SetByte(i, (byte)b);
                                   };
                               };
                           };

                       };
                   };
                   fs.Close();
               };
            };

            // prepare status
            _runtime_renderers[zoom].status_total_to_render = (ulong)(tileZoneSizeCount * (((int)(max_x / tileZoneSizeDiv) - (int)(min_x / tileZoneSizeDiv) + 1) * ((int)(max_y / tileZoneSizeDiv) - (int)(min_y / tileZoneSizeDiv) + 1)));
            _runtime_renderers[zoom].status_total_created = 0;
            _runtime_renderers[zoom].status_total_passed = 0;

            //ADD TO QUEUE            
            for (int x = min_x; x <= max_x; x += tileZoneSizeDiv)
                for (int y = min_y; y <= max_y; y += tileZoneSizeDiv)
                {
                    // добавляем в очередь
                    bool added = false;
                    while (!added)
                    {                        
                        if (queue.Count < queueMax)
                        {
                            //// SKIP IF DUMP LOADED
                            //queueMtx.WaitOne();
                            //queue.Enqueue(new int[] { x, y, zoom });
                            //queueMtx.ReleaseMutex();
                            //if (_runtime_renderers[zoom].VIEW != null) _runtime_renderers[zoom].VIEW.Set(x, y, 1);
                            //queueTotal++;
                            //added = true;
                            //// END SKIP
                           
                            int[] q = new int[] { x, y, zoom };
                            // SKIP IF DONE
                            byte czs = 0;
                            if ((_runtime_renderers[zoom].VIEW != null) && ((czs = _runtime_renderers[zoom].VIEW.Get(x, y)) > 3) && ((czs == 4) || (czs == 5) || (czs == 8)))
                            { // квадрат нарезан без дыр и ошибок
                                // обновляем статистику
                                ulong passed = (ulong)tileZoneSizeCount;
                                _runtime_renderers[zoom].status_total_skipped += passed;
                                _runtime_renderers[zoom].status_total_passed += passed;
                                queueTotal--;
                            }
                            else
                            {
                                queueMtx.WaitOne();
                                queue.Enqueue(q);
                                queueMtx.ReleaseMutex();
                                _runtime_renderers[zoom].last_enqueued = q;
                                if (_runtime_renderers[zoom].VIEW != null) _runtime_renderers[zoom].VIEW.Set(x, y, 1);
                            };
                            added = true;
                            queueTotal++;
                        };
                        if (!added)
                            System.Threading.Thread.Sleep(2);
                    };
                };

            // done
        }


        //////////////////////
         /////// SERVER ///////
          //////////////////////
        public class HTTPListener
        {
            private Thread mainThread = null;
            private TcpListener mainListener = null;
            private IPAddress ListenIP = IPAddress.Any;
            private int ListenPort = 80;
            private bool isRunning = false;
            private int MaxThreads = 1;
            private int ThreadsCount = 0;

            private string task = "";

            public HTTPListener(int Port, int MaxThreads)
            {
                this.ListenPort = Port;
                this.MaxThreads = MaxThreads;
            }

            public HTTPListener(int Port, int MaxThreads, string task)
            {
                this.ListenPort = Port;
                this.MaxThreads = MaxThreads;
                this.task = task;
            }

            public bool Running { get { return isRunning; } }
            public IPAddress ServerIP { get { return ListenIP; } }
            public int ServerPort { get { return ListenPort; } }

            public void Dispose() { Stop(); }
            ~HTTPListener() { Dispose();} 

            public virtual void Start()
            {
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
                        Request += Encoding.GetEncoding(1251).GetString(Buffer, 0, Count);
                        if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 4096) { break; };
                    };
                }
                catch { }; // READ REQUEST ERROR

                if (Count > 0) // PARSE REQUEST ERROR
                    try { ReceiveData(Client, Request); }
                    catch { };

                Client.Close();
                this.MaxThreads--;
            }

            // BEGIN METHOD RECEIVE DATE
            public void ReceiveData(TcpClient Client, string Request)
            {
                // PARSE REQUEST
                byte methodIS = 3; // 3-Get, 4-Post, 7-OPTIONS
                int x1 = Request.IndexOf("GET");
                int x2 = Request.IndexOf("HTTP");
                if ((x1 < 0) || (x1 > x2)) { x1 = Request.IndexOf("POST"); if (x1 >= 0) methodIS = 4; };
                if ((x1 < 0) || (x1 > x2)) { x1 = Request.IndexOf("OPTIONS"); if (x1 >= 0) methodIS = 7; };
                string query = "";
                if ((x1 >= 0) && (x2 >= 0) && (x2 > x1)) query = Request.Substring(x1 + methodIS, x2 - methodIS).Trim();

                // ONLY ALLOWED METHODS
                string[] qpc = query.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries);
                if ((qpc.Length == 0) || ((qpc[0] != "/dump") && (qpc[0] != "/image") && (qpc[0] != "/getstatus") && (qpc[0] != "/getwork") && (qpc[0] != "/setwork") && (qpc[0] != "/clrwerror")))
                {
                    HttpClientSendError(Client, 501);
                    return;
                };

                // Auto Save Dump
                AutoSaveDump();

                // console text to web
                if (qpc[0] == "/getstatus")
                {
                    // Host: ....
                    string IP = "127.0.0.1";
                    try
                    {
                        Regex rx = new Regex(@"(Host:[\s]{0,1})([\s\w\.]{0,})([:\d]{0,})(\r)");
                        Match mx = rx.Match(Request);
                        if (mx.Success)
                            IP = mx.Groups[2].Value;
                    }
                    catch { };
                    string status = MapnikTileRendererConsole.Server.GetStatus(true, IP);
                    HttpClientSendData(Client, status);
                    return;
                };

                // visualize map to web
                if (qpc[0] == "/image")
                {
                    if (!TileRendering.TileRenderingErrors.WithDump)
                    {
                        HttpClientSendData(Client, "Матрица информационной нарезки отсутствует");
                        return;
                    };

                    try
                    {
                        System.Collections.Specialized.NameValueCollection nvc = HttpUtility.ParseQueryString(qpc[1]);
                        int z = int.Parse(nvc["zoom"]);
                        if(_runtime_renderers[z].VIEW != null)
                        {
                            Bitmap bmp = _runtime_renderers[z].VIEW.GetView();
                            if ((bmp.Width < 300) || (bmp.Height < 300))
                            {
                                double dwh = (double)bmp.Width / (double)bmp.Height;
                                int neww = (bmp.Width >= bmp.Height) ? 300 : (int)(300.0 * dwh);
                                int newh = (bmp.Width >= bmp.Height) ? (int)(300.0 / dwh) : 300;
                                bmp = _runtime_renderers[z].VIEW.Resize(bmp, neww, newh);
                            };
                            //bmp = _runtime_renderers[z].VIEW.Compress(bmp);
                            HttpClientSendData(Client, bmp);
                            bmp.Dispose();
                            return;
                        };
                    } 
                    catch {};

                    string ztxt = "";
                    if (_runtime_renderers != null)
                        for (int i = 0; i < _runtime_renderers.Length; i++)
                            if (_runtime_renderers[i] != null)
                                if (_runtime_renderers[i].VIEW != null)
                                    ztxt += (ztxt.Length > 0 ? ", " : " ") + "<a href=\"/image?zoom="+i.ToString()+"\">" + i.ToString() + "</a>";
                    HttpClientSendData(Client, "Доступны только зумы: " + ztxt + "\r\n");
                    return;
                };

                // dump to web
                if (qpc[0] == "/dump")
                {                    
                    if (!TileRendering.TileRenderingErrors.WithDump)
                    {
                        HttpClientSendData(Client, "Матрица информационной нарезки отсутствует");
                        return;
                    };

                    List<TileRendering.RenderView> data = new List<TileRendering.RenderView>();
                    if (_runtime_renderers != null)
                        for (int i = 0; i < _runtime_renderers.Length; i++)
                            if (_runtime_renderers[i] != null)
                                if (_runtime_renderers[i].VIEW != null)
                                    data.Add(_runtime_renderers[i].VIEW);
                    if (data.Count > 0)
                    {
                        HttpClientSendData(Client, data.ToArray(), System.IO.Path.GetFileNameWithoutExtension(Server.projectFileName));
                        return;
                    }
                    else
                    {
                        HttpClientSendData(Client, "Матрица информационной нарезки отсутствует");
                        return;
                    };
                };

                //Check Client Version
                string cversion = "";
                if (Request.IndexOf("MTRC-Version:") > 0)
                {
                    cversion = Request.Substring(Request.IndexOf("MTRC-Version:") + 13);
                    cversion = cversion.Substring(0, cversion.IndexOf("\r\n")).Trim();  
                };
                if (cversion != Server.sversion)
                {
                    HttpClientSendData(Client, "wrongversion");
                    return;
                };

                
                // Check Client Task
                string ctask = "";
                if (Request.IndexOf("MTRC-TASK:") > 0)
                {
                    ctask = Request.Substring(Request.IndexOf("MTRC-TASK:") + 10);
                    ctask = ctask.Substring(0, ctask.IndexOf("\r\n")).Trim();                    
                };
                if (ctask != task)
                {
                    HttpClientSendData(Client, "wrongtask");
                    return;
                };

                // Check Clients Info
                string cthreads = "1";
                if (Request.IndexOf("MTRC-Threads:") > 0)
                {
                    cthreads = Request.Substring(Request.IndexOf("MTRC-Threads:") + 13);
                    cthreads = cthreads.Substring(0, cthreads.IndexOf("\r\n")).Trim();
                };
                string cname = "";
                if (Request.IndexOf("MTRC-Client:") > 0)
                {
                    cname = Request.Substring(Request.IndexOf("MTRC-Client:") + 12);
                    cname = cname.Substring(0, cname.IndexOf("\r\n")).Trim();
                };

                string ip = ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
                SClient rc = null;
                clmtx.WaitOne();
                int cind = ClientList.IndexOf(ip, cname);
                if (cind < 0)
                    ClientList.Add(rc = new SClient(ip, cname, cthreads));
                else
                {
                    rc = ClientList[cind];
                    ClientList[cind].la = DateTime.UtcNow;
                };
                clmtx.ReleaseMutex();

                if (qpc[0] == "/clrwerror")
                {
                    System.Collections.Specialized.NameValueCollection nvc = HttpUtility.ParseQueryString(qpc[1]);
                    rc.clrwError = byte.Parse(nvc["code"]);
                    HttpClientSendData(Client, "ok");
                    return;
                };

                if (qpc[0] == "/getwork")
                {
                    int[] p = null;
                    // если число последовательных ошибок превышет 5, то ничего не отдаем, ждем пока
                    // ошибки от клиента не прекратятся. В это время клиент выполняет задания из
                    // своего последнего листа в произвольном порядке, пока ошибка не исчезнет
                    if (rc.itec > 5)
                    {
                        if (queue.Count > 0)
                            HttpClientSendData(Client, "repeatlast");
                        else
                            HttpClientSendData(Client, "nodata");
                        return;
                    }
                    else
                    {
                        queueMtx.WaitOne();
                        if (queue.Count > 0)
                        {
                            p = queue.Dequeue();                            
                            rc.queued++;
                            rc.alive++;
                            queueTotal--;
                            queueRun.Add(new QeuryTileZoneByClientNow(p[0], p[1], p[2]));
                            if (_runtime_renderers[p[2]].VIEW != null) _runtime_renderers[p[2]].VIEW.Set(p[0], p[1], 2);
                            queueRunTotal++;
                        };
                        queueMtx.ReleaseMutex();
                    };

                    string response = "nodata";
                    if (p != null)
                    {
                        // обновляем статус
                        _runtime_renderers[p[2]].status_x_current = p[0];
                        _runtime_renderers[p[2]].status_y_current = p[1];

                        response = String.Format("zone:{0};{1};{2}", p[0], p[1], p[2]);
                    }
                    else if (hasEnded && closeAllClientsWhenDone) response = "cmd_exit";
                    HttpClientSendData(Client, response);
                    return;
                };
                if (qpc[0] == "/setwork")
                {
                    System.Collections.Specialized.NameValueCollection nvc = HttpUtility.ParseQueryString(qpc[1]);
                    int z = int.Parse(nvc["z"]);
                    ulong crtd = ulong.Parse(nvc["c"]);
                    ulong skpd = ulong.Parse(nvc["s"]);
                    ulong byps = ulong.Parse(nvc["b"]);
                    ulong itec = ulong.Parse(nvc["itec"]);
                    bool err = (int.Parse(nvc["e"]) > 0) || (byps > 0); // FULL ZONE NOT SAVED or JUST ONLY ONE TILE FROM ZONE                    
                   
                    queueMtx.WaitOne();
                    int[] p = new int[] { int.Parse(nvc["x"]), int.Parse(nvc["y"]), z };
                    bool resultActual = DelFromQueueRun(p, !err);
                    if (resultActual && err)
                    {
                        err = CheckErrorRepeatQueue(p);
                    };
                    queueMtx.ReleaseMutex();

                    // обновляем информацию по клиенту
                    //if(z != 0) rc.alive--;
                    if (resultActual) rc.alive--;
                    //if (rc.alive < 0) rc.alive = 0;
                    rc.created += crtd;
                    rc.skipped += skpd;
                    rc.witherr += byps;
                    rc.itec = itec;

                    // обновляем статистику по зуму только после errorRepeatTries неудачных попыток нарезки зоны
                    // либо если все ок
                    if (resultActual && (!err))
                    {
                        _runtime_renderers[z].status_total_created += crtd;
                        _runtime_renderers[z].status_total_skipped += skpd;
                        _runtime_renderers[z].status_total_witherr += byps;
                        _runtime_renderers[z].status_total_zoneerr += byps > 0 ? (ulong)1 : (ulong)0;
                        _runtime_renderers[z].status_total_size += (float)ulong.Parse(nvc["f"]);
                        // error nvc["e"]
                        _runtime_renderers[z].status_total_passed += (ulong)tileZoneSizeCount;
                        if (_runtime_renderers[z].status_total_passed == _runtime_renderers[z].status_total_to_render)
                        {
                            _runtime_stopped[z] = DateTime.Now;
                            _runtime_zooms_done++;
                        };

                        if (_runtime_renderers[z].VIEW != null)
                        {
                            _runtime_renderers[z].VIEW.Set(p[0], p[1], 10);
                            if ((crtd > 0) && (skpd == 0) && (byps == 0) && (!err)) _runtime_renderers[z].VIEW.Set(p[0], p[1], 4);
                            if ((crtd > 0) && (skpd > 0) && (byps == 0) && (!err)) _runtime_renderers[z].VIEW.Set(p[0], p[1], 5);
                            if ((crtd > 0) && (skpd == 0) && (byps > 0)) _runtime_renderers[z].VIEW.Set(p[0], p[1], 6);
                            if ((crtd > 0) && (skpd > 0) && (byps > 0)) _runtime_renderers[z].VIEW.Set(p[0], p[1], 7);
                            if ((crtd == 0) && (skpd > 0) && (byps == 0)) _runtime_renderers[z].VIEW.Set(p[0], p[1], 8);
                            if ((crtd == 0) && (skpd == 0) && (byps > 0)) _runtime_renderers[z].VIEW.Set(p[0], p[1], 9);
                        };
                    };
                    HttpClientSendData(Client, "ok");                    
                    return;
                };

                Client.Close();
            }
            // END METHOD RECEIVE DATE

            private static bool DelFromQueueRun(int[] p, bool good)
            {
                if(good)
                    for(int i= queueErr.Count -1;i>=0;i--)
                        if ((queueErr[i][0] == p[0]) && (queueErr[i][1] == p[1]) && (queueErr[i][2] == p[2]))
                        {
                            queueErr.RemoveAt(i);
                            break;
                        };
                for (int i = queueRun.Count - 1; i >= 0; i--)
                    if ((queueRun[i].x == p[0]) && (queueRun[i].y == p[1]) && (queueRun[i].z == p[2]))
                    {
                        queueRun.RemoveAt(i);
                        queueRunTotal--;
                        return true;
                    };
                return false;
            }

            private static bool CheckErrorRepeatQueue(int[] p)
            {
                int enc = 1;
                for (int i = queueErr.Count - 1; i >= 0; i--)
                    if ((queueErr[i][0] == p[0]) && (queueErr[i][1] == p[1]) && (queueErr[i][2] == p[2]))
                    {
                        enc = ++queueErr[i][3];
                        if (enc > errorRepeatTries)
                        {
                            queueErr.RemoveAt(i);
                            Add2ServerHolesLog(p);
                            return false;
                        };
                        break;
                    };
                if (enc == 1) queueErr.Add(new int[] { p[0], p[1], p[2], 2 });
                if (enc <= errorRepeatTries)
                {
                    queue.Enqueue(p);
                    if (_runtime_renderers[p[2]].VIEW != null) _runtime_renderers[p[2]].VIEW.Set(p[0], p[1], 1);
                    queueTotal++;
                };
                return enc <= errorRepeatTries;
            }

            private static void Add2ServerHolesLog(int[] p)
            {
                try
                {
                    string txt = String.Format("ZN:{0},{1},{2};TRIED{3}\r\n", new object[] { p[0], p[1], p[2], errorRepeatTries });
                    System.IO.FileStream fs = new System.IO.FileStream(TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\server_[" + projectFileName + "]" + curr_task + ".holes", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                    byte[] data = System.Text.Encoding.GetEncoding(1251).GetBytes(txt);
                    fs.Position = fs.Length;
                    fs.Write(data, 0, data.Length);
                    fs.Close();
                }
                catch { };
            }

            #region Private ReceiveData Methods
            private void HttpClientSendData(TcpClient Client, string data)
            {
                string CodeStr = "200 " + ((HttpStatusCode)200).ToString();
                string Str =
                    "HTTP/1.1 " + CodeStr + "\r\n" +
                    "Connection: close\r\n" +
                    "Content-Encoding: windows-1251\r\n" +
                    "Content-type: text/html\r\n" +
                    "Content-Length:" + data.Length.ToString() + "\r\n\r\n" + data;

                byte[] Buffer = Encoding.GetEncoding(1251).GetBytes(Str);
                Client.GetStream().Write(Buffer, 0, Buffer.Length);
                Client.Close();
            }
            private void HttpClientSendData(TcpClient Client, Bitmap data)
            {
                MemoryStream ms = new MemoryStream();
                data.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                string CodeStr = "200 " + ((HttpStatusCode)200).ToString();
                string Str =
                    "HTTP/1.1 " + CodeStr + "\r\n" +
                    "Connection: close\r\n" +
                    "Content-type: image/png\r\n" +
                    "Content-Length:" + ms.Length + "\r\n\r\n";

                byte[] Buffer = Encoding.GetEncoding(1251).GetBytes(Str);
                Client.GetStream().Write(Buffer, 0, Buffer.Length);
                byte[] rb = new byte[1024];
                int rw = 0;
                ms.Position = 0;
                while ((rw = ms.Read(rb, 0, rb.Length)) > 0)
                    Client.GetStream().Write(rb, 0, rw);
                Client.Close();
                ms.Close();
            }
            private void HttpClientSendData(TcpClient Client, TileRendering.RenderView[] data, string projFN)
            {
                int ttld = 0;
                foreach (TileRendering.RenderView d in data) ttld += d.Size;

                string CodeStr = "200 " + ((HttpStatusCode)200).ToString();
                string Str =
                    "HTTP/1.1 " + CodeStr + "\r\n" +
                    "Connection: close\r\n" +
                    "Content-type: application/octet-stream\r\n" +
                    "Content-Disposition: attachment; filename = " + projFN + ".dump\r\n" +
                    "Content-Length:" + (TileRendering.RenderView.fileheader.Length + ttld) + "\r\n\r\n";

                byte[] Buffer = Encoding.GetEncoding(1251).GetBytes(Str);
                Client.GetStream().Write(Buffer, 0, Buffer.Length);
                Client.GetStream().Write(TileRendering.RenderView.fileheader, 0, TileRendering.RenderView.fileheader.Length);
                foreach (TileRendering.RenderView d in data) 
                    for (int i = 0; i < d.Size; i++)
                    {
                        byte b = d.GetByte(i);
                        Client.GetStream().WriteByte(b);
                    };
                Client.Close();
            }
            #endregion

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
                    "Content-Encoding: windows-1251\r\n" +
                    "Content-type: text/html\r\n" +
                    "Content-Length:" + Html.Length.ToString() + "\r\n\r\n" + Html;
                // Приведем строку к виду массива байт
                byte[] Buffer = Encoding.GetEncoding(1251).GetBytes(Str);
                // Отправим его клиенту
                Client.GetStream().Write(Buffer, 0, Buffer.Length);
                // Закроем соединение
                Client.Close();
            }
        }

    }

    class Client
    {
        public static string cversion = "2016-12-22-B";

        public static DateTime started = DateTime.Now;

        static byte errorRepeatTries = 3;

        static string server = "";
        static string fileName = "";
        static MapnikTileRenderer.MapnikTileRendererProject proj;
        static TileRendering.TileRenderer[] _runtime_renderers;
        static Queue<int[]> queue = new Queue<int[]>();
        static Mutex queueMtx = new Mutex();
        static string curr_task = "";

        static Mutex qlm = new Mutex();
        static ulong c_ttl = 0;
        static ulong c_alv = 0;
        static ulong c_fin = 0;
        static ulong t_crt = 0;
        static ulong t_skp = 0;
        static ulong t_wer = 0;
        static ulong t_zne = 0;
        static List<object[]> qlast = new List<object[]>();
        static ulong[] c_get = new ulong[0];
        static ulong[] c_don = new ulong[0];
        static string[] c_st = new string[0];

        static string unicalID = Guid.NewGuid().ToString();

        private static void qulast(string[] xyz)
        {
            qlm.WaitOne();
            while (qlast.Count > 15) qlast.RemoveAt(0);
            qlast.Add(new object[] { DateTime.Now.ToString("dd.MM.yy HH:mm:ss"), xyz[0], xyz[1], xyz[2], "в ожидании" });
            qlm.ReleaseMutex();
        }

        private static void qurun(int[] xyz, int threadNo, byte tries)
        {
            qlm.WaitOne();
            if (qlast.Count > 0)
            {
                for (int i = qlast.Count-1; i >= 0; i--)
                    if (
                        (xyz[0].ToString() == qlast[i][1].ToString()) &&
                        (xyz[1].ToString() == qlast[i][2].ToString()) &&
                        (xyz[2].ToString() == qlast[i][3].ToString())
                       )
                    {
                        qlast[i][4] = "в нарезке [" + threadNo.ToString() + "]" + (tries > 1 ? ", попытка " + tries.ToString() : "");
                        break;
                    };
            };
            qlm.ReleaseMutex();
        }

        private static void qudone(int[] xyz, int threadNo, string status)
        {
            qlm.WaitOne();
            if (qlast.Count > 0)
            {
                for (int i = qlast.Count - 1; i >= 0; i--)
                    if (
                        (xyz[0].ToString() == qlast[i][1].ToString()) &&
                        (xyz[1].ToString() == qlast[i][2].ToString()) &&
                        (xyz[2].ToString() == qlast[i][3].ToString())
                       )
                    {
                        qlast[i][4] = status;
                        break;
                    };
            };
            qlm.ReleaseMutex();
        }

        private static void SendPermisionsErrorToServer()
        {
            while (true)
            {
                SendCLWRError2Server(1);
                Thread.Sleep(5000);
            };
        }

        private static void SendCLWRError2Server(byte code)
        {
            string Uri_url = String.Format("http://{0}/clrwerror?code={1}", server, code);
            System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(Uri_url);
            wr.Headers.Add("MTRC-Version", cversion);
            wr.Headers.Add("MTRC-TASK", curr_task);
            wr.Headers.Add("MTRC-Threads", proj.RendererConfig.multithreadCount.ToString());
            wr.Headers.Add("MTRC-Client", unicalID);
            try
            {
                System.Net.WebResponse res = wr.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(res.GetResponseStream());
                string result = sr.ReadToEnd();
                sr.Close();
                res.Close();
            }
            catch (System.Net.WebException err) { };
        }

        private static bool CheckDistinationFolderAccess(string tilesPath)
        {
            Console.WriteLine();
            Console.WriteLine("Проверка доступа к папке с тайлами:");
            Console.WriteLine("  " + tilesPath);
            
            bool de = false;
            try
            {
                de = Directory.Exists(tilesPath);
                Console.WriteLine("Папка "+(de ? "найдена" : "не найдена"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: Не удается найти и получить доступ к папке!");
            };            
            
            if (!de)
            {
                try
                {
                    de = Directory.CreateDirectory(tilesPath).Exists;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: Не удается создать папку!");
                };
            };
            
            string fn = tilesPath + @"\" + curr_task + ".txt";
            bool ok = false;
            try
            {
                FileStream fs = new FileStream(fn, FileMode.Create, FileAccess.Write);
                fs.WriteByte(0x43);
                fs.Close();
                ok = true;
                Console.WriteLine("Наличие прав на запись в папку с тайлами подтверждено!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: Отсутствует папка или права на запись!");
            };
            return ok;
        }

        static void BackUpLogs()
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

        public static void Start(string projFile, string remoteAddress, string task, int maxThreads)
        {
            curr_task = task;            

            TileRendering.TileRenderingErrors.PauseThreadsIfErrorsCountIs = 0;

            server = remoteAddress;
            fileName = System.IO.Path.GetFileName(projFile);
            proj = MapnikTileRenderer.MapnikTileRendererProject.FromFile(projFile);

            BackUpLogs();

            string errLogFileName = TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\client_[" + fileName + "]" + curr_task + ".errorlog";
            if (System.IO.File.Exists(errLogFileName)) System.IO.File.Delete(errLogFileName);            

            Console.Clear();
            Console.WriteLine("Mapnik Tile Renderer Console Client");
            Console.WriteLine("Файл проекта: " + fileName);

            string tilesPath = proj.TilesPath + @"\";
            if (proj.RendererConfig.pathStructure == TileRendering.TileRenderPathScheme.fsArcGIS)
                tilesPath = proj.TilesPath + @"\_alllayers\";

            if (!CheckDistinationFolderAccess(tilesPath))
            {
                Console.WriteLine();
                Console.WriteLine("Нельзя запустить клиента в работу!");
                Console.WriteLine("Если вы хотите продолжить независимо от предупреждения, нажмите `Enter`");

                Thread spets = new Thread(new ThreadStart(SendPermisionsErrorToServer));
                spets.Start();

                Console.ReadLine();
                spets.Abort();
            };
            Console.WriteLine("Запуск...");
            Console.WriteLine();

            if (maxThreads > 0) proj.RendererConfig.multithreadCount = maxThreads;

            _runtime_renderers = new TileRendering.TileRenderer[proj.RendererConfig.multithreadCount];
            c_get = new ulong[proj.RendererConfig.multithreadCount];
            c_don = new ulong[proj.RendererConfig.multithreadCount];
            c_st = new string[proj.RendererConfig.multithreadCount];
            List<System.Threading.Thread> renderThreads = new List<Thread>();
            for (int i = 0; i < _runtime_renderers.Length; i++)
            {
                c_st[i] = "запуск";
                _runtime_renderers[i] = new TileRendering.TileRenderer(proj.MapFile, tilesPath, proj.RendererConfig);
                System.Threading.Thread _runtime_thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(RunThread));
                _runtime_thread.Start(i);
                renderThreads.Add(_runtime_thread);
            };

            Thread upsc = new Thread(UpdateConsole);
            upsc.Start();

            updCurrentStateTxt = "Запрос заданий с сервера...";
            for (int i = 0; i < proj.RendererConfig.multithreadCount; i++)
                c_st[i] = "ожидание";

            string Uri_url = String.Format("http://{0}/getwork", server);
            bool loop = true;
            while (loop)
            {       
                // GETTING SERVER
                if (queue.Count < proj.RendererConfig.multithreadCount)
                {
                    System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(Uri_url);
                    wr.Headers.Add("MTRC-Version", cversion);
                    wr.Headers.Add("MTRC-TASK", task);
                    wr.Headers.Add("MTRC-Threads", proj.RendererConfig.multithreadCount.ToString());
                    wr.Headers.Add("MTRC-Client", unicalID);
                    wr.Timeout = 5000;
                    try
                    {
                        updCurrentStateTxt = "Запрос заданий с сервера...";
                        System.Net.WebResponse res = wr.GetResponse();
                        System.IO.StreamReader sr = new System.IO.StreamReader(res.GetResponseStream());
                        string tile = sr.ReadToEnd();
                        sr.Close();
                        if (tile.IndexOf("zone:") == 0)
                        {
                            string[] xyz = tile.Substring(5).Split(new string[] { ";" }, StringSplitOptions.None);
                            qulast(xyz);
                            c_ttl++;
                            queueMtx.WaitOne();
                            queue.Enqueue(new int[] { int.Parse(xyz[0]), int.Parse(xyz[1]), int.Parse(xyz[2]) });
                            queueMtx.ReleaseMutex();
                            updCurrentStateTxt = "Задания получены, ожидаю выполнения заданий потоками...";
                            System.Threading.Thread.Sleep(50);
                        }
                        else if (tile == "wrongtask")
                        {
                            updCurrentStateTxt = ("Сервер возвращает wrongtask, в ожидании...");
                            System.Threading.Thread.Sleep(2500);
                        }
                        else if (tile == "wrongversion")
                        {
                            updCurrentStateTxt = ("Сервер возвращает wrongversion, в ожидании...");
                            System.Threading.Thread.Sleep(2500);
                        }
                        else if (tile == "repeatlast")
                        {
                            updCurrentStateTxt = ("Сервер возвращает repeatlast, повтор...");

                            qlm.WaitOne();
                            Random rnd = new Random();
                            int ind = rnd.Next(0, qlast.Count - 1);
                            string[] xyz = new string[] { qlast[ind][1].ToString(), qlast[ind][2].ToString(), qlast[ind][3].ToString() };
                            qlm.ReleaseMutex();

                            queueMtx.WaitOne();
                            if((queue.Count == 0) && (c_alv == 0))
                                queue.Enqueue(new int[] { int.Parse(xyz[0]), int.Parse(xyz[1]), int.Parse(xyz[2]), 1 });
                            queueMtx.ReleaseMutex();
                            
                            System.Threading.Thread.Sleep(5000);                            
                        }
                        else if (tile == "cmd_exit")
                        {
                            for (int i = 0; i < renderThreads.Count; i++)
                                renderThreads[i].Abort();
                            loop = false;
                            break;
                        }
                        else
                        {
                            updCurrentStateTxt = ("Нет заданий от сервера, в ожидании...");
                            System.Threading.Thread.Sleep(2500);
                        };
                    }
                    catch (System.Net.WebException err)
                    {
                        updCurrentStateTxt = ("Сервер недоступен");
                        System.Threading.Thread.Sleep(2500);
                    };
                    ////////////////////////////////////
                    ///// PREVENT SYSTEM SLEEP MODE ////
                    ////////////////////////////////////
                    try
                    {
                        TileRendering.ClientServerUtils.KeepSystemAwake();
                    }
                    catch { };
                    ////////////////////////////////////
                    ////////////////////////////////////
                    ////////////////////////////////////
                }
                else // QUEUE FULL
                {
                    System.Threading.Thread.Sleep(10);
                };                
            };

            upcrunning = false;
            // upsc
        }

       
        static ulong inTimeErrCounter = 0;
        static void RunThread(object index)
        {
            TileRendering.TileRenderer tr = (TileRendering.TileRenderer)_runtime_renderers[(int)index];
            byte idleCounter = 0;            
            while (true)
            {
                // получаем параметры тайлов из очереди
                int[] p = null;
                queueMtx.WaitOne();
                if (queue.Count > 0)
                    p = queue.Dequeue();
                queueMtx.ReleaseMutex();

                // если парамеры получены, рисуем
                if (p != null)
                {
                    c_get[(int)index]++;
                    c_alv++;

                    idleCounter = 0;
                    c_st[(int)index] = String.Format("Режу {0}x {1}y {2}z; здн вып {4} из {3}", new object[] { p[0], p[1], p[2], c_get[(int)index], c_don[(int)index] }); ;
                    // отрисовываем тайлы
                    int[] rResult = new int[] { 0, 0, 0, 0, 4 }; // not runned
                    byte tries = 0;
                    bool errr = true;
                    while ((errr) && (tries < errorRepeatTries))
                    {
                        tries++;
                        qurun(p, (int)index, tries);
                        try
                        {
                            rResult = tr.render_tile(p[0], p[1], p[2]);
                        }
                        catch { };
                        errr = rResult[4] > 0; // only if full zone error
                    };
                    if (errr) inTimeErrCounter++; else inTimeErrCounter = 0;
                    //if (inTimeErrCounter >= 3) break;

                    // обновляем статистику
                    {
                        c_don[(int)index]++;
                        c_fin++;
                        c_alv--;
                        t_crt += (ulong)rResult[0];
                        t_skp += (ulong)rResult[1];
                        t_wer += (ulong)rResult[2];
                        if (rResult[2] > 0) t_zne++;
                        qudone(p, (int)index, (rResult[4] > 0 ? "ошбк, " : "готово, ") + "сзд:" + rResult[0].ToString() + ",ппщ:" + rResult[1].ToString() + ",дыр:" + rResult[2].ToString());
                    };

                    string result = "";                                        
                    // upserver      
                    {
                        // regulat task
                        string Uri_url = String.Format("http://{0}/setwork?x={1}&y={2}&z={3}&c={4}&s={5}&b={6}&f={7}&e={8}&itec={9}",
                        new object[] { server, p[0], p[1], p[2], rResult[0], rResult[1], rResult[2], rResult[3], rResult[4], inTimeErrCounter });
                        // repeatlast
                        if ((p.Length > 3) && (p[3] == 1)) 
                        {
                            Uri_url = String.Format("http://{0}/setwork?x={1}&y={2}&z={3}&c={4}&s={5}&b={6}&f={7}&e={8}&itec={9}",
                                new object[] { server, 0, 0, 0, 0, 0, 0, 0, rResult[4], inTimeErrCounter });
                        };
                        System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(Uri_url);
                        wr.Headers.Add("MTRC-Version", cversion);
                        wr.Headers.Add("MTRC-TASK", curr_task);
                        wr.Headers.Add("MTRC-Threads", proj.RendererConfig.multithreadCount.ToString());
                        wr.Headers.Add("MTRC-Client", unicalID);
                        try
                        {
                            System.Net.WebResponse res = wr.GetResponse();
                            System.IO.StreamReader sr = new System.IO.StreamReader(res.GetResponseStream());
                            result = sr.ReadToEnd();
                            sr.Close();
                            res.Close();
                        }
                        catch (System.Net.WebException err) { };
                    };

                    //if (result == "wrongtask")
                    {
                        c_st[(int)index] = String.Format("Нарезал {0} x {1} y {2} z; здн вып {4} из {3}", new object[] { p[0], p[1], p[2], c_get[(int)index], c_don[(int)index] });
                        System.Threading.Thread.Sleep(50);
                    };
                }
                else
                {
                    idleCounter++;
                    if (idleCounter == 15) 
                    { 
                        idleCounter = 0;
                        c_st[(int)index] = String.Format("Бездельничаю; здн вып {1} из {0}", c_get[(int)index], c_don[(int)index]);
                    };
                    //UpdateStatus("Waiting...", (int)index);
                    System.Threading.Thread.Sleep(1000);
                };
            };
        }

        private static string upLastError = "";
        private static bool upcrunning = true;
        private static string updCurrentStateTxt = "Загрузка";
        private static void UpdateConsole()
        {
            while (upcrunning)
            {
                UpdateStatus();
                Thread.Sleep(2000);
            };
        }
        
        static void UpdateStatus()
        {
            Console.Clear();

            Console.WriteLine(String.Format("Mapnik Tile Renderer Console Client, version: {0}", cversion));
            Console.WriteLine("Файл проекта: " + fileName);
            Console.WriteLine(String.Format("Клиент запущен в: {0}", started));
            Console.WriteLine("В работе {0} поток(а/ов), сервер: tcp://{1}/{2}", proj.RendererConfig.multithreadCount, server, curr_task.Replace("-", ""));
            Console.WriteLine();

            Console.WriteLine(String.Format("Здн плчн: {0}, в рбт {4}: в очрд {1}, в нрзк {2}; вып {3}", new object[] { c_ttl, queue.Count, c_alv, c_fin, (ulong)queue.Count + c_alv }));
            Console.WriteLine(String.Format("Тайл обр: {3}, сзд {0}, ппщ {1}, дыр {2} в {4} квдр", new object[] { t_crt, t_skp, t_wer, t_crt + t_skp + t_wer, t_zne }));
            Console.WriteLine(updCurrentStateTxt);
            Console.WriteLine();

            for (int i = 0; i < c_st.Length; i++)
                Console.WriteLine(String.Format("Поток {0}: {1}", i, c_st[i]));
            Console.WriteLine();
                        
            Console.WriteLine("Список последних полученых заданий от сервера:");            
            qlm.WaitOne();
            if (qlast.Count > 0)
                for (int i = qlast.Count - 1; i >= 0; i--)
                    Console.WriteLine(String.Format("{0} > {1}x {2}y {3}z - {4}", qlast[i]));
            qlm.ReleaseMutex();
            Console.WriteLine();
            
            // // // // ||  || \\ \\ \\ \\  
            // // //  LOG ERRORS  \\ \\ \\
            // // // // ||  || \\ \\ \\ \\
            
            // read stack errors
            List<TileRendering.TileRenderingErrors.ErrorInfo> errors = new List<TileRendering.TileRenderingErrors.ErrorInfo>();
            TileRendering.TileRenderingErrors.ErrorQueueMutex.WaitOne();
            while (TileRendering.TileRenderingErrors.ErrorQueue.Count > 0)
                errors.Add(TileRendering.TileRenderingErrors.ErrorQueue.Dequeue());
            TileRendering.TileRenderingErrors.ErrorQueueMutex.ReleaseMutex();

            if (errors.Count > 0)
                for (int i = 0; i < errors.Count; i++)
                {
                    TileRendering.TileRenderingErrors.ErrorInfo err = errors[i];
                    string txt = String.Format(
                        "\r\n{0}\r\nTHREAD {4} TILE {1}x {2}y {3}z\r\n{5}: {6}\r\nParameters: {7}\r\n"
                        , new object[]{
                        err.dt, err.x, err.y, err.z, err.ThreadID, err.comment, err.ex.Message, err.parameter
                    });

                    upLastError = txt;

                    try
                    {
                        System.IO.FileStream fs = new System.IO.FileStream(TileRendering.TileRenderer.GetCurrentDir() + @"\LOGS\client_[" + fileName + "]"+curr_task+".errorlog", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                        byte[] data = System.Text.Encoding.GetEncoding(1251).GetBytes(txt);
                        fs.Position = fs.Length;
                        fs.Write(data, 0, data.Length);
                        fs.Close();
                    }
                    catch { };
                };

            if (TileRendering.TileRenderingErrors.ErrorQueueTotalCount > 0)
            {
                Console.WriteLine("Всего ошибок: {0}\r\n", TileRendering.TileRenderingErrors.ErrorQueueTotalCount);
                Console.WriteLine("Последняя: {0}", upLastError);
            }
            else
                Console.WriteLine("Ошибок нет");
        }

        static void ColoredConsoleWriteLn(ConsoleColor color, string text)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }
    }
}
