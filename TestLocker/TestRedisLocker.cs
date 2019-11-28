using System;
using System.Threading;
using System.Threading.Tasks;
using Locker;
using SlugEnt.Locker;
using NUnit.Framework;

using StackExchange.Redis.Extensions.Core.Abstractions;


namespace SlugEnt.TestRedisLocker
{
	[Parallelizable(ParallelScope.Fixtures)]
	public class Tests
	{
		private RedisCacheClient _redisCacheClient;
		private RedisLocker _locker;
		private Random _idGenerator;


		private string _lockType1 = "A.App1";
		private string _lockType2 = "B.App1";
		private string _lockType3 = "B.App2";



		[OneTimeSetUp]
		public void InitialSetup()
		{
			// Connect to Redis
			RedisCommunicator redisCommunicator = new RedisCommunicator();
			redisCommunicator.TalkToRedis();

			_redisCacheClient = redisCommunicator.RedisCacheClient;

			// Now create a locker
			_locker = new RedisLocker(_redisCacheClient);

			// Setup random number generator
			_idGenerator = new Random();
		}


		[SetUp]
		public async Task Setup()
		{
			// Tell Redis to clear everything out of its system
			await _redisCacheClient.Db0.FlushDbAsync();
		}


		[Test]
		public async Task SetSimpleLock()
		{
			string id = _idGenerator.Next().ToString();

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(_lockType1, id), "A10:  Lock was not set");
		}



