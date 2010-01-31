using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace System.Data.Mongo.Tests
{
    [TestFixture]
    public class MongoDatabaseTest
    {
        [Test]
        public void GetAllCollections_Returns_Collections()
        {
            MongoContext context = new MongoContext();
            var db = context.GetDatabase("Test1");
            Assert.IsNotEmpty(db.GetAllCollections().ToList());
        }
    }
}
