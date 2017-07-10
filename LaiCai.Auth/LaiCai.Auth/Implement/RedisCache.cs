using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;
using System.Text;
using ServiceStack.Redis;
using Newtonsoft.Json;
using LaiCai.Auth.IServices;


namespace LaiCai.Auth.Implement
{
    /// <summary>
    /// Redis实现
    /// </summary>
    public class RedisCache : ICache
    {
        private ILog _log = null;
        public RedisCache()
        {

        }

        public RedisCache(ILog log)
        {
            _log = log;
        }

        private string _host = Common.Unity.AppSettings("redis_host");
        private int _port = Convert.ToInt32(Common.Unity.AppSettings("redis_port"));
        private string _password = Common.Unity.AppSettings("redis_password");

        private CacheConnection GetConnection()
        {
            return new CacheConnection(_host, "0", "", _password, _port);
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
            catch(Exception ex)
            {
                _log.Error(ex.Message);
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

        public void SetAll<T>(IDictionary<string, T> values)
        {
            using (var client = Client())
            {
                SetAll(values, client);
            }
        }

        public void SetAll<T>(IDictionary<string, T> values, RedisClient client)
        {
            client.SetAll(values);
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



        public void DelByPattern(string keyPattern)
        {
            using (var client = this.Client())
            {
                DelByPattern(keyPattern, client);
            }
        }

        public void DelByPattern(string keyPattern, RedisClient client)
        {
            var keys = client.Keys(keyPattern);
            if (keys != null)
            {
                for (int i = 0; i < keys.Length; i++)
                    client.Del(this.GetConnection().encoding.GetString(keys[i]));

            }
        }

        public void Del(params string[] keys)
        {
            using (RedisClient client = this.Client())
            {
                Del(client, keys);
            }
        }

        public void Del(RedisClient client, params string[] keys)
        {
            foreach(var item in keys)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                else
                    client.Del(item);
            }
        }

        public void FlushAll()
        {
            using (var client = this.Client())
            {
                client.FlushDb();
            }
        }

        public T Get<T>(string key)
        {
            using (RedisClient client = this.Client())
            {
                return client.Get<T>(key);
            }
        }

        /// <summary>
        /// 通配获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyPattern"></param>
        /// <returns></returns>
        public IList<T> GetByPattern<T>(string keyPattern)
        {
            IList<T> list = new List<T>();
            using (RedisClient client = this.Client())
            {
                var keys = client.Keys(keyPattern);
                if (keys != null)
                {
                    for (int i = 0; i < keys.Length; i++)
                        list.Add(client.Get<T>(this.GetConnection().encoding.GetString(keys[i])));                      
                }
            }
            return list;
        }

        public int LPush<T>(string listId, T obj)
        {
            using (RedisClient client = this.Client())
            {
                return LPush<T>(listId, obj, client);
            }

        }

        public int LPush<T>(string listId, T obj, RedisClient client)
        {
            var pushStr = JsonConvert.SerializeObject(obj);
            return client.LPush(listId, this.GetConnection().encoding.GetBytes(pushStr));
        }

        public void LPush<T>(string listId, T[] arrObj)
        {
            using (RedisClient client = this.Client())
            {
                foreach (var item in arrObj)
                {
                    LPush<T>(listId, item, client);
                }
            }
        }

        public T RPop<T>(string listId)
        {
            using (RedisClient client = this.Client())
            {
                var result = client.RPop(listId);
                if (result != null)
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<T>(this.GetConnection().encoding.GetString(result));
                    }
                    catch
                    {
                        return default(T);
                    }
                }
                else
                    return default(T);
            }
        }

        public T BRPop<T>(string listId)
        {
            using (RedisClient client = this.Client())
            {
                var result = client.BRPop(listId, 100);
                if (result != null && result.Length == 2)
                {

                    return JsonConvert.DeserializeObject<T>(this.GetConnection().encoding.GetString(result[1]));

                }
                return default(T);
            }
        }

        public void Close()
        {
            this._client.Quit();
            this._client.Dispose();
        }



    }

    /// <summary>
    /// 缓存连接信息
    /// </summary>
    public class CacheConnection
    {
        /// <summary>
        /// 缓存服务器地址
        /// </summary>
        public string Server { set; get; }

        /// <summary>
        /// 缓存数据库
        /// </summary>
        public string Db { set; get; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { set; get; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { set; get; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { set; get; }

        /// <summary>
        /// 编码
        /// </summary>
        public Encoding encoding { set; get; }

        public CacheConnection(string server, string db, string userName, string password, int port = 6379, string encodName = "utf-8")
        {
            this.Server = server;
            this.Port = port;
            this.Db = db;
            this.UserName = userName;
            this.Password = password;
            this.encoding = Encoding.GetEncoding(encodName);

        }


    }
}