using System;
using System.Linq;
using NUnit.Framework;
using Norm.Configuration;
using Norm.Tests.Helpers;

namespace Norm.Tests
{
    [TestFixture]
    public class LinqDeepGraph
    {
		private Mongod _proc;

		[TestFixtureSetUp]
		public void SetupTestFixture ()
		{
			_proc = new Mongod ();
		}

		[TestFixtureTearDown]
		public void TearDownTestFixture ()
		{
			_proc.Dispose ();
		}
		
        [SetUp]
        public void Setup()
        {
            MongoConfiguration.RemoveMapFor<TestProduct>();
            MongoConfiguration.RemoveMapFor<Supplier>();
            MongoConfiguration.RemoveMapFor<InventoryChange>();
            MongoConfiguration.RemoveMapFor<Address>();
            using (var session = new Session())
            {
                session.Drop<TestProduct>();
            }
        }

        [Test]
        public void OneProductShouldBeReturnedWhenNestedSupplierQueried()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3", Price = 10, Supplier = new Supplier { Name = "Steve" } });
                session.Add(new TestProduct { Name = "Test4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products;
                var products = queryable.Where(x => x.Supplier.Name == "Steve").ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void SupplierShouldBeQuerybleByDateMath()
        {
            using (var session = new Session())
            {
                var add = new Address { State = "HI", Street = "100 Main" };
                session.Add(new TestProduct { Name = "Test3", Price = 10, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add } });
                session.Add(new TestProduct { Name = "Test4", Price = 22, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2001, 2, 1) } });
                session.Add(new TestProduct { Name = "Test5", Price = 33, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2002, 2, 1) } });
                var queryable = session.Products;
                var products = queryable.Where(x => x.Supplier.CreatedOn.Year < 2001).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void SupplierShouldBeQuerybleByAddress()
        {
            using (var session = new Session())
            {
                var add = new Address { State = "HI", Street = "100 Main" };
                session.Add(new TestProduct { Name = "Test3", Price = 10, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add } });
                session.Add(new TestProduct { Name = "Test4", Price = 22, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2001, 2, 1) } });
                session.Add(new TestProduct { Name = "Test5", Price = 33, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2002, 2, 1) } });
                var queryable = session.Products;
                var products = queryable.Where(x => x.Supplier.Address.State == "HI").ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void SupplierShouldBeQuerybleByAddressAndWorkWithAndExpression()
        {
            using (var session = new Session())
            {
                var add = new Address { State = "HI", Street = "100 Main" };
                session.Add(new TestProduct { Name = "Test3", Price = 10, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add } });
                session.Add(new TestProduct { Name = "Test4", Price = 22, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2001, 2, 1) } });
                session.Add(new TestProduct { Name = "Test5", Price = 33, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2002, 2, 1) } });
                var queryable = session.Products;
                var products = queryable.Where(x => x.Supplier.Address.State == "HI" && x.Price == 10).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void SupplierShouldBeQuerybleByAddressAndWorkWithOrExpression()
        {
            using (var session = new Session())
            {
                var add = new Address { State = "HI", Street = "100 Main" };
                session.Add(new TestProduct { Name = "Test3", Price = 10, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add } });
                session.Add(new TestProduct { Name = "Test4", Price = 22, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2001, 2, 1) } });
                session.Add(new TestProduct { Name = "Test5", Price = 33, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2002, 2, 1) } });
                var queryable = session.Products;
                var products = queryable.Where(x => x.Supplier.Address.State == "HI" || x.Price == 33).ToList();
                Assert.AreEqual(2, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void InventorySubqueryShouldReturnOneForTwoProducts()
        {
            using (var session = new Session())
            {
                //create a Product
                var p = new TestProduct() { Name = "Test1", Price = 10 };
                //change the inventory
                p.Inventory.Add(new InventoryChange() { AmountChanged = 1 });
                session.Add(p);

                p = new TestProduct() { Name = "Test3", Price = 10 };
                //change the inventory
                p.Inventory.Add(new InventoryChange() { AmountChanged = 1 });
                p.Inventory.Add(new InventoryChange() { AmountChanged = 2 });
                p.Inventory.Add(new InventoryChange() { AmountChanged = -1 });

                session.Add(p);
                var queryable = session.Products;
                var products = queryable.Where(x => x.Inventory.Count() > 2).ToArray();

                Assert.AreEqual(1, products.Count());
                Assert.AreEqual("Test3", products[0].Name);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }
    }
}