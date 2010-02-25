namespace NoRM.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class MongoDatabaseTest : IDisposable
    {
        private const string _connectionString = "mongodb://localhost/NoRMTests?pooling=false";
        public void Dispose()
        {
            using (var admin = new MongoAdmin(_connectionString))
            {
                admin.DropDatabase();
            }      
        }
        
        [Fact]
        public void CreateCollectionCreatesACappedCollection()
        {
            using (var mongo = new Mongo(_connectionString))
            {
                mongo.Database.DropCollection("capped");
                Assert.Equal(true, mongo.Database.CreateCollection(new CreateCollectionOptions("capped") { Max = 3 }));
                var collection = mongo.GetCollection<FakeObject>("capped");
                collection.Insert(new FakeObject());
                collection.Insert(new FakeObject());
                collection.Insert(new FakeObject());
                collection.Insert(new FakeObject());
                Assert.Equal(3, collection.Find().Count());
            }
        }
        [Fact]
        public void CreateCollectionThrowsExceptionIfAlreadyExistsWithStrictMode()
        {
            using (var mongo = new Mongo(_connectionString))
            {
                mongo.Database.DropCollection("capped");
                mongo.Database.CreateCollection(new CreateCollectionOptions("capped"));
                var ex = Assert.Throws<MongoException>(() => mongo.Database.CreateCollection(new CreateCollectionOptions("capped")));
                Assert.Equal("Creation failed, the collection may already exist", ex.Message);
            }
        }
        [Fact]
        public void CreateCollectionReturnsFalseIfAlreadyExistsWithoutStrictMode()
        {
            using (var mongo = new Mongo(_connectionString + "&strict=false"))
            {
                mongo.Database.DropCollection("capped");
                mongo.Database.CreateCollection(new CreateCollectionOptions("capped"));
                Assert.Equal(false, mongo.Database.CreateCollection(new CreateCollectionOptions("capped")));                
            }
        }      
        
        [Fact]
        public void GetsAllCollections()
        {
            var expected = new List<string> { "NoRMTests.temp", "NoRMTests.temp2" };
            using (var mongo = new Mongo(_connectionString))
            {
                var database = mongo.Database;
                database.CreateCollection(new CreateCollectionOptions("temp"));
                database.CreateCollection(new CreateCollectionOptions("temp2"));
                foreach (var collection in database.GetAllCollections())
                {
                    Assert.Contains(collection.Name, expected);
                    expected.Remove(collection.Name);
                }
            }
            Assert.Equal(0, expected.Count);
        }
        [Fact]
        public void GetCollectionsReturnsNothingIfEmpty()
        {            
            using (var mongo = new Mongo(_connectionString))
            {                
                Assert.Equal(0, mongo.Database.GetAllCollections().Count());                
            }            
        }
    }
}

/*
namespace NoRM.Tests
{
    public class MiniObject
    {
        public OID ID { get; set; }
    }

    [TestFixture]
    [Category("Hits MongoDB")]
    public class MongoDatabaseTest
    {
        private MongoDatabase _db;
        private Mongo _server;

        [TestFixtureSetUp]
        public void SetUpDBTests()
        {
            var database = "test" + Guid.NewGuid().ToString().Substring(0, 5);
            var server = new Mongo("mongodb://localhost/" + database);
            this._server = server;
            this._db = server.Database;
        }
        [TestFixtureTearDown]
        public void TearDown()
        {
            using (var admin = new MongoAdmin("mongodb://localhost/" + _db.DatabaseName))
            {
                admin.DropDatabase();
            }                        
            _server.Dispose();
        }

        [Test]
        public void GetAllCollections_Returns_Collections()
        {
            var context = new Mongo();
            var db = context.GetDatabase("test");
            Assert.IsNotEmpty(db.GetAllCollections().ToList());
        }

        [Test]
        public void Drop_Collection_Returns_True()
        {
            var context = new Mongo();
            var db = context.GetDatabase("test");
            var collName = "testInsertCollection";
            db.GetCollection<MiniObject>(collName).Insert(new MiniObject());

            var results = db.DropCollection(collName);

            Assert.IsTrue((results.OK == 1.0));
        }

        [Test]
        public void Set_Profiling_Level()
        {
            var db = new Mongo().GetDatabase("test");

            var response = db.SetProfileLevel(NoRM.Protocol.SystemMessages.ProfileLevel.AllOperations);

            Assert.IsTrue((response.Was == 0.0));

            response = db.SetProfileLevel(NoRM.Protocol.SystemMessages.ProfileLevel.ProfilingOff);

            Assert.IsTrue((response.Was == 2.0));
        }

        [Test]
        public void Get_Profiling_Information()
        {
            var server = new Mongo();

            var db = server.GetDatabase("test");

            var results = db.GetProfilingInformation();

            foreach (var profile in results)
            {
                Console.WriteLine(profile.Info);
            }
            Assert.IsTrue((results.Count<ProfilingInformationResponse>() > 0));
        }

        [Test]
        public void Validate_Collection()
        {
            String collName = "validColl";
            var testColl = this._db.GetCollection<MiniObject>(collName);

            //must insert something before the collection will exist.
            testColl.Insert(new MiniObject());

            var response = this._db.ValidateCollection(collName, false);

            Assert.AreEqual(testColl.FullyQualifiedName, response.Ns);
        }


    }
}
*/