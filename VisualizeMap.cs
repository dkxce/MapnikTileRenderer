using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MapnikTileRenderer
{
    public partial class VisualizeMap : Form
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);        

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr SetFocus(HandleRef hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        enum ShowWindowCommands
        {
            /// <summary>
            /// Hides the window and activates another window.
            /// </summary>
            Hide = 0,
            /// <summary>
            /// Activates and displays a window. If the window is minimized or
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when displaying the window
            /// for the first time.
            /// </summary>
            Normal = 1,
            /// <summary>
            /// Activates the window and displays it as a minimized window.
            /// </summary>
            ShowMinimized = 2,
            /// <summary>
            /// Maximizes the specified window.
            /// </summary>
            Maximize = 3, // is this the right value?
            /// <summary>
            /// Activates the window and displays it as a maximized window.
            /// </summary>      
            ShowMaximized = 3,
            /// <summary>
            /// Displays a window in its most recent size and position. This value
            /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except
            /// the window is not activated.
            /// </summary>
            ShowNoActivate = 4,
            /// <summary>
            /// Activates the window and displays it in its current size and position.
            /// </summary>
            Show = 5,
            /// <summary>
            /// Minimizes the specified window and activates the next top-level
            /// window in the Z order.
            /// </summary>
            Minimize = 6,
            /// <summary>
            /// Displays the window as a minimized window. This value is similar to
            /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the
            /// window is not activated.
            /// </summary>
            ShowMinNoActive = 7,
            /// <summary>
            /// Displays the window in its current size and position. This value is
            /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the
            /// window is not activated.
            /// </summary>
            ShowNA = 8,
            /// <summary>
            /// Activates and displays the window. If the window is minimized or
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9,
            /// <summary>
            /// Sets the show state based on the SW_* value specified in the
            /// STARTUPINFO structure passed to the CreateProcess function by the
            /// program that started the application.
            /// </summary>
            ShowDefault = 10,
            /// <summary>
            ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread
            /// that owns the window is not responding. This flag should only be
            /// used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }

        private Form parent;
        public TileRendering.MultithreadTileRenderer[] renderers;
        //public TileRendering.MultithreadTileRenderer.ThreadRenderObject.

        public VisualizeMap(Form parent)
        {
            if (parent != null)
            {
                this.parent = parent;
                SetParent(this.Handle, this.parent.Handle);
            };
            InitializeComponent();
            panel2.Dock = DockStyle.Fill;
            comboBox2.SelectedIndex = 0;
            pictureBox1.Left = 0;
            pictureBox1.Top = 0;
        }

        public void ShowModal()
        {
            if(parent != null) parent.Enabled = false;
            timer1_Tick(this, null);
            if (skipbig == -2)
            {
                if (parent != null) parent.Enabled = true;
                Close();
                Dispose();
                return;
            };
            VisualizeMap_Resize(this, null);
            this.Show();
            while (Visible)
            {
                if (ApplicationIsActivated() && (GetForegroundWindow() != this.Handle))
                {
                    SetForegroundWindow(this.Handle);
                    Focus();
                };                
                Application.DoEvents();
            };
            if (parent != null) parent.Enabled = true;            
        }

        private static bool ApplicationIsActivated()
        {
            IntPtr activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero) return false; // No window is currently activated
            
            int procId = System.Diagnostics.Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }

        private void VisualizeMap_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (parent != null)
            {
                SetForegroundWindow(parent.Handle);
                parent.Focus();
            };
            Dispose();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == 0) timer1.Interval = 2000;
            if (comboBox2.SelectedIndex == 1) timer1.Interval = 5000;
            if (comboBox2.SelectedIndex == 2) timer1.Interval = 10000;
            if (comboBox2.SelectedIndex == 3) timer1.Interval = 15000;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((renderers == null) || (renderers.Length == 0)) return;
            timer1.Enabled = false;
            int ttls = 0;

            List<int> zooms = new List<int>();
            string zmt = "";
            for (int i = 1; i < renderers.Length; i++)
                if (renderers[i] != null)
                    if (renderers[i].VIEW != null)
                    {
                        zooms.Add(i);
                        zmt += String.Format("{0:00}", i);
                        ttls += renderers[i].VIEW.Size;
                    };
            string cbt = "";
            int selZoom = 0;
            if(comboBox1.Items.Count > 0)
                for (int i = 0; i < comboBox1.Items.Count; i++)
                {
                    cbt += String.Format("{0:00}", int.Parse(comboBox1.Items[i].ToString()));
                    if (comboBox1.SelectedIndex == i) selZoom = int.Parse(comboBox1.Items[i].ToString());
                };
            if (cbt != zmt)
            {
                comboBox1.Items.Clear();
                if(zooms.Count > 0)
                    for (int i = 0; i < zooms.Count; i++)
                    {
                        comboBox1.Items.Add(String.Format("{0:00}", zooms[i]));
                        if ((selZoom != 0) && (selZoom == zooms[i])) comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
                    };
                if ((comboBox1.SelectedIndex == -1) && (comboBox1.Items.Count > 0))
                {
                    comboBox1.SelectedIndex = 0;
                    selZoom = int.Parse(comboBox1.Items[0].ToString());
                };
            };

            tsZM.Text = TileRendering.TileRenderer.FileSizeSuffix(0);
            tsTTL.Text = TileRendering.TileRenderer.FileSizeSuffix(ttls);

            if ((selZoom > 0) && (renderers != null) && (renderers.Length != 0) && (renderers[selZoom] != null) && (renderers[selZoom].VIEW != null))
            {
                TileRendering.RenderView rv = renderers[selZoom].VIEW;
                tsZM.Text = TileRendering.TileRenderer.FileSizeSuffix(rv.Size);
                if((rv.Width > 3000) || (rv.Height > 2000))
                {
                    if(skipbig == -1)
                    {
                        DialogResult dr = MessageBox.Show(String.Format("Размер картинки составляет {0} x {1} пикселей!\r\nВы действительно хотите загрузить карту визуализации?", rv.Width, rv.Height), "Визуализация нарезки по квадратам", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                        if (dr == DialogResult.Cancel) 
                        { 
                            timer1.Enabled = true;
                            if ((Visible == false) && (zooms.Count < 2)) skipbig = -2;
                            return; 
                        };
                        if (dr == DialogResult.Yes) skipbig = 0;
                        if (dr == DialogResult.No) skipbig = 1;
                    };
                    if (skipbig == 1) { timer1.Enabled = true; return; };
                };
                Bitmap bmp = rv.GetView();
                if ((bmp.Width < 300) || (bmp.Height < 300))
                {
                    double dwh = (double)bmp.Width / (double)bmp.Height;
                    int neww = (bmp.Width >= bmp.Height) ? 300 : (int)(300.0 * dwh);
                    int newh = (bmp.Width >= bmp.Height) ? (int)(300.0 / dwh) : 300;
                    bmp = renderers[selZoom].VIEW.Resize(bmp, neww, newh);
                };
                pictureBox1.Image = bmp;
                pictureBox1.Width = bmp.Width;
                pictureBox1.Height = bmp.Height;
            };
            timer1.Enabled = true;
        }

        private int skipbig = -1;

        private void VisualizeMap_Resize(object sender, EventArgs e)
        {
            panel3.Left = Width - panel3.Width - 27;
            panel3.Top = Height - panel3.Height - 68;
        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {
            panel3.Visible = !panel3.Visible;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Enabled = false;
                timer1_Tick(sender, e);
            };
        }

        private System.Drawing.Imaging.PropertyItem CreatePropertyItem()
        {
            System.Reflection.ConstructorInfo ci = typeof(System.Drawing.Imaging.PropertyItem).GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null, new Type[0], null);
            return (System.Drawing.Imaging.PropertyItem)ci.Invoke(null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null) return;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".png";
            sfd.FileName = "image.png";
            sfd.Filter = "Portable Network Graphics (*.png)|*.png|All types (*.*)|*.*";
            if (sfd.ShowDialog() != DialogResult.OK) return;
            string fn = sfd.FileName;
            sfd.Dispose();
            
            // 0x010E Description
            // 0x010F CameraMaker
            // 0x0110 CameraModel            
            // 0x0131 Software  
            // 0x0132 DateTime Changed // count 20
            // 0x013B Artist
            // 0x8298 Copyright
            // 0x9003 DateTimeOrigin  // count 20
            // 0x9286 User Comment

            //System.Drawing.Imaging.PropertyItem pi = CreatePropertyItem();
            //pi.Id = 0x010F; //cameraMaker 
            //// 1- byte, 2 - array, 3 - short, 4 - long, 7 - byte[], 9 - int
            //pi.Type = 2;
            //pi.Value = Encoding.GetEncoding(1251).GetBytes("MyImageInfo");
            //pi.Len = pi.Value.Length;
            //pictureBox1.Image.SetPropertyItem(pi);

            //pi = CreatePropertyItem();
            //pi.Id = 0x010E; //Description 
            //pi.Type = 2;
            //pi.Value = Encoding.GetEncoding(1251).GetBytes("TEST");
            //pi.Len = pi.Value.Length;
            //pictureBox1.Image.SetPropertyItem(pi);

            //pi = CreatePropertyItem();
            //pi.Id = 0x9286; //Comment 
            //pi.Type = 2;
            //pi.Value = Encoding.GetEncoding(1251).GetBytes("ASSA");
            //pi.Len = pi.Value.Length;
            //pictureBox1.Image.SetPropertyItem(pi);

            pictureBox1.Image.Save(fn, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

    }   

}