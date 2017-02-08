using System;
using System.Net;

namespace ChromeAutoUpdate
{
    public class node
    {
        public IPEndPoint ip;

        public string uid;

        public int last_time;

        public node(string uid, IPEndPoint ip)
        {
            this.uid = uid;
            this.ip = ip;
            this.last_time = unixtime();
        }


        public static int unixtime()
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(DateTime.Now - startTime).TotalSeconds;
        }
    }
}
