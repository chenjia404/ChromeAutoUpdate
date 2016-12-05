using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
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


        /// <summary>
        /// 生成接口请求user-agent
        /// </summary>
        /// <returns></returns>
        public string user_agent()
        {
            Version local = new Version(Application.ProductVersion);
            string user_agent = "Updater/" + local.ToString() + " " + this.GetOSType();
            return user_agent;
        }


        /// <summary>
        /// 生成系统版本
        /// </summary>
        /// <returns></returns>
        public string GetOSType()
        {
            //定义系统版本
            Version ver = System.Environment.OSVersion.Version;
            string OSType = " (Windows NT " + ver.Major + "." + ver.Minor + ")";
            return OSType;
        }



        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">url地址</param>
        /// <param name="filename">本地文件，任意目录均可</param>
        /// <returns></returns>
        public bool DownloadFile(string url, string filename)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                var di = fi.Directory;
                if (!di.Exists)
                    di.Create();

                HttpWebRequest Myrq = (HttpWebRequest)HttpWebRequest.Create(url);
                Myrq.UserAgent = this.user_agent();
                Myrq.Timeout = 10 * 1000;
                HttpWebResponse myrp = (HttpWebResponse)Myrq.GetResponse();
                long totalBytes = myrp.ContentLength;
                Stream st = myrp.GetResponseStream();
                Stream so = new FileStream(filename, FileMode.Create);
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    so.Write(by, 0, osize);
                    osize = st.Read(by, 0, (int)by.Length);
                }
                so.Close();
                st.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
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
            string app_update_url = "http://weibo.wbdacdn.com/chrome/update/";

            string update_url = "http://chrome.wbdacdn.com/update.php";

            string index = "";

            string chromeParams = "";

            string user_agent = "";

            string app_path = Application.StartupPath + @"\Chrome-bin\";

            string app_filename = "chrome.exe";

            string Channel = "Dev";

            string bit = IntPtr.Size.ToString();

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
                else if (config_version == "3")
                {
                    //增加渠道默认参数
                    config.Writue("config", "version", "4");
                    config.Writue("app", "Channel", "Dev");
                }

                else if (config_version == "4")
                {
                    //增加渠道默认参数
                    config.Writue("config", "version", "5");
                    config.Writue("app", "Channel", "Dev");
                }
                else if (config_version == "5")
                {
                    config.Writue("config", "version", "6");
                    config.Writue("server", "app_update_url", app_update_url);
                    config.Writue("app", "user_agent", "");
                    config.Writue("app", "path", "");//默认为当前目录
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


                string ini_path = config.ReadValue("app", "path");
                if (ini_path.Length > 3)
                    app_path = ini_path;


                string ini_Channel = config.ReadValue("app", "Channel");
                if (ini_Channel.Length > 3)
                    Channel = ini_Channel;

                string ini_bit = config.ReadValue("app", "bit");
                if (ini_bit.Length > 3)
                    bit = ini_bit;
            }


            string localappdata = System.Environment.GetEnvironmentVariable("localappdata");

            //替换环境变量
            app_path = app_path.Replace("%localappdata%", localappdata);
            log("安装目录:" + app_path);

            //判断chrome目录是否存在，不存在就创建
            FileInfo fi = new FileInfo(app_path);
            var di = fi.Directory;
            if (!di.Exists)
                di.Create();

            app_filename = app_path + "chrome.exe";

            user_agent += " ChromeAutoUpdate/" + Application.ProductVersion.ToString();

            if (File.Exists(app_filename))
            {
                if (user_agent.Length > 35)
                    chromeParams += " --user-agent=\"" + user_agent + "\"";
                chromeParams += " " + index;
                //启动
                Process.Start(app_filename, chromeParams);
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

            //删除更新目录
            if(Directory.Exists("update"))
            {
                Directory.Delete("update", true);
            }


            //升级自身
            string updater = GetWebContent(update_url + "?v=" + Application.ProductVersion);
            if (updater.Length > 10)
            {
                try
                {
                    //需要判断文件是否成功下载，因为有时候会失败
                    if (DownloadFile(updater, "ChromeAutoUpdate.exe.new"))
                    {
                        File.Move("ChromeAutoUpdate.exe", "ChromeAutoUpdate.exe.old");
                        File.Move("ChromeAutoUpdate.exe.new", "ChromeAutoUpdate.exe");
                    }
                }
                catch (Exception ex)
                {
                    log(ex.Message.ToString());
                }
            }


            //升级app
            Version AppFileVersion = new Version("0.0.0.1");
            if (File.Exists(app_filename))
            {
                AppFileVersion = new Version(FileVersionInfo.GetVersionInfo(app_filename).FileVersion);
            }


            string api = GetWebContent(app_update_url + "?v=" + AppFileVersion.ToString() + "&bit=" + bit + "&Channel=" + Channel);
            if (api.Length > 10)
            {
                //this.Visible = true;
                //this.TopLevel = true;
                //lb_status.Text = "升级chrome中";


                string tmp_file = Path.GetTempFileName() + ".tmp";

                ///多个下载地址重试
                string[] urls = api.Split('|');
                foreach (string url in urls)
                {
                    if (DownloadFile(url, tmp_file))
                        break;
                }


                //验证文件签名
                try
                {
                    X509Certificate cert = X509Certificate.CreateFromSignedFile(tmp_file);
                    if(cert.Subject.IndexOf("CN=Google Inc") < 0)
                    {
                        return;
                    }
                }
                catch(Exception ex)
                {
                    log(ex);
                    return;
                }

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
                cmd += @"7zr.exe -y x chrome.7z -oupdate" + Environment.NewLine;
                cmd += "del " + tmp_file + Environment.NewLine + "exit" + Environment.NewLine;

                //向CMD窗口发送输入信息：  
                p.StandardInput.WriteLine(cmd);
                string cmd_log = p.StandardOutput.ReadToEnd();
                //记录cmd执行情况
                log(cmd_log);
                p.WaitForExit();//等待程序执行完退出进程

                /** 解压完成，移动文件 **/
                Version chromeVersion = new Version(FileVersionInfo.GetVersionInfo(@"update\Chrome-bin\chrome.exe").FileVersion);

                //移动chrome.exe
                log(@"update\Chrome-bin\chrome.exe" + "到" + app_filename + @".new");
                File.Move(@"update\Chrome-bin\chrome.exe", app_filename+@".new");

                try
                {
                    string move_dir = "xcopy /s /e /h /y \"" + Application.StartupPath + @"\update\Chrome-bin\" + chromeVersion.ToString() + "\\*\"  \"" + app_path + chromeVersion.ToString() + "\\\"" + Environment.NewLine + "exit" + Environment.NewLine;
                    log(move_dir);

                    //移动目录
                    Process p2 = new Process();
                    //要执行的程序名称，cmd  
                    p2.StartInfo.FileName = "cmd.exe";
                    p2.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                    p2.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                    p2.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                    p2.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                    p2.StartInfo.CreateNoWindow = true;//不显示程序窗口
                    p2.Start();//启动程序  
                    p2.StandardInput.WriteLine(move_dir);

                    //记录cmd执行情况
                    cmd_log = p2.StandardOutput.ReadToEnd();
                    log(cmd_log);
                    p2.WaitForExit();//等待程序执行完退出进程
                }
                catch (Exception ex)
                {
                    log(ex);

                }

                //删除目录
                Directory.Delete(Application.StartupPath + @"\update", true);

                File.Delete("chrome.7z");


            }
            else
            {
                log("不需要更新" + AppFileVersion.ToString());
            }

            while (!File.Exists(app_filename))
            {
                Thread.Sleep(1000);
            }

            if (!app_is_run && File.Exists(app_filename))
            {
                if (user_agent.Length > 35)
                    chromeParams += " --user-agent=\"" + user_agent + "\"";
                chromeParams += " " + index;
                //启动
                Process.Start(app_filename, chromeParams);
            }


            

            this.status = "exit";
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
                catch(Exception ex)
                {
                    log(ex);
                }
            }
            
        }


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
            }
            else
            {
                this.Visible = true;
                this.TopLevel = true;
            }

            try
            {

                Thread th = new Thread(update);
                th.Start();
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }

        public string status = "";

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.status == "exit")
            {
                this.Close();
            }
        }
    }
}
