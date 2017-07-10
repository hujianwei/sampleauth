using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace LaiCai.Auth.Models
{

    public class ResultItem
    {
        public ResultItem(bool isSuccess,string message)
        {
            this.IsSuccess = isSuccess;
            this.Message = message;
        }

        /// <summary>
        /// 是否成功标志
        /// </summary>
       
        public bool IsSuccess { set; get; }

        /// <summary>
        /// 消息
        /// </summary>
        
        public string Message { set; get; }
    }
}