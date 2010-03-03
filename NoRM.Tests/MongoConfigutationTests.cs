
using System.Linq;
using NoRM.Configuration;
using Xunit;

namespace NoRM.Tests
{
    public class MongoConfigutationTests
    {
        [Fact]
        public void MongoConfigurationMapsPropertyNameToAlias()
        {
            MongoConfiguration.Initialize(r => r.UseMap(new CustomMap()));

            using (var mongo = Mongo.ParseConnection("mongodb://localhost/NoRMTests"))
            {
                mongo.GetCollection<User>().Insert(new User { FirstName = "Test", LastName = "User" });

                var result = mongo.GetCollection<User>().Find().First();

                // Did the deserialization take into account the alias-to-field mapping?
                Assert.Equal("Test", result.FirstName);
                Assert.Equal("User", result.LastName);

                using (var admin = new MongoAdmin("mongodb://localhost/NoRMTests"))
                {
                    admin.DropDatabase();
                }
            }

            MongoConfiguration.Reset();
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

            using (var mongo = Mongo.ParseConnection("mongodb://localhost/NoRMTests"))
            {
                mongo.GetCollection<User>().Insert(new User { FirstName = "Test", LastName = "User" });

                var user = mongo.GetCollection<User>().Find().First();

                Assert.NotNull(user);
            }

            using (var admin = new MongoAdmin("mongodb://localhost/NoRMTests"))
            {
                admin.DropDatabase();
            }

            MongoConfiguration.Reset();
        }

        [Fact]
        public void MongoConfigurationSupportsRegistrationSubtypes()
        {
            MongoConfiguration.Initialize(r => r.UseMap(new CustomMap()));

            var first = MongoConfiguration.GetPropertyAlias(typeof(User), "FirstName");
            var last = MongoConfiguration.GetPropertyAlias(typeof(User), "LastName");
            var collection = MongoConfiguration.GetCollectionName(typeof(User));
            var connection = MongoConfiguration.GetConnectionString(typeof(User));

            Assert.Equal("first", first);
            Assert.Equal("last", last);
            Assert.Equal("UserBucket", collection);
            Assert.Equal("mongodb://localhost/NoRMTests", connection);

            MongoConfiguration.Reset();
        }

        [Fact]
        public void MongoConfigurationSupportsLambdaSyntaxRegistration()
        {
            MongoConfiguration.Initialize(r => r.For<User>(u => u.ForProperty(user => user.FirstName).UseAlias("first")));

            var alias = MongoConfiguration.GetPropertyAlias(typeof(User), "FirstName");

            Assert.Equal("first", alias);
            MongoConfiguration.Reset();
        }

        [Fact]
        public void MongoConfigurationEchoesMissingPropertyNames()
        {
            MongoConfiguration.Initialize(r => r.For<User>(u => u.ForProperty(user => user.FirstName).UseAlias("first")));

            var first = MongoConfiguration.GetPropertyAlias(typeof(User), "FirstName");
            var last = MongoConfiguration.GetPropertyAlias(typeof(User), "LastName");

            Assert.Equal("first", first);
            Assert.Equal("LastName", last);
            MongoConfiguration.Reset();
        }

        [Fact]
        public void MongoConfigurationReturnsNullForUninitializedTypeConnectionStrings()
        {
            MongoConfiguration.Initialize(r => r.For<User>(u => u.ForProperty(user => user.FirstName).UseAlias("first")));

            var connection = MongoConfiguration.GetConnectionString(typeof(Product));

            Assert.Equal(null, connection);
            MongoConfiguration.Reset();
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
                              cfg.UseConnectionString("mongodb://localhost/NoRMTests");
                          });

            For<Product>(cfg => cfg.ForProperty(p => p.Name).UseAlias("productname"));
        }
    }

    public class EmptyMap : MongoConfigurationMap
    {
        
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
