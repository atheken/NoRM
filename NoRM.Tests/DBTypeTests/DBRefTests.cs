using System.Collections.Generic;
using System.Linq;
using Norm.BSON.DbTypes;
using NUnit.Framework;
using Norm.Collections;

namespace Norm.Tests
{
    [TestFixture]
    public class DbRefTests
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

        [SetUp]
        public void Setup()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                db.Database.DropCollection<TestProduct>(false);
                db.Database.DropCollection<ProductReference>(false);
                db.Database.DropCollection<User3>(false);
                db.Database.DropCollection<Role>(false);
            }
        }

        [Test]
        public void DbRefMapsToOtherDocumentsByOid()
        {
            var id = ObjectId.NewObjectId();

            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {

                db.GetCollection<TestProduct>().Insert(new TestProduct { _id = id, Name = "RefProduct" });

                var productReference = new DbReference<TestProduct>(id);

                db.GetCollection<ProductReference>().Insert(new ProductReference
                    {
                        Id = ObjectId.NewObjectId(),
                        Name = "FullCart",
                        ProductsOrdered = new[] { productReference }
                    });

                var reference = db.GetCollection<ProductReference>().Find(new { }).First();
                var product = reference.ProductsOrdered[0].Fetch(() => db);
                Assert.AreEqual(id.Value, product._id.Value);
            }

        }

        [Test]
        public void DbRefMapsToOtherDocumentsByCustomId()
        {
            const string userId = "Tim Berners-Lee";
            const string roleName = "Administrator";

            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                db.GetCollection<User3>().Insert(new User3
                    {
                        Id = userId,
                        EmailAddress = "user@domain.com"
                    });
                db.GetCollection<Role>().Insert(new Role
                {
                    Id = roleName,
                    Users = new List<DbReference<User3, string>>
                        {
                            new DbReference<User3, string>(userId)
                        }
                });

                var role = db.GetCollection<Role>().Find().First();
                var user = role.Users[0].Fetch(() => db);

                Assert.AreEqual(userId, user.Id);
            }
        }
    }
}