using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Web.Http;
using LaiCai.Auth.IServices;

namespace LaiCai.Auth.Implement
{
    public class HttpRequestHelper : IRequestHelper
    {


 

        public HttpRequestHelper()
        {
        }

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
        /// 返回Tuple<int, string,string>数据,T1:状态码,T2:string服务端返回的结果
        /// </returns>
        public async Task<Tuple<int, string>> HttpToServer(string url, string sendContent, RequestMethod method, ContentType contentType, string token, IDictionary<string, string> headerDict, string encoding, int timeout = 10)
        {
            var result = await HttpToServer(url, sendContent, method, contentType, token, headerDict, encoding, timeout, false);
            return Tuple.Create<int, string>(result.Item1, result.Item2);
        }

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
        /// 返回Tuple<int, string,string>数据,T1:状态码,T2:string服务端返回的结果,T3:服务器响应的cookie
        /// </returns>
        public Task<Tuple<int, string,string>> HttpToServer(string url, string sendContent, RequestMethod method, ContentType contentType, string token, IDictionary<string, string> headerDict, string encoding, int timeout = 10, bool responseCookie = true)
        {

            System.Net.ServicePointManager.DefaultConnectionLimit = 512;
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
                    else if(paires.Key.ToLower()== "authorization"&&!string.IsNullOrEmpty(token))
                    {
                        throw new Exception("header 中authorization与token的Authorization重复,只能保留一个");
                    }
                    else
                        request.Headers.Add(paires.Key, paires.Value);
                }
            }
            if(!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", string.Format("bearer {0}", token));
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
                var cookie = "";
                if (responseCookie)
                    cookie = response.Headers.Get("Set-Cookie");
                return Task.FromResult(Tuple.Create<int, string,string>(200, sb.ToString(), cookie));
            }
            catch (WebException e)
            {
                return Task.FromResult(Tuple.Create<int, string,string>(500, e.Message,""));
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

    }
}