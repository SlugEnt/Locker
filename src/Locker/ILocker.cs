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

using System.Runtime.CompilerServices;
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