using Xunit;
using System.Linq;
    
namespace NoRM.Tests
{

    public class MongoCollectionTests
    {
        public MongoCollectionTests()
        {
            using (var mongo = Mongo.ParseConnection(TestHelper.ConnectionString("strict=false")))
            {
                mongo.Database.DropCollection("Fake");
            }
        }
        [Fact]
        public void SaveOrInsertThrowsExceptionIfTypeDoesntHaveAnId()
        {
            using (var mongo = Mongo.ParseConnection(TestHelper.ConnectionString()))
            {
                var ex = Assert.Throws<MongoException>(() => mongo.GetCollection<Address>("Fake").Save(new Address()));
                Assert.Equal("This collection does not accept insertions/updates, this is due to the fact that the collection's type NoRM.Tests.Address does not specify an identifier property", ex.Message);
            }
        }

        [Fact]
        public void InsertsNewEntityWithNonObjectIdKey()
        {
            using (var mongo = Mongo.ParseConnection(TestHelper.ConnectionString()))
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
        public void UpdatesEntityWithNonObjectIdKey()
        {
            using (var mongo = Mongo.ParseConnection(TestHelper.ConnectionString()))
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
            using (var mongo = Mongo.ParseConnection(TestHelper.ConnectionString()))
            {
                var id1 = new ObjectId("123456123456123456123456");
                var id2 = new ObjectId("123456123456123456123457");
                mongo.GetCollection<Product>("Fake").Save(new Product { Id = id1, Name = "Prod1" });
                mongo.GetCollection<Product>("Fake").Save(new Product { Id = id2, Name = "Prod2" });
                var found = mongo.GetCollection<Product>("Fake").Find();
                Assert.Equal(2, found.Count());
                Assert.Equal(id1, found.ElementAt(0).Id);
                Assert.Equal("Prod1", found.ElementAt(0).Name);
                Assert.Equal(id2, found.ElementAt(1).Id);
                Assert.Equal("Prod2", found.ElementAt(1).Name);

            }
        }
        
        [Fact]
        public void UpdatesEntityWithObjectIdKey()
        {
            using (var mongo = Mongo.ParseConnection(TestHelper.ConnectionString()))
            {
                var id = new ObjectId("123456123456123456123456");
                mongo.GetCollection<Product>("Fake").Save(new Product { Id = id, Name = "Prod" });
                mongo.GetCollection<Product>("Fake").Save(new Product { Id = id, Name = "Updated Prod" });
                var found = mongo.GetCollection<Product>("Fake").Find();
                Assert.Equal(1, found.Count());
                Assert.Equal(id, found.ElementAt(0).Id);
                Assert.Equal("Updated Prod", found.ElementAt(0).Name);
            }
        }
        
        [Fact]
        public void SavingANewEntityWithObjectIdKeyGeneratesAKey()
        {
            using (var mongo = Mongo.ParseConnection(TestHelper.ConnectionString()))
            {
                var product = new Product {Id = null};
                mongo.GetCollection<Product>("Fake").Save(product);
                Assert.NotNull(product.Id);
                Assert.NotEqual(ObjectId.Empty, product.Id);
            }
        }
        [Fact]
        public void InsertingANewEntityWithObjectIdKeyGeneratesAKey()
        {
            using (var mongo = Mongo.ParseConnection(TestHelper.ConnectionString()))
            {
                var product = new Product { Id = null };
                mongo.GetCollection<Product>("Fake").Insert(product);
                Assert.NotNull(product.Id);
                Assert.NotEqual(ObjectId.Empty, product.Id);
            }
        }

        [Fact]
        public void InsertingMultipleNewEntityWithObjectIdKeyGeneratesAKey()
        {
            using (var mongo = Mongo.ParseConnection(TestHelper.ConnectionString()))
            {
                var product1 = new Product { Id = null };
                var product2 = new Product { Id = null };
                mongo.GetCollection<Product>("Fake").Insert(new[]{product1, product2});
                Assert.NotNull(product1.Id);
                Assert.NotEqual(ObjectId.Empty, product1.Id);
                Assert.NotNull(product2.Id);
                Assert.NotEqual(ObjectId.Empty, product2.Id);
            }
        }
        
        [Fact]
        public void DeletesObjectsBasedOnTemplate()
        {
            using (var mongo = Mongo.ParseConnection(TestHelper.ConnectionString()))
            {
                var collection = mongo.GetCollection<Product>("Fake");
                collection.Insert(new[] { new Product { Price = 10}, new Product { Price = 5}, new Product { Price = 1} });
                Assert.Equal(3, collection.Count());
                collection.Delete(new{Price = 1});
                Assert.Equal(2, collection.Count());
                Assert.Equal(0, collection.Count(new {Price = 1}));
            }
        }

        [Fact]
        public void ThrowsExceptionWhenAttemptingToDeleteIdLessEntity()
        {
            using (var mongo = Mongo.ParseConnection(TestHelper.ConnectionString()))
            {
                var ex = Assert.Throws<MongoException>(() => mongo.GetCollection<Address>("Fake").Delete(new Address()));
                Assert.Equal("Cannot delete NoRM.Tests.Address since it has no id property", ex.Message);
            }
        }
        [Fact]
        public void DeletesEntityBasedOnItsId()
        {
            using (var mongo = Mongo.ParseConnection(TestHelper.ConnectionString()))
            {
                var collection = mongo.GetCollection<Product>("Fake");
                var product1 = new Product();
                var product2 = new Product();
                collection.Insert(new[] { product1, product2 });
                Assert.Equal(2, collection.Count());
                collection.Delete(product1);
                Assert.Equal(1, collection.Count());
                Assert.Equal(1, collection.Count(new { Id = product2.Id }));
            }
        }
                
        private class IntId
        {
            public int Id{ get; set;}
            public string Name{ get; set;}
        }
    }
}