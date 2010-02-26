using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NoRM.Tests {

    [TestFixture]
    public class LinqDeepGraph {

        [Test]
        public void One_Product_Should_Be_Returned_When_Nested_Supplier_Queried() {

            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product() { Name = "Test3", Price = 10, Supplier = new Supplier() { Name = "Steve" } });
            session.Add(new Product() { Name = "Test4", Price = 22 });
            session.Add(new Product() { Name = "Test5", Price = 33 });

            var products = session.Products.Where(x => x.Supplier.Name == "Steve").ToList();

            Assert.AreEqual(1, products.Count);

        }
        [Test]
        public void Supplier_Should_Be_Queryble_By_DateMath() {

            var session = new Session();
            session.Drop<Product>();
            var add = new Address() { State = "HI", Street = "100 Main" };
            
            session.Add(new Product() { Name = "Test3", Price = 10, Supplier = new Supplier() { Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add } });
            session.Add(new Product() { Name = "Test4", Price = 22, Supplier = new Supplier() { Name = "Steve", CreatedOn = new DateTime(2001, 2, 1) } });
            session.Add(new Product() { Name = "Test5", Price = 33, Supplier = new Supplier() { Name = "Steve", CreatedOn = new DateTime(2002, 2, 1) } });

            var products = session.Products.Where(x => x.Supplier.CreatedOn.Year<2001).ToList();
            
            //this is returning all three for some reason...
            Assert.AreEqual(1, products.Count);

        }
        [Test]
        public void Supplier_Should_Be_Queryble_By_Address() {

            var session = new Session();
            session.Drop<Product>();
            var add = new Address() { State = "HI", Street = "100 Main" };

            session.Add(new Product() { Name = "Test3", Price = 10, Supplier = new Supplier() { Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add } });
            session.Add(new Product() { Name = "Test4", Price = 22, Supplier = new Supplier() { Name = "Steve", CreatedOn = new DateTime(2001, 2, 1) } });
            session.Add(new Product() { Name = "Test5", Price = 33, Supplier = new Supplier() { Name = "Steve", CreatedOn = new DateTime(2002, 2, 1) } });

            var products = session.Products.Where(x => x.Supplier.Address.State=="HI").ToList();

            Assert.AreEqual(1, products.Count);

        }
        [Test]
        public void Supplier_Should_Be_Queryble_By_Address_And_Work_With_And_Expression() {

            var session = new Session();
            session.Drop<Product>();
            var add = new Address() { State = "HI", Street = "100 Main" };

            session.Add(new Product() { Name = "Test3", Price = 10, Supplier = new Supplier() { Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add } });
            session.Add(new Product() { Name = "Test4", Price = 22, Supplier = new Supplier() { Name = "Steve", CreatedOn = new DateTime(2001, 2, 1) } });
            session.Add(new Product() { Name = "Test5", Price = 33, Supplier = new Supplier() { Name = "Steve", CreatedOn = new DateTime(2002, 2, 1) } });

            var products = session.Products.Where(x => x.Supplier.Address.State == "HI" && x.Price == 10).ToList();

            Assert.AreEqual(1, products.Count);

        }
        [Test]
        public void Supplier_Should_Be_Queryble_By_Address_And_Work_With_Or_Expression() {

            var session = new Session();
            session.Drop<Product>();
            var add = new Address() { State = "HI", Street = "100 Main" };

            session.Add(new Product() { Name = "Test3", Price = 10, Supplier = new Supplier() { Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add } });
            session.Add(new Product() { Name = "Test4", Price = 22, Supplier = new Supplier() { Name = "Steve", CreatedOn = new DateTime(2001, 2, 1) } });
            session.Add(new Product() { Name = "Test5", Price = 33, Supplier = new Supplier() { Name = "Steve", CreatedOn = new DateTime(2002, 2, 1) } });

            var products = session.Products.Where(x => x.Supplier.Address.State == "HI" || x.Price == 33).ToList();

            Assert.AreEqual(2, products.Count);

        }
    }
}
