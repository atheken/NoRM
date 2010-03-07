using System;
using System.Linq;
using NoRM.BSON;
using Xunit;

namespace NoRM.Tests
{
    public class QueryTests : IDisposable
    {
        private readonly Mongo _server;
        private readonly MongoCollection<Person> _collection;
        public QueryTests()
        {
            _server = Mongo.ParseConnection("mongodb://localhost/NoRMTests?pooling=false");
            _collection = _server.GetCollection<Person>("People");
        }
        public void Dispose()
        {
            _server.Database.DropCollection("People");
            using (var admin = new MongoAdmin("mongodb://localhost/NoRMTests?pooling=false"))
            {
                admin.DropDatabase();
            }
            _server.Dispose();
        }

        [Fact]
        public void BasicQueryUsingProperty()
        {
            _collection.Insert(new Person { Name = "Joe Cool", Address = { Street="123 Main St", City="Anytown", State="CO", Zip="45123" } });
            _collection.Insert(new Person { Name = "Sam Cool", Address = { Street = "300 Main St", City = "Anytown", State = "CO", Zip = "45123" } });

            var query = new Flyweight();
            query["Name"] = Q.Equals<string>("Joe Cool");
            
            var results = _collection.Find(query);
            Assert.Equal(1, results.Count());
        }

        [Fact]
        public void BasicQueryUsingChildProperty()
        {
            _collection.Insert(new Person { Name = "Joe Cool", Address = { Street = "123 Main St", City = "Anytown", State = "CO", Zip = "45123" } });
            _collection.Insert(new Person { Name = "Sam Cool", Address = { Street = "300 Main St", City = "Anytown", State = "CO", Zip = "45123" } });

            var query = new Flyweight();
            query["Address.City"] = Q.Equals<string>("Anytown");

            var results = _collection.Find(query);
            Assert.Equal(2, results.Count());
        }
    }
}