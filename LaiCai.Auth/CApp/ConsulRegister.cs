using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consul;
using Xunit;

namespace CApp
{
    /// <summary>
    /// consul服务注册
    /// </summary>
    class ConsulRegister
    {
        private static string rootUri = "http://192.168.1.55:8500/v1/";
        private static string rootDir = @"D:\gitcode\auth\LaiCai.Auth\CApp\";
        static  void Main1(string[] args)
        {
            RegisterService();
            //DeregisterService("redis1");
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        static void RegisterService()
        {
            string regUrl = string.Format("{0}/agent/service/register", rootUri);
            string str = File.ReadAllText(string.Format(@"{0}consulconfig\regservice.txt",rootDir));
            var result = HttpRequestHelper.HttpToServer(regUrl, str, HttpRequestHelper.HttpMethod.PUT, HttpRequestHelper.ContentType.JSON, "", null, "utf-8");
        }

        /// <summary>
        /// 删除服务
        /// </summary>
        /// <param name="serviceId"></param>
        static void DeregisterService(string serviceId)
        {
            string url = string.Format("{0}/agent/service/deregister/{1}", rootUri, serviceId);
            var result = HttpRequestHelper.HttpToServer(url, "", HttpRequestHelper.HttpMethod.PUT, HttpRequestHelper.ContentType.JSON, "", null, "utf-8");
        }

    }
}
