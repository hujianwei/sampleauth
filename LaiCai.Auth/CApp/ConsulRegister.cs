using System;
using System.Collections.Generic;
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
        static  void Main(string[] args)
        {
            var client = new ConsulClient();
            Client_DefaultConfig_env();

            //var info = await client.Agent.Self();
            //var checks = await client.Health.Node((string)info.Response["Config"]["NodeName"]);


        }
        [Fact]
        public static async void Client_DefaultConfig_env()
        {
            const string addr = "192.168.1.55:8500";
            const string token = "abcd1234";
            const string auth = "username:password";
            Environment.SetEnvironmentVariable("CONSUL_HTTP_ADDR", addr);
            Environment.SetEnvironmentVariable("CONSUL_HTTP_TOKEN", token);
            Environment.SetEnvironmentVariable("CONSUL_HTTP_AUTH", auth);
            Environment.SetEnvironmentVariable("CONSUL_HTTP_SSL", "1");
            Environment.SetEnvironmentVariable("CONSUL_HTTP_SSL_VERIFY", "0");

            var client = new ConsulClient();
            var config = client.Config;
            await KV_Put_Get_Delete();
            var info =  await client.Agent.Self();
            var t = info.Response["Config"]["NodeName"];


            var opts = new QueryOptions()
            {
                Datacenter = "dc1",
                Consistency = ConsistencyMode.Consistent,
                WaitIndex = 1000,
                WaitTime = new TimeSpan(0, 0, 100),
                Token = "12345"
            };
            var tt = client.KV;
            //var request = client.Get<KVPair>("/v1/kv/foo", opts);



            Environment.SetEnvironmentVariable("CONSUL_HTTP_ADDR", string.Empty);
            Environment.SetEnvironmentVariable("CONSUL_HTTP_TOKEN", string.Empty);
            Environment.SetEnvironmentVariable("CONSUL_HTTP_AUTH", string.Empty);
            Environment.SetEnvironmentVariable("CONSUL_HTTP_SSL", string.Empty);
            Environment.SetEnvironmentVariable("CONSUL_HTTP_SSL_VERIFY", string.Empty);



        }

        [Fact]
        internal static string GenerateTestKeyName()
        {
            var keyChars = new char[16];

            for (var i = 0; i < keyChars.Length; i++)
            {
                keyChars[i] = Convert.ToChar(new Random().Next(65, 91));
            }
            return new string(keyChars);
        }

        public static async Task KV_Put_Get_Delete()
        {
            var client = new ConsulClient();
            var kv = client.KV;

            var key = GenerateTestKeyName();

            var value = Encoding.UTF8.GetBytes("test");

            var getRequest = await kv.Get(key);


            var pair = new KVPair(key)
            {
                Flags = 42,
                Value = value
            };

            var putRequest = await kv.Put(pair);


            try
            {
                // Put a key that begins with a '/'
                var invalidKey = new KVPair("/test")
                {
                    Flags = 42,
                    Value = value
                };
                await kv.Put(invalidKey);
               
            }
            catch (InvalidKeyPairException ex)
            {
               
            }

            getRequest = await kv.Get(key);
           
            var res = getRequest.Response;

            
          

            var del = await kv.Delete(key);
          

            getRequest = await kv.Get(key);
           
        }
    }
}
