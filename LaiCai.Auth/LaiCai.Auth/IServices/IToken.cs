using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LaiCai.Auth.Models;
using System.Threading.Tasks;


namespace LaiCai.Auth.IServices
{
    public interface IToken
    {
        /// <summary>
        /// 增加令牌
        /// </summary>
        /// <returns></returns>
        /// <param name="loginRequest">登陆信息(包括用户名，密码,clientId,clientSecret)</param>
        Task<TokenItem> CreateToken(RequstAuthContext loginRequest);

        /// <summary>
        /// 获取令牌信息
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        Task<TokenItem> GetToken(string tokenId);

        /// <summary>
        /// 用老令牌获取新的令牌信息(刷新令牌不变)
        /// </summary>
        /// <param name="oldTokenId"></param>
        /// <returns></returns>
        Task<TokenItem> UpdateToken(string clientId,string clientSecret, string oldTokenId);

        /// <summary>
        /// 通过刷新令牌获取访问令牌信息
        /// </summary>
        /// <param name="refreshTokenId"></param>
        /// <returns></returns>
        Task<TokenItem> UpdateTokenByRefresh(string clientId, string clientSecret, string refreshTokenId);
    }
}