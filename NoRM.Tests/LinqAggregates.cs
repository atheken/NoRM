using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
    using Xunit;

namespace NoRM.Tests {

    public class LinqAggregates {
        public LinqAggregates() {
            using (var session = new Session()) {
                session.Drop<Product>();
            }
        }

        [Fact]
        public void CountShouldReturn3WhenThreeProductsInDB() {
            using (var session = new Session()) {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 22 });
                session.Add(new Product { Name = "3", Price = 33 });
                var result = session.Products.Count();
                Assert.Equal(3, result);
            }
        }

        //[Fact]
        //public void SumShouldReturn60WhenThreeProductsInDBWIthSumPrice60() {
        //    using (var session = new Session()) {
        //        session.Add(new Product { Name = "1", Price = 10 });
        //        session.Add(new Product { Name = "2", Price = 20 });
        //        session.Add(new Product { Name = "3", Price = 30 });
        //        var result = session.Products.Sum(x=>x.Price);
        //        Assert.Equal(60, result);
        //    }
        //}

    }
}
