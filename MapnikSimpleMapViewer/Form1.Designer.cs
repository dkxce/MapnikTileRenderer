namespace MapnikSimpleMapCreator
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public System.ComponentModel.ComponentResourceManager resources;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.загрузитьНастройкиИзФайлаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьНайтсройкиВФайлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.проверитьСинтаксисXMLФайлаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewFBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.savImg = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.svrMnu = new System.Windows.Forms.ToolStripMenuItem();
            this.smm = new System.Windows.Forms.ToolStripMenuItem();
            this.ssm = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rLabel = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.sisr = new System.Windows.Forms.PictureBox();
            this.button2 = new System.Windows.Forms.Button();
            this.iZooms = new System.Windows.Forms.ComboBox();
            this.iStat = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.iMapFileName = new System.Windows.Forms.TextBox();
            this.isY = new System.Windows.Forms.TextBox();
            this.isX = new System.Windows.Forms.TextBox();
            this.Lat = new System.Windows.Forms.Label();
            this.lon = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sisr)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.ContextMenuStrip = this.contextMenuStrip1;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(629, 448);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 54;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.DoubleClick += new System.EventHandler(this.pictureBox1_DoubleClick);
            this.pictureBox1.MouseLeave += new System.EventHandler(this.pictureBox1_MouseLeave);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseEnter += new System.EventHandler(this.pictureBox1_MouseEnter);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.загрузитьНастройкиИзФайлаToolStripMenuItem,
            this.сохранитьНайтсройкиВФайлToolStripMenuItem,
            this.toolStripMenuItem1,
            this.проверитьСинтаксисXMLФайлаToolStripMenuItem,
            this.viewFBtn,
            this.toolStripMenuItem2,
            this.savImg,
            this.toolStripMenuItem3,
            this.svrMnu,
            this.smm,
            this.ssm});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(363, 198);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // загрузитьНастройкиИзФайлаToolStripMenuItem
            // 
            this.загрузитьНастройкиИзФайлаToolStripMenuItem.Name = "загрузитьНастройкиИзФайлаToolStripMenuItem";
            this.загрузитьНастройкиИзФайлаToolStripMenuItem.Size = new System.Drawing.Size(362, 22);
            this.загрузитьНастройкиИзФайлаToolStripMenuItem.Text = "Загрузить настройки из файла...";
            this.загрузитьНастройкиИзФайлаToolStripMenuItem.Click += new System.EventHandler(this.загрузитьНастройкиИзФайлаToolStripMenuItem_Click);
            // 
            // сохранитьНайтсройкиВФайлToolStripMenuItem
            // 
            this.сохранитьНайтсройкиВФайлToolStripMenuItem.Name = "сохранитьНайтсройкиВФайлToolStripMenuItem";
            this.сохранитьНайтсройкиВФайлToolStripMenuItem.Size = new System.Drawing.Size(362, 22);
            this.сохранитьНайтсройкиВФайлToolStripMenuItem.Text = "Сохранить настройки в файл...";
            this.сохранитьНайтсройкиВФайлToolStripMenuItem.Click += new System.EventHandler(this.сохранитьНайтсройкиВФайлToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(359, 6);
            // 
            // проверитьСинтаксисXMLФайлаToolStripMenuItem
            // 
            this.проверитьСинтаксисXMLФайлаToolStripMenuItem.Name = "проверитьСинтаксисXMLФайлаToolStripMenuItem";
            this.проверитьСинтаксисXMLФайлаToolStripMenuItem.Size = new System.Drawing.Size(362, 22);
            this.проверитьСинтаксисXMLФайлаToolStripMenuItem.Text = "Проверить синтаксис XML файла";
            this.проверитьСинтаксисXMLФайлаToolStripMenuItem.Click += new System.EventHandler(this.проверитьСинтаксисXMLФайлаToolStripMenuItem_Click);
            // 
            // viewFBtn
            // 
            this.viewFBtn.Name = "viewFBtn";
            this.viewFBtn.Size = new System.Drawing.Size(362, 22);
            this.viewFBtn.Text = "Просмотр списка шрифтов...";
            this.viewFBtn.Click += new System.EventHandler(this.viewFonts);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(359, 6);
            // 
            // savImg
            // 
            this.savImg.Name = "savImg";
            this.savImg.Size = new System.Drawing.Size(362, 22);
            this.savImg.Text = "Сохранить изображение...";
            this.savImg.Click += new System.EventHandler(this.saveImage);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(359, 6);
            // 
            // svrMnu
            // 
            this.svrMnu.Name = "svrMnu";
            this.svrMnu.Size = new System.Drawing.Size(362, 22);
            this.svrMnu.Text = "Simple WMS HTTP Server (localhot:7759)";
            this.svrMnu.Click += new System.EventHandler(this.svrMnu_Click);
            // 
            // smm
            // 
            this.smm.Enabled = false;
            this.smm.Name = "smm";
            this.smm.Size = new System.Drawing.Size(362, 22);
            this.smm.Text = "Открыть просмотр мультитайловой карты в браузере";
            this.smm.Click += new System.EventHandler(this.smm_Click);
            // 
            // ssm
            // 
            this.ssm.Enabled = false;
            this.ssm.Name = "ssm";
            this.ssm.Size = new System.Drawing.Size(362, 22);
            this.ssm.Text = "Открыть просмотр однотайловой карты в браузере";
            this.ssm.Click += new System.EventHandler(this.ssm_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.ContextMenuStrip = this.contextMenuStrip1;
            this.panel1.Controls.Add(this.rLabel);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 50);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 700);
            this.panel1.TabIndex = 54;
            // 
            // rLabel
            // 
            this.rLabel.AutoSize = true;
            this.rLabel.BackColor = System.Drawing.Color.White;
            this.rLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rLabel.Location = new System.Drawing.Point(292, 291);
            this.rLabel.Name = "rLabel";
            this.rLabel.Size = new System.Drawing.Size(248, 46);
            this.rLabel.TabIndex = 55;
            this.rLabel.Text = "Rendering...";
            this.rLabel.Visible = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.sisr);
            this.panel2.Controls.Add(this.button2);
            this.panel2.Controls.Add(this.iZooms);
            this.panel2.Controls.Add(this.iStat);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Controls.Add(this.button3);
            this.panel2.Controls.Add(this.button4);
            this.panel2.Controls.Add(this.iMapFileName);
            this.panel2.Controls.Add(this.isY);
            this.panel2.Controls.Add(this.isX);
            this.panel2.Controls.Add(this.Lat);
            this.panel2.Controls.Add(this.lon);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(800, 50);
            this.panel2.TabIndex = 55;
            // 
            // sisr
            // 
            this.sisr.Image = global::MapnikSimpleMapViewer.Properties.Resources.objectsPng;
            this.sisr.Location = new System.Drawing.Point(777, 28);
            this.sisr.Name = "sisr";
            this.sisr.Size = new System.Drawing.Size(19, 19);
            this.sisr.TabIndex = 57;
            this.sisr.TabStop = false;
            this.sisr.Visible = false;
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button2.Location = new System.Drawing.Point(147, 25);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(70, 20);
            this.button2.TabIndex = 3;
            this.button2.Text = "RENDER";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // iZooms
            // 
            this.iZooms.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.iZooms.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.iZooms.FormattingEnabled = true;
            this.iZooms.Items.AddRange(new object[] {
            "Zoom 1",
            "Zoom 2",
            "Zoom 3",
            "Zoom 4",
            "Zoom 5",
            "Zoom 6",
            "Zoom 7",
            "Zoom 8",
            "Zoom 9",
            "Zoom 10",
            "Zoom 11",
            "Zoom 12",
            "Zoom 13",
            "Zoom 14",
            "Zoom 15",
            "Zoom 16",
            "Zoom 17",
            "Zoom 18",
            "Zoom 19",
            "Zoom 20",
            "Zoom 21"});
            this.iZooms.Location = new System.Drawing.Point(147, 3);
            this.iZooms.Name = "iZooms";
            this.iZooms.Size = new System.Drawing.Size(70, 21);
            this.iZooms.TabIndex = 2;
            this.iZooms.SelectedIndexChanged += new System.EventHandler(this.iZooms_SelectedIndexChanged);
            // 
            // iStat
            // 
            this.iStat.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.iStat.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.iStat.Location = new System.Drawing.Point(222, 28);
            this.iStat.Name = "iStat";
            this.iStat.ReadOnly = true;
            this.iStat.Size = new System.Drawing.Size(551, 16);
            this.iStat.TabIndex = 56;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(219, 7);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(86, 13);
            this.label11.TabIndex = 54;
            this.label11.Text = "Mapnik XML file:";
            // 
            // button3
            // 
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button3.Location = new System.Drawing.Point(773, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(24, 20);
            this.button3.TabIndex = 53;
            this.button3.Text = "X";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button4.Location = new System.Drawing.Point(749, 3);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(24, 20);
            this.button4.TabIndex = 52;
            this.button4.Text = "...";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // iMapFileName
            // 
            this.iMapFileName.Location = new System.Drawing.Point(311, 3);
            this.iMapFileName.Name = "iMapFileName";
            this.iMapFileName.Size = new System.Drawing.Size(432, 20);
            this.iMapFileName.TabIndex = 51;
            // 
            // isY
            // 
            this.isY.Location = new System.Drawing.Point(60, 3);
            this.isY.Name = "isY";
            this.isY.Size = new System.Drawing.Size(82, 20);
            this.isY.TabIndex = 0;
            this.isY.Text = "52.86";
            // 
            // isX
            // 
            this.isX.Location = new System.Drawing.Point(60, 25);
            this.isX.Name = "isX";
            this.isX.Size = new System.Drawing.Size(82, 20);
            this.isX.TabIndex = 1;
            this.isX.Text = "39.33";
            // 
            // Lat
            // 
            this.Lat.AutoSize = true;
            this.Lat.Location = new System.Drawing.Point(-1, 6);
            this.Lat.Name = "Lat";
            this.Lat.Size = new System.Drawing.Size(59, 13);
            this.Lat.TabIndex = 48;
            this.Lat.Text = "Center Lat:";
            // 
            // lon
            // 
            this.lon.AutoSize = true;
            this.lon.Location = new System.Drawing.Point(-1, 28);
            this.lon.Name = "lon";
            this.lon.Size = new System.Drawing.Size(62, 13);
            this.lon.TabIndex = 47;
            this.lon.Text = "Center Lon:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 750);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "[NONE]";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sisr)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox iMapFileName;
        private System.Windows.Forms.TextBox isY;
        private System.Windows.Forms.TextBox isX;
        private System.Windows.Forms.Label Lat;
        private System.Windows.Forms.Label lon;
        private System.Windows.Forms.TextBox iStat;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem загрузитьНастройкиИзФайлаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сохранитьНайтсройкиВФайлToolStripMenuItem;
        private System.Windows.Forms.ComboBox iZooms;
        private System.Windows.Forms.ToolStripMenuItem svrMnu;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem smm;
        private System.Windows.Forms.ToolStripMenuItem ssm;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem viewFBtn;
        private System.Windows.Forms.ToolStripMenuItem проверитьСинтаксисXMLФайлаToolStripMenuItem;
        private System.Windows.Forms.Label rLabel;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem savImg;
        private System.Windows.Forms.PictureBox sisr;

    }
}

