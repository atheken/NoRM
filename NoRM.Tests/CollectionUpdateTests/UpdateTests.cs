using System;
using System.Linq;
using Norm.BSON;
using NUnit.Framework;
using Norm.Collections;

namespace Norm.Tests
{

    [TestFixture]
    public class UpdateTests 
    {
		private Mongod _proc;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			_proc = new Mongod ();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			_proc.Dispose ();
		}

        private  IMongo _server;
        private  IMongoCollection<CheeseClubContact> _collection;
       
        [SetUp]
        public void Setup()
        {
            _server = Mongo.Create(TestHelper.ConnectionString("pooling=false","NormTests",null,null));
            _collection = _server.GetCollection<CheeseClubContact>("CheeseClubContacts");
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                _server.Database.DropCollection("CheeseClubContacts");
            }
            catch(MongoException e)
            {
                if (e.Message != "ns not found")
                {
                    throw;
                }
            }

            using (var admin = new MongoAdmin(TestHelper.ConnectionString ("pooling=false", "NormTests", null, null)))
            {
                admin.DropDatabase();
            }
            _server.Dispose();
        }

        [Test]
        public void Save_Inserts_Or_Updates()
        {
            var c = new CheeseClubContact();
            c.Id = ObjectId.NewObjectId();
            _collection.Save(c);
            var a = _collection.FindOne(new { c.Id });
            //prove it was inserted.
            Assert.AreEqual(c.Id, a.Id);

            c.Name = "hello";
            _collection.Save(c);
            var b = _collection.FindOne(new { c.Id });
            //prove that it was updated.
            Assert.AreEqual(c.Name, b.Name);
        }

        [Test]
        public void Save_should_insert_and_then_update_when_id_is_nullable_int()
        {
            var collection = _server.GetCollection<CheeseClubContactWithNullableIntId>();
            var subject = new CheeseClubContactWithNullableIntId();
            collection.Save(subject);

            var a = collection.FindOne(new { subject.Id });
            //prove it was inserted.
            Assert.AreEqual(subject.Id, a.Id);

            subject.Name = "hello";
            collection.Save(subject);

            var b = collection.FindOne(new { subject.Id });
            //prove that it was updated.
            Assert.AreEqual(subject.Name, b.Name);

            _server.Database.DropCollection(typeof(CheeseClubContactWithNullableIntId).Name);
        }

        [Test]
        public void Update_Multiple_With_Lambda_Works()
        {
            _collection.Insert(new CheeseClubContact { Name = "Hello" }, new CheeseClubContact { Name = "World" });
            _collection.Update(new { Name = Q.NotEqual("") }, h => h.SetValue(y => y.Name, "Cheese"), true, false);
            Assert.AreEqual(2, _collection.Find(new { Name = "Cheese" }).Count());
        }

        [Test]
        public void BasicUsageOfUpdateOne()
        {
            var aPerson = new CheeseClubContact { Name = "Joe", FavoriteCheese = "Cheddar" };
            _collection.Insert(aPerson);

            var count = _collection.Find();
            Assert.AreEqual(1, count.Count());

            var matchDocument = new { Name = "Joe" };
            aPerson.FavoriteCheese = "Gouda";

            _collection.UpdateOne(matchDocument, aPerson);

            var query = new Expando();
            query["Name"] = Q.Equals<string>("Joe");

            var retreivedPerson = _collection.FindOne(query);

            Assert.AreEqual("Gouda", retreivedPerson.FavoriteCheese);
        }

        [Test]
        public void BasicUsageOfUpdateOneUsingObjectId()
        {
            var aPerson = new CheeseClubContact { Name = "Joe", FavoriteCheese = "American" };
            _collection.Insert(aPerson);

            var results = _collection.Find();
            Assert.AreEqual(1, results.Count());

            var matchDocument = new { _id = aPerson.Id };
            aPerson.FavoriteCheese = "Velveeta";

            _collection.UpdateOne(matchDocument, aPerson);

            var query = new Expando();
            query["_id"] = Q.Equals<ObjectId>(aPerson.Id);

            var retreivedPerson = _collection.FindOne(query);

            Assert.AreEqual("Velveeta", retreivedPerson.FavoriteCheese);
        }

        [Test]
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

            var query = new Expando();
            query["Name"] = Q.Equals<string>("Joseph");

            var retreivedPerson = _collection.FindOne(query);

            Assert.Null(retreivedPerson.FavoriteCheese);
        }

        [Test]
        public void UsingUpsertToInsertOnUpdate()
        {
            //Upsert will update an existing document if it can find a match, otherwise it will insert.
            //In this scenario, we're updating our collection without inserting first.

            var aPerson = new CheeseClubContact { Name = "Joe", FavoriteCheese = "American" };

            var matchDocument = new { Name = aPerson.Name };
            _collection.Update(matchDocument, aPerson, false, true);

            var results = _collection.Find();
            Assert.AreEqual(1, results.Count());
        }
    }
}