using System;
using NUnit.Framework;
using Norm.Collections;
using System.Text.RegularExpressions;
using Norm.Responses;

namespace Norm.Tests.CollectionUpdateTests
{

    [TestFixture]
    public class UpdateModifiersTests : IDisposable
    {
	    private readonly IMongo _server;
        private BuildInfoResponse _buildInfo = null;
        private readonly IMongoCollection<Post> _collection;

		private Mongod _proc;

		[TestFixtureSetUp]
		public void SetupTestFixture ()
		{
			_proc = new Mongod ();
		}

		[TestFixtureTearDown]
		public void TearDownTestFixture ()
		{
			_proc.Dispose ();
		}




        public UpdateModifiersTests()
        {
            var admin = new MongoAdmin("mongodb://localhost/admin?pooling=false&strict=true");
            _server = Mongo.Create("mongodb://localhost/NormTests?pooling=false&strict=true");
            _collection = _server.GetCollection<Post>("Posts");
            _buildInfo = admin.BuildInfo();
        }
        public void Dispose()
        {
            _server.Database.DropCollection("Posts");
            using (var admin = new MongoAdmin("mongodb://localhost/NormTests?pooling=false"))
            {
                admin.DropDatabase();
            }
            _server.Dispose();
        }


        [Test]
        public void PostScoreShouldBeEqualThreeWhenApplyingIncrementBy2CommandToScoreEqOne()
        {
            var post = new Post { Title = "About the name", Score = 1 };
            _collection.Insert(post);

            _collection.UpdateOne(new { _id = post.Id }, new { Score = M.Increment(2), Title = M.Set("ss") });

            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(3, result.Score);
        }

        [Test]
        public void PostScoreShouldBeEqualOneWhenApplyingIncrementByMinus2CommandToScoreEqThree()
        {
            var post = new Post { Title = "About the name 2", Score = 3 };
            _collection.Insert(post);

            _collection.UpdateOne(new { _id = post.Id }, new { Score = M.Increment(-2) });
            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(1, result.Score);
        }
        [Test]
        public void PostTitleShouldBeEqual_NoRm_WhenApplyingSetModifierCommandToTitle()
        {
            var post = new Post { Title = "About the name 2", Score = 3 };
            _collection.Insert(post);

            _collection.UpdateOne(new { _id = post.Id }, new { Title = M.Set("NoRm") });
            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual("NoRm", result.Title);
            Assert.AreEqual(3, result.Score);
        }
        [Test]
        public void PostCommentsCountShouldBeEqualOneWhenApplyingPushModifierCommandToPostWithNoComments()
        {
            var post = new Post { Title = "About the name 2", Score = 3 };
            _collection.Insert(post);

            _collection.UpdateOne(new { _id = post.Id }, new { Comments = M.Push(new Comment { Text = "SomeText" }) });
            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(1, result.Comments.Count);
            Assert.AreEqual(3, result.Score);
            Assert.AreEqual("About the name 2", result.Title);
        }
        [Test]
        public void PostCommentsCountShouldBeEqualTwoWhenApplyingPushModifierCommandToPostWithOneComment()
        {
            var post = new Post { Title = "About the name 2", Score = 3 };
            post.Comments.Add(new Comment { Text = "some text" });
            _collection.Insert(post);

            _collection.UpdateOne(new { _id = post.Id }, new { Comments = M.Push(new Comment { Text = "SomeText" }) });
            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(2, result.Comments.Count);
            Assert.AreEqual(3, result.Score);
            Assert.AreEqual("About the name 2", result.Title);
        }

        [Test]
        public void PostCommentsCountShouldBeEqualTwoWhenApplyingPushAllModifieWith2CommentsToPostWithNoComments()
        {
            var post = new Post { Title = "About the name 2", Score = 3 };

            _collection.Insert(post);

            _collection.UpdateOne(new { _id = post.Id }, new
            {
                Comments = M.PushAll(
                    new Comment { Text = "SomeText" },
                    new Comment { Text = "SecondComment" })
            });
            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(2, result.Comments.Count);
            Assert.AreEqual(3, result.Score);
            Assert.AreEqual("About the name 2", result.Title);
        }

