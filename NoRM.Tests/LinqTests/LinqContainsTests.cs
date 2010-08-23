using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Norm.Tests;
using Norm;

namespace NoRM.Tests.LinqTests
{
    public class LinqContainsTests
    {
        [Fact]
        public void WhenAddItems_ThenCheckContains()
        {
            string connection = TestHelper.ConnectionString();
            using (var db = Mongo.Create(connection))
            {
                // Arrange
                var names = new List<string>();
                var provider = db.Database.GetCollection<TestContains>();                

                provider.Insert(new TestContains { Label = "test1", Owners = new List<int> { 1, 2, 3 } });
                provider.Insert(new TestContains { Label = "test2", Owners = new List<int> { 2 } });
                provider.Insert(new TestContains { Label = "test3", Owners = new List<int> { 3, 5 } });

                var repo = provider.AsQueryable();

                // Act                
                var result1 = repo.Where(i => i.Owners.Contains(1)).ToList();

                // Assert
                Assert.NotNull(result1);
                Assert.Equal(result1.Count, 1);
                Assert.Equal(result1.FirstOrDefault().Label, "test1");

                // Act
                var result2 = repo.Where(i => i.Owners.Contains(3)).ToList();

                // Assert
                Assert.NotNull(result2);
                Assert.Equal(result2.Count, 2);
                Assert.Equal(result2[0].Label, "test1");
                Assert.Equal(result2[1].Label, "test3");

                db.Database.DropCollection("TestContains");
            }
        }
    }
}
