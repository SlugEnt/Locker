using System;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Core.Models;
using StackExchange.Redis.Extensions.Newtonsoft;


// TODO This can be refactored at some point. But it allows us to test, which is the important thing.
namespace SlugEnt.TestRedisLocker
{
	/// <summary>
	/// Class used to establish a connection to a Redis Server
	/// </summary>
	class RedisCommunicator
	{
		public const int RED_LOCKER = 0;

		private RedisCacheClient _redisCacheClient;
		private IRedisCacheConnectionPoolManager _connectionPoolManager;


		public RedisCommunicator() { }

		public bool TalkToRedis()
		{
			var serializer = new NewtonsoftSerializer();
			RedisConfiguration redisConfig = new RedisConfiguration();

			redisConfig.Hosts = new[]
			{
				new RedisHost { Host = "192.168.1.71", Port = 6379}
			};

			redisConfig.Password = "redis2020";
			redisConfig.ConnectTimeout = 1000;
			redisConfig.SyncTimeout = 900;

			// Enable admin mode to allow flushing of the database
			redisConfig.AllowAdmin = true;

			_connectionPoolManager = new RedisCacheConnectionPoolManager(redisConfig);
			_redisCacheClient = new RedisCacheClient(_connectionPoolManager,serializer,redisConfig);

			return true;
		}


		/// <summary>
		/// Returns the RedisCacheClient so that it may be accesed and used to talk to Redis.
		/// </summary>
		public RedisCacheClient RedisCacheClient
		{
			get { return _redisCacheClient; }
		}
	}
}

