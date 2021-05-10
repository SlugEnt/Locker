using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("SlugEnt.TestLocker")]

namespace SlugEnt.Locker
{
    /// <summary>
    ///     Provides a Locker backed by a Redis Database to be used for locking objects across a distributed system in a quick and easy manner.
    /// </summary>
    public class RedisLocker : ILocker
    {
        private readonly IRedisDatabase _redisDB;
        private readonly bool _isDedicatedLockDatabase;
        private const int DEFAULT_LOCK_TTL = 300000; // 300 seconds.


        /// <summary>
        ///     A general purpose ID locking system that utilizes Redis as its backend storage medium.  This is just an in memory store
        ///     that can be used to determine if some object (based upon its type (name) and unique identifier) are being used elsewhere
        ///     in the system and thus cannot be accessed.  
        /// </summary>
        /// <param name="redisCacheClient">Reference to a RedisCacheClient that can be used to communicate with the Redis infrastructure </param>
        /// <param name="redisDatabaseNumber">The number of the database that should be used to store locks in.  It is recommended, but not required
        /// that locks be stored in a database all to their own.  The reason, is if all locks need to be flushed it can be much faster to flush the
        /// entire Database than to work thru all the lock types.</param>
        /// <param name="isDedicatedLockDatabase">Set to True, if the database to be used for storing locks is dedicated to this use only or if it
        /// is shared with other uses (caching values, etc).  There is a slight performance boost if using dedicated. </param>
        public RedisLocker(IRedisCacheClient redisCacheClient, byte redisDatabaseNumber = 0, bool isDedicatedLockDatabase = false)
        {
            _redisDB = redisCacheClient.GetDb(redisDatabaseNumber);
            _isDedicatedLockDatabase = isDedicatedLockDatabase;

            // Clear the Lock Prefix if this is a dedicated database - no need for it, since everything in the database is a lock entry.
            if (isDedicatedLockDatabase) LockPrefix = "";
        }


        /// <summary>
        ///     Determines if a lock exists for the given lockCategory and LockID
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockId">The ID value of the lock object</param>
        /// <returns></returns>
        public async Task<bool> Exists(string lockCategory, string lockId) { return await _redisDB.ExistsAsync(BuildLockKey(lockCategory, lockId)); }


        /// <summary>
        ///     Returns a LockObject if a lock is set and if so, the type of lock that is set.  If your application only has one type of lock then calling Exists is much faster and recommended.
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockId">The ID value of the lock object</param>
        /// <returns>Null if Lock is not found</returns>
        public async Task<LockObject> GetLock(string lockCategory, string lockId)
        {
            string value = await _redisDB.GetAsync<string>(BuildLockKey(lockCategory, lockId));
            if (value == null) { return null; }

            string lockTypeAsString = value[0].ToString();
            string comment = value.Length > 1 ? value[1..] : "";

            LockObject lockObject = new LockObject(LockPrefix, lockCategory, lockId, lockTypeAsString, comment);
            return lockObject;
        }


        /// <summary>
        ///     Builds the lock value field, which is the type of lock (first character) and then a comment (everything after 1st char)
        /// </summary>
        /// <param name="lockType">The type of Lock</param>
        /// <param name="comment">The comment that belongs with the lock</param>
        /// <returns></returns>
        internal string SetLockValue(LockType lockType, string comment) { return (LockTypeValues.ValuesAsStrings[(int)lockType] + comment); }


        /// <summary>
        ///     Builds the lock value field, which is the type of lock (first character) and then a comment (everything after 1st char)
        /// </summary>
        /// <param name="lockTypeAsString">The type of Lock</param>
        /// <param name="comment">The comment that belongs with the lock</param>
        /// <returns></returns>
        internal string SetLockValue(string lockTypeAsString, string comment) { return (lockTypeAsString + comment); }


