namespace NaviMapNet
{
    partial class NaviMapNetViewer
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.imagePanel = new System.Windows.Forms.Panel();
            this.Selabel = new System.Windows.Forms.Label();
            this.mapImage = new System.Windows.Forms.PictureBox();
            this.selMapType = new System.Windows.Forms.ComboBox();
            this.defMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setMapXYZToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.getCursorLatLonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAddit = new System.Windows.Forms.ToolStripMenuItem();
            this.openExternalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.selBoxInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.save2Shp = new System.Windows.Forms.ToolStripMenuItem();
            this.Zoom2Sel = new System.Windows.Forms.ToolStripMenuItem();
            this.hideSelBox = new System.Windows.Forms.ToolStripMenuItem();
            this.HSB = new System.Windows.Forms.ToolStripSeparator();
            this.ShowNaviChangeBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.mttShift = new System.Windows.Forms.ToolStripMenuItem();
            this.mttZoomIn = new System.Windows.Forms.ToolStripMenuItem();
            this.mttZoomOut = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.ShowZoomBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowZoomsBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowCrossBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowTileBorderBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowXYZBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.ShowUseCacheBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.openCacheFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.saveMapImgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveBoundsShapeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.ShowReloadTilesBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.labelLoading = new System.Windows.Forms.Label();
            this.scaleImage = new System.Windows.Forms.PictureBox();
            this.crossImage = new System.Windows.Forms.PictureBox();
            this.zoomLevels = new System.Windows.Forms.PictureBox();
            this.imagePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mapImage)).BeginInit();
            this.defMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scaleImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.crossImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.zoomLevels)).BeginInit();
            this.SuspendLayout();
            // 
            // imagePanel
            // 
            this.imagePanel.BackColor = System.Drawing.Color.White;
            this.imagePanel.Controls.Add(this.Selabel);
            this.imagePanel.Controls.Add(this.mapImage);
            this.imagePanel.Location = new System.Drawing.Point(37, 32);
            this.imagePanel.Name = "imagePanel";
            this.imagePanel.Size = new System.Drawing.Size(522, 367);
            this.imagePanel.TabIndex = 3;
            // 
            // Selabel
            // 
            this.Selabel.BackColor = System.Drawing.Color.Transparent;
            this.Selabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Selabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Selabel.Location = new System.Drawing.Point(263, 120);
            this.Selabel.Name = "Selabel";
            this.Selabel.Size = new System.Drawing.Size(100, 23);
            this.Selabel.TabIndex = 6;
            this.Selabel.Visible = false;
            this.Selabel.Paint += new System.Windows.Forms.PaintEventHandler(this.Selabel_Paint);
            // 
            // mapImage
            // 
            this.mapImage.ErrorImage = null;
            this.mapImage.Location = new System.Drawing.Point(-34, 76);
            this.mapImage.Name = "mapImage";
            this.mapImage.Size = new System.Drawing.Size(243, 132);
            this.mapImage.TabIndex = 5;
            this.mapImage.TabStop = false;
            this.mapImage.MouseLeave += new System.EventHandler(this.mapImage_MouseLeave);
            this.mapImage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mapImage_MouseMove);
            this.mapImage.Click += new System.EventHandler(this.mapImage_Click);
            this.mapImage.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mapImage_MouseDoubleClick);
            this.mapImage.MouseClick += new System.Windows.Forms.MouseEventHandler(this.mapImage_MouseClick);
            this.mapImage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mapImage_MouseDown);
            this.mapImage.MouseHover += new System.EventHandler(this.mapImage_MouseHover);
            this.mapImage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mapImage_MouseUp);
            this.mapImage.MouseEnter += new System.EventHandler(this.mapImage_MouseEnter);
            // 
            // selMapType
            // 
            this.selMapType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selMapType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.selMapType.FormattingEnabled = true;
            this.selMapType.Location = new System.Drawing.Point(3, 3);
            this.selMapType.Name = "selMapType";
            this.selMapType.Size = new System.Drawing.Size(172, 21);
            this.selMapType.TabIndex = 10;
            this.selMapType.Visible = false;
            this.selMapType.SelectedIndexChanged += new System.EventHandler(this.selMapType_SelectedIndexChanged);
            // 
            // defMenu
            // 
            this.defMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnCopyToolStripMenuItem,
            this.setMapXYZToolStripMenuItem,
            this.getCursorLatLonToolStripMenuItem,
            this.showAddit,
            this.openExternalToolStripMenuItem,
            this.toolStripMenuItem1,
            this.selBoxInfo,
            this.save2Shp,
            this.Zoom2Sel,
            this.hideSelBox,
            this.HSB,
            this.ShowNaviChangeBtn,
            this.toolStripMenuItem5,
            this.ShowZoomBtn,
            this.ShowZoomsBtn,
            this.ShowCrossBtn,
            this.ShowTileBorderBtn,
            this.ShowXYZBtn,
            this.toolStripMenuItem3,
            this.ShowUseCacheBtn,
            this.openCacheFolderToolStripMenuItem,
            this.toolStripMenuItem4,
            this.saveMapImgToolStripMenuItem,
            this.saveBoundsShapeToolStripMenuItem,
            this.toolStripMenuItem2,
            this.ShowReloadTilesBtn});
            this.defMenu.Name = "defMenu";
            this.defMenu.Size = new System.Drawing.Size(328, 480);
            this.defMenu.Opening += new System.ComponentModel.CancelEventHandler(this.defMenu_Opening);
            // 
            // btnCopyToolStripMenuItem
            // 
            this.btnCopyToolStripMenuItem.Name = "btnCopyToolStripMenuItem";
            this.btnCopyToolStripMenuItem.Size = new System.Drawing.Size(327, 22);
            this.btnCopyToolStripMenuItem.Text = "Установить координаты центра карты...";
            this.btnCopyToolStripMenuItem.Click += new System.EventHandler(this.btnCopyToolStripMenuItem_Click);
            // 
            // setMapXYZToolStripMenuItem
            // 
            this.setMapXYZToolStripMenuItem.Name = "setMapXYZToolStripMenuItem";
            this.setMapXYZToolStripMenuItem.Size = new System.Drawing.Size(327, 22);
            this.setMapXYZToolStripMenuItem.Text = "Установить тайл центра карты...";
            this.setMapXYZToolStripMenuItem.Click += new System.EventHandler(this.setMapXYZToolStripMenuItem_Click);
            // 
            // getCursorLatLonToolStripMenuItem
            // 
            this.getCursorLatLonToolStripMenuItem.Name = "getCursorLatLonToolStripMenuItem";
            this.getCursorLatLonToolStripMenuItem.Size = new System.Drawing.Size(327, 22);
            this.getCursorLatLonToolStripMenuItem.Text = "Установить координаты курсора...";
            this.getCursorLatLonToolStripMenuItem.Click += new System.EventHandler(this.getCursorLatLonToolStripMenuItem_Click);
            // 
            // showAddit
            // 
            this.showAddit.Name = "showAddit";
            this.showAddit.Size = new System.Drawing.Size(327, 22);
            this.showAddit.Text = "Вспомогательная информация по точке...";
            this.showAddit.Click += new System.EventHandler(this.showAddit_Click);
            // 
            // openExternalToolStripMenuItem
            // 
            this.openExternalToolStripMenuItem.Name = "openExternalToolStripMenuItem";
            this.openExternalToolStripMenuItem.Size = new System.Drawing.Size(327, 22);
            this.openExternalToolStripMenuItem.Text = "Открыть тайл во внешнем просмоторщике...";
            this.openExternalToolStripMenuItem.Click += new System.EventHandler(this.openExternalToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(324, 6);
            // 
            // selBoxInfo
            // 
            this.selBoxInfo.Name = "selBoxInfo";
            this.selBoxInfo.Size = new System.Drawing.Size(327, 22);
            this.selBoxInfo.Text = "Информация по области выделения...";
            this.selBoxInfo.Visible = false;
            this.selBoxInfo.Click += new System.EventHandler(this.selBoxInfo_Click);
            // 
            // save2Shp
            // 
            this.save2Shp.Name = "save2Shp";
            this.save2Shp.Size = new System.Drawing.Size(327, 22);
            this.save2Shp.Text = "Сохранить область выделения в файл...";
            this.save2Shp.Visible = false;
            this.save2Shp.Click += new System.EventHandler(this.save2Shp_Click);
            // 
            // Zoom2Sel
            // 
            this.Zoom2Sel.Name = "Zoom2Sel";
            this.Zoom2Sel.Size = new System.Drawing.Size(327, 22);
            this.Zoom2Sel.Text = "Увеличить зону выделения и в центр";
            this.Zoom2Sel.Visible = false;
            this.Zoom2Sel.Click += new System.EventHandler(this.Zoom2Sel_Click);
            // 
            // hideSelBox
            // 
            this.hideSelBox.Name = "hideSelBox";
            this.hideSelBox.Size = new System.Drawing.Size(327, 22);
            this.hideSelBox.Text = "Убрать область выделения";
            this.hideSelBox.Visible = false;
            this.hideSelBox.Click += new System.EventHandler(this.hideSelBox_Click);
            // 
            // HSB
            // 
            this.HSB.Name = "HSB";
            this.HSB.Size = new System.Drawing.Size(324, 6);
            this.HSB.Visible = false;
            // 
            // ShowNaviChangeBtn
            // 
            this.ShowNaviChangeBtn.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mttShift,
            this.mttZoomIn,
            this.mttZoomOut});
            this.ShowNaviChangeBtn.Name = "ShowNaviChangeBtn";
            this.ShowNaviChangeBtn.Size = new System.Drawing.Size(327, 22);
            this.ShowNaviChangeBtn.Text = "Режим навигации по карте";
            // 
            // mttShift
            // 
            this.mttShift.Checked = true;
            this.mttShift.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mttShift.Name = "mttShift";
            this.mttShift.Size = new System.Drawing.Size(225, 22);
            this.mttShift.Text = "Сдвиг (стандарт)";
            this.mttShift.Click += new System.EventHandler(this.mttShift_Click);
            // 
            // mttZoomIn
            // 
            this.mttZoomIn.Name = "mttZoomIn";
            this.mttZoomIn.Size = new System.Drawing.Size(225, 22);
            this.mttZoomIn.Text = "Увеличение (приближение)";
            this.mttZoomIn.Click += new System.EventHandler(this.mttZoomIn_Click);
            // 
            // mttZoomOut
            // 
            this.mttZoomOut.Name = "mttZoomOut";
            this.mttZoomOut.Size = new System.Drawing.Size(225, 22);
            this.mttZoomOut.Text = "Уменьшение (отдаление)";
            this.mttZoomOut.Click += new System.EventHandler(this.mttZoomOut_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(324, 6);
            // 
            // ShowZoomBtn
            // 
            this.ShowZoomBtn.Checked = true;
            this.ShowZoomBtn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowZoomBtn.Name = "ShowZoomBtn";
            this.ShowZoomBtn.Size = new System.Drawing.Size(327, 22);
            this.ShowZoomBtn.Text = "Отображать шкалу масштаба";
            this.ShowZoomBtn.Click += new System.EventHandler(this.ShowZoomBtn_Click);
            // 
            // ShowZoomsBtn
            // 
            this.ShowZoomsBtn.Checked = true;
            this.ShowZoomsBtn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ShowZoomsBtn.Name = "ShowZoomsBtn";
            this.ShowZoomsBtn.Size = new System.Drawing.Size(327, 22);
            this.ShowZoomsBtn.Text = "Отображать масштабные уровни";
            this.ShowZoomsBtn.Click += new System.EventHandler(this.showZoomsBtn_Click);
            // 
            // ShowCrossBtn
            // 
            this.ShowCrossBtn.Name = "ShowCrossBtn";
            this.ShowCrossBtn.Size = new System.Drawing.Size(327, 22);
            this.ShowCrossBtn.Text = "Отображать перекрестие в центре карты";
            this.ShowCrossBtn.Click += new System.EventHandler(this.showCross_Click);
            // 
            // ShowTileBorderBtn
            // 
            this.ShowTileBorderBtn.Name = "ShowTileBorderBtn";
            this.ShowTileBorderBtn.Size = new System.Drawing.Size(327, 22);
            this.ShowTileBorderBtn.Text = "Отображать границы тайлов";
            this.ShowTileBorderBtn.Click += new System.EventHandler(this.ShowTileBorder_Click);
            // 
            // ShowXYZBtn
            // 
            this.ShowXYZBtn.Name = "ShowXYZBtn";
            this.ShowXYZBtn.Size = new System.Drawing.Size(327, 22);
            this.ShowXYZBtn.Text = "Отображать нумерацию тайлов";
            this.ShowXYZBtn.Click += new System.EventHandler(this.showXYZBtn_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(324, 6);
            // 
            // ShowUseCacheBtn
            // 
            this.ShowUseCacheBtn.Name = "ShowUseCacheBtn";
            this.ShowUseCacheBtn.Size = new System.Drawing.Size(327, 22);
            this.ShowUseCacheBtn.Text = "Использовать встроенное кэширование тайлов";
            this.ShowUseCacheBtn.Click += new System.EventHandler(this.UseCacheBtn_Click);
            // 
            // openCacheFolderToolStripMenuItem
            // 
            this.openCacheFolderToolStripMenuItem.Name = "openCacheFolderToolStripMenuItem";
            this.openCacheFolderToolStripMenuItem.Size = new System.Drawing.Size(327, 22);
            this.openCacheFolderToolStripMenuItem.Text = "Открыть папку с кэшем...";
            this.openCacheFolderToolStripMenuItem.Click += new System.EventHandler(this.openCacheFolderToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(324, 6);
            // 
            // saveMapImgToolStripMenuItem
            // 
            this.saveMapImgToolStripMenuItem.Name = "saveMapImgToolStripMenuItem";
            this.saveMapImgToolStripMenuItem.Size = new System.Drawing.Size(327, 22);
            this.saveMapImgToolStripMenuItem.Text = "Сохранить карту как картинку...";
            this.saveMapImgToolStripMenuItem.Click += new System.EventHandler(this.saveMapImgToolStripMenuItem_Click);
            // 
            // saveBoundsShapeToolStripMenuItem
            // 
            this.saveBoundsShapeToolStripMenuItem.Name = "saveBoundsShapeToolStripMenuItem";
            this.saveBoundsShapeToolStripMenuItem.Size = new System.Drawing.Size(327, 22);
            this.saveBoundsShapeToolStripMenuItem.Text = "Сохранить область карты в файл...";
            this.saveBoundsShapeToolStripMenuItem.Click += new System.EventHandler(this.saveBoundsShapeToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(324, 6);
            // 
            // ShowReloadTilesBtn
            // 
            this.ShowReloadTilesBtn.Name = "ShowReloadTilesBtn";
            this.ShowReloadTilesBtn.Size = new System.Drawing.Size(327, 22);
            this.ShowReloadTilesBtn.Text = "Перегрузить тайлы";
            this.ShowReloadTilesBtn.Click += new System.EventHandler(this.reloadTilesToolStripMenuItem_Click);
            // 
            // labelLoading
            // 
            this.labelLoading.BackColor = System.Drawing.Color.Transparent;
            this.labelLoading.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelLoading.ForeColor = System.Drawing.Color.DarkRed;
            this.labelLoading.Image = global::MergedTilesSimpleViewer.Properties.Resources.Globe;
            this.labelLoading.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelLoading.Location = new System.Drawing.Point(-2, 583);
            this.labelLoading.Name = "labelLoading";
            this.labelLoading.Size = new System.Drawing.Size(18, 18);
            this.labelLoading.TabIndex = 11;
            this.labelLoading.Text = "      ";
            this.labelLoading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelLoading.Visible = false;
            // 
            // scaleImage
            // 
            this.scaleImage.BackColor = System.Drawing.Color.Transparent;
            this.scaleImage.Location = new System.Drawing.Point(252, 489);
            this.scaleImage.Name = "scaleImage";
            this.scaleImage.Size = new System.Drawing.Size(264, 50);
            this.scaleImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.scaleImage.TabIndex = 9;
            this.scaleImage.TabStop = false;
            // 
            // crossImage
            // 
            this.crossImage.BackColor = System.Drawing.Color.Transparent;
            this.crossImage.Location = new System.Drawing.Point(347, 424);
            this.crossImage.Name = "crossImage";
            this.crossImage.Size = new System.Drawing.Size(100, 50);
            this.crossImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.crossImage.TabIndex = 8;
            this.crossImage.TabStop = false;
            this.crossImage.Visible = false;
            // 
            // zoomLevels
            // 
            this.zoomLevels.Location = new System.Drawing.Point(547, 302);
            this.zoomLevels.Name = "zoomLevels";
            this.zoomLevels.Size = new System.Drawing.Size(27, 237);
            this.zoomLevels.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.zoomLevels.TabIndex = 7;
            this.zoomLevels.TabStop = false;
            this.zoomLevels.MouseLeave += new System.EventHandler(this.zoomLevels_MouseLeave);
            this.zoomLevels.MouseMove += new System.Windows.Forms.MouseEventHandler(this.zoomLevels_MouseMove);
            this.zoomLevels.Click += new System.EventHandler(this.zoomLevels_Click);
            this.zoomLevels.Paint += new System.Windows.Forms.PaintEventHandler(this.zoomLevels_Paint);
            this.zoomLevels.MouseEnter += new System.EventHandler(this.zoomLevels_MouseEnter);
            // 
            // NaviMapNetViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelLoading);
            this.Controls.Add(this.selMapType);
            this.Controls.Add(this.scaleImage);
            this.Controls.Add(this.crossImage);
            this.Controls.Add(this.zoomLevels);
            this.Controls.Add(this.imagePanel);
            this.Name = "NaviMapNetViewer";
            this.Size = new System.Drawing.Size(600, 600);
            this.Load += new System.EventHandler(this.NaviMapNetViewer_Load);
            this.MouseLeave += new System.EventHandler(this.NaviMapNetViewer_MouseLeave);
            this.Resize += new System.EventHandler(this.onControlResize);
            this.MouseEnter += new System.EventHandler(this.NaviMapNetViewer_MouseEnter);
            this.imagePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mapImage)).EndInit();
            this.defMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scaleImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.crossImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.zoomLevels)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel imagePanel;
        private System.Windows.Forms.PictureBox mapImage;
        private System.Windows.Forms.PictureBox zoomLevels;
        private System.Windows.Forms.PictureBox crossImage;
        private System.Windows.Forms.PictureBox scaleImage;
        private System.Windows.Forms.ComboBox selMapType;
        private System.Windows.Forms.ContextMenuStrip defMenu;
        private System.Windows.Forms.ToolStripMenuItem btnCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mttShift;
        private System.Windows.Forms.ToolStripMenuItem mttZoomIn;
        private System.Windows.Forms.ToolStripMenuItem mttZoomOut;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem getCursorLatLonToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem showAddit;
        public System.Windows.Forms.ToolStripMenuItem ShowCrossBtn;
        public System.Windows.Forms.ToolStripMenuItem ShowZoomBtn;
        public System.Windows.Forms.ToolStripMenuItem ShowZoomsBtn;
        public System.Windows.Forms.ToolStripMenuItem ShowTileBorderBtn;
        public System.Windows.Forms.ToolStripMenuItem ShowXYZBtn;
        public System.Windows.Forms.ToolStripMenuItem ShowReloadTilesBtn;
        public System.Windows.Forms.ToolStripMenuItem ShowUseCacheBtn;
        public System.Windows.Forms.ToolStripMenuItem ShowNaviChangeBtn;
        private System.Windows.Forms.Label Selabel;
        private System.Windows.Forms.ToolStripMenuItem hideSelBox;
        private System.Windows.Forms.ToolStripSeparator HSB;
        private System.Windows.Forms.ToolStripMenuItem selBoxInfo;
        private System.Windows.Forms.ToolStripMenuItem save2Shp;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem saveBoundsShapeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveMapImgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openCacheFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setMapXYZToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openExternalToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem Zoom2Sel;
        private System.Windows.Forms.Label labelLoading;

    }
}
