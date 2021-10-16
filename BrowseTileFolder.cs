using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MapnikTileRenderer
{
    public partial class BrowseTileFolder : Form
    {
        public string current_project_dir = "";

        public BrowseTileFolder()
        {
            InitializeComponent();
            map.OnMapUpdate += new NaviMapNet.NaviMapNetViewer.MapEvent(MapUpdate);
        }

        private void BrowseTileFolder_FormClosed(object sender, FormClosedEventArgs e)
        {
            Dispose();
        }

        private void MapUpdate()
        {
            toolStripStatusLabel1.Text = "Last Requested File: " + map.LastRequestedFile;
            toolStripStatusLabel2.Text = map.CenterDegreesLat.ToString().Replace(",", ".");
            toolStripStatusLabel3.Text = map.CenterDegreesLon.ToString().Replace(",", ".");
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            PointF m = map.MousePositionDegrees;
            toolStripStatusLabel4.Text = m.Y.ToString().Replace(",", ".");
            toolStripStatusLabel5.Text = m.X.ToString().Replace(",", ".");
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            map.DrawTilesBorder = !map.DrawTilesBorder;
            toolStripMenuItem4.Checked = map.DrawTilesBorder;
            map.ReloadMap();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            map.DrawTilesXYZ = !map.DrawTilesXYZ;
            toolStripMenuItem5.Checked = map.DrawTilesXYZ;
            map.ReloadMap();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            toolStripMenuItem1.Checked = true;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem2.Checked = false;

            map.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.Custom_LocalFiles;
            map.ImageSourceUrl = current_project_dir;
            map.ReloadMap();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            toolStripMenuItem1.Checked = false;
            toolStripMenuItem3.Checked = true;
            toolStripMenuItem2.Checked = false;

            map.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.Navicom_Tiles;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            toolStripMenuItem1.Checked = false;
            toolStripMenuItem3.Checked = false;
            toolStripMenuItem2.Checked = true;

            map.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.OSM_Mapnik;
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            map.TilesRenderingZoneSize = ((short)(toolStripComboBox1.SelectedIndex+1));
            if(map.DrawTilesBorder) map.ReloadMap();
        }
    }
}