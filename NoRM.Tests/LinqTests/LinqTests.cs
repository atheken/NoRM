using System;
using System.Collections.Generic;
using System.Linq;
using Norm.Linq;
using Xunit;
using Norm.Configuration;
using System.Text.RegularExpressions;
using Norm.Tests.Helpers;

namespace Norm.Tests
{
    public class LinqTests
    {
        public LinqTests()
        {
            MongoConfiguration.RemoveMapFor<TestProduct>();
            MongoConfiguration.RemoveMapFor<Post>();
            using (var session = new Session())
            {
                session.Drop<TestProduct>();
                session.Drop<Post>();
            }
        }

        [Fact]
        public void ProviderSupportsProjection()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var coll = db.GetCollection<TestProduct>();

                coll.Insert(new TestProduct { Available = DateTime.Now }, new TestProduct { Available = DateTime.Now });

                var results = db.GetCollection<TestProduct>().AsQueryable()
                    .Select(y => new { y.Available, y._id }).ToArray();

                Assert.Equal(2, results.Length);
                Assert.Equal((new { Available = DateTime.Now, _id = ObjectId.Empty }).GetType(),
                    results[0].GetType());
            }
        }

        [Fact]
        public void ProviderSupportsSophisticatedProjection()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var coll = db.GetCollection<TestProduct>();

                coll.Insert(new TestProduct { Available = DateTime.Now }, new TestProduct { Available = DateTime.Now });

                var results = db.GetCollection<TestProduct>().AsQueryable()
                    .Select(y => new { Avail = y.Available, Id = y._id }).ToArray();

                Assert.Equal(2, results.Length);
                Assert.Equal((new {Avail = DateTime.Now, Id = ObjectId.Empty }).GetType(),
                    results[0].GetType());
            }
        }

        [Fact]
        public void ProviderSupportsProjectionWithFirst()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var coll = db.GetCollection<TestProduct>();

                coll.Insert(new TestProduct { Available = DateTime.Now }, new TestProduct { Available = DateTime.Now });

                var result = db.GetCollection<TestProduct>().AsQueryable()
                    .Select(y => new { y.Available, y._id }).First();

                Assert.NotEqual(null, result);
            }
        }

        [Fact]
        public void ProviderSupportsProjectionWithSingle()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var coll = db.GetCollection<TestProduct>();

                coll.Insert(new TestProduct { Available = DateTime.Now }, new TestProduct { Available = DateTime.Now });

                var result = db.GetCollection<TestProduct>().AsQueryable()
                    .Select(y => new { y.Available, y._id }).Single();

                Assert.NotEqual(null, result);
            }
        }

        [Fact]
        public void ProviderSupportsProjectionWithSingleOfDefault()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var coll = db.GetCollection<TestProduct>();

                coll.Insert(new TestProduct { Available = DateTime.Now }, new TestProduct { Available = DateTime.Now });

                var result = db.GetCollection<TestProduct>().AsQueryable()
                    .Select(y => new { y.Available, y._id }).SingleOrDefault();

                Assert.NotEqual(null, result);
            }
        }

        [Fact]
        public void ProviderSupportsProjectionWithFirstOfDefault()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var coll = db.GetCollection<TestProduct>();

                coll.Insert(new TestProduct { Available = DateTime.Now }, new TestProduct { Available = DateTime.Now });

                var result = db.GetCollection<TestProduct>().AsQueryable()
                    .Select(y => new { y.Available, y._id }).FirstOrDefault();

                Assert.NotEqual(null, result);
            }
        }

        [Fact]
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

                Assert.Equal(22, results[0].Price);
                var stucture = queryable.QueryStructure();
                Assert.Equal(false, stucture.IsComplex);

            }
        }

        [Fact]
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

                Assert.Equal(22, results[0].Price);
                var stucture = queryable.QueryStructure();
                Assert.Equal(false, stucture.IsComplex);

            }
        }

        [Fact]
        public void LinqQueriesShouldSupportExternalParameters()
        {
            var external = 10;
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "test1", Price = 20 });
                session.Add(new TestProduct { Name = "test", Price = 10 });
                var queryable = session.Products;
                var product = queryable.Where(p => p.Price == external).FirstOrDefault();
                Assert.Equal(10, product.Price);

                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportNulls()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = null, Price = 20 });
                session.Add(new TestProduct { Name = "test1", Price = 10 });
                var queryable = session.Products; var products = queryable.Where(p => p.Name == null).ToList();
                Assert.Equal(20, products[0].Price);
                Assert.Equal(1, products.Count);

                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportIsNotNulls()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = null, Price = 20 });
                session.Add(new TestProduct { Name = "test1", Price = 10 });
                var queryable = session.Products; var products = queryable.Where(p => p.Name != null).ToList();
                Assert.Equal(10, products[0].Price);
                Assert.Equal(1, products.Count);

                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportRegex()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = null, Price = 20 });
                session.Add(new TestProduct { Name = "test1", Price = 10 });
                var queryable = session.Products; var products = queryable.Where(p => Regex.IsMatch(p.Name, "test1")).ToList();
                Assert.Equal(10, products[0].Price);
                Assert.Equal(1, products.Count);

                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportRegexWithMultipleWhereClause()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = null, Price = 20 });
                session.Add(new TestProduct { Name = "test1", Price = 10 });
                var queryable = session.Products; var products = queryable.Where(p => Regex.IsMatch(p.Name, "^te") && p.Price == 10).ToList();
                Assert.Equal(10, products[0].Price);
                Assert.Equal(1, products.Count);

                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportRegexWithOptions()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = null, Price = 20 });
                session.Add(new TestProduct { Name = "test1", Price = 10 });
                var queryable = session.Products; var products = queryable.Where(p => Regex.IsMatch(p.Name, "TEST1", RegexOptions.Multiline | RegexOptions.IgnoreCase)).ToList();
                Assert.Equal(10, products[0].Price);
                Assert.Equal(1, products.Count);

                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportRegexWithOptionsInComplexQuery()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = null, Price = 20 });
                session.Add(new TestProduct { Name = "test1", Price = 10 });
                var queryable = session.Products; var products = queryable.Where(p => Regex.IsMatch(p.Name, "TEST1", RegexOptions.Multiline | RegexOptions.IgnoreCase) && p.Name.StartsWith("tes")).ToList();
                Assert.Equal(10, products[0].Price);
                Assert.Equal(1, products.Count);

                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportMultiplication()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.Price * 2 == 20).ToList();
                Assert.Equal(1, list.Count);
                Assert.Equal("1", list[0].Name);

                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportBooleans()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "2", Price = 20, IsAvailable = true });
                session.Add(new TestProduct { Name = "1", Price = 10, IsAvailable = false });
                session.Add(new TestProduct { Name = "3", Price = 30, IsAvailable = true });

                var queryable = session.Products; var list = queryable.Where(x => x.IsAvailable).ToList();

                Assert.Equal(2, list.Count);
                Assert.Equal(50, list.Sum(x => x.Price));

                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportBooleansWithNegation()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "2", Price = 20, IsAvailable = true });
                session.Add(new TestProduct { Name = "1", Price = 10, IsAvailable = false });
                session.Add(new TestProduct { Name = "3", Price = 30, IsAvailable = true });

                var queryable = session.Products; var list = queryable.Where(x => !x.IsAvailable).ToList();

                Assert.Equal(1, list.Count);
                Assert.Equal(10, list[0].Price);

                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportBooleansExplicitly()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "2", Price = 20, IsAvailable = true });
                session.Add(new TestProduct { Name = "1", Price = 10, IsAvailable = false });
                session.Add(new TestProduct { Name = "3", Price = 30, IsAvailable = true });

                var queryable = session.Products; var list = queryable.Where(x => x.IsAvailable == true).ToList();

                Assert.Equal(2, list.Count);
                Assert.Equal(50, list.Sum(x => x.Price));

                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportBooleansInComplexQuery()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "2", Price = 20, IsAvailable = true });
                session.Add(new TestProduct { Name = "1", Price = 10, IsAvailable = false, IsStillAvailable = true });
                session.Add(new TestProduct { Name = "3", Price = 30, IsAvailable = true });

                var queryable = session.Products; var list = queryable.Where(x => !x.IsAvailable && (!x.IsAvailable || x.IsStillAvailable)).ToList();

                Assert.Equal(1, list.Count);
                Assert.Equal(10, list[0].Price);

                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportMultipleBooleans()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "2", Price = 20, IsAvailable = true });
                session.Add(new TestProduct { Name = "1", Price = 10, IsAvailable = false });
                session.Add(new TestProduct { Name = "3", Price = 30, IsAvailable = true, IsStillAvailable = true });
                session.Add(new TestProduct { Name = "4", Price = 40 });

                var queryable = session.Products; var list = queryable.Where(x => x.IsAvailable && x.IsStillAvailable).ToList();

                Assert.Equal(1, list.Count);
                Assert.Equal(30, list[0].Price);

                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportMultipleBooleansWithNegation()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "2", Price = 20, IsAvailable = true });
                session.Add(new TestProduct { Name = "1", Price = 10, IsAvailable = false });
                session.Add(new TestProduct { Name = "3", Price = 30, IsAvailable = true, IsStillAvailable = true });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.IsAvailable && !x.IsStillAvailable).ToList();

                Assert.Equal(1, list.Count);
                Assert.Equal(20, list[0].Price);

                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportBitwiseOr()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "1", Price = 10, Inventory = new List<InventoryChange>() { new InventoryChange() } });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => (x.Inventory.Count() | 2) == 3).ToList();
                Assert.Equal(1, list.Count);

                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(1, list.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);

                list = session.Products.Where(x => (x.Inventory.Count() & 1) == 1).ToList();
                Assert.Equal(2, list.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportNativeComparisonWithInt()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Quantity = 10 });
                session.Add(new TestProduct { Name = "2", Quantity = 20, Price = 10 });
                session.Add(new TestProduct { Name = "3", Quantity = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.Quantity >= 20 && x.Price == 10).ToList();
                Assert.Equal(1, list.Count);
                Assert.Equal(20, list[0].Quantity);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);

                list = session.Products.Where(x => x.Quantity <= 20 && x.Price == 10).ToList();
                Assert.Equal(1, list.Count);
                Assert.Equal(20, list[0].Quantity);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);

                list = session.Products.Where(x => x.Quantity < 20).ToList();
                Assert.Equal(1, list.Count);
                Assert.Equal(10, list[0].Quantity);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);

                list = session.Products.Where(x => x.Quantity > 20).ToList();
                Assert.Equal(1, list.Count);
                Assert.Equal(30, list[0].Quantity);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportContainsWithComplexQueryWithNoList()
        {
            using (var session = new Session())
            {
                var names = new List<string>();

                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products; var result = queryable.Where(x => names.Contains(x.Name) || x.Name == "7").ToList();
                Assert.Equal(0, result.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportNativeContainsQueryUsingDollarInWithNoList()
        {
            using (var session = new Session())
            {
                var names = new List<string>();

                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });
                var queryable = session.Products; var result = queryable.Where(x => names.Contains(x.Name)).ToList();
                Assert.Equal(0, result.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(2, result.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(3, result.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(1, list.Count);
                Assert.Equal(10, list[0].Price);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);

                var datefromdb = list[0].Available.ToLocalTime();

                Assert.Equal(datelater.Year, datefromdb.Year);
                Assert.Equal(datelater.Month, datefromdb.Month);
                Assert.Equal(datelater.Day, datefromdb.Day);
                Assert.Equal(datelater.Hour, datefromdb.Hour);
                Assert.Equal(datelater.Minute, datefromdb.Minute);
                Assert.Equal(datelater.Second, datefromdb.Second);

            }
        }

        [Fact]
        public void LinqQueriesShouldSupportDateTimeInComplexQuery()
        {
            using (var session = new Session())
            {
                var date = new DateTime(2010, 3, 1, 15, 33, 33);

                session.Add(new TestProduct { Name = "1", Price = 10, Available = date });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.Available == date || x.Price == 13).ToList();
                Assert.Equal(1, list.Count);
                Assert.Equal(10, list[0].Price);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);

                var datefromdb = list[0].Available.ToLocalTime();

                Assert.Equal(date.Year, datefromdb.Year);
                Assert.Equal(date.Month, datefromdb.Month);
                Assert.Equal(date.Day, datefromdb.Day);
                Assert.Equal(date.Hour, datefromdb.Hour);
                Assert.Equal(date.Minute, datefromdb.Minute);
                Assert.Equal(date.Second, datefromdb.Second);

            }
        }

        [Fact]
        public void LinqQueriesShouldSupportDateTimeNested()
        {
            using (var session = new Session())
            {
                var date = new DateTime(2010, 3, 1, 15, 33, 33);

                session.Add(new TestProduct { Name = "1", Price = 10, Available = date });
                session.Add(new TestProduct { Name = "2", Price = 20, Supplier = new Supplier { CreatedOn = date } });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.Supplier.CreatedOn == date || x.Price == 13).ToList();
                Assert.Equal(1, list.Count);
                Assert.Equal(20, list[0].Price);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);

                var datefromdb = list[0].Supplier.CreatedOn.ToLocalTime();

                Assert.Equal(date.Year, datefromdb.Year);
                Assert.Equal(date.Month, datefromdb.Month);
                Assert.Equal(date.Day, datefromdb.Day);
                Assert.Equal(date.Hour, datefromdb.Hour);
                Assert.Equal(date.Minute, datefromdb.Minute);
                Assert.Equal(date.Second, datefromdb.Second);

            }
        }

        [Fact]
        public void LinqQueriesShouldSupportDivision()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.Price / 2 == 10).ToList();
                Assert.Equal(1, list.Count);
                Assert.Equal("2", list[0].Name);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportAddition()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.Price + 2 == 32).ToList();
                Assert.Equal(1, list.Count);
                Assert.Equal("3", list[0].Name);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportSubtraction()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 20 });
                session.Add(new TestProduct { Name = "3", Price = 30 });

                var queryable = session.Products; var list = queryable.Where(x => x.Price - 6 == 24).ToList();
                Assert.Equal(1, list.Count);
                Assert.Equal("3", list[0].Name);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(2, list.Count);
                Assert.Equal(30, list.Sum(x => x.Price));
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void LinqQueriesShouldSupportRegexInComplexQuery()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = null, Price = 20 });
                session.Add(new TestProduct { Name = "test1", Price = 10 });
                var queryable = session.Products; var products = queryable.Where(p => Regex.IsMatch(p.Name, "^te") && p.Name == "test1").ToList();
                Assert.Equal(10, products[0].Price);
                Assert.Equal(1, products.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void SingleQualifierQueryIsExecutedAsANDQuery()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "test1", Price = 20 });
                session.Add(new TestProduct { Name = "test", Price = 10 });
                var queryable = session.Products;
                var product = queryable.Where(p => p.Price == 10).Single();
                Assert.Equal(10, product.Price);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }


        [Fact]
        public void FirstQualifierQueryIsExecutedWithSort()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "test1", Price = 20 });
                session.Add(new TestProduct { Name = "test", Price = 10 });
                var queryable = session.Products; var product = queryable.OrderBy(x => x.Price).First();
                Assert.Equal(10, product.Price);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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

                Assert.Equal(10, product[0].Price);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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

                Assert.Equal(10, product.Price);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(22, result.SingleOrDefault().Price);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenThreeInDBWithChainedWhere()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 22 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                var queryable = session.Products; var result = queryable.Where(x => x.Price < 30);
                result = result.Where(x => x.Name.Contains("2"));
                Assert.Equal(22, result.SingleOrDefault().Price);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhenThreeInDBOrderedByPrice()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 40 });
                session.Add(new TestProduct { Name = "2", Price = 22 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                var queryable = session.Products; var products = queryable.OrderBy(x => x.Price).ToList();
                Assert.Equal(22, products[0].Price);
                Assert.Equal(40, products[2].Price);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhenThreeInDBOrderedByDeepAlias()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 40, Supplier = new Supplier { Name = "1" } });
                session.Add(new TestProduct { Name = "2", Price = 22, Supplier = new Supplier { Name = "2" } });
                session.Add(new TestProduct { Name = "3", Price = 33, Supplier = new Supplier { Name = "3" } });
                var queryable = session.Products; var products = queryable.OrderBy(x => x.Supplier.Name).ToList();
                Assert.Equal(40, products[0].Price);
                Assert.Equal(33, products[2].Price);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(10, products[0].Price);
                Assert.Equal(22, products[1].Price);
                Assert.Equal(33, products[2].Price);
                Assert.Equal(50, products[3].Price);
                Assert.Equal(50, products[4].Price);
                Assert.Equal("1", products[3].Name);
                Assert.Equal("2", products[4].Name);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(10, products[0].Price);
                Assert.Equal(22, products[1].Price);
                Assert.Equal(33, products[2].Price);
                Assert.Equal(50, products[3].Price);
                Assert.Equal(50, products[4].Price);
                Assert.Equal("2", products[3].Name);
                Assert.Equal("1", products[4].Name);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(22, products[0].Price);
                Assert.Equal(1, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(50, products[0].Price);
                Assert.Equal("5", products[0].Name);
                Assert.Equal(1, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhenThreeInDBOrderedDewscendingByPrice()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "1", Price = 10 });
                session.Add(new TestProduct { Name = "2", Price = 22 });
                session.Add(new TestProduct { Name = "3", Price = 33 });
                var queryable = session.Products; var products = queryable.OrderByDescending(x => x.Price).ToList();
                Assert.Equal(33, products[0].Price);
                Assert.Equal(10, products[2].Price);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenToLower()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.ToLower() == "test2").ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(22, products[0].Price);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenToLowerInvariant()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.ToLowerInvariant() == "test2").ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(22, products[0].Price);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenToUpper()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.ToUpper() == "TEST3").ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(33, products[0].Price);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenToUpperInvariant()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.ToUpperInvariant() == "TEST3").ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(33, products[0].Price);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenToUpperAndContains()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test", Price = 10 });
                session.Add(new TestProduct { Name = "Test", Price = 22 });
                session.Add(new TestProduct { Name = "Test1", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.ToUpper().Contains("EST")).ToList();
                Assert.Equal(3, products.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhenSubstringUsedWithLength()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "TestName1", Price = 10 });
                session.Add(new TestProduct { Name = "TestName2", Price = 22 });
                session.Add(new TestProduct { Name = "TestName3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Substring(2, 4) == "stNa").ToList();
                Assert.Equal(3, products.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenSubstringUsedWithOutLength()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "TestName1", Price = 10 });
                session.Add(new TestProduct { Name = "TestName2", Price = 22 });
                session.Add(new TestProduct { Name = "TestName3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Substring(2) == "stName2").ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(22, products[0].Price);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(4, products.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceEqual10()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3", Price = 10 });
                session.Add(new TestProduct { Name = "Test4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Price == 10).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhen3InDbWithPriceGreaterThan10()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3", Price = 10 });
                session.Add(new TestProduct { Name = "Test4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Price > 10).ToList();
                Assert.Equal(2, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceEqual10AndNameTest3()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3", Price = 10 });
                session.Add(new TestProduct { Name = "Test4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Price == 10 && x.Name == "Test3").ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceGreaterThan10LessThan30()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3", Price = 10 });
                session.Add(new TestProduct { Name = "Test4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Price > 10 && x.Price < 30).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhen3InDbWithPriceGreaterThan10LessThan40AndNot33()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3", Price = 10 });
                session.Add(new TestProduct { Name = "Test4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Price > 10 && x.Price < 40 && x.Price != 33).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(22, products[0].Price);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableDayIsFifth()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = new DateTime(2000, 2, 6) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available.Day == 5).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableDateIsSameAsRequired()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = new DateTime(2000, 2, 6) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available == new DateTime(2000, 2, 5)).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal("Test3X", products[0].Name);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(1, products.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableGreaterThanToday()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(1) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available > DateTime.Now).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableLessThanTodayPlus1()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(2) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available > DateTime.Now.AddDays(1)).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableMonthIs2()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = new DateTime(2001, 3, 6) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available.Month == 2).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenAvailableYearIs2000()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = new DateTime(2001, 2, 6) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available.Year == 2000).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(1, products.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(1, products.Count);
                Assert.Equal(33, products[0].Price);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(1, products.Count);
                Assert.Equal(33, products[0].Price);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(1, products.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(1, products.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(1, products.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhen3InDbWithNoExpression()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var products = queryable.ToList();
                Assert.Equal(3, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void ThreeProductsShouldBeReturnedWhenAvailableLessThanToday()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "Test4X", Price = 22, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "XTest3", Price = 10, Available = DateTime.Now.AddDays(-1) });
                session.Add(new TestProduct { Name = "XTest4", Price = 22, Available = DateTime.Now.AddDays(1) });
                var queryable = session.Products; var products = queryable.Where(x => x.Available < DateTime.Now).ToList();
                Assert.Equal(3, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhenNotEqualsOne()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var result = queryable.Where(x => x.Price != 22);
                Assert.Equal(2, result.Count());
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductShouldBeReturnedWhen3InDbWithFirst()
        {
            using (var session = new Session())
            {
                //session.CreateCappedCollection("Product"); //only capped collections return in insertion order
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var result = queryable.ToList();
                Assert.Equal(3, result.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductShouldBeReturnedWhen3InDbWithSingle()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var result = queryable.SingleOrDefault(x => x.Price == 22);
                Assert.Equal(22, result.Price);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductShouldBeReturnedUsingSimpleANDQuery()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 22 });
                var queryable = session.Products; var results = queryable.Where(x => x.Price == 22 && x.Name == "Test3").ToArray();
                Assert.Equal(1, results.Length);
                Assert.Equal(22, results[0].Price);
                Assert.Equal("Test3", results[0].Name);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductShouldBeReturnedWhen3InDbWithSingleUsingVariable()
        {
            using (var session = new Session())
            {
                var target = 22;
                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var result = queryable.SingleOrDefault(x => x.Price == target);
                Assert.Equal(target, result.Price);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhen3InDbWithPriceLessThan10OrGreaterThan30()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3", Price = 10 });
                session.Add(new TestProduct { Name = "Test4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Price > 10 || x.Price > 30).ToList();
                Assert.Equal(2, products.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhenContainsX()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "TestX3", Price = 10 });
                session.Add(new TestProduct { Name = "TestX4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Contains("X")).ToList();
                Assert.Equal(2, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenContainsUsesRegexEscapeChar()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "TestX3", Price = 10 });
                session.Add(new TestProduct { Name = "Test+X4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Contains("+X")).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenReplaceContainsRegexEscapeChar()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "TestX3", Price = 10 });
                session.Add(new TestProduct { Name = "Test+X4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Replace("+X", "X") == "TestX4").ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void OneProductsShouldBeReturnedWhenContainsMatchesFirstCharacter()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "ATest", Price = 10 });
                session.Add(new TestProduct { Name = "BTest", Price = 22 });
                session.Add(new TestProduct { Name = "CTest", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Contains("B")).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhenEndsWithX()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test3X", Price = 10 });
                session.Add(new TestProduct { Name = "Test4X", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });

                var queryable = session.Products; var products = queryable.Where(x => x.Name.EndsWith("X")).ToList();

                Assert.Equal(2, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void TwoProductsShouldBeReturnedWhenStartsWithX()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "XTest3", Price = 10 });
                session.Add(new TestProduct { Name = "XTest4", Price = 22 });
                session.Add(new TestProduct { Name = "Test5", Price = 33 });

                var queryable = session.Products; var products = queryable.Where(x => x.Name.StartsWith("X")).ToList();

                Assert.Equal(2, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(1, products.Count);
                Assert.Equal(33, products[0].Price);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(1, products.Count);
                Assert.Equal(33, products[0].Price);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal(1, products.Count);
                Assert.Equal(33, products[0].Price);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void TwoProductsofFourShouldBeReturnedWithSkipTake()
        {
            using (var session = new Session())
            {

                session.Add(new TestProduct { Name = "Test1", Price = 10 });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                session.Add(new TestProduct { Name = "Test4", Price = 44 });
                var queryable = session.Products; var products = queryable.Skip(1).Take(2).ToList();
                Assert.Equal(22.0, products[0].Price);
                Assert.Equal(33.0, products[1].Price);
                Assert.Equal(2, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void TwoProductsofFourShouldBeReturnedWithSkipTakeAndWhere()
        {
            using (var session = new Session())
            {

                session.Add(new TestProduct { Name = "Test", Price = 10 });
                session.Add(new TestProduct { Name = "Test1", Price = 22 });
                session.Add(new TestProduct { Name = "Test", Price = 33 });
                session.Add(new TestProduct { Name = "Test", Price = 44 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name == "Test").Skip(1).Take(2).ToList();
                Assert.Equal(33, products[0].Price);
                Assert.Equal(44, products[1].Price);
                Assert.Equal(2, products.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void TwoProductsofFourShouldBeReturnedWhereLengthOfNameEquals4()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test", Price = 10 });
                session.Add(new TestProduct { Name = "Test1", Price = 22 });
                session.Add(new TestProduct { Name = "Test1", Price = 33 });
                session.Add(new TestProduct { Name = "Test", Price = 44 });
                var queryable = session.Products; var products = queryable.Where(x => x.Name.Length == 4).ToList().OrderBy(x => x.Price).ToArray();
                Assert.Equal(10, products[0].Price);
                Assert.Equal(44, products[1].Price);
                Assert.Equal(2, products.Length);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void FiltersBasedOnObjectId()
        {
            var targetId = ObjectId.NewObjectId();
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10, _id = targetId });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(p => p._id == targetId).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(targetId, products[0]._id);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void FiltersBasedOnObjectIdInComplexQuery()
        {
            var targetId = ObjectId.NewObjectId();
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new TestProduct { Name = "Test2", Price = 22, Available = new DateTime(2000, 2, 6), _id = targetId });
                session.Add(new TestProduct { Name = "Test3", Price = 33, Available = new DateTime(2000, 2, 7) });
                var queryable = session.Products; var products = queryable.Where(p => p._id == targetId && p.Available.Day == 6).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(targetId, products[0]._id);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }
        [Fact]
        public void FiltersBasedOnObjectIdExclusionInComplexQuery()
        {
            var targetId = ObjectId.NewObjectId();
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10, Available = new DateTime(2000, 2, 5) });
                session.Add(new TestProduct { Name = "Test2", Price = 22, Available = new DateTime(2000, 2, 5), _id = targetId });
                session.Add(new TestProduct { Name = "Test3", Price = 33, Available = new DateTime(2000, 2, 5) });
                var queryable = session.Products; var products = queryable.Where(p => p._id != targetId && p.Available.Day == 5).ToList();
                Assert.Equal(2, products.Count);
                Assert.NotEqual(targetId, products[0]._id);
                Assert.NotEqual(targetId, products[1]._id);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
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
                var queryable = session.Posts; var posts = queryable.Where(p => p.Id == targetId).ToList();
                Assert.Equal(1, posts.Count);
                Assert.Equal(targetId, posts[0].Id);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void Filters_Based_On_Guid()
        {
            var targetId = Guid.NewGuid();
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "Test1", Price = 10, UniqueID = targetId });
                session.Add(new TestProduct { Name = "Test2", Price = 22 });
                session.Add(new TestProduct { Name = "Test3", Price = 33 });
                var queryable = session.Products; var products = queryable.Where(p => p.UniqueID == targetId).ToList();
                Assert.Equal(1, products.Count);
                Assert.Equal(targetId, products[0].UniqueID);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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

                Assert.Equal("Second", found.Title);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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

                Assert.Equal("Second", found.Title);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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

                Assert.Equal("Second", found.Title);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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

                Assert.Equal("Second", found.Title);
                Assert.Equal(true, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
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
                Assert.Equal("Second", found.Title);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);

                queryable = session.Posts;
                found = queryable.Where(p => p.Comments[1].Text == "comment2").SingleOrDefault();
                Assert.Equal("First", found.Title);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void MapReduceMax()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "ATest", Price = 10 });
                session.Add(new TestProduct { Name = "BTest", Price = 22 });
                session.Add(new TestProduct { Name = "CTest", Price = 33 });
                var queryable = session.Products;
                var productMax = queryable.Max(x => x.Price);
                Assert.Equal(33, productMax);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void MapReduceMaxDeepQuery()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "ATest", Price = 10, Supplier = new Supplier { RefNum = 3 } });
                session.Add(new TestProduct { Name = "BTest", Price = 22, Supplier = new Supplier { RefNum = 2 } });
                session.Add(new TestProduct { Name = "CTest", Price = 33, Supplier = new Supplier { RefNum = 1 } });
                var queryable = session.Products;
                var productMax = queryable.Max(x => x.Supplier.RefNum);
                Assert.Equal(3, productMax);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void MapReduceWhereWithMax()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "ATest", Price = 10 });
                session.Add(new TestProduct { Name = "BTest", Price = 22 });
                session.Add(new TestProduct { Name = "BTest", Price = 33 });
                var queryable = session.Products;
                var productMax = queryable.Where(x => x.Name == "BTest").Max(x => x.Price);
                Assert.Equal(33, productMax);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void MapReduceWhereWithMin()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "ATest", Price = 10 });
                session.Add(new TestProduct { Name = "BTest", Price = 22 });
                session.Add(new TestProduct { Name = "BTest", Price = 33 });
                var queryable = session.Products;
                var productMax = queryable.Where(x => x.Name == "BTest").Min(x => x.Price);
                Assert.Equal(22, productMax);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void FirstWhereNoResultsReturnedInWhere()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "ATest", Price = 10 });
                session.Add(new TestProduct { Name = "BTest", Price = 22 });
                session.Add(new TestProduct { Name = "BTest", Price = 33 });
                var noProducct = session.Products.Where(x => x.Name == "ZTest");
                var ex = Assert.Throws<InvalidOperationException>(() => noProducct.First());
                Assert.Equal("Sequence contains no elements", ex.Message);
            }
        }

        [Fact]
        public void FirstOrDefaultWhereNoResultsReturnedInWhere()
        {
            using (var session = new Session())
            {
                session.Add(new TestProduct { Name = "ATest", Price = 10 });
                session.Add(new TestProduct { Name = "BTest", Price = 22 });
                session.Add(new TestProduct { Name = "BTest", Price = 33 });
                var queryable = session.Products;
                var noProducct = queryable.Where(x => x.Name == "ZTest").FirstOrDefault();
                Assert.Equal(null, noProducct);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
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

                var queryable = session.DB.GetCollection<SuperClassObject>().AsQueryable();

                var dtos = queryable.Where(dto => dto.Title == "Find This").ToList();

                Assert.Equal(1, dtos.Count);
                Assert.Equal("Find This", dtos[0].Title);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void CanQueryAndReturnSubClassedObjects_EvenWhenAddedBySubClass()
        {
            using (var session = new Session())
            {
                session.Drop<SuperClassObject>();

                session.Add(new SubClassedObject());

                var queryable = session.DB.GetCollection<SuperClassObject>().AsQueryable();

                var dtos = queryable.ToList();

                Assert.Equal(1, dtos.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void CanQueryAndReturnSubClassedObjects_EvenWhenQueriedBySubClass()
        {
            using (var session = new Session())
            {
                session.Drop<SuperClassObject>();

                session.Add(new SubClassedObject());

                var queryable = session.DB.GetCollection<SubClassedObject>().AsQueryable();

                var dtos = queryable.ToList();

                Assert.Equal(1, dtos.Count);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
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

                var queryable = session.DB.GetCollection<IDiscriminated>().AsQueryable();
                var dtos = queryable.ToList();

                Assert.Equal(1, dtos.Count);
                Assert.Equal(obj.Id, dtos.Single().Id);
                Assert.Equal(false, queryable.QueryStructure().IsComplex);
            }
        }

        [Fact]
        public void CanAQuerySupportArrayIdentifiers()
        {
            MongoConfiguration.Initialize(c => c.AddMap<ShopperMap>());
            using (var shoppers = new Shoppers(Mongo.Create("mongodb://localhost:27017/test")))
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
                Assert.Equal("Jane", deepQuery[0].Name);
                Assert.Equal("Cart2", deepQuery[0].Cart.Name);
                Assert.Equal(1, deepQuery.Count);
                Assert.Equal(false, (shoppers.Provider as IMongoQueryResults).TranslationResults.IsComplex);
            }
        }
    }
}