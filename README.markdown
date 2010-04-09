NoRM is a .Net library for connecting to the document-oriented database, MongoDB.
=================================================================================

*We're _incubating_, things may be broken, may not do what you think they should, or otherwise just not be what you expect. We're working as fast as we can to bring this up to a best-of-class library for interacting with MongoDB on the .Net platform. If you have ideas how we can do that, let us know - or fork them and share with us. _If any of this scares you, you should hold off until the code is a bit more mature._*

_NoRM provides:_

* Strongly-typed interaction when querying and updating collections.
* Improved interface to send common Mongo commands (creating indices, getting all the existing dbs and collections, etc.).
* Ultra-fast de/serialization of BSON to .Net CLR types and back.
* An optional fluent configuration for mapping types and properties to different names and collections in the DB.
* LINQ-to-Mongo
* NoRM will work under both Mono & .Net


If you have any questions or ideas, please use the [Google Group](http://groups.google.com/group/norm-mongodb) to convey them (Hint: this will also be a good way to be notified when we get close to a release)

It would also be useful to read what we have on the [wiki](http://wiki.github.com/atheken/NoRM/) (we know it's sparse, workin' on that, too)


* The NoRM.dll can connect to MongoDB using a trusted connection, this is the workhorse of NoRM, and all you really need to get started.
* The NoRM.Tests.dll are nunit tests for NoRM, We are much more likely to accept patches that have corresponding tests.