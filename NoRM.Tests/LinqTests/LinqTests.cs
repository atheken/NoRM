using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Norm.Configuration;

namespace Norm.Tests
{
    public class LinqTests
    {
        public LinqTests()
        {
            MongoConfiguration.RemoveMapFor<Product>();
            MongoConfiguration.RemoveMapFor<Post>();
            using (var session = new Session())
            {
                session.Drop<Product>();
                session.Drop<Post>();
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportExternalParameters()
        {
            var external = 10;
            using(var session = new Session())
            {
                session.Add(new Product {Name = "test", Price = external});
                var product = session.Products.Where(p => p.Price == external).FirstOrDefault();

                Assert.Equal(10, product.Price);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportExternalObjectProperties()
        {
            // NOTE: This one fails because there's no support for parsing the object's property.
            // This even more complex when using a nested type like a product's supplier
            var external = new Product { Price = 10 };
            using (var session = new Session())
            {
                session.Add(new Product { Name = "test", Price = external.Price });
                var product = session.Products.Where(p => p.Price == external.Price).FirstOrDefault();

                Assert.Equal(10, product.Price);
            }
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhenThreeInDB() {
            using (var session = new Session()) {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 22 });
                session.Add(new Product { Name = "3", Price = 33 });
                var products = session.Products.ToList();
                Assert.Equal(3, products.Count);
            }
        }


        [Fact]
        public void FourProductsShouldBeReturnedWhenStartsOrEndsWithX()
        {
            using (var session = new Session())
            {                
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
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceEqual10() {
            using (var session = new Session()) {
                session.Add(new Product { Name = "Test3", Price = 10 });
                session.Add(new Product { Name = "Test4", Price = 22 });
                session.Add(new Product { Name = "Test5", Price = 33 });
                var products = session.Products.Where(x => x.Price ==10).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhen3InDbWithPriceGreaterThan10() {
            using (var session = new Session()) {
                session.Add(new Product { Name = "Test3", Price = 10 });
                session.Add(new Product { Name = "Test4", Price = 22 });
                session.Add(new Product { Name = "Test5", Price = 33 });
                var products = session.Products.Where(x => x.Price > 10).ToList();
                Assert.Equal(2, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceEqual10AndNameTest3() {
            using (var session = new Session()) {
                session.Add(new Product { Name = "Test3", Price = 10 });
                session.Add(new Product { Name = "Test4", Price = 22 });
                session.Add(new Product { Name = "Test5", Price = 33 });
                var products = session.Products.Where(x => x.Price == 10 && x.Name=="Test3").ToList();
                Assert.Equal(1, products.Count);
            }
        }
        [Fact]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceGreaterThan10LessThan30()
        {
            using (var session = new Session())
            {
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
            using (var session = new Session())
            {
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
                session.Add(new Product {Name = "Test1", Price = 10});
                session.Add(new Product {Name = "Test2", Price = 22});
                session.Add(new Product {Name = "Test3", Price = 33});
                var result = session.Products.SingleOrDefault(x => x.Price == 22);
                Assert.Equal(22, result.Price);
            }
        }

        [Fact]
        public void OneProductShouldBeReturnedWhen3InDbWithSingleUsingVariable()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test1", Price = 10 });
                session.Add(new Product { Name = "Test2", Price = 22 });
                var target = 22;
                session.Add(new Product { Name = "Test3", Price = 33 });
                var result = session.Products.SingleOrDefault(x => x.Price == target);
                Assert.Equal(target, result.Price);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhen3InDbWithPriceLessThan10OrGreaterThan30()
        {
            using (var session = new Session())
            {
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
                session.Add(new Product {Name = "XTest3", Price = 10});
                session.Add(new Product {Name = "XTest4", Price = 22});
                session.Add(new Product {Name = "Test5", Price = 33});
                var products = session.Products.Where(x => x.Name.StartsWith("X")).ToList();
                Assert.Equal(2, products.Count);
            }
        }

        [Fact]
        public void LastTwoProductsofThreeShouldBeReturnedWithSkipTake() {
            using (var session = new Session()) {
                session.Add(new Product { Name = "Test1", Price = 10 });
                session.Add(new Product { Name = "Test2", Price = 22 });
                session.Add(new Product { Name = "Test3", Price = 33 });
                var products = session.Products.Skip(1).Take(2).ToList();
                Assert.Equal(22.0, products[0].Price);
                Assert.Equal(33.0, products[1].Price);
            }
        }

        [Fact]
        public void FiltersBasedOnObjectId()
        {
            var targetId = ObjectId.NewObjectId();
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test1", Price = 10, _id = targetId });
                session.Add(new Product { Name = "Test2", Price = 22 });
                session.Add(new Product { Name = "Test3", Price = 33 });
                var products = session.Products.Where(p => p._id == targetId).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(targetId, products[0]._id);
            }
        }

        [Fact]
        public void CanQueryWithinEmbeddedArray()
        {
            using (var session = new Session())
            {
                var post1 = new Post { Title = "First", Comments = new List<Comment> { new Comment { Text = "comment1" }, new Comment { Text = "comment2" } } };
                var post2 = new Post { Title = "Second", Comments = new List<Comment> { new Comment { Text = "commentA" }, new Comment { Text = "commentB" } } };

                session.Add(post1);
                session.Add(post2);

                var found = session.Posts.FirstOrDefault(p => p.Comments.Any(a => a.Text == "commentA"));
                Assert.Equal(post2.Title, found.Title);

            }
        }

    }
}