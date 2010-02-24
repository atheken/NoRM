namespace NoRM.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BSON.DbTypes;
    using Linq;
    using Xunit;
    
    internal class Session
    {
        private readonly MongoQueryProvider _provider;

        public Session()
        {
            _provider = new MongoQueryProvider("mongodb://localhost/Northwind?pooling=false");
        }

        public IQueryable<Product> Products
        {
            get { return new MongoQuery<Product>(_provider); }
        }

        public void Add<T>(T item) where T : class, new()
        {
            var coll = _provider.DB.GetCollection<T>();            
            coll.Insert(item);
        }

        public void Update<T>(T item) where T : class, new()
        {
            var coll = _provider.DB.GetCollection<T>();
            coll.UpdateOne(item, item);
        }

        public void Drop<T>()
        {
            _provider.DB.DropCollection(typeof (T).Name);
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
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "Test3X", Price = 10});
            session.Add(new Product {Name = "Test4X", Price = 22});
            session.Add(new Product {Name = "Test5", Price = 33});
            session.Add(new Product {Name = "XTest3", Price = 10});
            session.Add(new Product {Name = "XTest4", Price = 22});

            var products = session.Products.Where(x => x.Name.StartsWith("X") || x.Name.EndsWith("X")).ToList();
            Assert.Equal(4, products.Count);
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceGreaterThan10LessThan30()
        {
            var session = new Session();
            session.Drop<Product>();
            session.Add(new Product {Name = "Test3", Price = 10});
            session.Add(new Product {Name = "Test4", Price = 22});
            session.Add(new Product {Name = "Test5", Price = 33});

            List<Product> products = session.Products.Where(x => x.Price > 10 && x.Price < 30).ToList();

            Assert.Equal(1, products.Count);
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableDayIsFifth()
        {
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5)});
            session.Add(new Product {Name = "Test4X", Price = 22, Available = new DateTime(2000, 2, 6)});

            var products = session.Products.Where(x => x.Available.Day == 5).ToList();
            Assert.Equal(1, products.Count);
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableDayIsMonday()
        {
            var session = new Session();
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

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableGreaterThanToday()
        {
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1)});
            session.Add(new Product {Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1)});
            session.Add(new Product {Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1)});
            session.Add(new Product {Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(1)});

            var products = session.Products.Where(x => x.Available > DateTime.Now).ToList();
            Assert.Equal(1, products.Count);
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableLessThanTodayPlus1()
        {
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1)});
            session.Add(new Product {Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1)});
            session.Add(new Product {Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1)});
            session.Add(new Product {Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(2)});

            var products = session.Products.Where(x => x.Available > DateTime.Now.AddDays(1)).ToList();
            Assert.Equal(1, products.Count);
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableMonthIs2()
        {
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5)});
            session.Add(new Product {Name = "Test4X", Price = 22, Available = new DateTime(2001, 3, 6)});

            var products = session.Products.Where(x => x.Available.Month == 2).ToList();
            Assert.Equal(1, products.Count);
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableYearIs2000()
        {
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5)});
            session.Add(new Product {Name = "Test4X", Price = 22, Available = new DateTime(2001, 2, 6)});

            var products = session.Products.Where(x => x.Available.Year == 2000).ToList();
            Assert.Equal(1, products.Count);
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenIndexOfXEqual2()
        {
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "Test3X", Price = 10});
            session.Add(new Product {Name = "TestX4", Price = 22});
            session.Add(new Product {Name = "TesXt5", Price = 33});
            session.Add(new Product {Name = "TeXst3", Price = 10});
            session.Add(new Product {Name = "TXest4", Price = 22});

            var products = session.Products.Where(x => x.Name.IndexOf("X") == 2).ToList();
            Assert.Equal(1, products.Count);
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenIsNullOrEmptyWithEmptyString()
        {
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "Test3X", Price = 10});
            session.Add(new Product {Name = "Test4X", Price = 22});
            session.Add(new Product {Name = "", Price = 33});
            session.Add(new Product {Name = "XTest3", Price = 10});
            session.Add(new Product {Name = "XTest4", Price = 22});

            var products = session.Products.Where(x => string.IsNullOrEmpty(x.Name)).ToList();
            Assert.Equal(1, products.Count);
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenIsNullOrEmptyWithNull()
        {
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "Test3X", Price = 10});
            session.Add(new Product {Name = "Test4X", Price = 22});
            session.Add(new Product {Price = 33});
            session.Add(new Product {Name = "XTest3", Price = 10});
            session.Add(new Product {Name = "XTest4", Price = 22});

            var products = session.Products.Where(x => string.IsNullOrEmpty(x.Name)).ToList();
            Assert.Equal(1, products.Count);
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenStartsAndEndsWithX()
        {
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "Test3X", Price = 10});
            session.Add(new Product {Name = "Test4X", Price = 22});
            session.Add(new Product {Name = "XTest5X", Price = 33});
            session.Add(new Product {Name = "XTest3", Price = 10});
            session.Add(new Product {Name = "XTest4", Price = 22});

            var products = session.Products.Where(x => x.Name.StartsWith("X") && x.Name.EndsWith("X")).ToList();
            Assert.Equal(1, products.Count);
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhen3InDbWithNoExpression()
        {
            var session = new Session();
            session.Drop<Product>();
            session.Add(new Product {Name = "Test1", Price = 10});
            session.Add(new Product {Name = "Test2", Price = 22});
            session.Add(new Product {Name = "Test3", Price = 33});

            var products = session.Products.ToList();
            Assert.Equal(3, products.Count);
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhenAvailableLessThanToday()
        {
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1)});
            session.Add(new Product {Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1)});
            session.Add(new Product {Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1)});
            session.Add(new Product {Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(1)});

            var products = session.Products.Where(x => x.Available < DateTime.Now).ToList();
            Assert.Equal(3, products.Count);
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhen3InDbWithPriceGreaterThan10()
        {
            var session = new Session();
            session.Drop<Product>();
            session.Add(new Product {Name = "Test3", Price = 10});
            session.Add(new Product {Name = "Test4", Price = 22});
            session.Add(new Product {Name = "Test5", Price = 33});

            var products = session.Products.Where(x => x.Price > 10).ToList();
            Assert.Equal(2, products.Count);
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhen3InDbWithPriceLessThan10OrGreaterThan30()
        {
            var session = new Session();
            session.Drop<Product>();
            session.Add(new Product {Name = "Test3", Price = 10});
            session.Add(new Product {Name = "Test4", Price = 22});
            session.Add(new Product {Name = "Test5", Price = 33});

            var products = session.Products.Where(x => x.Price > 10 || x.Price > 30).ToList();
            Assert.Equal(2, products.Count);
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhenContainsX()
        {
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "TestX3", Price = 10});
            session.Add(new Product {Name = "TestX4", Price = 22});
            session.Add(new Product {Name = "Test5", Price = 33});

            var products = session.Products.Where(x => x.Name.Contains("X")).ToList();
            Assert.Equal(2, products.Count);
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhenEndsWithX()
        {
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "Test3X", Price = 10});
            session.Add(new Product {Name = "Test4X", Price = 22});
            session.Add(new Product {Name = "Test5", Price = 33});

            var products = session.Products.Where(x => x.Name.EndsWith("X")).ToList();
            Assert.Equal(2, products.Count);
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhenStartsWithX()
        {
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "XTest3", Price = 10});
            session.Add(new Product {Name = "XTest4", Price = 22});
            session.Add(new Product {Name = "Test5", Price = 33});

            var products = session.Products.Where(x => x.Name.StartsWith("X")).ToList();
            Assert.Equal(2, products.Count);
        }
    }
}