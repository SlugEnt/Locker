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

namespace SlugEnt.Locker
{
    /// <summary>
    ///     The allowed Locker lock settings.  These set the type of lock, so that the calling applications know how to handle a locked object.
    /// </summary>
    public enum LockType
    {
        /// <summary>
        ///     A lock has been set in which reads are allowed, but no updates or deletes are allowed.  Callers must understand that the
        ///     object that is locked may be changed while they are viewing the data, thus resulting in them seeing outdated information.
        /// </summary>
        ReadOnly,

        /// <summary>
        ///     A lock has been set in which the object that is locked cannot be accessed for any purposes until the entity that set
        ///     the lock has released it or the locks TTL has expired.  The entity with the lock may update the object however.
        /// </summary>
        Exclusive,

        /// <summary>
        ///     A lock that is defined by the application that uses the lock.  The application determines what functionality is or is not allowed
        ///     based upon the value of the lock
        /// </summary>
        AppLevel1,

        /// <summary>
        ///     A lock that is defined by the application that uses the lock.  The application determines what functionality is or is not allowed
        ///     based upon the value of the lock
        /// </summary>
        AppLevel2,

        /// <summary>
        ///     A lock that is defined by the application that uses the lock.  The application determines what functionality is or is not allowed
        ///     based upon the value of the lock
        /// </summary>
        AppLevel3,

        /// <summary>
        ///     Indicates no lock exists or is set
        /// </summary>
        NoLock
    }


    /// <summary>
    ///     Class that represents the String values for the Lock Type Enum's.  
    /// </summary>
    public static class LockTypeValues
    {
        /// <summary>
        ///     A ReadOnly Lock.  Everyone but the Locker can read the object, only the Locker can update it.
        /// </summary>
        public const string READONLY = "R";

        /// <summary>
        ///     No one should be able to access the object at all, except the Locker.
        /// </summary>
        public const string EXCLUSIVE = "X";

        /// <summary>
        ///     Defined within the application
        /// </summary>
        public const string APPLEVEL1 = "A";

        /// <summary>
        ///     Defined within the application
        /// </summary>
        public const string APPLEVEL2 = "B";

        /// <summary>
        ///     Defined within the application
        /// </summary>
        public const string APPLEVEL3 = "C";

        /// <summary>
        ///     Indicates there is no lock.  Generally this should never be used by the application, it is here to indicate an error situation.
        /// </summary>
        public const string NOLOCK = "0";

        /// <summary>
        ///     Returns the string value of the given Enumeration value.
        /// </summary>
        public static readonly string[] ValuesAsStrings = { READONLY, EXCLUSIVE, APPLEVEL1, APPLEVEL2, APPLEVEL3, NOLOCK };
    }
}