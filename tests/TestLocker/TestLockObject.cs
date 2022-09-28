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

using NUnit.Framework;
using SlugEnt.Locker;

namespace SlugEnt.TestLocker
{
    [Parallelizable(ParallelScope.Fixtures)]
    public class TestLockObject
    {
        [Test]
        public void CreationSuccessful()
        {
            string prefix = "^";
            string lockCategory = "test";
            string lockId = "trgortg";
            string lockComment = "Some Text";

            LockType lockType = LockType.Exclusive;

            LockObject lockObject = new LockObject(prefix, lockCategory, lockId, lockType, lockComment);
            Assert.AreEqual(prefix, lockObject.Prefix);
            Assert.AreEqual(lockCategory, lockObject.Category);
            Assert.AreEqual(lockId, lockObject.ID);
            Assert.AreEqual(lockType, lockObject.Type);
            Assert.AreEqual(lockComment, lockObject.Comment);
        }


        [TestCase("R", LockType.ReadOnly)]
        [TestCase("X", LockType.Exclusive)]
        [TestCase("A", LockType.AppLevel1)]
        [TestCase("B", LockType.AppLevel2)]
        [TestCase("C", LockType.AppLevel3)]
        [TestCase("0", LockType.NoLock)]
        [TestCase("", LockType.NoLock)]

        [Test]
        public void CreationSuccessful_SettingAsString(string settingAsString, LockType expType)
        {
            string prefix = "^";
            string lockType = "test";
            string lockId = "trgortg";
            string lockComment = "Some Internal Text";

            LockObject lockObject = new LockObject(prefix, lockType, lockId, settingAsString, lockComment);
            Assert.AreEqual(prefix, lockObject.Prefix);
            Assert.AreEqual(lockType, lockObject.Category);
            Assert.AreEqual(lockId, lockObject.ID);
            Assert.AreEqual(expType, lockObject.Type);
            Assert.AreEqual(lockComment, lockObject.Comment);
        }
    }
}
