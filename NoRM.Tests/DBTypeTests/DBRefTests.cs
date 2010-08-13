using System.Collections.Generic;
using System.Linq;
using Norm.BSON.DbTypes;
using Xunit;
using Norm.Collections;

namespace Norm.Tests
{
    public class DbRefTests
    {
        public DbRefTests()
        {
            using (var db = Mongo.Create(TestHelper.ConnectionString()))
            {
                db.GetCollection<TestProduct>().Delete(new { });
                db.GetCollection<ProductReference>().Delete(new { });
                db.GetCollection<User3>().Delete(new { });
                db.GetCollection<Role>().Delete(new { });
            }
        }

        [Fact]
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
                Assert.Equal(id.Value, product._id.Value);
            }

        }

        [Fact]
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

                Assert.Equal(userId, user.Id);
            }
        }
    }
}