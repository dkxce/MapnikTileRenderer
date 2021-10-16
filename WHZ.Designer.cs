namespace MapnikTileRenderer
{
    partial class WHZ
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.iWidth = new System.Windows.Forms.NumericUpDown();
            this.iHeight = new System.Windows.Forms.NumericUpDown();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.iWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iHeight)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ширина карты:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Высота карты:";
            // 
            // iWidth
            // 
            this.iWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.iWidth.Location = new System.Drawing.Point(118, 22);
            this.iWidth.Maximum = new decimal(new int[] {
            6000,
            0,
            0,
            0});
            this.iWidth.Minimum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.iWidth.Name = "iWidth";
            this.iWidth.Size = new System.Drawing.Size(120, 20);
            this.iWidth.TabIndex = 3;
            this.iWidth.Value = new decimal(new int[] {
            1600,
            0,
            0,
            0});
            // 
            // iHeight
            // 
            this.iHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.iHeight.Location = new System.Drawing.Point(118, 54);
            this.iHeight.Maximum = new decimal(new int[] {
            6000,
            0,
            0,
            0});
            this.iHeight.Minimum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.iHeight.Name = "iHeight";
            this.iHeight.Size = new System.Drawing.Size(120, 20);
            this.iHeight.TabIndex = 4;
            this.iHeight.Value = new decimal(new int[] {
            1600,
            0,
            0,
            0});
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(59, 90);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(141, 90);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "Отмена";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // WHZ
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(272, 128);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.iHeight);
            this.Controls.Add(this.iWidth);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WHZ";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Укажите параметры";
            ((System.ComponentModel.ISupportInitialize)(this.iWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iHeight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.NumericUpDown iWidth;
        public System.Windows.Forms.NumericUpDown iHeight;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}