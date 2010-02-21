using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.Linq;
using NUnit.Framework;

namespace NoRM.Tests {

    class Session {
        MongoQueryProvider _provider;
        public Session() {
            _provider = new MongoQueryProvider("Northwind");
        }

        public IQueryable<Product> Products {
            get {
                return new MongoQuery<Product>(_provider);
            }
        }

        public void Add<T>(T item) where T:class, new() {
            var coll = _provider.DB.GetCollection<T>(typeof(T).Name);
            
            //see if the item exists
            coll.Insert(item);
            
        }
        public void Update<T>(T item) where T : class, new() {
            var coll = _provider.DB.GetCollection<T>(typeof(T).Name);

            //see if the item exists
            coll.UpdateOne(item,item);

        }
        public void Drop<T>() {
            _provider.DB.DropCollection(typeof(T).Name);
        }
    }

    class Address {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
    class Supplier {
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public Address Address { get; set; }
        public Supplier() {
            Address = new Address();
            CreatedOn = DateTime.Now;
        }
    }
    class Product {
        public object ID { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public Supplier Supplier { get; set; }
        public DateTime Available { get; set; }
        public Product() {
            Supplier = new Supplier();
        }
    }
    
    [TestFixture]
    public class LinqTests {

        [Test]
        public void Three_Products_Should_Be_Returned_When_3_In_Db_With_No_Expression() {

            var session = new Session();
            session.Drop<Product>();
            session.Add(new Product(){Name="Test1",Price=10});
            session.Add(new Product(){Name="Test2",Price=22});
            session.Add(new Product(){Name="Test3",Price=33});
            
            var products = session.Products.ToList();

            Assert.AreEqual(3, products.Count);

        }

        [Test]
        public void Two_Products_Should_Be_Returned_When_3_In_Db_With_Price_GreaterThan_10() {

            var session = new Session();
            session.Drop<Product>();
            session.Add(new Product() { Name = "Test3", Price = 10 });
            session.Add(new Product() { Name = "Test4", Price = 22 });
            session.Add(new Product() { Name = "Test5", Price = 33 });

            var products = session.Products.Where(x => x.Price > 10).ToList();

            Assert.AreEqual(2, products.Count);

        }
        [Test]
        public void One_Products_Should_Be_Returned_When_3_In_Db_With_Price_GreaterThan_10_Less_Than_30() {

            var session = new Session();
            session.Drop<Product>();
            session.Add(new Product() { Name = "Test3", Price = 10 });
            session.Add(new Product() { Name = "Test4", Price = 22 });
            session.Add(new Product() { Name = "Test5", Price = 33 });

            var products = session.Products.Where(x => x.Price > 10 && x.Price <30).ToList();

            Assert.AreEqual(1, products.Count);

        }

        [Test]
        public void Two_Products_Should_Be_Returned_When_3_In_Db_With_Price_LessThan_10_Or_GreaterThan_30() {

            var session = new Session();
            session.Drop<Product>();
            session.Add(new Product() { Name = "Test3", Price = 10 });
            session.Add(new Product() { Name = "Test4", Price = 22 });
            session.Add(new Product() { Name = "Test5", Price = 33 });

            var products = session.Products.Where(x => x.Price > 10 || x.Price > 30).ToList();

            Assert.AreEqual(2, products.Count);

        }


        [Test]
        public void Two_Products_Should_Be_Returned_When_StartsWith_X() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "XTest3", Price = 10 });
            session.Add(new Product() { Name = "XTest4", Price = 22 });
            session.Add(new Product() { Name = "Test5", Price = 33 });

            var products = session.Products.Where(x => x.Name.StartsWith("X")).ToList();

            Assert.AreEqual(2, products.Count);

        }
        [Test]
        public void Two_Products_Should_Be_Returned_When_EndsWith_X() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3X", Price = 10 });
            session.Add(new Product() { Name = "Test4X", Price = 22 });
            session.Add(new Product() { Name = "Test5", Price = 33 });

            var products = session.Products.Where(x => x.Name.EndsWith("X")).ToList();

