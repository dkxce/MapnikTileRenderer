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
                this.Text = "������ ������� � ������-��������� ������ ��-��� �������";
                button1.Text = "���� �������";
                button2.Text = "���� �������";
                label7.Text = "��������� �������...";
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

            tHolesLabel.Text = tServerAs.SelectedIndex == 1 ? "���� � ������:" : "���� �����: ";
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
                    if (MessageBox.Show(String.Format("���� � ������: {1}\r\n������ ����� {0}!\r\n�� ������������� ������ ��������� ������� ��������� � ������ ��-��� �������?", TileRendering.TileRenderer.FileSizeSuffix(fi.Length), tHoles.Text), "������ ��-��� �������", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
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
                ofd.Title = "�������� ���� � ������";
                if (ofd.ShowDialog() != DialogResult.OK) return;
                tHoles.Text = ofd.FileName;
                ofd.Dispose();
            };
            if (tServerAs.SelectedIndex == 2)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.DefaultExt = "*.holes";
                ofd.Filter = "Dump files (*.dump)|*.dump|All Types (*.*)|*.*";
                ofd.Title = "�������� ���� �����";
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
            words.Add("�", "a");
            words.Add("�", "b");
            words.Add("�", "v");
            words.Add("�", "g");
            words.Add("�", "d");
            words.Add("�", "e");
            words.Add("�", "yo");
            words.Add("�", "zh");
            words.Add("�", "z");
            words.Add("�", "i");
            words.Add("�", "j");
            words.Add("�", "k");
            words.Add("�", "l");
            words.Add("�", "m");
            words.Add("�", "n");
            words.Add("�", "o");
            words.Add("�", "p");
            words.Add("�", "r");
            words.Add("�", "s");
            words.Add("�", "t");
            words.Add("�", "u");
            words.Add("�", "f");
            words.Add("�", "h");
            words.Add("�", "c");
            words.Add("�", "ch");
            words.Add("�", "sh");
            words.Add("�", "sch");
            words.Add("�", "j");
            words.Add("�", "i");
            words.Add("�", "j");
            words.Add("�", "e");
            words.Add("�", "yu");
            words.Add("�", "ya");
            words.Add("�", "A");
            words.Add("�", "B");
            words.Add("�", "V");
            words.Add("�", "G");
            words.Add("�", "D");
            words.Add("�", "E");
            words.Add("�", "Yo");
            words.Add("�", "Zh");
            words.Add("�", "Z");
            words.Add("�", "I");
            words.Add("�", "J");
            words.Add("�", "K");
            words.Add("�", "L");
            words.Add("�", "M");
            words.Add("�", "N");
            words.Add("�", "O");
            words.Add("�", "P");
            words.Add("�", "R");
            words.Add("�", "S");
            words.Add("�", "T");
            words.Add("�", "U");
            words.Add("�", "F");
            words.Add("�", "H");
            words.Add("�", "C");
            words.Add("�", "Ch");
            words.Add("�", "Sh");
            words.Add("�", "Sch");
            words.Add("�", "J");
            words.Add("�", "I");
            words.Add("�", "J");
            words.Add("�", "E");
            words.Add("�", "Yu");
            words.Add("�", "Ya");

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