using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NoRM.BSON;

namespace NoRM.Tests
{
    [TestFixture]
    public class MongoCollectionTest
    {
        [Test]
        public void Distinct_For_Key_Returns_Correct_Set()
        {
            var server = new MongoServer();
            var testDB = server.GetDatabase("test");
            testDB.DropCollection("testObjects");
            var testColl = testDB.GetCollection<Object>("testObjects");
            var cache = new List<Object>();
            for (var i = 0; i < 10; i++)
            {
                cache.Add(new Object());
            }
            testColl.Insert(cache);

            Assert.AreEqual(cache.Count, testColl.Distinct<BSONOID>("_id").Count());
        }

        [Test]
        public void Collection_Statistics_Returns()
        {
            var server = new MongoServer();

            var db = server.GetDatabase("test");

            var stats = db.GetCollectionStatistics("foo");

            Assert.IsTrue((stats.Ns == "test.foo"));

        }
    }
}
