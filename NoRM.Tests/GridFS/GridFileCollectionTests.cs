using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Norm.Tests;
using Norm;
using Norm.GridFS;

namespace NoRM.Tests.GridFS
{
    public class GridFileCollectionTests
    {
        [Fact]
        public void Extension_Methods_Provide_Access_To_Collections()
        {
            using(var conn = Mongo.Create(TestHelper.ConnectionString("strict=false")))
            {
                var fileColl = conn.Database.Files();
                Assert.NotNull(fileColl);

                var fileColl2 = conn.GetCollection<TestClass>().Files();
            }
        }

        [Fact]
        public void File_Delete_Works()
        {
            using(var conn = Mongo.Create(TestHelper.ConnectionString("strict=false")))
            {

            }
        }
    }
}
