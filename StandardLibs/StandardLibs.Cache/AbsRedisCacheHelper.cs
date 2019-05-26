using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Newtonsoft.Json;
using StandardLibs.Utility;

namespace StandardLibs.Cache
{
    public abstract class AbsRedisCacheHelper<T> : ICacheHelper<T>
    {
        private ILogger logger { get; set; }

        /// <summary>
        /// Connection helper
        /// </summary>
        protected RedisConnection redisConnection { private get; set; }

        protected string keyPrefix = string.Empty;
        public string KeyPrefix
        {
            get { return this.keyPrefix; }
            set { this.keyPrefix = value; }
        }

        // timeout seconds
        protected int ttl = -1; // if ttl < 0 , no expired
        public int Ttl
        {
            get { return this.ttl; }
            set { this.ttl = value; }
        }  

        public IDateUtility DateUtility { private get; set; }

        public AbsRedisCacheHelper(ILogger logger, RedisConnection cacheConnection, string keyPrefix, IDateUtility dateUtility, int ttl = 60)
        {
            this.logger = logger;
            this.redisConnection = cacheConnection;
            this.keyPrefix = keyPrefix;
            this.DateUtility = dateUtility;
            this.Ttl = ttl;
        }

        public string GetExpiredDT()
        {
            DateTime dt = this.DateUtility.GetAddSecondsNow(this.Ttl);
            return this.DateUtility.GetStrByDateTime(dt).Substring(0, 14);
        }

        public bool IsExpired(string key)
        {
            try
            {
                return EqualityComparer<T>.Default.Equals(this.Get(key), default(T));
            }
            catch (Exception ex)
            {
                logger.LogWarning($"IsExpired exception:{0}", ex.Message);
                return true;
            }
        }

        public virtual void Add(string key, T obj)
        {
            this.AddEx(key, obj, this.ttl);
        }

        public virtual void AddEx(string key, T obj, int ttl)
        {
            IDatabase db = null;
            key = this.KeyPrefix + key;
            try
            {
                db = this.redisConnection.Connection.GetDatabase();
                string jstr = JsonConvert.SerializeObject(obj);
                if (ttl > 0)
                {
                    if (db.StringSet(key, jstr, TimeSpan.FromSeconds(ttl)))
                    {
                        logger.LogDebug($"Cache add key success => {0}:{1}:{2}", key, jstr, ttl);
                    }
                    else
                    {
                        logger.LogDebug($"Cache add key fail => {0}:{1}:{2}", key, jstr, ttl);
                    }
                }
                else
                {
                    if (db.StringSet(key, jstr, null, When.Always))
                    {
                        logger.LogDebug($"Cache add key success => {0}:{1}", key, jstr);
                    }
                    else
                    {
                        logger.LogDebug($"Cache add key fail => {0}:{1}", key, jstr);
                    }
                }
            }
            catch (RedisConnectionException rsex)
            {
                string errMsg = string.Format("Cache add key[{0}] fail: {1}, {2}", key, rsex.Message, rsex.StackTrace);
                logger.LogError($"{0}", errMsg);
                this.redisConnection.ResetConnection();
                throw new Exception(errMsg);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Cache add key[{0}] fail:{1}, {2}", key, ex.Message, ex.StackTrace);
                logger.LogError($"{0}", errMsg);
                throw new Exception(errMsg);
            }
        }

        public virtual T Get(string key)
        {
            key = this.KeyPrefix + key;
            IDatabase db = null;
            T ret = default(T);
            try
            {
                db = this.redisConnection.Connection.GetDatabase();
                if (!db.KeyExists(key))
                {
                    logger.LogDebug($"Cache get key not exists! => {0}", key);
                    //It's not in Redis - return default
                    return default(T);
                }
                RedisValue redisValue = db.StringGet(key);
                string retStr = redisValue.ToString();
                ret = JsonConvert.DeserializeObject<T>(retStr);
                logger.LogDebug($"Cache get key:value => {0}:{1}", key, retStr);
                //It's in Redis - return it
                return ret;
            }
            catch (RedisConnectionException rsex)
            {
                string errMsg = string.Format("Cache get key[{0}] fail:{1}, {2}", key, rsex.Message, rsex.StackTrace);
                logger.LogError($"{0}", errMsg);
                this.redisConnection.ResetConnection();
                throw new Exception(errMsg);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Cache get key[{0}] fail:{1}, {2}", key, ex.Message, ex.StackTrace);
                logger.LogError($"{0}", errMsg);
                throw new Exception(errMsg);
            }
        }

