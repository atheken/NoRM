using System;
using System.Linq;
using NoRM.BSON;
using Xunit;

namespace NoRM.Tests
{
    public class Person
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string FavoriteCheese { get; set; }

        public Person()
        {
            Id = ObjectId.NewObjectId();
        }
    }
    
    public class UpdateTests : IDisposable
    {
        private readonly Mongo _server;
        private readonly MongoCollection<Person> _collection;
        public UpdateTests()
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
        public void BasicUsageOfUpdateOne()
        {
            var aPerson = new Person { Name = "Joe", FavoriteCheese = "Cheddar" };
            _collection.Insert(aPerson);

            var count = _collection.Find();
            Assert.Equal(1, count.Count());

            var matchDocument = new { Name = "Joe" };
            aPerson.FavoriteCheese = "Gouda";

            _collection.UpdateOne(matchDocument, aPerson);

            var query = new Flyweight();
            query["Name"] = Q.Equals<string>("Joe");

            var retreivedPerson = _collection.FindOne(query);

            Assert.Equal("Gouda", retreivedPerson.FavoriteCheese);
        }

        [Fact]
        public void BasicUsageOfUpdateOneUsingObjectId()
        {
            var aPerson = new Person { Name = "Joe", FavoriteCheese = "American" };
            _collection.Insert(aPerson);

            var count = _collection.Find();
            Assert.Equal(1, count.Count());

            var matchDocument = new { _id = aPerson.Id };
            aPerson.FavoriteCheese = "Velveeta";

            _collection.UpdateOne(matchDocument, aPerson);

            var query = new Flyweight();
            query["_id"] = Q.Equals<ObjectId>(aPerson.Id);

            var retreivedPerson = _collection.FindOne(query);

            Assert.Equal("Velveeta", retreivedPerson.FavoriteCheese);
        }
    }
}