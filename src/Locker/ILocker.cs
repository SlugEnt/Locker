﻿using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("SlugEnt.TestLocker")]

namespace SlugEnt.Locker
{
#pragma warning disable CS1591

    public interface ILocker
    {
        Task<bool> SetLock(string lockCategory, string lockId, string comment, LockType lockType, int lockDurationSeconds);
        Task<bool> SetLockExclusive(string lockCategory, string lockId, string comment, int lockDurationSeconds);
        Task<bool> SetLockReadOnly(string lockCategory, string lockId, string comment, int lockDurationSeconds);

        Task<bool> SetLockAppLevel1(string lockCategory, string lockId, string comment, int lockDurationSeconds);

        Task<bool> SetLockAppLevel2(string lockCategory, string lockId, string comment, int lockDurationSeconds);

        Task<bool> SetLockAppLevel3(string lockCategory, string lockId, string comment, int lockDurationSeconds);

        Task<bool> DeleteLock(string lockCategory, string lockId);
        Task<bool> Exists(string lockCategory, string lockId);
        Task<int> LockCount(string lockCategory);
        Task DeleteAllLocksForlockCategory(string lockCategory);
        Task<bool> FlushAllLocks();
        Task<bool> UpdateLockExpirationTime(string lockCategory, string lockID, int lockDurationInSeconds);

        Task<LockObject> LockInfo(string lockCategory, string lockId);
    }
}