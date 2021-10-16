namespace MapnikTileRenderer
{
    partial class CreateCMD
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateCMD));
            this.label1 = new System.Windows.Forms.Label();
            this.tPort = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.tTask = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tServerAs = new System.Windows.Forms.ComboBox();
            this.tDumpLabel = new System.Windows.Forms.Label();
            this.tDump = new System.Windows.Forms.ComboBox();
            this.tHolesLabel = new System.Windows.Forms.Label();
            this.tHoles = new System.Windows.Forms.TextBox();
            this.tHolesB = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tSClose = new System.Windows.Forms.ComboBox();
            this.tCClose = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tIp = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.ccn = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tPort)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 85);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(198, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "TCP порт для подключения клиентов:";
            // 
            // tPort
            // 
            this.tPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tPort.Location = new System.Drawing.Point(333, 83);
            this.tPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.tPort.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.tPort.Name = "tPort";
            this.tPort.Size = new System.Drawing.Size(216, 20);
            this.tPort.TabIndex = 1;
            this.tPort.ThousandsSeparator = true;
            this.tPort.Value = new decimal(new int[] {
            9666,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 138);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(168, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Идентификатор задания (Task):";
            // 
            // tTask
            // 
            this.tTask.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tTask.Location = new System.Drawing.Point(333, 135);
            this.tTask.Name = "tTask";
            this.tTask.Size = new System.Drawing.Size(216, 20);
            this.tTask.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(303, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "IP адрес сервера, на который будут обращаться клиенты:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 164);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(130, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Режим работы сервера:";
            // 
            // tServerAs
            // 
            this.tServerAs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tServerAs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.tServerAs.FormattingEnabled = true;
            this.tServerAs.Items.AddRange(new object[] {
            "/server",
            "/holeserver",
            "/dumpserver"});
            this.tServerAs.Location = new System.Drawing.Point(333, 161);
            this.tServerAs.Name = "tServerAs";
            this.tServerAs.Size = new System.Drawing.Size(216, 21);
            this.tServerAs.TabIndex = 7;
            this.tServerAs.SelectedIndexChanged += new System.EventHandler(this.tServerAs_SelectedIndexChanged);
            // 
            // tDumpLabel
            // 
            this.tDumpLabel.AutoSize = true;
            this.tDumpLabel.Location = new System.Drawing.Point(12, 250);
            this.tDumpLabel.Name = "tDumpLabel";
            this.tDumpLabel.Size = new System.Drawing.Size(298, 13);
            this.tDumpLabel.TabIndex = 8;
            this.tDumpLabel.Text = "Создавать и хранить информационную матрицу нарезки:";
            // 
            // tDump
            // 
            this.tDump.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tDump.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.tDump.FormattingEnabled = true;
            this.tDump.Items.AddRange(new object[] {
            "нет",
            "да"});
            this.tDump.Location = new System.Drawing.Point(333, 247);
            this.tDump.Name = "tDump";
            this.tDump.Size = new System.Drawing.Size(216, 21);
            this.tDump.TabIndex = 9;
            // 
            // tHolesLabel
            // 
            this.tHolesLabel.AutoSize = true;
            this.tHolesLabel.Location = new System.Drawing.Point(11, 250);
            this.tHolesLabel.Name = "tHolesLabel";
            this.tHolesLabel.Size = new System.Drawing.Size(91, 13);
            this.tHolesLabel.TabIndex = 10;
            this.tHolesLabel.Text = "Файл с дырами:";
            this.tHolesLabel.Visible = false;
            // 
            // tHoles
            // 
            this.tHoles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tHoles.Location = new System.Drawing.Point(333, 247);
            this.tHoles.Name = "tHoles";
            this.tHoles.Size = new System.Drawing.Size(216, 20);
            this.tHoles.TabIndex = 11;
            this.tHoles.Visible = false;
            // 
            // tHolesB
            // 
            this.tHolesB.Location = new System.Drawing.Point(240, 245);
            this.tHolesB.Name = "tHolesB";
            this.tHolesB.Size = new System.Drawing.Size(75, 23);
            this.tHolesB.TabIndex = 12;
            this.tHolesB.Text = "Выбрать...";
            this.tHolesB.UseVisualStyleBackColor = true;
            this.tHolesB.Visible = false;
            this.tHolesB.Click += new System.EventHandler(this.tHolesB_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 306);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(310, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Закрывать приложения сервера после окончания нарезки:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 278);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(309, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Закрывать приложения клиента после окончания нарезки:";
            // 
            // tSClose
            // 
            this.tSClose.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tSClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.tSClose.FormattingEnabled = true;
            this.tSClose.Items.AddRange(new object[] {
            "нет",
            "да"});
            this.tSClose.Location = new System.Drawing.Point(333, 303);
            this.tSClose.Name = "tSClose";
            this.tSClose.Size = new System.Drawing.Size(216, 21);
            this.tSClose.TabIndex = 16;
            // 
            // tCClose
            // 
            this.tCClose.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tCClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.tCClose.FormattingEnabled = true;
            this.tCClose.Items.AddRange(new object[] {
            "нет",
            "да"});
            this.tCClose.Location = new System.Drawing.Point(333, 275);
            this.tCClose.Name = "tCClose";
            this.tCClose.Size = new System.Drawing.Size(216, 21);
            this.tCClose.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(195, 410);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(139, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Создать командный файл";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(352, 406);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(93, 23);
            this.button1.TabIndex = 18;
            this.button1.Text = "для севера...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(460, 406);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(90, 23);
            this.button2.TabIndex = 19;
            this.button2.Text = "для клиента...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tIp
            // 
            this.tIp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.tIp.FormattingEnabled = true;
            this.tIp.Location = new System.Drawing.Point(333, 108);
            this.tIp.Name = "tIp";
            this.tIp.Size = new System.Drawing.Size(216, 21);
            this.tIp.TabIndex = 2;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 31);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(542, 13);
            this.label9.TabIndex = 21;
            this.label9.Text = "----------------------------------------------------------------------- Параметры" +
                " запуска -----------------------------------------------------------------------" +
                "";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 353);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(541, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "---------------------------------------------------------------------------------" +
                "--------------------------------------------------------------------------------" +
                "-----------------";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.ForeColor = System.Drawing.Color.Maroon;
            this.label10.Location = new System.Drawing.Point(12, 368);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(505, 26);
            this.label10.TabIndex = 23;
            this.label10.Text = "Для запуска командных файлов через планировщик заданий у пользователя, от имени к" +
                "оторого\r\nзапускается командный файл, должны быть соответствующие права";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.ForeColor = System.Drawing.Color.Navy;
            this.label11.Location = new System.Drawing.Point(12, 50);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(503, 26);
            this.label11.TabIndex = 24;
            this.label11.Text = "С синтаксисом и параметрами запуска нарезки из-под командной строки можно ознаком" +
                "иться \r\nв файле mtrc `ReadMe.txt`";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.ForeColor = System.Drawing.Color.Indigo;
            this.label12.Location = new System.Drawing.Point(11, 8);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(514, 13);
            this.label12.TabIndex = 25;
            this.label12.Text = "Прежде чем запускать нарезку, убедитесь что все настройки актуальны и файл проект" +
                "а сохранен!\r\n";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(83, 186);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(463, 52);
            this.label13.TabIndex = 26;
            this.label13.Text = resources.GetString("label13.Text");
            // 
            // ccn
            // 
            this.ccn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ccn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ccn.FormattingEnabled = true;
            this.ccn.Items.AddRange(new object[] {
            "00 - как в настройках файла проекта",
            "cn - равное числу ядер процессора",
            "01",
            "02",
            "03",
            "04",
            "05",
            "06",
            "07",
            "08",
            "09",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32"});
            this.ccn.Location = new System.Drawing.Point(333, 331);
            this.ccn.Name = "ccn";
            this.ccn.Size = new System.Drawing.Size(216, 21);
            this.ccn.TabIndex = 17;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(11, 334);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(273, 13);
            this.label14.TabIndex = 27;
            this.label14.Text = "Число потоков нарезки на клиенте (одновременно):";
            // 
            // CreateCMD
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(566, 444);
            this.Controls.Add(this.ccn);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tIp);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tCClose);
            this.Controls.Add(this.tSClose);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tHolesB);
            this.Controls.Add(this.tHoles);
            this.Controls.Add(this.tHolesLabel);
            this.Controls.Add(this.tDump);
            this.Controls.Add(this.tDumpLabel);
            this.Controls.Add(this.tServerAs);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tTask);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tPort);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateCMD";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Создание командных файлов для запуска нарезки в клиент-серверном режиме";
            this.Load += new System.EventHandler(this.CreateCMD_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown tPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tTask;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox tServerAs;
        private System.Windows.Forms.Label tDumpLabel;
        private System.Windows.Forms.ComboBox tDump;
        private System.Windows.Forms.Label tHolesLabel;
        private System.Windows.Forms.TextBox tHoles;
        private System.Windows.Forms.Button tHolesB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox tSClose;
        private System.Windows.Forms.ComboBox tCClose;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox tIp;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox ccn;
        private System.Windows.Forms.Label label14;
    }
}