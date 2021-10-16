using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.IO;

using System.Xml;
using System.Xml.Serialization;
using System.Configuration.Install;

using System.Net;
using System.Net.Sockets;

using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using System.Runtime.InteropServices;
using System.Reflection;

using System.Security;
using System.Security.Cryptography;

namespace Render_Schedule_Client
{
    [RunInstallerAttribute(true)]
    public class MyProjInstaller : Installer
    {
        private ServiceInstaller RouteSearcherSvcInstaller;
        private ServiceProcessInstaller serviceProcessInstaller1;

        public MyProjInstaller()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            RouteSearcherSvcInstaller = new System.ServiceProcess.ServiceInstaller();
            serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            RouteSearcherSvcInstaller.Description = "Сервис удаленных заданий Mapnik Tile Renderer Console";
            RouteSearcherSvcInstaller.DisplayName = "Render Schedule Client";
            RouteSearcherSvcInstaller.ServiceName = "render_schedule_client";
            RouteSearcherSvcInstaller.StartType = ServiceStartMode.Automatic;

            serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            serviceProcessInstaller1.Password = null;
            serviceProcessInstaller1.Username = null;

            //serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.User;
            //serviceProcessInstaller1.Password = "system";//null;
            //serviceProcessInstaller1.Username = @"Navicom\Argis";//null;            
            
