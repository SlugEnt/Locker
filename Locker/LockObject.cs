using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TestLocker")]
 
namespace Locker
{
	/// <summary>
	/// Represents a Locker Lock on a given object.  This is typically used when retrieving lock information from a Locker.
	/// </summary>
	public class LockObject {

		/// <summary>
		/// Builds a LockObject object.
		/// </summary>
		/// <param name="prefix">The Locker Prefix value.  This only has meaning when recieving this object from a Locker call.
		/// It's value is not used when creating a lock</param>
		/// <param name="lockCategory">The category this lock belongs to</param>
		/// <param name="lockID">The specific identifier that associates this lock to an object</param>
		/// <param name="type">The type of lock you want.</param>
		public LockObject (string prefix, string lockCategory, string lockID, LockType type) {
			Prefix = prefix; 
			ID = lockID;
			Category = lockCategory;
			Type = type;
		}



		/// <summary>
		/// Builds a LockObject object..  Used internally when building the object as the result of a call to the Redis DB.
		/// </summary>
		/// <param name="prefix">The Lock Engine prefix value</param>
		/// <param name="lockCategory">The lockCategory of the lock</param>
		/// <param name="lockID">The ID value of the lock object</param>
		/// <param name="typeAsString">The Redis String value of the Lock Type</param>
		internal LockObject (string prefix, string lockCategory, string lockID, string typeAsString)
		{
			Prefix = prefix;
			ID = lockID;
			Category = lockCategory;

			Type = typeAsString switch
			{
				"" => LockType.NoLock,
				LockTypeValues.EXCLUSIVE => LockType.Exclusive,
				LockTypeValues.READONLY => LockType.ReadOnly,
				LockTypeValues.APPLEVEL1 => LockType.AppLevel1,
				LockTypeValues.APPLEVEL2 => LockType.AppLevel2,
				LockTypeValues.APPLEVEL3 => LockType.AppLevel3,
				LockTypeValues.NOLOCK => LockType.NoLock,
				_ => LockType.NoLock
			};
		}

		/// <summary>
		/// The prefix or Locker Identifier of the lock 
		/// </summary>
		public string Prefix { get; internal set; }

		/// <summary>
		/// The category this lock belongs to
		/// </summary>
		public string Category { get; set; }

		/// <summary>
		/// The object identifier this lock is for.
		/// </summary>
		public string ID { get; set; }

		/// <summary>
		/// The Type of lock that this is.
		/// </summary>
		public LockType Type { get; set; }

	}
}
