using System.Linq;
using NoRM.BSON.DbTypes;
using NoRM.Configuration;
using Xunit;


namespace NoRM.Tests
{
    public class DBRefTests
    {
        public DBRefTests()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Drop<ProductReference>();
            }
        }

        [Fact]
        public void DBRefMapsToOtherDocumentsByOid()
        {
            var id = ObjectId.NewObjectId();

            using (var session = new Session())
            {
                session.Add(new Product { Id = id, Name = "RefProduct" });

                var productReference = new DBReference
                                           {
                                               Collection = MongoConfiguration.GetCollectionName(typeof(Product)),
                                               DatabaseName = "test",
                                               ID = id,
                                           };

                session.Add(new ProductReference
                                {
                                    Id = ObjectId.NewObjectId(),
                                    Name = "FullCart",
                                    ProductsOrdered = new[] { productReference }
                                });
            }


            var server = Mongo.ParseConnection("mongodb://localhost/test");
            var reference = server.GetCollection<ProductReference>().Find().First();
            var product = reference.ProductsOrdered[0].Fetch(() => GetReferenceCollection());

            Assert.Equal(id.Value, product.Id.Value);
        }

        internal MongoCollection<Product> GetReferenceCollection()
        {
            var server = Mongo.ParseConnection("mongodb://localhost/test");
            return server.GetCollection<Product>();
        }

        public class ProductReference
        {
            public ProductReference()
            {
                Id = ObjectId.NewObjectId();
            }
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public DBReference[] ProductsOrdered { get; set; }
        }
    }
}
