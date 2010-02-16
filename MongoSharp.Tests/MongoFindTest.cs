using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using MongoSharp.Protocol.SystemMessages.Responses;

namespace MongoSharp.Tests
{

    public class TestClass
    {
        public double? ADouble { get; set; }
        public string AString { get; set; }
        public int? AInteger { get; set; }
        public List<String> AStringArray { get; set; }
    }

    // TODO rename this to MongoCollectionTest
    [TestFixture]
    public class MongoFindTests
    {
        private MongoServer _server;
        private MongoDatabase _db;
        private MongoCollection<TestClass> _coll;

        [SetUp]
        public void TestFixture_Setup()
        {
            _server = new MongoServer();
            _db = _server.GetDatabase("TestSuiteDatabase");
            _coll = _db.GetCollection<TestClass>("TestClasses");
        }

        [TearDown]
        public void TestFixture_Teardown()
        {
            DroppedCollectionResponse collResponse = _db.DropCollection("TestClasses");
            DroppedDatabaseResponse dbResponse = _server.DropDatabase("TestSuiteDatabase");

            _coll = null;
            _db = null;
            _server = null;
        }

        [Test]
        public void FindOne_Returns_Something()
        {
            _coll.Insert(new TestClass { ADouble = 1d });

            TestClass found = _coll.FindOne(new { ADouble = 1d });

            Assert.IsNotNull(found);
        }

        [Test]
        public void FindOne_Qualifier_All()
        {
            _coll.Insert(new TestClass { ADouble = 1d, AString = "teststring" });
            _coll.Insert(new TestClass { ADouble = 2d });
            _coll.Insert(new TestClass { ADouble = 3d });
            _coll.Insert(new TestClass { ADouble = 4d });
            _coll.Insert(new TestClass { ADouble = 5d });

            IEnumerable<TestClass> results = _coll.Find(
                new
                {
                    ADouble = Q.All<object>(new{ADouble = 1d}, new {AString = "teststring"})
                });

            Assert.AreEqual(1, results.Count<TestClass>());
        }

        [Test]
        public void FindOne_Qualifier_Exists()
        {
            _coll.Insert(new TestClass { ADouble = 1d });
            _coll.Insert(new TestClass { ADouble = 2d });
            _coll.Insert(new TestClass { ADouble = 3d });
            _coll.Insert(new TestClass { ADouble = 4d });
            _coll.Insert(new TestClass { ADouble = 5d });

            IEnumerable<TestClass> results = _coll.Find(new { ADouble = Q.Exists(true) });

            Assert.AreEqual(5, results.Count<TestClass>());
        }

        //[Test]
        //public void FindOne_Qualifier_Equals()
        //{
        //    // TODO Uh, I'm getting the object.Equals(objA, objB) method; implementation?
        //    _coll.Insert(new TestClass { ADouble = 1 });
        //    _coll.Insert(new TestClass { ADouble = 2 });
        //    _coll.Insert(new TestClass { ADouble = 3 });
        //    _coll.Insert(new TestClass { ADouble = 4 });
        //    _coll.Insert(new TestClass { ADouble = 5 });

        //    IEnumerable<TestClass> results = _coll.Find(new { ADouble = Q.Equals(1d,1d) });

        //    Assert.IsTrue((results.Count<TestClass>() == 3));
        //}

        [Test]
        public void FindOne_Qualifier_NotEqual()
        {
            // TODO this is failing currently. shouldn't be, I don't think?
            _coll.Insert(new TestClass { ADouble = 1d });
            _coll.Insert(new TestClass { ADouble = 2d });
            _coll.Insert(new TestClass { ADouble = 3d });
            _coll.Insert(new TestClass { ADouble = 4d });
            _coll.Insert(new TestClass { ADouble = 5d });

            IEnumerable<TestClass> results = _coll.Find(new { ADouble = Q.NotEqual(2d) });

            int count = results.Count();

            Console.WriteLine("Count: " + count);
            Assert.AreEqual(4, count);
        }

