﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using LaiCai.Auth.IServices;
using LaiCai.Auth.Models;
using LaiCai.Auth.Common;

namespace LaiCai.Auth.Controllers
{
    /// <summary>
    /// Client相关操作
    /// </summary>
    public class ClientController : BaseApiController
    {
        private IClient _client = null;
        private IToken _token = null;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="client"></param>
        /// <param name="token"></param>
        public ClientController(IClient client,IToken token)
        {
            _client = client;
            _token = token;
        }
        /// <summary>
        /// Get简单例子
        /// </summary>
        /// <returns></returns>
        public CustomActionResult Get()
        {
            return new CustomActionResult(true, Request);
        }

        /// <summary>
        /// 直连形式获取token
        /// </summary>
        /// <param name="dict">字典</param>
        /// <returns></returns>
        public async Task<CustomActionResult> Post(IDictionary<string,string> dict)
        {

            var checkResult =await this.ParamsContainKeys(dict,2, "client_id", "client_secret");
            if(!checkResult.IsSuccess)
                return new CustomActionResult(4001, checkResult.Message, null, Request);
            string clientID = dict["client_id"];
            string clientSecret = dict["client_secret"];
            var clientInfo = await _client.GetById(clientID);
            if (clientInfo == null || clientInfo.Status != 1)
                return new CustomActionResult(4000, "client不存在", null, Request);
            else if (clientInfo.ClientType != 1)
                return new CustomActionResult(4000, "client连接类型不对", null, Request);
            RequstAuthContext context = new RequstAuthContext();
            context.clientId = clientID;
            context.clientSecret = clientSecret;
            var result = await _token.CreateToken(context);
            if (result != null && result.Result != null && result.Result.IsSuccess)
                return new CustomActionResult(result, Request);
            else
                return new CustomActionResult(4000, result.Result.Message, null, Request);
        }

    }
}
