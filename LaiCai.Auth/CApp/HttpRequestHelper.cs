using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace CApp
{
    public class HttpRequestHelper 
    {





        //属性注入
        //public ICache cache { set; get; }

        //public ILog log { set; get; }
        /// <summary>
        /// 向服务器提交数据并返回对应的结果
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sendContent"></param>
        /// <param name="method"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <param name="headerDict"></param>
        /// <param name="encoding"></param>
        /// <returns>
        /// 返回Tuple<string,HttpStatusCode>数据,T1:string服务端返回的结果,T2:HttpStatusCode 返回的状态码
        /// </returns>
        public static Task<Tuple<int, string>> HttpToServer(string url, string sendContent, HttpMethod method, ContentType contentType, string token, IDictionary<string, string> headerDict, string encoding, int timeout = 10)
        {

            System.Net.ServicePointManager.DefaultConnectionLimit = 512;
            System.Net.ServicePointManager.Expect100Continue = false;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            StreamReader reader = null;
            HttpWebResponse response = null;
            request.Method = method.ToString();
            request.Timeout = timeout * 1000;
            //request.ReadWriteTimeout = timeout * 1000*2;
            switch (contentType)
            {
                case ContentType.FORM:
                    request.ContentType = "application/x-www-form-urlencoded";
                    break;
                case ContentType.JSON:
                    request.ContentType = "application/json";
                    break;
                case ContentType.XML:
                    request.ContentType = "application/xml ";
                    break;
            }
            if (headerDict != null && headerDict.Count > 0)
            {
                foreach (var paires in headerDict)
                {
                    if (paires.Key.ToLower() == "referer")
                        request.Referer = paires.Value;
                    else if (paires.Key.ToLower() == "user-agent")
                        request.UserAgent = paires.Value;
                    else
                        request.Headers.Add(paires.Key, paires.Value);
                }
            }

            try
            {
                if (!string.IsNullOrEmpty(sendContent))
                {
                    byte[] btBodys = Encoding.GetEncoding(encoding).GetBytes(sendContent);
                    request.ContentLength = btBodys.Length;
                    request.GetRequestStream().Write(btBodys, 0, btBodys.Length);
                }
                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                StringBuilder sb = new StringBuilder();
                char[] bufChar = new char[4096];
                int count = 0;
                while ((count = reader.Read(bufChar, 0, bufChar.Length)) > 0)
                {
                    sb.Append(new String(bufChar, 0, count));
                }
                //_log.Write(sb);
                //_cache.UpdateCache(DateTime.Now.Ticks.ToString()+"\r\n");
                return Task.FromResult(Tuple.Create<int, string>(200, sb.ToString()));
            }
            catch (WebException e)
            {
                //_cache.UpdateCache(e.Message);
                return Task.FromResult(Tuple.Create<int, string>(500, e.Message));
            }
            finally
            {
                try
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader = null;
                    }
                }
                catch { }
                try
                {
                    if (response != null)
                    {
                        response.Close();
                        response = null;
                    }
                }
                catch { }
                try
                {
                    if (request != null)
                    {
                        request.Abort();
                        request = null;
                    }
                }
                catch { }
            }

        }

        /// <summary>
        /// 从远程获取json的结果
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> GetJsonFromServer(string url)
        {
            var result = await HttpToServer(url, "", HttpMethod.GET, ContentType.JSON, "", null, "utf-8");
            if (result.Item1 != 200)
                return "";
            return await UnicodeToChinese(result.Item2);
        }

        /// <summary>
        /// unicode转中文字符
        /// </summary>
        /// <param name="unicodeStr">unicode字符串</param>
        /// <returns></returns>
        public Task<string> UnicodeToChinese(string unicodeStr)
        {

            string result = new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                          unicodeStr, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
            return Task.FromResult(result);
        }

        public IDictionary<string, string> ParseParams()
        {

            return null;
        }
        public enum HttpMethod
        {
            /// <summary>
            /// Get方法
            /// </summary>
            GET = 0,
            /// <summary>
            /// Post方法
            /// </summary>
            POST = 1,
            /// <summary>
            /// Put方法
            /// </summary>
            PUT = 2,
            /// <summary>
            /// Delete
            /// </summary>
            DELETE = 3
        }

        public enum ContentType
        {
            /// <summary>
            /// json数据类型
            /// </summary>
            JSON = 0,
            /// <summary>
            /// 表单数据
            /// </summary>
            FORM = 1,
            /// <summary>
            /// XML数据类型
            /// </summary>
            XML = 2
        }







    }
}
