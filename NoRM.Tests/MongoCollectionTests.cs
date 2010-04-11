using Xunit;
using System.Linq;
using Norm.Configuration;
using System.Collections.Generic;
using System;
using Norm.Protocol.Messages;

namespace Norm.Tests
{
    public class MongoCollectionTests
    {

        public MongoCollectionTests()
        {
            MongoConfiguration.RemoveMapFor<Address>();
            MongoConfiguration.RemoveMapFor<Product>();
            MongoConfiguration.RemoveMapFor<IntId>();

            using (var mongo = Mongo.Create(TestHelper.ConnectionString("strict=false")))
            {
                mongo.Database.DropCollection("Fake");
            }
        }

        [Fact]
        public void Find_On_Collection_Returning_More_Than_4MB_Of_Docs_Works()
        {
            //this tests Cursor management in the ReplyMessage<T>, 
            //we built NoRM so that the average user picking up the library
            //doesn't have to think about this.
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                List<Product> junkInTheTrunk = new List<Product>();
                for (int i = 0; i < 16000; i++)
                {
                    #region Initialize and add a product to the batch.
                    junkInTheTrunk.Add(new Product()
                                {
                                    Available = DateTime.Now,
                                    Inventory = new List<InventoryChange> { 
                            new InventoryChange{ 
                               AmountChanged=5, CreatedOn=DateTime.Now
                            }
                    },
                                    Name = "Pogo Stick",
                                    Price = 42.0,
                                    Supplier = new Supplier()
                                    {
                                        Address = new Address
                                        {
                                            Zip = "27701",
                                            City = "Durham",
                                            State = "NC",
                                            Street = "Morgan St."
                                        },
                                        CreatedOn = DateTime.Now,
                                        Name = "ACME"
                                    }
                                });
                    #endregion
                }
                var bytes = junkInTheTrunk.SelectMany(y => Norm.BSON.BsonSerializer.Serialize(y)).Count();

                Assert.InRange(bytes, 4194304, Int32.MaxValue);
                mongo.GetCollection<Product>("Fake").Insert(junkInTheTrunk);
                Assert.Equal(16000, mongo.GetCollection<Product>("Fake").Find().Count());
            }
        }

