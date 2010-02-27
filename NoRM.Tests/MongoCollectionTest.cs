/*
namespace NoRM.Tests
{
    [TestFixture]
    [Category("Hits MongoDB")]
    public class MongoCollectionTest
    {
        public class MiniObject
        {
            public ObjectId _id { get; set; }
        }

        private Mongo _server;
        private MongoDatabase _db;

        [TestFixtureSetUp]
        public void Setup()
        {
            this._server = new Mongo();
            this._db = this._server.GetDatabase("test" + Guid.NewGuid().ToString().Substring(0, 5));
            
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            this._server.DropDatabase(this._db.DatabaseName);
        }

        [Test]
        public void Distinct_For_Key_Returns_Correct_Set()
        {
            var server = new Mongo();
            var testDB = server.GetDatabase("test");
            testDB.DropCollection("testObjects");
            var testColl = testDB.GetCollection<MiniObject>("testObjects");
            var cache = new List<MiniObject>();
            for (var i = 0; i < 10; i++)
            {
                cache.Add(new MiniObject { _id = ObjectId.NewOID() });
            }

            testColl.Insert(cache);

            Assert.AreEqual(cache.Count, testColl.Distinct<ObjectId>("_id").Count());
        }

        [Test]
        public void Collection_Statistics_Returns()
        {
            var coll = this._db.GetCollection<MiniObject>("stats_test");

            //collections & dbs are lazily created - force it to happen.
            coll.Insert(new MiniObject());

            var stats = this._db.GetCollectionStatistics("stats_test");
            Assert.AreEqual(coll.FullyQualifiedName, stats.Ns);

        }
    }
}
*/