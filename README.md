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

