using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Norm;
using Norm.BSON;
using Norm.Collections;
using Norm.Responses;
using NUnit.Framework;

namespace Norm.Tests
{
	[TestFixture]
	public class QueryTests
	{
		private Mongod _proc;
		private IMongo _server;
		private BuildInfoResponse _buildInfo = null;
		private IMongoCollection<Person> _collection;

		[TestFixtureSetUpAttribute]
		public void SetupFixture ()
		{
			_proc = new Mongod ();
		}

		[TestFixtureTearDown]
		public void CloseFixture ()
		{
			_proc.Dispose ();
		}

		[SetUp]
		public void Setup ()
		{
			var admin = new MongoAdmin (TestHelper.ConnectionString("pooling=false&strict=true","admin",null,null));
			_server = Mongo.Create (TestHelper.ConnectionString("pooling=false","NormTests",null, null));
			_collection = _server.GetCollection<Person> ("People");
			_buildInfo = admin.BuildInfo ();
			//cause the collection to exist on the server by inserting, then deleting some things.
			_collection.Insert (new Person ());
			_collection.Delete (new {  });
		}

		[TearDown]
		public void TearDown ()
		{
			_server.Database.DropCollection ("People");
			using (var admin = new MongoAdmin (TestHelper.ConnectionString("pooling=false","NormTests",null, null))) {
				admin.DropDatabase ();
			}
			_server.Dispose ();
		}


		[Test]
		public void FindUsesLimit ()
		{
			_collection.Insert (new Person { Name = "BBB" });
			_collection.Insert (new Person { Name = "CCC" });
			_collection.Insert (new Person { Name = "AAA" });
			_collection.Insert (new Person { Name = "DDD" });
			
			var result = _collection.Find (new {  }, 3).ToArray ();
			Assert.AreEqual (3, result.Length);
		}

		[Test]
		public void MongoCollection_Supports_LINQ ()
		{
			_collection.Insert (new Person { Name = "BBB" });
			_collection.Insert (new Person { Name = "CCC" });
			_collection.Insert (new Person { Name = "AAA" });
			_collection.Insert (new Person { Name = "DDD" });
			
			var result = _collection.AsQueryable ().Where (y => y.Name == "AAA").ToArray ();
			Assert.AreEqual (1, result.Length);
		}

		[Test]
		public void Count_Works ()
		{
			_collection.Insert (new Person { Name = "BBB" });
			_collection.Insert (new Person { Name = "CCC" });
			_collection.Insert (new Person { Name = "AAA" });
			_collection.Insert (new Person { Name = "DDD" });
			
			var result = _collection.Count ();
			Assert.AreEqual (4, result);
		}


		[Test]
		public void Count_With_Filter_Works ()
		{
			_collection.Insert (new Person { Name = "BBB" });
			_collection.Insert (new Person { Name = "CCC" });
			_collection.Insert (new Person { Name = "AAA" });
			_collection.Insert (new Person { Name = "DDD" });
			
			var result = _collection.Count (new { Name = "AAA" });
			Assert.AreEqual (1, result);
		}

		[Test]
		public void DateTime_GreaterThan_Qualifier_Works ()
		{
			_collection.Insert (new Person { Birthday = new DateTime (1910, 1, 1) });
			_collection.Insert (new Person { Birthday = new DateTime (1920, 1, 1) });
			_collection.Insert (new Person { Birthday = new DateTime (1930, 1, 1) });
			
			var find = _collection.Find (new { Birthday = Q.GreaterThan (new DateTime (1920, 1, 1)) });
			Assert.AreEqual (1, find.Count ());
		}

		[Test]
		public void Element_Match_Matches ()
		{
			using (var db = Mongo.Create (TestHelper.ConnectionString ())) {
				var coll = db.GetCollection<Post> ();
				coll.Delete (new {  });
				coll.Insert (new Post { Comments = new Comment[] { new Comment { Text = "xabc" }, new Comment { Text = "abc" } } }, new Post { Tags = new String[] { "hello", "world" } }, new Post { Comments = new Comment[] { new Comment { Text = "xyz" }, new Comment { Text = "abc" } } });
				
				Assert.AreEqual (1, coll.Find (new { Comments = Q.ElementMatch (new { Text = "xyz" }) }).Count ());
				Assert.AreEqual (2, coll.Find (new { Comments = Q.ElementMatch (new { Text = Q.Matches ("^x") }) }).Count ());
			}
		}

		[Test]
		public void Where_Qualifier_Works ()
		{
			_collection.Insert (new Person { Name = "Gnomey" });
			_collection.Insert (new Person { Name = "kde" });
			_collection.Insert (new Person { Name = "Elfy" });
			
			var find = _collection.Find (Q.Where ("this.Name == 'Elfy';"));
			Assert.AreEqual (1, find.Count ());
		}