        [Test]
        public void FindOne_Qualifier_In()
        {
            // TODO this is failing - need to check with AT and see if I'm doing this right.
            _coll.Insert(new TestClass { ADouble = 1 });
            _coll.Insert(new TestClass { ADouble = 2 });
            _coll.Insert(new TestClass { ADouble = 3 });
            _coll.Insert(new TestClass { ADouble = 4 });
            _coll.Insert(new TestClass { ADouble = 5 });

            IEnumerable<TestClass> results = _coll.Find(new
            {
                ADouble = Q.In(1d, 3d, 5d)
            });

            int count = results.Count();
            Console.WriteLine("Count: " + count);

            Assert.AreEqual(2, count);
        }

        [Test]
        public void FindOne_Qualifier_NotIn()
        {
            // TODO this is failing - need to check with AT and see if I'm doing this right.
            _coll.Insert(new TestClass { ADouble = 1d });
            _coll.Insert(new TestClass { ADouble = 2d });
            _coll.Insert(new TestClass { ADouble = 3d });
            _coll.Insert(new TestClass { ADouble = 4d });
            _coll.Insert(new TestClass { ADouble = 5d });

            IEnumerable<TestClass> results = _coll.Find(new{
                ADouble = Q.NotIn(1d, 3d, 5d)
            });

            int count = results.Count();
            Console.WriteLine("Count: " + count);

            Assert.AreEqual(2, count);
        }

        [Test]
        public void FindOne_Qualifier_GreaterThan()
        {
            _coll.Insert(new TestClass { ADouble = 1d });
            _coll.Insert(new TestClass { ADouble = 2d });
            _coll.Insert(new TestClass { ADouble = 3d });
            _coll.Insert(new TestClass { ADouble = 4d });
            _coll.Insert(new TestClass { ADouble = 5d });

            IEnumerable<TestClass> results = _coll.Find(new { ADouble = Q.GreaterThan(2d) });


            Assert.AreEqual(3, results.Count<TestClass>());
        }

        [Test]
        public void FindOne_Qualifier_GreaterOrEqual()
        {
            _coll.Insert(new TestClass { ADouble = 1d });
            _coll.Insert(new TestClass { ADouble = 2d });
            _coll.Insert(new TestClass { ADouble = 3d });
            _coll.Insert(new TestClass { ADouble = 4d });
            _coll.Insert(new TestClass { ADouble = 5d });

            IEnumerable<TestClass> results = _coll.Find(new { ADouble = Q.GreaterOrEqual(2d) });

            Assert.AreEqual(4, results.Count<TestClass>());
        }

        [Test]
        public void FindOne_Qualifier_LessThan()
        {
            _coll.Insert(new TestClass { ADouble = 1d });
            _coll.Insert(new TestClass { ADouble = 2d });
            _coll.Insert(new TestClass { ADouble = 3d });
            _coll.Insert(new TestClass { ADouble = 4d });
            _coll.Insert(new TestClass { ADouble = 5d });

            IEnumerable<TestClass> results = _coll.Find(new { ADouble = Q.LessThan(2d) });

            Assert.AreEqual(1, results.Count<TestClass>());
        }

        [Test]
        public void FindOne_Qualifier_LessOrEqual()
        {
            _coll.Insert(new TestClass { ADouble = 1d });
            _coll.Insert(new TestClass { ADouble = 2d });
            _coll.Insert(new TestClass { ADouble = 3d });
            _coll.Insert(new TestClass { ADouble = 4d });
            _coll.Insert(new TestClass { ADouble = 5d });

            IEnumerable<TestClass> results = _coll.Find(new { ADouble = Q.LessOrEqual(2d) });

            Assert.AreEqual(2, results.Count<TestClass>());
        }

        [Test]
        public void FindOne_Qualifier_Size()
        {
            _coll.Insert(new TestClass { AStringArray = new string[] { "one" }.ToList() });
            _coll.Insert(new TestClass { AStringArray = new string[] { "one", "two" }.ToList() });
            _coll.Insert(new TestClass { AStringArray = new string[] { "one", "two", "three" }.ToList() });
            _coll.Insert(new TestClass { AStringArray = new string[] { "one", "two", "three", "four" }.ToList() });
            _coll.Insert(new TestClass { AStringArray = new string[] { "one", "two", "three", "four", "five" }.ToList() });

            IEnumerable<TestClass> results = _coll.Find(new { AStringArray = Q.Size(3d) });

            Assert.IsTrue((results.Count<TestClass>() == 1));
        }

    }
}
