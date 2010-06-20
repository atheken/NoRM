using System;
using System.Linq;
using Norm.BSON;
using Norm.Configuration;
using Norm.Linq;
using Xunit;

namespace Norm.Tests
{
    public class MongoConfigurationTests
    {
        public MongoConfigurationTests()
        {
            MongoConfiguration.RemoveMapFor<User2>();
            MongoConfiguration.RemoveMapFor<User>();
            MongoConfiguration.RemoveMapFor<TestProduct>();
            MongoConfiguration.RemoveMapFor<Shopper>();
            MongoConfiguration.RemoveMapFor<Cart>();
            MongoConfiguration.RemoveMapFor<TestProduct>();

            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                admin.DropDatabase();
            }
        }

        [Fact]
        public void Mongo_Configuration_Should_AutoMap_Id_Property()
        {
            Assert.Equal("_id", MongoConfiguration.GetPropertyAlias(typeof(IdMap0), "_ID"));
            Assert.Equal("_id", MongoConfiguration.GetPropertyAlias(typeof(IdMap1), "TheID"));
            Assert.Equal("_id", MongoConfiguration.GetPropertyAlias(typeof(IdMap2), "ID"));
            Assert.Equal("_id", MongoConfiguration.GetPropertyAlias(typeof(IdMap3), "id"));
            Assert.Equal("_id", MongoConfiguration.GetPropertyAlias(typeof(IdMap4), "Id"));
        }

        [Fact]
        public void Mongo_Configuration_Should_Notify_TypeHelper()
        {
            var typeHelper = ReflectionHelper.GetHelperForType(typeof(User2));
            Assert.Equal("LastName", typeHelper.FindProperty("LastName").Name);

            //the mapping should cause the typehelper cache to be rebuilt with the new properties.
            MongoConfiguration.Initialize(cfg => cfg.For<User2>(j => j.ForProperty(k => k.LastName).UseAlias("LNAME")));

            typeHelper = ReflectionHelper.GetHelperForType(typeof(User2));
            Assert.Equal("LastName", typeHelper.FindProperty("LNAME").Name);
        }

