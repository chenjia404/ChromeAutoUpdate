using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChromeAutoUpdate
{
    public class UdpListener
    {
        /// <summary>
        /// 用于UDP接收的网络服务类
        /// </summary>
        private UdpClient udpcRecv;


        /// <summary>
        /// 消息格式版本
        /// </summary>
        private int version = 1;


        private int udp_port = 20172;

        private string id = "";


        public UdpListener()
        {
            rsa ras_ = new rsa();
            if (!File.Exists("Private.xml") || !File.Exists("Public.xml"))
            {
                ras_.RSAKey("Private.xml", "Public.xml");
                this.id = ras_.sha1_file("Public.xml");
            }
            else
            {
                this.id = ras_.sha1_file("Public.xml");
            }

        }

    public void StartListener()
        {
            IPEndPoint localIpep = new IPEndPoint(IPAddress.Any, this.udp_port); // 本机IP和监听端口号
            udpcRecv = new UdpClient(localIpep);
            
            thrRecv = new Thread(ReceiveMessage);
            thrRecv.Start();

            log("UDP监听器已成功启动");

            this.ping();

        }


        /// <summary>
        /// 线程：不断监听UDP报文
        /// </summary>
        Thread thrRecv;

        /// <summary>
        /// 接收数据
        /// </summary>
        private void ReceiveMessage()
        {
            IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    byte[] bytRecv = udpcRecv.Receive(ref remoteIpep);
                    string message = Encoding.Unicode.GetString(bytRecv, 0, bytRecv.Length);

                    log(string.Format("{0}:{1}[{2}]",remoteIpep.Address,remoteIpep.Port, message));

                    //处理消息
                    var json = (IDictionary<string, object>)SimpleJson.SimpleJson.DeserializeObject(message);
                    string type = "";
                    if (json.ContainsKey("type"))
                        type = (string)json["type"];

                    if (json.ContainsKey("type"))
                    {
                        switch (type)
                        {
                            case "ping":
                                log("收到ping");
                                this.pong(remoteIpep);
                                break;
                            case "pong":
                                log("收到pong");
                                break;
                            default:
                                log("收到未知消息");
                                break;
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    log( ex.Message);
                    break;
                }
            }
        }


        public static bool log(string msg)
        {
            if (!File.Exists("debug"))
                return false;
            string filename = DateTime.Now.ToString("yyyy-MM-dd") + ".udplog.log";
            using (StreamWriter sw = new StreamWriter(@filename, true))//覆盖模式写入
            {
                sw.WriteLine(DateTime.Now.ToLongTimeString() + " udp:" + msg);
                sw.Close();
            }
            return true;
        }


        public void ping()
        {

            var obj = new Dictionary<string, object>();
            obj["type"] = "ping";
            obj["msg"] = "";
            obj["version"] = this.version;
            obj["uid"] = this.id;

            string json = SimpleJson.SimpleJson.SerializeObject(obj);

            byte[] sendbytes = Encoding.Unicode.GetBytes(json);

            IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Broadcast, this.udp_port); // 发送到的IP地址和端口号

            udpcRecv.Send(sendbytes, sendbytes.Length, remoteIpep);
        }


        /// <summary>
        /// 回应ping
        /// </summary>
        /// <param name="remoteIpep"></param>
        public void pong(IPEndPoint remoteIpep)
        {

            var obj = new Dictionary<string, object>();
            obj["type"] = "pong";
            obj["msg"] = "";
            obj["version"] = this.version;
            obj["uid"] = this.id;

            string json = SimpleJson.SimpleJson.SerializeObject(obj);

            byte[] sendbytes = Encoding.Unicode.GetBytes(json);

            udpcRecv.Send(sendbytes, sendbytes.Length, remoteIpep);
        }
    }
}
