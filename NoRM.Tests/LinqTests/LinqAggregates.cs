using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Norm.Configuration;
using Norm.Linq;
using Norm.Tests.Helpers;

namespace Norm.Tests
{
    [TestFixture]
    public class LinqAggregates
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
        public void TestSetup()
        {
            MongoConfiguration.RemoveMapFor<TestProduct>();
            using (var session = new Session())
            {
                session.Drop<TestProduct>();
            }
        }

        [Test]
        public void CountShouldReturn3WhenThreeProductsInDB()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 22 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                var productQueryable = session.Products;

                Assert.AreEqual(3, productQueryable.Count());

                Assert.AreEqual(false, productQueryable.QueryStructure().IsComplex);
            }
        }
        [Test]
        public void CountShouldReturn2WhenThreeProductsInDBAndWherePriceGreaterThan20()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 22 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                var queryable = session.Products;
                var result = queryable.Where(x => x.Price > 20).Count();
                Assert.AreEqual(2, result);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void Count_Should_Return_One_When_Id_Matches()
        {
            var target = ObjectId.NewObjectId();
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 40, _id = target });
                session.Add(new TestProduct { Name = "2", Price = 22, _id = ObjectId.NewObjectId() });
                session.Add(new TestProduct { Name = "3", Price = 33, _id = ObjectId.NewObjectId() });
                var queryable = session.Products;
                var result = queryable.Where(x => x._id == target).Count();
                Assert.AreEqual(1, result);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }
        [Test]
        public void SumShouldReturn60WhenThreeProductsInDBWIthSumPrice60()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "dd", Price = 10 });
                session.Add(new TestProduct { Name = "ss", Price = 20 });
                session.Add(new TestProduct { Name = "asdasddds", Price = 30 });
                var queryable = session.Products;
                var result = queryable.Sum(x => x.Price);
                Assert.AreEqual(60, result);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }
        [Test]
        public void SumShouldReturn30WhenThreeProductsInDBWIthSumPrice60AndPriceLessThan30()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products;
                var result = queryable.Where(x => x.Price < 30).Sum(x => x.Price);
                Assert.AreEqual(30, result);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
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
                var queryable = session.Products;
                var result = queryable.Where(x => names.Contains(x.Name)).Sum(x => x.Price);
                Assert.AreEqual(30, result);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void AvgShouldReturn20WhenThreeProductsInDBWIthSumPrice60()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products;
                var result = queryable.Average(x => x.Price);
                Assert.AreEqual(20, result);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void AvgShouldReturn15WhenThreeProductsInDBWIthSumPrice60AndLessThan30()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products;
                var result = queryable.Where(x => x.Price < 30).Average(x => x.Price);
                Assert.AreEqual(15, result);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void AvgShouldReturn500Point5WhenSumOfAllNumbersUpTo1000()
        {
            using (var session = new Session())
            {
                for (var i = 0; i < 1000; i++)
                {
                    session.Add(new TestProduct { Name = i.ToString(), Price = i + 1 });
                }
                var queryable = session.Products;
                var result = queryable.Average(x => x.Price);
                Assert.AreEqual(500.5, result);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void MinShouldReturn10WhenThreeProductsInDBWIthSumPrice60AndLowestIs10()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products;
                var result = queryable.Min(x => x.Price);
                Assert.AreEqual(10, result);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void MaxShouldReturn30WhenThreeProductsInDBWIthSumPrice60AndHighestIs30()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products;
                var result = queryable.Max(x => x.Price);
                Assert.AreEqual(30, result);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }
        [Test]
        public void AnyShouldReturnTrueWhenProductPrice10()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products;
                Assert.True(queryable.Any(x => x.Price == 10));
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void AnyShouldReturnTrueWhenProductPrice10AndWhere()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products;
                Assert.True(queryable.Where(x => x.Price < 30).Any(x => x.Price == 10));
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void AnyShouldReturnTrueWhenProductList()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products;
                Assert.True(queryable.Any());
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void AnyShouldReturnFalseWhenProductPrice100()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products;
                Assert.False(queryable.Any(x => x.Price == 100));
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

    }
}