        [Fact]
        public void SaveOrInsertThrowsExceptionIfTypeDoesntHaveAnId()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var ex = Assert.Throws<MongoException>(() => mongo.GetCollection<Address>("Fake").Save(new Address()));
                Assert.Equal("This collection does not accept insertions/updates, this is due to the fact that the collection's type Norm.Tests.Address does not specify an identifier property", ex.Message);
            }
        }

        [Fact]
        public void InsertsNewEntityWithNonObjectIdKey()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                mongo.GetCollection<IntId>("Fake").Save(new IntId { Id = 4, Name = "Test 1" });
                mongo.GetCollection<IntId>("Fake").Save(new IntId { Id = 5, Name = "Test 2" });
                var found = mongo.GetCollection<IntId>("Fake").Find();
                Assert.Equal(2, found.Count());
                Assert.Equal(4, found.ElementAt(0).Id);
                Assert.Equal("Test 1", found.ElementAt(0).Name);
                Assert.Equal(5, found.ElementAt(1).Id);
                Assert.Equal("Test 2", found.ElementAt(1).Name);

            }
        }

        [Fact]
        public void InsertThrowsExcpetionOnDuplicateKeyAndStrictMode()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("strict=true")))
            {
                mongo.GetCollection<IntId>("Fake").Insert(new IntId { Id = 4, Name = "Test 1" });
                var ex = Assert.Throws<MongoException>(() => mongo.GetCollection<IntId>("Fake").Insert(new IntId { Id = 4, Name = "Test 2" }));


            }
        }

        [Fact]
        public void MongoCollectionEnsuresDeleteIndices()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product
                {
                    Name = "ExplainProduct",
                    Price = 10,
                    Supplier = new Supplier { Name = "Supplier", CreatedOn = DateTime.Now }
                });
                session.Provider.DB.GetCollection<Product>().CreateIndex(p => p.Supplier.Name, "TestIndex", true, IndexOption.Ascending);

                int i;
                session.Provider.DB.GetCollection<Product>().DeleteIndices(out i);

                //it's TWO because there's always an index on _id by default.
                Assert.Equal(2, i);

            }
        }


        [Fact]
        public void MongoCollectionEnsuresDeletIndexByName()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product
                {
                    Name = "ExplainProduct",
                    Price = 10,
                    Supplier = new Supplier { Name = "Supplier", CreatedOn = DateTime.Now }
                });
                session.Provider.DB.GetCollection<Product>().CreateIndex(p => p.Supplier.Name, "TestIndex", true, IndexOption.Ascending);
                session.Provider.DB.GetCollection<Product>().CreateIndex(p => p.Available, "TestIndex1", false, IndexOption.Ascending);
                session.Provider.DB.GetCollection<Product>().CreateIndex(p => p.Name, "TestIndex2", false, IndexOption.Ascending);

                int i, j;
                session.Provider.DB.GetCollection<Product>().DeleteIndex("TestIndex1", out i);
                session.Provider.DB.GetCollection<Product>().DeleteIndex("TestIndex2", out j);

                Assert.Equal(4, i);
                Assert.Equal(3, j);
            }
        }

        [Fact]
        public void UpdatesEntityWithNonObjectIdKey()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                mongo.GetCollection<IntId>("Fake").Save(new IntId { Id = 4, Name = "Test" });
                mongo.GetCollection<IntId>("Fake").Save(new IntId { Id = 4, Name = "Updated" });
                var found = mongo.GetCollection<IntId>("Fake").Find();
                Assert.Equal(1, found.Count());
                Assert.Equal(4, found.ElementAt(0).Id);
                Assert.Equal("Updated", found.ElementAt(0).Name);
            }
        }

        [Fact]
        public void InsertsNewEntityWithObjectIdKey()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var id1 = new ObjectId("123456123456123456123456");
                var id2 = new ObjectId("123456123456123456123457");
                mongo.GetCollection<Product>("Fake").Save(new Product { _id = id1, Name = "Prod1" });
                mongo.GetCollection<Product>("Fake").Save(new Product { _id = id2, Name = "Prod2" });
                var found = mongo.GetCollection<Product>("Fake").Find();
                Assert.Equal(2, found.Count());
                Assert.Equal(id1, found.ElementAt(0)._id);
                Assert.Equal("Prod1", found.ElementAt(0).Name);
                Assert.Equal(id2, found.ElementAt(1)._id);
                Assert.Equal("Prod2", found.ElementAt(1).Name);

            }
        }

        [Fact]
        public void UpdatesEntityWithObjectIdKey()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var id = new ObjectId("123456123456123456123456");
                mongo.GetCollection<Product>("Fake").Save(new Product { _id = id, Name = "Prod" });
                mongo.GetCollection<Product>("Fake").Save(new Product { _id = id, Name = "Updated Prod" });
                var found = mongo.GetCollection<Product>("Fake").Find();
                Assert.Equal(1, found.Count());
                Assert.Equal(id, found.ElementAt(0)._id);
                Assert.Equal("Updated Prod", found.ElementAt(0).Name);
            }
        }

        [Fact]
        public void SavingANewEntityWithObjectIdKeyGeneratesAKey()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var product = new Product { _id = null };
                mongo.GetCollection<Product>("Fake").Save(product);
                Assert.NotNull(product._id);
                Assert.NotEqual(ObjectId.Empty, product._id);
            }
        }
        [Fact]
        public void InsertingANewEntityWithObjectIdKeyGeneratesAKey()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var product = new Product { _id = null };
                mongo.GetCollection<Product>("Fake").Insert(product);
                Assert.NotNull(product._id);
                Assert.NotEqual(ObjectId.Empty, product._id);
            }
        }

        [Fact]
        public void InsertingMultipleNewEntityWithObjectIdKeyGeneratesAKey()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var product1 = new Product { _id = null };
                var product2 = new Product { _id = null };
                mongo.GetCollection<Product>("Fake").Insert(new[] { product1, product2 });
                Assert.NotNull(product1._id);
                Assert.NotEqual(ObjectId.Empty, product1._id);
                Assert.NotNull(product2._id);
                Assert.NotEqual(ObjectId.Empty, product2._id);
            }
        }

        [Fact]
        public void DeletesObjectsBasedOnTemplate()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var collection = mongo.GetCollection<Product>("Fake");
                collection.Insert(new[] { new Product { Price = 10 }, new Product { Price = 5 }, new Product { Price = 1 } });
                Assert.Equal(3, collection.Count());
                collection.Delete(new { Price = 1 });
                Assert.Equal(2, collection.Count());
                Assert.Equal(0, collection.Count(new { Price = 1 }));
            }
        }

        [Fact]
        public void ThrowsExceptionWhenAttemptingToDeleteIdLessEntity()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var ex = Assert.Throws<MongoException>(() => mongo.GetCollection<Address>("Fake").Delete(new Address()));
                Assert.Equal("Cannot delete Norm.Tests.Address since it has no id property", ex.Message);
            }
        }
        [Fact]
        public void DeletesEntityBasedOnItsId()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var collection = mongo.GetCollection<Product>("Fake");
                var product1 = new Product();
                var product2 = new Product();
                collection.Insert(new[] { product1, product2 });
                Assert.Equal(2, collection.Count());
                collection.Delete(product1);
                Assert.Equal(1, collection.Count());
                Assert.Equal(1, collection.Count(new { Id = product2._id }));
            }
        }

        [Fact]
        public void MapReduceIsSuccessful()
        {
            var _map = "function(){emit(0, this.Price);}";
            var _reduce = "function(key, values){var sumPrice = 0;for(var i = 0; i < values.length; ++i){sumPrice += values[i];} return sumPrice;}";

            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false")))
            {
                if (mongo.Database.GetAllCollections().Any(y => y.Name.EndsWith("ReduceProduct")))
                {
                    mongo.Database.DropCollection("ReduceProduct");
                }
                var collection = mongo.GetCollection<ReduceProduct>();
                collection.Insert(new ReduceProduct { Price = 1.5f }, new ReduceProduct { Price = 2.5f });
                var r = collection.MapReduce<ProductSum>(_map, _reduce).FirstOrDefault();
                Assert.Equal(0, r.Id);
                Assert.Equal(4, r.Value);
            }


        }


        private class IntId
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}