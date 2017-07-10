using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using LaiCai.Auth.IServices;
using LaiCai.Auth.Models;
using LaiCai.Auth.Common;

namespace LaiCai.Auth.Implement
{
    /// <summary>
    /// 令牌相关实现
    /// </summary>
    public class TokenRepertory :BaseOperater,IToken
    {
        private IClient _client = null;
        private IUser _user = null;

        public TokenRepertory() { }

        public TokenRepertory(IClient client,IUser user)
        {
            _client = client;
            _user = user;
        }

        private async Task<ResultItem> Isvalid(RequstAuthContext request)
        {
            if (request == null)
                throw new Exception("请求对象错误");
            if (string.IsNullOrEmpty(request.clientId) || string.IsNullOrEmpty(request.clientSecret))
            {
                return new ResultItem(false, "clientID/clientSecret错误");
            }
            if(request.clientSecret.Length!=32)
            {
                return new ResultItem(false, "clientSecret长度错误");
            }
            var clientInfo = await _client.GetById(request.clientId);
            if (clientInfo == null)
                return new ResultItem(false, "client不存在");
            else if (clientInfo.Secret != request.clientSecret)
                return new ResultItem(false, "client_id或者client_secret错误");
            request.Result = clientInfo;
            return new ResultItem(true, "");

        }

        /// <summary>
        /// 增加令牌信息
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        public async Task<TokenItem> CreateToken(RequstAuthContext request)
        {           
            TokenItem item = new TokenItem();
            UserItem userInfo = null;
            string userName, password;
            //登陆信息判断
            var checkResult = await Isvalid(request);
            if(!checkResult.IsSuccess)
            {
                item.Result = checkResult;
                return item;
            }
            var curDateTime = DateTime.Now;
            var clientInfo = request.Result as ClientItem;
            item.TokenId = Unity.GetTokenId();
            item.ExpireTime = curDateTime.AddSeconds(clientInfo.TokenInterval);
            item.ClientId = clientInfo.ClientId;
            if (clientInfo.ClientType==2)  //用户登陆方式
            {
                if(!request.Params.ContainsKey("user_name"))
                {
                    item.Result = new ResultItem(false, "user_name错误");
                    return item;
                }
                if(!request.Params.ContainsKey("password"))
                {
                    item.Result = new ResultItem(false, "password错误");
                    return item;
                }
                userName = request.Params["user_name"];
                password = request.Params["password"];
                if(string.IsNullOrEmpty(userName))
                {
                    item.Result = new ResultItem(false, "账号空值");
                    return item;
                }
                if(string.IsNullOrEmpty(password))
                {
                    item.Result = new ResultItem(false, "密码空值");
                    return item;
                }
                userInfo = new UserItem();
                userInfo.UserName = userName;
                userInfo.Password = password;
                await _user.Check(userInfo);
                if(!userInfo.Result.IsSuccess)
                {
                    item.Result = new ResultItem(false, userInfo.Result.Message);
                    return item;
                }
                item.UserId = userInfo.UserId;                
            }
            //是否启用刷新令牌
            RefreshTokenItem refreshToken = null;
            if (clientInfo.EnableRefreshToken)
            {
                item.RefreshTokenId = Unity.GetTokenId();
                if (clientInfo.RefreshTokenInterval > 0)
                    item.RefreshExpireTime = curDateTime.AddSeconds(clientInfo.RefreshTokenInterval);
                //刷新令牌更新保存库
                refreshToken = new RefreshTokenItem();
                refreshToken.TokenId = item.RefreshTokenId;
                refreshToken.ExpireTime = item.RefreshExpireTime;
                refreshToken.Client = clientInfo;
                refreshToken.User = userInfo;
                if(refreshToken.ExpireTime==null)
                    Cache.Set(CacheKeyFormat.TokenFormat.GetTokenKey(refreshToken.TokenId), refreshToken);
                else
                    Cache.Set(CacheKeyFormat.TokenFormat.GetTokenKey(refreshToken.TokenId), refreshToken, Convert.ToDateTime(refreshToken.ExpireTime));
            }
            Cache.Set(CacheKeyFormat.TokenFormat.GetTokenKey(item.TokenId), item, item.ExpireTime);
            UpdateCache(request.clientId, item.UserId, item, refreshToken);
            item.Result = new ResultItem(true, "");
            return item;

        }

        /// <summary>
        /// 更新用户对应的令牌缓存
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="refreshToken"></param>
        private void UpdateCache(string clientId,string userId,TokenItem token,RefreshTokenItem refreshToken)
        {
            if (token == null && refreshToken == null)
                return;
            string key = CacheKeyFormat.UserFormat.GetClientUserKey(clientId, userId);
            var tokenDict = Cache.Get<IDictionary<string, string>>(key);
            if (tokenDict == null)
                tokenDict = new Dictionary<string, string>();
            string oldTokeIdKey = "";
            string oldRefreshIdKey = "";
            if (token != null) {
                if (tokenDict.ContainsKey("token"))
                {
                    oldTokeIdKey = CacheKeyFormat.TokenFormat.GetTokenKey(tokenDict["token"]);
                    tokenDict["token"] = token.TokenId;
                }
                else
                    tokenDict.Add("token", token.TokenId);
            }
            if (refreshToken != null)
            {
                if (tokenDict.ContainsKey("refresh"))
                {
                    oldRefreshIdKey = CacheKeyFormat.TokenFormat.GetTokenKey(tokenDict["refresh"]);
                    tokenDict["refresh"] = refreshToken.TokenId;
                }
                else
                    tokenDict.Add("refresh", refreshToken.TokenId);
            }
           
            //删除旧的令牌信息
            Cache.Del(oldTokeIdKey, oldRefreshIdKey);
            Cache.Set(key, tokenDict);
        }

