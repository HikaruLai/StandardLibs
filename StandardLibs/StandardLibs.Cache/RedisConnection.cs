using System;
using StackExchange.Redis;

namespace StandardLibs.Cache
{
    public class RedisConnection
    {
        private Lazy<ConnectionMultiplexer> lazyConnection;
        private string Config { get; set; }

        public RedisConnection() : this("127.0.0.1:6379,abortConnect=false")
        { }

        public RedisConnection(string config)
        {
            this.Config = config;
            this.lazyConnection = new Lazy<ConnectionMultiplexer>
            (
                () => { return ConnectionMultiplexer.Connect(this.Config); }
            );
        }

        public ConnectionMultiplexer Connection
        {
            get
            {
                return this.lazyConnection.Value;
            }
        }

        public void ResetConnection()
        {
            try
            {
                this.lazyConnection.Value.Close();
                this.lazyConnection.Value.Dispose();
            }
            catch
            {
            }

            this.lazyConnection = new Lazy<ConnectionMultiplexer>
            (
                () =>{ return ConnectionMultiplexer.Connect(this.Config); }
            );
        }
    }
}