		[Test]
		public void Find_Uses_Limit_Orderby_And_Skip ()
		{
			_collection.Insert (new Person { Name = "AAA" });
			_collection.Insert (new Person { Name = "BBB" });
			_collection.Insert (new Person { Name = "CCC" });
			_collection.Insert (new Person { Name = "AAA" });
			_collection.Insert (new Person { Name = "DDD" });
			
			var result = _collection.Find (new { Name = Q.NotEqual (new int? ()) }, new { Name = OrderBy.Descending }, 3, 1).ToArray ();
			Assert.AreEqual (3, result.Length);
			Assert.AreEqual ("CCC", result[0].Name);
			Assert.AreEqual ("BBB", result[1].Name);
			Assert.AreEqual ("AAA", result[2].Name);
		}

		[Test]
		public void Find_Uses_Query_And_Orderby ()
		{
			_collection.Insert (new Person { Name = "AAA" });
			_collection.Insert (new Person { Name = "BBB" });
			_collection.Insert (new Person { Name = "CCC" });
			_collection.Insert (new Person { Name = "AAA" });
			_collection.Insert (new Person { Name = "DDD" });
			
			var result = _collection.Find (new { Name = Q.NotEqual ("AAA") }, new { Name = OrderBy.Descending }).ToArray ();
			Assert.AreEqual (3, result.Length);
			Assert.AreEqual ("DDD", result[0].Name);
			Assert.AreEqual ("CCC", result[1].Name);
			Assert.AreEqual ("BBB", result[2].Name);
		}

		[Test]
		public void Find_Uses_Query_And_Orderby_And_Limit ()
		{
			_collection.Insert (new Person { Name = "AAA" });
			_collection.Insert (new Person { Name = "BBB" });
			_collection.Insert (new Person { Name = "CCC" });
			_collection.Insert (new Person { Name = "AAA" });
			_collection.Insert (new Person { Name = "DDD" });
			
			var result = _collection.Find (new { Name = Q.NotEqual ("DDD") }, new { Name = OrderBy.Descending }, 2, 0).ToArray ();
			Assert.AreEqual (2, result.Length);
			Assert.AreEqual ("CCC", result[0].Name);
			Assert.AreEqual ("BBB", result[1].Name);
		}

		[Test]
		public void Find_Uses_Null_Qualifier ()
		{
			_collection.Insert (new Person { Name = null });
			_collection.Insert (new Person { Name = "BBB" });
			_collection.Insert (new Person { Name = "CCC" });
			_collection.Insert (new Person { Name = "AAA" });
			_collection.Insert (new Person { Name = "DDD" });
			
			var result = _collection.Find (new { Name = Q.IsNull () }, new { Name = OrderBy.Descending }, 2, 0).ToArray ();
			Assert.AreEqual (1, result.Length);
			Assert.AreEqual (null, result[0].Name);
			
			result = _collection.Find (new { Name = Q.IsNotNull () }, new { Name = OrderBy.Descending }).ToArray ();
			Assert.AreEqual (4, result.Length);
			Assert.AreEqual ("DDD", result[0].Name);
		}

		[Test]
		public void FindUsesLimitAndSkip ()
		{
			_collection.Insert (new Person { Name = "BBB" });
			_collection.Insert (new Person { Name = "CCC" });
			_collection.Insert (new Person { Name = "AAA" });
			_collection.Insert (new Person { Name = "DDD" });
			
			var result = _collection.Find (new {  }, 1, 1).ToArray ();
			Assert.AreEqual (1, result.Length);
			Assert.AreEqual ("CCC", result[0].Name);
		}

		[Test]
		public void FindCanQueryEmbeddedArray ()
		{
			_collection.Delete (new {  });
				
			var person1 = new Person { Name = "Joe Cool", Address = { Street = "123 Main St", City = "Anytown", State = "CO", Zip = "45123" } };
			var person2 = new Person { Name = "Sam Cool", Address = { Street = "300 Main St", City = "Anytown", State = "CO", Zip = "45123" }, Relatives = new List<string> { "Emma", "Bruce", "Charlie" } };
			_collection.Insert (person1);
			_collection.Insert (person2);
			
			var elem = new Expando ();
			elem["Relatives"] = "Charlie";
			var a = _collection.Find (elem).ToArray ();
			Assert.AreEqual (1, a.Length);
		}