        /// <summary>
        /// 获取令牌信息
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        public Task<TokenItem> GetToken(string tokenId)
        {
            string key = CacheKeyFormat.TokenFormat.GetTokenKey(tokenId);
            var tokenItem = Cache.Get<TokenItem>(key);
            if (tokenItem == null)
                return Task.FromResult(tokenItem);
            if (tokenItem.ExpireTime < DateTime.Now)
                tokenItem.Result = new ResultItem(false, "令牌已过期");
            else
                tokenItem.Result = new ResultItem(true, "");
            return Task.FromResult(tokenItem);
        }

        /// <summary>
        /// 用老令牌获取新的令牌信息(刷新令牌不变)
        /// </summary>
        /// <param name="oldTokenId"></param>
        /// <returns></returns>
        public async Task<TokenItem> UpdateToken(string clientId, string clientSecret, string oldTokenId)
        {
            TokenItem newToken = new TokenItem();                     
            var oldToken = await GetToken(oldTokenId);
            if(oldToken==null)
            {
                newToken.Result = new ResultItem(false, "令牌已过期或不存在");
                return newToken;
            }
            else
            {
                var curDateTime = DateTime.Now;
                var clientInfo = await _client.GetById(oldToken.ClientId);
                if(clientInfo==null||clientInfo.Status!=1)
                {
                    throw new Exception("接入应用不存在或未启用");
                }   
                else if(clientId!=clientInfo.ClientId||clientSecret!=clientInfo.Secret)
                {
                    throw new Exception("非法错误");
                }
                newToken.TokenId = Unity.GetTokenId();
                newToken.ExpireTime = curDateTime.AddSeconds(clientInfo.TokenInterval);
                newToken.ClientId = oldToken.ClientId;
                newToken.RefreshTokenId = oldToken.RefreshTokenId;
                newToken.RefreshExpireTime = oldToken.RefreshExpireTime;
                newToken.UserId = oldToken.UserId;

                Cache.Del(CacheKeyFormat.TokenFormat.GetTokenKey(oldTokenId));
                Cache.Set(CacheKeyFormat.TokenFormat.GetTokenKey(newToken.TokenId), newToken, newToken.ExpireTime);
                string key = CacheKeyFormat.UserFormat.GetClientUserKey(oldToken.ClientId, oldToken.UserId);
                var tokenDict = Cache.Get<IDictionary<string,string>>(key);
                if(tokenDict!=null)
                {
                    if (tokenDict.ContainsKey("token"))
                        tokenDict["token"] = newToken.TokenId;
                    else
                        tokenDict.Add("token", newToken.TokenId);
                    Cache.Set(key, tokenDict);
                }

                return newToken;

            }           
        }
        /// <summary>
        /// 使用刷新令牌获取令牌信息
        /// </summary>
        /// <param name="refreshTokenId"></param>
        /// <returns></returns>
        public async Task<TokenItem> UpdateTokenByRefresh(string clientId, string clientSecret,string refreshTokenId)
        {
            TokenItem item = new TokenItem();
            var refreshToken = Cache.Get<RefreshTokenItem>(CacheKeyFormat.TokenFormat.GetTokenKey(refreshTokenId));
            if(refreshToken == null)
            {
                item.Result = new ResultItem(false, "刷新令牌不存在");
                return item;
            }
            else if(refreshToken.ExpireTime!=null&& refreshToken.ExpireTime<DateTime.Now)
            {
                item.Result = new ResultItem(false, "刷新令牌过期无效");
                return item;
            }
            var clientInfo = await _client.GetById(clientId);
            if(clientInfo==null||clientInfo.Status!=1)
            {
                throw new Exception("接入应用不存在或未启用");
            }
            if(!clientInfo.Equals(refreshToken.Client))
            {
                throw new Exception("非法访问");
            }
            item.TokenId = Unity.GetTokenId();
            item.ExpireTime = DateTime.Now.AddSeconds(clientInfo.TokenInterval);
            item.ClientId = clientInfo.ClientId;
            item.RefreshExpireTime = refreshToken.ExpireTime;
            item.RefreshTokenId = refreshToken.TokenId;
            if(clientInfo.ClientType==2)
            {
                item.UserId = refreshToken.User.UserId;
            }
            string clientUserKey = CacheKeyFormat.UserFormat.GetClientUserKey(item.ClientId, item.UserId);
            var tokenDict = Cache.Get<IDictionary<string,string>>(clientUserKey);
            if (tokenDict != null)
            {
                if (tokenDict.ContainsKey("token"))
                {
                    Cache.Del(CacheKeyFormat.TokenFormat.GetTokenKey(tokenDict["token"]));
                    tokenDict["token"] = item.TokenId;
                }
                else
                    tokenDict.Add("token", item.TokenId);
                Cache.Set(clientUserKey, tokenDict);
            }
            Cache.Set(CacheKeyFormat.TokenFormat.GetTokenKey(item.TokenId), item, item.ExpireTime);
            item.Result = new ResultItem(true, "");
            return item;
        }
    }
}