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
    /// <summary>
    /// 用户相关操作
    /// </summary>
    public class UserRepertory: BaseOperater,IUser
    {
        //private ICache _cache = null;
        //private ILog _log = null;
        //private static IList<UserItem> userList = new List<UserItem>();

        
        public UserRepertory():base()
        {
            //_cache = cache;
            //_log = log;
            //for (int i=1000;i<1010;i++)
            //{
            //    UserItem item = new UserItem();
            //    Random rd = new Random();
            //    item.UserId = i.ToString();
            //    item.UserName = i.ToString();
            //    item.Password = (rd.Next(8999) + i).ToString();
            //    item.NickName = item.Password;
            //    string key = string.Format(user_key, item.UserId);
            //    _cache.Set(key, item);
            //    userList.Add(item);
            //}
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<UserItem> GetUserById(string userId)
        {
            string key = CacheKeyFormat.UserFormat.GetUserKey(userId);
            var item = this.Cache.Get<UserItem>(key);
            return Task.FromResult(item);
        }

        /// <summary>
        /// 检查用户名和密码,数据库替换
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private Task<UserItem> CheckUser(string userName,string password)
        {
            var list = this.Cache.GetByPattern<UserItem>("user_id_*");
            var p = list.Where(c => c.UserName == userName && c.Password == password).FirstOrDefault();
            return Task.FromResult(p);
        }

        /// <summary>
        /// 检查用户是否合法
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task Check(UserItem item)
        {
            if(item==null)
            {
                this.Log.Error("CheckUser错误，对象为空");
                return;
            }
            var result = await CheckUser(item.UserName,item.Password);
            if (result == null)
            {
                item.Result = new ResultItem(false, "账户或密码不正确");
                return;
            }
            else
            {
                //将结果赋值用返回的对象
                item.UserId = result.UserId;
                item.UserName = result.UserName;
                item.Password = result.Password;
                item.NickName = result.NickName;
                item.Result = new ResultItem(true, "");
            }
        }
    }
}