using System;
using System.Collections.Generic;
using System.Text;
using CacheManager.Core;
using CacheManager.SystemRuntimeCaching;
using CacheManager.Redis;

namespace CApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Cache.Instance().Set("borthday", "fdfs1909983434", 300);

            Console.WriteLine(Cache.Instance().Get("145695"));

            Cache.Instance().Set("d", "test");

            Cache.Instance().Set("e", "test");

            Cache.Instance().Set("f", "test");
            Cache.Instance().Set("g", "test");
            Cache.Instance().Set("h", "test");
            for (int i = 0; i < 500; i++)
            {
                string name = new Random().Next(1000, 99999999).ToString();
                Cache.Instance().Set(name, "test");
                System.Threading.Thread.Sleep(10);
            }

            Console.ReadLine();
        }
    }

    public class Cache
    {
        //private string _host ="192.168.1.55";
        //private int _port = 6379;
        //private string _password = "laicai88.com";
        private string _host = "192.168.1.55";
        private int _port = 7000;
        private string _password = "";
        private static ICacheManager<object> manager = null;

        private Cache()
        {
            manager = CacheFactory.Build("getConsole", settings =>
            {
                settings.WithSystemRuntimeCacheHandle("handleName").WithExpiration(ExpirationMode.Sliding, TimeSpan.FromMinutes(60))
                .And
                .WithRedisConfiguration("redis", config =>
                {
                    config.WithAllowAdmin()
                        .WithDatabase(0)                      
                        .WithEndpoint(_host, _port)

                        //.WithEndpoint("192.168.1.55",7001)
                        //.WithEndpoint("192.168.1.55",7002)
                        .WithEndpoint("192.168.1.55", 7003)
                        //.WithEndpoint("192.168.1.55",7004)
                        //.WithEndpoint("192.168.1.55",7005)
                        .WithEndpoint("192.168.1.55", 7006)
                        //.WithEndpoint("192.168.1.55", 7007)
                        //.WithEndpoint("192.168.1.55", 7008)
                        //.WithEndpoint("192.168.1.55", 7009)
                        //.WithEndpoint("192.168.1.55", 7010)
                        //.WithEndpoint("192.168.1.55", 7011)
                        .WithPassword(_password);
                })

                .WithMaxRetries(100)
                .WithRetryTimeout(50)
                .WithRedisBackplane("redis")
                .WithRedisCacheHandle("redis", true);
                
            });
           
        }

        private static Cache _cache = new Cache();

        public static Cache Instance()
        {
            var items = manager.CacheHandles;
            foreach(var item in items)
            {
                if(item.GetType()==typeof(CacheManager.SystemRuntimeCaching.MemoryCacheHandle<object>))
                {
                   
                }
            }
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

        

    }
}