        public virtual bool Exists(string key)
        {
            key = this.KeyPrefix + key;
            IDatabase db = null;
            try
            {
                db = this.redisConnection.Connection.GetDatabase();
                return db.KeyExists(key);
            }
            catch (RedisConnectionException rsex)
            {
                string errMsg = string.Format("Cache Exists key[{0}] fail:{1}, {2}", key, rsex.Message, rsex.StackTrace);
                logger.LogError($"{0}", errMsg);
                this.redisConnection.ResetConnection();
                throw new Exception(errMsg);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Cache Exists key[{0}] fail:{1}, {2}", key, ex.Message, ex.StackTrace);
                logger.LogError($"{0}", errMsg);
                throw new Exception(errMsg);
            }
        }

        public virtual void Remove(string key)
        {
            IDatabase db = null;
            key = this.KeyPrefix + key;
            try
            {
                db = this.redisConnection.Connection.GetDatabase();
                if (db.KeyExists(key))
                {
                    db.KeyDelete(key);
                }
                logger.LogDebug($"Cache remove key OK!=> {0}", key);
            }
            catch (RedisConnectionException rsex)
            {
                string errMsg = string.Format("Cache remove key[{0}] fail:{1}, {2}", key, rsex.Message, rsex.StackTrace);
                logger.LogError($"{0}", errMsg);
                this.redisConnection.ResetConnection();
                throw new Exception(errMsg);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Cache remove key[{0}] fail:{1}, {2}", key, ex.Message, ex.StackTrace);
                logger.LogError($"{0}", errMsg);
                throw new Exception(errMsg);
            }
        }

        public virtual IList<T> GetAll()
        {
            IList<T> objs = null;
            IDatabase db = null;
            string patStr = this.KeyPrefix + "*";
            try
            {
                db = this.redisConnection.Connection.GetDatabase();
                System.Net.EndPoint enp = this.redisConnection.Connection.GetEndPoints().First();
                var svr = this.redisConnection.Connection.GetServer(enp);
                objs = new List<T>();

                foreach (string key in svr.Keys(pattern: patStr).ToArray())
                {
                    try
                    {
                        RedisValue redisValue = db.StringGet(key);
                        string retStr = redisValue.ToString();
                        T ret = JsonConvert.DeserializeObject<T>(retStr);
                        objs.Add(ret);
                    }
                    catch
                    { }
                }
                return objs;
            }
            catch (RedisConnectionException rsex)
            {
                string errMsg = string.Format("Cache get all fail: {0}", rsex.StackTrace);
                logger.LogError($"{0}", errMsg);
                this.redisConnection.ResetConnection();
                throw new Exception(errMsg);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Cache get all fail: {0}", ex.StackTrace);
                logger.LogError($"{0}", errMsg);
                throw new Exception(errMsg);
            }
        }

        public virtual IList<string> GetAllKeys()
        {
            IList<string> keys = null;
            string patStr = this.KeyPrefix + "*";
            try
            {
                System.Net.EndPoint enp = this.redisConnection.Connection.GetEndPoints().First();
                var svr = this.redisConnection.Connection.GetServer(enp);
                keys = new List<string>();
                foreach (string key in svr.Keys(pattern: patStr).ToArray())
                {
                    int pos = key.IndexOf(this.keyPrefix);
                    //log.Debug(key);
                    if (pos == 0)  // first char
                    {
                        //log.Debug( key + ":" + pos );
                        keys.Add(key.Substring(this.keyPrefix.Length));
                    }
                }
                return keys;
            }
            catch (RedisConnectionException rsex)
            {
                string errMsg = string.Format("Cache get all keys fail: {0}", rsex.StackTrace);
                logger.LogError($"{0}", errMsg);
                this.redisConnection.ResetConnection();
                throw new Exception(errMsg);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Cache get all keys fail: {0}", ex.StackTrace);
                logger.LogError($"{0}", errMsg);
                throw new Exception(errMsg);
            }
        }

        public virtual long Count()
        {
            string patStr = this.KeyPrefix + "*";
            try
            {
                System.Net.EndPoint enp = this.redisConnection.Connection.GetEndPoints().First();
                var svr = this.redisConnection.Connection.GetServer(enp);
                return svr.Keys(pattern: patStr).LongCount();
            }
            catch (RedisConnectionException rsex)
            {
                string errMsg = string.Format("Cache count fail: {0}", rsex.StackTrace);
                logger.LogError($"{0}", errMsg);
                this.redisConnection.ResetConnection();
                throw new Exception(errMsg);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Cache count fail: {0}", ex.StackTrace);
                logger.LogError($"{0}", errMsg);
                throw new Exception(errMsg);
            }
        }
    }
}
