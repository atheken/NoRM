using System;
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
            using (var session = new Session())
            {
                var id = ObjectId.NewObjectId();
                session.Add(new Product { Id = id, Name = "DbRefProduct", Price = 10 });


                var dbRef = new DBReference
                                {
                                    Collection = MongoConfiguration.GetCollectionName(typeof(Product)),
                                    DatabaseName = "test",
                                    ID = id
                                };

                session.Add(new Order { Quantity = 10, ProductOrdered = dbRef });
            }
        }

        public class Order
        {
            public Order()
            {
                ID = ObjectId.NewObjectId();
            }

            public ObjectId ID{get;set;}
            public int Quantity { get; set; }
            public DBReference ProductOrdered { get; set; }
        }
    }
}
