using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MongoSharp.Query.Tests
{
    public class TestClass
    {
        public TestClass() { }
        public double? a { get; set; }
    }

    // TODO rename this to MongoCollectionTest
    [TestFixture]
    public class MongoFindTest
    {
        [Test]
        public void MongoDatabase_FindOne_Returns()
        {
            MongoServer context = new MongoServer();
            var db = context.GetDatabase("test");
            MongoCollection<TestClass> coll = db.GetCollection<TestClass>("foo");
            coll.Insert(new TestClass { a = 1 });
            TestClass found = coll.FindOne(new { a = 1d } );

            Assert.IsNotNull(found);
        }

        [Test]
        public void Collection_Statistics_Returns()
        {
            MongoServer server = new MongoServer();

            var db = server.GetDatabase("test");

            MongoCollection<TestClass> coll = db.GetCollection<TestClass>("foo");

            var stats = coll.GetStatistics();

            Assert.IsTrue((stats.Ns == "test.foo"));

        }
    }
}
