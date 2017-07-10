using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LaiCai.Auth.Models
{
    /// <summary>
    /// 请求认证的内容
    /// </summary>
    public class RequstAuthContext
    {

        public RequstAuthContext()
        {
            this.Params = new Dictionary<string, string>();
        }

        /// <summary>
        /// 请求的参数
        /// </summary>
        public IDictionary<string,string> Params { set; get; }
        /// <summary>
        /// 接入应用ID
        /// </summary>
        public string clientId { set; get; }
        /// <summary>
        /// 接入Secret
        /// </summary>
        public string clientSecret { set; get; }

        /// <summary>
        /// 返回结果对象
        /// </summary>
        public Object Result { set; get; }


    }
}