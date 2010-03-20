namespace Norm.Tests
{
    using System.Collections.Generic;
    using System.Linq;    
    using Xunit;
    using Norm.Collections;

    public class MongoDatabaseTest
    {
        private const string _connectionString = "mongodb://localhost/NormTests?pooling=false";

        public MongoDatabaseTest()
        {
            using (var admin = new MongoAdmin(_connectionString))
            {
                admin.DropDatabase();
            }      
        }
        
        [Fact]
        public void CreateCollectionCreatesACappedCollection()
        {
            using (var mongo = Mongo.ParseConnection(_connectionString))
            {                
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
        public void CreateCollectionThrowsExceptionIfAlreadyExist()
        {
            using (var mongo = Mongo.ParseConnection(_connectionString))
            {                
                mongo.Database.CreateCollection(new CreateCollectionOptions("capped"));
                var ex = Assert.Throws<MongoException>(() => mongo.Database.CreateCollection(new CreateCollectionOptions("capped")));
                Assert.Equal("collection already exists", ex.Message);
            }
        }
        [Fact]
        public void CreateCollectionFailsSilentlyWithStrictModeOff()
        {
            using (var mongo = Mongo.ParseConnection(_connectionString + "&strict=false"))
            {
                mongo.Database.CreateCollection(new CreateCollectionOptions("capped"));
                Assert.Equal(false, mongo.Database.CreateCollection(new CreateCollectionOptions("capped")));
            }
        }

    
        [Fact]
        public void GetsAllCollections()
        {
            var expected = new List<string> { "NormTests.temp", "NormTests.temp2" };
            using (var mongo = Mongo.ParseConnection(_connectionString))
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
            using (var mongo = Mongo.ParseConnection(_connectionString))
            {
                Assert.Equal(0, mongo.Database.GetAllCollections().Count());
            }
        }

        [Fact]
        public void DropsACollection()
        {
            using (var mongo = Mongo.ParseConnection(_connectionString))
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
            using (var mongo = Mongo.ParseConnection(_connectionString))
            {
                var ex = Assert.Throws<MongoException>(() => mongo.Database.DropCollection("temp"));
                Assert.Equal("ns not found", ex.Message);
            }
        }
        [Fact]
        public void DropCollectionFailsSilentlyWithStrictModeOff()
        {
            using (var mongo = Mongo.ParseConnection(_connectionString + "&strict=false"))
            {
                Assert.Equal(false, mongo.Database.DropCollection("temp"));
            }
        }
        
        [Fact]
        public void ReturnsTheDatabasesName()
        {
            using (var mongo = Mongo.ParseConnection(_connectionString))
            {
                Assert.Equal("NormTests", mongo.Database.DatabaseName);
            }
        }

        [Fact(Skip = "failing to deserialized, this appears to return a more complex object than what we are ready to handle")]
        public void GetsACollectionsStatistics()
        {
            using (var mongo = Mongo.ParseConnection(_connectionString))
            {   
                mongo.Database.CreateCollection(new CreateCollectionOptions("temp"));
                var statistic = mongo.Database.GetCollectionStatistics("temp");                               
            }
        }
        [Fact]
        public void ThrowsExceptionIfGettingStatisticsFailsWithStrictModeOn()
        {
            using (var mongo = Mongo.ParseConnection(_connectionString))
            {
                var ex = Assert.Throws<MongoException>(() => mongo.Database.GetCollectionStatistics("temp"));
                Assert.Equal("ns not found", ex.Message);
            }
        }
        [Fact]
        public void GettingStatisticsFailsSilentlyWithStrictModeOff()
        {
            using (var mongo = Mongo.ParseConnection(_connectionString + "&strict=false"))
            {
                Assert.Equal(null, mongo.Database.GetCollectionStatistics("temp"));
            }
        }

        [Fact]
        public void SetProfilingLevel()
        {
            using (var mongo = Mongo.ParseConnection(_connectionString))
            {
                var response = mongo.Database.SetProfileLevel(Protocol.SystemMessages.ProfileLevel.AllOperations);
                Assert.True((response.Was == 0.0));

                response = mongo.Database.SetProfileLevel(Protocol.SystemMessages.ProfileLevel.ProfilingOff);
                Assert.True((response.Was == 2.0));
            }
        }
        [Fact]
        public void GetProfilingInformation()
        {
            using (var mongo = Mongo.ParseConnection(_connectionString))
            {
                mongo.Database.SetProfileLevel(Protocol.SystemMessages.ProfileLevel.AllOperations);
                mongo.GetCollection<FakeObject>().Insert(new FakeObject());
                mongo.GetCollection<FakeObject>().Find();
                mongo.Database.SetProfileLevel(Protocol.SystemMessages.ProfileLevel.ProfilingOff);

                var results = mongo.Database.GetProfilingInformation();                
                Assert.Equal("insert NormTests.FakeObject", results.ElementAt(0).Info);
                Assert.Equal("query NormTests.FakeObject ntoreturn:1 reslen:36 nscanned:1  \nquery: { getlasterror: 1.0 }  nreturned:0 bytes:20", results.ElementAt(1).Info);
                Assert.Equal("query NormTests.$cmd ntoreturn:1 command  reslen:66 bytes:50", results.ElementAt(2).Info);                
                Assert.Equal(3, results.Count());
            }
        }

        [Fact]
        public void ValidateCollection()
        {            
            using (var mongo = Mongo.ParseConnection(_connectionString))
            {
                var collection = mongo.Database.GetCollection<FakeObject>("validCollection");
                collection.Insert(new FakeObject());
                var response = mongo.Database.ValidateCollection("validCollection", false);
                Assert.Equal(collection.FullyQualifiedName, response.Ns);
            }
        }
    }
}