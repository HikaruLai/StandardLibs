using System;
using LightInject;
using NLog;
using Xunit;
using StandardLibs.Utility;
using StandardLibs.Dna;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace StandardLibs.Cache.UnitTest
{
    public class RedisOprationTests
    {
        private RedisConnection redis; // ConnectionMultiplexer.Connect( "localhost" );
        private static NLog.ILogger logger = LogManager.GetCurrentClassLogger();

        public RedisOprationTests()
        {
            this.redis = Framework.Container.GetInstance<RedisConnection>("redis");
        }

        [Fact]
        public void Test01RetriveString()
        {
            string key = "keyTest";
            string value = "Hello Redis!";

            //Try and retrieve from Redis
            var db = this.redis.Connection.GetDatabase();
            if (!db.KeyExists(key))
            {
                if (db.StringSet(key, value)) //Add to Redis
                {
                    logger.Debug($"Success set key:valule => {0}:{1}", key, value);
                }
            }
            RedisValue redisValue = db.StringGet(key);
            logger.Debug($"Get key:value => {0}:{1}", key, redisValue); //It's in Redis - return it
        }

        [Fact]
        public void Test02RetriveJson()
        {
            string key = "jsonTest";
            TestDO value = new TestDO
            {
                Key = key,
                value1 = 77,
                value2 = new byte[] { 0x01, 0x02, 0x03 },
                value3 = "jsonTest with key 77"
            };
            var db = this.redis.Connection.GetDatabase();
            //Try and retrieve from Redis
            if (!db.KeyExists(key))
            {
                if (db.StringSet(key, value.ToString())) //Add to Redis
                {
                    logger.Debug($"Success set key:valule => {0}:{1}", key, value);
                }
            }
            RedisValue redisValue = db.StringGet(key);
            TestDO result = JsonConvert.DeserializeObject<TestDO>(redisValue.ToString());
            logger.Debug($"Get key:value => {0}:{1}", key, result); //It's in Redis - return it
        }

        [Fact]
        public void Test03ModifyJson()
        {
            string key = "jsonTest";
            TestDO value = new TestDO
            {
                Key = key,
                value1 = 66,
                value2 = new byte[] { 0x02, 0x03, 0x04 },
                value3 = "jsonTest with key 66"
            };
            var db = this.redis.Connection.GetDatabase();
            if (db.KeyExists(key))
            {
                TestDO orig = JsonConvert.DeserializeObject<TestDO>(db.StringGet(key).ToString());
                logger.Debug($"Exists key:value => {0}:{1}", key, orig);
            }
            // modify it 
            db.StringSet(key, value.ToString(), null, When.Always);
            RedisValue redisValue = db.StringGet(key);
            TestDO result = JsonConvert.DeserializeObject<TestDO>(redisValue.ToString());
            logger.Debug($"Get key:value => {0}:{1}", key, result); //It's in Redis - return it
        }

        [Fact]
        public void Test04RemoveJson()
        {
            var db = this.redis.Connection.GetDatabase();
            string key = "jsonTest";
            if (db.KeyExists(key))
            {
                TestDO orig = JsonConvert.DeserializeObject<TestDO>(db.StringGet(key).ToString());
                logger.Debug($"Exists key:value => {0}:{1}", key, orig);
                // Remove it!
                Assert.True(db.KeyDelete(key));
            }
            Assert.False(db.KeyExists(key));
        }
    }

    public class TestDO : JsonComparable
    {
        public string Key { get; set; }
        public int value1 { get; set; }

        [JsonConverter(typeof(ByteArrayConvertor))]
        public byte[] value2 { get; set; }
        public string value3 { get; set; }
    }
}
