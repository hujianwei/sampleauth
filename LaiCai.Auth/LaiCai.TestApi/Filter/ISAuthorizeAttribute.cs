using System;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace LaiCai.TestApi.Filter
{
    public class ISAuthorizeAttribute:AuthorizationFilterAttribute
    {
        private string authUrl = System.Configuration.ConfigurationManager.AppSettings["AuthUrl"];


        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {

            var authInfo = actionContext.Request.Headers.Authorization;
            if (authInfo == null)
                Unauthorized(actionContext);
            else if (authInfo.Scheme != "bearer")
                Unauthorized(actionContext);
            else
            {
                string tokenId = authInfo.Parameter;
                string url = authUrl + tokenId;
                string result =  this.HttpToServer(url, "get", null);
                if(string.IsNullOrEmpty(result))
                    Unauthorized(actionContext);
                try
                {
                    var jObject = JsonConvert.DeserializeObject<JObject>(result);
                    string strToken = (string)jObject["access_token"];
                    DateTime expireTime = (DateTime)jObject["expire_time"];
                    string clientId = (string)jObject["client_id"];
                    string userId = (string)jObject["user_id"];
                    if(expireTime<DateTime.Now)
                        Unauthorized(actionContext);
                    var claims = new List<Claim>();
                    if (!string.IsNullOrEmpty(clientId))
                        claims.Add(new Claim("client_id", clientId));
                    if (!string.IsNullOrEmpty(userId))
                        claims.Add(new Claim("user_id", userId));
                    var id = new ClaimsIdentity(claims);
                    var Principal = new ClaimsPrincipal(id);
                    actionContext.RequestContext.Principal = Principal;



                }
                catch(Exception ex)
                {
                    Unauthorized(actionContext);
                }
            }
            return  base.OnAuthorizationAsync(actionContext, cancellationToken) ;
        }

        void Unauthorized(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized,"未授权");
        }

 
        /// <summary>
        /// 向服务器发送请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sendContent"></param>
        /// <param name="method"></param>
        /// <param name="headerDict"></param>
        /// <param name="encoding"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private string HttpToServer(string url, string method,IDictionary<string,string> headerDict, string encoding="utf-8", int timeout = 10)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            StreamReader reader = null;
            HttpWebResponse response = null;
            request.Method = method.ToString();
            request.Timeout = timeout * 1000;
            request.ContentType = "application/json";
  
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
                response = (HttpWebResponse)request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                StringBuilder sb = new StringBuilder();
                char[] bufChar = new char[4096];
                int count = 0;
                while ((count = reader.Read(bufChar, 0, bufChar.Length)) > 0)
                {
                    sb.Append(new String(bufChar, 0, count));
                }
                return sb.ToString();
            }
            catch (WebException e)
            {
                throw e;
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