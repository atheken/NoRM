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

        [Fact]
        public void DropsACollection()
        {
            using (var mongo = new Mongo(_connectionString))
            {
                var database = mongo.Database;
                database.CreateCollection(new CreateCollectionOptions("temp"));
                Assert.Equal(1d, database.DropCollection("temp").OK);
                Assert.Equal(0, mongo.Database.GetAllCollections().Count());                           
            }
        }
        [Fact]
        public void ThrowsExceptionIfDropConnectionFailsWithStrictModeOn()
        {
            using (var mongo = new Mongo(_connectionString))
            {
                var ex = Assert.Throws<MongoException>(() => mongo.Database.DropCollection("temp"));
                Assert.Equal("Drop failed, are you sure the collection exists", ex.Message);
            }
        }
        [Fact]
        public void DropCollectionFailsSilentlyWithStrictModeOff()
        {
            using (var mongo = new Mongo(_connectionString + "&strict=false"))
            {
                Assert.Equal(0d, mongo.Database.DropCollection("temp").OK);
            }
        }
        
        [Fact]
        public void ReturnsTheDatabasesName()
        {
            using (var mongo = new Mongo(_connectionString))
            {
                Assert.Equal("NoRMTests", mongo.Database.DatabaseName);
            }
        }

        [Fact(Skip = "failing to deserialized, this appears to return a more complex object than what we are ready to handle")]
        public void GetsACollectionsStatistics()
        {
            using (var mongo = new Mongo(_connectionString))
            {   
                mongo.Database.CreateCollection(new CreateCollectionOptions("temp"));
                var statistic = mongo.Database.GetCollectionStatistics("temp");                               
            }
        }
        [Fact]
        public void ThrowsExceptionIfGettingStatisticsFailsWithStrictModeOn()
        {
            using (var mongo = new Mongo(_connectionString))
            {
                var ex = Assert.Throws<MongoException>(() => mongo.Database.GetCollectionStatistics("temp"));
                Assert.Equal("Could not get statistics, are you sure the collection exists", ex.Message);
            }
        }
        [Fact]
        public void GettingStatisticsFailsSilentlyWithStrictModeOff()
        {
            using (var mongo = new Mongo(_connectionString + "&strict=false"))
            {
                Assert.Equal(0d, mongo.Database.GetCollectionStatistics("temp").OK);
            }
        }

        [Fact]
        public void SetProfilingLevel()
        {
            using (var mongo = new Mongo(_connectionString))
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
            using (var mongo = new Mongo(_connectionString))
            {
                mongo.Database.SetProfileLevel(Protocol.SystemMessages.ProfileLevel.AllOperations);
                mongo.GetCollection<FakeObject>().Insert(new FakeObject());
                mongo.GetCollection<FakeObject>().Find();
                mongo.Database.SetProfileLevel(Protocol.SystemMessages.ProfileLevel.ProfilingOff);

                var results = mongo.Database.GetProfilingInformation();                
                Assert.Equal("insert NoRMTests.FakeObject", results.ElementAt(0).Info);
                Assert.Equal("query NoRMTests.FakeObject ntoreturn:1 reslen:36 nscanned:1  \nquery: { getlasterror: 1.0 }  nreturned:0 bytes:20", results.ElementAt(1).Info);
                Assert.Equal("query NoRMTests.$cmd ntoreturn:1 command  reslen:66 bytes:50", results.ElementAt(2).Info);                
                Assert.Equal(3, results.Count());
            }
        }

        [Fact]
        public void ValidateCollection()
        {            
            using (var mongo = new Mongo(_connectionString))
            {
                var collection = mongo.Database.GetCollection<FakeObject>("validCollection");
                collection.Insert(new FakeObject());
                var response = mongo.Database.ValidateCollection("validCollection", false);
                Assert.Equal(collection.FullyQualifiedName, response.Ns);
            }
        }
    }
}