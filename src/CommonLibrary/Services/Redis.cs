using Newtonsoft.Json;
using StackExchange.Redis;

namespace CommonLibrary.Services
{
    /// <summary>
    /// 封装 Redis 相关操作的方法，新增加的
    /// </summary>
    public interface IRedisClientService
    {
        #region 同步
        /// <summary>
        /// 添加一个字符串对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        bool Set(string key, string value, TimeSpan? expiry = null);

        /// <summary>
        /// 添加一个对象，实体会序列换成字符串存储。
        /// </summary>
        /// <param name="key">键。</param>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="value">值。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        bool Set<T>(string key, T value, TimeSpan? expiry = null);


        /// <summary>
        /// 获取一个对象。
        /// </summary>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        T Get<T>(string key);

        /// <summary>
        /// 获取一个字符串对象。
        /// </summary>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        string Get(string key);

        /// <summary>
        /// 删除一个对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>返回是否执行成功。</returns>
        bool Delete(string key);

        /// <summary>
        /// 返回键是否存在。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>返回键是否存在。</returns>
        bool Exists(string key);

        /// <summary>
        /// 设置一个键的过期时间。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        bool SetExpire(string key, TimeSpan? expiry);
        #endregion

        #region 异步
        /// <summary>
        /// 异步添加一个字符串对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null);

        /// <summary>
        /// 异步添加一个对象，实体会序列换成字符串存储。
        /// </summary>
        /// <param name="key">键。</param>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="value">值。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// 异步获取一个对象。
        /// </summary>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// 异步获取一个字符串对象。
        /// </summary>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        Task<string> GetAsync(string key);

        /// <summary>
        /// 异步删除一个对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>返回是否执行成功。</returns>
        Task<bool> DeleteAsync(string key);

        /// <summary>
        /// 异步设置一个键的过期时间。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        Task<bool> SetExpireAsync(string key, TimeSpan? expiry);

        /// <summary>
        /// 返回键是否存在。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>返回键是否存在。</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// 根据前缀查找所有键值
        /// </summary>
        /// <param name="redisPrefix"></param>
        /// <returns></returns>
        Task<List<string>> FuzzyExistsAsync(string redisPrefix);
        #endregion

        #region 分布式锁...

        /// <summary>
        /// 获取锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <param name="expiry">过期时间。</param>
        /// <param name="lockToken">锁值</param>
        /// <returns>是否已锁。</returns>
        bool Lock(string key, string lockToken, TimeSpan expiry);

        /// <summary>
        /// 释放锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <param name="lockToken">锁值</param>
        /// <returns>是否成功。</returns>
        bool UnLock(string key, string lockToken);

        /// <summary>
        /// 异步获取锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <param name="expiry">过期时间。</param>
        /// <param name="lockToken">锁值</param>
        /// <returns>是否成功。</returns>
        Task<bool> LockAsync(string key, string lockToken, TimeSpan expiry);

        /// <summary>
        /// 异步释放锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <param name="lockToken">锁值</param>
        /// <returns>是否成功。</returns>
        Task<bool> UnLockAsync(string key, string lockToken);

        #endregion
    }
    /// <summary>
    /// 封装 Redis 相关操作的方法
    /// </summary>
    public class RedisClientService : IRedisClientService
    {
        private readonly IDatabase _database;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        public RedisClientService(IDatabase database)
        {
            _database = database;
        }

        #region 同步方法...

        /// <summary>
        /// 添加一个字符串对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public bool Set(string key, string value, TimeSpan? expiry = null)
        {
            return _database.StringSet(key, value, expiry);
        }

        /// <summary>
        /// 添加一个对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="value">值。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public bool Set<T>(string key, T value, TimeSpan? expiry = null)
        {
            var data = JsonConvert.SerializeObject(value);
            return _database.StringSet(key, data, expiry);
        }

        /// <summary>
        /// 获取一个对象。
        /// </summary>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        public T Get<T>(string key)
        {
            string json = _database.StringGet(key);
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }
            T entity = JsonConvert.DeserializeObject<T>(json);
            return entity;
        }

        /// <summary>
        /// 获取一个字符串对象。
        /// </summary>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        public string Get(string key)
        {
            return _database.StringGet(key);
        }