        /// <summary>
        ///     Sets a lock for the given lockCategory with the Identifier provided.  If you know the specific type of lock you want to set, it is better to
        ///     call the SetLockType methods.  They result in less GC and system overhead and are faster.
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <param name="comment">Additional info to be stored along with the lock ID.  For example, the user who has the lock.</param>
        /// <param name="lockDuration">The number of milli-seconds to maintain the lock, before automatically being freed.</param>
        /// <param name="lockType">The Type of lock you want.</param>
        /// <returns></returns>
        public async Task<bool> SetLock(string lockCategory, string lockID, string comment, LockType lockType = LockType.Exclusive, int lockDuration = 0)
        {
            int ttl = lockDuration == 0 ? TTL : lockDuration;
            return await _redisDB.AddAsync(BuildLockKey(lockCategory, lockID), SetLockValue(lockType, comment), new TimeSpan(0, 0, 0, 0, ttl));
        }

        /// <summary>
        ///     Sets a lock for the given lockCategory with the Identifier provided.  If you know the specific type of lock you want to set, it is better to
        ///     call the SetLockType methods.  They result in less GC and system overhead and are faster.
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <param name="comment">Additional info to be stored along with the lock ID.  For example, the user who has the lock.</param>
        /// <param name="lockDuration">The number of milli-seconds to maintain the lock, before automatically being freed.</param>
        /// <param name="lockType">The Type of lock you want.</param>
        /// <returns></returns>		
        public async Task<bool> SetLock(string lockCategory, string lockID, string comment, TimeSpan lockDuration, LockType lockType = LockType.Exclusive)
        {
            return await _redisDB.AddAsync(BuildLockKey(lockCategory, lockID), SetLockValue(lockType, comment), lockDuration);
        }


        /// <summary>
        ///     Sets an Exclusive lock for the given lockCategory with the Identifier provided.  An Exclusive lock means only 1 entity can access the object while
        ///     the lock is set.
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <param name="comment">Additional info to be stored along with the lock ID.  For example, the user who has the lock.</param>
        /// <param name="lockDuration">The number of milli-seconds to maintain the lock, before automatically being freed.</param>
        /// <returns></returns>
        public async Task<bool> SetLockExclusive(string lockCategory, string lockID, string comment, int lockDuration = 0)
        {
            int ttl = lockDuration == 0 ? TTL : lockDuration;
            return await _redisDB.AddAsync(BuildLockKey(lockCategory, lockID), SetLockValue(LockTypeValues.EXCLUSIVE, comment), new TimeSpan(0, 0, 0, 0, ttl));
        }


        /// <summary>
        ///     Sets an Exclusive lock for the given lockCategory with the Identifier provided.  An Exclusive lock means only 1 entity can access the object while
        ///     the lock is set.
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <param name="comment">Additional info to be stored along with the lock ID.  For example, the user who has the lock.</param>
        /// <param name="lockDuration">The number of milli-seconds to maintain the lock, before automatically being freed.</param>
        /// <returns></returns>
        public async Task<bool> SetLockExclusive(string lockCategory, string lockID, string comment, TimeSpan lockDuration)
        {
            return await _redisDB.AddAsync(BuildLockKey(lockCategory, lockID), SetLockValue(LockTypeValues.EXCLUSIVE, comment), lockDuration);
        }


        /// <summary>
        ///     Sets a ReadOnly lock for the given lockCategory with the Identifier provided.  ReadOnly, means that the entity that initiated the lock is the only
        ///     one who can update it, but all others are able to read.
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <param name="comment">Additional info to be stored along with the lock ID.  For example, the user who has the lock.</param>
        /// <param name="lockDuration">The number of milli-seconds to maintain the lock, before automatically being freed.</param>
        /// <returns></returns>
        public async Task<bool> SetLockReadOnly(string lockCategory, string lockID, string comment, int lockDuration = 0)
        {
            int ttl = lockDuration == 0 ? TTL : lockDuration;
            return await _redisDB.AddAsync(BuildLockKey(lockCategory, lockID), SetLockValue(LockTypeValues.READONLY, comment), new TimeSpan(0, 0, 0, 0, ttl));
        }


