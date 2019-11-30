# Locker

General purpose lock manager for distributed systems that need to coordinate access to objects.  The initial lock implementation is the RedisLocker which requires a Redis backend.

## Getting Started

Just clone and run.  At the present moment the IP Address of the Redis server is hard coded in the Unit Tests.  This will be changed at a future time

### Prerequisites
* [StackExchange.Redis.Extensions.Core](https://github.com/imperugo/StackExchange.Redis.Extensions)
* [Nunit 3.X](https://github.com/nunit/nunit)
* C# 8  (Utilizes new Switch statement)
* .NetStandard 2.1




### Installing



## Running the tests

At present there is 100% code coverage of all Locker classes.



## Deployment

Just add the library as a reference or download from Nuget

## Using
Locker is pretty easy to use.  There are basically 2 pre-requisites required before you can use the Locker and then it is quite easy.
* You need a RedisCacheClient object to have already been instantiated with a connection to a Redis Database. 
* You have to have a Redis Database running that is ready to receive commands.

So getting started:
````csharp
RedisLocker locker = new RedisLocker (redisCacheClient); 
locker.SetLock("appA", "94545045","a comment");
bool isThereALock = locker.Exists("appA", "94545045");
bool isDeleted = locker.DeleteLock("appA", "94545045");

````

Will create a new RedisLocker using the Redis client passed in.  Since the 2nd parameter is not passed, it assumes we will be using Database0 as the Locker database.  Since the 3rd parameter is not passed as well, then it assumes the database is shared with other applications (ie. it's not just the Lockers database).
It then sets a lock for a Lock Category called appA.  Within that appA the id of the object locked is 94545045. Finally, it sets a comment for the lock.  Locks by default will automatically expire. 

It then checks to see if the lock exists. Returning True if it does and False if it does not.
Finally it deletes the lock so that we free the resource up.  If we had not manually deleted it, it would have automatically been deleted 5 minutes after creation.

Depending on your application there are other methods that might yield slight performance increases over 100,000 uses - They are:
* Calling locker.SetLockTYPE where Type is Exclusive, ReadOnly, AppLevel1, AppLevel2, AppLevel3.  Doing this results in less GC and string manipulation than the stock SetLock command.
* If you are only using one Type of lock in your app then calling Exists vs GetLock will yield faster results and less GC, since it only needs to return True or False and not interogate the return value and convert into LockType.

### LockCategory
LockCategory is a parameter on many of the methods of the locker and for good reason.  It allows you to group locks by category.  In reality it is just a prefix of part of the Lock Key that gets sent to Redis.  So, lets say you were storing locks for 2 types of objects.  One for Addresses and one for people.  Assume the object ID (maybe a database ID) for both an address and a person is a long integer.  Without the LockCategory, you would need to somehow uniquely identify an address with id of 10 and a person with an id of 10.  With LockCategory your call for a persona and an address would be as follows:
````csharp
# Set a lock for address 10
locker.SetLockExclusive ("ADDRESS","10","comment");
# Set a lock for person 10
locker.SetLockReadOnly ("PERSON","10","comment");
````

The LockCategory can be anything you like, but recommendation is to keep as short as possible (2 or 3 characters) as it is part of the lookup key.
It is upto the callers to make sure they do not have collissions across their redis environment on LockCategories!

### Lock Timeouts 
The default lock timeout is 300 seconds.  But you can set it anytime you call SetLock as it is one of the parameters.
You can also extend a lock timeout by calling:
````csharp
locker.UpdateLockExpirationTime("PERSON", "10", 7));
````
Will extend the lock another 7 seconds.

### Lock Comments
All locks require a comment.  This can just be an empty string.  The comment is whatever additional information you wish to store with the lock, such as maybe the user that has the lock or a datetime when lock was created.  

### More Examples
````csharp
RedisLocker locker = new RedisLocker (redisCacheClient); 
# Set an Exclusive Lock - This is slightly faster than calling SetLock and should be preferred 
# over SetLock if you kow the type of lock you want to set.
locker.SetLockExclusive("appA", "945","Mary Poppins:12-32");

LockObject lockObject = await locker.GetLock ("appA", "945");
Console.WriteLine ("Lock Comment: {0}", lockObject.Comment);

# Create a lock with a TTL of 4 seconds.
locker.SetLockReadOnly ("ApBBB", "43AB","John Smith",4000);

# Create a lock with a TimeSpan as its TTLS - 2 hours
locker.SetLockAppLevel1 ("ApBBB", "32H","Pickard",new TimeSpan(0,2,0,0));

# Get the Count of Locks for a given Category:  count = 2
int count = await locker.LockCount("ApBBB");


````

## Built With

* [StackExchange.Redis.Extensions.Core](https://github.com/imperugo/StackExchange.Redis.Extensions) - Provides high level Redis functionality into a C# library.
* [Nunit 3.X](https://github.com/nunit/nunit) - Unit Testing


## Contributing

Please read 

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Scott Herrmann** - *Project Lead* 



## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

