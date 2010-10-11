using System;
using System.Linq;
using Norm.BSON;
using Norm.Configuration;
using Norm.Linq;
using NUnit.Framework;

namespace Norm.Tests
{
    [TestFixture]
    public class MongoConfigurationTests
    {
		private Mongod _proc;

		[TestFixtureSetUp]
		public void SetUp ()
		{
			_proc = new Mongod ();
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			_proc.Dispose ();
		}

        [SetUp]
        public void Setup()
        {
            MongoConfiguration.RemoveMapFor<User2>();
            MongoConfiguration.RemoveMapFor<User>();
            MongoConfiguration.RemoveMapFor<TestProduct>();
            MongoConfiguration.RemoveMapFor<Shopper>();
            MongoConfiguration.RemoveMapFor<Cart>();
            MongoConfiguration.RemoveMapFor<TestProduct>();
            MongoConfiguration.RemoveTypeConverterFor<NonSerializableValueObject>();

            using (var admin = new MongoAdmin(TestHelper.ConnectionString()))
            {
                admin.DropDatabase();
            }
        }

        [Test]
        public void Mongo_Configuration_Should_AutoMap_Id_Property()
        {
            Assert.AreEqual("_id", MongoConfiguration.GetPropertyAlias(typeof(IdMap0), "_ID"));
            Assert.AreEqual("_id", MongoConfiguration.GetPropertyAlias(typeof(IdMap1), "TheID"));
            Assert.AreEqual("_id", MongoConfiguration.GetPropertyAlias(typeof(IdMap2), "ID"));
            Assert.AreEqual("_id", MongoConfiguration.GetPropertyAlias(typeof(IdMap3), "id"));
            Assert.AreEqual("_id", MongoConfiguration.GetPropertyAlias(typeof(IdMap4), "Id"));
        }

        [Test]
        public void Mongo_Configuration_Should_Notify_TypeHelper()
        {
            var typeHelper = ReflectionHelper.GetHelperForType(typeof(User2));
            Assert.AreEqual("LastName", typeHelper.FindProperty("LastName").Name);

            //the mapping should cause the typehelper cache to be rebuilt with the new properties.
            MongoConfiguration.Initialize(cfg => cfg.For<User2>(j => j.ForProperty(k => k.LastName).UseAlias("LNAME")));

            typeHelper = ReflectionHelper.GetHelperForType(typeof(User2));
            Assert.AreEqual("LastName", typeHelper.FindProperty("LNAME").Name);
        }

        [Test]
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

