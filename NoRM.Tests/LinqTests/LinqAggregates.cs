using System.Collections.Generic;
using System.Linq;
using Xunit;
using Norm.Configuration;

namespace Norm.Tests
{

    public class LinqAggregates
    {
        public LinqAggregates()
        {
            MongoConfiguration.RemoveMapFor<TestProduct>();
            using (var session = new Session())
            {
                session.Drop<TestProduct>();
            }
        }

        [Fact]
        public void CountShouldReturn3WhenThreeProductsInDB()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 22 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                var result = session.Products.Count();
                Assert.Equal(3, result);
                Assert.Equal(false, session.TranslationResults.IsComplex);
            }
        }
        [Fact]
        public void CountShouldReturn2WhenThreeProductsInDBAndWherePriceGreaterThan20()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 22 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                var result = session.Products.Where(x => x.Price > 20).Count();
                Assert.Equal(2, result);
                Assert.Equal(false, session.TranslationResults.IsComplex);
            }
        }
   
        [Fact]
        public void Count_Should_Return_One_When_Id_Matches()
        {
            var target = ObjectId.NewObjectId();
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 40, _id = target });
                session.Add(new TestProduct { Name = "2", Price = 22, _id = ObjectId.NewObjectId() });
                session.Add(new TestProduct { Name = "3", Price = 33, _id = ObjectId.NewObjectId() });
                var result = session.Products.Where(x => x._id == target).Count();
                Assert.Equal(1, result);
                Assert.Equal(false, session.TranslationResults.IsComplex);
            }
        }        
        [Fact]
        public void SumShouldReturn60WhenThreeProductsInDBWIthSumPrice60()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "dd", Price = 10 });
                session.Add(new TestProduct { Name = "ss", Price = 20 });
                session.Add(new TestProduct { Name = "asdasddds", Price = 30 });
                var result = session.Products.Sum(x => x.Price);
                Assert.Equal(60, result);
                Assert.Equal(false, session.TranslationResults.IsComplex);
            }
        }
        [Fact]
        public void SumShouldReturn30WhenThreeProductsInDBWIthSumPrice60AndPriceLessThan30()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var result = session.Products.Where(x => x.Price < 30).Sum(x => x.Price);
                Assert.Equal(30, result);
                Assert.Equal(false, session.TranslationResults.IsComplex);
            }
        }

        [Fact]
        public void SumShouldReturn30WhenThreeProductsInDBWIthSumPrice60AndNameContains1or2()
        {
            using (var session = new Session())
            {
                var names = new List<string>();
                names.Add("1");
                names.Add("2");

                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var result = session.Products.Where(x => names.Contains(x.Name)).Sum(x => x.Price);
                Assert.Equal(30, result);
                Assert.Equal(false, session.TranslationResults.IsComplex);
            }
        }

        [Fact]
        public void AvgShouldReturn20WhenThreeProductsInDBWIthSumPrice60()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var result = session.Products.Average(x => x.Price);
                Assert.Equal(20, result);
                Assert.Equal(false, session.TranslationResults.IsComplex);
            }
        }

        [Fact]
        public void AvgShouldReturn15WhenThreeProductsInDBWIthSumPrice60AndLessThan30()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var result = session.Products.Where(x => x.Price < 30).Average(x => x.Price);
                Assert.Equal(15, result);
                Assert.Equal(false, session.TranslationResults.IsComplex);
            }
        }

        [Fact]
        public void AvgShouldReturn500Point5WhenSumOfAllNumbersUpTo1000()
        {
            using (var session = new Session())
            {
                for (var i = 0; i < 1000; i++)
                {
                    session.Add(new TestProduct { Name = i.ToString(), Price = i + 1 });
                }

                var result = session.Products.Average(x => x.Price);
                Assert.Equal(500.5, result);
                Assert.Equal(false, session.TranslationResults.IsComplex);
            }
        }

        [Fact]
        public void MinShouldReturn10WhenThreeProductsInDBWIthSumPrice60AndLowestIs10()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var result = session.Products.Min(x => x.Price);
                Assert.Equal(10, result);
                Assert.Equal(false, session.TranslationResults.IsComplex);
            }
        }

        [Fact]
        public void MaxShouldReturn30WhenThreeProductsInDBWIthSumPrice60AndHighestIs30()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var result = session.Products.Max(x => x.Price);
                Assert.Equal(30, result);
                Assert.Equal(false, session.TranslationResults.IsComplex);
            }
        }
        [Fact]
        public void AnyShouldReturnTrueWhenProductPrice10()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                Assert.True(session.Products.Any(x => x.Price == 10));
                Assert.Equal(false, session.TranslationResults.IsComplex);
            }
        }

        [Fact]
        public void AnyShouldReturnTrueWhenProductPrice10AndWhere()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                Assert.True(session.Products.Where(x => x.Price < 30).Any(x => x.Price == 10));
                Assert.Equal(true, session.TranslationResults.IsComplex);
            }
        }

        [Fact]
        public void AnyShouldReturnTrueWhenProductList()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                Assert.True(session.Products.Any());
                Assert.Equal(false, session.TranslationResults.IsComplex);
            }
        }

        [Fact]
        public void AnyShouldReturnFalseWhenProductPrice100()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                Assert.False(session.Products.Any(x => x.Price == 100));
                Assert.Equal(false, session.TranslationResults.IsComplex);
            }
        }

    }
}
