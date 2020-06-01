using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudDisk
{
    public static class Util
    {
        public static string UserAgent = "netdisk;6.9.7.4;PC;PC-Windows;10.0.18363;WindowsBaiduYunGuanJia";
        //public static string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.13 Safari/537.36 Edg/84.0.522.5";
        public static string Domain = "pan.baidu.com";
        public static string PanHome = "";
        public static string Order = "time";
        public static int Desc = 0;
        public static int Showempty = 0;
        public static int Web = 1;
        //public static int Page = 1;
        public static int Num = 100;
        public static string Dir = "/";
        public static string T = "0.21849106194607182";
        public static string Channel = "chunlei";

        public static int App_id = 250528;
        //public static int App_id = 778750;
        public static string Bdstoken = "";
        public static string BaiDuID = "";
        public static string Logid = "";
        public static string Sign1 = "";
        public static string Sign3 = "";
        public static string Sign = "";
        public static string Timestamp = "";
        public static int Clienttype = 0;

        public static string PanList = "";

        public static string BDUSS = "";
        public static string STOKEN = "";


        /// <summary>
        /// 获取秒时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetSecondsTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            Console.WriteLine(Convert.ToInt64(ts.TotalSeconds).ToString());
            return Convert.ToInt64(ts.TotalSeconds).ToString();

        }

        /// <summary>
        /// 获取毫秒时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetMillisecondsTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

        /// <summary>
        /// 转换指定时间戳到对应的时间
        /// </summary>
        /// <param name="timestamp">（10位或13位）时间戳</param>
        /// <returns>返回对应的时间</returns>
        public static DateTime ToDateTime(this string timestamp)
        {
            var tz = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            return timestamp.Length == 13
                ? tz.AddMilliseconds(Convert.ToInt64(timestamp))
                : tz.AddSeconds(Convert.ToInt64(timestamp));
        }

        /// <summary>
        /// 文件大小转换为储存
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string SizeToStorage(string size)
        {
            long storage = Convert.ToInt64(size);

            if (storage < 1024)
            {
                // B
                return string.Format("{0}B", storage.ToString());
            }
            else if (storage < (1024 * 1024))
            {
                // KB
                return string.Format("{0:0.##}KB", (storage / 1024.0));
            }
            else if (storage < (1024 * 1024 * 1024))
            {
                // M
                return string.Format("{0:0.##}M", (storage / (1024.0 * 1024.0)));
            }
            else
            {
                // G
                return string.Format("{0:0.##}G", (storage / (1024.0 * 1024.0 * 1024.0)));
            }

        }

        /// <summary>
        /// 生成指定长度的随机数
        /// </summary>
        /// <param name="iLength"></param>
        /// <returns></returns>
        public static string GetRandomString(int iLength = 16)
        {
            string buffer = "0123456789";// 随机字符中也可以为汉字（任何）
            StringBuilder sb = new StringBuilder();
            Random r = new Random();
            int range = buffer.Length;
            for (int i = 0; i < iLength; i++)
            {
                sb.Append(buffer.Substring(r.Next(range), 1));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取Sign，等效sign2，未编码
        /// </summary>
        /// <param name="sign3"></param>
        /// <param name="sign1"></param>
        /// <returns></returns>
        public static string GetSign(string sign3, string sign1)
        {
            int[] a = new int[256];
            int[] p = new int[256];
            string o = "";
            //j = sign3;
            var v = sign3.Length;
            var j = sign3.ToCharArray();
            for (int q = 0; q < 256; q++)
            {
                a[q] = (int)j[q % v];
                p[q] = q;
            }

            int u = 0;
            for (int q = 0; q < 256; q++)
            {
                u = (u + p[q] + a[q]) % 256;
                var t = p[q];
                p[q] = p[u];
                p[u] = t;
            }

            int i = 0;
            u = 0;
            for (var q = 0; q < sign1.Length; q++)
            {
                i = (i + 1) % 256;
                u = (u + p[i]) % 256;
                var t = p[i];
                p[i] = p[u];
                p[u] = t;
                var k = p[((p[i] + p[u]) % 256)];
                var r = sign1.ToCharArray();
                o += (char)((int)(r[q]) ^ k);
            }

            return o;
        }

    }
}
