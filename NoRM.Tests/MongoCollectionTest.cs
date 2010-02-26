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
        private MongoServer _server;
        private MongoDatabase _db;

        [TestFixtureSetUp]
        public void Setup()
        {
            this._server = new MongoServer();
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
            var server = new MongoServer();
            var testDB = server.GetDatabase("test");
            testDB.DropCollection("testObjects");
            var testColl = testDB.GetCollection<MiniObject>("testObjects");
            var cache = new List<MiniObject>();
            for (var i = 0; i < 10; i++)
            {
                cache.Add(new MiniObject { _id = OID.NewOID() });
            }

            testColl.Insert(cache);

            Assert.AreEqual(cache.Count, testColl.Distinct<OID>("_id").Count());
        }

        [Test]
        public void Collection_Statistics_Returns()
        {
            var coll = this._db.GetCollection<MiniObject>("stats_test");

            //collections & dbs are lazily created - force it to happen.
            coll.Insert(new MiniObject());

            var stats = this._db.GetCollectionStatistics("stats_test");
            Assert.AreEqual(coll.FullyQualifiedName, stats.Ns);

        }

        [Test]
        public void No_Filter_Count_Returns_Correct_Count()
        {
         
            var coll = this._db.GetCollection<MiniObject>("testing");
            coll.Delete(new { });
            for (int i = 0; i < 10; i++)
            {
                coll.Insert(new MiniObject() { _id=OID.NewOID()});
            }

            Assert.AreEqual(10, coll.Count());

        }

        [Test]
        public void Filtered_Count_Returns_Correct_Count()
        {
            var o = OID.NewOID();

            var coll = this._db.GetCollection<MiniObject>("testing");
            coll.Delete(new { });
            for (int i = 0; i < 10; i++)
            {
                if (i == 0)
                {
                    coll.Insert(new MiniObject() { _id =o });
                }
                else
                {
                    coll.Insert(new MiniObject() { _id = OID.NewOID() });
                }
            }

            Assert.AreEqual(1, coll.Count(new { _id = o }));
        }

    }
}
