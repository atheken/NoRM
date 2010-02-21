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

    class Supplier {
        public string Name { get; set; }
    }
    class Product {
        public object ID { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public Supplier Supplier { get; set; }
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
        public void One_Product_Should_Be_Returned_When_Nested_Supplier_Queried() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3", Price = 10, Supplier = new Supplier() { Name = "Steve" } });
            session.Add(new Product() { Name = "Test4", Price = 22 });
            session.Add(new Product() { Name = "Test5", Price = 33 });

            var products = session.Products.Where(x=>x.Supplier.Name=="Steve").ToList();

            Assert.AreEqual(1, products.Count);

        }

    }
}
