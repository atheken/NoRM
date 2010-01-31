using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace System.Data.Mongo.Tests
{
    [TestFixture]
    public class MongoContextTest
    {
        private MongoContext _context;
        private MongoDatabase _db1;
        private MongoDatabase _db2;

        [TestFixtureSetUp]
        public void ConfigureContext()
        {
            //creates connection to the local mongo Db install on the default port.
            this._context = new MongoContext();

            //create two databases.
            this._db1 = this._context.GetDatabase("Test1");
            this._db2 = this._context.GetDatabase("Test2");
        }

        [Test]
        public void GetAllDatabases_Returns_DBs()
        {
            Assert.IsNotEmpty(this._context.GetAllDatabases().ToList());
        }

        
    }
}