		[Test]
		public void BasicQueryUsingProperty ()
		{
			_collection.Insert (new Person { Name = "Lisa Cool", Address = { Street = "300 Main St", City = "Anytown", State = "CO", Zip = "45123" } });
			_collection.Insert (new Person { Name = "Joe Cool", Address = { Street = "123 Main St", City = "Anytown", State = "CO", Zip = "45123" } });
			_collection.Insert (new Person { Name = "Sam Cool", Address = { Street = "300 Main St", City = "Anytown", State = "CO", Zip = "45123" } });
			
			var matchRegex = new Regex ("^.{4}Cool$");
			var results = _collection.Find (new { Name = matchRegex }).ToArray ();
			Assert.AreEqual (2, results.Length);
			Assert.True (results.All (y => matchRegex.IsMatch (y.Name)));
		}

		[Test]
		public void BasicQueryWithSort ()
		{
			//remove everything from the collection.
			_collection.Delete (new {  });
			
			_collection.Insert (new Person { Name = "Third", LastContact = new DateTime (2010, 1, 1) });
			_collection.Insert (new Person { Name = "First", LastContact = new DateTime (2000, 1, 1) });
			_collection.Insert (new Person { Name = "Second", LastContact = new DateTime (2005, 1, 1) });
			
			var people = _collection.Find (new {  }, new { LastContact = 1 }).ToArray ();
			Assert.AreEqual (3, people.Length);
			Assert.AreEqual ("First", people[0].Name);
			Assert.AreEqual ("Second", people[1].Name);
			Assert.AreEqual ("Third", people[2].Name);
		}

		[Test]
		public void BasicQueryWithMultiSortOrdering ()
		{
			//remove everything from the collection.
			_collection.Delete (new {  });
			
			_collection.Insert (new Person { Name = "Third", LastContact = new DateTime (2010, 1, 1) });
			_collection.Insert (new Person { Name = "First", LastContact = new DateTime (2005, 1, 1) });
			_collection.Insert (new Person { Name = "Second", LastContact = new DateTime (2005, 1, 1) });
			
			var people = _collection.Find (new {  }, new { LastContact = -1, Name = 1 }).ToArray ();
			Assert.AreEqual (3, people.Length);
			Assert.AreEqual ("Third", people[0].Name);
			Assert.AreEqual ("First", people[1].Name);
			Assert.AreEqual ("Second", people[2].Name);
		}

		[Test]
		public void BasicQueryUsingChildProperty ()
		{
			_collection.Insert (new Person { Name = "Joe Cool", Address = { Street = "123 Main St", City = "Anytown", State = "CO", Zip = "45123" } });
			_collection.Insert (new Person { Name = "Sam Cool", Address = { Street = "300 Main St", City = "Anytown", State = "CO", Zip = "45123" } });
			
			var query = new Expando ();
			query["Address.City"] = Q.Equals<string> ("Anytown");
			
			var results = _collection.Find (query);
			Assert.AreEqual (2, results.Count ());
		}
		[Test]
		public void QueryWithinEmbeddedArray ()
		{
			
			var post1 = new Person { Name = "First", Relatives = new List<String> { "comment1", "comment2" } };
			var post2 = new Person { Name = "Second", Relatives = new List<String> { "commentA", "commentB" } };
			_collection.Insert (post1);
			_collection.Insert (post2);
			
			var results = _collection.Find (new { Relatives = "commentA" });
			Assert.AreEqual ("Second", results.First ().Name);
		}


		[Test]
		public void Distinct_On_Collection_Should_Return_Arrays_As_Value_If_Earlier_Than_1_5_0 ()
		{
			var isLessThan150 = Regex.IsMatch (_buildInfo.Version, "^([01][.][01234])");
			
			// Any version earlier than MongoDB 1.5.0
			if (isLessThan150) {
				_collection.Insert (new Person { Name = "Joe Cool", Relatives = new List<string> (new[] { "Tom Cool", "Sam Cool" }) });
				_collection.Insert (new Person { Name = "Sam Cool", Relatives = new List<string> (new[] { "Joe Cool", "Jay Cool" }) });
				_collection.Insert (new Person { Name = "Ted Cool", Relatives = new List<string> (new[] { "Tom Cool", "Sam Cool" }) });
				_collection.Insert (new Person { Name = "Jay Cool", Relatives = new List<string> (new[] { "Sam Cool" }) });
				
				var results = _collection.Distinct<string[]> ("Relatives");
				Assert.AreEqual (3, results.Count ());
			}
		}

		[Test]
		public void Distinct_On_Collection_Should_Return_Array_Values_In_1_5_0_Or_Later ()
		{
			var isLessThan150 = Regex.IsMatch (_buildInfo.Version, "^([01][.][01234])");
			
			// Any version MongoDB 1.5.0 +
			if (!isLessThan150) {
				_collection.Insert (new Person { Name = "Joe Cool", Relatives = new List<string> (new[] { "Tom Cool", "Sam Cool" }) });
				_collection.Insert (new Person { Name = "Sam Cool", Relatives = new List<string> (new[] { "Joe Cool", "Jay Cool" }) });
				_collection.Insert (new Person { Name = "Ted Cool", Relatives = new List<string> (new[] { "Tom Cool", "Sam Cool" }) });
				_collection.Insert (new Person { Name = "Jay Cool", Relatives = new List<string> (new[] { "Sam Cool" }) });
				
				var results = _collection.Distinct<string> ("Relatives");
				Assert.AreEqual (4, results.Count ());
			}
		}

