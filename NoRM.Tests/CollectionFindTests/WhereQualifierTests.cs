using System;
using System.Linq;
using Norm.BSON;
using Xunit;
using Norm.Collections;

namespace Norm.Tests
{

    public class WhereQualifierTests : IDisposable
    {
        private readonly Mongo _server;
        private readonly MongoCollection<TestClass> _collection;
        public WhereQualifierTests()
        {
            _server = Mongo.Create("mongodb://localhost/NormTests?pooling=false");
            _collection = _server.GetCollection<TestClass>("TestClasses");
        }
        public void Dispose()
        {
            _server.Database.DropCollection("TestClasses");
            using (var admin = new MongoAdmin("mongodb://localhost/NormTests?pooling=false"))
            {
                admin.DropDatabase();
            }
            _server.Dispose();
        }

        [Fact]
        public void MultiQualifier()
        {
            _collection.Insert(new TestClass { AInteger = 78 },
                new TestClass { AInteger = 79 },
                new TestClass { AInteger = 80 },
                new TestClass { AInteger = 81 });

            var result = _collection.Find(new { AInteger = Q.LessThan(81).And(Q.GreaterThan(78)) }).ToArray();
            Assert.Equal(2,result.Length);
            Assert.Equal(79, result[0].AInteger);
            Assert.Equal(80, result[1].AInteger);
        }

        [Fact]
        public void WhereExpressionShouldWorkWithFlyweight()
        {
            _collection.Insert(new TestClass { ADouble = 1d });
            _collection.Insert(new TestClass { ADouble = 2d });
            _collection.Insert(new TestClass { ADouble = 3d });
            _collection.Insert(new TestClass { ADouble = 4d });

            var count = _collection.Find();
            Assert.Equal(4, count.Count());

            var query = new Expando();
            query["$where"] = " function(){return this.ADouble > 1;} ";
            var results = _collection.Find(query);
            Assert.Equal(3, results.Count());
        }
    }
}