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

    [TestFixture]
    public class MongoFindTest
    {
        [Test]
        public void MongoDatabase_FindOne_Returns()
        {
            // This test assumes you did the little test from the MongoDB getting started docs
            MongoServer context = new MongoServer();

            var db = context.GetDatabase("test");

            MongoCollection<TestClass> coll = db.GetCollection<TestClass>("foo");

            TestClass found = coll.FindOne(new { a = 1d } );

            Assert.IsNotNull(found);
        }


    }
}
