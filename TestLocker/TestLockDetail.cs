using System;
using System.Collections.Generic;
using System.Text;
using Locker;
using NUnit.Framework;


namespace TestLocker
{
	[Parallelizable(ParallelScope.Fixtures)]
	class TestLockDetail
	{
		[Test]
		public void CreationSuccessful () {
			string prefix = "^";
			string lockCategory = "test";
			string lockId = "trgortg";
			LockType lockType = LockType.Exclusive;

			LockObject lockObject = new LockObject(prefix, lockCategory,lockId,lockType);
			Assert.AreEqual(prefix, lockObject.Prefix);
			Assert.AreEqual(lockCategory,lockObject.Category);
			Assert.AreEqual(lockId, lockObject.ID);
			Assert.AreEqual(lockType, lockObject.Type);
		}


		[TestCase("R", LockType.ReadOnly)]
		[TestCase("X", LockType.Exclusive)]
		[TestCase("A", LockType.AppLevel1)]
		[TestCase("B", LockType.AppLevel2)]
		[TestCase("C", LockType.AppLevel3)]
		[TestCase("0", LockType.NoLock)]
		[TestCase("", LockType.NoLock)]

		[Test]
		public void CreationSuccessful_SettingAsString (string settingAsString, LockType expType) {
			string prefix = "^";
			string lockType = "test";
			string lockId = "trgortg";
			

			LockObject lockObject = new LockObject(prefix, lockType, lockId, settingAsString);
			Assert.AreEqual(prefix, lockObject.Prefix);
			Assert.AreEqual(lockType, lockObject.Category);
			Assert.AreEqual(lockId, lockObject.ID);
			Assert.AreEqual(expType, lockObject.Type);

		}
	}
}
