using System;
using System.Linq;
using Norm.BSON;
using NUnit.Framework;
using Norm.Collections;

namespace Norm.Tests
{

    [TestFixture]
    public class WhereQualifierTests
    {
        private IMongo _server;
        private IMongoCollection<TestClass> _collection;
        
        [SetUp]
        public void Setup()
        {
            _server = Mongo.Create("mongodb://localhost/NormTests?pooling=false");
            _collection = _server.GetCollection<TestClass>("TestClasses");
        }

        [TearDown]
        public void Teardown()
        {
            _server.Database.DropCollection("TestClasses");
            using (var admin = new MongoAdmin("mongodb://localhost/NormTests?pooling=false"))
            {
                admin.DropDatabase();
            }
            _server.Dispose();
        }

        [Test]
        public void MultiQualifierAnd()
        {
            _collection.Insert(new TestClass { AInteger = 78 },
                new TestClass { AInteger = 79 },
                new TestClass { AInteger = 80 },
                new TestClass { AInteger = 81 });

            var result = _collection.Find(new { AInteger = Q.LessThan(81).And(Q.GreaterThan(78)) }).ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(79, result[0].AInteger);
            Assert.AreEqual(80, result[1].AInteger);
        }

        [Test]
        public void MultiQualifieOr()
        {
            _collection.Insert(
                new TestClass { AInteger = 78 },
                new TestClass { AInteger = 79 },
                new TestClass { AInteger = 80 },
                new TestClass { AInteger = 81 });



            var result = _collection.Find(
                Q.Or(new { AInteger = Q.LessOrEqual(78) },
                new { AInteger = Q.GreaterOrEqual(81) })
                ).ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(78, result[0].AInteger);
            Assert.AreEqual(81, result[1].AInteger);
        }

        [Test]
        public void WhereExpressionShouldWorkWithFlyweight()
        {
            _collection.Insert(new TestClass { ADouble = 1d });
            _collection.Insert(new TestClass { ADouble = 2d });
            _collection.Insert(new TestClass { ADouble = 3d });
            _collection.Insert(new TestClass { ADouble = 4d });

            var count = _collection.Find();
            Assert.AreEqual(4, count.Count());

            var query = new Expando();
            query["$where"] = " function(){return this.ADouble > 1;} ";
            var results = _collection.Find(query);
            Assert.AreEqual(3, results.Count());
        }
    }
}