            Assert.AreEqual(2, products.Count);

        }
        [Test]
        public void Two_Products_Should_Be_Returned_When_Contains_X() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "TestX3", Price = 10 });
            session.Add(new Product() { Name = "TestX4", Price = 22 });
            session.Add(new Product() { Name = "Test5", Price = 33 });

            var products = session.Products.Where(x => x.Name.Contains("X")).ToList();

            Assert.AreEqual(2, products.Count);

        }

        [Test]
        public void Four_Products_Should_Be_Returned_When_Starts_Or_EndsWith_X() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3X", Price = 10 });
            session.Add(new Product() { Name = "Test4X", Price = 22 });
            session.Add(new Product() { Name = "Test5", Price = 33 });
            session.Add(new Product() { Name = "XTest3", Price = 10 });
            session.Add(new Product() { Name = "XTest4", Price = 22 });

            var products = session.Products.Where(x => x.Name.StartsWith("X") || x.Name.EndsWith("X")).ToList();

            Assert.AreEqual(4, products.Count);

        }
        [Test]
        public void One_Products_Should_Be_Returned_When_Starts_And_EndsWith_X() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3X", Price = 10 });
            session.Add(new Product() { Name = "Test4X", Price = 22 });
            session.Add(new Product() { Name = "XTest5X", Price = 33 });
            session.Add(new Product() { Name = "XTest3", Price = 10 });
            session.Add(new Product() { Name = "XTest4", Price = 22 });

            var products = session.Products.Where(x => x.Name.StartsWith("X") && x.Name.EndsWith("X")).ToList();

            Assert.AreEqual(1, products.Count);

        }

        [Test]
        public void One_Products_Should_Be_Returned_When_IndexOf_X_Equal_2() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3X", Price = 10 });
            session.Add(new Product() { Name = "TestX4", Price = 22 });
            session.Add(new Product() { Name = "TesXt5", Price = 33 });
            session.Add(new Product() { Name = "TeXst3", Price = 10 });
            session.Add(new Product() { Name = "TXest4", Price = 22 });

            var products = session.Products.Where(x => x.Name.IndexOf("X") == 2).ToList();

            Assert.AreEqual(1, products.Count);

        }

        [Test]
        public void One_Products_Should_Be_Returned_When_IsNullOrEmpty_With_EmptyString() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3X", Price = 10 });
            session.Add(new Product() { Name = "Test4X", Price = 22 });
            session.Add(new Product() { Name = "", Price = 33 });
            session.Add(new Product() { Name = "XTest3", Price = 10 });
            session.Add(new Product() { Name = "XTest4", Price = 22 });

            var products = session.Products.Where(x => string.IsNullOrEmpty(x.Name)).ToList();

            Assert.AreEqual(1, products.Count);

        }

        [Test]
        public void One_Products_Should_Be_Returned_When_IsNullOrEmpty_With_Null() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3X", Price = 10 });
            session.Add(new Product() { Name = "Test4X", Price = 22 });
            session.Add(new Product() { Price = 33 });
            session.Add(new Product() { Name = "XTest3", Price = 10 });
            session.Add(new Product() { Name = "XTest4", Price = 22 });

            var products = session.Products.Where(x => string.IsNullOrEmpty(x.Name)).ToList();

            Assert.AreEqual(1, products.Count);

        }

        [Test]
        public void One_Products_Should_Be_Returned_When_Available_GreaterThan_Today() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3X", Price = 10, Available=DateTime.Now.AddDays(-1) });
            session.Add(new Product() { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1) });
            session.Add(new Product() { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1) });
            session.Add(new Product() { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(1) });

            var products = session.Products.Where(x => x.Available >DateTime.Now).ToList();

            Assert.AreEqual(1, products.Count);

        }

        [Test]
        public void Three_Products_Should_Be_Returned_When_Available_LessThan_Today() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1) });
            session.Add(new Product() { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1) });
            session.Add(new Product() { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1) });
            session.Add(new Product() { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(1) });

            var products = session.Products.Where(x => x.Available < DateTime.Now).ToList();

            Assert.AreEqual(3, products.Count);

        }

        [Test]
        public void One_Products_Should_Be_Returned_When_Available_LessThan_Today_Plus1() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1) });
            session.Add(new Product() { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1) });
            session.Add(new Product() { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1) });
            session.Add(new Product() { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(2) });

            var products = session.Products.Where(x => x.Available > DateTime.Now.AddDays(1)).ToList();

            Assert.AreEqual(1, products.Count);

        }

        [Test]
        public void One_Products_Should_Be_Returned_When_Available_Day_Is_Monday() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3X", Price = 10, Available = DateTime.Now });
            session.Add(new Product() { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(1) });
            session.Add(new Product() { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(2) });
            session.Add(new Product() { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(3) });
            session.Add(new Product() { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(4) });
            session.Add(new Product() { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(5) });
            session.Add(new Product() { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(6) });

            var products = session.Products.Where(x => x.Available.DayOfWeek==DayOfWeek.Monday).ToList();

            Assert.AreEqual(1, products.Count);

        }
        [Test]
        public void One_Products_Should_Be_Returned_When_Available_Day_Is_Fifth() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3X", Price = 10, Available = new DateTime(2000,2,5) });
            session.Add(new Product() { Name = "Test4X", Price = 22, Available = new DateTime(2000,2, 6) });

            var products = session.Products.Where(x => x.Available.Day==5).ToList();

            Assert.AreEqual(1, products.Count);

        }

        [Test]
        public void One_Products_Should_Be_Returned_When_Available_Year_Is_2000() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5) });
            session.Add(new Product() { Name = "Test4X", Price = 22, Available = new DateTime(2001, 2, 6) });

            var products = session.Products.Where(x => x.Available.Year == 2000).ToList();

            Assert.AreEqual(1, products.Count);

        }

        [Test]
        public void One_Products_Should_Be_Returned_When_Available_Month_Is_2() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5) });
            session.Add(new Product() { Name = "Test4X", Price = 22, Available = new DateTime(2001, 3, 6) });

            var products = session.Products.Where(x => x.Available.Month == 2).ToList();

            Assert.AreEqual(1, products.Count);

        }

    }
}
