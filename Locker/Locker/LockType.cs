using System;
using System.Collections.Generic;
using System.Text;

namespace Locker
{
	/// <summary>
	/// The allowed Locker lock settings.  These set the type of lock, so that the calling applications know how to handle a locked object.
	/// </summary>
	public enum LockType
	{
		/// <summary>
		/// A lock has been set in which reads are allowed, but no updates or deletes are allowed.  Callers must understand that the
		/// object that is locked may be changed while they are viewing the data, thus resulting in them seeing outdated information.
		/// </summary>
		ReadOnly, 

		/// <summary>
		/// A lock has been set in which the object that is locked cannot be accessed for any purposes until the entity that set
		/// the lock has released it or the locks TTL has expired.  The entity with the lock may update the object however.
		/// </summary>
		Exclusive, 

		/// <summary>
		/// A lock that is defined by the application that uses the lock.  The application determines what functionality is or is not allowed
		/// based upon the value of the lock
		/// </summary>
		AppLevel1,

		/// <summary>
		/// A lock that is defined by the application that uses the lock.  The application determines what functionality is or is not allowed
		/// based upon the value of the lock
		/// </summary>
		AppLevel2,

		/// <summary>
		/// A lock that is defined by the application that uses the lock.  The application determines what functionality is or is not allowed
		/// based upon the value of the lock
		/// </summary>
		AppLevel3,

		/// <summary>
		/// Indicates no lock exists or is set
		/// </summary>
		NoLock 

	}

	// This char array must match the above enum. 
	public static class LockTypeValues {
		public const string READONLY = "R";
		public const string EXCLUSIVE = "X";
		public const string APPLEVEL1 = "A";
		public const string APPLEVEL2 = "B";
		public const string APPLEVEL3 = "C";
		public const string NOLOCK = "0";

		public static readonly string[] ValuesAsStrings = { READONLY, EXCLUSIVE, APPLEVEL1, APPLEVEL2, APPLEVEL3, NOLOCK};
	}
}