            Installers.AddRange(new System.Configuration.Install.Installer[] {
            RouteSearcherSvcInstaller,
            serviceProcessInstaller1});
        }
    }

    public partial class RenderScheduleClientCtrl : ServiceBase
    {
        private RenderScheduleClient rsc;

        public RenderScheduleClientCtrl()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            rsc = new RenderScheduleClient();
            rsc.Start();
        }

        protected override void OnStop()
        {
            rsc.Stop();
            rsc = null;
        }
    }

    public class RenderScheduleClient
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

            public TASK(){}
        }

        string url = "http://127.0.0.1:9665/any";
        List<TASK> tasks = new List<TASK>();
        Thread runThread;
        bool runAlive = false;

        public RenderScheduleClient()
        {
            string rfn = GetCurrentDir() + @"\Render_Schedule_Client.ini";
            FileStream fs = new FileStream(rfn, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            url = sr.ReadToEnd().Replace("\r", "").Replace("\n", "").Trim();
            sr.Close();
            fs.Close();
        }

        public void Start()
        {
            runThread = new Thread(run);
            runThread.Start();
        }

        public void run()
        {
            runAlive = true;
            byte s5Counter = 0;
            while(runAlive)
            {
                //run
                if(tasks.Count > 0)
                {
                    for(int i=tasks.Count-1;i>=0;i--)
                    {
                        if ((DateTime.Now > tasks[i].RunAt) && (tasks[i].status == "queued"))
                        {
                            tasks[i].status = "starting";
                            
                            ProcessStartInfo psi = new ProcessStartInfo();
                            psi.UseShellExecute = false;
                            psi.FileName = tasks[i].cmd;
                            if (!String.IsNullOrEmpty(tasks[i].User))
                            {
                                string un = tasks[i].User;
                                if (un.IndexOf(@"\") > 0)
                                {
                                    psi.Domain = un.Substring(0, un.IndexOf(@"\"));
                                    un = un.Substring(un.IndexOf(@"\") + 1);
                                }
                                else
                                    psi.Domain = System.Environment.MachineName;
                                psi.UserName = un;
                            };
                            if (!String.IsNullOrEmpty(tasks[i].Pass)) psi.Password = ConvertToSecureString(tasks[i].Pass);                            

                            Process proc;
                            try
                            {                                
                                proc = Process.Start(psi);
                                tasks[i].Started = DateTime.Now;
                                if ((proc != null) && (proc.Handle != IntPtr.Zero))
                                    tasks[i].status = "started";
                                else
                                    tasks[i].status = "failed";
                            }
                            catch (Exception ex) 
                            {
                                tasks[i].Started = DateTime.Now;
                                tasks[i].status = "failed";
                                tasks[i].error = ex.Message;
                            };

                            XmlSimple<int>.Add2SysLog(String.Format("TASK `{0}` {1} try {2}\r\n{3}", new object[] { tasks[i].ID, tasks[i].status, tasks[i].Started, tasks[i].error }));
                        };                        
                    };
                    for (int i = tasks.Count - 1; i >= 0; i--)
                        UpdateServer(i, true);
                };
                // get tasks from server
                if(s5Counter == 0) // one times per 5 min
                {
                    // get server
                    System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(String.Format(url,"get"));
                    wr.Timeout = 5000;
                    try
                    {
                        System.Net.WebResponse res = wr.GetResponse();
                        System.IO.StreamReader sr = new System.IO.StreamReader(res.GetResponseStream(),Encoding.GetEncoding(1251));
                        string resp = sr.ReadToEnd();
                        sr.Close();

                        if (resp != "nothingtodo")
                        {
                            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(TASK[]));
                            MemoryStream ms = new MemoryStream();
                            byte[] bb = System.Text.Encoding.UTF8.GetBytes(resp);
                            ms.Write(bb, 0, bb.Length);
                            ms.Flush();
                            ms.Position = 0;
                            System.IO.StreamReader reader = new System.IO.StreamReader(ms);
                            TASK[] tasks_from_server = (TASK[])xs.Deserialize(reader);
                            reader.Close();
                            
                            if ((tasks_from_server != null) && (tasks_from_server.Length > 0))
                            {
                                if (tasks.Count > 0)
                                {
                                    // update tasks from server
                                    for (int infs = 0; infs < tasks_from_server.Length; infs++)
                                    {
                                        bool exists_on_client = false;
                                        for (int b = 0; b < tasks.Count; b++)
                                            if (tasks_from_server[infs].ID == tasks[b].ID)
                                            {
                                                exists_on_client = true;
                                                if (tasks[b].status == "queued") // started or failed need to update
                                                    tasks[b] = tasks_from_server[infs]; // update if not started
                                            };
                                        if (!exists_on_client) 
                                            tasks.Add(tasks_from_server[infs]);
                                    };
                                    // clear tasks not exists on server
                                    for (int b = tasks.Count - 1; b >= 0; b--)
                                    {
                                        bool exists_on_server = false;
                                        for (int infs = 0; infs < tasks_from_server.Length; infs++)
                                            if (tasks_from_server[infs].ID == tasks[b].ID)
                                                exists_on_server = true;
                                        if (!exists_on_server) 
                                        {
                                            UpdateServer(b, false);
                                            tasks.RemoveAt(b);
                                        };
                                    };
                                }
                                else 
                                    tasks.AddRange(tasks_from_server);
                            };
                        }
                        else if (resp == "nothingtodo")
                        {
                            if (tasks.Count > 0)
                                for (int i = tasks.Count - 1; i >= 0; i--)
                                    UpdateServer(i, true);
                            tasks.Clear();
                        };
                    }
                    catch (Exception ex)
                    {
                    };
                };
                Thread.Sleep(5000); // sleep 5 sec
                s5Counter++;
                if (s5Counter == 2) s5Counter = 0; // Set to ==60 when not-in-debug !!!!!!!!!!!!!!!!!!!!
            };

            
        }

        private bool UpdateServer(int taskIndex, bool canDelete)
        {

            if ((DateTime.Now > tasks[taskIndex].RunAt) && (tasks[taskIndex].status != "queued"))
            {
                // set server
                System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(String.Format(url, "set"));
                wr.Timeout = 5000;
                try
                {
                    byte[] arr = XmlSimple<TASK[]>.Save2BA(new TASK[] { tasks[taskIndex] });
                    wr.Method = "POST";
                    wr.ContentLength = arr.Length;
                    wr.ContentType = "data";
                    wr.GetRequestStream().Write(arr, 0, arr.Length);
                    wr.GetRequestStream().Flush();
                    //wr.GetRequestStream().Close();

                    System.Net.WebResponse res = wr.GetResponse();
                    System.IO.StreamReader sr = new System.IO.StreamReader(res.GetResponseStream(), Encoding.GetEncoding(1251));
                    string resp = sr.ReadToEnd();
                    sr.Close();

                    if (resp == "candelete")
                    {
                        if(canDelete) tasks.RemoveAt(taskIndex);
                        return true;
                    }
                    if(resp == "notfound")
                    {
                        XmlSimple<int>.Add2SysLog(String.Format("Task `{0}` not found on server",tasks[taskIndex].ID));
                        if (canDelete) tasks.RemoveAt(taskIndex);
                        return true;
                    };
                }
                catch (Exception ex)
                {
                };
            };
            return false;
        }

        public void Stop()
        {
            runAlive = false;
        }

        public static System.Security.SecureString ConvertToSecureString(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            unsafe
            {
                fixed (char* passwordChars = password)
                {
                    System.Security.SecureString securePassword = new System.Security.SecureString(passwordChars, password.Length);
                    securePassword.MakeReadOnly();
                    return securePassword;
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
    }

    [Serializable]
    public class XmlSimple<T>
    {               
        public static byte[] Save2BA(T obj)
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
            return bb;
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