        /// <summary>
        /// 删除一个对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>返回是否执行成功。</returns>
        public bool Delete(string key)
        {
            return _database.KeyDelete(key);
        }

        /// <summary>
        /// 返回键是否存在。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>返回键是否存在。</returns>
        public bool Exists(string key)
        {
            return _database.KeyExists(key);
        }

        /// <summary>
        /// 设置一个键的过期时间。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public bool SetExpire(string key, TimeSpan? expiry)
        {
            return _database.KeyExpire(key, expiry);
        }
        #endregion

        #region 异步方法...

        /// <summary>
        /// 异步添加一个字符串对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            return await _database.StringSetAsync(key, value, expiry);
        }

        /// <summary>
        /// 异步添加一个对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="value">值。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var data = JsonConvert.SerializeObject(value);
            return await _database.StringSetAsync(key, data, expiry);
        }

        /// <summary>
        /// 异步获取一个对象。
        /// </summary>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        public async Task<T> GetAsync<T>(string key)
        {
            string json = await _database.StringGetAsync(key);
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }
            T entity = JsonConvert.DeserializeObject<T>(json);
            return entity;
        }

        /// <summary>
        /// 异步获取一个字符串对象。
        /// </summary>
        /// <param name="key">值。</param>
        /// <returns>返回对象的值。</returns>
        public async Task<string> GetAsync(string key)
        {
            return await _database.StringGetAsync(key);
        }

        /// <summary>
        /// 异步删除一个对象。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>返回是否执行成功。</returns>
        public async Task<bool> DeleteAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        /// <summary>
        /// 异步设置一个键的过期时间。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="expiry">过期时间（时间间隔）。</param>
        /// <returns>返回是否执行成功。</returns>
        public async Task<bool> SetExpireAsync(string key, TimeSpan? expiry)
        {
            return await _database.KeyExpireAsync(key, expiry);
        }

        /// <summary>
        /// 返回键是否存在。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>返回键是否存在。</returns>
        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        /// <summary>
        /// 根据前缀查找所有键值
        /// </summary>
        /// <param name="redisPrefix"></param>
        /// <returns></returns>
        public async Task<List<string>> FuzzyExistsAsync(string redisPrefix)
        {
            List<string> result = new List<string>();
            var pattern = $"{redisPrefix}*";
            //Redis的keys模糊查询：
            var redisResult = await _database.ScriptEvaluateAsync(LuaScript.Prepare("return redis.call('keys', @keypattern)"), new { @keypattern = pattern });
            if (!redisResult.IsNull)
            {
                foreach (var item in (string[])redisResult)
                {
                    result.Add(item);
                }
            }
            return result;
        }
        #endregion

        #region 分布式锁... 需要注意的是一定要禁用 UNLINK,UNLINK 需要 Redis 4.0 以上的版本才支持。

        /// <summary>
        /// 获取锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <param name="expiry">过期时间。</param>
        /// <param name="lockToken">锁值</param>
        /// <returns>是否已锁。</returns>
        public bool Lock(string key, string lockToken, TimeSpan expiry)
        {
            return _database.LockTake(key, lockToken, expiry);
        }

        /// <summary>
        /// 释放锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <param name="lockToken">锁值</param>
        /// <returns>是否成功。</returns>
        public bool UnLock(string key, string lockToken)
        {
            return _database.LockRelease(key, lockToken);
        }

        /// <summary>
        /// 异步获取锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <param name="expiry">过期时间。</param>
        /// <param name="lockToken">锁值</param>
        /// <returns>是否成功。</returns>
        public async Task<bool> LockAsync(string key, string lockToken, TimeSpan expiry)
        {
            return await _database.LockTakeAsync(key, lockToken, expiry);
        }

        /// <summary>
        /// 异步释放锁。
        /// </summary>
        /// <param name="key">锁名称。</param>
        /// <param name="lockToken">锁值</param>
        /// <returns>是否成功。</returns>
        public async Task<bool> UnLockAsync(string key, string lockToken)
        {
            return await _database.LockReleaseAsync(key, lockToken);
        }

        #endregion
    }
}
