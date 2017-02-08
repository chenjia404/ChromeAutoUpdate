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
        public FrmMain()
        {
            InitializeComponent();
        }

        //升级模块
        public update updater;


        public UdpListener uListener;





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

        public static bool log(string msg)
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
                INI config = new INI(Application.StartupPath + @"\config.ini");

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
            string app_path = Application.StartupPath + @"\Chrome-bin\";

            string app_filename = "chrome.exe";

            if (File.Exists(Application.StartupPath + @"\config.ini"))
            {
                INI config = new INI(Application.StartupPath + @"\config.ini");

                string ini_path = config.ReadValue("app", "path");
                if (ini_path.Length > 3)
                {
                    string localappdata = System.Environment.GetEnvironmentVariable("localappdata");

                    //替换环境变量
                    app_path = app_path.Replace("%localappdata%", localappdata);

                }
            }


            app_filename = app_path + app_filename;

            return app_filename;
        }


        /// <summary>
        /// 升级逻辑
        /// </summary>
        public void update()
        {
            this.updater.checkUpdate();
        }



        /// <summary>
        /// 删除老版本
        /// </summary>
        public void deleteOld()
        {
            //获取chrome主程序位置
            string app_filename = getAppFilename();
            string path = this.getAppPath();



            //使用新版
            if (File.Exists(app_filename + ".new"))
            {
                try
                {
                    File.Delete(app_filename);
                    File.Move(app_filename + ".new", app_filename);

                    ///当前chrome版本
                    Version AppFileVersion = new Version("0.0.0.1");
                    if (File.Exists(app_filename))
                    {
                        AppFileVersion = new Version(FileVersionInfo.GetVersionInfo(app_filename).FileVersion);
                    }

                    //定义用于验证正整数的表达式
                    // ^ 表示从字符串的首部开始验证
                    // $ 表示从字符串的尾部开始验证
                    Regex rx = new Regex(@"^(\d+\.\d+\.\d+\.\d+)$", RegexOptions.Compiled);
                    //删除多余的目录
                    DirectoryInfo dir = new DirectoryInfo(path);
                    try
                    {
                        DirectoryInfo[] info = dir.GetDirectories();
                        foreach (DirectoryInfo d in info)
                        {
                            //判断是否是当前运行版本
                            if (rx.IsMatch(d.ToString()) && d.ToString() != AppFileVersion.ToString())
                            {
                                try
                                {
                                    d.MoveTo(dir.ToString() + @"delete_" + d.ToString());
                                    Directory.Delete(d.ToString(), true);
                                }
                                catch (Exception ee)
                                {
                                    //如果正在运行，就不能删除
                                    log(ee);
                                }
                            }
                            else if (d.ToString().IndexOf("delete_") > 0)
                            {
                                Directory.Delete(d.ToString(), true);
                            }
                        }
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show(ee.Message);
                    }
                }
                catch (Exception ex)
                {
                    log(ex);
                }
            }

        }

        Thread update_th;

        private void FrmMain_Load(object sender, EventArgs e)
        {
            

            //删除老文件
            this.deleteOld();

            int processCount = 0;
            Process[] pa = Process.GetProcesses();//获取当前进程数组。
            foreach (Process PTest in pa)
            {
                if (PTest.ProcessName == Process.GetCurrentProcess().ProcessName)
                {
                    processCount += 1;
                }
            }
            if (processCount > 1)
            {
                this.Close();
            }

            //获取chrome主程序位置
            string app_filename = getAppFilename();


            if (File.Exists(app_filename))
            {
                this.Visible = false;
                this.TopLevel = false;
                this.startApp();
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

            Thread th = new Thread(StartListener);
            th.Start();

            try
            {
                this.updater = new update();
                update_th = new Thread(update);
                update_th.Start();
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }


        /// <summary>
        /// 启动监听
        /// </summary>
        public void StartListener()
        {
            uListener = new UdpListener();
            uListener.StartListener();
        }

        public void startApp()
        {
            string app_filename = getAppFilename();

            string index = "";

            string chromeParams = "";

            string user_agent = "";

            if (File.Exists(Application.StartupPath + @"\config.ini"))
            {
                INI config = new INI(Application.StartupPath + @"\config.ini");

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
            if (MessageBox.Show("确定要退出吗?", "关闭提示", MessageBoxButtons.OKCancel) == DialogResult.OK)//如果点击“确定”按钮
            {
                System.Environment.Exit(0);
            }
            else//如果点击“取消”按钮
            {
                e.Cancel = true;
            }
        }
    }
}
