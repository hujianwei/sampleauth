using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LaiCai.TestApi.Filter;

namespace LaiCai.TestApi.Controllers
{
    public class ShopController : ApiController
    {
        [ISAuthorize]
        public HttpResponseMessage Get()
        {
            IDictionary<string, string> shopInfo = new Dictionary<string, string>();
            shopInfo.Add("name", "彩票一号");
            shopInfo.Add("address", "文一西路89号");
            shopInfo.Add("phone", "0571-67798457");
            shopInfo.Add("cantact","大宋");
            var user = User as System.Security.Claims.ClaimsPrincipal;
            var claim = user.Claims.Where(c => c.Type == "client_id").FirstOrDefault();
            if (claim != null)
                shopInfo.Add("认证中心读取的接入应用ID", claim.Value);
            claim = user.Claims.Where(c => c.Type == "user_id").FirstOrDefault();
            if (claim != null)
                shopInfo.Add("请求用户ID", claim.Value);
            if (Request.Headers.Authorization != null)
                shopInfo.Add("令牌ID", Request.Headers.Authorization.Parameter);
            return Request.CreateResponse(HttpStatusCode.OK, shopInfo);
        }
    }
}
