using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Security;
using System.Web;
using System.Web.Security;

namespace MapnikTileRenderer
{
    public partial class CreateCMD : Form
    {
        string projectFile;
        Multilang ml = new Multilang();
        bool noSave_Start = false;

        public CreateCMD(string projectFile, bool noSave_Start)
        {
            InitializeComponent();
            this.noSave_Start = noSave_Start;
            if (noSave_Start)
            {
                this.Text = "Запуск нарезки в клиент-серверном режиме из-под консоли";
                button1.Text = "пуск сервера";
                button2.Text = "пуск клиента";
                label7.Text = "Запустить нарезку...";
            };
            this.projectFile = projectFile;
            string[] ips = GetLocalIPs();
            tIp.Items.AddRange(ips);
            this.tIp.Text = ips[ips.Length-1];            
            this.tTask.Text = ml.Translit(System.IO.Path.GetFileNameWithoutExtension(projectFile).Trim().Replace(" ", "_"));
            this.tServerAs.SelectedIndex = 0;
            this.tDump.SelectedIndex = 1;
            this.tSClose.SelectedIndex = 0;
            this.tCClose.SelectedIndex = 0;
            this.ccn.SelectedIndex = 0;
        }

        private void CreateCMD_Load(object sender, EventArgs e)
        {

        }

        private void tServerAs_SelectedIndexChanged(object sender, EventArgs e)
        {
            tDump.Visible = tServerAs.SelectedIndex == 0;
            tDumpLabel.Visible = tServerAs.SelectedIndex == 0;

            tHolesLabel.Visible = tServerAs.SelectedIndex > 0;
            tHoles.Visible = tServerAs.SelectedIndex > 0;
            tHolesB.Visible = tServerAs.SelectedIndex > 0;

            tHolesLabel.Text = tServerAs.SelectedIndex == 1 ? "Файл с дырами:" : "Файл дампа: ";
        }

