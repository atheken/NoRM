
using System.Linq;
using NoRM.Configuration;
using Xunit;

namespace NoRM.Tests
{
    public class MongoConfigutationTests
    {
        [Fact]
        public void MongoConfigurationMapsMergesConfigurationMaps()
        {
            MongoConfiguration.Initialize(r => r.AddMap<CustomMap>());
            MongoConfiguration.Initialize(r => r.AddMap<OtherMap>());

            using (var mongo = Mongo.ParseConnection(TestHelper.ConnectionString()))
            {
                mongo.GetCollection<User>().Insert(new User { FirstName = "Test", LastName = "User" });

                var result = mongo.GetCollection<User>().Find().First();

                // Did the deserialization take into account the alias-to-field mapping?
                Assert.Equal("Test", result.FirstName);
                Assert.Equal("User", result.LastName);

                using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
                {
                    admin.DropDatabase();
                }
            }
        }

        [Fact]
        public void MongoConfigurationMapsCollectionNameToAlias()
        {
            /// xUnit doesn't have a tear down method, and it uses the same app domain for all tests.  
            /// That means that the static properties used for BSON deserialization as well as this
            /// configuration aren't destroyed after each test.  This test will fail if all tests
            /// in this class are run at once.
            /// 
            MongoConfiguration.Initialize(r => r.For<User>(user => user.UseCollectionNamed("UserCollection")));

            using (var mongo = Mongo.ParseConnection(TestHelper.ConnectionString()))
            {
                mongo.GetCollection<User>().Insert(new User { FirstName = "Test", LastName = "User" });

                var user = mongo.GetCollection<User>().Find().First();

                Assert.NotNull(user);
            }

            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                admin.DropDatabase();
            }
        }

        [Fact]
        public void MongoConfigurationSupportsLambdaSyntaxRegistration()
        {
            MongoConfiguration.Initialize(r => r.For<User>(u => u.ForProperty(user => user.FirstName).UseAlias("first")));

            var alias = MongoConfiguration.GetPropertyAlias(typeof(User), "FirstName");

            Assert.Equal("first", alias);
        }

        [Fact]
        public void MongoConfigurationEchoesMissingPropertyNames()
        {
            MongoConfiguration.Initialize(r => r.For<User>(u => u.ForProperty(user => user.FirstName).UseAlias("first"))/*.WithProfileNamed("Sample")*/);

            var first = MongoConfiguration.GetPropertyAlias(typeof(User), "FirstName");
            var last = MongoConfiguration.GetPropertyAlias(typeof(User), "LastName");

            Assert.Equal("first", first);
            Assert.Equal("LastName", last);
        }

        [Fact]
        public void MongoConfigurationReturnsNullForUninitializedTypeConnectionStrings()
        {
            MongoConfiguration.Initialize(r => r.For<User>(u => u.ForProperty(user => user.FirstName).UseAlias("first"))/*.WithProfileNamed("Sample")*/);

            var connection = MongoConfiguration.GetConnectionString(typeof(Product));

            Assert.Equal(null, connection);
        }
    }

    public class OtherMap : MongoConfigurationMap
    {
        public OtherMap()
        {
            For<User>(cfg => cfg.ForProperty(u => u.LastName).UseAlias("last"));
        }
    }

    public class CustomMap : MongoConfigurationMap
    {
        public CustomMap()
        {
            For<User>(cfg =>
                          {
                              cfg.ForProperty(u => u.FirstName).UseAlias("first");
                              cfg.ForProperty(u => u.LastName).UseAlias("last");
                              cfg.UseCollectionNamed("UserBucket");
                              cfg.UseConnectionString(TestHelper.ConnectionString());
                          });

            For<Product>(cfg => cfg.ForProperty(p => p.Name).UseAlias("productname"));
        }
    }

    public class User
    {
        public User()
        {
            Id = ObjectId.NewObjectId();
        }
        public ObjectId Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
