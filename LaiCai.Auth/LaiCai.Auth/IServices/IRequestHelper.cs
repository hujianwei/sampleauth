using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace LaiCai.Auth.IServices
{
    public interface IRequestHelper
    {
        /// <summary>
        /// 向服务器提交数据并返回对应的结果
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sendContent"></param>
        /// <param name="method"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <param name="headerDict"></param>
        /// <param name="encoding"></param>
        /// <param name="timeout">过期时间</param>
        /// <returns>
        /// 返回Tuple<string,HttpStatusCode>数据,T1:int 服务器返回的状态码,T2:string 返回的消息
        /// </returns>
        Task<Tuple<int, string>> HttpToServer(string url, string sendContent, RequestMethod method, ContentType contentType, string token, IDictionary<string, string> headerDict, string encoding, int timeout = 10);

        /// <summary>
        /// 向服务器提交数据并返回对应的结果
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sendContent"></param>
        /// <param name="method"></param>
        /// <param name="contentType"></param>
        /// <param name="token"></param>
        /// <param name="headerDict"></param>
        /// <param name="encoding"></param>
        /// <param name="timeout">过期时间</param>
        /// <returns>
        /// 返回Tuple<string,HttpStatusCode>数据,T1:int 服务器返回的状态码,T2:string 返回的消息,T3:响应的cookie信息
        /// </returns>
        Task<Tuple<int, string,string>> HttpToServer(string url, string sendContent, RequestMethod method, ContentType contentType, string token, IDictionary<string, string> headerDict, string encoding, int timeout ,bool responseCookie );

    }
    /// <summary>
    /// 向http发送的方法
    /// </summary>
    public enum RequestMethod
    {
        /// <summary>
        /// Get方法
        /// </summary>
        GET = 0,
        /// <summary>
        /// Post方法
        /// </summary>
        POST = 1,
        /// <summary>
        /// Put方法
        /// </summary>
        PUT = 2,
        /// <summary>
        /// Delete
        /// </summary>
        DELETE = 3
    }

    public enum ContentType
    {
        /// <summary>
        /// json数据类型
        /// </summary>
        JSON = 0,
        /// <summary>
        /// 表单数据
        /// </summary>
        FORM = 1,
        /// <summary>
        /// XML数据类型
        /// </summary>
        XML = 2
    }
}