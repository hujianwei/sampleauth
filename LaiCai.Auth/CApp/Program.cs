using System;
using System.Collections.Generic;
using System.Text;
using CacheManager.Core;
using CacheManager.SystemRuntimeCaching;
using CacheManager.Redis;
using System.Threading;
using System.IO;

using ServiceStack.Common;
using ServiceStack.Redis;

namespace CApp
{
    class Program
    {
        private static string logDir = AppDomain.CurrentDomain.BaseDirectory + "log";
        static void Main1(string[] args)
        {

            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

           //Cache.Instance().Clear();


            IList<Thread> threadList = new List<Thread>();
            var prevTime = DateTime.Now;
            for (int i = 0; i < 1000; i++)
            {
                Thread td = new Thread(new System.Threading.ParameterizedThreadStart(SetTask));
                td.Start(i);
                threadList.Add(td);
            }
            foreach (var thread in threadList)
            {
                thread.Join();
            }
            threadList = new List<Thread>();



            for (int i = 0; i < 1000; i++)
            {
                Thread td = new Thread(new System.Threading.ParameterizedThreadStart(GetTask));
                td.Start(i);
                threadList.Add(td);
            }
            foreach(var thread in threadList)
            {
                thread.Join();
            }
            Console.WriteLine(DateTime.Now.Subtract(prevTime).TotalSeconds);
            Console.ReadLine();



           
            Console.ReadLine();
        }

        public static void SetTask(object sp)
        {
          
            var prevDate = DateTime.Now;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var rdVal = new Random(1000000 + i).Next(9999999);
                    IDictionary<string, object> dict = new Dictionary<string, object>();
                    dict.Add("name", sp.ToString() + "_" + i.ToString());
                    dict.Add("value", rdVal.ToString() + DateTime.Now.Ticks.ToString());
                    Cache.Instance().Set(sp.ToString() + "_" + i.ToString(), dict, 3600);
                }
                catch(Exception e)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("\r\n时间:{0},错误消息:{1},source:{2},StackTrace:{3},TargetSite:{4}", DateTime.Now, e.Message, e.Source, e.StackTrace, e.TargetSite);
                    System.IO.File.AppendAllText(string.Format(@"{0}\seterror_{1}.txt", logDir,sp.ToString()), sb.ToString());
                }
            }
            Console.WriteLine("settask" + "_" + sp.ToString()+ "----"+DateTime.Now.Subtract(prevDate).TotalSeconds);
          
        }

        public static void GetTask(object sp)
        {
            var prevDate = DateTime.Now;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    //Console.WriteLine(Cache.manager.Exists(sp.ToString() + "_" + i.ToString()));

                    var obj = Cache.Instance().Get(sp.ToString() + "_" + i.ToString());
                    //Console.WriteLine(obj);
                }
                catch(Exception e)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("\r\n\r\n\r\n时间:{0},{5}错误消息:{1},source:{2},StackTrace:{3},TargetSite:{4}", DateTime.Now, e.Message, e.Source, e.StackTrace, e.TargetSite,i);
                    System.IO.File.AppendAllText(string.Format(@"{0}\geterror_{1}.txt", logDir, sp.ToString()), sb.ToString());
                }
            }
            Console.WriteLine("gettask" + "_" + sp.ToString()+ "-----"+DateTime.Now.Subtract(prevDate).TotalSeconds);
        }
    }

    public class Cache
    {
        //private string _host ="192.168.1.55";
        //private int _port = 6379;
        //private string _password = "laicai88.com";
        private string _host = "192.168.1.55";
        private int _port = 7000;
        private string _password = "laicai88.com";
        
        public static ICacheManager<object> manager = null;

        private Cache()
        {

            manager = CacheFactory.Build("getConsole", settings =>
            {

                settings.WithSystemRuntimeCacheHandle("handleName").WithExpiration(ExpirationMode.Sliding, TimeSpan.FromMinutes(20))
                .And
                .WithRedisConfiguration("redis", config =>
                {
                    config.WithAllowAdmin()
                        .WithDatabase(0)
                        .WithEndpoint(_host, _port)
                        .WithEndpoint("192.168.1.55", 7001)
                        .WithEndpoint("192.168.1.55", 7002)
                        .WithEndpoint("192.168.1.55", 7003)
                        .WithEndpoint("192.168.1.55", 7004)
                        .WithEndpoint("192.168.1.55", 7005)
                        .WithEndpoint("192.168.1.55", 7010)
                        .WithEndpoint("192.168.1.55", 7011)
                        .WithPassword(_password)
                        .WithConnectionTimeout(500);
                })
                .WithMaxRetries(100)
                .WithRetryTimeout(50)
                .WithRedisBackplane("redis")
                .WithRedisCacheHandle("redis", true);

            });

        }

        private static Cache _cache = null;

        public static Cache Instance()
        {
            //var items = manager.CacheHandles;


            //foreach(var item in items)
            //{
            //    if(item.GetType()==typeof(CacheManager.Redis.RedisCacheHandle<object>))
            //    {
            //        var temp = (CacheManager.Redis.RedisCacheHandle<object>)item;
            //        var serverList = temp.Servers;

            //    }
            //}
            if (_cache == null)
                _cache = new Cache();
            return _cache;
        }

        public object Get(string key)
        {
            return manager.Get(key);
        }

        public void Set(string key,object value,int expireTime)
        {
            CacheItem<object> item = new CacheItem<object>(key, value, ExpirationMode.Sliding, TimeSpan.FromSeconds(expireTime));
            manager.Put(item);
        }

        public void Set(string key,object value)
        {
            manager.Put(key, value);
        }

        public void Clear()
        {
            manager.Clear();
        }

        

    }
}
