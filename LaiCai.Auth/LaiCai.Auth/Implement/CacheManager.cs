using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json;
using LaiCai.Auth.IServices;

using CacheManager.Core;
using CacheManager.Redis;
using CacheManager.SystemRuntimeCaching;

namespace LaiCai.Auth.Implement
{
    public class CacheManager : ICache
    {
        private string _host = Common.Unity.AppSettings("redis_host");
        private int _port = Convert.ToInt32(Common.Unity.AppSettings("redis_port"));
        private string _password = Common.Unity.AppSettings("redis_password");

        private static ICacheManager<object> manager = null;

        public CacheManager()
        {
            manager = CacheFactory.Build("getStartedCache", settings =>
            {
                settings.WithSystemRuntimeCacheHandle("handleName")
                

                .And
                .WithRedisConfiguration("redis", config =>
                {
                    config.WithAllowAdmin()
                        .WithDatabase(0)
                        .WithEndpoint(_host, _port)
                        .WithPassword(_password);
                })
                .WithMaxRetries(100)
                .WithRetryTimeout(50)
                .WithRedisBackplane("redis")
                .WithRedisCacheHandle("redis", true)
                ;
            });
        }

        public T BRPop<T>(string listId)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Del(string key)
        {
            manager.Remove(key);
           
        }

        public void Del(params string[] keys)
        {
            foreach(var key in keys)
            {
                Del(key);
            }
          
        }

        public void DelByPattern(string keyPattern)
        {
            throw new NotImplementedException();
        }

        public void FlushAll()
        {
            manager.Clear();
            throw new NotImplementedException();
        }

        public T Get<T>(string key)
        {
            return manager.Get<T>(key);
        }

        public IList<T> GetByPattern<T>(string keyPattern)
        {
            throw new NotImplementedException();
        }

        public int LPush<T>(string listId, T obj)
        {
            throw new NotImplementedException();
        }

        public void LPush<T>(string listId, T[] arrObj)
        {
            throw new NotImplementedException();
        }

        public T RPop<T>(string listId)
        {
            throw new NotImplementedException();
        }

        public bool Set<T>(string key, T value)
        {           
            manager.Put(key, value);
            return true;   
        }

        public bool Set<T>(string key, T value, DateTime expireTime)
        {
            CacheItem<object> item = new CacheItem<object>(key, value, ExpirationMode.Absolute, expireTime.Subtract(DateTime.Now));
            manager.Put(item);
            return true;
        }

        public void SetAll<T>(IDictionary<string, T> values)
        {
            if(values!=null&& values.Count>0)
            {
                foreach (var item in values)
                    Set(item.Key, item.Value);
            }
        }
    }
}