        /// <summary>
        ///     Sets a ReadOnly lock for the given lockCategory with the Identifier provided.  ReadOnly, means that the entity that initiated the lock is the only
        ///     one who can update it, but all others are able to read.
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <param name="comment">Additional info to be stored along with the lock ID.  For example, the user who has the lock.</param>
        /// <param name="lockDuration">The number of milli-seconds to maintain the lock, before automatically being freed.</param>
        /// <returns></returns>
        public async Task<bool> SetLockReadOnly(string lockCategory, string lockID, string comment, TimeSpan lockDuration)
        {
            return await _redisDB.AddAsync(BuildLockKey(lockCategory, lockID), SetLockValue(LockTypeValues.READONLY, comment), lockDuration);
        }


        /// <summary>
        ///     Sets an AppLevel1 lock for the given lockCategory with the Identifier provided.  The meaning of this is wholly up to the calling application
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <param name="comment">Additional info to be stored along with the lock ID.  For example, the user who has the lock.</param>
        /// <param name="lockDuration">The number of seconds to maintain the lock, before automatically being freed.</param>
        /// <returns></returns>
        public async Task<bool> SetLockAppLevel1(string lockCategory, string lockID, string comment, int lockDuration = 0)
        {
            int ttl = lockDuration == 0 ? TTL : lockDuration;
            return await _redisDB.AddAsync(BuildLockKey(lockCategory, lockID), SetLockValue(LockTypeValues.APPLEVEL1, comment), new TimeSpan(0, 0, 0, 0, ttl));
        }

        /// <summary>
        ///     Sets an AppLevel1 lock for the given lockCategory with the Identifier provided.  The meaning of this is wholly up to the calling application
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <param name="comment">Additional info to be stored along with the lock ID.  For example, the user who has the lock.</param>
        /// <param name="lockDuration">The number of seconds to maintain the lock, before automatically being freed.</param>
        /// <returns></returns>
        public async Task<bool> SetLockAppLevel1(string lockCategory, string lockID, string comment, TimeSpan lockDuration)
        {
            return await _redisDB.AddAsync(BuildLockKey(lockCategory, lockID), SetLockValue(LockTypeValues.APPLEVEL1, comment), lockDuration);
        }


        /// <summary>
        ///     Sets an AppLevel2 lock for the given lockCategory with the Identifier provided.  The meaning of this is wholly up to the calling application
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <param name="comment">Additional info to be stored along with the lock ID.  For example, the user who has the lock.</param>
        /// <param name="lockDuration">The number of seconds to maintain the lock, before automatically being freed.</param>
        /// <returns></returns>
        public async Task<bool> SetLockAppLevel2(string lockCategory, string lockID, string comment, int lockDuration = 0)
        {
            int ttl = lockDuration == 0 ? TTL : lockDuration;
            return await _redisDB.AddAsync(BuildLockKey(lockCategory, lockID), SetLockValue(LockTypeValues.APPLEVEL2, comment), new TimeSpan(0, 0, 0, 0, ttl));
        }


        /// <summary>
        ///     Sets an AppLevel2 lock for the given lockCategory with the Identifier provided.  The meaning of this is wholly up to the calling application
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <param name="comment">Additional info to be stored along with the lock ID.  For example, the user who has the lock.</param>
        /// <param name="lockDuration">The number of seconds to maintain the lock, before automatically being freed.</param>
        /// <returns></returns>
        public async Task<bool> SetLockAppLevel2(string lockCategory, string lockID, string comment, TimeSpan lockDuration)
        {
            return await _redisDB.AddAsync(BuildLockKey(lockCategory, lockID), SetLockValue(LockTypeValues.APPLEVEL2, comment), lockDuration);
        }


        /// <summary>
        ///     Sets an AppLevel3 lock for the given lockCategory with the Identifier provided.  The meaning of this is wholly up to the calling application
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <param name="comment">Additional info to be stored along with the lock ID.  For example, the user who has the lock.</param>
        /// <param name="lockDuration">The number of seconds to maintain the lock, before automatically being freed.</param>
        /// <returns></returns>
        public async Task<bool> SetLockAppLevel3(string lockCategory, string lockID, string comment, int lockDuration = 0)
        {
            int ttl = lockDuration == 0 ? TTL : lockDuration;
            return await _redisDB.AddAsync(BuildLockKey(lockCategory, lockID), SetLockValue(LockTypeValues.APPLEVEL3, comment), new TimeSpan(0, 0, 0, 0, ttl));
        }

