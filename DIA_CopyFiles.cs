using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CopyFiles
{

    public partial class DIA_CopyFiles : Form, ICopyFilesDiag
    {

        // Properties
        public System.ComponentModel.ISynchronizeInvoke syncObj;
        public System.ComponentModel.ISynchronizeInvoke SynchronizationObject { get {return syncObj;}  set {syncObj = value;} }

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        // Constructors
        public DIA_CopyFiles(Form parent)
        {
            SetParent(this.Handle, parent.Handle);
            InitializeComponent();
        }

        // Methods
        public void update(Int32 totalFiles, Int32 copiedFiles, Int64 totalBytes, Int64 copiedBytes, String currentFilename)
        {
            Prog_TotalFiles.Maximum = totalFiles;
            Prog_TotalFiles.Value = copiedFiles;
            Prog_CurrentFile.Maximum = 100;
            if (totalBytes != 0)
            {
                Prog_CurrentFile.Value = Convert.ToInt32((100f / (totalBytes / 1024f)) * (copiedBytes / 1024f));
            }

            Lab_TotalFiles.Text = "Всего файлов (" + copiedFiles + "/" + totalFiles + ")";
            if (currentFilename.Length > 50) currentFilename = "..."+currentFilename.Substring(currentFilename.Length - 50);
            Lab_CurrentFile.Text = currentFilename;
            this.Refresh();
            Application.DoEvents();
        }
        private void But_Cancel_Click(object sender, EventArgs e)
        {
            RaiseCancel();
        }
        private void DIA_CopyFiles_Closed(object sender, System.EventArgs e)
        {
            RaiseCancel();
        }
        private void RaiseCancel()
        {
            if (EN_cancelCopy != null)
            {
                EN_cancelCopy();
            }
        }

        //Events
        public event CopyFiles.DEL_cancelCopy EN_cancelCopy;


    }

}
