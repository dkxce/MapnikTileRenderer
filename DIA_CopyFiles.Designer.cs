namespace CopyFiles
{
    partial class DIA_CopyFiles
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
            this.But_Cancel = new System.Windows.Forms.Button();
            this.Prog_CurrentFile = new System.Windows.Forms.ProgressBar();
            this.Prog_TotalFiles = new System.Windows.Forms.ProgressBar();
            this.Lab_TotalFiles = new System.Windows.Forms.Label();
            this.Lab_CurrentFile = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // But_Cancel
            // 
            this.But_Cancel.Location = new System.Drawing.Point(139, 104);
            this.But_Cancel.Name = "But_Cancel";
            this.But_Cancel.Size = new System.Drawing.Size(118, 23);
            this.But_Cancel.TabIndex = 8;
            this.But_Cancel.Text = "Отмена";
            this.But_Cancel.UseVisualStyleBackColor = true;
            this.But_Cancel.Click += new System.EventHandler(this.But_Cancel_Click);
            // 
            // Prog_CurrentFile
            // 
            this.Prog_CurrentFile.Location = new System.Drawing.Point(12, 75);
            this.Prog_CurrentFile.Name = "Prog_CurrentFile";
            this.Prog_CurrentFile.Size = new System.Drawing.Size(361, 23);
            this.Prog_CurrentFile.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.Prog_CurrentFile.TabIndex = 7;
            // 
            // Prog_TotalFiles
            // 
            this.Prog_TotalFiles.Location = new System.Drawing.Point(12, 24);
            this.Prog_TotalFiles.Name = "Prog_TotalFiles";
            this.Prog_TotalFiles.Size = new System.Drawing.Size(361, 23);
            this.Prog_TotalFiles.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.Prog_TotalFiles.TabIndex = 6;
            // 
            // Lab_TotalFiles
            // 
            this.Lab_TotalFiles.AutoSize = true;
            this.Lab_TotalFiles.Location = new System.Drawing.Point(12, 8);
            this.Lab_TotalFiles.Name = "Lab_TotalFiles";
            this.Lab_TotalFiles.Size = new System.Drawing.Size(78, 13);
            this.Lab_TotalFiles.TabIndex = 9;
            this.Lab_TotalFiles.Text = "Всего файлов";
            // 
            // Lab_CurrentFile
            // 
            this.Lab_CurrentFile.AutoSize = true;
            this.Lab_CurrentFile.Location = new System.Drawing.Point(12, 59);
            this.Lab_CurrentFile.Name = "Lab_CurrentFile";
            this.Lab_CurrentFile.Size = new System.Drawing.Size(81, 13);
            this.Lab_CurrentFile.TabIndex = 10;
            this.Lab_CurrentFile.Text = "Текущий файл";
            // 
            // DIA_CopyFiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 133);
            this.Controls.Add(this.Lab_CurrentFile);
            this.Controls.Add(this.Lab_TotalFiles);
            this.Controls.Add(this.But_Cancel);
            this.Controls.Add(this.Prog_CurrentFile);
            this.Controls.Add(this.Prog_TotalFiles);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DIA_CopyFiles";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Копирование файлов";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button But_Cancel;
        private System.Windows.Forms.ProgressBar Prog_CurrentFile;
        private System.Windows.Forms.ProgressBar Prog_TotalFiles;
        private System.Windows.Forms.Label Lab_TotalFiles;
        private System.Windows.Forms.Label Lab_CurrentFile;
    }
}