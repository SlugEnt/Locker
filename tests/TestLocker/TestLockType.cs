using Locker;
using NUnit.Framework;

namespace SlugEnt.TestRedisModels
{
	[Parallelizable(ParallelScope.Fixtures)]
	class TestLockType
	{
		[Test]
		public void EnumAndStringArrayValuesMatch () {
			Assert.AreEqual(LockTypeValues.ValuesAsStrings[0], LockTypeValues.ValuesAsStrings[(int)LockType.ReadOnly]);
			Assert.AreEqual(LockTypeValues.ValuesAsStrings[1], LockTypeValues.ValuesAsStrings[(int)LockType.Exclusive]);
			Assert.AreEqual(LockTypeValues.ValuesAsStrings[2], LockTypeValues.ValuesAsStrings[(int)LockType.AppLevel1]);
			Assert.AreEqual(LockTypeValues.ValuesAsStrings[3], LockTypeValues.ValuesAsStrings[(int)LockType.AppLevel2]);
			Assert.AreEqual(LockTypeValues.ValuesAsStrings[4], LockTypeValues.ValuesAsStrings[(int)LockType.AppLevel3]);
			Assert.AreEqual(LockTypeValues.ValuesAsStrings[5], LockTypeValues.ValuesAsStrings[(int)LockType.NoLock]);
		}


		[Test]
		public void ValuesAreExpected () {

		}
	}
}