        [Fact]
        public void Mongo_Configuration_Maps_Collection_Name_To_Alias()
        {
            MongoConfiguration.Initialize(r => r.For<User2>(user => user.UseCollectionNamed("User2Collection")));
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                mongo.GetCollection<User2>().Insert(new User2 { FirstName = "Test", LastName = "User" });
                var user = mongo.GetCollection<User2>().Find().First();
                Assert.NotNull(user);
            }
        }

        [Fact]
        public void Mongo_Configuration_Maps_Merges_Configuration_Maps()
        {
            //this is not a very good test because it doesn't confirm that on of the properties is ever set.
            MongoConfiguration.Initialize(r => r.AddMap<CustomMap>());
            MongoConfiguration.Initialize(r => r.AddMap<OtherMap>());

            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                mongo.GetCollection<User>().Insert(new User { FirstName = "Test", LastName = "User" });

                var result = mongo.GetCollection<User>().Find().First();

                // Did the deserialization take into account the alias-to-field mapping?
                Assert.Equal("Test", result.FirstName);
                Assert.Equal("User", result.LastName);
            }
        }

        [Fact]
        public void Mongo_Configuration_Supports_Lambda_Syntax_Registration()
        {
            MongoConfiguration.Initialize(r => r.For<User>(u => u.ForProperty(user => user.FirstName).UseAlias("first")));
            var alias = MongoConfiguration.GetPropertyAlias(typeof(User), "FirstName");
            Assert.Equal("first", alias);
        }

        [Fact]
        public void Mongo_Configuration_Can_Remove_Mapping()
        {
            MongoConfiguration.Initialize(r => r.For<User>(u => u.ForProperty(h => h.LastName).UseAlias("lName")));
            //confirm that mapping was set.
            Assert.Equal("lName", MongoConfiguration.GetPropertyAlias(typeof(User), "LastName"));

            MongoConfiguration.RemoveMapFor<User>();
            //confirm that mapping was unset.
            Assert.Equal("LastName", MongoConfiguration.GetPropertyAlias(typeof(User), "LastName"));
        }

        [Fact]
        public void Mongo_Configuration_Remove_Mapping_Of_Norm_Types_Fails()
        {
            //removal of maps for Norm types is verboden.
            Assert.Throws<NotSupportedException>(() => MongoConfiguration.RemoveMapFor<MongoDatabase>());
        }

        [Fact]
        public void Mongo_Configuration_Echos_Unmapped_Property_Names()
        {
            MongoConfiguration.Initialize(r => r.For<User>(u => u.ForProperty(user => user.FirstName)
                .UseAlias("first"))/*.WithProfileNamed("Sample")*/);

            var first = MongoConfiguration.GetPropertyAlias(typeof(User), "FirstName");
            var last = MongoConfiguration.GetPropertyAlias(typeof(User), "LastName");

            Assert.Equal("first", first);
            Assert.Equal("LastName", last);
        }

        [Fact]
        public void Mongo_Configuration_Returns_Null_For_Uninitialized_Type_Connection_Strings()
        {
            MongoConfiguration.Initialize(r => r.For<User>(u => u.ForProperty(user => user.FirstName).UseAlias("thisIsntDying")));

            var connection = MongoConfiguration.GetConnectionString(typeof(TestProduct));

            Assert.Equal(null, connection);
        }

        [Fact]
        public void Mongo_Configuration_With_Linq_Supports_Aliases()
        {

            MongoConfiguration.Initialize(c => c.AddMap<ShopperMap>());
            using (var shoppers = new Shoppers(Mongo.Create("mongodb://localhost:27017/test")))
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
                        Product = new TestProduct { Name = "SomeProduct" }
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
                        Product = new TestProduct { Name = "OtherProduct" }
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
        public void Are_Queries_Fully_Linqified()
        {
            MongoConfiguration.Initialize(c => c.AddMap<ShopperMap>());
            using (var shoppers = new Shoppers(Mongo.Create("mongodb://localhost:27017/test")))
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

                var deepQuery = shoppers.Where(x => x.Cart.CartSuppliers.Any(y=>y.Name == "Supplier1")).ToList();
                Assert.Equal("John", deepQuery[0].Name);
                Assert.Equal("Cart1", deepQuery[0].Cart.Name);
                Assert.Equal(1, deepQuery.Count);
            }
        }

        [Fact]
        public void Can_correctly_determine_collection_name()
        {
            var collectionName = MongoConfiguration.GetCollectionName(typeof(SuperClassObject));

            Assert.Equal("SuperClassObject", collectionName);
        }

        [Fact]
        public void Can_correctly_determine_collection_name_from_discriminated_sub_class()
        {
            var collectionName = MongoConfiguration.GetCollectionName(typeof(SubClassedObject));

            Assert.Equal("SuperClassObject", collectionName);
        }

        [Fact]
        public void Can_correctly_determine_collection_name_when_discriminator_is_on_an_interface()
        {
            var collectionName = MongoConfiguration.GetCollectionName(typeof(InterfaceDiscriminatedClass));

            Assert.Equal("IDiscriminated", collectionName);
        }

        [Fact]
        public void Can_correctly_determine_collection_name_from_discriminated_sub_class_when_fluent_mapped()
        {
            var collectionName = MongoConfiguration.GetCollectionName(typeof(SubClassedObjectFluentMapped));

            Assert.Equal("SuperClassObjectFluentMapped", collectionName);

        }

        [Fact]
        public void Subclass_Adheres_To_Superclass_Fluent_Alias()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var obj1 = new SubClassedObjectFluentMapped { Title = "Prod1", ABool = true };
                var obj2 = new SubClassedObjectFluentMapped { Title = "Prod2", ABool = false };

                mongo.GetCollection<SubClassedObjectFluentMapped>("Fake").Insert(obj1);
                mongo.GetCollection<SubClassedObjectFluentMapped>("Fake").Insert(obj2);
                var found = mongo.GetCollection<SuperClassObjectFluentMapped>("Fake").Find();

                Assert.Equal(2, found.Count());
                Assert.Equal(obj1.Id, found.ElementAt(0).Id) ;
                Assert.Equal("Prod1", found.ElementAt(0).Title);
                Assert.Equal(obj2.Id, found.ElementAt(1).Id);
                Assert.Equal("Prod2", found.ElementAt(1).Title);

            }
        }

        [Fact]
        public void Subclassed_Type_Is_Returned_When_Superclass_Is_Used_For_The_Collection()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var obj1 = new SubClassedObjectFluentMapped { Title = "Prod1", ABool = true };
                mongo.GetCollection<SubClassedObjectFluentMapped>("Fake").Insert(obj1);
                var found = mongo.GetCollection<SuperClassObjectFluentMapped>("Fake").Find();

                Assert.Equal(1, found.Count());
                Assert.Equal(typeof(SubClassedObjectFluentMapped), found.ElementAt(0).GetType());
            }
        }

        [Fact]
        public void Can_fluently_configure_discriminator_for_all_implementations_of_an_interface()
        {
            MongoConfiguration.Initialize(r => r.AddMap<DiscriminationMap>());
            
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var obj1 = new InterfacePropertyContainingClass();
                mongo.GetCollection<InterfacePropertyContainingClass>().Insert(obj1);
                var found = mongo.GetCollection<InterfacePropertyContainingClass>().Find();

                Assert.Equal(1, found.Count());
                Assert.Equal(typeof(NotDiscriminatedClass), found.ElementAt(0).InterfaceProperty.GetType());
            }
        }
    }
}