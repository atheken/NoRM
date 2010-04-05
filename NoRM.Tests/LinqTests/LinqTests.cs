using System;
using System.Collections.Generic;
using System.Linq;
using Norm.Linq;
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
            using (var session = new Session())
            {
                session.Add(new Product { Name = "test1", Price = 20 });
                session.Add(new Product { Name = "test", Price = 10 });
                var product = session.Products.Where(p => p.Price == external).FirstOrDefault();
                Assert.Equal(10, product.Price);
            }
        }

        [Fact]
        public void SingleQualifierQueryIsExecutedAsANDQuery()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "test1", Price = 20 });
                session.Add(new Product { Name = "test", Price = 10 });
                var product = session.Products.Where(p => p.Price == 10).First();
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
                var product = session.Products
                    .Where(p => p.Price == external.Price)
                    .FirstOrDefault();

                Assert.Equal(10, product.Price);
            }
        }
        [Fact]
        public void OneProductsShouldBeReturnedWhenThreeInDBWithChainedWhere()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 22 });
                session.Add(new Product { Name = "3", Price = 33 });
                var products = session.Products.Where(x => x.Price > 10);
                var result = products.Where(x => x.Price < 30);
                result = result.Where(x => x.Name.Contains("2"));
                Assert.Equal(22, result.SingleOrDefault().Price);
            }
        }
        [Fact]
        public void ThreeProductsShouldBeReturnedWhenThreeInDBOrderedByPrice()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 40 });
                session.Add(new Product { Name = "2", Price = 22 });
                session.Add(new Product { Name = "3", Price = 33 });
                var products = session.Products.OrderBy(x => x.Price).ToList();
                Assert.Equal(22, products[0].Price);
                Assert.Equal(40, products[2].Price);
            }
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhenThreeInDBOrderedByPriceThenByName()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 22 });
                session.Add(new Product { Name = "3", Price = 33 });
                session.Add(new Product { Name = "2", Price = 50 });
                session.Add(new Product { Name = "1", Price = 50 });
                var products = session.Products.OrderBy(x => x.Price).ThenBy(x => x.Name).ToList();
                Assert.Equal(10, products[0].Price);
                Assert.Equal(22, products[1].Price);
                Assert.Equal(33, products[2].Price);
                Assert.Equal(50, products[3].Price);
                Assert.Equal(50, products[4].Price);
                Assert.Equal("1", products[3].Name);
                Assert.Equal("2", products[4].Name);
            }
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhenThreeInDBOrderedByPriceThenByNameDescending()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 22 });
                session.Add(new Product { Name = "3", Price = 33 });
                session.Add(new Product { Name = "2", Price = 50 });
                session.Add(new Product { Name = "1", Price = 50 });
                var products = session.Products.OrderBy(x => x.Price).ThenByDescending(x => x.Name).ToList();
                Assert.Equal(10, products[0].Price);
                Assert.Equal(22, products[1].Price);
                Assert.Equal(33, products[2].Price);
                Assert.Equal(50, products[3].Price);
                Assert.Equal(50, products[4].Price);
                Assert.Equal("2", products[3].Name);
                Assert.Equal("1", products[4].Name);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenFiveInDBOrderedByPriceThenSkipThreeAndTakeOne()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 22 });
                session.Add(new Product { Name = "3", Price = 33 });
                session.Add(new Product { Name = "2", Price = 50 });
                session.Add(new Product { Name = "1", Price = 50 });
                var products = session.Products.OrderByDescending(x => x.Price).Skip(3).Take(1).ToList();
                Assert.Equal(22, products[0].Price);
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhenThreeInDBOrderedDewscendingByPrice()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 22 });
                session.Add(new Product { Name = "3", Price = 33 });
                var products = session.Products.OrderByDescending(x => x.Price).ToList();
                Assert.Equal(33, products[0].Price);
                Assert.Equal(10, products[2].Price);
            }
        }
        [Fact]
        public void FourProductsShouldBeReturnedWhenStartsOrEndsWithX()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3X", Price = 10 });
                session.Add(new Product { Name = "Test4X", Price = 22 });
                session.Add(new Product { Name = "Test5", Price = 33 });
                session.Add(new Product { Name = "XTest3", Price = 10 });
                session.Add(new Product { Name = "XTest4", Price = 22 });
                var products = session.Products.Where(x => x.Name.StartsWith("X") || x.Name.EndsWith("X")).ToList();
                Assert.Equal(4, products.Count);
            }
        }
        [Fact]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceEqual10()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3", Price = 10 });
                session.Add(new Product { Name = "Test4", Price = 22 });
                session.Add(new Product { Name = "Test5", Price = 33 });
                var products = session.Products.Where(x => x.Price == 10).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhen3InDbWithPriceGreaterThan10()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3", Price = 10 });
                session.Add(new Product { Name = "Test4", Price = 22 });
                session.Add(new Product { Name = "Test5", Price = 33 });
                var products = session.Products.Where(x => x.Price > 10).ToList();
                Assert.Equal(2, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceEqual10AndNameTest3()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3", Price = 10 });
                session.Add(new Product { Name = "Test4", Price = 22 });
                session.Add(new Product { Name = "Test5", Price = 33 });
                var products = session.Products.Where(x => x.Price == 10 && x.Name == "Test3").ToList();
                Assert.Equal(1, products.Count);
            }
        }
        [Fact]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceGreaterThan10LessThan30()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3", Price = 10 });
                session.Add(new Product { Name = "Test4", Price = 22 });
                session.Add(new Product { Name = "Test5", Price = 33 });
                var products = session.Products.Where(x => x.Price > 10 && x.Price < 30).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableDayIsFifth()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new Product { Name = "Test4X", Price = 22, Available = new DateTime(2000, 2, 6) });
                var products = session.Products.Where(x => x.Available.Day == 5).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableDateIsSameAsRequired()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new Product { Name = "Test4X", Price = 22, Available = new DateTime(2000, 2, 6) });
                var products = session.Products.Where(x => x.Available == new DateTime(2000, 2, 5)).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal("Test3X", products[0].Name);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableDayIsMonday()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3X", Price = 10, Available = DateTime.Now });
                session.Add(new Product { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(1) });
                session.Add(new Product { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(2) });
                session.Add(new Product { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(3) });
                session.Add(new Product { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(4) });
                session.Add(new Product { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(5) });
                session.Add(new Product { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(6) });
                var products = session.Products.Where(x => x.Available.DayOfWeek == DayOfWeek.Monday).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableGreaterThanToday()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new Product { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1) });
                session.Add(new Product { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new Product { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(1) });
                var products = session.Products.Where(x => x.Available > DateTime.Now).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableLessThanTodayPlus1()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new Product { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1) });
                session.Add(new Product { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new Product { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(2) });
                var products = session.Products.Where(x => x.Available > DateTime.Now.AddDays(1)).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableMonthIs2()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new Product { Name = "Test4X", Price = 22, Available = new DateTime(2001, 3, 6) });
                var products = session.Products.Where(x => x.Available.Month == 2).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableYearIs2000()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new Product { Name = "Test4X", Price = 22, Available = new DateTime(2001, 2, 6) });
                var products = session.Products.Where(x => x.Available.Year == 2000).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenIndexOfXEqual2()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3X", Price = 10 });
                session.Add(new Product { Name = "TestX4", Price = 22 });
                session.Add(new Product { Name = "TesXt5", Price = 33 });
                session.Add(new Product { Name = "TeXst3", Price = 10 });
                session.Add(new Product { Name = "TXest4", Price = 22 });
                var products = session.Products.Where(x => x.Name.IndexOf("X") == 2).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenIsNullOrEmptyWithEmptyString()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3X", Price = 10 });
                session.Add(new Product { Name = "Test4X", Price = 22 });
                session.Add(new Product { Name = "", Price = 33 });
                session.Add(new Product { Name = "XTest3", Price = 10 });
                session.Add(new Product { Name = "XTest4", Price = 22 });
                var products = session.Products.Where(x => string.IsNullOrEmpty(x.Name)).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenIsNullOrEmptyWithNull()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3X", Price = 10 });
                session.Add(new Product { Name = "Test4X", Price = 22 });
                session.Add(new Product { Price = 33 });
                session.Add(new Product { Name = "XTest3", Price = 10 });
                session.Add(new Product { Name = "XTest4", Price = 22 });
                var products = session.Products.Where(x => string.IsNullOrEmpty(x.Name)).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenStartsAndEndsWithX()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3X", Price = 10 });
                session.Add(new Product { Name = "Test4X", Price = 22 });
                session.Add(new Product { Name = "XTest5X", Price = 33 });
                session.Add(new Product { Name = "XTest3", Price = 10 });
                session.Add(new Product { Name = "XTest4", Price = 22 });
                var products = session.Products.Where(x => x.Name.StartsWith("X") && x.Name.EndsWith("X")).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhen3InDbWithNoExpression()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test1", Price = 10 });
                session.Add(new Product { Name = "Test2", Price = 22 });
                session.Add(new Product { Name = "Test3", Price = 33 });
                var products = session.Products.ToList();
                Assert.Equal(3, products.Count);
            }
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhenAvailableLessThanToday()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new Product { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1) });
                session.Add(new Product { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new Product { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(1) });
                var products = session.Products.Where(x => x.Available < DateTime.Now).ToList();
                Assert.Equal(3, products.Count);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhenNotEqualsOne()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test1", Price = 10 });
                session.Add(new Product { Name = "Test2", Price = 22 });
                session.Add(new Product { Name = "Test3", Price = 33 });
                var result = session.Products.Where(x => x.Price != 22);
                Assert.Equal(2, result.Count());
            }
        }

        [Fact]
        public void OneProductShouldBeReturnedWhen3InDbWithFirst()
        {
            using (var session = new Session())
            {
                session.CreateCappedCollection("Product"); //only capped collections return in insertion order
                session.Add(new Product { Name = "Test1", Price = 10 });
                session.Add(new Product { Name = "Test2", Price = 22 });
                session.Add(new Product { Name = "Test3", Price = 33 });
                var result = session.Products.First();
                Assert.Equal("Test1", result.Name);
            }
        }

        [Fact]
        public void OneProductShouldBeReturnedWhen3InDbWithSingle()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test1", Price = 10 });
                session.Add(new Product { Name = "Test2", Price = 22 });
                session.Add(new Product { Name = "Test3", Price = 33 });
                var result = session.Products.SingleOrDefault(x => x.Price == 22);
                Assert.Equal(22, result.Price);
            }
        }

        [Fact]
        public void OneProductShouldBeReturnedUsingSimpleANDQuery()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test1", Price = 10 });
                session.Add(new Product { Name = "Test2", Price = 22 });
                session.Add(new Product { Name = "Test3", Price = 22 });
                var results = session.Products.Where(x => x.Price == 22 && x.Name == "Test3").ToArray();
                Assert.Equal(1, results.Length);
                Assert.Equal(22, results[0].Price);
                Assert.Equal("Test3", results[0].Name);
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
                session.Add(new Product { Name = "Test3", Price = 10 });
                session.Add(new Product { Name = "Test4", Price = 22 });
                session.Add(new Product { Name = "Test5", Price = 33 });
                var products = session.Products.Where(x => x.Price > 10 || x.Price > 30).ToList();
                Assert.Equal(2, products.Count);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhenContainsX()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "TestX3", Price = 10 });
                session.Add(new Product { Name = "TestX4", Price = 22 });
                session.Add(new Product { Name = "Test5", Price = 33 });
                var products = session.Products.Where(x => x.Name.Contains("X")).ToList();
                Assert.Equal(2, products.Count);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenContainsMatchesFirstCharacter()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "ATest", Price = 10 });
                session.Add(new Product { Name = "BTest", Price = 22 });
                session.Add(new Product { Name = "CTest", Price = 33 });
                var products = session.Products.Where(x => x.Name.Contains("B")).ToList();
                Assert.Equal(1, products.Count);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhenEndsWithX()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test3X", Price = 10 });
                session.Add(new Product { Name = "Test4X", Price = 22 });
                session.Add(new Product { Name = "Test5", Price = 33 });
                var products = session.Products.Where(x => x.Name.EndsWith("X")).ToList();
                Assert.Equal(2, products.Count);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhenStartsWithX()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "XTest3", Price = 10 });
                session.Add(new Product { Name = "XTest4", Price = 22 });
                session.Add(new Product { Name = "Test5", Price = 33 });
                var products = session.Products.Where(x => x.Name.StartsWith("X")).ToList();
                Assert.Equal(2, products.Count);
            }
        }

        [Fact]
        public void LastTwoProductsofThreeShouldBeReturnedWithSkipTake()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test1", Price = 10 });
                session.Add(new Product { Name = "Test2", Price = 22 });
                session.Add(new Product { Name = "Test3", Price = 33 });
                session.Add(new Product { Name = "Test4", Price = 44 });
                var products = session.Products.Skip(1).Take(2).ToList();
                Assert.Equal(22.0, products[0].Price);
                Assert.Equal(33.0, products[1].Price);
                Assert.Equal(2, products.Count);
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
        public void FiltersBasedOnObjectIdInComplexQuery()
        {
            var targetId = ObjectId.NewObjectId();
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test1", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new Product { Name = "Test2", Price = 22, Available = new DateTime(2000, 2, 6), _id = targetId });
                session.Add(new Product { Name = "Test3", Price = 33, Available = new DateTime(2000, 2, 7) });
                var products = session.Products.Where(p => p._id == targetId && p.Available.Day == 6).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(targetId, products[0]._id);
            }
        }
        [Fact]
        public void FiltersBasedOnObjectIdExclusionInComplexQuery()
        {
            var targetId = ObjectId.NewObjectId();
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test1", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new Product { Name = "Test2", Price = 22, Available = new DateTime(2000, 2, 5), _id = targetId });
                session.Add(new Product { Name = "Test3", Price = 33, Available = new DateTime(2000, 2, 5) });
                var products = session.Products.Where(p => p._id != targetId && p.Available.Day == 5).ToList();
                Assert.Equal(2, products.Count);
                Assert.NotEqual(targetId, products[0]._id);
                Assert.NotEqual(targetId, products[1]._id);
            }
        }

        [Fact]
        public void FiltersBasedOnMagicObjectId()
        {
            var targetId = ObjectId.NewObjectId();
            using (var session = new Session())
            {
                session.Add(new Post { Id = targetId });
                session.Add(new Post());
                session.Add(new Post());
                var posts = session.Posts.Where(p => p.Id == targetId).ToList();
                Assert.Equal(1, posts.Count);
                Assert.Equal(targetId, posts[0].Id);
            }
        }

        [Fact]
        public void Filters_Based_On_Guid()
        {
            var targetId = Guid.NewGuid();
            using (var session = new Session())
            {
                session.Add(new Product { Name = "Test1", Price = 10, UniqueID = targetId });
                session.Add(new Product { Name = "Test2", Price = 22 });
                session.Add(new Product { Name = "Test3", Price = 33 });
                var products = session.Products.Where(p => p.UniqueID == targetId).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(targetId, products[0].UniqueID);
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
                var found = session.Posts.Where(p => p.Comments.Any(a => a.Text == "commentA")).SingleOrDefault();
                
                Assert.Equal("Second", found.Title);
            }
        }

        [Fact]
        public void CanQueryAndReturnSubClassedObjects()
        {
            using (var session = new Session())
            {
                session.Drop<SuperClassObject>();

                session.Add<SuperClassObject>(new SubClassedObject { Title = "Find This", ABool = true });
                session.Add<SuperClassObject>(new SubClassedObject { Title = "Don't Find This", ABool = false });

                var query = new MongoQuery<SuperClassObject>(session.Provider);

                var dtos = query.Where(dto => dto.Title == "Find This").ToList();

                Assert.Equal(1, dtos.Count);
                Assert.Equal("Find This", dtos[0].Title);
            }
        }

        [Fact]
        public void CanQueryAndReturnSubClassedObjects_EvenWhenAddedBySubClass()
        {
            using (var session = new Session())
            {
                session.Drop<SuperClassObject>();

                session.Add(new SubClassedObject());

                var query = new MongoQuery<SuperClassObject>(session.Provider);

                var dtos = query.ToList();

                Assert.Equal(1, dtos.Count);
            }
        }

        [Fact]
        public void CanQueryAndReturnSubClassedObjects_EvenWhenQueriedBySubClass()
        {
            using (var session = new Session())
            {
                session.Drop<SuperClassObject>();

                session.Add(new SubClassedObject());

                var query = new MongoQuery<SubClassedObject>(session.Provider);

                var dtos = query.ToList();

                Assert.Equal(1, dtos.Count);
            }
        }

        [Fact]
        public void CanQueryAndReturnSubClassedObjects_EvenWhenQueriedByInterface()
        {
            using (var session = new Session())
            {
                session.Drop<IDiscriminated>();

                var obj = new InterfaceDiscriminatedClass();
                session.Add(obj);

                var query = new MongoQuery<IDiscriminated>(session.Provider);

                var dtos = query.ToList();

                Assert.Equal(1, dtos.Count);
                Assert.Equal(obj.Id, dtos.Single().Id);
            }
        }
    }
}