        public static string[] GetLocalIPs()
        {
            List<string> ips = new List<string>();
            ips.Add("127.0.0.1");
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    ips.Add(ip.ToString());
            return ips.ToArray();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (noSave_Start)
            {
                if (tServerAs.SelectedIndex == 1)
                {
                    FileInfo fi = new FileInfo(tHoles.Text);
                    if (MessageBox.Show(String.Format("Файл с дырами: {1}\r\nРазмер файла {0}!\r\nВы действительно хотите запустить нарезку квадратов с дырами из-под сервера?", TileRendering.TileRenderer.FileSizeSuffix(fi.Length), tHoles.Text), "Запуск из-под сервера", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        return;
                };

                string pars = String.Format("\"{0}\"", projectFile);

                if (tServerAs.SelectedIndex == 0)
                    pars += (" /server");
                else if (tServerAs.SelectedIndex == 1)
                    pars += (" /holeserver");
                else
                    pars += (" /dumpserver");
                pars += String.Format(" {0}", (int)tPort.Value);
                if (tServerAs.SelectedIndex > 0)
                    pars += String.Format(" \"{0}\"", tHoles.Text);
                pars += String.Format(" -task_{0}", ml.Translit(tTask.Text.Trim().Replace(" ", "_")));
                if ((tServerAs.SelectedIndex == 0) && (tDump.SelectedIndex == 1))
                    pars += String.Format(" -dump");
                if (tSClose.SelectedIndex == 1)
                    pars += String.Format(" -cs");
                if (tCClose.SelectedIndex == 1)
                    pars += String.Format(" -cc");

                System.Diagnostics.Process.Start(TileRendering.TileRenderer.GetCurrentDir() + @"\mtrc.exe", pars);
                return;
            };

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "SERVER CMD FILE";
            sfd.DefaultExt = ".cmd";
            sfd.Filter = "CMD files (*.cmd)|*.cmd|Batch files (*.bat)|*.bat|All types (*.*)|*.*";
            sfd.FileName = "mtrc." + System.IO.Path.GetFileNameWithoutExtension(projectFile.Replace(" ", "_")) + ".multi.SERVER.cmd";
            if (sfd.ShowDialog() != DialogResult.OK) return;
            string fn = sfd.FileName;
            sfd.Dispose();
            string ext = System.IO.Path.GetExtension(fn).ToLower();
            if ((ext != ".cmd") && (ext != ".bat")) fn += ".cmd";

            FileStream fs = new FileStream(fn, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write("{0}\\mtrc.exe \"{1}\"",TileRendering.TileRenderer.GetCurrentDir(), projectFile);
            if (tServerAs.SelectedIndex == 0)
                sw.Write(" /server");
            else if (tServerAs.SelectedIndex == 1)
                sw.Write(" /holeserver");
            else
                sw.Write(" /dumpserver");
            sw.Write(" {0}", (int)tPort.Value);
            if (tServerAs.SelectedIndex > 0)
                sw.Write(" \"{0}\"", tHoles.Text);
            sw.Write(" -task_{0}", ml.Translit(tTask.Text.Trim().Replace(" ", "_")));
            if((tServerAs.SelectedIndex == 0) && (tDump.SelectedIndex == 1))
                sw.Write(" -dump");
            if(tSClose.SelectedIndex == 1)
                sw.Write(" -cs");
            if (tCClose.SelectedIndex == 1)
                sw.Write(" -cc");
            sw.Close();
            fs.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (noSave_Start)
            {
                string pars = String.Format("\"{0}\" /client", projectFile);
                pars += String.Format(" {0}:{1}", tIp.Text.Trim(), (int)tPort.Value);
                pars += String.Format(" -task_{0}", ml.Translit(tTask.Text.Trim().Replace(" ", "_")));
                pars += String.Format(" -threads_{0}", ccn.Text.Substring(0,2));

                System.Diagnostics.Process.Start(TileRendering.TileRenderer.GetCurrentDir() + @"\mtrc.exe", pars);
                return;
            };

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "CLIENT CMD FILE";
            sfd.DefaultExt = ".cmd";
            sfd.Filter = "CMD files (*.cmd)|*.cmd|Batch files (*.bat)|*.bat|All types (*.*)|*.*";
            sfd.FileName = "mtrc." + System.IO.Path.GetFileNameWithoutExtension(projectFile.Replace(" ", "_")) + ".multi.CLIENT-" + ccn.Text.Substring(0, 2).ToUpper() + ".cmd";
            if (sfd.ShowDialog() != DialogResult.OK) return;
            string fn = sfd.FileName;
            sfd.Dispose();
            string ext = System.IO.Path.GetExtension(fn).ToLower();
            if ((ext != ".cmd") && (ext != ".bat")) fn += ".cmd";

            FileStream fs = new FileStream(fn, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write("{0}\\mtrc.exe \"{1}\" /client", TileRendering.TileRenderer.GetCurrentDir(), projectFile);
            sw.Write(" {0}:{1}", tIp.Text.Trim(), (int)tPort.Value);
            sw.Write(" -task_{0}", ml.Translit(tTask.Text.Trim().Replace(" ", "_")));
            sw.Write(" -threads_{0}", ccn.Text.Substring(0, 2));
            sw.Close();
            fs.Close();
        }

        private void tHolesB_Click(object sender, EventArgs e)
        {
            if (tServerAs.SelectedIndex == 1)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.DefaultExt = "*.holes";
                ofd.Filter = "Holes & Error files (*.holes;*.errorlog)|*.holes;*.errorlog|Holes files (*.holes)|*.holes|Error Log files (*.errorlog)|*.errorlog|All Types (*.*)|*.*";
                ofd.Title = "Выберите файл с дырами";
                if (ofd.ShowDialog() != DialogResult.OK) return;
                tHoles.Text = ofd.FileName;
                ofd.Dispose();
            };
            if (tServerAs.SelectedIndex == 2)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.DefaultExt = "*.holes";
                ofd.Filter = "Dump files (*.dump)|*.dump|All Types (*.*)|*.*";
                ofd.Title = "Выберите файл дампа";
                if (ofd.ShowDialog() != DialogResult.OK) return;
                tHoles.Text = ofd.FileName;
                ofd.Dispose();
            };
        }
    }

    [Serializable]
    public class Multilang
    {
        private Dictionary<string, string> words = new Dictionary<string, string>();

        private string en = "";
        private string ru = "";

        public string EN
        {
            get { return en; }
            set
            {
                en = System.Web.HttpUtility.HtmlEncode(value);
            }
        }

        public string RU
        {
            get { return ru; }
            set
            {
                ru = System.Web.HttpUtility.HtmlEncode(value);
                if ((en == null) || (en == String.Empty) || (en.Length == 0)) en = Translit(ru);
            }
        }

        private void InitDict()
        {
            words.Add("а", "a");
            words.Add("б", "b");
            words.Add("в", "v");
            words.Add("г", "g");
            words.Add("д", "d");
            words.Add("е", "e");
            words.Add("ё", "yo");
            words.Add("ж", "zh");
            words.Add("з", "z");
            words.Add("и", "i");
            words.Add("й", "j");
            words.Add("к", "k");
            words.Add("л", "l");
            words.Add("м", "m");
            words.Add("н", "n");
            words.Add("о", "o");
            words.Add("п", "p");
            words.Add("р", "r");
            words.Add("с", "s");
            words.Add("т", "t");
            words.Add("у", "u");
            words.Add("ф", "f");
            words.Add("х", "h");
            words.Add("ц", "c");
            words.Add("ч", "ch");
            words.Add("ш", "sh");
            words.Add("щ", "sch");
            words.Add("ъ", "j");
            words.Add("ы", "i");
            words.Add("ь", "j");
            words.Add("э", "e");
            words.Add("ю", "yu");
            words.Add("я", "ya");
            words.Add("А", "A");
            words.Add("Б", "B");
            words.Add("В", "V");
            words.Add("Г", "G");
            words.Add("Д", "D");
            words.Add("Е", "E");
            words.Add("Ё", "Yo");
            words.Add("Ж", "Zh");
            words.Add("З", "Z");
            words.Add("И", "I");
            words.Add("Й", "J");
            words.Add("К", "K");
            words.Add("Л", "L");
            words.Add("М", "M");
            words.Add("Н", "N");
            words.Add("О", "O");
            words.Add("П", "P");
            words.Add("Р", "R");
            words.Add("С", "S");
            words.Add("Т", "T");
            words.Add("У", "U");
            words.Add("Ф", "F");
            words.Add("Х", "H");
            words.Add("Ц", "C");
            words.Add("Ч", "Ch");
            words.Add("Ш", "Sh");
            words.Add("Щ", "Sch");
            words.Add("Ъ", "J");
            words.Add("Ы", "I");
            words.Add("Ь", "J");
            words.Add("Э", "E");
            words.Add("Ю", "Yu");
            words.Add("Я", "Ya");

        }

        public string Translit(string RU)
        {
            string EN = RU;
            foreach (KeyValuePair<string, string> pair in words)
                EN = EN.Replace(pair.Key, pair.Value);
            return EN;
        }

        public Multilang()
        {
            InitDict();
        }

        public Multilang(string EN, string RU)
        {
            InitDict();
            this.EN = EN;
            this.RU = RU;
        }
    }

}