using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Norm.Responses;
using Norm;
using Norm.Tests;


namespace NoRM.Tests
{
    public class GeneralTests
    {
        [Fact]
        public void Get_Last_Error_Returns()
        {
            using (var mongo = Mongo.Create(TestHelper.ConnectionString()))
            {
                var le = mongo.LastError();
                Assert.Equal(true, le.WasSuccessful);
            }
        }

        [Fact]
        public void Base_Status_Message_Supports_Expando()
        {
            BaseStatusMessage bsm = new BaseStatusMessage();
            bsm["hello"] = "world";
            Assert.Equal("world",bsm["hello"]);
            Assert.Equal(bsm.AllProperties().First().PropertyName, "hello");

            Assert.Equal(bsm.AllProperties().First().Value, "world");
            bsm.Delete("hello");
            Assert.False(bsm.AllProperties().Any());

        }
    }
}
