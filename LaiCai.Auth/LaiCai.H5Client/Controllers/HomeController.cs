using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LaiCai.H5Client.Controllers
{
    public class HomeController : Controller
    {
        public static IList<Token> tokenList = new List<Token>();
        /// <summary>
        /// 接入的应用ID
        /// </summary>
        private string clientId = "acai.com";
        /// <summary>
        /// 接入的secret
        /// </summary>
        private string clientSecret = "4fc8710733229acab11cc1d4efb3f0";

        /// <summary>
        /// 用户登陆地址
        /// </summary>
        private string userUrl = "http://auth.api.com/api/user";
        /// <summary>
        /// 令牌地址
        /// </summary>
        private string tokenUrl = "http://auth.api.com/api/token";
        /// <summary>
        /// 测试webapi地址
        /// </summary>
        private string apiUrl = "http://shop.api.com/api/shop";
        /// <summary>
        /// 访问aspx的api接口
        /// </summary>
        private string oldApiUrl = "http://old.api.com/api.aspx";
        /// <summary>
        /// 应用地址
        /// </summary>
        private string clientUrl = "http://auth.api.com/api/client";

        /// <summary>
        /// 用户登陆获取令牌信息
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //查找static list是否存在令牌相关信息                  
            if (Request.Cookies["sign"]!=null&&!string.IsNullOrEmpty(Request.Cookies["sign"].Value))
            {
                string cookieValue = Request.Cookies["sign"].Value;
                var tokenInfo = tokenList.Where(c => c.sign == cookieValue).FirstOrDefault();
                if (tokenInfo != null)
                    ViewBag.Result = JsonConvert.SerializeObject(tokenInfo);
            }
            else
                ViewBag.Result = "无令牌信息，请先登陆初始化令牌";
            ViewBag.ClientId = clientId;
            ViewBag.ClientSecret = clientSecret;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(FormCollection form)
        {
            string userName = form["username"];
            string password = form["password"];
            //用户登陆获取令牌
            Dictionary<string, string> loginDict = new Dictionary<string, string>();
            loginDict.Add("client_id", clientId);
            loginDict.Add("client_secret", clientSecret);
            loginDict.Add("user_name", userName);
            loginDict.Add("password", password);
            var result = await HttpToServer(userUrl, JsonConvert.SerializeObject(loginDict), "post", "json", "", null, "utf-8");
            SaveTokenOnServer(result);
            ViewBag.Result = result.Item2;
            return View();
        }
        /// <summary>
        /// 将令牌保存至static list中
        /// 实际应用中可以根据情况保存至缓存或数据库中
        /// </summary>
        /// <param name="result"></param>
        private void SaveTokenOnServer(Tuple<int, string> result)
        {
            if (result.Item1 == 200)
            {
                var jObject = JsonConvert.DeserializeObject<JObject>(result.Item2);
                var objCode = jObject["code"] as object;
                if (objCode == null) //有返回令牌信息
                {
                    string accessToken = (string)jObject["access_token"];
                    DateTime expireTime = (DateTime)jObject["expire_time"];
                    string refreshToken = (string)jObject["refresh_token"];
                    DateTime refreshExpireTime = (DateTime)jObject["refresh_expire_time"];
                    string clientId = (string)jObject["client_id"];
                    string userId = (string)jObject["user_id"];
                    Token tokenInfo = new Token();
                    tokenInfo.accessToken = accessToken;
                    tokenInfo.expireTime = expireTime;
                    tokenInfo.refreshToken = refreshToken;
                    tokenInfo.refreshExpireTime = refreshExpireTime;
                    tokenInfo.userId = userId;
                    tokenInfo.clientId = clientId;
                    tokenInfo.sign = tokenInfo.clientId + "_" + tokenInfo.userId;
                    //保证一个应用及一个用户只保存一个令牌信息，以免tokenList中的值过多
                    var item = tokenList.Where(c => c.sign==tokenInfo.sign).FirstOrDefault();
                    if (item != null)
                        tokenList.Remove(item);
                    //令牌信息持久化,示例中保存在static变量中,其他的可以保存在cookie或缓存中
                    tokenList.Add(tokenInfo);

                    //将凭证保存在客户端，下次再取时以此凭证在tokenList查找,
                    HttpCookie cookie = new HttpCookie("sign");
                    cookie.Value = tokenInfo.sign; //为了安全期间可以加密或保存其他内容,保证通过cookie能查找到对应的tokenList中的令牌
                    cookie.Expires = expireTime.AddHours(3);
                    Response.Cookies.Add(cookie);
                }
            }
        }

        /// <summary>
        /// 获取令牌ID
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetAccessTokenId()
        {
            if (Request.Cookies["sign"] == null || string.IsNullOrEmpty(Request.Cookies["sign"].Value))
            {
                return "";
            }
            string cookieValue = Request.Cookies["sign"].Value;
            var tokenInfo = tokenList.Where(c => c.sign == cookieValue).FirstOrDefault();
            if (tokenInfo != null)
            {
                if (DateTime.Now.AddMinutes(-3) > tokenInfo.expireTime)//为了避免在多次调用api中，出现令牌过期的情况，在令牌失效前的3分钟，取新的令牌
                {
                    IDictionary<string, string> loginDict = new Dictionary<string, string>();
                    loginDict.Add("client_id", clientId);
                    loginDict.Add("client_secret", clientSecret);
                    loginDict.Add("token_id", tokenInfo.refreshToken);
                    loginDict.Add("method", "refresh");//刷新令牌换新的访问令牌
                    //loginDict.Add("method", "token"); //用当前有效的令牌换新的访问令牌
                    var result1 = await HttpToServer(tokenUrl, JsonConvert.SerializeObject(loginDict), "post", "json", "", null, "utf-8");
                    SaveTokenOnServer(result1);
                }
                return tokenInfo.accessToken;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 换取新令牌
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> NewToken()
        {
            if (Request.Cookies["sign"]==null||string.IsNullOrEmpty(Request.Cookies["sign"].Value))
            {
                ViewBag.Result = "请先登陆初始化令牌";
                return View();
            }
            string cookieValue = Request.Cookies["sign"].Value;
            var tokenInfo = tokenList.Where(c => c.sign == cookieValue).FirstOrDefault();
            if (tokenInfo != null)
            {

                IDictionary<string, string> loginDict = new Dictionary<string, string>();
                loginDict.Add("client_id", clientId);
                loginDict.Add("client_secret", clientSecret);
                loginDict.Add("token_id", tokenInfo.refreshToken);
                loginDict.Add("method", "refresh");//刷新令牌换新的访问令牌
                /*用当前有效的令牌换新的访问令牌
                    loginDict.Add("token_id", tokenInfo.accessToken);
                    loginDict.Add("method", "token");
                 */
                var result1 = await HttpToServer(tokenUrl, JsonConvert.SerializeObject(loginDict), "post", "json", "", null, "utf-8");
                SaveTokenOnServer(result1);
                ViewBag.Result = result1.Item2;

            }
            else
                ViewBag.Result = "请先登陆初始化令牌";

            return View();
        }

        /// <summary>
        /// 调用api例子
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ShopApi()
        {
            string tokenId = await GetAccessTokenId();
            if(string.IsNullOrEmpty(tokenId))
            {
                ViewBag.Result = "无效，请重新取令牌";
                return View();
            }
            var result = await HttpToServer(apiUrl, "", "get", "JSON", tokenId, null, "utf-8");
            ViewBag.Result = result.Item2;
            return View();
        }

        /// <summary>
        /// 获取.net2.0的接口
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> OldShopApi()
        {
            string tokenId = await GetAccessTokenId();
            if (string.IsNullOrEmpty(tokenId))
            {
                ViewBag.Result = "无效，请重新取令牌";
                return View();
            }
            oldApiUrl = string.Format("{0}?method={1}", oldApiUrl, Request["method"]);

            if (Request["method"] == "GetClientInfo")
            {
                var result = await HttpToServer(oldApiUrl, "", "get", "JSON", tokenId, null, "utf-8");
                ViewBag.Result = result.Item2;
            }
            else
            {
                var result = await HttpToServer(oldApiUrl, "", "get", "JSON", "", null, "utf-8");
                ViewBag.Result = result.Item2;
            }
            return View();

        }

        /// <summary>
        /// 应用直连服务器
        /// </summary>
        /// <returns></returns>
        public ActionResult ClientRedirect()
        {

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ClientRedirect(FormCollection form)
        {
            string clientId = form["clientId"];
            string clientSecret = form["clientSecret"];
            IDictionary<string, string> postDict = new Dictionary<string, string>();
            postDict.Add("client_id", clientId);
            postDict.Add("client_secret", clientSecret);
            var result = await HttpToServer(clientUrl, JsonConvert.SerializeObject(postDict), "post", "json", "", null, "utf-8");
            SaveTokenOnServer(result);
            ViewBag.Result = result.Item2;

            return View();
        }


        public Task<Tuple<int, string>> HttpToServer(string url, string sendContent, string method, string contentType, string token, IDictionary<string, string> headerDict, string encoding, int timeout = 10)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            StreamReader reader = null;
            HttpWebResponse response = null;
            request.Method = method.ToString();
            request.Timeout = timeout * 1000;
            //request.ReadWriteTimeout = timeout * 1000*2;
            switch (contentType.ToUpper())
            {
                case "FORM":
                    request.ContentType = "application/x-www-form-urlencoded";
                    break;
                case "JSON":
                    request.ContentType = "application/json";
                    break;
                case "XML":
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
                    else if (paires.Key.ToLower() == "authorization" && !string.IsNullOrEmpty(token))
                    {
                        throw new Exception("header 中authorization与token的Authorization重复,只能保留一个");
                    }
                    else
                        request.Headers.Add(paires.Key, paires.Value);
                }
            }
            if (!string.IsNullOrEmpty(token))
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
                return Task.FromResult(Tuple.Create<int, string>(200, sb.ToString()));
            }
            catch (WebException e)
            {
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
    }

    public class Token
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        public string accessToken { set; get; }

        /// <summary>
        /// 令牌过期时间
        /// </summary>
        public DateTime expireTime { set; get; }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string refreshToken { set; get; }
        /// <summary>
        /// 刷新令牌过期时间
        /// </summary>
        public DateTime refreshExpireTime { set; get; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { set; get; }

        /// <summary>
        /// 应用ID
        /// </summary>
        public string clientId { set; get; }

        /// <summary>
        /// 与客户端保存关系凭证
        /// </summary>
        public string sign { set; get; }
    }
}
