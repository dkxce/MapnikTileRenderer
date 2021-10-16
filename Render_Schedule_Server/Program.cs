using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.IO;

using System.Xml;
using System.Xml.Serialization;

using System.Net;
using System.Net.Sockets;

using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using System.Runtime.InteropServices;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;

using System.Web;

namespace Render_Schedule_Server
{
    public class Program
    {        
        static void Main(string[] args)
        {
            int port = 9665;
            if ((args != null) && (args.Length > 0)) int.TryParse(args[0], out port);
            RSS rss = new RSS(port);
        }

        static void ColoredConsoleWriteLn(ConsoleColor color, string text)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }

        public class RSS
        {
            [Serializable]
            public class TASK
            {
                [XmlAttribute]
                public string IP;
                [XmlAttribute]
                public string ID;
                [XmlAttribute]
                public string User;
                [XmlAttribute]
                public string Pass;
                [XmlAttribute]
                public string cmd;
                [XmlAttribute]
                public DateTime RunAt;
                [XmlAttribute]
                public DateTime Started;
                [XmlAttribute]
                public string status; // todo,queued,started,failed,disable
                public string error;

                public TASK() { }

                public override string ToString()
                {
                    if((Started == null) || (Started < DateTime.Now.AddYears(-10)))
                        return String.Format("{0}:{1} [{2}] at {3} never", new object[] { IP, ID, status, RunAt });
                    else
                        return String.Format("{0}:{1} [{2}] at {3} try {4}", new object[] { IP, ID, status, RunAt, Started });
                }
            }

            int port = 9665;
            HTTPListener svr;
            public List<TASK> tasks = new List<TASK>();
            public Mutex tasksm = new Mutex();

            public RSS(int port)
            {
                this.port = port;

                //TASK tt = new TASK();
                //tt.error = "ошбк";
                //tt.ID = "testA";
                //tt.IP = "127.0.0.1";
                ////tt.User = "sa";
                ////tt.Pass = "q";
                //tt.RunAt = DateTime.Now.AddSeconds(15);
                //tt.status = "todo";
                //tt.cmd = "cmd.exe";
                //tasks.Add(tt);

                //tt = new TASK();
                //tt.ID = "testB";
                //tt.IP = "127.0.0.1";
                //tt.User = "sa";
                //tt.Pass = "q";
                //tt.RunAt = DateTime.Now.AddSeconds(25);
                //tt.status = "todo";
                //tt.cmd = "cmd.exe";
                //tasks.Add(tt);

                //tt = new TASK();
                //tt.ID = "testC";
                //tt.IP = "127.0.0.1";
                //tt.User = "Navicom\\Argis";
                //tt.Pass = "system";
                //tt.RunAt = DateTime.Now.AddSeconds(45);
                //tt.status = "todo";
                //tt.cmd = "cmd.exe";
                //tasks.Add(tt);

                //tt = new TASK();
                //tt.ID = "testD";
                //tt.IP = "127.0.0.1";
                //tt.User = "dsdg";
                //tt.Pass = "gfhreh";
                //tt.RunAt = DateTime.Now.AddSeconds(-45);
                //tt.status = "todo";
                //tt.cmd = "cmd.exe";
                //tasks.Add(tt);

                TASK[] tlist = XMLSimple<TASK[]>.LoadFile(GetCurrentDir() + @"\clients_tasks_list.todo");
                tasks.AddRange(tlist);

                foreach (TASK t in tasks)
                {
                    if (t.status == "queued") 
                        t.status = "todo";
                    if ((t.status == "todo") && (t.RunAt >= DateTime.Now)) 
                        t.error = null;
                    if ((t.status == "todo") && ((t.RunAt == null) || (t.RunAt < DateTime.Now))) 
                        t.status = "skip";                    
                };

                (svr = new HTTPListener(port, 5, this)).Start();
                (new Thread(RefreshTimer)).Start();
            }

            public void RefreshTimer()
            {
                while (true)
                {                    
                    Refresh();
                    Thread.Sleep(1000);
                };
            }

