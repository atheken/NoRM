using System;
using System.Linq;
using NoRM.BSON;
using Xunit;

namespace NoRM.Tests
{
    public class UpdateTests : IDisposable
    {
        private readonly Mongo _server;
        private readonly MongoCollection<CheeseClubContact> _collection;
        public UpdateTests()
        {
            _server = Mongo.ParseConnection("mongodb://localhost/NoRMTests?pooling=false");
            _collection = _server.GetCollection<CheeseClubContact>("CheeseClubContacts");
        }
        public void Dispose()
        {
            _server.Database.DropCollection("CheeseClubContacts");
            using (var admin = new MongoAdmin("mongodb://localhost/NoRMTests?pooling=false"))
            {
                admin.DropDatabase();
            }
            _server.Dispose();
        }

        [Fact]
        public void BasicUsageOfUpdateOne()
        {
            var aPerson = new CheeseClubContact { Name = "Joe", FavoriteCheese = "Cheddar" };
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
            var aPerson = new CheeseClubContact { Name = "Joe", FavoriteCheese = "American" };
            _collection.Insert(aPerson);

            var results = _collection.Find();
            Assert.Equal(1, results.Count());

            var matchDocument = new { _id = aPerson.Id };
            aPerson.FavoriteCheese = "Velveeta";

            _collection.UpdateOne(matchDocument, aPerson);

            var query = new Flyweight();
            query["_id"] = Q.Equals<ObjectId>(aPerson.Id);

            var retreivedPerson = _collection.FindOne(query);

            Assert.Equal("Velveeta", retreivedPerson.FavoriteCheese);
        }

        [Fact]
        public void UpdateMustSpecifyEverything()
        {
            //Note, when doing an update, MongoDB replaces everything in your document except the id. So, if you don't re-specify a property, it'll disappear.
            //In this example, the cheese is gone.

            var aPerson = new CheeseClubContact { Name = "Joe", FavoriteCheese = "Cheddar" };

            Assert.NotNull(aPerson.FavoriteCheese);

            _collection.Insert(aPerson);

            var matchDocument = new { Name = "Joe" };
            var updatesToApply = new { Name = "Joseph" };
            
            _collection.UpdateOne(matchDocument, updatesToApply);

            var query = new Flyweight();
            query["Name"] = Q.Equals<string>("Joseph");

            var retreivedPerson = _collection.FindOne(query);

            Assert.Null(retreivedPerson.FavoriteCheese);
        }

        [Fact]
        public void UsingUpsertToInsertOnUpdate()
        {
            //Upsert will update an existing document if it can find a match, otherwise it will insert.
            //In this scenario, we're updating our collection without inserting first.

            var aPerson = new CheeseClubContact { Name = "Joe", FavoriteCheese = "American" };

            var matchDocument = new { Name = aPerson.Name };
            _collection.Update(matchDocument, aPerson, false, true);

            var results = _collection.Find();
            Assert.Equal(1, results.Count());
        }
    }
}