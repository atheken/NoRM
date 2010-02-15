using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MongoSharp.Tests
{
    [TestFixture]
    public class MongoDatabaseTest
    {
        [Test]
        public void GetAllCollections_Returns_Collections()
        {
            MongoServer context = new MongoServer();
            var db = context.GetDatabase("test");
            Assert.IsNotEmpty(db.GetAllCollections().ToList());
        }

        [Test]
        public void Drop_Collection_Returns_True()
        {
            MongoServer context = new MongoServer();
            var db = context.GetDatabase("test");
            string collName = "testInsertCollection";
            db.GetCollection<object>(collName).Insert(new { Title = "TestInsert" });
            
            var results = db.DropCollection(collName);

            Assert.IsTrue((results.OK == 1.0));
        }
    }
}
