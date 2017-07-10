using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LaiCai.Auth.IServices
{
    /// <summary>
    /// 日志接口
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// 消息
        /// </summary>
        /// <param name="obj"></param>
        void Info(object obj);

        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="obj"></param>
        void Warn(object obj);
        /// <summary>
        /// 异常错误
        /// </summary>
        /// <param name="obj"></param>
        void Error(object obj);

        /// <summary>
        /// 严重错误
        /// </summary>
        /// <param name="obj"></param>
        void Fatal(object obj);


    }
}