        [Test]
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
                Assert.AreEqual("Test", result.FirstName);
                Assert.AreEqual("User", result.LastName);
            }
        }

        [Test]
        public void Mongo_Configuration_Supports_Lambda_Syntax_Registration()
        {
            MongoConfiguration.Initialize(r => r.For<User>(u => u.ForProperty(user => user.FirstName).UseAlias("first")));
            var alias = MongoConfiguration.GetPropertyAlias(typeof(User), "FirstName");
            Assert.AreEqual("first", alias);
        }

        [Test]
        public void Mongo_Configuration_Can_Remove_Mapping()
        {
            MongoConfiguration.Initialize(r => r.For<User>(u => u.ForProperty(h => h.LastName).UseAlias("lName")));
            //confirm that mapping was set.
            Assert.AreEqual("lName", MongoConfiguration.GetPropertyAlias(typeof(User), "LastName"));

            MongoConfiguration.RemoveMapFor<User>();
            //confirm that mapping was unset.
            Assert.AreEqual("LastName", MongoConfiguration.GetPropertyAlias(typeof(User), "LastName"));
        }

        [Test]
        public void Mongo_Configuration_Remove_Mapping_Of_Norm_Types_Fails()
        {
            //removal of maps for Norm types is verboden.
            Assert.Throws<NotSupportedException>(() => MongoConfiguration.RemoveMapFor<IMongoDatabase>());
        }

        [Test]
        public void Mongo_Configuration_Echos_Unmapped_Property_Names()
        {
            MongoConfiguration.Initialize(r => r.For<User>(u => u.ForProperty(user => user.FirstName)
                .UseAlias("first"))/*.WithProfileNamed("Sample")*/);

            var first = MongoConfiguration.GetPropertyAlias(typeof(User), "FirstName");
            var last = MongoConfiguration.GetPropertyAlias(typeof(User), "LastName");

            Assert.AreEqual("first", first);
            Assert.AreEqual("LastName", last);
        }

        [Test]
        public void Mongo_Configuration_Returns_Null_For_Uninitialized_Type_Connection_Strings()
        {
            MongoConfiguration.Initialize(r => r.For<User>(u => u.ForProperty(user => user.FirstName).UseAlias("thisIsntDying")));

            var connection = MongoConfiguration.GetConnectionString(typeof(TestProduct));

            Assert.AreEqual(null, connection);
        }

        [Test]
        public void Mongo_Configuration_With_Linq_Supports_Aliases()
        {

            MongoConfiguration.Initialize(c => c.AddMap<ShopperMap>());
            using (var shoppers = new Shoppers(Mongo.Create(TestHelper.ConnectionString("pooling=false","test",null,null))))
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
                Assert.AreEqual(1, shallowResult.Count());

                var deepResult = shoppers.Where(x => x.Cart.Name == "Cart1").ToList();
                Assert.AreEqual(1, deepResult.Count());

                var deeperResult = shoppers.Where(x => x.Cart.Product.Name == "OtherProduct").ToList();
                Assert.AreEqual(1, deeperResult.Count());
            }
        }

        [Test]
        public void Are_Queries_Fully_Linqified()
        {
            MongoConfiguration.Initialize(c => c.AddMap<ShopperMap>());
            using (var shoppers = new Shoppers(Mongo.Create(TestHelper.ConnectionString("pooling=false","test",null,null))))
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

                var deepQuery = shoppers.Where(x => x.Cart.CartSuppliers.Any(y => y.Name == "Supplier1")).ToList();
                Assert.AreEqual("John", deepQuery[0].Name);
                Assert.AreEqual("Cart1", deepQuery[0].Cart.Name);
                Assert.AreEqual(1, deepQuery.Count);
            }
        }

        [Test]
        public void should_ignore_name_property_when_inserting__as_specified_in_mappings()
        {

            MongoConfiguration.Initialize(c => c.AddMap<ShopperMapWithIgnoreImmutableAndIgnoreIfNullConfigurationForProperties>());
            using (
                Shoppers shoppers =
                    new Shoppers(Mongo.Create(TestHelper.ConnectionString("pooling=false", "test", null, null))))
            {
                shoppers.Drop<Shopper>();
                shoppers.Add(new Shopper
                                 {
                                     Id = ObjectId.NewObjectId(),
                                     Name = "John",
                                 
                                 });

                shoppers.Add(new Shopper
                                 {
                                     Id = ObjectId.NewObjectId(),
                                     Name = "Jane",
                                   
                                 });

               
                var deepQuery = shoppers.ToList();

                Assert.IsNull(deepQuery[0].Name);
                Assert.IsNull(deepQuery[1].Name);

               
            }
        }

        [Test]
        public void Can_correctly_determine_collection_name()
        {
            var collectionName = MongoConfiguration.GetCollectionName(typeof(SuperClassObject));

            Assert.AreEqual("SuperClassObject", collectionName);
        }

        [Test]
        public void Can_correctly_determine_collection_name_from_discriminated_sub_class()
        {
            var collectionName = MongoConfiguration.GetCollectionName(typeof(SubClassedObject));

            Assert.AreEqual("SuperClassObject", collectionName);
        }

        [Test]
        public void Can_correctly_determine_collection_name_when_discriminator_is_on_an_interface()
        {
            var collectionName = MongoConfiguration.GetCollectionName(typeof(InterfaceDiscriminatedClass));

            Assert.AreEqual("IDiscriminated", collectionName);
        }

        [Test]
        public void Can_correctly_determine_collection_name_from_discriminated_sub_class_when_fluent_mapped()
        {
            var collectionName = MongoConfiguration.GetCollectionName(typeof(SubClassedObjectFluentMapped));

            Assert.AreEqual("SuperClassObjectFluentMapped", collectionName);

        }

        [Test]
        public void Subclass_Adheres_To_Superclass_Fluent_Alias()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var obj1 = new SubClassedObjectFluentMapped { Title = "Prod1", ABool = true };
                var obj2 = new SubClassedObjectFluentMapped { Title = "Prod2", ABool = false };

                mongo.GetCollection<SubClassedObjectFluentMapped>("Fake").Insert(obj1);
                mongo.GetCollection<SubClassedObjectFluentMapped>("Fake").Insert(obj2);
                var found = mongo.GetCollection<SuperClassObjectFluentMapped>("Fake").Find();

                Assert.AreEqual(2, found.Count());
                Assert.AreEqual(obj1.Id, found.ElementAt(0).Id);
                Assert.AreEqual("Prod1", found.ElementAt(0).Title);
                Assert.AreEqual(obj2.Id, found.ElementAt(1).Id);
                Assert.AreEqual("Prod2", found.ElementAt(1).Title);

            }
        }

        [Test]
        public void Subclassed_Type_Is_Returned_When_Superclass_Is_Used_For_The_Collection()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var obj1 = new SubClassedObjectFluentMapped { Title = "Prod1", ABool = true };
                mongo.GetCollection<SubClassedObjectFluentMapped>("Fake").Insert(obj1);
                var found = mongo.GetCollection<SuperClassObjectFluentMapped>("Fake").Find();

                Assert.AreEqual(1, found.Count());
                Assert.AreEqual(typeof(SubClassedObjectFluentMapped), found.ElementAt(0).GetType());
            }
        }
       
        [Test]
        public void Can_Register_TypeConverter()
        {
            MongoConfiguration.Initialize(c => c.TypeConverterFor<NonSerializableValueObject, NonSerializableValueObjectTypeConverter>());
            IBsonTypeConverter converter = MongoConfiguration.ConfigurationContainer.GetTypeConverterFor(typeof(NonSerializableValueObject));
            Assert.AreEqual(typeof(NonSerializableValueObjectTypeConverter), converter.GetType());
        }
    }
}