            public void Refresh()
            {
                Console.Clear();
                Console.WriteLine("Render Schedule Server");
                Console.WriteLine("Порт входящих подключений: "+port.ToString());                
                if (tasks.Count > 0)
                {
                    Console.WriteLine("Список Заданий:");
                    tasksm.WaitOne();
                    foreach (TASK t in tasks)
                    {
                        if (t.status == "skip")
                            ColoredConsoleWriteLn(ConsoleColor.DarkGray, t.ToString());
                        else if(t.status == "started")
                            ColoredConsoleWriteLn(ConsoleColor.DarkGreen, t.ToString());
                        else if(t.status == "failed")
                            ColoredConsoleWriteLn(ConsoleColor.DarkRed, t.ToString());
                        else
                            Console.WriteLine(t);
                    };
                    tasksm.ReleaseMutex();
                }
                else
                    Console.WriteLine("Нет заданий");
            }

            /////// SERVER
            public class HTTPListener
            {
                private Thread mainThread = null;
                private TcpListener mainListener = null;
                private IPAddress ListenIP = IPAddress.Any;
                private int ListenPort = 80;
                private bool isRunning = false;
                private int MaxThreads = 1;
                private int ThreadsCount = 0;
                private RSS rss;

                public HTTPListener(int Port, int MaxThreads, RSS rss)
                {
                    this.ListenPort = Port;
                    this.MaxThreads = MaxThreads;
                    this.rss = rss;
                }
                
                public bool Running { get { return isRunning; } }
                public IPAddress ServerIP { get { return ListenIP; } }
                public int ServerPort { get { return ListenPort; } }

                public void Dispose() { Stop(); }
                ~HTTPListener() { Dispose(); }

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
                    byte[] Buffer = new byte[20480];
                    int Count = 0;

                    try
                    {
                        while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
                        {
                            Request += Encoding.GetEncoding(1251).GetString(Buffer, 0, Count);
                            if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 20480) { break; };
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
                    string data = Request.Substring(Request.IndexOf("\r\n\r\n") + 4);

                    // ONLY ALLOWED METHODS
                    string[] qpc = query.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries);
                    if ((qpc.Length == 0) || (qpc[0] != "/render_schedule"))
                    {
                        HttpClientSendError(Client, 501);
                        return;
                    };

                    string ip = ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
                    System.Collections.Specialized.NameValueCollection nvc = HttpUtility.ParseQueryString(qpc[1]);
                    string mth = nvc["mth"];
                    
                    if(mth == "get")
                    {
                        if (rss.tasks.Count == 0)
                        {
                            HttpClientSendData(Client, "nothingtodo");
                            return;
                        }
                        else
                        {
                            bool save = false;
                            rss.tasksm.WaitOne();
                            List<TASK> ipt = new List<TASK>();
                            for (int i = 0; i < rss.tasks.Count; i++)
                                if (rss.tasks[i].IP == ip)
                                    if ((rss.tasks[i].status == "todo") || (rss.tasks[i].status == "queued"))
                                    {
                                        if (rss.tasks[i].status == "todo") save = true;
                                        rss.tasks[i].status = "queued";
                                        ipt.Add(rss.tasks[i]);
                                    };
                            if (save)
                                XMLSimple<TASK[]>.Save(GetCurrentDir() + @"\clients_tasks_list.todo", rss.tasks.ToArray());
                            rss.tasksm.ReleaseMutex();
                            if (ipt.Count == 0)
                            {
                                HttpClientSendData(Client, "nothingtodo");                                                                        
                                return;
                            }
                            else
                            {
                                string xml = XMLSimple<TASK[]>.Save(ipt.ToArray());
                                HttpClientSendData(Client, xml);
                                return;
                            };                            
                        };
                    };
                    if (mth == "set")
                    {
                        bool found = false;
                        TASK[] ipt = null;
                        try
                        {
                            ipt = XMLSimple<TASK[]>.LoadText(data);
                        }
                        catch (Exception ex)
                        {
                            HttpClientSendData(Client, "parseerror");
                            return;
                        };
                        if ((ipt != null) && (ipt.Length > 0) && (rss.tasks.Count > 0))
                        {
                            rss.tasksm.WaitOne();
                            for(int a=0;a<ipt.Length;a++)
                                for(int b=0;b<rss.tasks.Count;b++)
                                    if(ipt[a].IP == rss.tasks[b].IP)
                                        if (ipt[a].ID == rss.tasks[b].ID)
                                        {
                                            found = true;
                                            rss.tasks[b].Started = ipt[a].Started;
                                            rss.tasks[b].status = ipt[a].status;
                                            rss.tasks[b].error = ipt[a].error;
                                        };
                            rss.tasksm.ReleaseMutex();
                        };
                        if (found)
                        {
                            HttpClientSendData(Client, "candelete");
                            XMLSimple<TASK[]>.Save(GetCurrentDir() + @"\clients_tasks_list.todo", rss.tasks.ToArray());
                        }
                        else
                            HttpClientSendData(Client, "notfound");
                        return;
                    };

