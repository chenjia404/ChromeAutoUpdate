using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Management;

namespace ChromeAutoUpdate
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        public string user_agent()
        {
            Version local = new Version(Application.ProductVersion);
            string user_agent = "Updater/" + local.ToString() + " " + this.GetOSType();
            return user_agent;
        }

        public string GetOSType()
        {
            //定义系统版本
            Version ver = System.Environment.OSVersion.Version;
            string OSType = " (Windows NT " + ver.Major + "." + ver.Minor + ")";
            return OSType;
        }




        public bool DownloadFile(string url, string filename)
        {
            FileInfo fi = new FileInfo(filename);
            var di = fi.Directory;
            if (!di.Exists)
                di.Create();

            WebClient wc = new WebClient();
            try
            { 
                wc.Headers.Add(HttpRequestHeader.UserAgent, this.user_agent());
                wc.DownloadFile(url, filename);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return true;
        }



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



        /// <summary>
        /// 抓取网页,支持gzip
        /// </summary>
        /// <param name="sUrl"></param>
        /// <returns></returns>
        public string GetWebContent(string sUrl)
        {
            string strResult = "";
            string charset = "UTF-8";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sUrl);
                request.UserAgent = this.user_agent();
                //声明一个HttpWebRequest请求
                request.Timeout = 5000;
                //设置连接超时时间
                request.Headers.Set("Pragma", "no-cache");
                request.Headers.Set("Accept-Encoding", "gzip");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.CharacterSet != "")
                    charset = response.CharacterSet;
                if (response.ToString() == "")
                    return "";

                if (response.ContentEncoding.ToLower().Contains("gzip") | (response.Headers["Content-Encoding"] != null && response.Headers["Content-Encoding"].Contains("gzip")))
                {
                    using (GZipStream stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                    {
                        using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.GetEncoding(charset)))
                        {
                            strResult = reader.ReadToEnd();
                        }
                    }
                }
                else
                {
                    Stream myStream = response.GetResponseStream();
                    using (StreamReader reader = new StreamReader(myStream, System.Text.Encoding.GetEncoding(charset)))
                    {
                        strResult = reader.ReadToEnd();
                    }
                }
            }
            catch
            {
                strResult = "";
            }
            return strResult;
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



        public void update()
        {
            string app_update_url = "http://chrome.wbdacdn.com/app_update.php";

            string update_url = "http://chrome.wbdacdn.com/update.php";

            string index = "";

            string chromeParams = "";

            string user_agent = "";

            string app_filename = "Chrome-bin/chrome.exe";

            string Channel = "Canary";

            bool app_is_run = false;

            if (File.Exists(Application.StartupPath + @"\config.ini"))
            {
                INI config = new INI(Application.StartupPath + @"\config.ini");



                string config_version = config.ReadValue("config", "version");
                if (config_version.Length == 0)
                {
                    config.Writue("config", "version", "1");
                    config.Writue("server", "Params", "");
                    config.Writue("app", "Params", "");
                    config.Writue("app", "user_agent", "\"Mozilla/5.0(Windows NT 10.0; Win64; x64) AppleWebKit/537.36(KHTML, like Gecko) Chrome/56.0.2902.0 Safari/537.36\"");
                    user_agent = "Mozilla/5.0(Windows NT 10.0; Win64; x64) AppleWebKit/537.36(KHTML, like Gecko) Chrome/56.0.2902.0 Safari/537.36";
                }
                else if (config_version == "1")
                {
                    config.Writue("server", "update_url", update_url);
                    config.Writue("server", "app_update_url", app_update_url);
                    config.Writue("config", "version", "2");
                }
                else if (config_version == "2")
                {
                    config.Writue("config", "version", "3");
                    config.Writue("app", "user_agent", "\"Mozilla/5.0(Windows NT 10.0; Win64; x64) AppleWebKit/537.36(KHTML, like Gecko) Chrome/56.0.2902.0 Safari/537.36\"");
                    user_agent = "Mozilla/5.0(Windows NT 10.0; Win64; x64) AppleWebKit/537.36(KHTML, like Gecko) Chrome/56.0.2902.0 Safari/537.36";
                }

                string ini_app_update_url = config.ReadValue("server", "app_update_url");
                if (ini_app_update_url.Length > 3)
                    app_update_url = ini_app_update_url;



                string ini_update_url = config.ReadValue("server", "update_url");
                if (ini_update_url.Length > 3)
                    update_url = ini_update_url;

                string ini_chromeParams = config.ReadValue("app", "Params");
                if (ini_chromeParams.Length > 3)
                    chromeParams = ini_chromeParams;


                string ini_index = config.ReadValue("app", "index");
                if (ini_index.Length > 3)
                    index = ini_index;


                string ini_user_agent = config.ReadValue("app", "user_agent");
                if (ini_user_agent.Length > 3)
                    user_agent = ini_user_agent;



                string ini_Channel = config.ReadValue("app", "Channel");
                if (ini_Channel.Length > 3)
                    Channel = ini_Channel;
            }

            user_agent += " ChromeAutoUpdate/" + Application.ProductVersion.ToString();


            if (File.Exists(app_filename))
            {
                chromeParams += " --user-agent=\"" + user_agent + "\"";
                chromeParams += " " + index;
                //启动
                Process.Start(Application.StartupPath + @"\" + app_filename, chromeParams);
                app_is_run = true;
            }

            if (File.Exists("chrome.7z"))
            {
                File.Delete("chrome.7z");
            }
            if (File.Exists("ChromeAutoUpdate.exe.old"))
            {
                File.Delete("ChromeAutoUpdate.exe.old");
            }
            if (File.Exists("ChromeAutoUpdate.exe.new"))
            {
                File.Delete("ChromeAutoUpdate.exe.new");
            }
            if (File.Exists(app_filename + ".old"))
            {
                File.Delete(app_filename + ".old");
            }


            //升级自身
            string updater = GetWebContent(update_url + "?v=" + Application.ProductVersion);
            if (updater.Length > 10)
            {
                DownloadFile(updater, "ChromeAutoUpdate.exe.new");
                try
                {

                    File.Move("ChromeAutoUpdate.exe", "ChromeAutoUpdate.exe.old");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }

                try
                {
                    File.Move("ChromeAutoUpdate.exe.new", "ChromeAutoUpdate.exe");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }

            }


            //升级app
            Version AppFileVersion = new Version("0.0.0.1");
            if (File.Exists(app_filename))
            {
                AppFileVersion = new Version(FileVersionInfo.GetVersionInfo(app_filename).FileVersion);
            }


            string api = GetWebContent(app_update_url + "?v=" + AppFileVersion.ToString() + "&bit=" + IntPtr.Size.ToString() + "&Channel=" + Channel);
            if (api.Length > 10)
            {
                this.Visible = true;
                this.TopLevel = true;
                lb_status.Text = "升级chrome";


                string tmp_file = Path.GetTempFileName() + ".tmp";
                DownloadFile(api, tmp_file);

                //实例化process对象  
                Process p = new Process();
                //要执行的程序名称，cmd  
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序  

                string cmd = @"7zr.exe -y e " + tmp_file + System.Environment.NewLine;
                cmd += "move " + app_filename + " chrome.exe.old" + System.Environment.NewLine;
                cmd += @"7zr.exe -y x chrome.7z" + Environment.NewLine;
                cmd += "del " + tmp_file + Environment.NewLine + "exit" + Environment.NewLine;

                //向CMD窗口发送输入信息：  
                p.StandardInput.WriteLine(cmd);
                string cmd_log = p.StandardOutput.ReadToEnd();
                log(cmd_log);
                p.WaitForExit();//等待程序执行完退出进程
            }

            while (!File.Exists(app_filename))
            {
                Thread.Sleep(1000);
            }

            if (!app_is_run && File.Exists(app_filename))
            {
                chromeParams += " --user-agent=\"" + user_agent + "\"";
                chromeParams += " " + index;
                //启动
                Process.Start(Application.StartupPath + @"\" + app_filename, chromeParams);
            }


            Application.Exit();
        }


        private void FrmMain_Load(object sender, EventArgs e)
        {
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
                Application.Exit();

            string app_filename = "Chrome-bin/chrome.exe";

            if (File.Exists(app_filename))
            {
                this.Visible = false;
                this.TopLevel = false;
            }
            else
            {
                this.Visible = true;
                this.TopLevel = true;
            }

            Thread th = new Thread(update);
            th.Start();
        }
    }
}
