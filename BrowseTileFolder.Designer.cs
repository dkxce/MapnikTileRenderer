namespace MapnikTileRenderer
{
    partial class BrowseTileFolder
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrowseTileFolder));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel5 = new System.Windows.Forms.ToolStripStatusLabel();
            this.map = new NaviMapNet.NaviMapNetViewer();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel3,
            this.toolStripStatusLabel4,
            this.toolStripStatusLabel5});
            this.statusStrip1.Location = new System.Drawing.Point(0, 605);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(950, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem3,
            this.toolStripMenuItem2,
            this.toolStripSeparator1,
            this.toolStripMenuItem4,
            this.toolStripComboBox1,
            this.toolStripMenuItem5});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(51, 20);
            this.toolStripDropDownButton1.Text = "Карта";
            this.toolStripDropDownButton1.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Checked = true;
            this.toolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(260, 22);
            this.toolStripMenuItem1.Text = "Текущий проект";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(260, 22);
            this.toolStripMenuItem3.Text = "Navicom Tiles";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(260, 22);
            this.toolStripMenuItem2.Text = "OSM Mapnik";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(257, 6);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Checked = true;
            this.toolStripMenuItem4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(260, 22);
            this.toolStripMenuItem4.Text = "Отображать границы тайлов";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.toolStripMenuItem4_Click);
            // 
            // toolStripComboBox1
            // 
            this.toolStripComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox1.Items.AddRange(new object[] {
            "Размер зоны 1x1, 256x256 (1 тайл)",
            "Размер зоны 2x2, 512x512 (4 тайла)",
            "Размер зоны 3х3, 768x768 (9 тайлов)",
            "Размер зоны 4x4, 1024х1024 (16 тайлов)",
            "Размер зоны 5х5, 1280х1280 (25 тайлов)",
            "Размер зоны 6х6, 1536х1536 (36 тайлов)",
            "Размер зоны 7х7, 1792х1792 (49 тайлов)",
            "Размер зоны 8x8, 2048x2048 (64 тайла)",
            "Размер зоны 9х9, 2304x2304 (81 тайл)",
            "Размер зоны 10х10, 2560х2560 (100 тайлов)",
            "Размер зоны 11х11, 2816х2816 (121 тайл)",
            "Размер зоны 12х12, 3072х3072 (144 тайла)",
            "Размер зоны 13х13, 3328x3328 (169 тайлов)",
            "Размер зоны 14х14, 3584x3584 (196 тайлов)",
            "Размер зоны 15х15, 3840x3840 (225 тайлов)"});
            this.toolStripComboBox1.Name = "toolStripComboBox1";
            this.toolStripComboBox1.Size = new System.Drawing.Size(200, 21);
            this.toolStripComboBox1.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox1_SelectedIndexChanged);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Checked = true;
            this.toolStripMenuItem5.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(260, 22);
            this.toolStripMenuItem5.Text = "Отображать нумерацию тайлов";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.toolStripMenuItem5_Click);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.toolStripStatusLabel1.ForeColor = System.Drawing.Color.Maroon;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(109, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.BackColor = System.Drawing.Color.White;
            this.toolStripStatusLabel2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(109, 17);
            this.toolStripStatusLabel2.Text = "toolStripStatusLabel2";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.BackColor = System.Drawing.Color.White;
            this.toolStripStatusLabel3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(109, 17);
            this.toolStripStatusLabel3.Text = "toolStripStatusLabel3";
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(109, 17);
            this.toolStripStatusLabel4.Text = "toolStripStatusLabel4";
            // 
            // toolStripStatusLabel5
            // 
            this.toolStripStatusLabel5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.toolStripStatusLabel5.Name = "toolStripStatusLabel5";
            this.toolStripStatusLabel5.Size = new System.Drawing.Size(109, 17);
            this.toolStripStatusLabel5.Text = "toolStripStatusLabel5";
            // 
            // map
            // 
            this.map.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.map.CenterDegrees = ((System.Drawing.PointF)(resources.GetObject("map.CenterDegrees")));
            this.map.CenterDegreesLat = 52.590535060652833;
            this.map.CenterDegreesLon = 39.549407958984375;
            this.map.CenterDegreesX = 39.549407958984375;
            this.map.CenterDegreesY = 52.590535060652833;
            this.map.CenterMeters = ((System.Drawing.PointF)(resources.GetObject("map.CenterMeters")));
            this.map.CenterMetersX = 4402620;
            this.map.CenterMetersY = 6907614;
            this.map.CenterPixels = new System.Drawing.Point(159871, 85887);
            this.map.CenterPixelsX = 159871;
            this.map.CenterPixelsY = 85887;
            this.map.DefaultMapCursor = System.Windows.Forms.Cursors.Arrow;
            this.map.Dock = System.Windows.Forms.DockStyle.Fill;
            this.map.DrawMap = false;
            this.map.DrawMapTestVectorData = false;
            this.map.DrawTilesBorder = true;
            this.map.DrawTilesXYZ = true;
            this.map.DrawVector = true;
            this.map.ImageSourceProjection = NaviMapNet.NaviMapNetViewer.ImageSourceProjections.EPSG4326;
            this.map.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.Custom_LocalFiles;
            this.map.ImageSourceType = NaviMapNet.NaviMapNetViewer.ImageSourceTypes.tiles;
            this.map.ImageSourceUrl = "C:\\Downloads\\CD-REC\\_MAPNIK_TILE_RENDERER\\_TILES\\L{l}\\R{r}\\C{c}.png";
            this.map.Location = new System.Drawing.Point(0, 0);
            this.map.MapBackgroundColor = System.Drawing.Color.White;
            this.map.MapMaxZoom = ((byte)(18));
            this.map.MapMinMovement = ((byte)(35));
            this.map.MapMinZoom = ((byte)(2));
            this.map.MapTool = NaviMapNet.NaviMapNetViewer.MapTools.mtShift;
            this.map.Name = "map";
            this.map.NotFoundTileColor = System.Drawing.Color.Transparent;
            this.map.ShowCross = false;
            this.map.ShowMapTypes = false;
            this.map.ShowScale = true;
            this.map.ShowZooms = true;
            this.map.Size = new System.Drawing.Size(950, 605);
            this.map.TabIndex = 2;
            this.map.TilesMaxZoom = ((byte)(21));
            this.map.TilesMinZoom = ((byte)(1));
            this.map.TilesRenderingZoneSize = ((short)(0));
            this.map.WebRequestTimeout = 100000;
            this.map.ZoomID = ((byte)(10));
            this.map.MouseMove += new System.Windows.Forms.MouseEventHandler(this.map_MouseMove);
            // 
            // BrowseTileFolder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(950, 627);
            this.Controls.Add(this.map);
            this.Controls.Add(this.statusStrip1);
            this.Name = "BrowseTileFolder";
            this.Text = "Просмотр тайлов";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BrowseTileFolder_FormClosed);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public NaviMapNet.NaviMapNetViewer map;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel5;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        public System.Windows.Forms.ToolStripComboBox toolStripComboBox1;

    }
}