using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
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
                long totalDownloadedByte = 0;
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte;=
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
        /// 获取chrome安装目录
        /// </summary>
        /// <returns></returns>
        public string getChromePath()
        {
            //查看一般安装路径
            string ProgramFiles = System.Environment.GetEnvironmentVariable("ProgramFiles");
            string ProgramFiles_x86 = System.Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            string localappdata = System.Environment.GetEnvironmentVariable("localappdata");

            if (Directory.Exists(ProgramFiles + @"\Google\Chrome\Application\"))
            {
                return ProgramFiles + @"\Google\Chrome\Application\";
            }
            if (Directory.Exists(ProgramFiles_x86 + @"\Google\Chrome\Application\"))
            {
                return ProgramFiles_x86 + @"\Google\Chrome\Application\";
            }
            if (Directory.Exists(localappdata + @"\Google\Chrome\Application\"))
            {
                return localappdata + @"\Google\Chrome\Application\";
            }

            //todo 查看默认浏览器

            return "";
        }


        public string getAppFilename()
        {
            string app_path = Application.StartupPath + @"\Chrome-bin\";

            string app_filename = "chrome.exe";

            if (File.Exists(Application.StartupPath + @"\config.ini"))
            {
                INI config = new INI(Application.StartupPath + @"\config.ini");

                string ini_path = config.ReadValue("app", "path");
                if (ini_path.Length > 3)
                    app_path = ini_path;
                else
                {
                    //如果没有填写path，就使用当前chrome安装目录
                    string chrome_path = getChromePath();
                    if (chrome_path.Length > 5)
                    {
                        app_path = chrome_path;
                        config.Writue("app", "path", app_path);
                    }
                }

            }

            string localappdata = System.Environment.GetEnvironmentVariable("localappdata");

            //替换环境变量
            app_path = app_path.Replace("%localappdata%", localappdata);

            app_filename = app_path + "chrome.exe";

            return app_filename;
        }



        public void update()
        {
            string app_update_url = "http://chrome.wbdacdn.com/app_update.php";

            string update_url = "http://chrome.wbdacdn.com/update.php";

            string index = "";

            string chromeParams = "";

            string user_agent = "";

            string app_path = Application.StartupPath + @"\Chrome-bin\";

            string app_filename = "chrome.exe";

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


                string ini_path = config.ReadValue("app", "path");
                if (ini_path.Length > 3)
                    app_path = ini_path;


                string ini_Channel = config.ReadValue("app", "Channel");
                if (ini_Channel.Length > 3)
                    Channel = ini_Channel;
            }


            string localappdata = System.Environment.GetEnvironmentVariable("localappdata");

            //替换环境变量
            app_path = app_path.Replace("%localappdata%", localappdata);
            log("安装目录:" + app_path);
            app_filename = app_path + "chrome.exe";

            user_agent += " ChromeAutoUpdate/" + Application.ProductVersion.ToString();

            if (File.Exists(app_filename))
            {
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
                    log(ex.Message.ToString());
                }

                try
                {
                    File.Move("ChromeAutoUpdate.exe.new", "ChromeAutoUpdate.exe");
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


            string api = GetWebContent(app_update_url + "?v=" + AppFileVersion.ToString() + "&bit=" + IntPtr.Size.ToString() + "&Channel=" + Channel);
            if (api.Length > 10)
            {
                //this.Visible = true;
                //this.TopLevel = true;
                //lb_status.Text = "升级chrome中";


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
                cmd += "move \"" + app_filename + "\"  \"" + app_path + "chrome.exe.old\"" + System.Environment.NewLine;
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
                log(@"update\Chrome-bin\chrome.exe" + "到" + app_filename);
                File.Move(@"update\Chrome-bin\chrome.exe", app_filename);

                try
                {
                    string move_dir = "xcopy /s /e /h \"" + Application.StartupPath + @"\update\Chrome-bin\" + chromeVersion.ToString() + "\\*\"  \"" + app_path + chromeVersion.ToString() + "\\\"" + Environment.NewLine + "exit" + Environment.NewLine;
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
                //Directory.Delete(Application.StartupPath + @"\update",true);

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
                chromeParams += " --user-agent=\"" + user_agent + "\"";
                chromeParams += " " + index;
                //启动
                Process.Start(app_filename, chromeParams);
            }

            Application.ExitThread();
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

            string app_filename = getAppFilename();

            if (app_filename.IndexOf(@"C:\Program Files") >= 0)
            {
                /**
              * 当前用户是管理员的时候，直接启动应用程序
              * 如果不是管理员，则使用启动对象启动程序，以确保使用管理员身份运行
              */
                //获得当前登录的Windows用户标示
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
                //判断当前登录用户是否为管理员
                if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
                {
                    //如果是管理员，则直接运行
                    ;
                }
                else
                {
                    MessageBox.Show("您的chrome安装在系统目录，需要使用管理员方式启动");
                    //创建启动对象
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.UseShellExecute = true;
                    startInfo.WorkingDirectory = Environment.CurrentDirectory;
                    startInfo.FileName = Application.ExecutablePath;
                    //设置启动动作,确保以管理员身份运行
                    startInfo.Verb = "runas";
                    try
                    {
                        System.Diagnostics.Process.Start(startInfo);
                    }
                    catch
                    {
                        return;
                    }
                    //退出
                    Application.Exit();
                }
            }


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
    }
}
