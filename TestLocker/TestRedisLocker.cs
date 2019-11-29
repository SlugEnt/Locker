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
		private UniqueKeys _uniqueKeys;

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

			// Create Unique Key object to provide unique LockCategories per test, so they can run in parallel without clobbering each other
			_uniqueKeys = new UniqueKeys();
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
			string lockCategory = _uniqueKeys.GetKey("SSL");

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(lockCategory, id), "A10:  Lock was not set");
		}



		[Test]
		public async Task SetLockExclusive()
		{
			string id = _idGenerator.Next().ToString();
			string lockCategory = _uniqueKeys.GetKey("SLE");

			// Set Lock
			Assert.IsTrue(await _locker.SetLockExclusive(lockCategory, id), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj =  await _locker.LockInfo(lockCategory, id);
			Assert.AreEqual(LockType.Exclusive, lockObj.Type);
		}



		[Test]
		public async Task SetLockReadOnly()
		{
			string id = _idGenerator.Next().ToString();
			string lockCategory = _uniqueKeys.GetKey("SLRO");


			// Set Lock
			Assert.IsTrue(await _locker.SetLockReadOnly(lockCategory, id), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj = await _locker.LockInfo(lockCategory, id);
			Assert.AreEqual(LockType.ReadOnly, lockObj.Type);
		}



		[Test]
		public async Task SetLockAppLevel1()
		{
			string id = _idGenerator.Next().ToString();
			string lockCategory = _uniqueKeys.GetKey("SLAL1");

			// Set Lock
			Assert.IsTrue(await _locker.SetLockAppLevel1(lockCategory, id), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj = await _locker.LockInfo(lockCategory, id);
			Assert.AreEqual(LockType.AppLevel1, lockObj.Type);
		}




		[Test]
		public async Task SetLockAppLevel2()
		{
			string id = _idGenerator.Next().ToString();
			string lockCategory = _uniqueKeys.GetKey("SLAL2");

			// Set Lock
			Assert.IsTrue(await _locker.SetLockAppLevel2(lockCategory, id), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj = await _locker.LockInfo(lockCategory, id);
			Assert.AreEqual(LockType.AppLevel2, lockObj.Type);
		}



		[Test]
		public async Task SetLockAppLevel3()
		{
			string id = _idGenerator.Next().ToString();
			string lockCategory = _uniqueKeys.GetKey("SLAL3");

			// Set Lock
			Assert.IsTrue(await _locker.SetLockAppLevel3(lockCategory, id), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj = await _locker.LockInfo(lockCategory, id);
			Assert.AreEqual(LockType.AppLevel3, lockObj.Type);
		}


		
		// Test the following things for each LockType:
		//  - Calling the basic SetLock method with the Type of Lock
		//  - Calling the basic SetLock method with the Type of Lock and overriding the lock TTL
		//  - Calling the Specific SetLock method
		//  - Calling thje Specific SetLock method and override the Lock TTL
		//  We validate:
		//   - The correct Type of lock was created
		//   - The lock expiration was set correctly.
		[Test]
		public async Task SetLockFullSuiteTests ([Range((int)LockType.ReadOnly, (int)LockType.AppLevel3)] int lockTypeInt) {
			// We use our own locker with its own Redis DB for this test so we can adjust TTL's
			RedisLocker rl = new RedisLocker(_redisCacheClient,1,true);
			await rl.FlushAllLocks();
			rl.TTL = 300;
			int ttl2 = 2000;

			LockType lockType = (LockType) lockTypeInt;

			string id1 = _idGenerator.Next(1000,19999).ToString();
			string lockCategory = _uniqueKeys.GetKey("SLD");

			// Set Lock using base lock method
			Assert.IsTrue(await rl.SetLock(lockCategory, id1,lockType), "A10:  Base SetLock did not work for LockType: {0}", lockType);

			// Make sure the correct type of Lock was set.
			LockObject lockObj = await rl.LockInfo(lockCategory, id1);
			Assert.AreEqual(lockType, lockObj.Type, "A11:  Lock was supposed to be set to {0}, but actually was {1}", lockType,lockObj.Type);

			// Now create a standard lock, but with a time override
			string id4 = _idGenerator.Next(4000, 4999).ToString();
			Assert.IsTrue(await rl.SetLock(lockCategory, id4, lockType,ttl2), "A12:  Base SetLock with time override did not work for LockType: {0}", lockType);


			string id2 = _idGenerator.Next(2000, 2999).ToString();
			string id3 = _idGenerator.Next(3000, 3999).ToString();
			

			// Now set lock using Specific Method
			switch ( lockType ) {
				case LockType.Exclusive: 
					Assert.IsTrue(await rl.SetLockExclusive(lockCategory,id2), "A20: Exclusive SetLock failed.");
					Assert.IsTrue(await rl.SetLockExclusive(lockCategory, id3,ttl2), "A20B: Exclusive SetLock failed.");
					break;
				case LockType.ReadOnly:
					Assert.IsTrue(await rl.SetLockReadOnly(lockCategory, id2), "A21: ReadOnly SetLock failed.");
					Assert.IsTrue(await rl.SetLockReadOnly(lockCategory, id3, ttl2), "A21B: ReadOnly SetLock failed.");

					break;
				case LockType.AppLevel1:
					Assert.IsTrue(await rl.SetLockAppLevel1(lockCategory, id2), "A23: AppLevel1 SetLock failed.");
					Assert.IsTrue(await rl.SetLockAppLevel1(lockCategory, id3, ttl2), "A23B: AppLevel1 SetLock failed.");
					break;
				case LockType.AppLevel2:
					Assert.IsTrue(await rl.SetLockAppLevel2(lockCategory, id2), "A24: AppLevel2 SetLock failed.");
					Assert.IsTrue(await rl.SetLockAppLevel2(lockCategory, id3, ttl2), "A24B: AppLevel2 SetLock failed.");
					break;
				case LockType.AppLevel3:
					Assert.IsTrue(await rl.SetLockAppLevel3(lockCategory, id2), "A25: AppLevel3 SetLock failed.");
					Assert.IsTrue(await rl.SetLockAppLevel3(lockCategory, id3, ttl2), "A25B: AppLevel3 SetLock failed.");
					break;
			}

			lockObj = await rl.LockInfo(lockCategory, id2);
			Assert.AreEqual(lockType, lockObj.Type, "A29:  Lock was supposed to be set to {0}, but actually was {1}", lockType, lockObj.Type);

			// Validate all default ttl locks are expired - indicating they were using the default ttl
			Thread.Sleep(rl.TTL);
			Assert.IsFalse(await rl.Exists(lockCategory,id1), "A50:  The first lock did not expire on time!");
			Assert.IsFalse(await rl.Exists(lockCategory,id2),"A51:  The second lock did not expire on time!");

			Thread.Sleep(ttl2 - rl.TTL + 100);
			Assert.IsFalse(await rl.Exists(lockCategory, id3), "A52:  The second override time lock did not expire on time!");
			Assert.IsFalse(await rl.Exists(lockCategory, id4), "A53:  The first override time lock did not expire on time!");


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
		public async Task UpdateLockExpirationWorks()
		{
			string id = _idGenerator.Next(1000, 9999).ToString();
			int expireMSeconds = 2000;

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(_lockType1, id, LockType.Exclusive, expireMSeconds), "A10:  Lock was not set");

			// Validate Lock exists
			Assert.IsTrue(await _locker.Exists(_lockType1, id));

			// Update lock expiration to 7 seconds
			Assert.IsTrue(await _locker.UpdateLockExpirationTime(_lockType1, id, 7000));

			// Sleep 5 seconds - enough time that original expiration would have happened.
			Thread.Sleep(5000);

			// Validate Lock is still there
			Assert.IsTrue(await _locker.Exists(_lockType1, id));

			// Sleep again - enough time to allow for the lock to expire with new expiration amount
			Thread.Sleep(3000);
			Assert.IsFalse(await _locker.Exists(_lockType1, id));
		}


		[Test]
		public async Task LockExpirationWorks()
		{
			string id = _idGenerator.Next(34000, 49999).ToString();
			int expireMSeconds = 2000;

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(_lockType1, id, LockType.Exclusive, expireMSeconds), "A10:  Lock was not set");

			// Validate Lock exists
			Assert.IsTrue(await _locker.Exists(_lockType1, id));

			// Sleep 2.2 seconds.
			Thread.Sleep(2200);

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


		// Validate that the TTL value on the Locker is used for the default lock duration
		[Test]
		public async Task LockTTLSetCorrectly () {
			string id = _idGenerator.Next(34000, 49999).ToString();

			// Create our own custom Locker for this experiment
			RedisLocker testLocker =new RedisLocker(_redisCacheClient,0,false);
			int ttl = 3300;

			testLocker.TTL = ttl;
			Assert.AreEqual(ttl,testLocker.TTL);

			// Create a lock with no time in method, to see if default is used.
			Assert.IsTrue(await testLocker.SetLock("abc",id));
			Thread.Sleep( ttl + 250);
			Assert.IsFalse(await testLocker.Exists("abc", id));
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