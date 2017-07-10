using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using LaiCai.Auth.IServices;
using LaiCai.Auth.Models;

namespace LaiCai.Auth.Controllers
{
    public class TokenController : BaseApiController
    {
        private IToken _token = null;
        public TokenController(IToken token)
        {
            _token = token;
        }

        //public async Task<CustomActionResult> Get()
        //{

        //    RequstAuthContext authContext = new RequstAuthContext();
        //    authContext.clientId = "app_laicai88_cn";
        //    authContext.clientSecret = "93ffb6119718d13dfc6ff3d6edec7f0";
        //    authContext.Params.Add("user_name", "1005");
        //    authContext.Params.Add("password", "5241");
        //    var result = await _token.AddToken(authContext);
        //    if (result != null && result.Result != null && result.Result.IsSuccess)
        //        return new CustomActionResult(result, Request);
        //    else
        //        return new CustomActionResult(4000, result.Result.Message, null,Request);
        //}

        /// <summary>
        /// 获取访问令牌
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CustomActionResult> Get(string id)
        {            

            var checkResult = await this.ParamsValidLength(32,id);
            if(!checkResult.IsSuccess)
                return new CustomActionResult(4001, checkResult.Message, null, Request);
            var result = await _token.GetToken(id);
            if (result != null && result.Result != null && result.Result.IsSuccess)
                return new CustomActionResult(result, Request);
            else
                return new CustomActionResult(4000, "令牌不存在或失效", null, Request);
        }
        /// <summary>
        /// 更新令牌
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public async Task<CustomActionResult> Post(IDictionary<string,string> dict)
        {
            var checkResult = await this.ParamsContainKeys(dict,4, "client_id", "client_secret", "token_id", "method");
            if (!checkResult.IsSuccess)
                return new CustomActionResult(4000, checkResult.Message, null, Request);
            checkResult = await this.ParamsValidLength(32, dict["client_secret"], dict["token_id"]);
            if (!checkResult.IsSuccess)
                return new CustomActionResult(4000, checkResult.Message, null, Request);
            string method = dict["method"];
            string clientId = dict["client_id"];
            string clientSecret = dict["client_secret"];
            string tokenId = dict["token_id"];
            if(method=="token")
            {
                var item = await _token.UpdateToken(clientId, clientSecret, tokenId);
                if (item.Result.IsSuccess)
                    return new CustomActionResult(item, Request);
                else
                    return new CustomActionResult(4000, item.Result.Message, null, Request);
            }
            else if(method=="refresh")
            {
                var item = await _token.UpdateTokenByRefresh(clientId, clientSecret, tokenId);
                if (item.Result.IsSuccess)
                    return new CustomActionResult(item, Request);
                else
                    return new CustomActionResult(4000, item.Result.Message, null, Request);
            }
            else
                return new CustomActionResult(4001, "参数错误", null, Request);
        }

    }
}
