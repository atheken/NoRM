using System.Collections.Generic;
using System.Linq;
using Xunit;
using Norm.Collections;

namespace Norm.Tests
{
    
    public class MongoDatabaseTest
    {    
        public MongoDatabaseTest()
        {
            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                admin.DropDatabase();
            }      
        }

        [Fact]
        public void Get_Last_Error_Returns()
        {
            using(var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var le = mongo.Database.LastError();
                Assert.Equal(true,le.WasSuccessful);
            }
        }

        [Fact]
        public void CreateCollectionCreatesACappedCollection()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {                
                Assert.Equal(true, mongo.Database.CreateCollection(new CreateCollectionOptions("capped") { Capped = true, Size = 10000, Max = 3 }));
                var collection = mongo.GetCollection<FakeObject>("capped");
                collection.Insert(new FakeObject());
                collection.Insert(new FakeObject());
                collection.Insert(new FakeObject());
                collection.Insert(new FakeObject());
                Assert.Equal(3, collection.Find().Count());
            }
        }
        [Fact]
        public void CreateCollectionThrowsExceptionIfAlreadyExist()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {                
                mongo.Database.CreateCollection(new CreateCollectionOptions("capped"));
                var ex = Assert.Throws<MongoException>(() => mongo.Database.CreateCollection(new CreateCollectionOptions("capped")));
                Assert.Equal("collection already exists", ex.Message);
            }
        }
        [Fact]
        public void CreateCollectionFailsSilentlyWithStrictModeOff()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("strict=false")))
            {
                mongo.Database.CreateCollection(new CreateCollectionOptions("capped"));
                Assert.Equal(false, mongo.Database.CreateCollection(new CreateCollectionOptions("capped")));
            }
        }

    
        [Fact]
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
            Assert.Equal(0, expected.Count);
        }
        [Fact]
        public void GetCollectionsReturnsNothingIfEmpty()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                Assert.Equal(0, mongo.Database.GetAllCollections().Count());
            }
        }

        [Fact]
        public void DropsACollection()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var database = mongo.Database;
                database.CreateCollection(new CreateCollectionOptions("temp"));
                Assert.Equal(true, database.DropCollection("temp"));
                Assert.Equal(0, mongo.Database.GetAllCollections().Count());                           
            }
        }
        [Fact]
        public void ThrowsExceptionIfDropCollectionFailsWithStrictModeOn()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var ex = Assert.Throws<MongoException>(() => mongo.Database.DropCollection("temp"));
                Assert.Equal("ns not found", ex.Message);
            }
        }
        [Fact]
        public void DropCollectionFailsSilentlyWithStrictModeOff()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("&strict=false")))
            {
                Assert.Equal(false, mongo.Database.DropCollection("temp"));
            }
        }
        
        [Fact]
        public void ReturnsTheDatabasesName()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                Assert.Equal("NormTests", mongo.Database.DatabaseName);
            }
        }

        [Fact]
        public void GetsACollectionsStatistics()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {   
                mongo.Database.CreateCollection(new CreateCollectionOptions("temp"));
                var statistic = mongo.Database.GetCollectionStatistics("temp");                               
            }
        }

        [Fact]
        public void ThrowsExceptionIfGettingStatisticsFailsWithStrictModeOn()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var ex = Assert.Throws<MongoException>(() => mongo.Database.GetCollectionStatistics("temp"));
                Assert.Equal("ns not found", ex.Message);
            }
        }
        [Fact]
        public void GettingStatisticsFailsSilentlyWithStrictModeOff()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("&strict=false")))
            {
                Assert.Equal(null, mongo.Database.GetCollectionStatistics("temp"));
            }
        }

        [Fact]
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
        [Fact]
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

        [Fact]
        public void ValidateCollection()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var collection = mongo.Database.GetCollection<FakeObject>("validCollection");
                collection.Insert(new FakeObject());
                var response = mongo.Database.ValidateCollection("validCollection", false);
                Assert.Equal(collection.FullyQualifiedName, response.Ns);
            }
        }
    }
}