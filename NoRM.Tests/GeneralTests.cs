using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Norm.Responses;
using Norm;
using Norm.Tests;


namespace Norm.Tests
{
    [TestFixture]
    public class GeneralTests
    {
        private Mongod _proc;

        [TestFixtureSetUp]
        public void SetUp ()
        {
            _proc = new Mongod ();
        }

        [TestFixtureTearDown]
        public void TearDown ()
        {
            _proc.Dispose ();
        }

        [Test]
        public void Get_Last_Error_Returns()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var le = mongo.LastError();
                Assert.AreEqual(true, le.WasSuccessful);
            }
        }

        [Test]
        public void Base_Status_Message_Supports_Expando()
        {
            BaseStatusMessage bsm = new BaseStatusMessage();
            bsm["hello"] = "world";
            Assert.AreEqual("world", bsm["hello"]);
            Assert.AreEqual(bsm.AllProperties().First().PropertyName, "hello");

            Assert.AreEqual(bsm.AllProperties().First().Value, "world");
            bsm.Delete("hello");
            Assert.False(bsm.AllProperties().Any());

        }
    }
}
