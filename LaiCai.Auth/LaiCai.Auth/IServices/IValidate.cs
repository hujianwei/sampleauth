using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LaiCai.Auth.Models;
using System.Threading.Tasks;

namespace LaiCai.Auth.IServices
{
    /// <summary>
    /// 检查合法接口
    /// </summary>
    public interface IValidate<T>
    {
        /// <summary>
        /// 检查对象
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task Check(T item);
    }
}