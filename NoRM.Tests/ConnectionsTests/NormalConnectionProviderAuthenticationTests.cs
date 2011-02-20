using NUnit.Framework;
using Norm;
using Norm.BSON;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Norm.Tests
{
    [TestFixture]
    public class NormalConnectionProviderAuthenticationTests : AuthenticatedFixture
    {
        private Mongod _proc;
		
		private string Hash(String instring)
		{
			using (var md5 = MD5.Create())
			{
                var rawDigest = Encoding.UTF8.GetBytes(string.Concat(instring));
                var hashed = md5.ComputeHash(rawDigest);
                var sb = new StringBuilder(hashed.Length * 2);
                Array.ForEach(hashed, b => sb.Append(b.ToString("X2")));
                return sb.ToString().ToLower();
            }
		}
		
		[TestFixtureSetUp]
		public void SetUp ()
		{
			var authed = new Mongod();
			
			using(var db = Mongo.Create(this.NonAuthenticatedConnectionString("admin")))
			{
				 db.Database.GetCollection<object>("system.users").Insert(new {user="admin", pwd=Hash("admin:monogo:admin")});
			}
			using(var db = Mongo.Create(this.NonAuthenticatedConnectionString("main"))){
				 db.Database.GetCollection<object>("system.users").Insert(new {user="usr", pwd=Hash("usr:monogo:pss")});
			}
			
			authed.Dispose();
			
			_proc = new Mongod (true);
		}

        [TestFixtureTearDown]
        public void TearDown ()
        {
            _proc.Dispose ();
        }

        //[Test(Skip="authenticated connection seems to be hanging when we run this in sequence")]
        public void ThrowsExceptionIfConnectingWithInvalidCredentials()
        {
            var provider = new NormalConnectionProvider(ConnectionOptions.Create(AuthenticatedConnectionString("bad", "boy")));

            var ex = Assert.Throws<MongoException>(() => provider.Open(null));
            Assert.AreEqual("auth fails", ex.Message);
        }

        [Test]
        public void AuthenticatesWithProperCredentials()
        {
            var provider = new NormalConnectionProvider(ConnectionOptions.Create(AuthenticatedConnectionString("usr", "pss")));
            var connection = provider.Open(null);
            Assert.AreEqual(true, connection.Client.Connected);
            provider.Close(connection);
        }

        
    }
}