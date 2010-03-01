using System;
using System.Linq;
using NoRM.BSON;
using Xunit;

namespace NoRM.Tests
{
   
    
    public class WhereQualifierTests : IDisposable
    {
        private readonly Mongo _server;
        private readonly MongoCollection<TestClass> _collection;
        public WhereQualifierTests()
        {
            _server = new Mongo("mongodb://localhost/NoRMTests?pooling=false");            
            _collection = _server.GetCollection<TestClass>("TestClasses");
        }
        public void Dispose()
        {
            _server.Database.DropCollection("TestClasses");
            using (var admin = new MongoAdmin("mongodb://localhost/NoRMTests?pooling=false"))
            {
                admin.DropDatabase();
            }
            _server.Dispose();
        }

        [Fact]
        public void WhereExpressionShouldWorkWithFLyweight()
        {
            _collection.Insert(new TestClass {ADouble = 1d});
            _collection.Insert(new TestClass {ADouble = 2d});
            _collection.Insert(new TestClass {ADouble = 3d});
            _collection.Insert(new TestClass {ADouble = 4d});

            var count = _collection.Find();
            Assert.Equal(4, count.Count());

            var query = new Flyweight();
            query["$where"] = " function(){return this.ADouble > 1;} ";
            var results = _collection.Find(query);
            Assert.Equal(3, results.Count());
        }
    }
}