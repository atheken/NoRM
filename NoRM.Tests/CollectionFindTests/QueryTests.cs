using System;
using System.Linq;
using Norm.BSON;
using Xunit;
using Norm.Collections;
using System.Collections.Generic;

namespace Norm.Tests
{
    public class QueryTests : IDisposable
    {
        private readonly Mongo _server;
        private readonly MongoCollection<Person> _collection;
        public QueryTests()
        {
            _server = Mongo.ParseConnection("mongodb://localhost/NormTests?pooling=false");
            _collection = _server.GetCollection<Person>("People");
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
        public void BasicQueryUsingProperty()
        {
            _collection.Insert(new Person { Name = "Joe Cool", Address = { Street = "123 Main St", City = "Anytown", State = "CO", Zip = "45123" } });
            _collection.Insert(new Person { Name = "Sam Cool", Address = { Street = "300 Main St", City = "Anytown", State = "CO", Zip = "45123" } });

            var results = _collection.Find(new { Name = "Joe Cool" });
            Assert.Equal(1, results.Count());

            Assert.Equal(1, _collection.Find(new { Name = Q.Equals("Sam Cool") }).Count());
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
            Assert.Equal("First",people[0].Name);
            Assert.Equal("Second",people[1].Name);
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

            var query = new Flyweight();
            query["Address.City"] = Q.Equals<string>("Anytown");

            var results = _collection.Find(query);
            Assert.Equal(2, results.Count());
        }
        [Fact]
        public void QueryWithinEmbeddedArray() {
            using (var session = new Session()) {
                var post1 = new Post { Title = "First", Comments = new List<Comment> { new Comment { Text = "comment1" }, new Comment { Text = "comment2" } } };
                var post2 = new Post { Title = "Second", Comments = new List<Comment> { new Comment { Text = "commentA" }, new Comment { Text = "commentB" } } };
                var posts = _server.Database.GetCollection<Post>();
                posts.Delete(new Object());
                posts.Insert(post1);
                posts.Insert(post2);

                var fun = @"function(){for(var i in this.Comments){if(i.Text === 'commentA') return true;}}";

                var query = new Flyweight();
                query["$where"] = fun;
                var results = posts.Find(query);
                Assert.Equal("Second",results.First().Title);

            }
        }

        [Fact]
        public void FindCanQueryEmbeddedArray() {
            _collection.Delete(new { });
            var person1 = new Person {
                Name = "Joe Cool",
                Address = {
                    Street = "123 Main St",
                    City = "Anytown",
                    State = "CO",
                    Zip = "45123"
                }

            };
            var person2 = new Person {
                Name = "Sam Cool",
                Address = {
                    Street = "300 Main St",
                    City = "Anytown",
                    State = "CO",
                    Zip = "45123"
                },
                Relatives = new List<string>(){ "Emma","Bruce","Charlie"
                }
            };
            _collection.Insert(person1);
            _collection.Insert(person2);

            var elem = new Flyweight();
            elem["this.Relatives"] = "Charlie";
            var query = new Flyweight();
            query["$elemMatch"] = elem;

            var a = _collection.Find(query).ToArray();
            Assert.Equal(1, a.Length);
        }
    }
}