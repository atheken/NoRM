using System;
using System.Linq;
using Xunit;
using Norm.Configuration;

namespace Norm.Tests
{
    public class LinqDeepGraph
    {
        public LinqDeepGraph()
        {
            MongoConfiguration.RemoveMapFor<Product>();
            MongoConfiguration.RemoveMapFor<Supplier>();
            MongoConfiguration.RemoveMapFor<InventoryChange>();
            MongoConfiguration.RemoveMapFor<Address>();
            using (var session = new Session())
            {
                session.Drop<Product>();
            }
        }

        [Fact]
        public void OneProductShouldBeReturnedWhenNestedSupplierQueried()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3", Price = 10, Supplier = new Supplier { Name = "Steve" } });
                session.Add(new Product { Name = "Test4", Price = 22 });
                session.Add(new Product { Name = "Test5", Price = 33 });
                var products = session.Products.Where(x => x.Supplier.Name == "Steve").ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void SupplierShouldBeQuerybleByDateMath()
        {
            using (var session = new Session())
            {
                var add = new Address { State = "HI", Street = "100 Main" };
                session.Add(new Product { Name = "Test3", Price = 10, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add } });
                session.Add(new Product { Name = "Test4", Price = 22, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2001, 2, 1) } });
                session.Add(new Product { Name = "Test5", Price = 33, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2002, 2, 1) } });
                var products = session.Products.Where(x => x.Supplier.CreatedOn.Year < 2001).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void SupplierShouldBeQuerybleByAddress()
        {
            using (var session = new Session())
            {
                var add = new Address { State = "HI", Street = "100 Main" };
                session.Add(new Product { Name = "Test3", Price = 10, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add } });
                session.Add(new Product { Name = "Test4", Price = 22, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2001, 2, 1) } });
                session.Add(new Product { Name = "Test5", Price = 33, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2002, 2, 1) } });
                var products = session.Products.Where(x => x.Supplier.Address.State == "HI").ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void SupplierShouldBeQuerybleByAddressAndWorkWithAndExpression()
        {
            using (var session = new Session())
            {
                var add = new Address { State = "HI", Street = "100 Main" };
                session.Add(new Product { Name = "Test3", Price = 10, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add } });
                session.Add(new Product { Name = "Test4", Price = 22, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2001, 2, 1) } });
                session.Add(new Product { Name = "Test5", Price = 33, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2002, 2, 1) } });
                var products = session.Products.Where(x => x.Supplier.Address.State == "HI" && x.Price == 10).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void SupplierShouldBeQuerybleByAddressAndWorkWithOrExpression()
        {
            using (var session = new Session())
            {
                var add = new Address { State = "HI", Street = "100 Main" };
                session.Add(new Product { Name = "Test3", Price = 10, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add } });
                session.Add(new Product { Name = "Test4", Price = 22, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2001, 2, 1) } });
                session.Add(new Product { Name = "Test5", Price = 33, Supplier = new Supplier { Name = "Steve", CreatedOn = new DateTime(2002, 2, 1) } });
                var products = session.Products.Where(x => x.Supplier.Address.State == "HI" || x.Price == 33).ToList();
                Assert.Equal(2, products.Count);
            }
        }

        [Fact]
        public void InventorySubqueryShouldReturnOneForTwoProducts()
        {
            using (var session = new Session())
            {
                //create a Product
                var p = new Product() { Name = "Test1", Price = 10 };
                //change the inventory
                p.Inventory.Add(new InventoryChange() { AmountChanged = 1 });
                session.Add(p);

                p = new Product() { Name = "Test1", Price = 10 };
                //change the inventory
                p.Inventory.Add(new InventoryChange() { AmountChanged = 1 });
                p.Inventory.Add(new InventoryChange() { AmountChanged = 2 });
                p.Inventory.Add(new InventoryChange() { AmountChanged = -1 });

                session.Add(p);

                var products = session.Products.Where(x => x.Inventory.Count() > 1).ToList();
                Assert.Equal(1, products.Count());
            }
        }


    }
}