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

[assembly: InternalsVisibleTo("SlugEnt.TestLocker")]

namespace SlugEnt.Locker
{
    /// <summary>
    ///     Represents a Locker Lock on a given object.  This is typically used when retrieving lock information from a Locker.
    /// </summary>
    public class LockObject
    {
        /// <summary>
        ///     Builds a LockObject object.
        /// </summary>
        /// <param name="prefix">The Locker Prefix value.  This only has meaning when receiving this object from a Locker call.
        /// It's value is not used when creating a lock</param>
        /// <param name="lockCategory">The category this lock belongs to</param>
        /// <param name="lockID">The specific identifier that associates this lock to an object</param>
        /// <param name="type">The type of lock you want.</param>
        /// <param name="comment">Any clarifying information about the lock, such as userID, datetime, etc</param>
        public LockObject(string prefix, string lockCategory, string lockID, LockType type, string comment)
        {
            Prefix = prefix;
            ID = lockID;
            Category = lockCategory;
            Type = type;
            Comment = comment;
        }


        /// <summary>
        ///     Builds a LockObject object..  Used internally when building the object as the result of a call to the Redis DB.
        /// </summary>
        /// <param name="prefix">The Lock Engine prefix value</param>
        /// <param name="lockCategory">The lockCategory of the lock</param>
        /// <param name="lockID">The ID value of the lock object</param>
        /// <param name="lockTypeAsString">The Redis String value of the Lock Type</param>
        /// <param name="comment">Any clarifying information about the lock, such as userID, datetime, etc</param>
        internal LockObject(string prefix, string lockCategory, string lockID, string lockTypeAsString, string comment)
        {
            Prefix = prefix;
            ID = lockID;
            Category = lockCategory;
            Comment = comment;

            Type = lockTypeAsString switch
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
        ///     The prefix or Locker Identifier of the lock 
        /// </summary>
        public string Prefix { get; internal set; }

        /// <summary>
        ///     The category this lock belongs to
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        ///     The object identifier this lock is for.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        ///     The Type of lock that this is.
        /// </summary>
        public LockType Type { get; set; }

        /// <summary>
        ///     Any information the lock setter wanted to pass onto the lock, such as the userId who has the lock, or the datetime it was created, etc.
        /// </summary>
        public string Comment { get; set; }
    }
}