                    Client.Close();
                }
                // END METHOD RECEIVE DATE
                

                #region Private ReceiveData Methods
                private void HttpClientSendData(TcpClient Client, string data)
                {
                    string CodeStr = "200 " + ((HttpStatusCode)200).ToString();
                    string Str =
                        "HTTP/1.1 " + CodeStr + "\r\n" +
                        "Connection: close\r\n" +
                        "Content-type: text/html\r\n" +
                        "Content-Encoding: windows-1251\r\n" +
                        "Content-Length:" + data.Length.ToString() + "\r\n\r\n" + data;

                    byte[] Buffer = Encoding.GetEncoding(1251).GetBytes(Str);
                    Client.GetStream().Write(Buffer, 0, Buffer.Length);
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
                        "Content-type: text/html\r\n" +
                        "Content-Encoding: windows-1251\r\n" +
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

        [Serializable]
        public class XMLSimple<T>
        {
            /// <summary>
            ///     Сохранение структуры в файл
            /// </summary>
            /// <param name="file">Полный путь к файлу</param>
            /// <param name="obj">Структура</param>
            public static void Save(string file, T obj)
            {
                //Create our own namespaces for the output
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
                System.IO.StreamWriter writer = System.IO.File.CreateText(file);
                xs.Serialize(writer, obj, ns);
                writer.Flush();
                writer.Close();
            }

            public static string Save(T obj)
            {
                //Create our own namespaces for the output
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
                System.IO.MemoryStream ms = new MemoryStream();
                System.IO.StreamWriter writer = new StreamWriter(ms, System.Text.Encoding.GetEncoding(1251));
                xs.Serialize(writer, obj, ns);
                writer.Flush();
                ms.Position = 0;
                byte[] bb = new byte[ms.Length];
                ms.Read(bb, 0, bb.Length);
                writer.Close();
                return System.Text.Encoding.GetEncoding(1251).GetString(bb); ;
            }

            /// <summary>
            ///     Подключение структуры из файла
            /// </summary>
            /// <param name="file">Полный путь к файлу</param>
            /// <returns>Структура</returns>
            public static T Load(string file)
            {
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
                System.IO.StreamReader reader = System.IO.File.OpenText(file);
                T c = (T)xs.Deserialize(reader);
                reader.Close();
                return c;
            }

            public static T LoadText(string text)
            {
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
                MemoryStream ms = new MemoryStream();
                byte[] bb = System.Text.Encoding.UTF8.GetBytes(text);
                ms.Write(bb, 0, bb.Length);
                ms.Flush();
                ms.Position = 0;
                System.IO.StreamReader reader = new System.IO.StreamReader(ms);
                T c = (T)xs.Deserialize(reader);
                reader.Close();
                return c;
            }

            /// <summary>
            ///     Подключение структуры из файла
            /// </summary>
            /// <param name="file">Полный путь к файлу</param>
            /// <returns>Структура</returns>
            public static T LoadFile(string file) { return Load(file); }
            
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
        }
    }
}
