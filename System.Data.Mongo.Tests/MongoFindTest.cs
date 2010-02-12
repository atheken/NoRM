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
        public string a { get; set; }
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

            TestClass found = coll.FindOne(new { a = "1" } );

            Assert.IsNotNull(found);
        }
    }
}
