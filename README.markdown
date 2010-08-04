NoRM is a .Net library for connecting to the document-oriented database, MongoDB.
=================================================================================

Please check the project website for more samples and a screencast: [normproject.org](http://normproject.org)

If you have any questions or ideas, please use the [Google Group](http://groups.google.com/group/norm-mongodb) to convey them (Hint: this will also be a good way to be notified when we get close to a release)

*The API may change a bit over time, and there remain a few more features before we will cut a "Release" but the library is pretty stable, and has good unit test coverage for the core components. If you have ideas or requests, let us know - or fork them and share with us.*

_NoRM provides:_

* Strongly-typed interaction when querying and updating collections.
* Improved interface to send common MongoDB commands (creating indices, getting all the existing dbs and collections, etc.).
* Ultra-fast de/serialization of BSON to .Net CLR types and back.
* Fluent mappings to specify the property names and characteristics at runtime, rather than attributes.
* An optional fluent configuration for mapping types and properties to different names and collections in the DB.
* LINQ-to-Mongo
* NoRM will work under both Mono & .Net

It would also be useful to read what we have on the [wiki](http://wiki.github.com/atheken/NoRM/).

* The NoRM.dll can connect to MongoDB, this is the workhorse of NoRM, and all you really need to get started.
* The NoRM.Tests.dll are xunit tests for NoRM, we are much more likely to accept patches that have corresponding tests.
