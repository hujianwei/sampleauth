using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace LaiCai.Auth.Models
{
    public class TokenItem
    {
        /// <summary>
        /// 令牌ID
        /// </summary>
        [JsonProperty("access_token")]
        public string TokenId { set; get; }
        /// <summary>
        /// 过期时间
        /// </summary>
        [JsonProperty("expire_time")]
        public DateTime ExpireTime { set; get; }
        /// <summary>
        /// 刷新令牌ID
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshTokenId { set; get; }
        /// <summary>
        /// 刷新令牌到期时间
        /// </summary>
        [JsonProperty("refresh_expire_time")]
        public DateTime? RefreshExpireTime { set; get; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [JsonProperty("user_id")]
        public string UserId { set; get; }
        /// <summary>
        /// 应用ID
        /// </summary>
        [JsonProperty("client_id")]
        public string ClientId { set; get; }

        /// <summary>
        /// 执行结果
        /// </summary>
        [JsonIgnore]
        public ResultItem Result { set; get; }


    }
}