h3. NoRM is a .Net library for connecting to the document-oriented database, MongoDB.

NoRM will provide:
* Strongly-typed interaction when querying and updating collections.
* Improved interface to send common Mongo commands (creating indices, getting all the existing dbs and collections, etc.).
* Ultra-fast de/serialization of BSON to .Net CLR types and back.
* LINQ-to-Mongo

NoRM works in Mono & .Net

We're incubating, things may be broken, may not do what you think they should, or otherwise just not be what you expect. We're working as fast as we can to bring this up to a best-of-class library for interacting with MongoDB on the .Net platform. If you have ideas how we can do that, let us know - or fork them and share with us.

If you have any questions or ideas, please use the Google Group here:
http://groups.google.com/group/norm-mongodb

* The NoRM.dll can connect to MongoDB using a trusted connection, this is the workhorse of NoRM, and all you really need to get started.
* The NoRM.Tests.dll are nunit tests for NoRM, We are much more likely to accept patches that have corresponding tests.
* BSONHarness is just a nice entry point to see how the NoRM library is functioning, this will probably go away soon, since you can do most of this via tests.