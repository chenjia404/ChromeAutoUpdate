using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace ChromeAutoUpdate
{
    public partial class FrmMain : Form
    {
        public string[] cmd_args;

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            dllName = dllName.Replace(".", "_");
            if (dllName.EndsWith("_resources")) return null;
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(GetType().Namespace + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            byte[] bytes = (byte[])rm.GetObject(dllName);
            return System.Reflection.Assembly.Load(bytes);
        }


        public FrmMain(string[] args)
        {
            this.cmd_args = args;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            InitializeComponent();
        }

        //升级模块
        public update updater;


        public UdpListener uListener;


        private INI config;


        delegate void AddItemToListBoxDelegate(string str);

        /// <summary>  
        /// 在ListBox中追加状态信息  
        /// </summary>  
        /// <param name="str">要追加的信息</param>
        private void AddItemToListBox(string str)
        {
            if (lst_runlog.InvokeRequired)
            {
                AddItemToListBoxDelegate d = AddItemToListBox;
                lst_runlog.Invoke(d, str);
            }
            else
            {
                lst_runlog.Items.Add(str);
                lst_runlog.SelectedIndex = lst_runlog.Items.Count - 1;
                lst_runlog.ClearSelected();
            }
        }


        /// <summary>
        /// 获取mac地址
        /// </summary>
        /// <returns></returns>
        public string GetMacAddressByNetworkInformation()
        {
            string key = "SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\";
            string macAddress = string.Empty;
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in nics)
                {
                    if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                        && adapter.GetPhysicalAddress().ToString().Length != 0)
                    {
                        string fRegistryKey = key + adapter.Id + "\\Connection";
                        RegistryKey rk = Registry.LocalMachine.OpenSubKey(fRegistryKey, false);
                        if (rk != null)
                        {
                            string fPnpInstanceID = rk.GetValue("PnpInstanceID", "").ToString();
                            int fMediaSubType = Convert.ToInt32(rk.GetValue("MediaSubType", 0));
                            if (fPnpInstanceID.Length > 3 &&
                                fPnpInstanceID.Substring(0, 3) == "PCI")
                            {
                                macAddress = adapter.GetPhysicalAddress().ToString();
                                for (int i = 1; i < 6; i++)
                                {
                                    macAddress = macAddress.Insert(3 * i - 1, ":");
                                }
                                break;
                            }
                        }

                    }
                }
            }
            catch
            {
            }
            return macAddress;
        }





        public static bool log(Exception ex)
        {
            try
            {
                if (!File.Exists(Application.StartupPath + @"\debug"))
                    return false;
                if (!Directory.Exists(Application.StartupPath + @"\log"))
                    Directory.CreateDirectory(Application.StartupPath + @"\log");
                string filename = Application.StartupPath + @"\log\" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                using (StreamWriter sw = new StreamWriter(@filename, true))//覆盖模式写入
                {
                    sw.WriteLine(DateTime.Now.ToLongTimeString() + " update:" + ex.Message + ex.StackTrace);
                    sw.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool log(string msg)
        {
            try
            {
                if (!File.Exists(Application.StartupPath + @"\debug"))
                    return false;
                if (!Directory.Exists(Application.StartupPath + @"\log"))
                    Directory.CreateDirectory(Application.StartupPath + @"\log");
                string filename = Application.StartupPath + @"\log\" + DateTime.Now.ToString("yyyy-MM-dd") + ".msg.log";
                using (StreamWriter sw = new StreamWriter(@filename, true))//覆盖模式写入
                {
                    sw.WriteLine(DateTime.Now.ToLongTimeString() + " update:" + msg);
                    sw.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 获取chrome目录
        /// </summary>
        /// <returns></returns>
        public string getAppPath()
        {
            string app_path = Application.StartupPath + @"\Chrome-bin\";

            //如果有配置文件
            if (File.Exists(Application.StartupPath + @"\config.ini"))
            {
                string ini_path = config.ReadValue("app", "path");
                if (ini_path.Length > 3)
                {
                    string localappdata = System.Environment.GetEnvironmentVariable("localappdata");

                    //替换环境变量
                    app_path = app_path.Replace("%localappdata%", localappdata);

                }
            }

            return app_path;
        }

        /// <summary>
        /// 获取chrome主程序路径
        /// </summary>
        /// <returns></returns>
        public string getAppFilename()
        {
            string app_path = getAppPath();

            string app_filename = "chrome.exe";

            app_filename = app_path + app_filename;

            return app_filename;
        }


        /// <summary>
        /// 升级逻辑
        /// </summary>
        public void update()
        {
            if(this.updater == null)
                this.updater = new update();
            try
            {
                this.updater.checkUpdate();
            }
            catch(Exception ex)
            {
                log(ex);
            }
        }
        

        Thread update_th;

        private void FrmMain_Load(object sender, EventArgs e)
        {
            bool set_from = false;

            if (this.cmd_args.Length > 0)
            {
                set_from = "-set" == this.cmd_args[0].ToString();
            }

            //更改工作目录
            Directory.SetCurrentDirectory(Application.StartupPath);

            #region 配置文件
            if (File.Exists(Application.StartupPath + @"\config.ini"))
            {
                this.config = new INI(Application.StartupPath + @"\config.ini");

                string dht = config.ReadValue("config", "dht");

                if (dht == "1" || Directory.Exists("node"))
                {
                    chb_dht.Checked = true;
                    Thread th = new Thread(StartListener);
                    th.Start();
                }


                string startup = config.ReadValue("config", "startup");
                if (startup == "1")
                    chb_start.Checked = true;

                txt_dir.Text = config.ReadValue("app", "path");


                //chrome启动参数
                string Params = config.ReadValue("config", "Params");
                txt_Params.Text = Params;


                //版本选择
                string Channel = config.ReadValue("app", "Channel");
                switch(Channel)
                {
                    case "Stable":
                        rbtnStable.Checked = true;
                        break;
                    case "Beta":
                        rbtnBeta.Checked = true;
                        break;
                    case "Dev":
                        rbtnDev.Checked = true;
                        break;
                    case "Canary":
                        rbtnCanary.Checked = true;
                        break;
                    default:
                        rbtnDev.Checked = true;
                        break;

                }


                //位数
                string bit = config.ReadValue("app", "bit");
                switch (bit)
                {
                    case "4":
                        rbtn_bit4.Checked = true;
                        break;
                    case "8":
                        rbtn_bit8.Checked = true;
                        break;
                    default:
                        rbtn_bit4.Checked = true;
                        break;

                }
            }
            #endregion


            //获取chrome主程序位置
            string app_filename = getAppFilename();


            int processCount = 0;
            Process[] pa = Process.GetProcesses();//获取当前进程数组。
            foreach (Process PTest in pa)
            {
                if (PTest.ProcessName == Process.GetCurrentProcess().ProcessName)
                {
                    processCount += 1;
                }
            }

            //如果已经有当前实例，启动chrome并退出
            if (processCount > 1 && !set_from)
            {
                if (File.Exists(app_filename))
                {
                    this.startApp();
                }
                System.Environment.Exit(0);
            }

            //只运行DHT
            if (this.config.ReadValue("config", "only_dht") == "1")
            {
                this.Visible = set_from;
                this.TopLevel = set_from;
                return;
            }


            if (File.Exists(app_filename))
            {
                this.Visible = set_from;
                this.TopLevel = set_from;
                this.startApp();


                //如果存在就直接启动更新
                try
                {
                    update_th = new Thread(update);
                    update_th.Start();

                    //启动定时更新检查
                    timer_updater.Enabled = true;
                }
                catch (Exception ex)
                {
                    log(ex);
                }
            }
            else
            {
                this.Visible = true;
                this.TopLevel = true;
            }

            if (File.Exists(Application.StartupPath + @"\debug"))
            {
                this.Visible = true;
                this.TopLevel = true;
            }
            
        }


        /// <summary>
        /// 启动监听
        /// </summary>
        public void StartListener()
        {
            try
            {
                uListener = new UdpListener();
                uListener.StartListener();
            }
            catch(Exception ex)
            {
                log(ex);
            }
        }

        public void startApp()
        {
            string app_filename = getAppFilename();

            string index = "";

            string chromeParams = "";

            string user_agent = "";

            if(!File.Exists(app_filename))
            {
                AddItemToListBox(app_filename + "不存在");
                return;
            }

            if (File.Exists(Application.StartupPath + @"\config.ini"))
            {
                string ini_index = config.ReadValue("app", "index");
                if (ini_index.Length > 3)
                    index = ini_index;

                string ini_user_agent = config.ReadValue("app", "user_agent");
                if (ini_user_agent.Length > 3)
                    user_agent = ini_user_agent;

                string ini_chromeParams = config.ReadValue("app", "Params");
                if (ini_chromeParams.Length > 3)
                    chromeParams = ini_chromeParams;
            }

            if (user_agent.Length > 35)
                chromeParams += " --user-agent=\"" + user_agent + "\"";
            chromeParams += " " + index;
            log("启动:" + app_filename);
            //启动
            Process.Start(app_filename, chromeParams);
            AddItemToListBox("启动chrome");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(this.updater != null && updater.run_log.Count>0)
            {
                AddItemToListBox((string)updater.run_log.Dequeue());
            }

            if(this.updater != null)
            {
                //更新进度条
                progressBar1.Maximum = updater.prog_totalBytes;
                progressBar1.Value = updater.prog_Value;

                //更新提示
                lb_status.Text = updater.la_status;
            }
        }

        private void timer_updater_Tick(object sender, EventArgs e)
        {
            update_th = new Thread(update);
            update_th.Start();
        }


        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //后台状态，不提示
            if(!this.Visible)
            {
                return;
            }

            //关机不提示
            if(e.CloseReason == CloseReason.WindowsShutDown)
            {
                System.Environment.Exit(0);
            }


            if (MessageBox.Show("确定要退出吗?", "关闭提示", MessageBoxButtons.OKCancel) == DialogResult.OK)//如果点击“确定”按钮
            {
                System.Environment.Exit(0);
            }
            else//如果点击“取消”按钮
            {
                e.Cancel = true;
            }
        }




        private void chb_start_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey reg = null;
            try
            {
                string fileName = Application.StartupPath + @"\ChromeAutoUpdate.exe";
                String name = "ChromeAutoUpdate_" + Application.StartupPath.GetHashCode();


                if (chb_start.Checked && config.ReadValue("config", "startup") != "1")
                {
                    reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                    config.Writue("config", "startup", "1");
                    reg.SetValue(name, fileName);
                }
                else if(chb_start.Checked == false && config.ReadValue("config", "startup") == "1")
                {
                    reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                    reg.SetValue(name, false);
                    config.Writue("config", "startup", "0");
                }
            }
            catch(System.Security.SecurityException)
            {
                MessageBox.Show("需要管理员权限，请重新打开本程序(右键『已管理员身份运行』后再设置)");
            }
        }

        private void chb_dht_CheckedChanged(object sender, EventArgs e)
        {
            if (File.Exists(Application.StartupPath + @"\config.ini"))
            {
                if(chb_dht.Checked)
                    config.Writue("config", "dht","1");
                else
                    config.Writue("config", "dht", "0");
            }
        }

        private void btn_dir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dilog = new FolderBrowserDialog();
            dilog.Description = "请选择文件夹";
            if (dilog.ShowDialog() == DialogResult.OK || dilog.ShowDialog() == DialogResult.Yes)
            {
                txt_dir.Text = dilog.SelectedPath + @"\";
            }
        }

        private void btn_check_update_Click(object sender, EventArgs e)
        {
            update_th = new Thread(update);
            update_th.Start();
        }

        private void rbtn_Canary_CheckedChanged(object sender, EventArgs e)
        {
            config.Writue("app", "Channel", "Canary");
        }

        private void rbtn_Beta_CheckedChanged(object sender, EventArgs e)
        {
            config.Writue("app", "Channel", "Beta");
        }

        private void rbtn_dev_CheckedChanged(object sender, EventArgs e)
        {
            config.Writue("app", "Channel", "Dev");
        }

        private void rbtnStable_CheckedChanged(object sender, EventArgs e)
        {
            config.Writue("app", "Channel", "Stable");
        }

        private void rbtn_bit8_CheckedChanged(object sender, EventArgs e)
        {
            config.Writue("app", "bit", "8");
        }

        private void rbtn_bit4_CheckedChanged(object sender, EventArgs e)
        {
            config.Writue("app", "bit", "4");
        }

        private void btn_open_Click(object sender, EventArgs e)
        {
            startApp();
        }


        /// <summary>
        /// 通知栏右下角双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = !this.TopLevel;
            this.TopLevel = !this.TopLevel;
        }


        /// <summary>
        /// 关于
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_about_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/chenjia404/ChromeAutoUpdate");
        }


        /// <summary>
        /// 检测更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_update_Click(object sender, EventArgs e)
        {
            update_th = new Thread(update);
            update_th.Start();
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_exit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要退出吗?", "关闭提示", MessageBoxButtons.OKCancel) == DialogResult.OK)//如果点击“确定”按钮
            {
                System.Environment.Exit(0);
            }
        }

        private void txt_Params_TextChanged(object sender, EventArgs e)
        {
            config.Writue("app", "Params", txt_Params.Text);
        }
    }
}
