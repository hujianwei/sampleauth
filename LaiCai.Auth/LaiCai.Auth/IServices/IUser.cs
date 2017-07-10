using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LaiCai.Auth.Models;
using System.Threading.Tasks;

namespace LaiCai.Auth.IServices
{
    /// <summary>
    /// 用户接口
    /// </summary>
    public interface IUser: IValidate<UserItem>
    {
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<UserItem> GetUserById(string userId);
    }
}