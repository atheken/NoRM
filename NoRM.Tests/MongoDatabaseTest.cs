using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Norm.Collections;

namespace Norm.Tests
{
    [TestFixture]
    public class MongoDatabaseTest
    {
        [SetUp]
        public void Setup()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                admin.DropDatabase();
            }      
        }

        [Test]
        public void Get_Last_Error_Returns()
        {
            using(var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var le = mongo.Database.LastError();
                Assert.AreEqual(true,le.WasSuccessful);
            }
        }

        [Test]
        public void CreateCollectionCreatesACappedCollection()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {                
                Assert.AreEqual(true, mongo.Database.CreateCollection(new CreateCollectionOptions("capped") { Capped = true, Size = 10000, Max = 3 }));
                var collection = mongo.GetCollection<FakeObject>("capped");
                collection.Insert(new FakeObject());
                collection.Insert(new FakeObject());
                collection.Insert(new FakeObject());
                collection.Insert(new FakeObject());
                Assert.AreEqual(3, collection.Find().Count());
            }
        }
        [Test]
        public void CreateCollectionThrowsExceptionIfAlreadyExist()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {                
                mongo.Database.CreateCollection(new CreateCollectionOptions("capped"));
                var ex = Assert.Throws<MongoException>(() => mongo.Database.CreateCollection(new CreateCollectionOptions("capped")));
                Assert.AreEqual("collection already exists", ex.Message);
            }
        }
        [Test]
        public void CreateCollectionFailsSilentlyWithStrictModeOff()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("strict=false")))
            {
                mongo.Database.CreateCollection(new CreateCollectionOptions("capped"));
                Assert.AreEqual(false, mongo.Database.CreateCollection(new CreateCollectionOptions("capped")));
            }
        }

    
        [Test]
        public void GetsAllCollections()
        {
            var expected = new List<string> { "NormTests.temp", "NormTests.temp2" };
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
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
            Assert.AreEqual(0, expected.Count);
        }
        [Test]
        public void GetCollectionsReturnsNothingIfEmpty()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                Assert.AreEqual(0, mongo.Database.GetAllCollections().Count());
            }
        }

        [Test]
        public void DropsACollection()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var database = mongo.Database;
                database.CreateCollection(new CreateCollectionOptions("temp"));
                Assert.AreEqual(true, database.DropCollection("temp"));
                Assert.AreEqual(0, mongo.Database.GetAllCollections().Count());                           
            }
        }
        [Test]
        public void ThrowsExceptionIfDropCollectionFailsWithStrictModeOn()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var ex = Assert.Throws<MongoException>(() => mongo.Database.DropCollection("temp"));
                Assert.AreEqual("ns not found", ex.Message);
            }
        }
        [Test]
        public void DropCollectionFailsSilentlyWithStrictModeOff()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("&strict=false")))
            {
                Assert.AreEqual(false, mongo.Database.DropCollection("temp"));
            }
        }
        
        [Test]
        public void ReturnsTheDatabasesName()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                Assert.AreEqual("NormTests", mongo.Database.DatabaseName);
            }
        }

        [Test]
        public void GetsACollectionsStatistics()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {   
                mongo.Database.CreateCollection(new CreateCollectionOptions("temp"));
                var statistic = mongo.Database.GetCollectionStatistics("temp");                               
            }
        }

        [Test]
        public void ThrowsExceptionIfGettingStatisticsFailsWithStrictModeOn()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var ex = Assert.Throws<MongoException>(() => mongo.Database.GetCollectionStatistics("temp"));
                Assert.AreEqual("ns not found", ex.Message);
            }
        }
        [Test]
        public void GettingStatisticsFailsSilentlyWithStrictModeOff()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("&strict=false")))
            {
                Assert.AreEqual(null, mongo.Database.GetCollectionStatistics("temp"));
            }
        }

        [Test]
        public void SetProfilingLevel()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var response = mongo.Database.SetProfileLevel(Protocol.SystemMessages.ProfileLevel.AllOperations);
                Assert.True((response.PreviousLevel == 0.0));

                response = mongo.Database.SetProfileLevel(Protocol.SystemMessages.ProfileLevel.ProfilingOff);
                Assert.True((response.PreviousLevel == 2.0));
            }
        }
        [Test]
        public void GetProfilingInformation()
        {
            //this seems to vary a lot from version to version and who knows what else
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                mongo.Database.SetProfileLevel(Protocol.SystemMessages.ProfileLevel.AllOperations);
                mongo.GetCollection<FakeObject>().Insert(new FakeObject());
                mongo.GetCollection<FakeObject>().Find();
                mongo.Database.SetProfileLevel(Protocol.SystemMessages.ProfileLevel.ProfilingOff);

                var results = mongo.Database.GetProfilingInformation();
                var resultsInfos = results.Select(r => r.Info).ToArray();
                Assert.True(results.Any());
                Assert.True(results.All(y => y.Info != null));
            }
        }

        [Test]
        public void ValidateCollection()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var collection = mongo.Database.GetCollection<FakeObject>("validCollection");
                collection.Insert(new FakeObject());
                var response = mongo.Database.ValidateCollection("validCollection", false);
                Assert.AreEqual(collection.FullyQualifiedName, response.Ns);
            }
        }
    }
}