		[Test]
		public void DistinctOnSimpleProperty ()
		{
			_collection.Insert (new Person { Name = "Joe Cool", Relatives = new List<string> (new[] { "Tom Cool", "Sam Cool" }) });
			_collection.Insert (new Person { Name = "Sam Cool", Relatives = new List<string> (new[] { "Joe Cool", "Jay Cool" }) });
			_collection.Insert (new Person { Name = "Ted Cool", Relatives = new List<string> (new[] { "Tom Cool", "Sam Cool" }) });
			_collection.Insert (new Person { Name = "Jay Cool", Relatives = new List<string> (new[] { "Sam Cool" }) });
			
			var results = _collection.Distinct<string> ("Name");
			Assert.AreEqual (4, results.Count ());
		}

		[Test]
		public void DistinctOnComplexProperty ()
		{
			_collection.Insert (new Person { Name = "Joe Cool", Address = new Address { State = "CA" } });
			_collection.Insert (new Person { Name = "Sam Cool", Address = new Address { State = "CA" } });
			_collection.Insert (new Person { Name = "Ted Cool", Address = new Address { State = "CA", Zip = "90010" } });
			_collection.Insert (new Person { Name = "Jay Cool", Address = new Address { State = "NY" } });
			
			var results = _collection.Distinct<Address> ("Address");
			Assert.AreEqual (3, results.Count ());
		}

		[Test]
		public void FindAndModify ()
		{
			_collection.Insert (new Person { Name = "Joe Cool", Age = 10 });
			
			var update = new Expando ();
			update["$inc"] = new { Age = 1 };
			
			var result = _collection.FindAndModify (new { Name = "Joe Cool" }, update);
			Assert.AreEqual (10, result.Age);
			
			var result2 = _collection.Find (new { Name = "Joe Cool" }).FirstOrDefault ();
			Assert.AreEqual (11, result2.Age);
		}

		[Test]
		public void FindAndModifyWithSort ()
		{
			_collection.Insert (new Person { Name = "Joe Cool", Age = 10 });
			_collection.Insert (new Person { Name = "Joe Cool", Age = 15 });
			
			var update = new Expando ();
			update["$inc"] = new { Age = 1 };
			
			var result = _collection.FindAndModify (new { Name = "Joe Cool" }, update, new { Age = Norm.OrderBy.Descending });
			Assert.AreEqual (15, result.Age);
			
			var result2 = _collection.Find (new { Name = "Joe Cool" }).OrderByDescending (x => x.Age).ToList ();
			Assert.AreEqual (16, result2[0].Age);
			Assert.AreEqual (10, result2[1].Age);
			
		}

		[Test]
		public void FindAndModifyReturnsNullWhenQueryNotFound ()
		{
			_collection.Insert (new Person { Name = "Joe Cool", Age = 10 });
			_collection.Insert (new Person { Name = "Joe Cool", Age = 15 });
			
			var update = new Expando ();
			update["$inc"] = new { Age = 1 };
			
			var result = _collection.FindAndModify (new { Name = "Joe Cool1" }, update, new { Age = Norm.OrderBy.Descending });
			Assert.Null (result);
			
			var result2 = _collection.Find (new { Age = 15 }).ToList ();
			Assert.AreEqual (1, result2.Count);
		}

		[Test]
		public void SliceOperatorBringsBackCorrectItems ()
		{
			/*
			var isLessThan151 = Regex.IsMatch (_buildInfo.Version, "^(([01][.][01234])|(1.5.0))");
			if (!isLessThan151) {
				Person p = new Person { Relatives = new List<string> { "Bob", "Joe", "Helen" } };
				_collection.Insert (p);
				var result = _collection.Find (new {  }, new { _id = 1 }, new { Relatives = Q.Slice (1) }, 1, 0).FirstOrDefault ();
				Assert.NotNull (result);
				Assert.AreEqual ("Joe", result.Relatives.First ());
				
				result = _collection.Find (new {  }, new { _id = 1 }, new { Relatives = Q.Slice (1, 2) }, 1, 0).FirstOrDefault ();
				Assert.NotNull (result);
				Assert.True ((new[] { "Joe", "Helen" }).SequenceEqual (result.Relatives));
			}
			*/
		}
	}
}