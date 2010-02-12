using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace System.Data.Mongo.Tests
{
    public class TestClass
    {
        public TestClass() { }
        public int? a { get; set; }
    }

    [TestFixture]
    public class MongoFindTest
    {

        [Test]
        public void MongoDatabase_FindOne_Returns()
        {
            // This test assumes you did the little test from the MongoDB getting started docs
            MongoContext context = new MongoContext();

            var db = context.GetDatabase("test");

            MongoCollection<TestClass> coll = db.GetCollection<TestClass>("foo");

            TestClass found = coll.Find(new { a = 1 } ).First();

            Assert.IsNotNull(found);
        }
    }
}
