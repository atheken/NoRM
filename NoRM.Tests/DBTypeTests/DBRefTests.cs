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
        	const string databaseName = "NormTests";
			var id = ObjectId.NewObjectId();

			using (var session = new Session())
			{
				session.Add(new Product { _id = id, Name = "RefProduct" });

				var productReference = new DBReference<Product>
										   {
											   Collection = MongoConfiguration.GetCollectionName(typeof(Product)),
											   DatabaseName = databaseName,
											   ID = id,
										   };

				session.Add(new ProductReference
								{
									Id = ObjectId.NewObjectId(),
									Name = "FullCart",
									ProductsOrdered = new[] { productReference }
								});
			}

			var server = Mongo.ParseConnection("mongodb://localhost/" + databaseName);
            var reference = server.GetCollection<ProductReference>().Find().First();
			var product = reference.ProductsOrdered[0].Fetch(() => server);

            Assert.Equal(id.Value, product._id.Value);
        }
    }
}
