

using System.Linq;
using NoRM.Linq;
using Xunit;

namespace NoRM.Tests
{
    public class MongoTypedSessionTests
    {
        [Fact]
        public void TypedSessionSupportsIQueryableCollections()
        {
            using (var session = new MongoTypedSession<Product>("mongodb://localhost/NoRMTests?pooling=false&strict=false"))
            {
                session.Drop<Product>();
                session.Add(new Product { Name = "Test2X", Price = 10 });
                session.Add(new Product { Name = "Test3", Price = 33 });
                session.Add(new Product { Name = "XTest4", Price = 22 });

                var products = session.Query.Where(x => x.Name.StartsWith("X") || x.Name.EndsWith("X")).ToList();
                Assert.Equal(2, products.Count);
            }
        }
        [Fact]
        public void TypedSessionQueriesInnerListTypes()
        {
            using (var session = new MongoTypedSession<Product>("mongodb://localhost/NoRMTests?pooling=false&strict=false"))
            {
                session.Drop<Product>();
                session.Drop<NotAProduct>();
                session.Add(new NotAProduct { Name = "Test1X" });
                session.Add(new Product { Name = "Test2X", Price = 10 });
                session.Add(new Product { Name = "Test3", Price = 33 });
                session.Add(new Product { Name = "XTest4", Price = 22 });

                var products = session.Query.Where(x => x.Name.StartsWith("X") || x.Name.EndsWith("X")).ToList();
                Assert.Equal(2, products.Count);
            }
        }
    }

    public class NotAProduct 
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
    }
}