        [Test]
        public void PostCommentsCountShouldBeEqualThreeWhenApplyingPushAllModifieWith2CommentsToPostWithOneComment()
        {
            var post = new Post { Title = "About the name 2", Score = 3 };
            post.Comments.Add(new Comment { Text = "some text" });
            _collection.Insert(post);

            _collection.UpdateOne(new { _id = post.Id }, new
            {
                Comments = M.PushAll(
                    new Comment { Text = "SomeText" },
                    new Comment { Text = "SecondComment" })
            });
            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(3, result.Comments.Count);
            Assert.AreEqual(3, result.Score);
            Assert.AreEqual("About the name 2", result.Title);
        }


        [Test]
        public void AddToSet_Should_Add_When_Element_Doesnt_Exist_In_1_3_3_Or_Later()
        {
            //only works with versions 1.3.3 + 
            var incompatible = Regex.IsMatch(_buildInfo.Version, "^([01][.][012]|[01][.]3[.][012])");
            var post = new Post { Title = "About the name 2", Score = 3 };
            _collection.Insert(post);
            if (!incompatible)
            {
                _collection.UpdateOne(new { _id = post.Id }, new
                {
                    Tags = M.AddToSet("NoSql")
                });
                var result = _collection.FindOne(new { _id = post.Id });
                Assert.AreEqual(1, result.Tags.Count);
                Assert.AreEqual(3, result.Score);
                Assert.AreEqual("About the name 2", result.Title);
            }
        }
        [Test]
        public void AddToSet_Should_Not_Add_When_Element_Already_Exists_In_1_3_3_Or_Later()
        {
            //only works with versions 1.3.3 + 
            var incompatible = Regex.IsMatch(_buildInfo.Version, "^([01][.][012]|[01][.]3[.][012])");

            //we add these because the collection is going to get dropped on dispose.
            var post = new Post { Title = "About the name 2", Score = 3 };
            post.Tags.Add("NoSql");
            _collection.Insert(post);

            if (!incompatible)
            {
                _collection.UpdateOne(new { _id = post.Id }, new
                {
                    Tags = M.AddToSet("NoSql")
                });

                var result = _collection.FindOne(new { _id = post.Id });
                Assert.AreEqual(1, result.Tags.Count);
                Assert.AreEqual(3, result.Score);
                Assert.AreEqual("About the name 2", result.Title);
            }
        }



        [Test]
        public void PullingTag_NoSql_FromPostWith_NoSql_TagWithPullModifierShouldRemoveThatTag()
        {
            var post = new Post { Title = "About the name 2", Score = 3 };
            post.Tags.Add("NoSql");
            _collection.Insert(post);

            _collection.Update(post.Id, op => op.Pull(prop => prop.Tags, "NoSql"));

            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(0, result.Tags.Count);
            Assert.AreEqual(3, result.Score);
            Assert.AreEqual("About the name 2", result.Title);
        }

        [Test]
        public void Push_Modifier_Expression_Works()
        {
            var post = new Post { Tags = new String[] { "Yard", "Gnomes", "get" } };
            _collection.Insert(post);
            _collection.Update(post.Id, op => op.Push(prop => prop.Tags, "stolen."));
            var result = _collection.FindOne(new { _id = post.Id });
            Assert.True(result.Tags.Contains("stolen."));
        }

        [Test]
        public void PushAll_Modifier_Expression_Works()
        {
            var post = new Post { Tags = new String[] { } };
            _collection.Insert(post);
            _collection.Update(post.Id, op => op.PushAll(prop => prop.Tags, "Yard", "Gnomes", "get", "stolen."));
            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(4, result.Tags.Count);
            Assert.True(result.Tags.Contains("stolen."));
        }

