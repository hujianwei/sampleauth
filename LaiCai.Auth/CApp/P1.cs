using System;
using System.Collections.Generic;
using System.Text;
using CacheManager.Core;
using CacheManager.Redis;
using CacheManager.SystemRuntimeCaching;
using System.Threading;
using System.IO;

using ServiceStack.Common;
using StackExchange.Redis;

namespace CApp
{
    class P1
    {
        static void Main1(string[] args)
        {



            //ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("192.168.1.21:7011,password=)b[%xm6NrUO8ge#t");

            //ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("192.168.1.21:7011,allowAdmin=True,connectTimeout=5000,password=)b[%xm6NrUO8ge#t,ssl=False,abortConnect=False,connectRetry=10,proxy=None");
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("192.168.1.55:7000,allowAdmin=True,connectTimeout=5000,password=laicai88.com,ssl=False,abortConnect=False,connectRetry=10,proxy=None");
            IDatabase db = redis.GetDatabase();
            db.StringSet("twerere", "shaocan");
            Redis_UseExistingConnection();
        }

        public static void Redis_UseExistingConnection()
        {
            var conConfig = new ConfigurationOptions()
            {
                ConnectTimeout = 10000,
                AbortOnConnectFail = false,
                ConnectRetry = 10
            };
            conConfig.EndPoints.Add("192.168.1.21:7011");
            conConfig.Password = ")b[%xm6NrUO8ge#t";

            var multiplexer = ConnectionMultiplexer.Connect(conConfig);

            var cfg = ConfigurationBuilder.BuildConfiguration(
                s => s
                   
                    .WithRedisConfiguration("redisKey", multiplexer)
                    .WithRedisCacheHandle("redisKey"));
            



            using (multiplexer)
            using (var cache = new BaseCacheManager<long>(cfg))
            {
                cache.Add(Guid.NewGuid().ToString(), 12345);
            }
        }
    }
}
