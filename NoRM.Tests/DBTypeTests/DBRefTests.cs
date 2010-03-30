using System.Linq;
using Norm.BSON.DbTypes;
using Norm.Configuration;
using Xunit;
using Norm.Collections;


namespace Norm.Tests
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
                session.Add(new Product { _id = id, Name = "RefProduct" });

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


			var server = Mongo.Create("mongodb://localhost/NormTests");
            var reference = server.GetCollection<ProductReference>().Find().First();
            var product = reference.ProductsOrdered[0].Fetch(() => GetReferenceCollection());

            Assert.Equal(id.Value, product._id.Value);
        }

        internal MongoCollection<Product> GetReferenceCollection()
        {
			var server = Mongo.Create("mongodb://localhost/NormTests");
            return server.GetCollection<Product>();
        }
    }
}
