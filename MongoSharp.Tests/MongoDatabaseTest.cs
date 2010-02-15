using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using MongoSharp.Protocol.SystemMessages.Responses;

namespace MongoSharp.Tests
{
    [TestFixture]
    public class MongoDatabaseTest
    {
        [Test]
        public void GetAllCollections_Returns_Collections()
        {
            MongoServer context = new MongoServer();
            var db = context.GetDatabase("test");
            Assert.IsNotEmpty(db.GetAllCollections().ToList());
        }

        [Test]
        public void Drop_Collection_Returns_True()
        {
            MongoServer context = new MongoServer();
            var db = context.GetDatabase("test");
            string collName = "testInsertCollection";
            db.GetCollection<object>(collName).Insert(new { Title = "TestInsert" });
            
            var results = db.DropCollection(collName);

            Assert.IsTrue((results.OK == 1.0));
        }

        [Test]
        public void Get_Profiling_Information()
        {
            MongoServer server = new MongoServer();

            var db = server.GetDatabase("test");

            var results = db.GetProfilingInformation();

            foreach (ProfilingInformationResponse profile in results)
            {
                Console.WriteLine(profile.Info);
            }
            Assert.IsTrue((results.Count<ProfilingInformationResponse>() > 0));
        }

        [Test]
        public void Validate_Collection()
        {
            var db = new MongoServer().GetDatabase("test");

            var response = db.ValidateCollection("foo", false);

            Assert.IsTrue((response.Ns == "test.foo"));
            
        }

    }
}
