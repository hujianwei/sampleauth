using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Redis;

namespace LaiCai.Auth.Implement
{
    public class RedisCli
    {
        private string host = "127.0.0.1";
        private int port = 6379;
        private static Lazy<RedisCli> lazy = new Lazy<RedisCli>();

        public RedisCli Instance()
        {
            return lazy.Value;
        }

        private CacheConnection GetConnection()
        {
            return new CacheConnection(host, "0", "", "", port);
        }

        private RedisClient _client = null;
        public RedisClient Client()
        {
            try
            {
                var connection = GetConnection();
                _client = new RedisClient(connection.Server, connection.Port);
                _client.Password = connection.Password;
                _client.Db = Convert.ToInt32(connection.Db);

            }
            catch (Exception ex)
            {
               
            }
            return _client;
        }



        public bool Set<T>(string key, T value)
        {
            using (var client = this.Client())
            {
                return Set<T>(key, value, client);
            }
        }

        public bool Set<T>(string key, T value, RedisClient client)
        {
            return client.Set<T>(key, value);
        }

        public bool Set<T>(string key, T value, DateTime expireTime)
        {
            using (var client = this.Client())
            {
                return Set<T>(key, value, expireTime, client);
            }
        }

        public bool Set<T>(string key, T value, DateTime expireTime, RedisClient client)
        {
            return client.Set<T>(key, value, expireTime);
        }

        public void Del(string key)
        {
            using (RedisClient client = this.Client())
            {
                Del(key, client);
            }
        }

        public void Del(string key, RedisClient client)
        {
            client.Del(key);
        }

        public T Get<T>(string key)
        {
            using (RedisClient client = this.Client())
            {
                return client.Get<T>(key);
            }
        }
    }
}