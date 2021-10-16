using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace WindowsApplication1
{
    public partial class MFrom : Form
    {
        public List<PointF> line = new List<PointF>();
        public PolyLineBuffer.PolyLineBufferCreator.PolyResult buffer = null;
        public PointF pin;

        public MFrom()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            line.Clear();
            buffer = new PolyLineBuffer.PolyLineBufferCreator.PolyResult();
            Repaint();
        }

        private void Repaint()
        {
            Graphics g = Graphics.FromHwnd(this.Handle);
            g.Clear(Color.White);

            PointF[] ln = line.ToArray();
            if (ln.Length > 0)
                for (int i = 0; i < ln.Length; i++)
                    ln[i].Y = (this.Height - ln[i].Y);

            if (line.Count > 1)
                buffer = PolyLineBuffer.PolyLineBufferCreator.GetLineBufferPolygon(line.ToArray(), (float)numericUpDown1.Value, rBox.Checked, lBox.Checked, PolyLineBuffer.PolyLineBufferCreator.SampleDistFunc);

            if (buffer != null)
            {
                if (buffer.segments.Count > 0)
                    for (int i = 0; i < buffer.segments.Count; i++)
                    {
                        List<PointF> poly2Out = new List<PointF>();
                        for (int j = 0; j < buffer.segments[i].Length; j++)
                            poly2Out.Add(new PointF(buffer.segments[i][j].X, this.Height - buffer.segments[i][j].Y));
                        g.DrawPolygon(new Pen(new SolidBrush(Color.Lime), 4), poly2Out.ToArray());
                    };

                if ((buffer.polygon != null) && (buffer.polygon.Length > 1))
                {
                    List<PointF> poly2Out = new List<PointF>();
                    for (int i = 0; i < buffer.polygon.Length; i++)
                        poly2Out.Add(new PointF(buffer.polygon[i].X, this.Height - buffer.polygon[i].Y));
                    g.DrawPolygon(new Pen(new SolidBrush(Color.Navy), 2), poly2Out.ToArray());
                    g.FillPolygon(new SolidBrush(Color.FromArgb(25, Color.Navy)), poly2Out.ToArray());
                };
            };

            if (ln.Length > 0)
            {
                for (int i = 0; i < ln.Length; i++)
                    g.DrawEllipse(new Pen(new SolidBrush(Color.Maroon), 2), ln[i].X - 1, ln[i].Y - 1, 2, 2);
                if (ln.Length > 1)
                    g.DrawLines(new Pen(new SolidBrush(Color.Red), 2), ln);
            };

            if (pin != null)
            {
                g.DrawEllipse(new Pen(new SolidBrush(Color.Magenta), 2), pin.X - 2, (this.Height - pin.Y) - 2, 5, 5);
            };

            g.Dispose();
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                line.Add(new PointF(e.X, this.Height - e.Y));                
                Repaint();
            };
            if (e.Button == MouseButtons.Right)
            {
                pin = new PointF(e.X, this.Height - e.Y);    
                if (buffer != null)
                    Text = buffer.PointIn(pin) ? " Inside" : " Outside";
                Repaint();
            };
        }

        private void MFrom_Paint(object sender, PaintEventArgs e)
        {
            Repaint();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Repaint();
        }

        private void rBox_CheckedChanged(object sender, EventArgs e)
        {
            Repaint();
        }

        private void lBox_CheckedChanged(object sender, EventArgs e)
        {
            Repaint();
        }
    }    
}