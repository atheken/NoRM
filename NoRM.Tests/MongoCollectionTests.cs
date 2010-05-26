using Xunit;
using System.Linq;
using Norm.Configuration;
using System.Collections.Generic;
using System;
using Norm.Protocol.Messages;
using Norm.BSON;

namespace Norm.Tests
{
    public class MongoCollectionTests
    {

        public MongoCollectionTests()
        {
            MongoConfiguration.RemoveMapFor<Address>();
            MongoConfiguration.RemoveMapFor<TestProduct>();
            MongoConfiguration.RemoveMapFor<IntId>();

            using (var mongo = Mongo.Create(TestHelper.ConnectionString("strict=false")))
            {
                mongo.Database.DropCollection("Fake");
            }
        }

        [Fact]
        public void Find_On_Unspecified_Type_Returns_Expando_When_No_Discriminator_Available()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                db.Database.GetCollection("helloWorld").Insert(new { _id = 1 });
                db.Database.DropCollection("helloWorld");
                var coll = db.Database.GetCollection("helloWorld");
                coll.Insert(new IntId { Id = 5, Name = "hi there" },
                    new { Id = Guid.NewGuid(), Value = "22" },
                    new { _id = ObjectId.NewObjectId(), Key = 578 });

                var allObjs = coll.Find().ToArray();
                Assert.True(allObjs.All(y => y is Expando));
                Assert.Equal(3, allObjs.Length);
            }
        }

        [Fact(Skip="This times out, but I don't know why..")]
        public void Get_Collection_Statistics_Works()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("timeout=3")))
            {
                var coll = mongo.GetCollection<IntId>("Fake");
                coll.Insert(new IntId { Id = 4, Name = "Test 1" });
                coll.Insert(new IntId { Id = 5, Name = "Test 2" });
                var stats = coll.GetCollectionStatistics();
                Assert.NotNull(stats);
                Assert.Equal(stats.Count, 2);
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
                List<TestProduct> junkInTheTrunk = new List<TestProduct>();
                for (int i = 0; i < 16000; i++)
                {
                    #region Initialize and add a product to the batch.
                    junkInTheTrunk.Add(new TestProduct()
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
                mongo.GetCollection<TestProduct>("Fake").Insert(junkInTheTrunk);
                Assert.Equal(16000, mongo.GetCollection<TestProduct>("Fake").Find().Count());
            }
        }

        [Fact]
        public void SaveOrInsertThrowsExceptionIfTypeDoesntHaveAnId()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var ex = Assert.Throws<MongoException>(() => mongo.GetCollection<Address>("Fake").Insert(new Address()));
                Assert.Equal("This collection does not accept insertions/updates, this is due to the fact that the collection's type Norm.Tests.Address does not specify an identifier property", ex.Message);
            }
        }

        [Fact]
        public void InsertsNewEntityWithNonObjectIdKey()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                mongo.GetCollection<IntId>("Fake").Insert(new IntId { Id = 4, Name = "Test 1" });
                mongo.GetCollection<IntId>("Fake").Insert(new IntId { Id = 5, Name = "Test 2" });
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
                session.Drop<TestProduct>();
                session.Add(new TestProduct
                {
                    Name = "ExplainProduct",
                    Price = 10,
                    Supplier = new Supplier { Name = "Supplier", CreatedOn = DateTime.Now }
                });
                session.Provider.DB.GetCollection<TestProduct>().CreateIndex(p => p.Supplier.Name, "TestIndex", true, IndexOption.Ascending);

                int i;
                session.Provider.DB.GetCollection<TestProduct>().DeleteIndices(out i);

                //it's TWO because there's always an index on _id by default.
                Assert.Equal(2, i);

            }
        }


        [Fact]
        public void MongoCollectionEnsuresDeletIndexByName()
        {
            using (var session = new Session())
            {
                session.Drop<TestProduct>();
                session.Add(new TestProduct
                {
                    Name = "ExplainProduct",
                    Price = 10,
                    Supplier = new Supplier { Name = "Supplier", CreatedOn = DateTime.Now }
                });
                session.Provider.DB.GetCollection<TestProduct>().CreateIndex(p => p.Supplier.Name, "TestIndex", true, IndexOption.Ascending);
                session.Provider.DB.GetCollection<TestProduct>().CreateIndex(p => p.Available, "TestIndex1", false, IndexOption.Ascending);
                session.Provider.DB.GetCollection<TestProduct>().CreateIndex(p => p.Name, "TestIndex2", false, IndexOption.Ascending);

                int i, j;
                session.Provider.DB.GetCollection<TestProduct>().DeleteIndex("TestIndex1", out i);
                session.Provider.DB.GetCollection<TestProduct>().DeleteIndex("TestIndex2", out j);

                Assert.Equal(4, i);
                Assert.Equal(3, j);
            }
        }

        [Fact]
        public void UpdatesEntityWithNonObjectIdKey()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                mongo.GetCollection<IntId>("Fake").Insert(new IntId { Id = 4, Name = "Test" });
                mongo.GetCollection<IntId>("Fake").Update(new { Id = 4 }, new { Name = "Updated" }, false, false);
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
                var id1 = ObjectId.NewObjectId();
                var id2 = ObjectId.NewObjectId();
                mongo.GetCollection<TestProduct>("Fake").Insert(new TestProduct { _id = id1, Name = "Prod1" });
                mongo.GetCollection<TestProduct>("Fake").Insert(new TestProduct { _id = id2, Name = "Prod2" });
                var found = mongo.GetCollection<TestProduct>("Fake").Find();
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
                var id = ObjectId.NewObjectId();
                mongo.GetCollection<TestProduct>("Fake").Insert(new TestProduct { _id = id, Name = "Prod" });
                mongo.GetCollection<TestProduct>("Fake").Update(new { _id = id }, new { Name = "Updated Prod" }, false, false);
                var found = mongo.GetCollection<TestProduct>("Fake").Find();
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
                var product = new TestProduct { _id = null };
                mongo.GetCollection<TestProduct>("Fake").Insert(product);
                Assert.NotNull(product._id);
                Assert.NotEqual(ObjectId.Empty, product._id);
            }
        }
        [Fact]
        public void InsertingANewEntityWithObjectIdKeyGeneratesAKey()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var product = new TestProduct { _id = null };
                mongo.GetCollection<TestProduct>("Fake").Insert(product);
                Assert.NotNull(product._id);
                Assert.NotEqual(ObjectId.Empty, product._id);
            }
        }

        [Fact]
        public void InsertingMultipleNewEntityWithObjectIdKeyGeneratesAKey()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var product1 = new TestProduct { _id = null };
                var product2 = new TestProduct { _id = null };
                mongo.GetCollection<TestProduct>("Fake").Insert(new[] { product1, product2 });
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
                var collection = mongo.GetCollection<TestProduct>("Fake");
                collection.Insert(new[] { new TestProduct { Price = 10 }, new TestProduct { Price = 5 }, new TestProduct { Price = 1 } });
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
                var collection = mongo.GetCollection<TestProduct>("Fake");
                var product1 = new TestProduct();
                var product2 = new TestProduct();
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

            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false&strict=false")))
            {
                mongo.Database.DropCollection("ReduceProduct");
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