        /// <summary>
        ///     Sets an AppLevel3 lock for the given lockCategory with the Identifier provided.  The meaning of this is wholly up to the calling application
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <param name="comment">Additional info to be stored along with the lock ID.  For example, the user who has the lock.</param>
        /// <param name="lockDuration">The number of seconds to maintain the lock, before automatically being freed.</param>
        /// <returns></returns>

        public async Task<bool> SetLockAppLevel3(string lockCategory, string lockID, string comment, TimeSpan lockDuration)
        {
            return await _redisDB.AddAsync(BuildLockKey(lockCategory, lockID), SetLockValue(LockTypeValues.APPLEVEL3, comment), lockDuration);
        }


        /// <summary>
        ///     Deletes the specified lock.  Returns False if the lock did not exist
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <returns></returns>
        public async Task<bool> DeleteLock(string lockCategory, string lockID) { return await _redisDB.RemoveAsync(BuildLockKey(lockCategory, lockID)); }




        /// <summary>
        ///     Returns the number of Locks there are for the given lockCategory
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <returns></returns>
        public async Task<int> LockCount(string lockCategory)
        {
            IEnumerable<string> keys = await _redisDB.SearchKeysAsync(BuildLockPrefix(lockCategory) + "*");
            return keys.Count();
        }



        /// <summary>
        ///     Removes all locks for the specified lockCategory
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <returns></returns>
        public async Task DeleteAllLocksForlockCategory(string lockCategory)
        {
            IEnumerable<string> keys = await _redisDB.SearchKeysAsync(BuildLockPrefix(lockCategory) + "*");
            await _redisDB.RemoveAllAsync(keys);

        }


        /// <summary>
        ///     Flushes all locks in the database
        /// </summary>
        /// <returns></returns>
        public async Task<bool> FlushAllLocks()
        {
            // If this database is dedicated to locks only, then we can just do a flush which is much faster.
            if (_isDedicatedLockDatabase) await _redisDB.FlushDbAsync();
            else
            {
                // Need to do a key search - getting all the keys that start with our lock prefix
                IEnumerable<string> keys = await _redisDB.SearchKeysAsync(LockPrefix + "*");
                await _redisDB.RemoveAllAsync(keys);
            }
            return true;
        }



        /// <summary>
        ///     Updates the lock expiration time to the new value.  It does not add the new time to existing, but sets it to expire in the given seconds.
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <param name="lockDuration">Number of milli-seconds to set the lock duration for.</param>
        /// <returns></returns>
        public async Task<bool> UpdateLockExpirationTime(string lockCategory, string lockID, int lockDuration)
        {
            return await _redisDB.UpdateExpiryAsync(
                BuildLockKey(lockCategory, lockID), new TimeSpan(0, 0, 0, 0, lockDuration));
        }


        /// <summary>
        ///     Retrieves a LockObject that provides information about the lock, including what type of lock it is.
        /// </summary>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockId">The ID value of the lock object</param>
        /// <returns>Null if the lock does not exist</returns>
        public async Task<LockObject> LockInfo(string lockCategory, string lockId)
        {
            LockObject lockobj = await GetLock(lockCategory, lockId);
            return lockobj;
        }


        /// <summary>
        ///     The Lock TTL in Milliseconds.  Determines the default lock lifetime when setting a Lock if it is not overridden in the call method.
        /// </summary>
        public int TTL { get; set; } = DEFAULT_LOCK_TTL;


        /// <summary>
        ///     Returns what the Lock Prefix is
        /// </summary>
        public string LockPrefix { get; } = "L^";

        /// <summary>
        ///     Builds the Lock Key
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
        internal string BuildLockPrefix(string lockCategory) { return LockPrefix + lockCategory + ":"; }
    }
}