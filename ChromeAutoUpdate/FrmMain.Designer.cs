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
            this.SuspendLayout();
            // 
            // lb_status
            // 
            this.lb_status.AutoSize = true;
            this.lb_status.Location = new System.Drawing.Point(12, 240);
            this.lb_status.Name = "lb_status";
            this.lb_status.Size = new System.Drawing.Size(89, 12);
            this.lb_status.TabIndex = 0;
            this.lb_status.Text = "努力加载中……";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(14, 12);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(259, 23);
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
            this.lst_runlog.Location = new System.Drawing.Point(14, 42);
            this.lst_runlog.Name = "lst_runlog";
            this.lst_runlog.Size = new System.Drawing.Size(258, 196);
            this.lst_runlog.TabIndex = 2;
            // 
            // timer_updater
            // 
            this.timer_updater.Interval = 3600000;
            this.timer_updater.Tick += new System.EventHandler(this.timer_updater_Tick);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.lst_runlog);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.lb_status);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmMain";
            this.Text = "chrome自动更新";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lb_status;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ListBox lst_runlog;
        private System.Windows.Forms.Timer timer_updater;
    }
}

