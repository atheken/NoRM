using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NoRM.Protocol.SystemMessages.Responses;
using NoRM.BSON;

namespace NoRM.Tests
{
    [TestFixture]
    [Category("Hits MongoDB")]
    public class MongoDatabaseTest
    {
        private MongoDatabase _db;
        private MongoContext _server;

        [TestFixtureSetUp]
        public void SetUpDBTests()
        {
            var server = new MongoContext();
            this._server = server;
            this._db = server.GetDatabase("test" + Guid.NewGuid().ToString().Substring(0, 5));
        }
        [TestFixtureTearDown]
        public void TearDown()
        {
            this._server.DropDatabase(this._db.DatabaseName);
        }

        [Test]
        public void GetAllCollections_Returns_Collections()
        {
            var context = new MongoContext();
            var db = context.GetDatabase("test");
            Assert.IsNotEmpty(db.GetAllCollections().ToList());
        }

        [Test]
        public void Drop_Collection_Returns_True()
        {
            var context = new MongoContext();
            var db = context.GetDatabase("test");
            var collName = "testInsertCollection";
            db.GetCollection<object>(collName).Insert(new { Title = "TestInsert" });

            var results = db.DropCollection(collName);

            Assert.IsTrue((results.OK == 1.0));
        }

        [Test]
        public void Set_Profiling_Level()
        {
            var db = new MongoContext().GetDatabase("test");

            var response = db.SetProfileLevel(NoRM.Protocol.SystemMessages.ProfileLevel.AllOperations);

            Assert.IsTrue((response.Was == 0.0));

            response = db.SetProfileLevel(NoRM.Protocol.SystemMessages.ProfileLevel.ProfilingOff);

            Assert.IsTrue((response.Was == 2.0));
        }

        [Test]
        public void Get_Profiling_Information()
        {
            var server = new MongoContext();

            var db = server.GetDatabase("test");

            var results = db.GetProfilingInformation();

            foreach (var profile in results)
            {
                Console.WriteLine(profile.Info);
            }
            Assert.IsTrue((results.Count<ProfilingInformationResponse>() > 0));
        }

        [Test]
        public void Validate_Collection()
        {
            String collName = "validColl";
            var testColl = this._db.GetCollection<Object>(collName);

            //must insert something before the collection will exist.
            testColl.Insert(new Object());

            var response = this._db.ValidateCollection(collName, false);

            Assert.AreEqual(testColl.FullyQualifiedName, response.Ns);
        }


    }
}
