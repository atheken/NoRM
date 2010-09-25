using System;
using System.Linq;
using Norm.BSON;
using Norm.Collections;
using NUnit.Framework;

namespace Norm.Tests
{
	[TestFixture]
	public class GeospatialTests
	{
		private IMongo _server;
		private IMongoCollection<GeoTestClass> _collection;
		private Mongod _proc;
		private String _connString =TestHelper.ConnectionString("pooling=false");
		
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
		public void Setup()
        {
			
            _server = Mongo.Create(_connString);
			_collection = _server.GetCollection<GeoTestClass>("LatLngTests");

			_collection.Insert(new GeoTestClass { Location = new LatLng { Latitude = 57.1056, Longitude = 12.2508 }, Name = "Varberg"});
			_collection.Insert(new GeoTestClass { Location = new LatLng { Latitude = 57.7, Longitude = 11.9166667 }, Name = "Gothenburg" }); //  ~70km North of Varberg
			_collection.Insert(new GeoTestClass { Location = new LatLng { Latitude = 29.950975, Longitude = -90.081217 }, Name = "New Orleans Saints" });
			_collection.Insert(new GeoTestClass { Location = new LatLng { Latitude = 44.973876, Longitude = -93.258133 }, Name = "Minnesota Vikings" });
			_collection.Insert(new GeoTestClass { Location = new LatLng { Latitude = 0, Longitude = 0 }, Name = "Center of the Earth" });
			_collection.CreateGeoIndex(geo => geo.Location, "loc", true);
        }
		
		[TearDown]
        public void TearDown()
        {
			_server.Database.DropCollection("LatLngTests");
            using (var admin = new MongoAdmin(_connString))
            {
                admin.DropDatabase();
            }
            _server.Dispose();
        }

		[Test]
		public void LatLngWorks()
		{
			var location = new LatLng { Latitude = 57, Longitude = 12 };
			var values = location.ToArray();

			Assert.AreEqual(2, values.Length);
			Assert.AreEqual(57, values[0]);
			Assert.AreEqual(12, values[1]);
		}

		[Test]
		public void NearQualifier()
		{
			var result = _collection.Find(new { Location = Q.Near(new LatLng { Latitude = 0, Longitude = 0 }) }).ToArray();
			Assert.AreEqual(5, result.Length);
		}

        [Test]
        public void NearQualifier1()
        {
			var result = _collection.Find(new { Location = Q.Near(new LatLng { Latitude = 0, Longitude = 0 }) }, 1).FirstOrDefault();
            Assert.AreEqual("Center of the Earth", result.Name);
        }

		[Test]
		public void NearQualifier2()
		{
			var result = _collection.Find(new { Location = Q.Near(new LatLng { Latitude = 57.1056, Longitude = 12.2508 }) }, 1).FirstOrDefault();
			Assert.AreEqual("Varberg", result.Name);
		}

		[Test]
		public void NearQualifier3()
		{
			var result = _collection.Find(new { Location = Q.Near(new LatLng { Latitude = 57.7, Longitude = 11.9166667 }) }, 1).FirstOrDefault();
			Assert.AreEqual("Gothenburg", result.Name);
		}

		[Test]
		public void NearQualifier4()
		{
			var result = _collection.Find(new { Location = Q.Near(new LatLng { Latitude = 29.950975, Longitude = -90.081217 }) }, 1).FirstOrDefault();
			Assert.AreEqual("New Orleans Saints", result.Name);
		}

		[Test]
		public void NearQualifier5()
		{
			var result = _collection.Find(new { Location = Q.Near(new LatLng { Latitude = 44.973876, Longitude = -93.258133 }) }, 1).FirstOrDefault();
			Assert.AreEqual("Minnesota Vikings", result.Name);
		}

		[Test]
		public void NearQualifier6()
		{
			var result = _collection.Find(new { Location = Q.Near(new LatLng { Latitude = 57.1056, Longitude = 12.2508 }) }, 2).Select(g => g.Name).ToArray();
			Assert.Contains("Varberg", result);
			Assert.Contains("Gothenburg", result);
		}

		[Test]
		public void WithinCircleQualifier_40()
		{
			var result = _collection.Find(new { Location = Q.WithinCircle(new LatLng { Latitude = 57.1056, Longitude = 12.2508 }, LatLng.Kilometers2ArcDegree(40)) }).ToArray();
			Assert.AreEqual(1, result.Length);
			Assert.AreEqual("Varberg", result[0].Name);
		}

		[Test]
		public void WithinCircleQualifier_80()
		{
			var result = _collection.Find(new { Location = Q.WithinCircle(new LatLng { Latitude = 57.1056, Longitude = 12.2508 }, LatLng.Kilometers2ArcDegree(80)) }).Select(g => g.Name).ToArray();
			Assert.AreEqual(2, result.Length);
			Assert.Contains("Varberg", result);
			Assert.Contains("Gothenburg", result);
		}

		[Test]
		public void WithinBoxQualifier()
		{
			var result = _collection.Find(new { Location = Q.WithinBox(new LatLng { Latitude = 28.420391, Longitude = -94.614258 }, new LatLng { Latitude = 45.583290, Longitude = -85.649414 }) }).Select(g => g.Name).ToArray();
			Assert.AreEqual(2, result.Length);
			Assert.Contains("New Orleans Saints", result);
			Assert.Contains("Minnesota Vikings", result);
		}

		private class GeoTestClass
		{
			[MongoIdentifier]
			public ObjectId _id { get; set; }
			public LatLng Location { get; set; }
			public String Name { get; set; }
		}
	}
}
