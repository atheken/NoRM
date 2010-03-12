
using System;
using System.Linq;
using NoRM.Configuration;
using NoRM.Linq;
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

        [Fact]
        public void MongoConfigurationWithLinqSupportsAliases()
        {
            MongoConfiguration.Initialize(c => c.AddMap<ShopperMap>());
            using (var shoppers = new Shoppers(new MongoQueryProvider("test", "localhost", "27017", "")))
            {
                shoppers.Drop<Shopper>();
                shoppers.Add(new Shopper
                {
                    Id = ObjectId.NewObjectId(),
                    Name = "John",
                    Cart = new Cart
                    {
                        Id = ObjectId.NewObjectId(),
                        Name = "Cart1",
                        Product = new Product { Name = "SomeProduct" }
                    }
                });

                shoppers.Add(new Shopper
                {
                    Id = ObjectId.NewObjectId(),
                    Name = "Jane",
                    Cart = new Cart
                    {
                        Id = ObjectId.NewObjectId(),
                        Name = "Cart2",
                        Product = new Product { Name = "OtherProduct" }
                    }
                });

                var shallowResult = shoppers.Where(x => x.Name == "Jane").ToList();
                Assert.Equal(1, shallowResult.Count());

                var deepResult = shoppers.Where(x => x.Cart.Name == "Cart1").ToList();
                Assert.Equal(1, deepResult.Count());

                var deeperResult = shoppers.Where(x => x.Cart.Product.Name == "OtherProduct").ToList();
                Assert.Equal(1, deeperResult.Count());
            }
        }

        [Fact]
        public void AreQueriesFullyLinqified()
        {
            MongoConfiguration.Initialize(c => c.AddMap<ShopperMap>());
            using (var shoppers = new Shoppers(new MongoQueryProvider("test", "localhost", "27017", "")))
            {
                shoppers.Drop<Shopper>();
                shoppers.Add(new Shopper
                {
                    Id = ObjectId.NewObjectId(),
                    Name = "John",
                    Cart = new Cart
                    {
                        Id = ObjectId.NewObjectId(),
                        Name = "Cart1",
                        CartSuppliers = new[] { new Supplier { Name = "Supplier1" }, new Supplier { Name = "Supplier2" } }
                    }
                });

                shoppers.Add(new Shopper
                {
                    Id = ObjectId.NewObjectId(),
                    Name = "Jane",
                    Cart = new Cart
                    {
                        Id = ObjectId.NewObjectId(),
                        Name = "Cart2",
                        CartSuppliers = new[] { new Supplier { Name = "Supplier3" }, new Supplier { Name = "Supplier4" } }
                    }
                });

                // Dies a miserable death when the translator splits this query by "." leaving
                // no way to deal with an argument called "First()"
                var tooDeep = shoppers.Where(x => x.Cart.CartSuppliers.First().Name == "Supplier1").ToList();
            }
        }

        public class ShopperMap : MongoConfigurationMap
        {
            public ShopperMap()
            {
                For<Shopper>(config =>
                {
                    config.UseCollectionNamed("MyProducts");
                    config.ForProperty(u => u.Name).UseAlias("shopperName");
                    config.ForProperty(u => u.Cart).UseAlias("MyCart");
                });

                For<Cart>(c =>
                {
                    c.UseCollectionNamed("ListOfCarts");
                    c.ForProperty(cart => cart.Product).UseAlias("ProductsGoHere");
                    c.ForProperty(ca => ca.Name).UseAlias("ThisCartName");
                });

                For<Product>(c => c.ForProperty(p => p.Price).UseAlias("DiscountPrice"));
            }
        }

        internal class Shopper
        {
            public Shopper()
            {
                Id = ObjectId.NewObjectId();
            }

            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public Cart Cart { get; set; }
        }

        internal class Cart
        {
            public Cart()
            {
                Id = ObjectId.NewObjectId();
            }
            public string Name { get; set; }
            public ObjectId Id { get; set; }
            public Product Product { get; set; }
            public Supplier[] CartSuppliers { get; set; }
        }

        internal class Shoppers : MongoQuery<Shopper>, IDisposable
        {
            private readonly MongoQueryProvider _provider;

            public Shoppers(MongoQueryProvider provider)
                : base(provider)
            {
                _provider = provider;
            }

            public MongoQueryProvider Provider
            {
                get
                {
                    return _provider;
                }
            }

            public T MapReduce<T>(string map, string reduce)
            {
                var result = default(T);
                using (var mr = _provider.Mongo.CreateMapReduce())
                {
                    var response = mr.Execute(new MapReduceOptions(typeof(T).Name) { Map = map, Reduce = reduce });
                    var coll = response.GetCollection<MapReduceResult<T>>();
                    var r = coll.Find().FirstOrDefault();
                    result = r.Value;
                }
                return result;
            }

            public void Add<T>(T item) where T : class, new()
            {
                _provider.DB.GetCollection<T>().Insert(item);
            }

            public void Update<T>(T item) where T : class, new()
            {
                _provider.DB.GetCollection<T>().UpdateOne(item, item);
            }

            public void Drop<T>()
            {
                _provider.DB.DropCollection(MongoConfiguration.GetCollectionName(typeof(T)));
            }

            #region IDisposable Members

            public void Dispose()
            {
                _provider.Mongo.Dispose();
            }

            #endregion
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
    }
}