		[Test]
		public async Task SetLockExclusive()
		{
			string id = _idGenerator.Next().ToString();

			// Set Lock
			Assert.IsTrue(await _locker.SetLockExclusive(_lockType1, id), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj =  await _locker.LockInfo(_lockType1, id);
			Assert.AreEqual(LockType.Exclusive, lockObj.Type);
		}



		[Test]
		public async Task SetLockReadOnly()
		{
			string id = _idGenerator.Next().ToString();

			// Set Lock
			Assert.IsTrue(await _locker.SetLockReadOnly(_lockType1, id), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj = await _locker.LockInfo(_lockType1, id);
			Assert.AreEqual(LockType.ReadOnly, lockObj.Type);
		}



		[Test]
		public async Task SetLockAppLevel1()
		{
			string id = _idGenerator.Next().ToString();

			// Set Lock
			Assert.IsTrue(await _locker.SetLockAppLevel1(_lockType1, id), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj = await _locker.LockInfo(_lockType1, id);
			Assert.AreEqual(LockType.AppLevel1, lockObj.Type);
		}




		[Test]
		public async Task SetLockAppLevel2()
		{
			string id = _idGenerator.Next().ToString();

			// Set Lock
			Assert.IsTrue(await _locker.SetLockAppLevel2(_lockType1, id), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj = await _locker.LockInfo(_lockType1, id);
			Assert.AreEqual(LockType.AppLevel2, lockObj.Type);
		}



		[Test]
		public async Task SetLockAppLevel3()
		{
			string id = _idGenerator.Next().ToString();

			// Set Lock
			Assert.IsTrue(await _locker.SetLockAppLevel3(_lockType1, id), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj = await _locker.LockInfo(_lockType1, id);
			Assert.AreEqual(LockType.AppLevel3, lockObj.Type);
		}



		// Validates that we can delete a lock
		[Test]
		public async Task DeleteLock()
		{
			string id = _idGenerator.Next(4, 4000).ToString();

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(_lockType1, id), "A10:  Lock was not set");

			// Delete the lock
			Assert.IsTrue(await _locker.DeleteLock(_lockType1, id), "A20:  Deletion of Lock failed");

			// Get count of locks
			Assert.AreEqual(0, await _locker.LockCount(_lockType1));
		}


		// Validates that LockCount works correctly
		[Test]
		public async Task LockCount()
		{
			// Get initial count of locks
			Assert.AreEqual(0, await _locker.LockCount(_lockType1), "A10:  Initial Lock count should be zero");

			// Set a random number of locks
			int maxLocks = await GenerateRandomLocks(_lockType1, 3, 10);
			Assert.AreEqual(maxLocks, await _locker.LockCount(_lockType1), "A110:  Expected there to be {0} locks", maxLocks);
		}


		// Validate that lock count only takes into account locks of the specified type.
		[Test]
		public async Task LockCount_OnlyCountsTheSpecifiedLockType()
		{
			// Generate random number of locks for 3 different lock types
			int lockCount1 = await GenerateRandomLocks(_lockType1, 4, 10);
			int lockCount2 = await GenerateRandomLocks(_lockType2, 12, 20);
			int lockCount3 = await GenerateRandomLocks(_lockType3, 2, 15);

			Assert.AreEqual(lockCount1, await _locker.LockCount(_lockType1));
			Assert.AreEqual(lockCount2, await _locker.LockCount(_lockType2));
			Assert.AreEqual(lockCount3, await _locker.LockCount(_lockType3));
		}


		// Validate that Lock Exists method works
		[Test]
		public async Task LockExists_WorksCorrectly()
		{
			// Create a lock
			string id = _idGenerator.Next().ToString();

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(_lockType1, id), "A10:  Lock was not set");

			// Make sure it exists
			Assert.IsTrue(await _locker.Exists(_lockType1, id), "A20:  Lock should have existed after setting, but does not.");

			// Delete the lock
			Assert.IsTrue(await _locker.DeleteLock(_lockType1, id), "A30:  Deletion of lock failed.");

			// Make sure it does not exist
			Assert.IsFalse(await _locker.Exists(_lockType1, id), "A40:  Lock no longer should have existed after deleting, but it does");
		}


		// Validates that we are able to delete all locks of a specific type and that no other lock types are affected.
		[Test]
		public async Task DeleteAllLocks_OfSpecificType_Works()
		{
			// Generate random number of locks for 3 different lock types
			int lockCount1 = await GenerateRandomLocks(_lockType1, 4, 20);
			int lockCount2 = await GenerateRandomLocks(_lockType2, 12, 30);
			int lockCount3 = await GenerateRandomLocks(_lockType3, 2, 45);

			Assert.AreEqual(lockCount1, await _locker.LockCount(_lockType1));
			Assert.AreEqual(lockCount2, await _locker.LockCount(_lockType2));
			Assert.AreEqual(lockCount3, await _locker.LockCount(_lockType3));

			// Now delete all locks of lock type 2
			await _locker.DeleteAllLocksForlockCategory(_lockType2);

			// Verify counts
			Assert.AreEqual(lockCount1, await _locker.LockCount(_lockType1));
			Assert.AreEqual(0, await _locker.LockCount(_lockType2));
			Assert.AreEqual(lockCount3, await _locker.LockCount(_lockType3));
		}


		// Validates that Flush All Locks works
		[Test]
		public async Task DeleteAllLocks_OfAllTypes()
		{
			// Generate random number of locks for 3 different lock types
			int lockCount1 = await GenerateRandomLocks(_lockType1, 4, 20);
			int lockCount2 = await GenerateRandomLocks(_lockType2, 12, 30);
			int lockCount3 = await GenerateRandomLocks(_lockType3, 2, 45);

			Assert.AreEqual(lockCount1, await _locker.LockCount(_lockType1));
			Assert.AreEqual(lockCount2, await _locker.LockCount(_lockType2));
			Assert.AreEqual(lockCount3, await _locker.LockCount(_lockType3));

			// Now delete all locks of lock type 2
			await _locker.FlushAllLocks();

			// Verify counts
			Assert.AreEqual(0, await _locker.LockCount(_lockType1));
			Assert.AreEqual(0, await _locker.LockCount(_lockType2));
			Assert.AreEqual(0, await _locker.LockCount(_lockType3));

		}


		// Validates that updating expiration time works.
		[Test]
		public async Task LockExpirationWorks()
		{
			string id = _idGenerator.Next(1000, 9999).ToString();
			int expireSeconds = 2;

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(_lockType1, id, LockType.Exclusive, expireSeconds), "A10:  Lock was not set");

			// Validate Lock exists
			Assert.IsTrue(await _locker.Exists(_lockType1, id));

			// Update lock expiration to 7 seconds
			Assert.IsTrue(await _locker.UpdateLockExpirationTime(_lockType1, id, 7));

			// Sleep 5 seconds - enough time that original expiration would have happened.
			Thread.Sleep(5000);

			// Validate Lock is still there
			Assert.IsTrue(await _locker.Exists(_lockType1, id));

			// Sleep again - enough time to allow for the lock to expire with new expiration amount
			Thread.Sleep(3000);
			Assert.IsFalse(await _locker.Exists(_lockType1, id));
		}


		[Test]
		public async Task UpdateLockExpirationWorks()
		{
			string id = _idGenerator.Next(34000, 49999).ToString();
			int expireSeconds = 2;

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(_lockType1, id, LockType.Exclusive, expireSeconds), "A10:  Lock was not set");

			// Validate Lock exists
			Assert.IsTrue(await _locker.Exists(_lockType1, id));

			// Sleep 2 seconds.
			Thread.Sleep(2000);

			// Validate Lock is gone
			Assert.IsFalse(await _locker.Exists(_lockType1, id));

		}


		[TestCase(LockType.Exclusive)]
		[TestCase(LockType.ReadOnly)]
		[TestCase(LockType.AppLevel1)]
		[TestCase(LockType.AppLevel2)]
		[TestCase(LockType.AppLevel3)]
		[Test]
		public async Task GetExistingLock (LockType lockType) {
			string id = _idGenerator.Next(34000, 49999).ToString();

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(_lockType1, id, lockType,300), "A10:  Lock was not set");

			// Validate Lock exists
			Assert.IsTrue(await _locker.Exists(_lockType1, id));

			// Now get the lock
			LockObject lockObject = await _locker.GetLock(_lockType1, id);
			Assert.AreEqual(id, lockObject.ID);
			Assert.AreEqual(lockType,lockObject.Type);
		}


		// Generates a random number of locks and sets them in Redis
		internal async Task<int> GenerateRandomLocks(string lockType, int min, int max)
		{
			Random lockNumber = new Random();
			int maxLocks = new Random().Next(min, max);

			for (int i = 0; i < maxLocks; i++)
			{
				string id = _idGenerator.Next(1000, 99999999).ToString();
				Assert.IsTrue(await _locker.SetLock(lockType, id), "A1000:  Lock was not set");
			}

			return maxLocks;
		}
	}


}