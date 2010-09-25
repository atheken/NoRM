using System;
using System.Collections.Generic;
using System.Linq;
using Norm.Linq;
using NUnit.Framework;
using Norm.Configuration;
using System.Text.RegularExpressions;
using Norm.Tests.Helpers;

namespace Norm.Tests
{
    [TestFixture]
    public class LinqTests
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
        public void Setup ()
        {
        	MongoConfiguration.RemoveMapFor<TestProduct> ();
        	MongoConfiguration.RemoveMapFor<Post> ();
        	using (var session = new Session ())
            {
        		session.Drop<TestProduct> ();
        		session.Drop<Post> ();
        	}
        	using (var db = Mongo.Create (TestHelper.ConnectionString ("strict=false")))
            {
        		db.Database.DropCollection ("acmePost");
        	}
        }
		
		[Test]
		public void LinqQueriesShouldSupportComplexQueriesWithLongDatatypes ()
		{
			using (var session = new Session ()) {
				session.Add (new TestProduct { Name = "1", Quantity = 1, LongId = 1 });
				session.Add (new TestProduct { Name = "2", Quantity = 1, LongId = 2 });
				session.Add (new TestProduct { Name = "3", Quantity = 0, LongId = 3 });
				var queryable = session.Products;
				
				var test1 = (from t in queryable
					where t.LongId == 3
					select t).FirstOrDefault ();
				Assert.NotNull (test1);
				
				var test2 = (from t in queryable
					where (t.Quantity & 1) == 0
					select t).FirstOrDefault ();
				Assert.NotNull (test2);
				
				var test3 = (from t in queryable
					where t.LongId == 3 && ((t.Quantity & 1) == 0)
					select t).FirstOrDefault ();
				Assert.NotNull (test3);
				
				//fails
				Assert.True(queryable.QueryStructure ().IsComplex);
			}
		}


        [Test]
        public void ProviderDoesntChokeOnCustomCollectionNames()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var collname = "acmePost";
                var coll = db.GetCollection<Post>(collname);
                coll.Insert(new Post { Title = "a" }, new Post { Title = "b" }, new Post { Title = "c" });
                var results = coll.AsQueryable().Where(y => y.Title == "c").ToArray();