        [Test]
        public void AddToSet_Modified_Expression_Works()
        {
            //only works with versions 1.3.3 + 
            var incompatible = Regex.IsMatch(_buildInfo.Version, "^([01][.][012]|[01][.]3[.][012])");
            var post = new Post { Tags = new String[] { "Gnome", "Yard" } };
            _collection.Insert(post);
            if (!incompatible)
            {
                _collection.Update(post.Id, op => op.AddToSet(prop => prop.Tags, "stolen"));
                var result = _collection.FindOne(new { _id = post.Id });
                Assert.True(result.Tags.Contains("stolen"));
            }
        }

        [Test]
        public void SetValue_Modifier_Expression_Works()
        {
            var post = new Post { Title = null };
            _collection.Insert(post);
            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(null, result.Title);
            _collection.Update(post.Id, op => op.SetValue(prop => prop.Title, "Gnome"));
            result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual("Gnome", result.Title);

        }


        [Test]
        public void Increment_Modifier_Expression_Works()
        {
            var post = new Post { Score = 3 };
            _collection.Insert(post);
            _collection.Update(post.Id, op => op.Increment(prop => prop.Score, 5));
            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(8, result.Score);
        }

        [Test]
        public void PullingTag_NoSql_FromPostWithout_NoSql_TagWithPullModifierShouldDoNothing()
        {
            var post = new Post { Title = "About the name 2", Score = 3 };
            post.Tags.Add("NoSql2");
            _collection.Insert(post);
            _collection.Update(post.Id, op => op.Pull(prop => prop.Tags, "NoSql"));

            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(1, result.Tags.Count);
            Assert.AreEqual(3, result.Score);
            Assert.AreEqual("About the name 2", result.Title);
        }

        [Test]
        public void PullingTag_NoSql_FromPostWith_NoSql_Tag_And_ABC_TagWithPullModifierShouldRemoveOnly_NoSql_Tag()
        {
            var post = new Post { Title = "About the name 2", Score = 3 };
            post.Tags.Add("NoSql");
            post.Tags.Add("ABC");
            _collection.Insert(post);
            _collection.Update(post.Id, op => op.Pull(prop => prop.Tags, "NoSql"));

            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(1, result.Tags.Count);
            Assert.AreEqual(3, result.Score);
            Assert.AreEqual("About the name 2", result.Title);
        }

        [Test]
        public void PopModifierLastItemUsage()
        {
            var post = new Post { Title = "About the name 2", Score = 3 };
            post.Tags.Add("NoSql");
            post.Tags.Add("ABC");
            post.Tags.Add("mongo");
            _collection.Insert(post);
            _collection.Update(post.Id, op => op.PopLast(prop => prop.Tags));

            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(2, result.Tags.Count);
            Assert.IsFalse(result.Tags.Contains("mongo"));
            Assert.AreEqual(3, result.Score);
            Assert.AreEqual("About the name 2", result.Title);

        }
        [Test]
        public void PopModifierFirstItemUsage()
        {
            var post = new Post { Title = "About the name 2", Score = 3 };
            post.Tags.Add("NoSql");
            post.Tags.Add("ABC");
            post.Tags.Add("mongo");
            _collection.Insert(post);
            _collection.Update(post.Id, op => op.PopFirst(prop => prop.Tags));

            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(2, result.Tags.Count);
            Assert.IsFalse(result.Tags.Contains("NoSql"));
            Assert.AreEqual(3, result.Score);
            Assert.AreEqual("About the name 2", result.Title);

        }

        [Test]
        public void PullAllModifierUsage()
        {
            var post = new Post { Title = "About the name 2", Score = 3 };
            post.Tags.Add("NoSql");
            post.Tags.Add("ABC");
            post.Tags.Add("mongo");
            _collection.Insert(post);
            _collection.Update(post.Id, op => op.PullAll(prop => prop.Tags, "NoSql", "ABC"));

            var result = _collection.FindOne(new { _id = post.Id });
            Assert.AreEqual(1, result.Tags.Count);
            Assert.IsFalse(result.Tags.Contains("NoSql"));
            Assert.IsFalse(result.Tags.Contains("ABC"));


        }



    }
}