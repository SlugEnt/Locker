using System;
using System.Threading;
using System.Threading.Tasks;
using Locker;
using SlugEnt.Locker;
using NUnit.Framework;

using StackExchange.Redis.Extensions.Core.Abstractions;


namespace SlugEnt.TestRedisLocker
{
//	[Parallelizable(ParallelScope.Fixtures)]
	[Parallelizable(ParallelScope.All)]
	public class Tests
	{
		private RedisCacheClient _redisCacheClient;
		private RedisLocker _locker;
		private Random _idGenerator;
		private UniqueKeys _uniqueKeys;

	
		[OneTimeSetUp]
		public async Task InitialSetup()
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

			// Tell Redis to clear everything out of its system
			await _redisCacheClient.Db0.FlushDbAsync();

		}


		[SetUp]
		public void Setup()
		{
		}


		[Test]
		public async Task SetSimpleLock()
		{
			string id = _idGenerator.Next().ToString();
			string lockCategory = _uniqueKeys.GetKey("SSL");

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(lockCategory, id, "SetSimple"), "A10:  Lock was not set");
		}



		[Test]
		public async Task SetLockExclusive()
		{
			string id = _idGenerator.Next().ToString();
			string lockCategory = _uniqueKeys.GetKey("SLE");
			string comment = "Exclusive";

			// Set Lock
			Assert.IsTrue(await _locker.SetLockExclusive(lockCategory, id, comment), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj =  await _locker.LockInfo(lockCategory, id);
			Assert.AreEqual(LockType.Exclusive, lockObj.Type);
			Assert.AreEqual(comment,lockObj.Comment);
		}



		[Test]
		public async Task SetLockReadOnly()
		{
			string id = _idGenerator.Next().ToString();
			string lockCategory = _uniqueKeys.GetKey("SLRO");
			string lockComment = "ReadOnly";

			// Set Lock
			Assert.IsTrue(await _locker.SetLockReadOnly(lockCategory, id, lockComment), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj = await _locker.LockInfo(lockCategory, id);
			Assert.AreEqual(LockType.ReadOnly, lockObj.Type);
			Assert.AreEqual(lockComment, lockObj.Comment);
		}



		[Test]
		public async Task SetLockAppLevel1()
		{
			string id = _idGenerator.Next().ToString();
			string lockCategory = _uniqueKeys.GetKey("SLAL1");
			string lockComment = "AppL1";

			// Set Lock
			Assert.IsTrue(await _locker.SetLockAppLevel1(lockCategory, id, lockComment), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj = await _locker.LockInfo(lockCategory, id);
			Assert.AreEqual(LockType.AppLevel1, lockObj.Type);
			Assert.AreEqual(lockComment, lockObj.Comment);
		}




		[Test]
		public async Task SetLockAppLevel2()
		{
			string id = _idGenerator.Next().ToString();
			string lockCategory = _uniqueKeys.GetKey("SLAL2");
			string lockComment = "AppL2";

			// Set Lock
			Assert.IsTrue(await _locker.SetLockAppLevel2(lockCategory, id, lockComment), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj = await _locker.LockInfo(lockCategory, id);
			Assert.AreEqual(LockType.AppLevel2, lockObj.Type);
			Assert.AreEqual(lockComment, lockObj.Comment);
		}



		[Test]
		public async Task SetLockAppLevel3()
		{
			string id = _idGenerator.Next().ToString();
			string lockCategory = _uniqueKeys.GetKey("SLAL3");
			string lockComment = "AppL3";

			// Set Lock
			Assert.IsTrue(await _locker.SetLockAppLevel3(lockCategory, id, lockComment), "A10:  Lock was not set");

			// Make sure Exclusive Lock was set.
			LockObject lockObj = await _locker.LockInfo(lockCategory, id);
			Assert.AreEqual(LockType.AppLevel3, lockObj.Type);
			Assert.AreEqual(lockComment, lockObj.Comment);
		}


		
		// Test the following things for each LockType:
		//  - Calling the basic SetLock method with the Type of Lock
		//  - Calling the basic SetLock method with the Type of Lock and overriding the lock TTL in milliseconds
		//  - Calling the basic SetLock method with a Timespan
		//  - Calling the Specific SetLock method
		//  - Calling thje Specific SetLock method and override the Lock TTL
		//  - Calling the Specific SetLock method and override expiration with a TimeSpan
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
			string lockComment = "FX:11-01-19";

			LockType lockType = (LockType) lockTypeInt;

			string testID_1 = _idGenerator.Next(1000,19999).ToString();
			string lockCategory = _uniqueKeys.GetKey("SLFST");

			// TestID_1:  Set Lock using base lock method with default TTL
			Assert.IsTrue(await rl.SetLock(lockCategory, testID_1, lockComment, lockType), "A10:  Base SetLock did not work for LockType: {0}", lockType);

			// Make sure the correct type of Lock was set.
			LockObject lockObj = await rl.LockInfo(lockCategory, testID_1);
			Assert.AreEqual(lockType, lockObj.Type, "A11:  Lock was supposed to be set to {0}, but actually was {1}", lockType,lockObj.Type);

			// Make sure it's comment is on it.
			Assert.AreEqual(lockComment, lockObj.Comment, "A12:  Lock Comment is incorrect");

			// TestID_2:  Create a standard lock, but with a time in milli-second override
			string testID_2 = _idGenerator.Next(4000, 4999).ToString();
			Assert.IsTrue(await rl.SetLock(lockCategory, testID_2, lockComment, lockType, ttl2), "A13:  Base SetLock with time override did not work for LockType: {0}", lockType);

			// TestID_3:  Create a standard lock, but with a TimeSpan override
			string testID_3 = _idGenerator.Next(5000, 5999).ToString();
			TimeSpan t5 = new TimeSpan(0,0,0,2);
			Assert.IsTrue(await rl.SetLock(lockCategory, testID_3, lockComment, t5 ), "A14:  Base SetLock with time override did not work for LockType: {0}", lockType);
			// Make sure it's comment is on it.
			Assert.AreEqual(lockComment, lockObj.Comment, "A15:  Lock Comment is incorrect");

			// TestID_4-6:  Create a lock of a specific type and then test with default TTL (TestID_4), milli-sec override (TestID_5) and TimeSpan override (TestID_6)
			string testID_4 = _idGenerator.Next(2000, 2999).ToString();
			string testID_5 = _idGenerator.Next(3000, 3999).ToString();
			string testID_6 = _idGenerator.Next(6000, 6999).ToString();
			TimeSpan t6 = new TimeSpan(0,0,0,2);

			// Now set lock using Specific Method
			switch ( lockType ) {
				case LockType.Exclusive: 
					Assert.IsTrue(await rl.SetLockExclusive(lockCategory,testID_4, lockComment), "A20: Exclusive SetLock failed.");
					Assert.IsTrue(await rl.SetLockExclusive(lockCategory, testID_5, lockComment, ttl2), "A20B: Exclusive SetLock failed.");
					Assert.IsTrue(await rl.SetLockExclusive(lockCategory, testID_6, lockComment, t6), "A20C: Exclusive SetLock failed.");
					break;
				case LockType.ReadOnly:
					Assert.IsTrue(await rl.SetLockReadOnly(lockCategory, testID_4, lockComment), "A21: ReadOnly SetLock failed.");
					Assert.IsTrue(await rl.SetLockReadOnly(lockCategory, testID_5, lockComment, ttl2), "A21B: ReadOnly SetLock failed.");
					Assert.IsTrue(await rl.SetLockReadOnly(lockCategory, testID_6, lockComment, t6), "A21C: ReadOnly SetLock failed.");

					break;
				case LockType.AppLevel1:
					Assert.IsTrue(await rl.SetLockAppLevel1(lockCategory, testID_4, lockComment), "A23: AppLevel1 SetLock failed.");
					Assert.IsTrue(await rl.SetLockAppLevel1(lockCategory, testID_5, lockComment, ttl2), "A23B: AppLevel1 SetLock failed.");
					Assert.IsTrue(await rl.SetLockAppLevel1(lockCategory, testID_6, lockComment, t6), "A23C: AppLevel1 SetLock failed.");

					break;
				case LockType.AppLevel2:
					Assert.IsTrue(await rl.SetLockAppLevel2(lockCategory, testID_4, lockComment), "A24: AppLevel2 SetLock failed.");
					Assert.IsTrue(await rl.SetLockAppLevel2(lockCategory, testID_5, lockComment, ttl2), "A24B: AppLevel2 SetLock failed.");
					Assert.IsTrue(await rl.SetLockAppLevel2(lockCategory, testID_6, lockComment, t6), "A24C: AppLevel2 SetLock failed.");
					break;
				case LockType.AppLevel3:
					Assert.IsTrue(await rl.SetLockAppLevel3(lockCategory, testID_4, lockComment), "A25: AppLevel3 SetLock failed.");
					Assert.IsTrue(await rl.SetLockAppLevel3(lockCategory, testID_5, lockComment, ttl2), "A25B: AppLevel3 SetLock failed.");
					Assert.IsTrue(await rl.SetLockAppLevel3(lockCategory, testID_6, lockComment, t6), "A25C: AppLevel3 SetLock failed.");
					break;
			}

			// Validate the Lock was created correctly
			lockObj = await rl.LockInfo(lockCategory, testID_4);
			Assert.AreEqual(lockType, lockObj.Type, "A30:  Lock was supposed to be set to {0}, but actually was {1}", lockType, lockObj.Type);
			Assert.AreEqual(lockComment, lockObj.Comment, "A30A:  Lock Comment is incorrect");
			lockObj = await rl.LockInfo(lockCategory, testID_5);
			Assert.AreEqual(lockType, lockObj.Type, "A31:  Lock was supposed to be set to {0}, but actually was {1}", lockType, lockObj.Type);
			Assert.AreEqual(lockComment, lockObj.Comment, "A31A:  Lock Comment is incorrect");
			lockObj = await rl.LockInfo(lockCategory, testID_6);
			Assert.AreEqual(lockType, lockObj.Type, "A32:  Lock was supposed to be set to {0}, but actually was {1}", lockType, lockObj.Type);
			Assert.AreEqual(lockComment, lockObj.Comment, "A32A:  Lock Comment is incorrect");

			// Validate all default ttl locks are expired - indicating they were using the default ttl
			Thread.Sleep(rl.TTL);
			Assert.IsFalse(await rl.Exists(lockCategory,testID_1), "A50:  TestID_1 lock did not expire on time!");
			Assert.IsFalse(await rl.Exists(lockCategory,testID_4),"A51:   TestID_4 lock did not expire on time!");

			// Give the locks a chance to expire
			Thread.Sleep(ttl2);
			Assert.IsFalse(await rl.Exists(lockCategory, testID_5), "A52:  TestID_5 lock did not expire on time!");
			Assert.IsFalse(await rl.Exists(lockCategory, testID_2), "A53:  TestID_2 lock did not expire on time!");
			Assert.IsFalse(await rl.Exists(lockCategory, testID_3), "A54:  TestID_3 lock did not expire on time!");
			Assert.IsFalse(await rl.Exists(lockCategory, testID_6), "A55:  TestID_6 Lock did not expire on time!");

		}


		// Validates that we can delete a lock
		[Test]
		public async Task DeleteLock()
		{
			string testID_1 = _idGenerator.Next(1000, 19999).ToString();
			string lockCategory = _uniqueKeys.GetKey("DL");

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(lockCategory, testID_1, "DelLock"), "A10:  Lock was not set");

			// Delete the lock
			Assert.IsTrue(await _locker.DeleteLock(lockCategory, testID_1), "A20:  Deletion of Lock failed");

			// Get count of locks
			Assert.AreEqual(0, await _locker.LockCount(lockCategory));
		}


		// Validates that LockCount works correctly
		[Test]
		public async Task LockCount()
		{
			string testID_1 = _idGenerator.Next(1000, 19999).ToString();
			string lockCategory = _uniqueKeys.GetKey("LC22");

			// Get initial count of locks
			Assert.AreEqual(0, await _locker.LockCount(lockCategory), "A10:  Initial Lock count should be zero");

			// Set a random number of locks
			int maxLocks = await GenerateRandomLocks(lockCategory, 3, 10);
			Assert.AreEqual(maxLocks, await _locker.LockCount(lockCategory), "A110:  Expected there to be {0} locks", maxLocks);
		}


		// Validate that lock count only takes into account locks of the specified type.
		[Test]
		public async Task LockCount_OnlyCountsTheSpecifiedLockType()
		{
			string lockCategory = _uniqueKeys.GetKey("LCOCTSLT1");
			string lockCategory2 = _uniqueKeys.GetKey("LCOCTSLT2");
			string lockCategory3 = _uniqueKeys.GetKey("LCOCTSLT3");

			// Generate random number of locks for 3 different lock types
			int lockCount1 = await GenerateRandomLocks(lockCategory, 4, 10);
			int lockCount2 = await GenerateRandomLocks(lockCategory2, 12, 20);
			int lockCount3 = await GenerateRandomLocks(lockCategory3, 2, 15);

			Assert.AreEqual(lockCount1, await _locker.LockCount(lockCategory));
			Assert.AreEqual(lockCount2, await _locker.LockCount(lockCategory2));
			Assert.AreEqual(lockCount3, await _locker.LockCount(lockCategory3));
		}


		// Validate that Lock Exists method works
		[Test]
		public async Task LockExists_WorksCorrectly()
		{
			// Create a lock
			string id = _idGenerator.Next().ToString();
			string lockCategory = _uniqueKeys.GetKey("LEWC");

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(lockCategory, id,"LockExists"), "A10:  Lock was not set");

			// Make sure it exists
			Assert.IsTrue(await _locker.Exists(lockCategory, id), "A20:  Lock should have existed after setting, but does not.");

			// Delete the lock
			Assert.IsTrue(await _locker.DeleteLock(lockCategory, id), "A30:  Deletion of lock failed.");

			// Make sure it does not exist
			Assert.IsFalse(await _locker.Exists(lockCategory, id), "A40:  Lock no longer should have existed after deleting, but it does");
		}


		// Validates that we are able to delete all locks of a specific type and that no other lock types are affected.
		[Parallelizable(ParallelScope.None)]
		[Test]
		public async Task DeleteAllLocks_OfSpecificType_Works()
		{
			string lockCategory = _uniqueKeys.GetKey("DALOSTW1");
			string lockCategory2 = _uniqueKeys.GetKey("DALOSTW2");
			string lockCategory3 = _uniqueKeys.GetKey("DALOSTW3");

			// Generate random number of locks for 3 different lock types
			int lockCount1 = await GenerateRandomLocks(lockCategory, 4, 20);
			int lockCount2 = await GenerateRandomLocks(lockCategory2, 12, 30);
			int lockCount3 = await GenerateRandomLocks(lockCategory3, 2, 45);

			Assert.AreEqual(lockCount1, await _locker.LockCount(lockCategory));
			Assert.AreEqual(lockCount2, await _locker.LockCount(lockCategory2));
			Assert.AreEqual(lockCount3, await _locker.LockCount(lockCategory3));

			// Now delete all locks of lock type 2
			await _locker.DeleteAllLocksForlockCategory(lockCategory2);

			// Verify counts
			Assert.AreEqual(lockCount1, await _locker.LockCount(lockCategory));
			Assert.AreEqual(0, await _locker.LockCount(lockCategory2));
			Assert.AreEqual(lockCount3, await _locker.LockCount(lockCategory3));
		}


		// Validates that Flush All Locks works
		[Parallelizable(ParallelScope.None)]
		[Test]
		public async Task ZZZZZZDeleteAllLocks_OfAllTypes()
		{
			string lockCategory = _uniqueKeys.GetKey("DALOAT1");
			string lockCategory2 = _uniqueKeys.GetKey("DALOAT2");
			string lockCategory3 = _uniqueKeys.GetKey("DALOAT3");

			// Generate random number of locks for 3 different lock types
			int lockCount1 = await GenerateRandomLocks(lockCategory, 4, 20);
			int lockCount2 = await GenerateRandomLocks(lockCategory2, 12, 30);
			int lockCount3 = await GenerateRandomLocks(lockCategory3, 2, 45);


			Assert.AreEqual(lockCount1, await _locker.LockCount(lockCategory));
			Assert.AreEqual(lockCount2, await _locker.LockCount(lockCategory2));
			Assert.AreEqual(lockCount3, await _locker.LockCount(lockCategory3));

			// Now delete all locks
			await _locker.FlushAllLocks();

			// Verify counts
			Assert.AreEqual(0, await _locker.LockCount(lockCategory));
			Assert.AreEqual(0, await _locker.LockCount(lockCategory2));
			Assert.AreEqual(0, await _locker.LockCount(lockCategory3));
		}


		// Validates that updating expiration time works.
		[Test]
		public async Task UpdateLockExpirationWorks()
		{
			string id = _idGenerator.Next(1000, 9999).ToString();
			string lockCategory = _uniqueKeys.GetKey("ULEW");
			int expireMSeconds = 2000;

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(lockCategory, id, "Comment",LockType.Exclusive, expireMSeconds), "A10:  Lock was not set");

			// Validate Lock exists
			Assert.IsTrue(await _locker.Exists(lockCategory, id));

			// Update lock expiration to 7 seconds
			Assert.IsTrue(await _locker.UpdateLockExpirationTime(lockCategory, id, 7000));

			// Sleep 5 seconds - enough time that original expiration would have happened.
			Thread.Sleep(5000);

			// Validate Lock is still there
			Assert.IsTrue(await _locker.Exists(lockCategory, id));

			// Sleep again - enough time to allow for the lock to expire with new expiration amount
			Thread.Sleep(3000);
			Assert.IsFalse(await _locker.Exists(lockCategory, id));
		}


		[Test]
		public async Task LockExpirationWorks()
		{
			string id = _idGenerator.Next(34000, 49999).ToString();
			string lockCategory = _uniqueKeys.GetKey("LEW");
			int expireMSeconds = 2000;

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(lockCategory, id, "Comment",LockType.Exclusive, expireMSeconds), "A10:  Lock was not set");

			// Validate Lock exists
			Assert.IsTrue(await _locker.Exists(lockCategory, id));

			// Sleep 2.2 seconds.
			Thread.Sleep(2200);

			// Validate Lock is gone
			Assert.IsFalse(await _locker.Exists(lockCategory, id));

		}


		[TestCase(LockType.Exclusive)]
		[TestCase(LockType.ReadOnly)]
		[TestCase(LockType.AppLevel1)]
		[TestCase(LockType.AppLevel2)]
		[TestCase(LockType.AppLevel3)]
		[Test]
		public async Task GetExistingLock (LockType lockType) {
			string id = _idGenerator.Next(34000, 49999).ToString();
			string lockCategory = _uniqueKeys.GetKey("GEL");
			string lockComment = "Internal:12-10-96:male";

			// Set Lock
			Assert.IsTrue(await _locker.SetLock(lockCategory, id, lockComment,lockType,300), "A10:  Lock was not set");

			// Validate Lock exists
			Assert.IsTrue(await _locker.Exists(lockCategory, id));

			// Now get the lock
			LockObject lockObject = await _locker.GetLock(lockCategory, id);
			Assert.AreEqual(id, lockObject.ID);
			Assert.AreEqual(lockType,lockObject.Type);
			Assert.AreEqual(lockComment, lockObject.Comment);
			Assert.AreEqual(lockCategory, lockObject.Category);
			Assert.AreEqual(_locker.LockPrefix, lockObject.Prefix);
		}


		// Validate that the TTL value on the Locker is used for the default lock duration
		[Test]
		public async Task LockTTLSetCorrectly () {
			string id = _idGenerator.Next(34000, 49999).ToString();
			string lockCategory = _uniqueKeys.GetKey("LTSC");

			// Create our own custom Locker for this experiment
			RedisLocker testLocker =new RedisLocker(_redisCacheClient,0,false);
			int ttl = 3300;

			testLocker.TTL = ttl;
			Assert.AreEqual(ttl,testLocker.TTL);

			// Create a lock with no time in method, to see if default is used.
			Assert.IsTrue(await testLocker.SetLock(lockCategory,id,"Comment"));
			Thread.Sleep( ttl + 250);
			Assert.IsFalse(await testLocker.Exists(lockCategory, id));
		}


		// Generates a random number of locks and sets them in Redis
		internal async Task<int> GenerateRandomLocks(string lockCategory, int min, int max)
		{
			Random lockNumber = new Random();
			int maxLocks = new Random().Next(min, max);
			int startingId = _idGenerator.Next(18000, 18899);
			int j = 0;
			for ( int i = 0; i < maxLocks; i++, j++) {
				string id = (startingId + j).ToString();
				Assert.IsTrue(await _locker.SetLock(lockCategory, id, "Comment"), "A1000:  Lock was not set");
			}

			return maxLocks;
		}


		#region "TestInternalMethods"


		// Tests the internal function that builds the lock prefix string
		[TestCase(true, Description = "Should not have a lock prefix")]
		[TestCase(false, Description = "Should have a lock prefix")]
		[Test]
		public void BuildLockPrefix (bool isDedicatedLockDB) {
			RedisLocker rl = new RedisLocker(_redisCacheClient,3, isDedicatedLockDB);

			string lockCategory = "ABC";
			string result = rl.BuildLockPrefix(lockCategory);
			string expected;

			if ( isDedicatedLockDB ) { expected = lockCategory + ":"; }
			else
				expected = rl.LockPrefix + lockCategory + ":";

			Assert.AreEqual(expected,result);
		}


		// Tests the internal function that builds the lock key string
		[TestCase(true, Description = "Should not have a lock prefix")]
		[TestCase(false, Description = "Should have a lock prefix")]
		[Test]
		public void BuildLockKey (bool isDedicatedLockDB) {
			RedisLocker rl = new RedisLocker(_redisCacheClient, 3, isDedicatedLockDB);

			string lockCategory = "ABC";
			string lockID = "987123654";

			string result = rl.BuildLockKey(lockCategory,lockID);
			string expected;

			if (isDedicatedLockDB) { expected = lockCategory + ":" + lockID; }
			else
				expected = rl.LockPrefix + lockCategory + ":" + lockID;

			Assert.AreEqual(expected, result);

		}


		// Validates that SetLockValue puts the LockType in the first character position and then the comment follows it
		[TestCase(LockType.Exclusive)]
		[TestCase(LockType.ReadOnly)]
		[TestCase(LockType.AppLevel1)]
		[TestCase(LockType.AppLevel2)]
		[TestCase(LockType.AppLevel1)]
		[Test]
		public void SetLockValueTest (LockType lockType) {
			string comment = "a value:12-10-96:Female:Germany";

			string lockValue = _locker.SetLockValue(lockType, comment);
			
			string expected = (LockTypeValues.ValuesAsStrings[(int)lockType] + comment);
			Assert.AreEqual(expected,lockValue);
		}
		#endregion
	}


}