namespace NoRM.Tests
{
    using System;
    using System.Linq;
    using BSON.DbTypes;
    using Linq;
    using Xunit;
    
    internal class Session : MongoSession
    {
        public Session() : base("mongodb://localhost/Northwind?pooling=false"){}
        
        public IQueryable<Product> Products
        {
            get { return new MongoQuery<Product>(Provider); }
        }

        public void Add<T>(T item) where T : class, new()
        {
            Provider.Mongo.GetCollection<T>().Insert(item);         
        }

        public void Update<T>(T item) where T : class, new()
        {
            Provider.Mongo.GetCollection<T>().UpdateOne(item, item);         
        }

        public void Drop<T>()
        {
            Provider.Mongo.Database.DropCollection(typeof(T).Name);            
        }
        
        public void CreateCappedCollection(string name)
        {
            Provider.Mongo.Database.CreateCollection(new CreateCollectionOptions(name));
        }
    }

    internal class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }

    internal class Supplier
    {
        public Supplier()
        {
            Address = new Address();
            CreatedOn = DateTime.Now;
        }

        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public Address Address { get; set; }
    }

    internal class Product
    {
        public Product()
        {
            Supplier = new Supplier();
            _id = OID.NewOID();
        }

        
        public object _id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public Supplier Supplier { get; set; }
        public DateTime Available { get; set; }
    }

    public class LinqTests
    {
        [Fact]
        public void FourProductsShouldBeReturnedWhenStartsOrEndsWithX()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3X", Price = 10});
                session.Add(new Product {Name = "Test4X", Price = 22});
                session.Add(new Product {Name = "Test5", Price = 33});
                session.Add(new Product {Name = "XTest3", Price = 10});
                session.Add(new Product {Name = "XTest4", Price = 22});
                var products = session.Products.Where(x => x.Name.StartsWith("X") || x.Name.EndsWith("X")).ToList();
                Assert.Equal(4, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceGreaterThan10LessThan30()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3", Price = 10});
                session.Add(new Product {Name = "Test4", Price = 22});
                session.Add(new Product {Name = "Test5", Price = 33});
                var products = session.Products.Where(x => x.Price > 10 && x.Price < 30).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableDayIsFifth()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5)});
                session.Add(new Product {Name = "Test4X", Price = 22, Available = new DateTime(2000, 2, 6)});
                var products = session.Products.Where(x => x.Available.Day == 5).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableDayIsMonday()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3X", Price = 10, Available = DateTime.Now});
                session.Add(new Product {Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(1)});
                session.Add(new Product {Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(2)});
                session.Add(new Product {Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(3)});
                session.Add(new Product {Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(4)});
                session.Add(new Product {Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(5)});
                session.Add(new Product {Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(6)});
                var products = session.Products.Where(x => x.Available.DayOfWeek == DayOfWeek.Monday).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableGreaterThanToday()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1)});
                session.Add(new Product {Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1)});
                session.Add(new Product {Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1)});
                session.Add(new Product {Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(1)});
                var products = session.Products.Where(x => x.Available > DateTime.Now).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableLessThanTodayPlus1()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1)});
                session.Add(new Product {Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1)});
                session.Add(new Product {Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1)});
                session.Add(new Product {Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(2)});
                var products = session.Products.Where(x => x.Available > DateTime.Now.AddDays(1)).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableMonthIs2()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5)});
                session.Add(new Product {Name = "Test4X", Price = 22, Available = new DateTime(2001, 3, 6)});
                var products = session.Products.Where(x => x.Available.Month == 2).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableYearIs2000()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5)});
                session.Add(new Product {Name = "Test4X", Price = 22, Available = new DateTime(2001, 2, 6)});
                var products = session.Products.Where(x => x.Available.Year == 2000).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenIndexOfXEqual2()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3X", Price = 10});
                session.Add(new Product {Name = "TestX4", Price = 22});
                session.Add(new Product {Name = "TesXt5", Price = 33});
                session.Add(new Product {Name = "TeXst3", Price = 10});
                session.Add(new Product {Name = "TXest4", Price = 22});
                var products = session.Products.Where(x => x.Name.IndexOf("X") == 2).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenIsNullOrEmptyWithEmptyString()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3X", Price = 10});
                session.Add(new Product {Name = "Test4X", Price = 22});
                session.Add(new Product {Name = "", Price = 33});
                session.Add(new Product {Name = "XTest3", Price = 10});
                session.Add(new Product {Name = "XTest4", Price = 22});
                var products = session.Products.Where(x => string.IsNullOrEmpty(x.Name)).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenIsNullOrEmptyWithNull()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3X", Price = 10});
                session.Add(new Product {Name = "Test4X", Price = 22});
                session.Add(new Product {Price = 33});
                session.Add(new Product {Name = "XTest3", Price = 10});
                session.Add(new Product {Name = "XTest4", Price = 22});
                var products = session.Products.Where(x => string.IsNullOrEmpty(x.Name)).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenStartsAndEndsWithX()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3X", Price = 10});
                session.Add(new Product {Name = "Test4X", Price = 22});
                session.Add(new Product {Name = "XTest5X", Price = 33});
                session.Add(new Product {Name = "XTest3", Price = 10});
                session.Add(new Product {Name = "XTest4", Price = 22});
                var products = session.Products.Where(x => x.Name.StartsWith("X") && x.Name.EndsWith("X")).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhen3InDbWithNoExpression()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test1", Price = 10});
                session.Add(new Product {Name = "Test2", Price = 22});
                session.Add(new Product {Name = "Test3", Price = 33});
                var products = session.Products.ToList();
                Assert.Equal(3, products.Count);
            }
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhenAvailableLessThanToday()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1)});
                session.Add(new Product {Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1)});
                session.Add(new Product {Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1)});
                session.Add(new Product {Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(1)});
                var products = session.Products.Where(x => x.Available < DateTime.Now).ToList();
                Assert.Equal(3, products.Count);
            }
        }

        [Fact]
        public void OneProductShouldBeReturnedWhen3InDbWithFirst()
        {
            using(var session = new Session())
            {
                session.Drop<Product>();
                session.CreateCappedCollection("Product"); //only capped collections return in insertion order
                session.Add(new Product {Name = "Test1", Price = 10});
                session.Add(new Product {Name = "Test2", Price = 22});
                session.Add(new Product {Name = "Test3", Price = 33});
                var result = session.Products.First();
                Assert.Equal("Test1", result.Name);
            }
        }
        
        [Fact]
        public void OneProductShouldBeReturnedWhen3InDbWithSingle()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test1", Price = 10});
                session.Add(new Product {Name = "Test2", Price = 22});
                session.Add(new Product {Name = "Test3", Price = 33});
                var result = session.Products.SingleOrDefault(x => x.Price == 22);
                Assert.Equal(22, result.Price);
            }
        }
        
        [Fact]
        public void TwoProductsShouldBeReturnedWhen3InDbWithPriceGreaterThan10()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3", Price = 10});
                session.Add(new Product {Name = "Test4", Price = 22});
                session.Add(new Product {Name = "Test5", Price = 33});
                var products = session.Products.Where(x => x.Price > 10).ToList();
                Assert.Equal(2, products.Count);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhen3InDbWithPriceLessThan10OrGreaterThan30()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3", Price = 10});
                session.Add(new Product {Name = "Test4", Price = 22});
                session.Add(new Product {Name = "Test5", Price = 33});
                var products = session.Products.Where(x => x.Price > 10 || x.Price > 30).ToList();
                Assert.Equal(2, products.Count);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhenContainsX()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "TestX3", Price = 10});
                session.Add(new Product {Name = "TestX4", Price = 22});
                session.Add(new Product {Name = "Test5", Price = 33});
                var products = session.Products.Where(x => x.Name.Contains("X")).ToList();
                Assert.Equal(2, products.Count);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhenEndsWithX()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "Test3X", Price = 10});
                session.Add(new Product {Name = "Test4X", Price = 22});
                session.Add(new Product {Name = "Test5", Price = 33});
                var products = session.Products.Where(x => x.Name.EndsWith("X")).ToList();
                Assert.Equal(2, products.Count);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhenStartsWithX()
        {
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Add(new Product {Name = "XTest3", Price = 10});
                session.Add(new Product {Name = "XTest4", Price = 22});
                session.Add(new Product {Name = "Test5", Price = 33});
                var products = session.Products.Where(x => x.Name.StartsWith("X")).ToList();
                Assert.Equal(2, products.Count);
            }
        }
    }
}