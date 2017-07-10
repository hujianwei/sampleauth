using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using LaiCai.Auth.IServices;
using LaiCai.Auth.Models;
using System.Net.Http.Headers;

namespace LaiCai.Auth.Controllers
{
    public class UserController : BaseApiController
    {
        private IToken _token = null;
        private IUser _user = null;
        public UserController(IUser user,IToken token)
        {
            _user = user;
            _token = token;
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CustomActionResult> Get(string id)
        {

            var authInfo = Request.Headers.Authorization;
            if(authInfo==null)
            {
                return new CustomActionResult(4001, "非法访问", null, HttpStatusCode.BadRequest, Request);
            }
            string tokenId = authInfo.Parameter;
            var tokenInfo = await _token.GetToken(tokenId);
            if(tokenInfo==null)
            {
                return new CustomActionResult(4001, "令牌不存在", null, Request);
            }
            else if(tokenInfo.Result!=null&&!tokenInfo.Result.IsSuccess)
            {
                return new CustomActionResult(4001, tokenInfo.Result.Message, null, Request);
            }
            else if(string.IsNullOrEmpty(tokenInfo.UserId))
            {
                return new CustomActionResult(4001, "令牌不存在用户信息", null, Request);
            }
            else if(!tokenInfo.UserId.Equals(id))
            {
                return new CustomActionResult(4001, "非法访问", null, HttpStatusCode.OK, Request);
            }
            var userInfo = await _user.GetUserById(id);
            if (userInfo == null)
                return new CustomActionResult(4001, "用户不存在", null, HttpStatusCode.OK, Request);
            return new CustomActionResult(userInfo, Request);
        }

        /// <summary>
        /// 用户登陆获取令牌
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public async Task<CustomActionResult> Post(IDictionary<string,string> dict)
        {
            if (dict == null || dict.Count != 4)
                return new CustomActionResult(4001, "参数错误", null, Request);
            if(!dict.ContainsKey("client_id"))
                return new CustomActionResult(4001, "参数错误", null, Request);
            else if(!dict.ContainsKey("client_secret"))
                return new CustomActionResult(4001, "参数错误", null, Request);
            else if(!dict.ContainsKey("user_name"))
                return new CustomActionResult(4001, "参数错误", null, Request);
            else if(!dict.ContainsKey("password"))
                return new CustomActionResult(4001, "参数错误", null, Request);
            RequstAuthContext context = new RequstAuthContext();
            context.clientId = dict["client_id"];
            context.clientSecret = dict["client_secret"];
            context.Params.Add("user_name", dict["user_name"]);
            context.Params.Add("password", dict["password"]);
            var result = await _token.CreateToken(context);
            if (result != null && result.Result != null && result.Result.IsSuccess)
                return new CustomActionResult(result, Request);
            else
                return new CustomActionResult(4000, result.Result.Message, null, Request);
        }
    }
}
