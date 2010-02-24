namespace NoRM.Tests
{
    using System;
    using System.Linq;
    using Xunit;

    public class LinqDeepGraph
    {
        [Fact]
        public void OneProductShouldBeReturnedWhenNestedSupplierQueried()
        {
            var session = new Session();
            session.Drop<Product>();

            session.Add(new Product {Name = "Test3", Price = 10, Supplier = new Supplier {Name = "Steve"}});
            session.Add(new Product {Name = "Test4", Price = 22});
            session.Add(new Product {Name = "Test5", Price = 33});

            var products = session.Products.Where(x => x.Supplier.Name == "Steve").ToList();
            Assert.Equal(1, products.Count);
        }

        [Fact]
        public void SupplierShouldBeQuerybleByDateMath()
        {
            var session = new Session();
            session.Drop<Product>();
            var add = new Address {State = "HI", Street = "100 Main"};

            session.Add(new Product {Name = "Test3", Price = 10, Supplier = new Supplier {Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add}});
            session.Add(new Product {Name = "Test4", Price = 22, Supplier = new Supplier {Name = "Steve", CreatedOn = new DateTime(2001, 2, 1)}});
            session.Add(new Product {Name = "Test5", Price = 33, Supplier = new Supplier {Name = "Steve", CreatedOn = new DateTime(2002, 2, 1)}});

            var products = session.Products.Where(x => x.Supplier.CreatedOn.Year < 2001).ToList();
            //this is returning all three for some reason...
            Assert.Equal(1, products.Count);
        }

        [Fact]
        public void SupplierShouldBeQuerybleByAddress()
        {
            var session = new Session();
            session.Drop<Product>();
            var add = new Address {State = "HI", Street = "100 Main"};

            session.Add(new Product {Name = "Test3", Price = 10, Supplier = new Supplier {Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add}});
            session.Add(new Product {Name = "Test4", Price = 22, Supplier = new Supplier {Name = "Steve", CreatedOn = new DateTime(2001, 2, 1)}});
            session.Add(new Product {Name = "Test5", Price = 33, Supplier = new Supplier {Name = "Steve", CreatedOn = new DateTime(2002, 2, 1)}});

            var products = session.Products.Where(x => x.Supplier.Address.State == "HI").ToList();
            Assert.Equal(1, products.Count);
        }

        [Fact]
        public void SupplierShouldBeQuerybleByAddressAndWorkWithAndExpression()
        {
            var session = new Session();
            session.Drop<Product>();
            var add = new Address {State = "HI", Street = "100 Main"};

            session.Add(new Product {Name = "Test3", Price = 10, Supplier = new Supplier {Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add}});
            session.Add(new Product {Name = "Test4", Price = 22, Supplier = new Supplier {Name = "Steve", CreatedOn = new DateTime(2001, 2, 1)}});
            session.Add(new Product {Name = "Test5", Price = 33, Supplier = new Supplier {Name = "Steve", CreatedOn = new DateTime(2002, 2, 1)}});

            var products = session.Products.Where(x => x.Supplier.Address.State == "HI" && x.Price == 10).ToList();
            Assert.Equal(1, products.Count);
        }

        [Fact]
        public void SupplierShouldBeQuerybleByAddressAndWorkWithOrExpression()
        {
            var session = new Session();
            session.Drop<Product>();
            var add = new Address {State = "HI", Street = "100 Main"};

            session.Add(new Product {Name = "Test3", Price = 10, Supplier = new Supplier {Name = "Steve", CreatedOn = new DateTime(2000, 2, 1), Address = add}});
            session.Add(new Product {Name = "Test4", Price = 22, Supplier = new Supplier {Name = "Steve", CreatedOn = new DateTime(2001, 2, 1)}});
            session.Add(new Product {Name = "Test5", Price = 33, Supplier = new Supplier {Name = "Steve", CreatedOn = new DateTime(2002, 2, 1)}});

            var products = session.Products.Where(x => x.Supplier.Address.State == "HI" || x.Price == 33).ToList();
            Assert.Equal(2, products.Count);
        }
    }
}