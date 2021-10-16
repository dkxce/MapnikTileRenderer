using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Web;

namespace SaveHttpDump
{
    class Program
    {
        static void Main(string[] args)
        {
            if ((args == null) || (args.Length == 0))
            {
                Console.WriteLine("Use syntax: SaveHttpDump.exe IP:Port [savefilename]");
                Console.WriteLine("   example: SaveHttpDump.exe 127.0.0.1:9666");
                Console.WriteLine("   example: SaveHttpDump.exe 127.0.0.1:9666 last_render.dump");
                Console.ReadLine();
            };
            string preurl = args[0].Trim();
            string fileName = "last_render.dump";
            if (args.Length > 1) fileName = args[1].Trim();

            string Uri_url = String.Format("http://{0}/dump", preurl);
            Console.WriteLine("Downloading dump from " + Uri_url);
            Console.Write("Getting server status...");
            System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(Uri_url);            
            try
            {                
                System.Net.WebResponse res = wr.GetResponse();
                System.IO.Stream sr = res.GetResponseStream();
                int fileLength = (int)res.ContentLength;
                int ttlBytes = fileLength;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.WriteLine("Dump file size is {0} bytes", fileLength);                
                FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                byte[] ba = new byte[8192];
                while (fileLength > ba.Length)
                {
                    sr.Read(ba, 0, ba.Length);
                    fs.Write(ba, 0, ba.Length);
                    fileLength -= ba.Length;
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("Download {1:0}%, {0}/{2} bytes", fs.Length, (double)fs.Length / (double)ttlBytes * 100.0, ttlBytes);
                };
                if (fileLength > 0)
                {
                    sr.Read(ba, 0, fileLength);
                    fs.Write(ba, 0, fileLength);
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("Download {1:0}%, {0}/{2} bytes", fs.Length, (double)fs.Length / (double)ttlBytes * 100.0, ttlBytes);
                };                
                sr.Close();
                res.Close();
                fs.Close();
                Console.WriteLine();
                Console.WriteLine("Dwonload completed");
                Console.WriteLine("Dump saved in `"+fileName+"`");
                System.Threading.Thread.Sleep(3000);
            }
            catch (System.Net.WebException err) 
            {
                Console.WriteLine();
                Console.WriteLine("Error: " + err.ToString());
                System.Threading.Thread.Sleep(8000);
            };            
        }
    }
}
