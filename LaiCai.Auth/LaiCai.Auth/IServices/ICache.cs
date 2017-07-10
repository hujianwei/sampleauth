using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LaiCai.Auth.IServices
{
    public interface ICache
    {
        
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns></returns>
        bool Set<T>(string key, T value);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="expireTime">过期时间</param>
        /// <returns></returns>
        bool Set<T>(string key, T value, DateTime expireTime);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="values">键值对</param>
        void SetAll<T>(IDictionary<string, T> values);

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key"></param>
        void Del(string key);


        /// <summary>
        /// 批量删除附合表达式的缓存
        /// </summary>
        /// <param name="keyPattern"></param>
        void DelByPattern(string keyPattern);

        /// <summary>
        /// 删除多个key
        /// </summary>
        /// <param name="keys"></param>
        void Del(params string[] keys);

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);

        /// <summary>
        /// 通配获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyPattern"></param>
        /// <returns></returns>
        IList<T> GetByPattern<T>(string keyPattern);

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        void FlushAll();

        /// <summary>
        /// 向list写入内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        int LPush<T>(string listId, T obj);
        /// <summary>
        /// 向list写入多个内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <param name="arrObj"></param>
        void LPush<T>(string listId, T[] arrObj);
        /// <summary>
        /// 向list读取内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <returns></returns>
        T RPop<T>(string listId);

        /// <summary>
        /// 以阻塞开式向list读取内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <returns></returns>
        T BRPop<T>(string listId);

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();

    }
}