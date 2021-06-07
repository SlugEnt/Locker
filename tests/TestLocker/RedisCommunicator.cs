using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;


// TODO This can be refactored at some point. But it allows us to test, which is the important thing.
namespace SlugEnt.TestLocker
{
    /// <summary>
    ///     Class used to establish a connection to a Redis Server
    /// </summary>
    public class RedisCommunicator
    {
        public const int RED_LOCKER = 0;

        private RedisCacheClient _redisCacheClient;
        private IRedisCacheConnectionPoolManager _connectionPoolManager;


        public RedisCommunicator() { }


        public bool TalkToRedis()
        {
            NewtonsoftSerializer serializer = new NewtonsoftSerializer();
            RedisConfiguration redisConfig = new RedisConfiguration
            {
                Hosts = new[]
                {
                    new RedisHost { Host = "192.168.100.21", Port = 6379 }
                },
                Password = "redis2020",
                ConnectTimeout = 1000,
                SyncTimeout = 900,
                AllowAdmin = true // Enable admin mode to allow flushing of the database
            };

            _connectionPoolManager = new RedisCacheConnectionPoolManager(redisConfig);
            _redisCacheClient = new RedisCacheClient(_connectionPoolManager, serializer, redisConfig);

            return true;
        }


        /// <summary>
        ///     Returns the RedisCacheClient so that it may be accesed and used to talk to Redis.
        /// </summary>
        public RedisCacheClient RedisCacheClient => _redisCacheClient;
    }
}