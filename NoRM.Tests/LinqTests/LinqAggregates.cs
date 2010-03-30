using System.Linq;
using Xunit;
using Norm.Configuration;

namespace Norm.Tests
{

    public class LinqAggregates
    {
        public LinqAggregates()
        {
            MongoConfiguration.RemoveMapFor<Product>();
            using (var session = new Session())
            {
                session.Drop<Product>();
            }
        }

        [Fact]
        public void CountShouldReturn3WhenThreeProductsInDB()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 22 });
                session.Add(new Product { Name = "3", Price = 33 });
                var result = session.Products.Count();
                Assert.Equal(3, result);
            }
        }
        [Fact]
        public void CountShouldReturn2WhenThreeProductsInDBAndWherePriceGreaterThan20()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 22 });
                session.Add(new Product { Name = "3", Price = 33 });
                var result = session.Products.Where(x => x.Price > 20).Count();
                Assert.Equal(2, result);
            }
        }
        [Fact]
        public void CountShouldReturn2WhenThreeProductsInDBAndWhereIdMatches()
        {
            var target = ObjectId.NewObjectId();
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10, _id = target });
                session.Add(new Product { Name = "2", Price = 22, _id = ObjectId.NewObjectId() });
                session.Add(new Product { Name = "3", Price = 33, _id = ObjectId.NewObjectId() });
                var result = session.Products.Where(x => x._id == target).Count();
                Assert.Equal(1, result);
            }
        }        
        [Fact]
        public void SumShouldReturn60WhenThreeProductsInDBWIthSumPrice60()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "dd", Price = 10 });
                session.Add(new Product { Name = "ss", Price = 20 });
                session.Add(new Product { Name = "asdasddds", Price = 30 });
                var result = session.Products.Sum(x => x.Price);
                Assert.Equal(60, result);
            }
        }
        [Fact]
        public void SumShouldReturn30WhenThreeProductsInDBWIthSumPrice60AndPriceLessThan30()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 20 });
                session.Add(new Product { Name = "3", Price = 30 });
                var result = session.Products.Where(x => x.Price < 30).Sum(x => x.Price);
                Assert.Equal(30, result);
            }
        }
        [Fact]
        public void AvgShouldReturn20WhenThreeProductsInDBWIthSumPrice60()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 20 });
                session.Add(new Product { Name = "3", Price = 30 });
                var result = session.Products.Average(x => x.Price);
                Assert.Equal(20, result);
            }
        }
        [Fact]
        public void AvgShouldReturn15WhenThreeProductsInDBWIthSumPrice60AndLessThan30()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 20 });
                session.Add(new Product { Name = "3", Price = 30 });
                var result = session.Products.Where(x => x.Price < 30).Average(x => x.Price);
                Assert.Equal(15, result);
            }
        }

        [Fact]
        public void AvgShouldReturn500Point5WhenSumOfAllNumbersUpTo1000()
        {
            using (var session = new Session())
            {
                for (var i = 0; i < 1000; i++)
                {
                    session.Add(new Product { Name = i.ToString(), Price = i + 1 });
                }

                var result = session.Products.Average(x => x.Price);
                Assert.Equal(500.5, result);
            }
        }

        [Fact]
        public void MinShouldReturn10WhenThreeProductsInDBWIthSumPrice60AndLowestIs10()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 20 });
                session.Add(new Product { Name = "3", Price = 30 });
                var result = session.Products.Min(x => x.Price);
                Assert.Equal(10, result);
            }
        }

        [Fact]
        public void MaxShouldReturn30WhenThreeProductsInDBWIthSumPrice60AndHighestIs30()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 20 });
                session.Add(new Product { Name = "3", Price = 30 });
                var result = session.Products.Max(x => x.Price);
                Assert.Equal(30, result);
            }
        }
        [Fact]
        public void AnyShouldReturnTrueWhenProductPrice10()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 20 });
                session.Add(new Product { Name = "3", Price = 30 });

                Assert.True(session.Products.Any(x => x.Price == 10));
            }
        }
        [Fact]
        public void AnyShouldReturnFalseWhenProductPrice100()
        {
            using (var session = new Session())
            {
                session.Add(new Product { Name = "1", Price = 10 });
                session.Add(new Product { Name = "2", Price = 20 });
                session.Add(new Product { Name = "3", Price = 30 });

                Assert.False(session.Products.Any(x => x.Price == 100));
            }
        }
    }
}
