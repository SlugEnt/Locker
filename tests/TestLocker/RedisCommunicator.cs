/*
 * MIT License
 * Copyright (c) 2022 SlugEnt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

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