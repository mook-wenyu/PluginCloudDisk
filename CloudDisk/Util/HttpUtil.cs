using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CloudDisk
{
    class HttpUtil
    {
        public static CookieContainer cookieContainer = new CookieContainer();
        public static string html = "text/html";
        public static string json = "application/json; charset=UTF-8";
        public static string stream = "application/octet-stream";
        public static string plain = "text/plain; charset=utf-8";
        public static string gif = "image/gif";

        //contentType application/json or application/xml
        public static string HttpGet(string Url, string contentType)
        {
            try
            {
                string retString = string.Empty;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Method = "GET";
                request.UserAgent = Util.UserAgent;
                request.ContentType = contentType;
                request.CookieContainer = cookieContainer;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(myResponseStream);
                retString = streamReader.ReadToEnd();
                streamReader.Close();
                myResponseStream.Close();
                return retString;
            }
            catch (Exception ex)
            {
                //throw ex;
                Console.WriteLine(ex.Message);
            }

            return "";
        }


        public static string HttpPost(string Url, string postDataStr, string contentType, out bool isOK)
        {
            string retString = string.Empty;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Method = "POST";
                request.UserAgent = Util.UserAgent;
                request.ContentType = contentType;
                request.CookieContainer = cookieContainer;
                request.Timeout = 600000;//设置超时时间
                request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
                Stream requestStream = request.GetRequestStream();
                StreamWriter streamWriter = new StreamWriter(requestStream);
                streamWriter.Write(postDataStr);
                streamWriter.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream);
                retString = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();

                isOK = true;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(WebException))//捕获400错误
                {
                    var response = ((WebException)ex).Response;
                    Stream responseStream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(responseStream);
                    retString = streamReader.ReadToEnd();
                    streamReader.Close();
                    responseStream.Close();
                }
                else
                {
                    retString = ex.ToString();
                }
                isOK = false;
            }

            return retString;
        }

        /// <summary>
        /// 获取HTTP header中Location
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetLocation(string url)
        {
            string newUrl = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                request.UserAgent = Util.UserAgent;
                request.ContentType = plain;
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = false;
                HttpWebResponse httpRes = (HttpWebResponse)request.GetResponse();
                newUrl = httpRes.Headers["Location"];//获取重定向的网址
            }
            catch (Exception e)
            {
                Console.WriteLine("获取重定向网址失败！" + e.Message);
            }
            
            return newUrl;
        }

        /// <summary>
        /// 获取URL的Cookies
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetURLCookies(string url, string contentType)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                request.UserAgent = Util.UserAgent;
                request.ContentType = contentType;
                request.CookieContainer = cookieContainer;
                request.GetResponse().Close();
                string cookiesstr = request.CookieContainer.GetCookieHeader(request.RequestUri); //把cookies转换成字符串
                return cookiesstr;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return "";
        }
    }
}
