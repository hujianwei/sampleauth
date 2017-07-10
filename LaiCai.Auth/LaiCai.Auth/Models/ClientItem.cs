using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace LaiCai.Auth.Models
{
    /// <summary>
    /// 接入应用的信息
    /// </summary>
    public class ClientItem
    {
        /// <summary>
        /// 接入应用ID
        /// </summary>
        [JsonProperty("client_id")]
        public string ClientId { set; get; }
        /// <summary>
        /// Secret
        /// </summary>
        [JsonIgnore]
        public string Secret { set; get; }
        /// <summary>
        /// 客户端名称
        /// </summary>
        [JsonProperty("client_name")]
        public string ClientName { set; get; }
        /// <summary>
        /// 令牌有效间隔(秒)
        /// </summary>
        [JsonProperty("token_interval")]
        public int TokenInterval { set; get; }

        /// <summary>
        /// 是否启用刷新令牌
        /// </summary>
        [JsonProperty("enable_refresh_token")]
        public bool EnableRefreshToken { set; get; }

        /// <summary>
        /// 刷新令牌有效间隔(秒)
        /// </summary>
        [JsonProperty("refresh_token_interval")]
        public int RefreshTokenInterval { set; get; }

        /// <summary>
        /// 接入类型(1:服务器与服务器直连,2:用户登陆连接)
        /// </summary>
        [JsonProperty("client_type")]
        public int ClientType { set; get; }
        /// <summary>
        /// 第二种加密的key
        /// </summary>
        [JsonIgnore]
        public string SecondSecretKey { set; get; }
        /// <summary>
        /// 状态(1:有效/0:待审核/-1:无效)
        /// </summary>
        [JsonProperty("status")]
        public int Status { set; get; }
        /// <summary>
        /// 状态更新时间
        /// </summary>
        [JsonProperty("status_time")]
        public DateTime StatusTime { set; get; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonProperty("create_time")]
        public DateTime CreateTime { set; get; }

        /// <summary>
        /// 执行结果
        /// </summary>
        public ResultItem Result { set; get; }

        public override bool Equals(object obj)
        {
            try
            {
                ClientItem item = obj as ClientItem;
                if (item.ClientId == this.ClientId && item.Secret == this.Secret)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }


    }
}