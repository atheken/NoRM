using System;
using System.Linq;
using Norm.BSON;
using Norm.Responses;
using Xunit;
using Norm.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Norm.Tests
{
    public class QueryTests : IDisposable
    {
        private readonly Mongo _server;
        private BuildInfoResponse _buildInfo = null;
        private readonly MongoCollection<Person> _collection;
        public QueryTests()
        {
            var admin = new MongoAdmin("mongodb://localhost/admin?pooling=false&strict=true");
            _server = Mongo.Create("mongodb://localhost/NormTests?pooling=false");
            _collection = _server.GetCollection<Person>("People");
            _buildInfo = admin.BuildInfo();
            //cause the collection to exist on the server by inserting, then deleting some things.
            _collection.Insert(new Person());
            _collection.Delete(new { });
        }
        public void Dispose()
        {
            _server.Database.DropCollection("People");
            using (var admin = new MongoAdmin("mongodb://localhost/NormTests?pooling=false"))
            {
                admin.DropDatabase();
            }
            _server.Dispose();
        }


        [Fact]
        public void FindUsesLimit()
        {
            _collection.Insert(new Person { Name = "BBB" });
            _collection.Insert(new Person { Name = "CCC" });
            _collection.Insert(new Person { Name = "AAA" });
            _collection.Insert(new Person { Name = "DDD" });

            var result = _collection.Find(new { }, 3).ToArray();
            Assert.Equal(3, result.Length);
        }

        [Fact(Skip="broken")]
        public void MongoCollection_Supports_LINQ()
        {
            _collection.Insert(new Person { Name = "BBB" });
            _collection.Insert(new Person { Name = "CCC" });
            _collection.Insert(new Person { Name = "AAA" });
            _collection.Insert(new Person { Name = "DDD" });

            //var result = _collection.Where(y => y.Name == "AAA").ToArray();
            //Assert.Equal(1, result.Length);
        }

        [Fact]
        public void Count_Works()
        {
            _collection.Insert(new Person { Name = "BBB" });
            _collection.Insert(new Person { Name = "CCC" });
            _collection.Insert(new Person { Name = "AAA" });
            _collection.Insert(new Person { Name = "DDD" });

            var result = _collection.Count();
            Assert.Equal(4, result);
        }


        [Fact]
        public void Count_With_Filter_Works()
        {
            _collection.Insert(new Person { Name = "BBB" });
            _collection.Insert(new Person { Name = "CCC" });
            _collection.Insert(new Person { Name = "AAA" });
            _collection.Insert(new Person { Name = "DDD" });

            var result = _collection.Count(new { Name = "AAA" });
            Assert.Equal(1, result);
        }

        [Fact]
        public void DateTime_GreaterThan_Qualifier_Works()
        {
            _collection.Insert(new Person { Birthday = new DateTime(1910, 1, 1) });
            _collection.Insert(new Person { Birthday = new DateTime(1920, 1, 1) });
            _collection.Insert(new Person { Birthday = new DateTime(1930, 1, 1) });

            var find = _collection.Find(new { Birthday = Q.GreaterThan(new DateTime(1920, 1, 1)) });
            Assert.Equal(1, find.Count());
        }

        [Fact]
        public void Element_Match_Matches()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                var coll = db.GetCollection<Post>();
                coll.Delete(new { });
                coll.Insert(new Post
                {
                    Comments = new Comment[] { 
                            new Comment { Text = "xabc" },
                            new Comment { Text = "abc" } 
                        }
                },
                    new Post { Tags = new String[] { "hello", "world" } },
                    new Post
                    {
                        Comments = new Comment[] { 
                            new Comment { Text = "xyz" },
                            new Comment { Text = "abc" } 
                        }
                    });

                Assert.Equal(1, coll.Find(new { Comments = Q.ElementMatch(new { Text = "xyz" }) }).Count());
                Assert.Equal(2, coll.Find(new { Comments = Q.ElementMatch(new { Text = Q.Matches("^x") }) }).Count());
            }
        }

        [Fact]
        public void Where_Qualifier_Works()
        {
            _collection.Insert(new Person { Name = "Gnomey" });
            _collection.Insert(new Person { Name = "kde" });
            _collection.Insert(new Person { Name = "Elfy" });

            var find = _collection.Find(Q.Where("this.Name === 'Elfy';"));
            Assert.Equal(1, find.Count());
        }

        [Fact]
        public void Find_Uses_Limit_Orderby_And_Skip()
        {
            _collection.Insert(new Person { Name = "AAA" });
            _collection.Insert(new Person { Name = "BBB" });
            _collection.Insert(new Person { Name = "CCC" });
            _collection.Insert(new Person { Name = "AAA" });
            _collection.Insert(new Person { Name = "DDD" });

            var result = _collection.Find(new { Name = Q.NotEqual(new int?()) }, new { Name = OrderBy.Descending }, 3, 1).ToArray();
            Assert.Equal(3, result.Length);
            Assert.Equal("CCC", result[0].Name);
            Assert.Equal("BBB", result[1].Name);
            Assert.Equal("AAA", result[2].Name);
        }

        [Fact]
        public void Find_Uses_Query_And_Orderby()
        {
            _collection.Insert(new Person { Name = "AAA" });
            _collection.Insert(new Person { Name = "BBB" });
            _collection.Insert(new Person { Name = "CCC" });
            _collection.Insert(new Person { Name = "AAA" });
            _collection.Insert(new Person { Name = "DDD" });

            var result = _collection.Find(new { Name = Q.NotEqual("AAA") }, new { Name = OrderBy.Descending }).ToArray();
            Assert.Equal(3, result.Length);
            Assert.Equal("DDD", result[0].Name);
            Assert.Equal("CCC", result[1].Name);
            Assert.Equal("BBB", result[2].Name);
        }

        [Fact]
        public void Find_Uses_Query_And_Orderby_And_Limit()
        {
            _collection.Insert(new Person { Name = "AAA" });
            _collection.Insert(new Person { Name = "BBB" });
            _collection.Insert(new Person { Name = "CCC" });
            _collection.Insert(new Person { Name = "AAA" });
            _collection.Insert(new Person { Name = "DDD" });

            var result = _collection.Find(new { Name = Q.NotEqual("DDD") }, new { Name = OrderBy.Descending }, 2, 0).ToArray();
            Assert.Equal(2, result.Length);
            Assert.Equal("CCC", result[0].Name);
            Assert.Equal("BBB", result[1].Name);
        }

        [Fact]
        public void Find_Uses_Null_Qualifier()
        {
            _collection.Insert(new Person { Name = null });
            _collection.Insert(new Person { Name = "BBB" });
            _collection.Insert(new Person { Name = "CCC" });
            _collection.Insert(new Person { Name = "AAA" });
            _collection.Insert(new Person { Name = "DDD" });

            var result = _collection.Find(new { Name = Q.IsNull() }, new { Name = OrderBy.Descending }, 2, 0).ToArray();
            Assert.Equal(1, result.Length);
            Assert.Equal(null, result[0].Name);

            result = _collection.Find(new { Name = Q.IsNotNull() }, new { Name = OrderBy.Descending }).ToArray();
            Assert.Equal(4, result.Length);
            Assert.Equal("DDD", result[0].Name);
        }

        [Fact]
        public void FindUsesLimitAndSkip()
        {
            _collection.Insert(new Person { Name = "BBB" });
            _collection.Insert(new Person { Name = "CCC" });
            _collection.Insert(new Person { Name = "AAA" });
            _collection.Insert(new Person { Name = "DDD" });

            var result = _collection.Find(new { }, 1, 1).ToArray();
            Assert.Equal(1, result.Length);
            Assert.Equal("CCC", result[0].Name);
        }

        [Fact]
        public void FindCanQueryEmbeddedArray()
        {
            _collection.Delete(new { });
            var person1 = new Person
            {
                Name = "Joe Cool",
                Address =
                {
                    Street = "123 Main St",
                    City = "Anytown",
                    State = "CO",
                    Zip = "45123"
                }

            };
            var person2 = new Person
            {
                Name = "Sam Cool",
                Address =
                {
                    Street = "300 Main St",
                    City = "Anytown",
                    State = "CO",
                    Zip = "45123"
                },
                Relatives = new List<string>() { "Emma", "Bruce", "Charlie" }
            };
            _collection.Insert(person1);
            _collection.Insert(person2);

            var elem = new Expando();
            elem["Relatives"] = "Charlie";
            var a = _collection.Find(elem).ToArray();
            Assert.Equal(1, a.Length);
        }


        [Fact]
        public void BasicQueryUsingProperty()
        {
            _collection.Insert(new Person { Name = "Lisa Cool", Address = { Street = "300 Main St", City = "Anytown", State = "CO", Zip = "45123" } });
            _collection.Insert(new Person { Name = "Joe Cool", Address = { Street = "123 Main St", City = "Anytown", State = "CO", Zip = "45123" } });
            _collection.Insert(new Person { Name = "Sam Cool", Address = { Street = "300 Main St", City = "Anytown", State = "CO", Zip = "45123" } });

            var matchRegex = new Regex("^.{4}Cool$");
            var results = _collection.Find(new { Name = matchRegex }).ToArray();
            Assert.Equal(2, results.Length);
            Assert.True(results.All(y => matchRegex.IsMatch(y.Name)));
        }

        [Fact]
        public void BasicQueryWithSort()
        {
            //remove everything from the collection.
            _collection.Delete(new { });

            _collection.Insert(new Person { Name = "Third", LastContact = new DateTime(2010, 1, 1) });
            _collection.Insert(new Person { Name = "First", LastContact = new DateTime(2000, 1, 1) });
            _collection.Insert(new Person { Name = "Second", LastContact = new DateTime(2005, 1, 1) });

            var people = _collection.Find(new { }, new { LastContact = 1 }).ToArray();
            Assert.Equal(3, people.Length);
            Assert.Equal("First", people[0].Name);
            Assert.Equal("Second", people[1].Name);
            Assert.Equal("Third", people[2].Name);
        }

        [Fact]
        public void BasicQueryWithMultiSortOrdering()
        {
            //remove everything from the collection.
            _collection.Delete(new { });

            _collection.Insert(new Person { Name = "Third", LastContact = new DateTime(2010, 1, 1) });
            _collection.Insert(new Person { Name = "First", LastContact = new DateTime(2005, 1, 1) });
            _collection.Insert(new Person { Name = "Second", LastContact = new DateTime(2005, 1, 1) });

            var people = _collection.Find(new { }, new { LastContact = -1, Name = 1 }).ToArray();
            Assert.Equal(3, people.Length);
            Assert.Equal("Third", people[0].Name);
            Assert.Equal("First", people[1].Name);
            Assert.Equal("Second", people[2].Name);
        }

        [Fact]
        public void BasicQueryUsingChildProperty()
        {
            _collection.Insert(new Person { Name = "Joe Cool", Address = { Street = "123 Main St", City = "Anytown", State = "CO", Zip = "45123" } });
            _collection.Insert(new Person { Name = "Sam Cool", Address = { Street = "300 Main St", City = "Anytown", State = "CO", Zip = "45123" } });

            var query = new Expando();
            query["Address.City"] = Q.Equals<string>("Anytown");

            var results = _collection.Find(query);
            Assert.Equal(2, results.Count());
        }
        [Fact]
        public void QueryWithinEmbeddedArray()
        {

            var post1 = new Person { Name = "First", Relatives = new List<String> { "comment1", "comment2" } };
            var post2 = new Person { Name = "Second", Relatives = new List<String> { "commentA", "commentB" } };
            _collection.Insert(post1);
            _collection.Insert(post2);

            var results = _collection.Find(new { Relatives = "commentA" });
            Assert.Equal("Second", results.First().Name);
        }


        [Fact]
        public void Distinct_On_Collection_Should_Return_Arrays_As_Value_If_Earlier_Than_1_5_0()
        {
            var isLessThan150 = Regex.IsMatch(_buildInfo.Version, "^([01][.][01234])");

            // Any version earlier than MongoDB 1.5.0
            if (isLessThan150)
            {
                _collection.Insert(new Person { Name = "Joe Cool", Relatives = new List<string>(new[] { "Tom Cool", "Sam Cool" }) });
                _collection.Insert(new Person { Name = "Sam Cool", Relatives = new List<string>(new[] { "Joe Cool", "Jay Cool" }) });
                _collection.Insert(new Person { Name = "Ted Cool", Relatives = new List<string>(new[] { "Tom Cool", "Sam Cool" }) });
                _collection.Insert(new Person { Name = "Jay Cool", Relatives = new List<string>(new[] { "Sam Cool" }) });

                var results = _collection.Distinct<string[]>("Relatives");
                Assert.Equal(3, results.Count());
            }
        }

        [Fact]
        public void Distinct_On_Collection_Should_Return_Array_Values_In_1_5_0_Or_Later()
        {
            var isLessThan150 = Regex.IsMatch(_buildInfo.Version, "^([01][.][01234])");

            // Any version MongoDB 1.5.0 +
            if (!isLessThan150)
            {
                _collection.Insert(new Person { Name = "Joe Cool", Relatives = new List<string>(new[] { "Tom Cool", "Sam Cool" }) });
                _collection.Insert(new Person { Name = "Sam Cool", Relatives = new List<string>(new[] { "Joe Cool", "Jay Cool" }) });
                _collection.Insert(new Person { Name = "Ted Cool", Relatives = new List<string>(new[] { "Tom Cool", "Sam Cool" }) });
                _collection.Insert(new Person { Name = "Jay Cool", Relatives = new List<string>(new[] { "Sam Cool" }) });

                var results = _collection.Distinct<string>("Relatives");
                Assert.Equal(4, results.Count());
            }
        }

        [Fact]
        public void DistinctOnSimpleProperty()
        {
            _collection.Insert(new Person { Name = "Joe Cool", Relatives = new List<string>(new[] { "Tom Cool", "Sam Cool" }) });
            _collection.Insert(new Person { Name = "Sam Cool", Relatives = new List<string>(new[] { "Joe Cool", "Jay Cool" }) });
            _collection.Insert(new Person { Name = "Ted Cool", Relatives = new List<string>(new[] { "Tom Cool", "Sam Cool" }) });
            _collection.Insert(new Person { Name = "Jay Cool", Relatives = new List<string>(new[] { "Sam Cool" }) });

            var results = _collection.Distinct<string>("Name");
            Assert.Equal(4, results.Count());
        }

        [Fact]
        public void DistinctOnComplexProperty()
        {
            _collection.Insert(new Person { Name = "Joe Cool", Address = new Address { State = "CA" } });
            _collection.Insert(new Person { Name = "Sam Cool", Address = new Address { State = "CA" } });
            _collection.Insert(new Person { Name = "Ted Cool", Address = new Address { State = "CA", Zip = "90010" } });
            _collection.Insert(new Person { Name = "Jay Cool", Address = new Address { State = "NY" } });

            var results = _collection.Distinct<Address>("Address");
            Assert.Equal(3, results.Count());
        }
    }
}