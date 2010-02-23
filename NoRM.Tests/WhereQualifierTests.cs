using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NoRM.Protocol.SystemMessages.Responses;
using NoRM.Protocol.Messages;
using NoRM.BSON;

namespace NoRM.Tests {
    
    [TestFixture]
    public class WhereQualifierTests {

        private MongoServer _server;
        private MongoDatabase _db;
        private MongoCollection<TestClass> _coll;

        [SetUp]
        public void TestFixture_Setup() {
            _server = new MongoServer("mongodb://127.0.0.1/TestSuiteDatabase");
            _db = _server.Database;
            DroppedCollectionResponse collResponse = _db.DropCollection("TestClasses");
            _coll = _db.GetCollection<TestClass>("TestClasses");
        }

        [Test]
        public void Where_Expression_Should_Work_With_FLyweight() {

            _coll.Insert(new TestClass { ADouble = 1d });
            _coll.Insert(new TestClass { ADouble = 2d });
            _coll.Insert(new TestClass { ADouble = 3d });
            _coll.Insert(new TestClass { ADouble = 4d });

            var count = _coll.Find();
            Assert.AreEqual(4, count.Count());

            var query = new Flyweight();
            query["$where"] = " function(){return this.ADouble > 1;} ";
            var results = _coll.Find(query);
            Assert.AreEqual(3, results.Count());


        }

    }
}
