namespace MapnikTileRenderer
{
    partial class StopProcessDialog
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
            this.stopNormal = new System.Windows.Forms.RadioButton();
            this.stopKill = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // stopNormal
            // 
            this.stopNormal.AutoSize = true;
            this.stopNormal.Location = new System.Drawing.Point(28, 38);
            this.stopNormal.Name = "stopNormal";
            this.stopNormal.Size = new System.Drawing.Size(514, 17);
            this.stopNormal.TabIndex = 0;
            this.stopNormal.TabStop = true;
            this.stopNormal.Text = "Нормально - Подождать пока каждый поток сохранит тайлы из текущей обрабатываемой " +
                "зоны";
            this.stopNormal.UseVisualStyleBackColor = true;
            // 
            // stopKill
            // 
            this.stopKill.AutoSize = true;
            this.stopKill.Location = new System.Drawing.Point(28, 57);
            this.stopKill.Name = "stopKill";
            this.stopKill.Size = new System.Drawing.Size(516, 17);
            this.stopKill.TabIndex = 1;
            this.stopKill.TabStop = true;
            this.stopKill.Text = "Принудительно  - Остановить все потоки, не сохраняя тайлы из текущей обрабатываем" +
                "ой зоны";
            this.stopKill.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(202, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(179, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Как вы хотите прервать нарезку?";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(205, 90);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Прервать";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(298, 90);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Отмена";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // StopProcessDialog
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(578, 125);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.stopKill);
            this.Controls.Add(this.stopNormal);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StopProcessDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Прервать нарезку";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.RadioButton stopKill;
        public System.Windows.Forms.RadioButton stopNormal;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}