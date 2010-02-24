using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NoRM.Protocol.SystemMessages.Responses;
using NoRM.BSON;
using NoRM.BSON.DbTypes;
/*
namespace NoRM.Tests
{
    public class MiniObject
    {
        public OID ID { get; set; }
    }

    [TestFixture]
    [Category("Hits MongoDB")]
    public class MongoDatabaseTest
    {
        private MongoDatabase _db;
        private Mongo _server;

        [TestFixtureSetUp]
        public void SetUpDBTests()
        {
            var database = "test" + Guid.NewGuid().ToString().Substring(0, 5);
            var server = new Mongo("mongodb://localhost/" + database);
            this._server = server;
            this._db = server.Database;
        }
        [TestFixtureTearDown]
        public void TearDown()
        {
            using (var admin = new MongoAdmin("mongodb://localhost/" + _db.DatabaseName))
            {
                admin.DropDatabase();
            }                        
            _server.Dispose();
        }

        [Test]
        public void GetAllCollections_Returns_Collections()
        {
            var context = new Mongo();
            var db = context.GetDatabase("test");
            Assert.IsNotEmpty(db.GetAllCollections().ToList());
        }

        [Test]
        public void Drop_Collection_Returns_True()
        {
            var context = new Mongo();
            var db = context.GetDatabase("test");
            var collName = "testInsertCollection";
            db.GetCollection<MiniObject>(collName).Insert(new MiniObject());

            var results = db.DropCollection(collName);

            Assert.IsTrue((results.OK == 1.0));
        }

        [Test]
        public void Set_Profiling_Level()
        {
            var db = new Mongo().GetDatabase("test");

            var response = db.SetProfileLevel(NoRM.Protocol.SystemMessages.ProfileLevel.AllOperations);

            Assert.IsTrue((response.Was == 0.0));

            response = db.SetProfileLevel(NoRM.Protocol.SystemMessages.ProfileLevel.ProfilingOff);

            Assert.IsTrue((response.Was == 2.0));
        }

        [Test]
        public void Get_Profiling_Information()
        {
            var server = new Mongo();

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
            var testColl = this._db.GetCollection<MiniObject>(collName);

            //must insert something before the collection will exist.
            testColl.Insert(new MiniObject());

            var response = this._db.ValidateCollection(collName, false);

            Assert.AreEqual(testColl.FullyQualifiedName, response.Ns);
        }


    }
}
*/