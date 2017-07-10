using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace LaiCai.Auth.Models
{
    /// <summary>
    /// 刷新令牌信息
    /// </summary>
    public class RefreshTokenItem
    {
        /// <summary>
        /// 令牌ID
        /// </summary>
        [JsonProperty("refresh_token")]
        public string TokenId { set; get; }
        /// <summary>
        /// 接入应用信息
        /// </summary>
        [JsonProperty("client")]
        public ClientItem Client { set; get; }
        /// <summary>
        /// 用户信息
        /// </summary>
        [JsonProperty("user")]
        public UserItem User { set; get; }
        /// <summary>
        /// 过期时间
        /// </summary>
        [JsonProperty("expire_time")]
        public DateTime? ExpireTime { set; get; }

        /// <summary>
        /// 执行结果
        /// </summary>
        [JsonIgnore]
        public ResultItem Result { set; get; }

    }
}