namespace WindowsApplication1
{
    partial class MFrom
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
            this.button1 = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.rBox = new System.Windows.Forms.CheckBox();
            this.lBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Clear";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericUpDown1.Location = new System.Drawing.Point(82, 3);
            this.numericUpDown1.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(62, 20);
            this.numericUpDown1.TabIndex = 1;
            this.numericUpDown1.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // rBox
            // 
            this.rBox.AutoSize = true;
            this.rBox.Checked = true;
            this.rBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rBox.Location = new System.Drawing.Point(150, -1);
            this.rBox.Name = "rBox";
            this.rBox.Size = new System.Drawing.Size(46, 17);
            this.rBox.TabIndex = 2;
            this.rBox.Text = "right";
            this.rBox.UseVisualStyleBackColor = true;
            this.rBox.CheckedChanged += new System.EventHandler(this.rBox_CheckedChanged);
            // 
            // lBox
            // 
            this.lBox.AutoSize = true;
            this.lBox.Checked = true;
            this.lBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.lBox.Location = new System.Drawing.Point(150, 14);
            this.lBox.Name = "lBox";
            this.lBox.Size = new System.Drawing.Size(40, 17);
            this.lBox.TabIndex = 3;
            this.lBox.Text = "left";
            this.lBox.UseVisualStyleBackColor = true;
            this.lBox.CheckedChanged += new System.EventHandler(this.lBox_CheckedChanged);
            // 
            // MFrom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(958, 567);
            this.Controls.Add(this.lBox);
            this.Controls.Add(this.rBox);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.button1);
            this.Name = "MFrom";
            this.Text = "PolyLineBufferCreator";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MFrom_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseClick);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.CheckBox rBox;
        private System.Windows.Forms.CheckBox lBox;
    }
}

