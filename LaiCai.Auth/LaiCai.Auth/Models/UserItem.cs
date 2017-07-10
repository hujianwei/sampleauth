using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace LaiCai.Auth.Models
{
    public class UserItem:ICloneable
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [JsonProperty("user_id")]
        public string UserId { set; get; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        [JsonProperty("nick_name")]
        public string NickName { set; get; }

        /// <summary>
        /// 密码
        /// </summary>
        [JsonProperty("password")]
        public string Password { set; get; }


        /// <summary>
        /// 用户名
        /// </summary>
        [JsonProperty("user_name")]
        public string UserName { set; get; }


        /// <summary>
        /// 执行结果
        /// </summary>
        public ResultItem Result { set; get; }

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }
    }
}