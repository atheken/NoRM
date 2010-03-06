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
                session.Drop<Order>();
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
                                    Order = new Order{ ProductOrdered = productReference }
                                });
            }

            //var server = Mongo.ParseConnection("mongodb://localhost/test");
            //var collection = server.GetCollection<Cart>();

            // TODO: This should return a product
            //var query = new Flyweight(); 
            //query["$where"] = " function(){return this.Order.ProductOrdered.fetch();} ";

            // TODO: Not sure this makes sense.
            var product = new DBReference{ID = id}.FollowReference<Product>(Mongo.ParseConnection("mongodb://localhost/test"));
            Assert.Equal(id, product.Id);
        }

        internal class Cart
        {
            public Cart()
            {
                Id = ObjectId.NewObjectId();                
            }

            public string Name { get; set; }
            public ObjectId Id { get; set; }
            public Order Order { get; set; }
        }

        public class Order
        {
            public Order()
            {
                Id = ObjectId.NewObjectId();
            }

            public ObjectId Id{get;set;}
            public int Quantity { get; set; }
            public DBReference ProductOrdered { get; set; }
        }
    }
}
