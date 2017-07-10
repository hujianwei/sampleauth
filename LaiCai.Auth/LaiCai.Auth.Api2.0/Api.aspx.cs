using System;
using System.Collections.Generic;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace LaiCai.Auth.Api.net20
{
   
    public partial class Api : System.Web.UI.Page
    {
        /// <summary>
        /// 认证的用户ID
        /// </summary>
        private string userId = "";
        /// <summary>
        /// 认证的clientID
        /// </summary>
        private string clientId = "";

        private string accessTokenId = "";
        private string authUrl = System.Configuration.ConfigurationManager.AppSettings["AuthUrl"];

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "application/json";

            ValidRequestHeader();

            string methodName = Request["method"];
            if (!string.IsNullOrEmpty(methodName))
            {
                try
                {
                    MethodInfo memberInfo = this.GetType().GetMethod(methodName);
                    memberInfo.Invoke(this, null);
                }
                catch(Exception ex)
                {
                    Response.Write(ex.Message);
                }
            }
            Response.End();


        }

        public void GetUserInfo()
        {
            IDictionary<string, string> userDict = new Dictionary<string, string>();
            userDict.Add("username", "测试");
            userDict.Add("password", "密码");
            Response.Write(JsonConvert.SerializeObject(userDict));
            
        }

       
        public void GetClientInfo()
        {
            IDictionary<string, string> clientDict = new Dictionary<string, string>();
            clientDict.Add("client", "来彩手机网站");
            clientDict.Add("url", "m.laicai88.cn");
            clientDict.Add("access_token", accessTokenId);
            clientDict.Add("认证中心返回userId", userId);
            clientDict.Add("认证中心返回clientId", clientId);
            Response.Write(JsonConvert.SerializeObject(clientDict));
        }

        /// <summary>
        /// 初始化请求，获取令牌的合法性
        /// </summary>
        private void ValidRequestHeader()
        {
            string authInfo = Request.Headers["Authorization"];
            if(!string.IsNullOrEmpty(authInfo))
            {
                string bearerKey = "bearer ";
                if (!authInfo.Contains(bearerKey))
                {
                    Response.StatusCode = 401;
                    Response.Write("未授权访问");
                    Response.End();
                }
                accessTokenId = authInfo.Substring(bearerKey.Length);
                string url = authUrl + accessTokenId;
                string result = this.HttpToServer(url, "get", null);
                if (string.IsNullOrEmpty(result))
                {
                    Response.StatusCode = 401;
                    Response.Write("未授权访问");
                    Response.End();
                }
                try
                {
                    var jObject = JsonConvert.DeserializeObject<JObject>(result);
                    string strToken = (string)jObject["access_token"];
                    DateTime expireTime = (DateTime)jObject["expire_time"];
                    clientId = (string)jObject["client_id"];
                    userId = (string)jObject["user_id"];
                    if (expireTime < DateTime.Now)
                    {
                        Response.StatusCode = 401;
                        Response.Write("未授权访问");
                        Response.End();
                    }                    
                }
                catch (Exception ex)
                {
                    Response.StatusCode = 401;
                    Response.Write(ex.Message);
                    Response.End();
                }
            }
            else
            {
                Response.StatusCode = 401;
                Response.Write("未授权访问");
                Response.End();
            }
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
        
        private string HttpToServer(string url, string method, IDictionary<string, string> headerDict, string encoding = "utf-8", int timeout = 10)
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

    public class BasePage : System.Web.UI.Page
    {
        public string userId = "";
        public string clientId = "";
        public BasePage()
        {
            this.Load += new EventHandler(BasePage_Load);
        }
        void BasePage_Load(object sender, EventArgs e)
        {
            userId = "10212";
            clientId = "3654566";
        }

    }

    public class Login: Attribute
    {
       

        public Login(BasePage page)
        {
            if(string.IsNullOrEmpty(page.userId))
            {
                IDictionary<string, string> resultDict = new Dictionary<string, string>();
                resultDict.Add("code", "4001");
                resultDict.Add("message", "未授权访问");
                page.Response.StatusCode = 401;
                page.Response.Write(JsonConvert.SerializeObject(resultDict));
                page.Response.End();
            }
        }

    }
}