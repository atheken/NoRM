using System.Linq;
using NUnit.Framework;

namespace Norm.Tests
{
    [TestFixture]
    public class MapReduceTests
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

        private const string _map = "function(){emit(0, this.Price);}";
        private const string _reduce = "function(key, values){var sumPrice = 0;for(var i = 0; i < values.length; ++i){sumPrice += values[i];} return sumPrice;}";

        [SetUp]
        public void Setup()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false&strict=false")))
            {
                mongo.Database.DropCollection("ReduceProduct");
            }
        }
        
        [Test]
        public void TypedMapReduceOptionSetsCollectionName()
        {
            var options = new MapReduceOptions<ReduceProduct>();
            Assert.AreEqual(typeof(ReduceProduct).Name, options.CollectionName);
        }

        [Test]
        public void MapReduceCreatesACollection()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false")))
            {
                mongo.GetCollection<ReduceProduct>().Insert(new ReduceProduct { Price = 1.5f }, new ReduceProduct { Price = 2.5f }); 
                var mr = mongo.Database.CreateMapReduce();
                var result = mr.Execute(new MapReduceOptions<ReduceProduct> {Map = _map, Reduce = _reduce});
                var found = false;
                foreach(var c in mongo.Database.GetAllCollections())
                {
                    if (c.Name.EndsWith(result.Result))
                    {
                        found = true;
                        break;
                    }
                }
                Assert.AreEqual(true, found);
            }
        }

        [Test]
        public void TemporaryCollectionIsCleanedUpWhenConnectionIsClosed()
        {
            string name;
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false")))
            {
                mongo.GetCollection<ReduceProduct>().Insert(new ReduceProduct { Price = 1.5f }, new ReduceProduct { Price = 2.5f }); 
                var mr = mongo.Database.CreateMapReduce();
                name = mr.Execute(new MapReduceOptions<ReduceProduct> { Map = _map, Reduce = _reduce }).Result;
            }

            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false")))
            {
                foreach (var c in mongo.Database.GetAllCollections())
                {
                    Assert.AreEqual(false, c.Name.EndsWith(name));
                }
            }
        }

        
        [Test]
        public void CreatesACollectionWithTheSpecifiedOutputName()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false")))
            {
                mongo.GetCollection<ReduceProduct>().Insert(new ReduceProduct { Price = 1.5f }, new ReduceProduct { Price = 2.5f });
                var mr = mongo.Database.CreateMapReduce();
                
                    var result = mr.Execute(new MapReduceOptions<ReduceProduct> { Map = _map, Reduce = _reduce, OutputCollectionName = "TempMr" });
                    Assert.AreEqual("TempMr", result.Result);
                
            }
        }



        [Test]
        public void ActuallydoesAMapAndReduce()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false")))
            {
                mongo.GetCollection<ReduceProduct>().Insert(new ReduceProduct {Price = 1.5f},
                                                            new ReduceProduct {Price = 2.5f});
                var mr = mongo.Database.CreateMapReduce();

                var response =
                    mr.Execute(new MapReduceOptions<ReduceProduct> {Map = _map, Reduce = _reduce, Permanant = true});
                var collection = response.GetCollection<ProductSum>();
                var r = collection.Find().FirstOrDefault();
                Assert.AreEqual(0, r.Id);
                Assert.AreEqual(4, r.Value);

            }
        }

        [Test]
        public void MapReduceWithQuerySpecified()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false")))
            {
                mongo.GetCollection<ReduceProduct>().Insert(new ReduceProduct {Price = 1.5f},
                                                            new ReduceProduct {Price = 2.5f},
                                                            new ReduceProduct {Price = 2.5f});
                var mr = mongo.Database.CreateMapReduce();

                var _query = new {Price = Q.GreaterThan(2)};
                var response =
                    mr.Execute(new MapReduceOptions<ReduceProduct> {Map = _map, Reduce = _reduce, Query = _query});
                var collection = response.GetCollection<ProductSum>();
                var r = collection.Find().FirstOrDefault();
                Assert.AreEqual(0, r.Id);
                Assert.AreEqual(5, r.Value);

            }
        }

        [Test]
        public void MapReduceWithGenericMapReduceResponse()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false")))
            {
                mongo.GetCollection<ReduceProduct>().Insert(new ReduceProduct { Price = 1.5f },
                                                            new ReduceProduct { Price = 2.5f },
                                                            new ReduceProduct { Price = 2.5f });
                var mr = mongo.Database.CreateMapReduce();

                var _query = new { Price = Q.GreaterThan(2) };
                var response =
                    mr.Execute(new MapReduceOptions<ReduceProduct> { Map = _map, Reduce = _reduce, Query = _query });
                var collection = response.GetCollection<MapReduceResult<int, int>>();
                var r = collection.Find().FirstOrDefault();
                Assert.AreEqual(0, r.Key);
                Assert.AreEqual(5, r.Value);

            }
        }


        [Test]
        public void SettingLimitLimitsTheNumberOfResults()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false")))
            {
                mongo.GetCollection<ReduceProduct>().Insert(new ReduceProduct {Price = 1.5f},
                                                            new ReduceProduct {Price = 2.5f});
                var mr = mongo.Database.CreateMapReduce();

                var response =
                    mr.Execute(new MapReduceOptions<ReduceProduct>
                                   {Map = "function(){emit(this._id, this.Price);}", Reduce = _reduce, Limit = 1});
                var collection = response.GetCollection<ProductSumObjectId>();
                Assert.AreEqual(1, collection.Find().Count());

            }
        }

        [Test]
        public void NotSettingLimitDoesntLimitTheNumberOfResults()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false")))
            {
                mongo.GetCollection<ReduceProduct>().Insert(new ReduceProduct {Price = 1.5f},
                                                            new ReduceProduct {Price = 2.5f});
                var mr = mongo.Database.CreateMapReduce();

                var response =
                    mr.Execute(new MapReduceOptions<ReduceProduct>
                                   {Map = "function(){emit(this._id, this.Price);}", Reduce = _reduce});
                var collection = response.GetCollection<ProductSumObjectId>();
                Assert.AreEqual(2, collection.Find().Count());

            }
        }

        [Test]
        public void FinalizesTheResults()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false")))
            {
                mongo.GetCollection<ReduceProduct>().Insert(new ReduceProduct {Price = 1.5f},
                                                            new ReduceProduct {Price = 2.5f});
                var mr = mongo.Database.CreateMapReduce();

                const string finalize = "function(key, value){return 1;}";
                var response =
                    mr.Execute(new MapReduceOptions<ReduceProduct>
                                   {Map = _map, Reduce = _reduce, Permanant = true, Finalize = finalize});
                var collection = response.GetCollection<ProductSum>();
                var r = collection.Find().FirstOrDefault();
                Assert.AreEqual(0, r.Id);
                Assert.AreEqual(1, r.Value);

            }
        }
    }
}