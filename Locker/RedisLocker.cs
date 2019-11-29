using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System.Runtime.CompilerServices;
using Locker;

namespace SlugEnt.Locker
{
	/// <summary>
	/// Provides a Locker backed by a Redis Database to be used for locking objects across a distributed system in a quick and easy manner.
	/// </summary>
	public class RedisLocker : ILocker
	{
		private RedisCacheClient _redisCacheClient;
		private readonly IRedisDatabase _redisDB;
		private bool _isDedicatedLockDatabase;
		private string _lockPrefix = "L^";
		private int _lockTTL = DEFAULT_LOCK_TTL;

		private const int DEFAULT_LOCK_TTL = 300000;   // 300 seconds.


		/// <summary>
		/// A general purpose ID locking system that utilizes Redis as its backend storage medium.  This is just an in memory store
		/// that can be used to determine if some object (based upon its type (name) and unique identifier) are being used elsewhere
		/// in the system and thus cannot be accessed.  
		/// </summary>
		/// <param name="redisCacheClient">Reference to a RedisCacheClient that can be used to communicate with the Redis infrastructure </param>
		/// <param name="redisDatabaseNumber">The number of the database that should be used to store locks in.  It is recommended, but not required
		/// that locks be stored in a database all to their own.  The reason, is if all locks need to be flushed it can be much faster to flush the
		/// entire Database than to work thru all the lock types.</param>
		/// <param name="isDedicatedLockDatabase">Set to True, if the database to be used for storing locks is dedicated to this use only or if it
		/// is shared with other uses (caching values, etc).  There is a slight performance boost if using dedicated. </param>
		public RedisLocker(RedisCacheClient redisCacheClient, byte redisDatabaseNumber = 0, bool isDedicatedLockDatabase = false)
		{
			_redisCacheClient = redisCacheClient;
			_redisDB = _redisCacheClient.GetDb(redisDatabaseNumber);
			_isDedicatedLockDatabase = isDedicatedLockDatabase;

			// Clear the Lock Prefix if this is a dedicated database - no need for it, since everything in the database is a lock entry.
			if ( isDedicatedLockDatabase ) _lockPrefix = "";
		}


		/// <summary>
		/// Determines if a lock exists for the given lockCategory and LockID
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <param name="lockID">The ID value of the lock object</param>
		/// <returns></returns>
		public async Task<bool> Exists(string lockCategory, string lockId) { return await _redisDB.ExistsAsync(BuildLockKey(lockCategory, lockId)); }


		/// <summary>
		/// Returns if a lock is set and if so, the type of lock that is set.  If your application only has one type of lock then calling Exists is much faster and recommended.
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <param name="lockID">The ID value of the lock object</param>
		/// <returns></returns>
		public async Task<LockObject> GetLock (string lockCategory, string lockId) {
			string value = await _redisDB.GetAsync<string>(BuildLockKey(lockCategory, lockId));

			LockObject lockObject = new LockObject(_lockPrefix,lockCategory,lockId,value);
			return lockObject;
		}


		/// <summary>
		/// Sets a lock for the given lockCategory with the Identifier provided.  If you know the specific type of lock you want to set, it is better to
		/// call the SetLockType methods.  They result in less GC and system overhead and are faster.
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <param name="lockID">The ID value of the lock object</param>
		/// <param name="lockDuration">The number of seconds to maintain the lock, before automatically being freed.</param>
		/// <param name="lockType">The Type of lock you want.</param>
		/// <returns></returns>
		public async Task<bool> SetLock(string lockCategory, string lockID,  LockType lockType = LockType.Exclusive, int lockDuration = 0) {
			int ttl = lockDuration == 0 ? _lockTTL : lockDuration;
			return await _redisDB.AddAsync<string>(BuildLockKey(lockCategory, lockID), LockTypeValues.ValuesAsStrings[(int)lockType], new TimeSpan(0, 0, 0,0,ttl));
			}


		/// <summary>
		/// Sets an Exclusive lock for the given lockCategory with the Identifier provided.  An Exclusive lock means only 1 entity can access the object while
		/// the lock is set.
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <param name="lockID">The ID value of the lock object</param>
		/// <param name="lockDuration">The number of milli-seconds to maintain the lock, before automatically being freed.</param>
		/// <returns></returns>
		public async Task<bool> SetLockExclusive (string lockCategory, string lockID, int lockDuration = 0)
		{
			int ttl = lockDuration == 0 ? _lockTTL : lockDuration;
			return await _redisDB.AddAsync<string>(BuildLockKey(lockCategory, lockID), LockTypeValues.EXCLUSIVE, new TimeSpan(0, 0, 0,0,ttl));
		}


		/// <summary>
		/// Sets a ReadOnly lock for the given lockCategory with the Identifier provided.  ReadOnly, means that the entity that initiated the lock is the only
		/// one who can update it, but all others are able to read.
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <param name="lockID">The ID value of the lock object</param>
		/// <param name="lockDuration">The number of milli-seconds to maintain the lock, before automatically being freed.</param>
		/// <returns></returns>
		public async Task<bool> SetLockReadOnly(string lockCategory, string lockID, int lockDuration = 0)
		{
			int ttl = lockDuration == 0 ? _lockTTL : lockDuration;
			return await _redisDB.AddAsync<string>(BuildLockKey(lockCategory, lockID), LockTypeValues.READONLY, new TimeSpan(0, 0, 0,0,ttl));
		}


