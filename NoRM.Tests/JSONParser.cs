using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Norm.BSON;

namespace NoRM.Tests
{
    [TestFixture]
    public class JSONParserTests
    {
        private ObjectParser _parser = new ObjectParser();

        [Test]
        public void ParserCanParseEmptyObject()
        {
            var result = _parser.ParseJSON("{}");
            Assert.NotNull(result);

        }

        [Test]
        public void ParserCanParseMemberAndNull()
        {
            var result = _parser.ParseJSON("{\"Hello\":null}");
            Assert.Null(result["Hello"]);
        }

        [Test]
        public void ParserCanParseMemberAndBool()
        {
            var result = _parser.ParseJSON("{\"Hello\": true }");
            Assert.AreEqual(true, result["Hello"]);
        }

        [Test]
        public void ParserCanParseMemberAndNumber()
        {
            var result = _parser.ParseJSON("{\"Pi\": -3.1415  , \"Pie\" : 314e-2 }");
            Assert.AreEqual(-3.1415d, result["Pi"]);
            Assert.AreEqual(3.14d, result["Pie"]);
        }

        [Test]
        public void ParserCanParseMemberAndString()
        {
            var result = _parser.ParseJSON("{\"Hello\": \"World\" }");
            Assert.AreEqual("World", result["Hello"]);
        }

        [Test]
        public void ParserCanParseArray()
        {
            var result = _parser.ParseJSON(@"{""Hello"": [1,2,3]}");
            var values = (Object[])result["Hello"];
            Assert.True(values.SequenceEqual(new object[] { 1d, 2d, 3d }));
        }

        [Test]
        public void ParserCanParseNestedObjects()
        {
            var result = _parser.ParseJSON(@"{""Hello"": {""a"": 1}, ""World"" : {""b"": { ""52"":""bomber"" } } }");
            var nestedObject1 = (Expando)result["Hello"];
            Assert.AreEqual(1d, nestedObject1["a"]);
            var nestedObject2 = (Expando)result["World"];
            Assert.AreEqual("bomber", ((Expando)nestedObject2["b"])["52"]);
        }

        [Test]
        public void ParserCanParseMultipleMembers()
        {
            var results = _parser.ParseJSON("{\"Hello\":1,\"World\":false}");
            Assert.AreEqual(1d, results["Hello"]);
            Assert.AreEqual(false, results["World"]);
        }

    }
}
