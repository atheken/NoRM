using System.Linq;
using Xunit;

namespace Norm.Tests
{
    public class MapReduceTests
    {
        private const string _map = "function(){emit(0, this.Price);}";
        private const string _reduce = "function(key, values){var sumPrice = 0;for(var i = 0; i < values.length; ++i){sumPrice += values[i];} return sumPrice;}";

        public MapReduceTests()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false&strict=false")))
            {
                mongo.Database.DropCollection("ReduceProduct");
            }
        }
        
        [Fact]
        public void TypedMapReduceOptionSetsCollectionName()
        {
            var options = new MapReduceOptions<ReduceProduct>();
            Assert.Equal(typeof(ReduceProduct).Name, options.CollectionName);
        }

        [Fact]
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
                Assert.Equal(true, found);
            }
        }

        [Fact]
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
                    Assert.Equal(false, c.Name.EndsWith(name));
                }
            }
        }

        
        [Fact]
        public void CreatesACollectionWithTheSpecifiedOutputName()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString("pooling=false")))
            {
                mongo.GetCollection<ReduceProduct>().Insert(new ReduceProduct { Price = 1.5f }, new ReduceProduct { Price = 2.5f });
                var mr = mongo.Database.CreateMapReduce();
                
                    var result = mr.Execute(new MapReduceOptions<ReduceProduct> { Map = _map, Reduce = _reduce, OutputCollectionName = "TempMr" });
                    Assert.Equal("TempMr", result.Result);
                
            }
        }



        [Fact]
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
                Assert.Equal(0, r.Id);
                Assert.Equal(4, r.Value);

            }
        }

        [Fact]
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
                Assert.Equal(0, r.Id);
                Assert.Equal(5, r.Value);

            }
        }

        [Fact]
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
                Assert.Equal(0, r.Key);
                Assert.Equal(5, r.Value);

            }
        }


        [Fact]
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
                Assert.Equal(1, collection.Find().Count());

            }
        }

        [Fact]
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
                Assert.Equal(2, collection.Find().Count());

            }
        }

        [Fact]
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
                Assert.Equal(0, r.Id);
                Assert.Equal(1, r.Value);

            }
        }
    }
}