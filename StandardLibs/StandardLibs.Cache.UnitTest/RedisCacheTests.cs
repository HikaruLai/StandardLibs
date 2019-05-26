using System;
using System.Collections.Generic;
using Xunit;
using NLog;


namespace StandardLibs.Cache.UnitTest
{
    public class RedisCacheTests
    {
        private static NLog.ILogger logger = LogManager.GetCurrentClassLogger();
        ICacheHelper<TestDO> cacheHelper = null;

        [Fact]
        public void Test01Add()
        {
            string key = "jsonTest01";
            string value = "Hello Redis!";

            TestDO expected = new TestDO { Key = key, value1 = 111, value2 = new byte[] { 0x01, 0x02, 0x03 }, value3 = value };

            //Try add to Redis
            this.cacheHelper.Add(key, expected);
            Assert.True(this.cacheHelper.Exists(key));
            TestDO result = this.cacheHelper.Get(key);
            logger.Debug($"Get add key:value => {0}:{1}", key, result); //It's in Redis
            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Test02Get()
        {
            string key = "jsonTest02";
            TestDO expected = new TestDO
            {
                Key = key,
                value1 = 77,
                value2 = new byte[] { 0x01, 0x02, 0x03, 0x04 },
                value3 = @"Hello redis@@!"
            };
            //Try add to Redis
            this.cacheHelper.Add(key, expected);
            Assert.True(this.cacheHelper.Exists(key));
            TestDO result = this.cacheHelper.Get(key);
            logger.Debug($"Get key:value => {0}:{1}", key, result); //It's in Redis
            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Test03Modify()
        {
            string key = "jsonTest03";
            TestDO old = new TestDO
            {
                Key = key,
                value1 = 77,
                value2 = new byte[] { 0x01, 0x02, 0x03, 0x04 },
                value3 = @"Hello redis@@!"
            };
            //Try add to Redis
            this.cacheHelper.Add(key, old);
            Assert.True(this.cacheHelper.Exists(key));
            TestDO result = this.cacheHelper.Get(key);
            logger.Debug($"Get orig key:value => {0}:{1}", key, result); //It's in Redis
            
            TestDO expected = new TestDO
            {
                Key = key,
                value1 = 66,
                value2 = new byte[] { 0x02, 0x03, 0x04 },
                value3 = @"Hello redis@@!"
            };
            this.cacheHelper.Add(key, expected);
            result = this.cacheHelper.Get(key);
            logger.Debug($"Get new key:value => {0}:{1}", key, result); //It's in Redis
            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Test04Delete()
        {
            string key = "jsonTest04";
            TestDO old = new TestDO
            {
                Key = key,
                value1 = 77,
                value2 = new byte[] { 0x01, 0x02, 0x03, 0x04 },
                value3 = @"Hello redis@@!"
            };
            //Try add to Redis
            this.cacheHelper.Add(key, old);
            TestDO result = this.cacheHelper.Get(key);
            logger.Debug($"Exits key:value => {0}:{1}", key, result); //It's in Redis
            this.cacheHelper.Remove(key);
            result = this.cacheHelper.Get(key);
            logger.Debug($"After remove key:value => {0}:{1}", key, result);
            Assert.Null(this.cacheHelper.Get(key));
            
            Assert.False(this.cacheHelper.Exists(key));
        }

        [Fact]
        public void Test05Count()
        {
            long result = this.cacheHelper.Count();
            logger.Debug($"Cache count: {0}", result);
        }

        [Fact]
        public void Test06GetAllKeys()
        {
            IList<string> keys = this.cacheHelper.GetAllKeys();
            foreach (string key in keys)
            {
                logger.Debug($"{0}", key);
            }
        }

        [Fact]
        public void Test06GetAll()
        {
            IList<TestDO> objs = this.cacheHelper.GetAll();
            foreach (TestDO testDO in objs)
            {
                logger.Debug($"{0}", testDO);
            }
        }
    }
}
