using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ffmpegForRTMP
{
    public class HTMLHelper
    {
        /// <summary>
        /// 获取CooKie
        /// </summary>
        /// <param name="loginUrl"></param>
        /// <param name="postdata"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static CookieContainer GetCooKie(string loginUrl, string postdata, HttpHeader header)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                CookieContainer cc = new CookieContainer();
                request = (HttpWebRequest)WebRequest.Create(loginUrl);
                request.Method = header.method;
                request.ContentType = header.contentType;
                byte[] postdatabyte = Encoding.UTF8.GetBytes(postdata);     //提交的请求主体的内容
                request.ContentLength = postdatabyte.Length;    //提交的请求主体的长度
                request.AllowAutoRedirect = false;
                request.CookieContainer = cc;
                request.KeepAlive = true;
                request.ContentType = "text/html;charset=UTF-8";

                //提交请求
                Stream stream;
                stream = request.GetRequestStream();
                stream.Write(postdatabyte, 0, postdatabyte.Length);     //带上请求主体
                stream.Close();

                //接收响应
                response = (HttpWebResponse)request.GetResponse();      //正式发起请求
                response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);

                CookieCollection cook = response.Cookies;
                //Cookie字符串格式
                string strcrook = request.CookieContainer.GetCookieHeader(request.RequestUri);

                return cc;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
         public static string HttpGet(string Url, string postDataStr)
         {
             HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
             request.Method = "GET";
             request.ContentType = "text/html;charset=UTF-8";
  
             HttpWebResponse response = (HttpWebResponse)request.GetResponse();
             Stream myResponseStream = response.GetResponseStream();
             StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
             string retString = myStreamReader.ReadToEnd();
             myStreamReader.Close();
             myResponseStream.Close();
  
             return retString;
         }
        /// <summary>
        /// 获取html
        /// </summary>
        /// <param name="getUrl"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static string GetHtml(string getUrl, CookieContainer cookieContainer, HttpHeader header)
        {
            //Thread.Sleep(1000);
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(getUrl);
                httpWebRequest.CookieContainer = cookieContainer;
                /*
                httpWebRequest.ContentType = header.contentType;
                httpWebRequest.ServicePoint.ConnectionLimit = header.maxTry;
                httpWebRequest.Referer = getUrl;
                httpWebRequest.Accept = header.accept;
                httpWebRequest.UserAgent = header.userAgent;*/
                httpWebRequest.ContentType = "text/html;charset=UTF-8";
                httpWebRequest.Method = "GET";
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                string html = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();
                httpWebRequest.Abort();
                httpWebResponse.Close();
                return html;
            }
            catch (Exception e)
            {
                if (httpWebRequest != null) httpWebRequest.Abort();
                if (httpWebResponse != null) httpWebResponse.Close();
                return string.Empty;
            }
        }



        /// <summary>
        /// 获取html
        /// </summary>
        /// <param name="getUrl"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static string POST(string url, CookieContainer cookieContainer, HttpHeader header, string data)
        {
            //Thread.Sleep(1000);
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                //httpWebRequest.ContentType = header.contentType;
                //httpWebRequest.ServicePoint.ConnectionLimit = header.maxTry;
                //httpWebRequest.Referer = url;
                //httpWebRequest.Accept = header.accept;
                //httpWebRequest.UserAgent = header.userAgent;
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                //POST数据
                byte[] postdatabyte = Encoding.UTF8.GetBytes(data);
                httpWebRequest.ContentLength = postdatabyte.Length;
                //httpWebRequest.AllowAutoRedirect = true; //是否跳转，需要设置，否则不执行php自动跳转看不到是否成功的页面
                //httpWebRequest.CookieContainer = cookieContainer;
                //httpWebRequest.KeepAlive = true;
                httpWebRequest.ProtocolVersion = HttpVersion.Version11;
                Stream stream;
                stream = httpWebRequest.GetRequestStream();
                stream.Write(postdatabyte, 0, postdatabyte.Length); //设置请求主体的内容
                stream.Close();

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                string html = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();
                httpWebRequest.Abort();
                httpWebResponse.Close();
                return html;
            }
            catch (Exception e)
            {
                if (httpWebRequest != null) httpWebRequest.Abort();
                if (httpWebResponse != null) httpWebResponse.Close();
                return string.Empty;
            }
        }

        public static List<Cookie> GetAllCookies(CookieContainer cc0)
        {
            List<Cookie> lstCookies = new List<Cookie>();
            Hashtable table = (Hashtable)cc0.GetType().InvokeMember("m_domainTable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
                System.Reflection.BindingFlags.Instance, null, cc0, new object[] { });
            foreach (object pathList in table.Values)
            {
                SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField
                    | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
                foreach (CookieCollection colCookies in lstCookieCol.Values)
                    foreach (Cookie c in colCookies) lstCookies.Add(c);
            }
            return lstCookies;
        }
        public static void SaveCookies(CookieContainer cc0)
        {
            StringBuilder sbc = new StringBuilder();
            List<Cookie> cooklist = GetAllCookies(cc0);
            foreach (Cookie cookie in cooklist)
            {
                sbc.AppendFormat("{0};{1};{2};{3};{4};{5}\r\n",
                    cookie.Domain, cookie.Name, cookie.Path, cookie.Port,
                    cookie.Secure.ToString(), cookie.Value);
            }
            FileStream fs = File.Create(@"chinarencookies.txt");
            fs.Close();
            File.WriteAllText(@"chinarencookies.txt", sbc.ToString(), System.Text.Encoding.Default);
            string[] cookies = File.ReadAllText(@"chinarencookies.txt", System.Text.Encoding.Default).Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string c in cookies)
            {
                string[] cc = c.Split(";".ToCharArray());
                Cookie ck = new Cookie(); ;
                ck.Discard = false;
                ck.Domain = cc[0];
                ck.Expired = true;
                ck.HttpOnly = true;
                ck.Name = cc[1];
                ck.Path = cc[2];
                ck.Port = cc[3];
                ck.Secure = bool.Parse(cc[4]);
                ck.Value = cc[5];
                cc0.Add(ck);
            }
        }

    }

    public class HttpHeader
    {
        public string contentType { get; set; }

        public string accept { get; set; }

        public string userAgent { get; set; }

        public string method { get; set; }

        public int maxTry { get; set; }
    }



}

