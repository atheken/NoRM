using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Norm.Responses;


namespace NoRM.Tests
{
    public class GeneralTests
    {
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
