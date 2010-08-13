using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Norm.Collections;

namespace Norm.Tests
{
    
    public class MongoSearchQualifierTests : IDisposable
    {
        private readonly IMongoCollection<TestClass> _coll;
        private readonly IMongo _server;

        public MongoSearchQualifierTests()
        {
            _server = Mongo.Create(TestHelper.ConnectionString("pooling=false"));
            _coll = _server.GetCollection<TestClass>("TestClasses");
        }

        public void Dispose()
        {
            _server.Database.DropCollection("TestClasses");
            using (var admin = new MongoAdmin(TestHelper.ConnectionString("pooling=false")))
            {
                admin.DropDatabase();
            }
            _server.Dispose();
        }

        [Fact]
        public void FindOneReturnsSomething()
        {
            _coll.Insert(new TestClass { ADouble = 1d });
            var found = _coll.FindOne(new { ADouble = 1d });
            Assert.NotNull(found);
        }

        [Fact]
        public void FindOneQualifierAll()
        {
            _coll.Insert(new TestClass { ADouble = 1d, AStringArray = (new[] { "a", "b", "c" }).ToList() });
            _coll.Insert(new TestClass { ADouble = 2d });
            _coll.Insert(new TestClass { ADouble = 3d });
            _coll.Insert(new TestClass { ADouble = 4d });
            _coll.Insert(new TestClass { ADouble = 5d });
            _coll.Insert(new TestClass { ADouble = 1d, AStringArray = (new[] { "a", "b", "c" }).ToList() });

            var results = _coll.Find(new { AStringArray = Q.All("a", "b") });
            Assert.Equal(2, results.Count());
        }

        [Fact]
        public void FindOneQualifierExists()
        {
            _coll.Insert(new TestClass { ADouble = 1d },
                         new TestClass { ADouble = 2d },
                         new TestClass { ADouble = 3d },
                         new TestClass { ADouble = 4d },
                         new TestClass { ADouble = 5d });

            var results = _coll.Find(new { ADouble = Q.Exists(true) });
            Assert.Equal(5, results.Count());
        }

        [Fact]
        public void FindOneQualifierEquals()
        {
            _coll.Insert(new TestClass { ADouble = 1 });
            _coll.Insert(new TestClass { ADouble = 1 });
            _coll.Insert(new TestClass { ADouble = 3 });
            _coll.Insert(new TestClass { ADouble = 4 });
            _coll.Insert(new TestClass { ADouble = 5 });

            var results = _coll.Find(new { ADouble = Q.Equals(1d) });
            Assert.Equal(results.Count(), 2);
        }

        [Fact]
        public void FindOneQualifierNotEqual()
        {
            // TODO this is failing currently. shouldn't be, I don't think?
            _coll.Insert(new TestClass { ADouble = 1d },
                         new TestClass { ADouble = 2d },
                         new TestClass { ADouble = 3d },
                         new TestClass { ADouble = 4d },
                         new TestClass { ADouble = 5d });

            var results = _coll.Find(new { ADouble = Q.NotEqual(2d) });
            Assert.Equal(4, results.Count());
        }

        [Fact]
        public void FindOneQualifierIn()
        {
            _coll.Insert(new TestClass { ADouble = 1 },
                         new TestClass { ADouble = 1 },
                         new TestClass { ADouble = 2 },
                         new TestClass { ADouble = 3 },
                         new TestClass { ADouble = 4 },
                         new TestClass { ADouble = 5 });

            var results = _coll.Find(new { ADouble = Q.In(1d) });
            Assert.Equal(2, results.Count());
        }

        [Fact]
        public void FindOneQualifierNotIn()
        {
            // TODO this is failing - need to check with AT and see if I'm doing this right.
            _coll.Insert(new TestClass { ADouble = 1d },
                         new TestClass { ADouble = 2d },
                         new TestClass { ADouble = 3d },
                         new TestClass { ADouble = 4d },
                         new TestClass { ADouble = 5d });

            var results = _coll.Find(new { ADouble = Q.NotIn(1d, 3d, 5d) });
            Assert.Equal(2, results.Count());
        }

        [Fact]
        public void FindOneQualifierGreaterThan()
        {
            _coll.Insert(new TestClass { ADouble = 1d },
                         new TestClass { ADouble = 2d },
                         new TestClass { ADouble = 3d },
                         new TestClass { ADouble = 4d },
                         new TestClass { ADouble = 5d });

            var results = _coll.Find(new { ADouble = Q.GreaterThan(2d) });
            Assert.Equal(3, results.Count());
        }

        [Fact]
        public void FindOneQualifierGreaterOrEqual()
        {
            _coll.Insert(new TestClass { ADouble = 1d },
                         new TestClass { ADouble = 2d },
                         new TestClass { ADouble = 3d },
                         new TestClass { ADouble = 4d },
                         new TestClass { ADouble = 5d });

            var results = _coll.Find(new { ADouble = Q.GreaterOrEqual(2d) });
            Assert.Equal(4, results.Count());
        }

        [Fact]
        public void FindOneQualifierLessThan()
        {
            _coll.Insert(new TestClass { ADouble = 1d },
                         new TestClass { ADouble = 2d },
                         new TestClass { ADouble = 3d },
                         new TestClass { ADouble = 4d },
                         new TestClass { ADouble = 5d });

            var results = _coll.Find(new { ADouble = Q.LessThan(2d) });
            Assert.Equal(1, results.Count());
        }

        [Fact]
        public void FindOneQualifierLessOrEqual()
        {
            _coll.Insert(new TestClass { ADouble = 1d },
                         new TestClass { ADouble = 2d },
                         new TestClass { ADouble = 3d },
                         new TestClass { ADouble = 4d },
                         new TestClass { ADouble = 5d });

            var results = _coll.Find(new { ADouble = Q.LessOrEqual(2d) });
            Assert.Equal(2, results.Count());
        }

        [Fact]
        public void FindOneQualifierSize()
        {
            _coll.Insert(new TestClass { AStringArray = new[] { "one" }.ToList() });
            _coll.Insert(new TestClass { AStringArray = new[] { "one", "two" }.ToList() });
            _coll.Insert(new TestClass { AStringArray = new[] { "one", "two", "three" }.ToList() });
            _coll.Insert(new TestClass { AStringArray = new[] { "one", "two", "three" }.ToList() });
            _coll.Insert(new TestClass { AStringArray = new[] { "one", "two", "three", "four" }.ToList() });
            _coll.Insert(new TestClass { AStringArray = new[] { "one", "two", "three", "four", "five" }.ToList() });

            var results = _coll.Find(new { AStringArray = Q.Size(3d) });
            Assert.Equal(2, results.Count());
        }

    }
}