		/// <summary>
		/// Sets an AppLevel1 lock for the given lockCategory with the Identifier provided.  The meaning of this is wholly up to the calling application
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <param name="lockID">The ID value of the lock object</param>
		/// <param name="lockDuration">The number of seconds to maintain the lock, before automatically being freed.</param>
		/// <returns></returns>
		public async Task<bool> SetLockAppLevel1(string lockCategory, string lockID, int lockDuration = 0)
		{
			int ttl = lockDuration == 0 ? _lockTTL : lockDuration;
			return await _redisDB.AddAsync<string>(BuildLockKey(lockCategory, lockID), LockTypeValues.APPLEVEL1, new TimeSpan(0, 0, 0,0,ttl));
		}


		/// <summary>
		/// Sets an AppLevel2 lock for the given lockCategory with the Identifier provided.  The meaning of this is wholly up to the calling application
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <param name="lockID">The ID value of the lock object</param>
		/// <param name="lockDuration">The number of seconds to maintain the lock, before automatically being freed.</param>
		/// <returns></returns>
		public async Task<bool> SetLockAppLevel2(string lockCategory, string lockID, int lockDuration = 0)
		{
			int ttl = lockDuration == 0 ? _lockTTL : lockDuration;
			return await _redisDB.AddAsync<string>(BuildLockKey(lockCategory, lockID), LockTypeValues.APPLEVEL2, new TimeSpan(0, 0, 0,0,ttl));
		}


		/// <summary>
		/// Sets an AppLevel3 lock for the given lockCategory with the Identifier provided.  The meaning of this is wholly up to the calling application
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <param name="lockID">The ID value of the lock object</param>
		/// <param name="lockDuration">The number of seconds to maintain the lock, before automatically being freed.</param>
		/// <returns></returns>
		public async Task<bool> SetLockAppLevel3(string lockCategory, string lockID, int lockDuration = 0)
		{
			int ttl = lockDuration == 0 ? _lockTTL : lockDuration;
			return await _redisDB.AddAsync<string>(BuildLockKey(lockCategory, lockID), LockTypeValues.APPLEVEL3, new TimeSpan(0, 0, 0,0,ttl));
		}


		/// <summary>
		/// Deletes the specified lock
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <param name="lockID">The ID value of the lock object</param>
		/// <returns></returns>
		public async Task<bool> DeleteLock(string lockCategory, string lockID) { return await _redisDB.RemoveAsync(BuildLockKey(lockCategory, lockID)); }




		/// <summary>
		/// Returns the number of Locks there are for the given lockCategory
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <returns></returns>
		public async Task<int> LockCount(string lockCategory)
		{
			IEnumerable<string> keys = await _redisDB.SearchKeysAsync(BuildLockPrefix(lockCategory) + "*");
			return keys.Count();
		}



		/// <summary>
		/// Removes all locks for the specified lockCategory
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <returns></returns>
		public async Task DeleteAllLocksForlockCategory(string lockCategory) {
				IEnumerable<string> keys = await _redisDB.SearchKeysAsync(BuildLockPrefix(lockCategory) + "*");
				await _redisDB.RemoveAllAsync(keys);
		
		}


		/// <summary>
		/// Flushes all locks in the database
		/// </summary>
		/// <returns></returns>
		public async Task<bool> FlushAllLocks()
		{
			//TODO If this is only a lock database, then we can just do a flush.
			if (_isDedicatedLockDatabase) await _redisDB.FlushDbAsync();
			else
			{
				// Need to do a key search - getting all the keys that start with our lock prefix
				IEnumerable<string> keys = await _redisDB.SearchKeysAsync(_lockPrefix + "*");
				await _redisDB.RemoveAllAsync(keys);
			}
			return true;
		}



		/// <summary>
		/// Updates the lock expiration time to the new value.  It does not add the new time to existing, but sets it to expire in the given seconds.
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <param name="lockID">The ID value of the lock object</param>
		/// <param name="lockDuration">Number of milli-seconds to set the lock duration for.</param>
		/// <returns></returns>
		public async Task<bool> UpdateLockExpirationTime(string lockCategory, string lockID, int lockDuration)
		{
			return await _redisDB.UpdateExpiryAsync(
				BuildLockKey(lockCategory, lockID), new TimeSpan(0, 0, 0,0,lockDuration));
		}


		/// <summary>
		/// Retrieves a LockObject that provides information about the lock, including what type of lock it is.
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <param name="lockID">The ID value of the lock object</param>
		/// <returns></returns>
		public async Task<LockObject> LockInfo (string lockCategory, string lockId) {
			LockObject lockobj = await GetLock(lockCategory, lockId);
			return lockobj;
		}


		/// <summary>
		/// The Lock TTL in Milliseconds.  Determines the default lock lifetime for a given lock.
		/// </summary>
		public int TTL {
			get { return _lockTTL; }
			set { _lockTTL = value; }
		}


		/// <summary>
		/// Builds the Lock Key
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <param name="id">The ID value of the lock object</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal string BuildLockKey(string lockCategory, string id) { return (BuildLockPrefix(lockCategory) + id); }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal string BuildLockPrefix(string lockCategory) { return _lockPrefix + lockCategory + ":"; }
	}
}
