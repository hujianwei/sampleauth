using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using LaiCai.Auth.IServices;

namespace LaiCai.Auth.Common
{
    /// <summary>
    /// 常用方法
    /// </summary>
    public class Unity
    {
        /// <summary>
        /// md5回密
        /// </summary>
        /// <param name="inputStr"></param>
        /// <returns></returns>
        public static string Md5(string inputStr)
        {
            string pwd = string.Empty;
            MD5 md5 = MD5.Create();
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(inputStr));
            for (int i = 0; i < s.Length; i++)
            {
                pwd = pwd + s[i].ToString("X2");
            }
            return pwd.ToLower();
        }

        /// <summary>
        /// 产生令牌ID
        /// </summary>
        /// <returns></returns>
        public static string GetTokenId()
        {
            Random rd = new Random();
            int rdNum = 1000000 + rd.Next(8999999);
            return Md5(Guid.NewGuid().ToString() + rdNum.ToString());
        }

        /// <summary>
        /// 获取配置文件的节点
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AppSettings(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }
    }

    /// <summary>
    /// 缓存的key格式
    /// </summary>
    public class CacheKeyFormat
    {
        /// <summary>
        /// 用户相关的key格式
        /// </summary>
        public class UserFormat
        {
            /// <summary>
            /// 用户信息, "user_id_{0}"
            /// </summary>
            private static string userKey = "user_id_{0}";

            /// <summary>
            /// 用户信息Key, "user_id_{0}"
            /// </summary>
            /// <param name="userId"></param>
            /// <returns></returns>
            public static string GetUserKey(string userId)
            {
                return string.Format(userKey, userId);
            }

            /// <summary>
            /// 接入应用的用户,"client_{0}:user_{1}"
            /// </summary>
            private static string clientUserKey = "client_{0}:user_{1}";

            /// <summary>
            /// 接入应用的用户,"client_{0}:user_{1}"
            /// </summary>
            public static string GetClientUserKey(string clientId,string userId)
            {
                return string.Format(clientUserKey, clientId, userId);
            }
        }

        /// <summary>
        /// 接入应用相关的key格式
        /// </summary>
        public class ClientFormat
        {
            /// <summary>
            /// 接入应用的信息,"client_{0}"
            /// </summary>
            public static string clientKey = "client_{0}";

            /// <summary>
            /// 接入应用的信息,"client_{0}"
            /// </summary>
            public static string GetClientKey(string clientId)
            {
                return string.Format(clientKey, clientId);
            }

        }

        /// <summary>
        /// 令牌的格式
        /// </summary>
        public class TokenFormat
        {
            /// <summary>
            /// 令牌信息，"token_id_{0}"
            /// </summary>
            public static string tokenKey = "token_id_{0}";
            /// <summary>
            /// 令牌信息，"token_id_{0}"
            /// </summary>
            public static string GetTokenKey(string tokenId)
            {
                return string.Format(tokenKey, tokenId);
            }
        }
    }
}