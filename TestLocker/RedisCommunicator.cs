using System;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;


// TODO This can be refactored at some point. But it allows us to test, which is the important thing.
namespace SlugEnt.TestRedisLocker
{
	class RedisCommunicator
	{
		public const int RED_LOCKER = 0;

		private RedisCacheClient _redisCacheClient;


		public RedisCommunicator() { }

		public bool TalkToRedis()
		{
			var serializer = new NewtonsoftSerializer();
			var config = new RedisConfiguration();

			config.Hosts = new[]
			{
				new RedisHost { Host = "192.168.1.92", Port = 6379}
			};


			config.ConnectTimeout = 1000;
			config.SyncTimeout = 900;

			//TODO Wondering if this should not be something we turn on just when we need it?
			// Enable admin mode to allow flushing of the database
			config.AllowAdmin = true;

			SingleRedisPool singleRedisPool = new SingleRedisPool(config);
			_redisCacheClient = new RedisCacheClient(singleRedisPool, serializer, config);

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



internal class SingleRedisPool : IRedisCacheConnectionPoolManager
{
	private readonly RedisConfiguration redisConfiguration;

	public SingleRedisPool(RedisConfiguration redisConfiguration)
	{
		this.redisConfiguration = redisConfiguration;
	}

	public void Dispose()
	{
		redisConfiguration.Connection.Dispose();
	}

	public IConnectionMultiplexer GetConnection()
	{
		return redisConfiguration.Connection;
	}
}
