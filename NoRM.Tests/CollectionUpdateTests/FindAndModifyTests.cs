using NUnit.Framework;
using System;
using Norm.Configuration;
using Norm.BSON;
using Norm;

namespace Norm.Tests
{
	[TestFixture]
	public class FindAndModifyTests
	{
		protected class User
		{
			public int UserID { get; set; }
			public string UserName { get; set; }
			public string CreatedDateUtc { get; set; }
			public DateTime UpdatedDateUtc { get; set; }
			public override string ToString ()
			{
				return UserID.ToString () + ":" + UserName + ":" + UpdatedDateUtc.ToString ("r");
			}
		}

		Mongod _server;
		
		[TestFixtureSetUp]
		public void SetupTestFixture ()
		{
			_server = new Mongod ();
		}
		
		[TestFixtureTearDown]
		public void TearDownTF ()
		{
			_server.Dispose ();
		}
		
		[SetUp]
		public void Setup ()
		{
			MongoConfiguration.RemoveMapFor<User> ();
			MongoConfiguration.Initialize (config => { config.For<User> (c =>{
					c.ForProperty (e => e.UserID).UseAlias ("UI");
					c.ForProperty (e => e.UserName).UseAlias ("UN");
					c.ForProperty (e => e.CreatedDateUtc).UseAlias ("CD");
					c.ForProperty (e => e.UpdatedDateUtc).UseAlias ("UD");
					c.IdIs (e => e.UserID);
				}); 
			});
			using (var db = Mongo.Create ("mongodb://localhost:27701/test?strict=false&pooling=false"))
			{
				db.Database.DropCollection ("UserSet");
			}
		}
		
		[Test]
		public void MyTestMethod ()
		{
			
			using (var db = Mongo.Create ("mongodb://localhost:27701/test")) {
				var dt = DateTime.UtcNow;
				var users = db.GetCollection<User> ("UserSet");
				
				users.Save (new User { CreatedDateUtc = dt.ToString (), UpdatedDateUtc = dt, UserID = 1, UserName = "user1" });
				users.Save (new User { CreatedDateUtc = dt.ToString (), UpdatedDateUtc = dt, UserID = 2, UserName = "user2" });
				users.Save (new User { CreatedDateUtc = dt.ToString (), UpdatedDateUtc = dt, UserID = 3, UserName = "user3" });
				
				Assert.AreEqual (3, users.Count ());
				
				var update = new Expando ();
				//update["UD"] = new { UD = dt.AddMinutes(9).ToString() };
				update["$inc"] = new { UpdatedDateUtc = dt.AddMinutes (9).ToString () };
				//update["$inc"] = new { UD = dt.AddMinutes(9).ToString() };
				//update["UD"] = new { UD = dt.AddMinutes(9).ToString() };
				//var foundUser = users.FindAndModify(new { CD = dt, UD = dt, UI = 1, UN = "user2" }, update);
				var foundUser = users.FindAndModify (new { UN = "user2" }, update);
				//var foundUser = users.FindAndModify(new { UN = "user2" }, update);
				//var foundUser = users.FindAndModify("user2", update);
				
				Assert.IsNotNull (foundUser);
			}
		}
	}
}
