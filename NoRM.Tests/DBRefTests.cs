using System;
using System.Linq;
using NoRM.BSON;
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
                session.Drop<Cart>();
            }
        }

        [Fact]
        public void DBRefMapsToOtherDocumentsByOid()
        {
            var id = ObjectId.NewObjectId();

            using (var session = new Session())
            {
                session.Add(new Product {Id = id, Name = "RefProduct"});

                var productReference = new DBReference
                                           {
                                               Collection = MongoConfiguration.GetCollectionName(typeof (Product)),
                                               DatabaseName = "test",
                                               ID = id,
                                           };

                session.Add(new Cart
                                {
                                    Id = ObjectId.NewObjectId(),
                                    Name = "FullCart",
                                    ProductsOrdered = new[] { productReference }
                                });
            }


            var server = Mongo.ParseConnection("mongodb://localhost/test");
            var cart = server.GetCollection<Cart>().Find().First();
            var product = cart.ProductsOrdered[0].Fetch(() => GetReferenceCollection());
                
            Assert.Equal(id.Value, product.Id.Value);
        }

        internal MongoCollection<Product> GetReferenceCollection()
        {
            var server = Mongo.ParseConnection("mongodb://localhost/test");
            return server.GetCollection<Product>();
        }

        internal class Cart
        {
            public Cart()
            {
                Id = ObjectId.NewObjectId();                
            }

            public string Name { get; set; }
            public ObjectId Id { get; set; }
            public DBReference[] ProductsOrdered { get; set; }
        }

        //public class Order
        //{
        //    public Order()
        //    {
        //        Id = ObjectId.NewObjectId();
        //    }

        //    public ObjectId Id{get;set;}
        //    public int Quantity { get; set; }
        //    public DBReference ProductOrdered { get; set; }
        //}
    }
}
