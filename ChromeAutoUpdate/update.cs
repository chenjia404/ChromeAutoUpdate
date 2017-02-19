using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;

namespace ChromeAutoUpdate
{
    public class update
    {
        public Queue run_log = new Queue();

        //唯一用户id，用于统计用户数
        public string uid = "";


        public update()
        {
            this.run_log.Enqueue("升级模块初始化");


            rsa UserRsa = new rsa();
            //不存在就创建
            if (!File.Exists("Private.xml") || !File.Exists("Public.xml"))
            {
                UserRsa.RSAKey("Private.xml", "Public.xml");
                this.uid = UserRsa.sha1(UserRsa.readFile("Public.xml"));
            }
            else
            {
                this.uid = UserRsa.sha1(UserRsa.readFile("Public.xml"));
            }
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

        public int prog_totalBytes = 0;
        public int prog_Value = 0;
        public string la_status = "";


        /// <summary>
        /// 带进度的下载文件
        /// </summary>
        /// <param name="URL">下载文件地址</param>
        /// <param name="Filename">下载后的存放地址</param>
        ///
        public bool DownloadFileProg(string URL, string filename)
        {
            float percent = 0;
            try
            {
                System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                long totalBytes = myrp.ContentLength;
                this.prog_totalBytes = (int)totalBytes;
                System.IO.Stream st = myrp.GetResponseStream();
                System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                long totalDownloadedByte = 0;
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte;
                    System.Windows.Forms.Application.DoEvents();
                    so.Write(by, 0, osize);
                    this.prog_Value = (int)totalDownloadedByte;
                    osize = st.Read(by, 0, (int)by.Length);

                    percent = (float)totalDownloadedByte / (float)totalBytes * 100;
                    this.la_status = "当前下载进度" + percent.ToString() + "%";
                }
                so.Close();
                st.Close();
                return true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
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


        public void AddItemToListBox(string msg)
        {
            this.run_log.Enqueue(msg);
        }


        public void checkUpdate()
        {
            string only_dht = "";

            string app_update_url = "http://weibo.wbdacdn.com/chrome/update/";

            string update_url = "http://chrome.wbdacdn.com/update.php";

            string index = "";

            string chromeParams = "";

            string user_agent = "";

            string app_path = Application.StartupPath + @"\Chrome-bin\";

            string app_filename = "chrome.exe";

            string Channel = "Dev";

            string bit = IntPtr.Size.ToString();

            bool app_is_run = File.Exists(app_path+app_filename);


            #region 获取配置文件
            if (File.Exists(Application.StartupPath + @"\config.ini"))
            {
                INI config = new INI(Application.StartupPath + @"\config.ini");

                only_dht = config.ReadValue("config", "only_dht");


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
                {
                    app_path = ini_path;
                    if(app_path.Substring(app_path.Length-1) != @"\")
                    {
                        config.Writue("app", "path", app_path+@"\");
                        app_path = app_path + @"\";
                    }
                }


                string ini_Channel = config.ReadValue("app", "Channel");
                if (ini_Channel.Length > 3)
                    Channel = ini_Channel;

                string ini_bit = config.ReadValue("app", "bit");
                if (ini_bit.Length > 3)
                    bit = ini_bit;

                AddItemToListBox("读取配置文件成功");
            }
            #endregion


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
            if (Directory.Exists("update"))
            {
                Directory.Delete("update", true);
            }


            //升级自身
            string updater = GetWebContent(update_url + "?v=" + Application.ProductVersion + "&uid=" + this.uid);
            if (updater.Length > 10)
            {
                AddItemToListBox("更新ChromeAutoUpdate");
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

            //只启用dht功能
            if(only_dht == "1")
            {
                return;
            }

            //升级app
            Version AppFileVersion = new Version("0.0.0.1");

            //如果已经更新了，但是还没有替换文件
            if (File.Exists(app_filename + ".new"))
            {
                AppFileVersion = new Version(FileVersionInfo.GetVersionInfo(app_filename + ".new").FileVersion);
            }
            else if (File.Exists(app_filename))
            {
                AppFileVersion = new Version(FileVersionInfo.GetVersionInfo(app_filename).FileVersion);
            }


            string api = GetWebContent(app_update_url + "?v=" + AppFileVersion.ToString() + "&bit=" + bit + "&Channel=" + Channel + "&format=json");

            var apiJson = (IDictionary<string, object>)SimpleJson.SimpleJson.DeserializeObject(api);

            Version serverVersion = new Version(apiJson["version"].ToString());

            #region 升级chrome流程
            if (serverVersion > AppFileVersion)
            {
                //this.Visible = true;
                //this.TopLevel = true;
                //lb_status.Text = "升级chrome中";
                AddItemToListBox("升级chrome(" + apiJson["version"].ToString());

                string tmp_file = Path.GetTempFileName() + ".tmp";

                ///多个下载地址重试
                string[] urls = apiJson["url"].ToString().Split('|');
                foreach (string url in urls)
                {
                    AddItemToListBox("下载:"+ url);
                    if (DownloadFileProg(url, tmp_file))
                        break;
                }


                //验证文件签名
                try
                {
                    X509Certificate cert = X509Certificate.CreateFromSignedFile(tmp_file);
                    if (cert.Subject.IndexOf("CN=Google Inc") < 0)
                    {
                        return;
                    }
                }
                catch (Exception ex)
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

                AddItemToListBox("7z解压chrome.7z");
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

                //如果存在新版本，就删除新版本
                if(File.Exists(app_filename + @".new"))
                {
                    File.Delete(app_filename + @".new");
                }

                //如果存在，就保存新版本，不然就直接移动
                if (File.Exists(app_filename))
                    File.Move(@"update\Chrome-bin\chrome.exe", app_filename + @".new");
                else
                    File.Move(@"update\Chrome-bin\chrome.exe", app_filename);

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
                AddItemToListBox("没有新版本chrome");
            }
            #endregion

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
            AddItemToListBox("结束本次更新");
        }
    }
}
