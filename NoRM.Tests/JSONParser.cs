using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Norm.BSON;

namespace NoRM.Tests
{
    public class JSONParserTests
    {
        private ObjectParser _parser = new ObjectParser();

        [Fact]
        public void ParserCanParseEmptyObject()
        {
            var result = _parser.ParseJSON("{}");
            Assert.NotNull(result);

        }

        [Fact]
        public void ParserCanParseMemberAndNull()
        {
            var result = _parser.ParseJSON("{\"Hello\":null}");
            Assert.Null(result["Hello"]);
        }

        [Fact]
        public void ParserCanParseMemberAndBool()
        {
            var result = _parser.ParseJSON("{\"Hello\": true }");
            Assert.Equal(true, result["Hello"]);
        }

        [Fact]
        public void ParserCanParseMemberAndNumber()
        {
            var result = _parser.ParseJSON("{\"Pi\": -3.1415  , \"Pie\" : 314e-2 }");
            Assert.Equal(-3.1415d, result["Pi"]);
            Assert.Equal(3.14d, result["Pie"]);
        }

        [Fact]
        public void ParserCanParseMemberAndString()
        {
            var result = _parser.ParseJSON("{\"Hello\": \"World\" }");
            Assert.Equal("World", result["Hello"]);
        }

        [Fact]
        public void ParserCanParseArray()
        {
            var result = _parser.ParseJSON(@"{""Hello"": [1,2,3]}");
            var values = (Object[])result["Hello"];
            Assert.True(values.SequenceEqual(new object[] { 1d, 2d, 3d }));
        }

        [Fact]
        public void ParserCanParseNestedObjects()
        {
            var result = _parser.ParseJSON(@"{""Hello"": {""a"": 1}, ""World"" : {""b"": { ""52"":""bomber"" } } }");
            var nestedObject1 = (Expando)result["Hello"];
            Assert.Equal(1d, nestedObject1["a"]);
            var nestedObject2 = (Expando)result["World"];
            Assert.Equal("bomber", ((Expando)nestedObject2["b"])["52"]);
        }

        [Fact]
        public void ParserCanParseMultipleMembers()
        {
            var results = _parser.ParseJSON("{\"Hello\":1,\"World\":false}");
            Assert.Equal(1d, results["Hello"]);
            Assert.Equal(false, results["World"]);
        }

    }
}
