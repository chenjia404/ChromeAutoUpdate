namespace ChromeAutoUpdate
{
    partial class FrmMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.lb_status = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lst_runlog = new System.Windows.Forms.ListBox();
            this.timer_updater = new System.Windows.Forms.Timer(this.components);
            this.chb_start = new System.Windows.Forms.CheckBox();
            this.chb_dht = new System.Windows.Forms.CheckBox();
            this.lbl_dir = new System.Windows.Forms.Label();
            this.txt_dir = new System.Windows.Forms.TextBox();
            this.btn_dir = new System.Windows.Forms.Button();
            this.btn_check_update = new System.Windows.Forms.Button();
            this.lbl_Channel = new System.Windows.Forms.Label();
            this.rbtnStable = new System.Windows.Forms.RadioButton();
            this.rbtnBeta = new System.Windows.Forms.RadioButton();
            this.rbtnDev = new System.Windows.Forms.RadioButton();
            this.rbtnCanary = new System.Windows.Forms.RadioButton();
            this.lbl_bit = new System.Windows.Forms.Label();
            this.rbtn_bit4 = new System.Windows.Forms.RadioButton();
            this.rbtn_bit8 = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btn_open = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_update = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_about = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // lb_status
            // 
            this.lb_status.AutoSize = true;
            this.lb_status.Location = new System.Drawing.Point(10, 415);
            this.lb_status.Name = "lb_status";
            this.lb_status.Size = new System.Drawing.Size(89, 12);
            this.lb_status.TabIndex = 0;
            this.lb_status.Text = "等待配置中……";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 378);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(470, 23);
            this.progressBar1.TabIndex = 1;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lst_runlog
            // 
            this.lst_runlog.FormattingEnabled = true;
            this.lst_runlog.ItemHeight = 12;
            this.lst_runlog.Location = new System.Drawing.Point(16, 42);
            this.lst_runlog.Name = "lst_runlog";
            this.lst_runlog.Size = new System.Drawing.Size(456, 196);
            this.lst_runlog.TabIndex = 2;
            // 
            // timer_updater
            // 
            this.timer_updater.Interval = 3600000;
            this.timer_updater.Tick += new System.EventHandler(this.timer_updater_Tick);
            // 
            // chb_start
            // 
            this.chb_start.AutoSize = true;
            this.chb_start.Location = new System.Drawing.Point(410, 415);
            this.chb_start.Name = "chb_start";
            this.chb_start.Size = new System.Drawing.Size(72, 16);
            this.chb_start.TabIndex = 3;
            this.chb_start.Text = "开机启动";
            this.chb_start.UseVisualStyleBackColor = true;
            this.chb_start.CheckedChanged += new System.EventHandler(this.chb_start_CheckedChanged);
            // 
            // chb_dht
            // 
            this.chb_dht.AutoSize = true;
            this.chb_dht.Location = new System.Drawing.Point(362, 415);
            this.chb_dht.Name = "chb_dht";
            this.chb_dht.Size = new System.Drawing.Size(42, 16);
            this.chb_dht.TabIndex = 4;
            this.chb_dht.Text = "DHT";
            this.chb_dht.UseVisualStyleBackColor = true;
            this.chb_dht.CheckedChanged += new System.EventHandler(this.chb_dht_CheckedChanged);
            // 
            // lbl_dir
            // 
            this.lbl_dir.AutoSize = true;
            this.lbl_dir.Location = new System.Drawing.Point(14, 17);
            this.lbl_dir.Name = "lbl_dir";
            this.lbl_dir.Size = new System.Drawing.Size(29, 12);
            this.lbl_dir.TabIndex = 5;
            this.lbl_dir.Text = "目录";
            // 
            // txt_dir
            // 
            this.txt_dir.Location = new System.Drawing.Point(49, 14);
            this.txt_dir.Name = "txt_dir";
            this.txt_dir.Size = new System.Drawing.Size(342, 21);
            this.txt_dir.TabIndex = 6;
            // 
            // btn_dir
            // 
            this.btn_dir.Location = new System.Drawing.Point(397, 12);
            this.btn_dir.Name = "btn_dir";
            this.btn_dir.Size = new System.Drawing.Size(75, 23);
            this.btn_dir.TabIndex = 7;
            this.btn_dir.Text = "选择";
            this.btn_dir.UseVisualStyleBackColor = true;
            this.btn_dir.Click += new System.EventHandler(this.btn_dir_Click);
            // 
            // btn_check_update
            // 
            this.btn_check_update.Location = new System.Drawing.Point(397, 340);
            this.btn_check_update.Name = "btn_check_update";
            this.btn_check_update.Size = new System.Drawing.Size(75, 23);
            this.btn_check_update.TabIndex = 8;
            this.btn_check_update.Text = "检测更新";
            this.btn_check_update.UseVisualStyleBackColor = true;
            this.btn_check_update.Click += new System.EventHandler(this.btn_check_update_Click);
            // 
            // lbl_Channel
            // 
            this.lbl_Channel.AutoSize = true;
            this.lbl_Channel.Location = new System.Drawing.Point(10, 9);
            this.lbl_Channel.Name = "lbl_Channel";
            this.lbl_Channel.Size = new System.Drawing.Size(41, 12);
            this.lbl_Channel.TabIndex = 9;
            this.lbl_Channel.Text = "版本：";
            // 
            // rbtnStable
            // 
            this.rbtnStable.AutoSize = true;
            this.rbtnStable.Location = new System.Drawing.Point(58, 7);
            this.rbtnStable.Name = "rbtnStable";
            this.rbtnStable.Size = new System.Drawing.Size(47, 16);
            this.rbtnStable.TabIndex = 10;
            this.rbtnStable.Text = "稳定";
            this.rbtnStable.UseVisualStyleBackColor = true;
            this.rbtnStable.CheckedChanged += new System.EventHandler(this.rbtnStable_CheckedChanged);
            // 
            // rbtnBeta
            // 
            this.rbtnBeta.AutoSize = true;
            this.rbtnBeta.Location = new System.Drawing.Point(122, 7);
            this.rbtnBeta.Name = "rbtnBeta";
            this.rbtnBeta.Size = new System.Drawing.Size(47, 16);
            this.rbtnBeta.TabIndex = 10;
            this.rbtnBeta.Text = "测试";
            this.rbtnBeta.UseVisualStyleBackColor = true;
            this.rbtnBeta.CheckedChanged += new System.EventHandler(this.rbtn_Beta_CheckedChanged);
            // 
            // rbtnDev
            // 
            this.rbtnDev.AutoSize = true;
            this.rbtnDev.Location = new System.Drawing.Point(189, 7);
            this.rbtnDev.Name = "rbtnDev";
            this.rbtnDev.Size = new System.Drawing.Size(47, 16);
            this.rbtnDev.TabIndex = 11;
            this.rbtnDev.Text = "开发";
            this.rbtnDev.UseVisualStyleBackColor = true;
            this.rbtnDev.CheckedChanged += new System.EventHandler(this.rbtn_dev_CheckedChanged);
            // 
            // rbtnCanary
            // 
            this.rbtnCanary.AutoSize = true;
            this.rbtnCanary.Location = new System.Drawing.Point(253, 7);
            this.rbtnCanary.Name = "rbtnCanary";
            this.rbtnCanary.Size = new System.Drawing.Size(59, 16);
            this.rbtnCanary.TabIndex = 12;
            this.rbtnCanary.Text = "金丝雀";
            this.rbtnCanary.UseVisualStyleBackColor = true;
            this.rbtnCanary.CheckedChanged += new System.EventHandler(this.rbtn_Canary_CheckedChanged);
            // 
            // lbl_bit
            // 
            this.lbl_bit.AutoSize = true;
            this.lbl_bit.Location = new System.Drawing.Point(10, 14);
            this.lbl_bit.Name = "lbl_bit";
            this.lbl_bit.Size = new System.Drawing.Size(41, 12);
            this.lbl_bit.TabIndex = 9;
            this.lbl_bit.Text = "系统：";
            // 
            // rbtn_bit4
            // 
            this.rbtn_bit4.AutoSize = true;
            this.rbtn_bit4.Location = new System.Drawing.Point(58, 12);
            this.rbtn_bit4.Name = "rbtn_bit4";
            this.rbtn_bit4.Size = new System.Drawing.Size(47, 16);
            this.rbtn_bit4.TabIndex = 10;
            this.rbtn_bit4.Text = "32位";
            this.rbtn_bit4.UseVisualStyleBackColor = true;
            this.rbtn_bit4.CheckedChanged += new System.EventHandler(this.rbtn_bit4_CheckedChanged);
            // 
            // rbtn_bit8
            // 
            this.rbtn_bit8.AutoSize = true;
            this.rbtn_bit8.Location = new System.Drawing.Point(122, 12);
            this.rbtn_bit8.Name = "rbtn_bit8";
            this.rbtn_bit8.Size = new System.Drawing.Size(47, 16);
            this.rbtn_bit8.TabIndex = 10;
            this.rbtn_bit8.Text = "64位";
            this.rbtn_bit8.UseVisualStyleBackColor = true;
            this.rbtn_bit8.CheckedChanged += new System.EventHandler(this.rbtn_bit8_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rbtnStable);
            this.panel1.Controls.Add(this.rbtnCanary);
            this.panel1.Controls.Add(this.lbl_Channel);
            this.panel1.Controls.Add(this.rbtnDev);
            this.panel1.Controls.Add(this.rbtnBeta);
            this.panel1.Location = new System.Drawing.Point(16, 244);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(358, 31);
            this.panel1.TabIndex = 13;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rbtn_bit4);
            this.panel2.Controls.Add(this.lbl_bit);
            this.panel2.Controls.Add(this.rbtn_bit8);
            this.panel2.Location = new System.Drawing.Point(16, 281);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 31);
            this.panel2.TabIndex = 14;
            // 
            // btn_open
            // 
            this.btn_open.AccessibleRole = System.Windows.Forms.AccessibleRole.PageTabList;
            this.btn_open.Location = new System.Drawing.Point(310, 340);
            this.btn_open.Name = "btn_open";
            this.btn_open.Size = new System.Drawing.Size(81, 23);
            this.btn_open.TabIndex = 15;
            this.btn_open.Text = "打开浏览器";
            this.btn_open.UseVisualStyleBackColor = true;
            this.btn_open.Click += new System.EventHandler(this.btn_open_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "chrome自动升级工具";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_about,
            this.toolStripMenuItem_update,
            this.toolStripMenuItem_exit});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(153, 92);
            // 
            // toolStripMenuItem_exit
            // 
            this.toolStripMenuItem_exit.Name = "toolStripMenuItem_exit";
            this.toolStripMenuItem_exit.RightToLeftAutoMirrorImage = true;
            this.toolStripMenuItem_exit.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem_exit.Text = "退出";
            this.toolStripMenuItem_exit.Click += new System.EventHandler(this.toolStripMenuItem_exit_Click);
            // 
            // toolStripMenuItem_update
            // 
            this.toolStripMenuItem_update.Name = "toolStripMenuItem_update";
            this.toolStripMenuItem_update.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem_update.Text = "检测更新";
            this.toolStripMenuItem_update.Click += new System.EventHandler(this.toolStripMenuItem_update_Click);
            // 
            // toolStripMenuItem_about
            // 
            this.toolStripMenuItem_about.Name = "toolStripMenuItem_about";
            this.toolStripMenuItem_about.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem_about.Text = "关于";
            this.toolStripMenuItem_about.Click += new System.EventHandler(this.toolStripMenuItem_about_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 439);
            this.Controls.Add(this.btn_open);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btn_check_update);
            this.Controls.Add(this.btn_dir);
            this.Controls.Add(this.txt_dir);
            this.Controls.Add(this.lbl_dir);
            this.Controls.Add(this.chb_dht);
            this.Controls.Add(this.chb_start);
            this.Controls.Add(this.lst_runlog);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.lb_status);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmMain";
            this.Text = "chrome自动更新";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lb_status;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ListBox lst_runlog;
        private System.Windows.Forms.Timer timer_updater;
        private System.Windows.Forms.CheckBox chb_start;
        private System.Windows.Forms.CheckBox chb_dht;
        private System.Windows.Forms.Label lbl_dir;
        private System.Windows.Forms.TextBox txt_dir;
        private System.Windows.Forms.Button btn_dir;
        private System.Windows.Forms.Button btn_check_update;
        private System.Windows.Forms.Label lbl_Channel;
        private System.Windows.Forms.RadioButton rbtnStable;
        private System.Windows.Forms.RadioButton rbtnBeta;
        private System.Windows.Forms.RadioButton rbtnDev;
        private System.Windows.Forms.RadioButton rbtnCanary;
        private System.Windows.Forms.Label lbl_bit;
        private System.Windows.Forms.RadioButton rbtn_bit4;
        private System.Windows.Forms.RadioButton rbtn_bit8;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btn_open;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_exit;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_update;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_about;
    }
}

