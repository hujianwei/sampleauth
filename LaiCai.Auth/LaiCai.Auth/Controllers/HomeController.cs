using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LaiCai.Auth.IServices;
using LaiCai.Auth.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace LaiCai.Auth.Controllers
{
    public class HomeController : Controller
    {
        private IRequestHelper _helper = null;
        private ICache _cache = null;
        public HomeController(IRequestHelper helper,ICache cache)
        {
            _cache = cache;
            _helper = helper;
        }


        public async Task<ActionResult> Index()
        {
            //用户登陆获取令牌
            Dictionary<string, string> loginDict = new Dictionary<string, string>();
            loginDict.Add("client_id", "acai.com");
            loginDict.Add("client_secret", "4fc8710733229acab11cc1d4efb3f0");
            //loginDict.Add("user_name", "1005");
            //loginDict.Add("password", "5241");
            //var result = await _helper.HttpToServer("http://auth.api.com/api/user", JsonConvert.SerializeObject(loginDict), RequestMethod.POST, ContentType.JSON, "", null, "utf-8");


            //获取令牌
            //var result = await _helper.HttpToServer("http://auth.api.com/api/token/f760d52b409d6643765e895e7c38", "", RequestMethod.GET, ContentType.JSON, "", null, "utf-8");

            //换取新令牌
            loginDict.Add("token_id", "b2b0e4851f1ee9ffcf8347ee4a5339");
            loginDict.Add("method", "refresh");
            var result = await _helper.HttpToServer("http://auth.api.com/api/token", JsonConvert.SerializeObject(loginDict), RequestMethod.POST, ContentType.JSON, "", null, "utf-8");

            //获取用户信息
            //Dictionary<string, string> headerDict = new Dictionary<string, string>();
            //headerDict.Add("client_id", "acai.com");
            //headerDict.Add("client_secret", "4fc8710733229acab11cc1d4efb3f0");
            //var result = await _helper.HttpToServer("http://auth.api.com/api/user/1007", "", RequestMethod.GET, ContentType.JSON, "6e3374846bcf65a6cdcb1f49f82cd51", headerDict, "utf-8");

            Response.Write(result.Item2);
            Response.End();

            return View();
        }


        public async Task<ActionResult> Shop()
        {
            Dictionary<string, DateTime> dict = new Dictionary<string, DateTime>();
            dict.Add("regtime", DateTime.Now);
            dict.Add("borthday", new DateTime(1984, 5, 5));
            _cache.SetAll<DateTime>(dict);
            Response.Write(_cache.Get<DateTime>("borthday"));
            Response.End();

            var result = await _helper.HttpToServer("http://shop.api.com/api/shop", "", RequestMethod.GET, ContentType.JSON, "6d4c4635db41f77abbd11693c11cb", null, "utf-8");
            Response.Write(result.Item2);
            Response.End();
            return View();
        }

        public ActionResult CacheTest()
        {
            Dictionary<string, DateTime> dict = new Dictionary<string, DateTime>();
            dict.Add("regtime", DateTime.Now);
            dict.Add("borthday", new DateTime(1984, 5, 5));
            _cache.SetAll<DateTime>(dict);

            var tt = System.Runtime.Caching.MemoryCache.Default;

            Response.Write(_cache.Get<DateTime>("borthday"));
            Response.End();
            return View();
        }
    }
}