                Assert.AreEqual("c", results.ElementAt(0).Title);

            }

        }

        [Test]
        public void ProviderSupportsProjection()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var coll = db.GetCollection<TestProduct>();

                coll.Insert(new TestProduct { Available = DateTime.Now }, new TestProduct { Available = DateTime.Now });

                var results = db.GetCollection<TestProduct>().AsQueryable()
                    .Select(y => new { y.Available, y._id }).ToArray();

                Assert.AreEqual(2, results.Length);
                Assert.AreEqual((new { Available = DateTime.Now, _id = ObjectId.Empty }).GetType(),
                    results[0].GetType());
            }
        }

        [Test]
        public void ProviderSupportsSophisticatedProjection()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var coll = db.GetCollection<TestProduct>();

                coll.Insert(new TestProduct { Available = DateTime.Now }, new TestProduct { Available = DateTime.Now });

                var results = db.GetCollection<TestProduct>().AsQueryable()
                    .Select(y => new { Avail = y.Available, Id = y._id }).ToArray();

                Assert.AreEqual(2, results.Length);
                Assert.AreEqual((new { Avail = DateTime.Now, Id = ObjectId.Empty }).GetType(),
                    results[0].GetType());
            }
        }

        [Test]
        public void ProviderSupportsSophisticatedProjectionWithConcreteType()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var coll = db.GetCollection<TestProduct>();

                coll.Insert(new TestProduct { Name = "AAA", Price = 10 }, new TestProduct { Name = "BBB", Price = 20 });

                var results = db.GetCollection<TestProduct>().AsQueryable()
                    .Select(y => new TestProductSummary { _id = y._id, Name = y.Name, Price = y.Price }).ToArray();

                Assert.AreEqual(2, results.Length);
                Assert.AreEqual((new TestProductSummary()).GetType(), results[0].GetType());
            }
        }

        [Test]
        public void ProviderSupportsProjectionWithFirst()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var coll = db.GetCollection<TestProduct>();

                coll.Insert(new TestProduct { Available = DateTime.Now }, new TestProduct { Available = DateTime.Now });

                var result = db.GetCollection<TestProduct>().AsQueryable()
                    .Select(y => new { y.Available, y._id }).First();

                Assert.AreNotEqual(null, result);
            }
        }

        [Test]
        public void ProviderSupportsProjectionWithSingle()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var coll = db.GetCollection<TestProduct>();

                coll.Insert(new TestProduct { Available = DateTime.Now }, new TestProduct { Available = DateTime.Now });

                var result = db.GetCollection<TestProduct>().AsQueryable()
                    .Select(y => new { y.Available, y._id }).Single();

                Assert.AreNotEqual(null, result);
            }
        }

        [Test]
        public void ProviderSupportsProjectionWithSingleOfDefault()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var coll = db.GetCollection<TestProduct>();

                coll.Insert(new TestProduct { Available = DateTime.Now }, new TestProduct { Available = DateTime.Now });

                var result = db.GetCollection<TestProduct>().AsQueryable()
                    .Select(y => new { y.Available, y._id }).SingleOrDefault();

                Assert.AreNotEqual(null, result);
            }
        }

        [Test]
        public void ProviderSupportsProjectionWithFirstOfDefault()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var coll = db.GetCollection<TestProduct>();

                coll.Insert(new TestProduct { Available = DateTime.Now }, new TestProduct { Available = DateTime.Now });

                var result = db.GetCollection<TestProduct>().AsQueryable()
                    .Select(y => new { y.Available, y._id }).FirstOrDefault();

                Assert.AreNotEqual(null, result);
            }
        }

        [Test]
        public void ProviderSupportsProjectionInAnyOrderWithWhere()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Price = 22, Name = "AA" });
                session.Add(new TestProduct { Price = 11, Name = "BB" });

                var queryable = session.Products;

                var results = queryable
                    .Select(y => new { y.Price })
                    .Where(x => x.Price == 22)
                    .ToArray();

                Assert.AreEqual(22, results[0].Price);
                var stucture = queryable.QueryStructure();
                Assert.AreEqual(false, stucture.IsComplex);

            }
        }

        [Test]
        public void ProviderSupportsProjectionInAnyOrderWithWhereFirst()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Price = 22, Name = "AA" });
                session.Add(new TestProduct { Price = 11, Name = "BB" });

                var queryable = session.Products;

                var results = queryable
                    .Where(x => x.Name == "AA")
                    .Select(y => new { y.Price })
                    .ToArray();

                Assert.AreEqual(22, results[0].Price);
                var stucture = queryable.QueryStructure();
                Assert.AreEqual(false, stucture.IsComplex);

            }
        }

        [Test]
        public void LinqQueriesShouldSupportExternalParameters()
        {
            var external = 10;
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "test1", Price = 20 });
                session.Add(new TestProduct { Name = "test", Price = 10 });
                var queryable = session.Products;
                var product = queryable.Where(p => p.Price == external).FirstOrDefault();
                Assert.AreEqual(10, product.Price);

                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportNulls()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = null, Price = 20 });
                session.Add(new TestProduct { Name = "test1", Price = 10 });
                var queryable = session.Products; var products = queryable.Where(p => p.Name == null).ToList();
                Assert.AreEqual(20, products[0].Price);
                Assert.AreEqual(1, products.Count);

                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportIsNotNulls()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = null, Price = 20 });
                session.Add(new TestProduct { Name = "test1", Price = 10 });
                var queryable = session.Products; var products = queryable.Where(p => p.Name != null).ToList();
                Assert.AreEqual(10, products[0].Price);
                Assert.AreEqual(1, products.Count);

                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportRegex()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = null, Price = 20 });
                session.Add(new TestProduct { Name = "test1", Price = 10 });
                var queryable = session.Products; var products = queryable.Where(p => Regex.IsMatch(p.Name, "test1")).ToList();
                Assert.AreEqual(10, products[0].Price);
                Assert.AreEqual(1, products.Count);

                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportRegexWithMultipleWhereClause()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = null, Price = 20 });
                session.Add(new TestProduct { Name = "test1", Price = 10 });
                var queryable = session.Products; var products = queryable.Where(p => Regex.IsMatch(p.Name, "^te") && p.Price == 10).ToList();
                Assert.AreEqual(10, products[0].Price);
                Assert.AreEqual(1, products.Count);

                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportRegexWithOptions()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = null, Price = 20 });
                session.Add(new TestProduct { Name = "test1", Price = 10 });
                var queryable = session.Products; var products = queryable.Where(p => Regex.IsMatch(p.Name, "TEST1", RegexOptions.Multiline | RegexOptions.IgnoreCase)).ToList();
                Assert.AreEqual(10, products[0].Price);
                Assert.AreEqual(1, products.Count);

                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportRegexWithOptionsInComplexQuery()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = null, Price = 20 });
                session.Add(new TestProduct { Name = "test1", Price = 10 });
                var queryable = session.Products; var products = queryable.Where(p => Regex.IsMatch(p.Name, "TEST1", RegexOptions.Multiline | RegexOptions.IgnoreCase) && p.Name.StartsWith("tes")).ToList();
                Assert.AreEqual(10, products[0].Price);
                Assert.AreEqual(1, products.Count);

                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportMultiplication()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.Price * 2 == 20).ToList();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual("1", list[0].Name);

                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportBooleans()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "2", Price = 20, IsAvailable = true });
                session.Add(new TestProduct { Name = "1", Price = 10, IsAvailable = false });
                session.Add(new TestProduct { Name = "3", Price = 30, IsAvailable = true });

                var queryable = session.Products; var list = queryable.Where(x => x.IsAvailable).ToList();

                Assert.AreEqual(2, list.Count);
                Assert.AreEqual(50, list.Sum(x => x.Price));

                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportBooleansWithNegation()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "2", Price = 20, IsAvailable = true });
                session.Add(new TestProduct { Name = "1", Price = 10, IsAvailable = false });
                session.Add(new TestProduct { Name = "3", Price = 30, IsAvailable = true });

                var queryable = session.Products; var list = queryable.Where(x => !x.IsAvailable).ToList();

                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(10, list[0].Price);

                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportBooleansExplicitly()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "2", Price = 20, IsAvailable = true });
                session.Add(new TestProduct { Name = "1", Price = 10, IsAvailable = false });
                session.Add(new TestProduct { Name = "3", Price = 30, IsAvailable = true });

                var queryable = session.Products; var list = queryable.Where(x => x.IsAvailable == true).ToList();

                Assert.AreEqual(2, list.Count);
                Assert.AreEqual(50, list.Sum(x => x.Price));

                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportBooleansInComplexQuery()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "2", Price = 20, IsAvailable = true });
                session.Add(new TestProduct { Name = "1", Price = 10, IsAvailable = false, IsStillAvailable = true });
                session.Add(new TestProduct { Name = "3", Price = 30, IsAvailable = true });

                var queryable = session.Products; var list = queryable.Where(x => !x.IsAvailable && (!x.IsAvailable || x.IsStillAvailable)).ToList();

                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(10, list[0].Price);

                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportMultipleBooleans()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "2", Price = 20, IsAvailable = true });
                session.Add(new TestProduct { Name = "1", Price = 10, IsAvailable = false });
                session.Add(new TestProduct { Name = "3", Price = 30, IsAvailable = true, IsStillAvailable = true });
                session.Add(new TestProduct { Name = "4", Price = 40 });

                var queryable = session.Products; var list = queryable.Where(x => x.IsAvailable && x.IsStillAvailable).ToList();

                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(30, list[0].Price);

                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportMultipleBooleansWithNegation()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "2", Price = 20, IsAvailable = true });
                session.Add(new TestProduct { Name = "1", Price = 10, IsAvailable = false });
                session.Add(new TestProduct { Name = "3", Price = 30, IsAvailable = true, IsStillAvailable = true });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.IsAvailable && !x.IsStillAvailable).ToList();

                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(20, list[0].Price);

                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportBitwiseOr()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "1", Price = 10, Inventory = new List<InventoryChange>() { new InventoryChange() } });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => (x.Inventory.Count() | 2) == 3).ToList();
                Assert.AreEqual(1, list.Count);

                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportBitwiseAnd()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10, Inventory = new List<InventoryChange>() { new InventoryChange() } });
                session.Add(new TestProduct { Name = "2", Price = 20, Inventory = new List<InventoryChange>() { new InventoryChange() } });
                session.Add(new TestProduct
                {
                    Name = "3",
                    Price = 30,
                    Inventory = new List<InventoryChange>() {
                    new InventoryChange(), new InventoryChange() }
                });

                var queryable = session.Products; var list = queryable.Where(x => (x.Inventory.Count() & 2) == 2).ToList();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);

                list = session.Products.Where(x => (x.Inventory.Count() & 1) == 1).ToList();
                Assert.AreEqual(2, list.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportNativeComparisonWithInt()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Quantity = 10 });
                session.Add(new TestProduct { Name = "2", Quantity = 20, Price = 10 });
                session.Add(new TestProduct { Name = "3", Quantity = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.Quantity >= 20 && x.Price == 10).ToList();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(20, list[0].Quantity);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);

                list = session.Products.Where(x => x.Quantity <= 20 && x.Price == 10).ToList();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(20, list[0].Quantity);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);

                list = session.Products.Where(x => x.Quantity < 20).ToList();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(10, list[0].Quantity);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);

                list = session.Products.Where(x => x.Quantity > 20).ToList();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(30, list[0].Quantity);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportContainsWithComplexQueryWithNoList()
        {
            using (var session = new Session())
            {
                var names = new List<string>();

                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products; var result = queryable.Where(x => names.Contains(x.Name) || x.Name == "7").ToList();
                Assert.AreEqual(0, result.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportNativeContainsQueryUsingDollarInWithNoList()
        {
            using (var session = new Session())
            {
                var names = new List<string>();

                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products; var result = queryable.Where(x => names.Contains(x.Name)).ToList();
                Assert.AreEqual(0, result.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportNativeContainsQueryUsingDollarIn()
        {
            using (var session = new Session())
            {
                var names = new List<string>();
                names.Add("1");
                names.Add("2");

                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products; var result = queryable.Where(x => names.Contains(x.Name)).ToList();
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportNativeContainsQueryComplexQuery()
        {
            using (var session = new Session())
            {
                var names = new List<string>();
                names.Add("1");
                names.Add("2");

                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products; var result = queryable.Where(x => names.Contains(x.Name) || x.Name == "3").ToList();
                Assert.AreEqual(3, result.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportDateTime()
        {
            using (var session = new Session())
            {
                var date = new DateTime(2010, 3, 1, 15, 33, 33);
                var datelater = new DateTime(2010, 3, 1, 15, 33, 34);

                session.Add(new TestProduct { Name = "1", Price = 10, Available = datelater });
                session.Add(new TestProduct { Name = "2", Price = 20, Available = date });
                session.Add(new TestProduct { Name = "3", Price = 30, Available = date });

                var queryable = session.Products; var list = queryable.Where(x => x.Available > date).ToList();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(10, list[0].Price);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);

                var datefromdb = list[0].Available.ToLocalTime();

                Assert.AreEqual(datelater.Year, datefromdb.Year);
                Assert.AreEqual(datelater.Month, datefromdb.Month);
                Assert.AreEqual(datelater.Day, datefromdb.Day);
                Assert.AreEqual(datelater.Hour, datefromdb.Hour);
                Assert.AreEqual(datelater.Minute, datefromdb.Minute);
                Assert.AreEqual(datelater.Second, datefromdb.Second);

            }
        }

        [Test]
        public void LinqQueriesShouldSupportDateTimeInComplexQuery()
        {
            using (var session = new Session())
            {
                var date = new DateTime(2010, 3, 1, 15, 33, 33);

                session.Add(new TestProduct { Name = "1", Price = 10, Available = date });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.Available == date || x.Price == 13).ToList();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(10, list[0].Price);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);

                var datefromdb = list[0].Available.ToLocalTime();

                Assert.AreEqual(date.Year, datefromdb.Year);
                Assert.AreEqual(date.Month, datefromdb.Month);
                Assert.AreEqual(date.Day, datefromdb.Day);
                Assert.AreEqual(date.Hour, datefromdb.Hour);
                Assert.AreEqual(date.Minute, datefromdb.Minute);
                Assert.AreEqual(date.Second, datefromdb.Second);

            }
        }

        [Test]
        public void LinqQueriesShouldSupportDateTimeNested()
        {
            using (var session = new Session())
            {
                var date = new DateTime(2010, 3, 1, 15, 33, 33);

                session.Add(new TestProduct { Name = "1", Price = 10, Available = date });
                session.Add(new TestProduct { Name = "2", Price = 20, Supplier = new Supplier { CreatedOn = date } });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.Supplier.CreatedOn == date || x.Price == 13).ToList();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(20, list[0].Price);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);

                var datefromdb = list[0].Supplier.CreatedOn.ToLocalTime();

                Assert.AreEqual(date.Year, datefromdb.Year);
                Assert.AreEqual(date.Month, datefromdb.Month);
                Assert.AreEqual(date.Day, datefromdb.Day);
                Assert.AreEqual(date.Hour, datefromdb.Hour);
                Assert.AreEqual(date.Minute, datefromdb.Minute);
                Assert.AreEqual(date.Second, datefromdb.Second);

            }
        }

        [Test]
        public void LinqQueriesShouldSupportDivision()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.Price / 2 == 10).ToList();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual("2", list[0].Name);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportAddition()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.Price + 2 == 32).ToList();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual("3", list[0].Name);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportSubtraction()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.Price - 6 == 24).ToList();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual("3", list[0].Name);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void ContainsShouldReturnBothItems()
        {
            using (var session = new Session())
            {
                var names = new List<string>();
                names.Add("1");
                names.Add("2");

                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products; var list = queryable.Where(x => names.Contains(x.Name)).ToList();
                Assert.AreEqual(2, list.Count);
                Assert.AreEqual(30, list.Sum(x => x.Price));
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportRegexInComplexQuery()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = null, Price = 20 });
                session.Add(new TestProduct { Name = "test1", Price = 10 });
                var queryable = session.Products; var products = queryable.Where(p => Regex.IsMatch(p.Name, "^te") && p.Name == "test1").ToList();
                Assert.AreEqual(10, products[0].Price);
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void SingleQualifierQueryIsExecutedAsANDQuery()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "test1", Price = 20 });
                session.Add(new TestProduct { Name = "test", Price = 10 });
                var queryable = session.Products;
                var product = queryable.Where(p => p.Price == 10).Single();
                Assert.AreEqual(10, product.Price);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }


        [Test]
        public void FirstQualifierQueryIsExecutedWithSort()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "test1", Price = 20 });
                session.Add(new TestProduct { Name = "test", Price = 10 });
                var queryable = session.Products; var product = queryable.OrderBy(x => x.Price).First();
                Assert.AreEqual(10, product.Price);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportExternalObjectProperties()
        {
            var external = new TestProduct { Price = 10 };
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "test", Price = 30 });
                session.Add(new TestProduct { Name = "test1", Price = external.Price });
                var queryable = session.Products;
                var product = queryable.Where(p => p.Price == external.Price)
                    .ToList();

                Assert.AreEqual(10, product[0].Price);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void LinqQueriesShouldSupportExternalObjectProperties2()
        {
            var another = new Supplier { Name = "test1" };
            var external = new TestProduct { Price = 10, Supplier = another };
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "test", Price = 30 });
                session.Add(new TestProduct { Name = "test1", Price = external.Price });
                var queryable = session.Products; var product = queryable
                      .Where(p => p.Name == external.Supplier.Name)
                      .SingleOrDefault();

                Assert.AreEqual(10, product.Price);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenThreeInDBWithChainedWhereComplexQuery()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 22 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Price > 10);
                var result = products.Where(x => x.Price < 30);
                result = result.Where(x => x.Name.Contains("2"));
                Assert.AreEqual(22, result.SingleOrDefault().Price);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenThreeInDBWithChainedWhere()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 22 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                var queryable = session.Products; var result = queryable.Where(x => x.Price < 30);
                result = result.Where(x => x.Name.Contains("2"));
                Assert.AreEqual(22, result.SingleOrDefault().Price);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void ThreeProductsShouldBeReturnedWhenThreeInDBOrderedByPrice()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 40 });
                session.Add(new TestProduct { Name = "2", Price = 22 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                var queryable = session.Products; var products = queryable.OrderBy(x => x.Price).ToList();
                Assert.AreEqual(22, products[0].Price);
                Assert.AreEqual(40, products[2].Price);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void ThreeProductsShouldBeReturnedWhenThreeInDBOrderedByDeepAlias()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 40, Supplier = new Supplier { Name = "1" } });
                session.Add(new TestProduct { Name = "2", Price = 22, Supplier = new Supplier { Name = "2" } });
                session.Add(new TestProduct { Name = "3", Price = 33, Supplier = new Supplier { Name = "3" } });
                var queryable = session.Products; var products = queryable.OrderBy(x => x.Supplier.Name).ToList();
                Assert.AreEqual(40, products[0].Price);
                Assert.AreEqual(33, products[2].Price);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void ThreeProductsShouldBeReturnedWhenThreeInDBOrderedByPriceThenByName()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 22 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                session.Add(new TestProduct { Name = "2", Price = 50 });
                session.Add(new TestProduct { Name = "1", Price = 50 });
                var queryable = session.Products; var products = queryable.OrderBy(x => x.Price).ThenBy(x => x.Name).ToList();
                Assert.AreEqual(10, products[0].Price);
                Assert.AreEqual(22, products[1].Price);
                Assert.AreEqual(33, products[2].Price);
                Assert.AreEqual(50, products[3].Price);
                Assert.AreEqual(50, products[4].Price);
                Assert.AreEqual("1", products[3].Name);
                Assert.AreEqual("2", products[4].Name);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void ThreeProductsShouldBeReturnedWhenThreeInDBOrderedByPriceThenByNameDescending()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 22 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                session.Add(new TestProduct { Name = "2", Price = 50 });
                session.Add(new TestProduct { Name = "1", Price = 50 });
                var queryable = session.Products; var products = queryable.OrderBy(x => x.Price).ThenByDescending(x => x.Name).ToList();
                Assert.AreEqual(10, products[0].Price);
                Assert.AreEqual(22, products[1].Price);
                Assert.AreEqual(33, products[2].Price);
                Assert.AreEqual(50, products[3].Price);
                Assert.AreEqual(50, products[4].Price);
                Assert.AreEqual("2", products[3].Name);
                Assert.AreEqual("1", products[4].Name);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductShouldBeReturnedWhenFiveInDBOrderedByPriceThenSkipThreeAndTakeOne()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 22 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                session.Add(new TestProduct { Name = "2", Price = 50 });
                session.Add(new TestProduct { Name = "1", Price = 50 });
                var queryable = session.Products; var products = queryable.OrderByDescending(x => x.Price).Skip(3).Take(1).ToList();
                Assert.AreEqual(22, products[0].Price);
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductShouldBeReturnedWhenFiveInDBOrderedByPriceAndWhereThenSkipTwoAndTakeOne()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 10 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                session.Add(new TestProduct { Name = "6", Price = 50 });
                session.Add(new TestProduct { Name = "5", Price = 50 });
                var queryable = session.Products; var products = queryable.OrderBy(x => x.Price).ThenByDescending(x => x.Name).Where(x => x.Price == 50).Skip(1).Take(1).ToList();
                Assert.AreEqual(50, products[0].Price);
                Assert.AreEqual("5", products[0].Name);
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void ThreeProductsShouldBeReturnedWhenThreeInDBOrderedDewscendingByPrice()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 22 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                var queryable = session.Products; var products = queryable.OrderByDescending(x => x.Price).ToList();
                Assert.AreEqual(33, products[0].Price);
                Assert.AreEqual(10, products[2].Price);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenToLower()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.ToLower() == "test2").ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(22, products[0].Price);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenToLowerInvariant()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.ToLowerInvariant() == "test2").ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(22, products[0].Price);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenToUpper()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.ToUpper() == "TEST3").ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(33, products[0].Price);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenToUpperInvariant()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.ToUpperInvariant() == "TEST3").ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(33, products[0].Price);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenToUpperAndContains()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test", Price = 10 });
                session.Add(new TestProduct { Name = "Test", Price = 22 });
                session.Add(new TestProduct { Name = "Test1", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.ToUpper().Contains("EST")).ToList();
                Assert.AreEqual(3, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void ThreeProductsShouldBeReturnedWhenSubstringUsedWithLength()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "TestName1", Price = 10 });
                session.Add(new TestProduct { Name = "TestName2", Price = 22 });
                session.Add(new TestProduct { Name = "TestName3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Substring(2, 4) == "stNa").ToList();
                Assert.AreEqual(3, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenSubstringUsedWithOutLength()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "TestName1", Price = 10 });
                session.Add(new TestProduct { Name = "TestName2", Price = 22 });
                session.Add(new TestProduct { Name = "TestName3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Substring(2) == "stName2").ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(22, products[0].Price);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void FourProductsShouldBeReturnedWhenStartsOrEndsWithX()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10 });
                session.Add(new TestProduct { Name = "Test4X", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                session.Add(new TestProduct { Name = "XTest3", Price = 10 });
                session.Add(new TestProduct { Name = "XTest4", Price = 22 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.StartsWith("X") || x.Name.EndsWith("X")).ToList();
                Assert.AreEqual(4, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceEqual10()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3", Price = 10 });
                session.Add(new TestProduct { Name = "Test4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Price == 10).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void TwoProductsShouldBeReturnedWhen3InDbWithPriceGreaterThan10()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3", Price = 10 });
                session.Add(new TestProduct { Name = "Test4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Price > 10).ToList();
                Assert.AreEqual(2, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceEqual10AndNameTest3()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3", Price = 10 });
                session.Add(new TestProduct { Name = "Test4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Price == 10 && x.Name == "Test3").ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceGreaterThan10LessThan30()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3", Price = 10 });
                session.Add(new TestProduct { Name = "Test4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Price > 10 && x.Price < 30).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceGreaterThan10LessThan40AndNot33()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3", Price = 10 });
                session.Add(new TestProduct { Name = "Test4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Price > 10 && x.Price < 40 && x.Price != 33).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(22, products[0].Price);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenAvailableDayIsFifth()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = new DateTime(2000, 2, 6) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available.Day == 5).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenAvailableDateIsSameAsRequired()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = new DateTime(2000, 2, 6) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available == new DateTime(2000, 2, 5)).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual("Test3X", products[0].Name);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenAvailableDayIsMonday()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = DateTime.Now });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(1) });
                session.Add(new TestProduct { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(2) });
                session.Add(new TestProduct { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(3) });
                session.Add(new TestProduct { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(4) });
                session.Add(new TestProduct { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(5) });
                session.Add(new TestProduct { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(6) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available.DayOfWeek == DayOfWeek.Monday).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenAvailableGreaterThanToday()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(1) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available > DateTime.Now).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenAvailableLessThanTodayPlus1()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(2) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available > DateTime.Now.AddDays(1)).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenAvailableMonthIs2()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = new DateTime(2001, 3, 6) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available.Month == 2).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenAvailableYearIs2000()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = new DateTime(2001, 2, 6) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available.Year == 2000).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenIndexOfXEqual2()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10 });
                session.Add(new TestProduct { Name = "TestX4", Price = 22 });
                session.Add(new TestProduct { Name = "TesXt5", Price = 33 });
                session.Add(new TestProduct { Name = "TeXst3", Price = 10 });
                session.Add(new TestProduct { Name = "TXest4", Price = 22 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.IndexOf("X") == 2).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenLastIndexOfXEqual6()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10 });
                session.Add(new TestProduct { Name = "TestX4", Price = 22 });
                session.Add(new TestProduct { Name = "TXest5X", Price = 33 });
                session.Add(new TestProduct { Name = "TeXst3", Price = 10 });
                session.Add(new TestProduct { Name = "TXest4", Price = 22 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.LastIndexOf("X") == 6).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(33, products[0].Price);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenReplaceOfXWithY()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10 });
                session.Add(new TestProduct { Name = "TestX4", Price = 22 });
                session.Add(new TestProduct { Name = "TXesXt5", Price = 33 });
                session.Add(new TestProduct { Name = "TeXst3", Price = 10 });
                session.Add(new TestProduct { Name = "TXest4", Price = 22 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Replace("X", "Y") == "TYesYt5").ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(33, products[0].Price);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenIsNullOrEmptyWithEmptyString()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10 });
                session.Add(new TestProduct { Name = "Test4X", Price = 22 });
                session.Add(new TestProduct { Name = "", Price = 33 });
                session.Add(new TestProduct { Name = "XTest3", Price = 10 });
                session.Add(new TestProduct { Name = "XTest4", Price = 22 });
                var queryable = session.Products; var products = queryable.Where(x => string.IsNullOrEmpty(x.Name)).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenIsNullOrEmptyWithNull()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10 });
                session.Add(new TestProduct { Name = "Test4X", Price = 22 });
                session.Add(new TestProduct { Price = 33 });
                session.Add(new TestProduct { Name = "XTest3", Price = 10 });
                session.Add(new TestProduct { Name = "XTest4", Price = 22 });
                var queryable = session.Products; var products = queryable.Where(x => string.IsNullOrEmpty(x.Name)).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenStartsAndEndsWithX()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10 });
                session.Add(new TestProduct { Name = "Test4X", Price = 22 });
                session.Add(new TestProduct { Name = "XTest5X", Price = 33 });
                session.Add(new TestProduct { Name = "XTest3", Price = 10 });
                session.Add(new TestProduct { Name = "XTest4", Price = 22 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.StartsWith("X") && x.Name.EndsWith("X")).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void ThreeProductsShouldBeReturnedWhen3InDbWithNoExpression()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var products = queryable.ToList();
                Assert.AreEqual(3, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void ThreeProductsShouldBeReturnedWhenAvailableLessThanToday()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(1) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available < DateTime.Now).ToList();
                Assert.AreEqual(3, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void TwoProductsShouldBeReturnedWhenNotEqualsOne()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var result = queryable.Where(x => x.Price != 22);
                Assert.AreEqual(2, result.Count());
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductShouldBeReturnedWhen3InDbWithFirst()
        {
            using (var session = new Session())
            {
                //session.CreateCappedCollection("Product"); //only capped collections return in insertion order
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var result = queryable.ToList();
                Assert.AreEqual(3, result.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductShouldBeReturnedWhen3InDbWithSingle()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var result = queryable.SingleOrDefault(x => x.Price == 22);
                Assert.AreEqual(22, result.Price);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductShouldBeReturnedUsingSimpleANDQuery()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 22 });
                var queryable = session.Products; var results = queryable.Where(x => x.Price == 22 && x.Name == "Test3").ToArray();
                Assert.AreEqual(1, results.Length);
                Assert.AreEqual(22, results[0].Price);
                Assert.AreEqual("Test3", results[0].Name);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductShouldBeReturnedWhen3InDbWithSingleUsingVariable()
        {
            using (var session = new Session())
            {
                var target = 22;
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var result = queryable.SingleOrDefault(x => x.Price == target);
                Assert.AreEqual(target, result.Price);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void TwoProductsShouldBeReturnedWhen3InDbWithPriceLessThan10OrGreaterThan30()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3", Price = 10 });
                session.Add(new TestProduct { Name = "Test4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Price > 10 || x.Price > 30).ToList();
                Assert.AreEqual(2, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void TwoProductsShouldBeReturnedWhenContainsX()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "TestX3", Price = 10 });
                session.Add(new TestProduct { Name = "TestX4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Contains("X")).ToList();
                Assert.AreEqual(2, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenContainsUsesRegexEscapeChar()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "TestX3", Price = 10 });
                session.Add(new TestProduct { Name = "Test+X4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Contains("+X")).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenReplaceContainsRegexEscapeChar()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "TestX3", Price = 10 });
                session.Add(new TestProduct { Name = "Test+X4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Replace("+X", "X") == "TestX4").ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void OneProductsShouldBeReturnedWhenContainsMatchesFirstCharacter()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "ATest", Price = 10 });
                session.Add(new TestProduct { Name = "BTest", Price = 22 });
                session.Add(new TestProduct { Name = "CTest", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Contains("B")).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void TwoProductsShouldBeReturnedWhenEndsWithX()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10 });
                session.Add(new TestProduct { Name = "Test4X", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });

                var queryable = session.Products; var products = queryable.Where(x => x.Name.EndsWith("X")).ToList();

                Assert.AreEqual(2, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void TwoProductsShouldBeReturnedWhenStartsWithX()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "XTest3", Price = 10 });
                session.Add(new TestProduct { Name = "XTest4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });

                var queryable = session.Products; var products = queryable.Where(x => x.Name.StartsWith("X")).ToList();

                Assert.AreEqual(2, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void TwoProductsShouldBeReturnedWhenStartsWithXWithQuoteComplex()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10 });
                session.Add(new TestProduct { Name = "Test4X", Price = 22 });
                session.Add(new TestProduct { Name = "X\"Test5X", Price = 33 });
                session.Add(new TestProduct { Name = "XTest3", Price = 10 });
                session.Add(new TestProduct { Name = "XTest4", Price = 22 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.StartsWith("X\"Test") && x.Name.EndsWith("X")).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(33, products[0].Price);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void TwoProductsShouldBeReturnedWhenEndsWithXWithQuote()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10 });
                session.Add(new TestProduct { Name = "Test4X", Price = 22 });
                session.Add(new TestProduct { Name = "XTest\"5X", Price = 33 });
                session.Add(new TestProduct { Name = "XTest3", Price = 10 });
                session.Add(new TestProduct { Name = "XTest4", Price = 22 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.EndsWith("\"5X")).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(33, products[0].Price);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void TwoProductsShouldBeReturnedWhenEndsWithXWithQuoteComplex()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10 });
                session.Add(new TestProduct { Name = "Test4X", Price = 22 });
                session.Add(new TestProduct { Name = "XTest\"5X", Price = 33 });
                session.Add(new TestProduct { Name = "XTest3", Price = 10 });
                session.Add(new TestProduct { Name = "XTest4", Price = 22 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.EndsWith("\"5X") && x.Name.StartsWith("X")).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(33, products[0].Price);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void TwoProductsofFourShouldBeReturnedWithSkipTake()
        {
            using (var session = new Session())
            {

                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                session.Add(new TestProduct { Name = "Test4", Price = 44 });
                var queryable = session.Products; var products = queryable.Skip(1).Take(2).ToList();
                Assert.AreEqual(22.0, products[0].Price);
                Assert.AreEqual(33.0, products[1].Price);
                Assert.AreEqual(2, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void TwoProductsofFourShouldBeReturnedWithSkipTakeAndWhere()
        {
            using (var session = new Session())
            {

                session.Add(new TestProduct { Name = "Test", Price = 10 });
                session.Add(new TestProduct { Name = "Test1", Price = 22 });
                session.Add(new TestProduct { Name = "Test", Price = 33 });
                session.Add(new TestProduct { Name = "Test", Price = 44 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name == "Test").Skip(1).Take(2).ToList();
                Assert.AreEqual(33, products[0].Price);
                Assert.AreEqual(44, products[1].Price);
                Assert.AreEqual(2, products.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void TwoProductsofFourShouldBeReturnedWhereLengthOfNameEquals4()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test", Price = 10 });
                session.Add(new TestProduct { Name = "Test1", Price = 22 });
                session.Add(new TestProduct { Name = "Test1", Price = 33 });
                session.Add(new TestProduct { Name = "Test", Price = 44 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Length == 4).ToList().OrderBy(x => x.Price).ToArray();
                Assert.AreEqual(10, products[0].Price);
                Assert.AreEqual(44, products[1].Price);
                Assert.AreEqual(2, products.Length);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void FiltersBasedOnObjectId()
        {
            var targetId = ObjectId.NewObjectId();
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10, _id = targetId });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(p => p._id == targetId).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(targetId, products[0]._id);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void FiltersBasedOnObjectIdInComplexQuery()
        {
            var targetId = ObjectId.NewObjectId();
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new TestProduct { Name = "Test2", Price = 22, Available = new DateTime(2000, 2, 6), _id = targetId });
                session.Add(new TestProduct { Name = "Test3", Price = 33, Available = new DateTime(2000, 2, 7) });
                var queryable = session.Products; var products = queryable.Where(p => p._id == targetId && p.Available.Day == 6).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(targetId, products[0]._id);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }
        [Test]
        public void FiltersBasedOnObjectIdExclusionInComplexQuery()
        {
            var targetId = ObjectId.NewObjectId();
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new TestProduct { Name = "Test2", Price = 22, Available = new DateTime(2000, 2, 5), _id = targetId });
                session.Add(new TestProduct { Name = "Test3", Price = 33, Available = new DateTime(2000, 2, 5) });
                var queryable = session.Products; var products = queryable.Where(p => p._id != targetId && p.Available.Day == 5).ToList();
                Assert.AreEqual(2, products.Count);
                Assert.AreNotEqual(targetId, products[0]._id);
                Assert.AreNotEqual(targetId, products[1]._id);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void FiltersBasedOnMagicObjectId()
        {
            var targetId = ObjectId.NewObjectId();
            using (var session = new Session())
            {
                session.Add(new Post { Id = targetId });
                session.Add(new Post());
                session.Add(new Post());
                var queryable = session.Posts; var posts = queryable.Where(p => p.Id == targetId).ToList();
                Assert.AreEqual(1, posts.Count);
                Assert.AreEqual(targetId, posts[0].Id);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void Filters_Based_On_Guid()
        {
            var targetId = Guid.NewGuid();
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10, UniqueID = targetId });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(p => p.UniqueID == targetId).ToList();
                Assert.AreEqual(1, products.Count);
                Assert.AreEqual(targetId, products[0].UniqueID);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void CanQueryWithinSimpleEmbeddedArray()
        {
            using (var session = new Session())
            {
                var post1 = new Post { Title = "First", Tags = new List<string> { "tag1", "tag2" } };
                var post2 = new Post { Title = "Second", Tags = new List<string> { "tag3", "tag4" } };

                session.Add(post1);
                session.Add(post2);

                var queryable = session.Posts;
                var found = queryable.Where(p => p.Tags.Any(x => x == "tag3")).SingleOrDefault();

                Assert.AreEqual("Second", found.Title);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void CanQueryWithinEmbeddedArrayUsingAny()
        {
            using (var session = new Session())
            {
                var post1 = new Post { Title = "First", Comments = new List<Comment> { new Comment { Text = "comment1" }, new Comment { Text = "comment2" } } };
                var post2 = new Post { Title = "Second", Comments = new List<Comment> { new Comment { Text = "commentA", Name = "name1" }, new Comment { Text = "commentB", Name = "name2" } } };

                session.Add(post1);
                session.Add(post2);

                var queryable = session.Posts;
                var found = queryable.Where(p => p.Comments.Any(x => x.Text == "commentA")).SingleOrDefault();

                Assert.AreEqual("Second", found.Title);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void CanQueryWithinEmbeddedArrayUsingAnyWithBool()
        {
            using (var session = new Session())
            {
                var post1 = new Post
                {
                    Title = "First",
                    Comments = new List<Comment> { 
                        new Comment { Text = "comment1", IsOld = false }, 
                        new Comment { Text = "comment2", IsOld = false }
                    }
                };
                var post2 = new Post
                {
                    Title = "Second",
                    Comments = new List<Comment> { 
                        new Comment { Text = "commentA", Name = "name1", IsOld = true }, 
                        new Comment { Text = "commentB", Name = "name2", IsOld = false } 
                    }
                };

                session.Add(post1);
                session.Add(post2);

                var queryable = session.Posts;
                var found = queryable.Where(p => p.Comments.Any(x => x.IsOld)).ToList();

                Assert.AreEqual(1, found.Count);
                Assert.AreEqual("Second", found[0].Title);

                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void CanQueryWithinEmbeddedArrayUsingAnyWithBoolNegated()
        {
            using (var session = new Session())
            {
                var tf = false;

                var post1 = new Post { Title = "First", Comments = new List<Comment> { new Comment { Text = "comment1", IsOld = false }, new Comment { Text = "comment2", IsOld = false } } };
                var post2 = new Post { Title = "Second", Comments = new List<Comment> { new Comment { Text = "commentA", Name = "name1", IsOld = true }, new Comment { Text = "commentB", Name = "name2", IsOld = true } } };

                session.Add(post1);
                session.Add(post2);

                var queryable = session.Posts;
                var found = queryable.Where(p => p.Comments.Any(x => x.IsOld == tf)).ToList();

                Assert.AreEqual(1, found.Count);
                Assert.AreEqual("First", found[0].Title);

                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void CanQueryWithinEmbeddedArrayUsingTwoAnys()
        {
            using (var session = new Session())
            {
                var post1 = new Post { Title = "First", Comments = new List<Comment> { new Comment { Text = "comment1" }, new Comment { Text = "comment2" } } };
                var post2 = new Post { Title = "Second", Comments = new List<Comment> { new Comment { Text = "commentA" }, new Comment { Text = "commentB", CommentTags = new List<Tag> { new Tag { TagName = "Cool" }, new Tag { TagName = "Yes" } } } } };

                session.Add(post1);
                session.Add(post2);
                var queryable = session.Posts;
                var found = queryable.Where(p => p.Comments.Any(x => x.CommentTags.Any(y => y.TagName == "Cool"))).SingleOrDefault();

                Assert.AreEqual("Second", found.Title);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void CanQueryWithinEmbeddedArrayUsingAnyWithNoParameters()
        {
            using (var session = new Session())
            {
                var post1 = new Post { Title = "First", Comments = new List<Comment>() };
                var post2 = new Post { Title = "Second", Comments = new List<Comment> { new Comment { Text = "commentA", Name = "name1" }, new Comment { Text = "commentB", Name = "name2" } } };

                session.Add(post1);
                session.Add(post2);

                var queryable = session.Posts;
                var found = queryable.Where(p => p.Comments.Any()).SingleOrDefault();

                Assert.AreEqual("Second", found.Title);
                Assert.AreEqual(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void CanQueryWithinEmbeddedArrayUsingArrayIdentifiers()
        {
            using (var session = new Session())
            {
                var post1 = new Post { Title = "First", Comments = new List<Comment> { new Comment { Text = "comment1" }, new Comment { Text = "comment2" } } };
                var post2 = new Post { Title = "Second", Comments = new List<Comment> { new Comment { Text = "commentA", Name = "name1" }, new Comment { Text = "commentB", Name = "name2" } } };

                session.Add(post1);
                session.Add(post2);

                var queryable = session.Posts;
                var found = queryable.Where(p => p.Comments[0].Text == "commentA").SingleOrDefault();
                Assert.AreEqual("Second", found.Title);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);

                queryable = session.Posts;
                found = queryable.Where(p => p.Comments[1].Text == "comment2").SingleOrDefault();
                Assert.AreEqual("First", found.Title);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void MapReduceMax()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "ATest", Price = 10 });
                session.Add(new TestProduct { Name = "BTest", Price = 22 });
                session.Add(new TestProduct { Name = "CTest", Price = 33 });
                var queryable = session.Products;
                var productMax = queryable.Max(x => x.Price);
                Assert.AreEqual(33, productMax);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void MapReduceMaxDeepQuery()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "ATest", Price = 10, Supplier = new Supplier { RefNum = 3 } });
                session.Add(new TestProduct { Name = "BTest", Price = 22, Supplier = new Supplier { RefNum = 2 } });
                session.Add(new TestProduct { Name = "CTest", Price = 33, Supplier = new Supplier { RefNum = 1 } });
                var queryable = session.Products;
                var productMax = queryable.Max(x => x.Supplier.RefNum);
                Assert.AreEqual(3, productMax);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void MapReduceWhereWithMax()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "ATest", Price = 10 });
                session.Add(new TestProduct { Name = "BTest", Price = 22 });
                session.Add(new TestProduct { Name = "BTest", Price = 33 });
                var queryable = session.Products;
                var productMax = queryable.Where(x => x.Name == "BTest").Max(x => x.Price);
                Assert.AreEqual(33, productMax);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void MapReduceWhereWithMin()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "ATest", Price = 10 });
                session.Add(new TestProduct { Name = "BTest", Price = 22 });
                session.Add(new TestProduct { Name = "BTest", Price = 33 });
                var queryable = session.Products;
                var productMax = queryable.Where(x => x.Name == "BTest").Min(x => x.Price);
                Assert.AreEqual(22, productMax);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void FirstWhereNoResultsReturnedInWhere ()
        {
        	using (var session = new Session (true))
            {
        		session.Add (new TestProduct { Name = "ATest", Price = 10 });
        		session.Add (new TestProduct { Name = "BTest", Price = 22 });
        		session.Add (new TestProduct { Name = "BTest", Price = 33 });
				
                var noProducct = session.Products.Where(x => x.Name == "ZTest");
                var ex = Assert.Throws<InvalidOperationException>(() => noProducct.First());
                Assert.IsTrue(("Sequence contains no elements" == ex.Message) || ("The source sequence is empty" == ex.Message));
            }
		}

        [Test]
        public void FirstOrDefaultWhereNoResultsReturnedInWhere()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "ATest", Price = 10 });
                session.Add(new TestProduct { Name = "BTest", Price = 22 });
                session.Add(new TestProduct { Name = "BTest", Price = 33 });
                var queryable = session.Products;
                var noProducct = queryable.Where(x => x.Name == "ZTest").FirstOrDefault();
                Assert.AreEqual(null, noProducct);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }


        [Test]
        public void CanQueryAndReturnSubClassedObjects()
        {
            using (var session = new Session())
            {
                session.Drop<SuperClassObject>();

                session.Add<SuperClassObject>(new SubClassedObject { Title = "Find This", ABool = true });
                session.Add<SuperClassObject>(new SubClassedObject { Title = "Don't Find This", ABool = false });

                var queryable = session.DB.GetCollection<SuperClassObject>().AsQueryable();

                var dtos = queryable.Where(dto => dto.Title == "Find This").ToList();

                Assert.AreEqual(1, dtos.Count);
                Assert.AreEqual("Find This", dtos[0].Title);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void CanQueryAndReturnSubClassedObjects_EvenWhenAddedBySubClass()
        {
            using (var session = new Session())
            {
                session.Drop<SuperClassObject>();

                session.Add(new SubClassedObject());

                var queryable = session.DB.GetCollection<SuperClassObject>().AsQueryable();

                var dtos = queryable.ToList();

                Assert.AreEqual(1, dtos.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void CanQueryAndReturnSubClassedObjects_EvenWhenQueriedBySubClass()
        {
            using (var session = new Session())
            {
                session.Drop<SuperClassObject>();

                session.Add(new SubClassedObject());

                var queryable = session.DB.GetCollection<SubClassedObject>().AsQueryable();

                var dtos = queryable.ToList();

                Assert.AreEqual(1, dtos.Count);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void CanQueryAndReturnSubClassedObjects_EvenWhenQueriedByInterface()
        {
            using (var session = new Session())
            {
                session.Drop<IDiscriminated>();

                var obj = new InterfaceDiscriminatedClass();
                session.Add(obj);

                var queryable = session.DB.GetCollection<IDiscriminated>().AsQueryable();
                var dtos = queryable.ToList();

                Assert.AreEqual(1, dtos.Count);
                Assert.AreEqual(obj.Id, dtos.Single().Id);
                Assert.AreEqual(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Test]
        public void CanAQuerySupportArrayIdentifiers()
        {
            MongoConfiguration.Initialize(c => c.AddMap<ShopperMap>());
            using (var shoppers = new Shoppers(Mongo.Create(TestHelper.ConnectionString("pooling=false","test",null,null))))
            {
                shoppers.Drop<Shopper>();
                shoppers.Add(new Shopper
                {
                    Id = ObjectId.NewObjectId(),
                    Name = "John",
                    Cart = new Cart
                    {
                        Id = ObjectId.NewObjectId(),
                        Name = "Cart1",
                        CartSuppliers = new[] { new Supplier { Name = "Supplier1" }, new Supplier { Name = "Supplier2" } }
                    }
                });

                shoppers.Add(new Shopper
                {
                    Id = ObjectId.NewObjectId(),
                    Name = "Jane",
                    Cart = new Cart
                    {
                        Id = ObjectId.NewObjectId(),
                        Name = "Cart2",
                        CartSuppliers = new[] { new Supplier { Name = "Supplier3" }, new Supplier { Name = "Supplier4" } }
                    }
                });

                var deepQuery = shoppers.Where(x => x.Cart.CartSuppliers.Any(y => y.Name == "Supplier4")).ToList();
                Assert.AreEqual("Jane", deepQuery[0].Name);
                Assert.AreEqual("Cart2", deepQuery[0].Cart.Name);
                Assert.AreEqual(1, deepQuery.Count);
                Assert.AreEqual(false, (shoppers.Provider as IMongoQueryResults).TranslationResults.IsComplex);
            }
        }
    }
}