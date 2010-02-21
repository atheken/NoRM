using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NoRM.BSON;
using NoRM.BSON.DbTypes;

namespace NoRM.Tests
{
    [TestFixture]
    [Category("Hits MongoDB")]
    public class MongoCollectionTest
    {
        private MongoContext _server;
        private MongoDatabase _db;

        [TestFixtureSetUp]
        public void Setup()
        {
            this._server = new MongoContext();
            this._db = this._server.GetDatabase("test" + Guid.NewGuid().ToString().Substring(0, 5));
            
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            this._server.DropDatabase(this._db.DatabaseName);
        }

        [Test]
        public void Distinct_For_Key_Returns_Correct_Set()
        {
            var server = new MongoContext();
            var testDB = server.GetDatabase("test");
            testDB.DropCollection("testObjects");
            var testColl = testDB.GetCollection<Object>("testObjects");
            var cache = new List<Object>();
            for (var i = 0; i < 10; i++)
            {
                cache.Add(new Object());
            }

            testColl.Insert(cache);

            Assert.AreEqual(cache.Count, testColl.Distinct<OID>("_id").Count());
        }

        [Test]
        public void Collection_Statistics_Returns()
        {
            var coll = this._db.GetCollection<object>("stats_test");

            //collections & dbs are lazily created - force it to happen.
            coll.Insert(new object());

            var stats = this._db.GetCollectionStatistics("stats_test");
            Assert.AreEqual(coll.FullyQualifiedName, stats.Ns);

        }
    }
}
