using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LaiCai.Auth.Models;
using System.Threading.Tasks;

namespace LaiCai.Auth.IServices
{
    /// <summary>
    /// 接入应用的相关接口
    /// </summary>
    public interface IClient:IValidate<ClientItem>
    {

        /// <summary>
        /// 增加应用
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> AddClient(ClientItem item);

        /// <summary>
        /// 根据clientId获取应用信息
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<ClientItem> GetById(string clientId);

        /// <summary>
        /// 获取所有的应用信息
        /// </summary>
        /// <returns></returns>
        Task<IList<ClientItem>> GetAll();

        


    }
}