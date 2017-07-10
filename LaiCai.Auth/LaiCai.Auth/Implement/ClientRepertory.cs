using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using LaiCai.Auth.IServices;
using LaiCai.Auth.Models;
using LaiCai.Auth.Common;

namespace LaiCai.Auth.Implement
{
    public class ClientRepertory:BaseOperater,IClient
    {
        private static IList<ClientItem> _clientList = new List<ClientItem>();

        public ClientRepertory():base()
        {

        }

        /// <summary>
        /// 增加应用
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Task<bool> AddClient(ClientItem item)
        {
            var p = _clientList.Where(c => c.ClientId == item.ClientId).FirstOrDefault();
            if (p != null)
            {
                Log.Info("已存在接入应用");
                throw new Exception("已存在接入应用");
            }
            _clientList.Add(item);//改成写入数据库
            string key = CacheKeyFormat.ClientFormat.GetClientKey(item.ClientId);
            Cache.Set(key, item);
            return Task.FromResult(true);
        }

        /// <summary>
        /// 根据clientId获取应用信息
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public Task<ClientItem> GetById(string clientId)
        {
            string key = CacheKeyFormat.ClientFormat.GetClientKey(clientId);
            var item = Cache.Get<ClientItem>(key);
            if(item==null)//从数据库中读取
            {

            }
            return Task.FromResult(item);
        }

        /// <summary>
        /// 获取所有的应用信息
        /// </summary>
        /// <returns></returns>
        public Task<IList<ClientItem>> GetAll()
        {
            return Task.FromResult(_clientList);
        }

        /// <summary>
        /// 检查应用是否合法
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task Check(ClientItem item)
        {
            if (item == null)
                throw new Exception("对象为空");
            else if (string.IsNullOrEmpty(item.ClientId))
            {
                item.Result = new ResultItem(false, "clientId为空");
                return;
            }
            else if(string.IsNullOrEmpty(item.Secret))
            {
                item.Result = new ResultItem(false, "secret为空");
                return;
            }
            var result = await this.GetById(item.ClientId);
            if(result==null)
            {
                item.Result = new ResultItem(false, "库中不存在client对象");
                return;
            }
            else if(!result.Secret.Equals(item.Secret))
            {
                item.Result = new ResultItem(false, "secret错误");
                return;
            }
            else
            {
                item.Result = new ResultItem(true, "");
                return;
            }
        }


    }
}