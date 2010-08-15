using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Norm.BSON;

namespace Norm.Tests
{
    [TestFixture]
    public class ExpandoTests
    {
        [Test]
        public void Expando_Get_Should_Throw_Exception_When_Property_Doesnt_Exist()
        {
            Assert.Throws<InvalidOperationException>(() => { var a = new Expando(); a.Get<bool>("doesn't exist"); });
        }

        [Test]
        public void Expando_TryGet_Should_Not_Throw_Exception_When_Property_Doesnt_Exist()
        {
            var e = new Expando();
            e["hello"] = "world";
            var outval = "";
            Assert.True(e.TryGet<String>("hello",out outval));
            Assert.False(e.TryGet<String>("hello2", out outval));
            
            bool out2;
            Assert.False(e.TryGet<bool>("hello", out out2));

        }

        [Test]
        public void SerializationOfFlyweightListsInsideFlyweightWorks()
        {
            var testObj = new Expando();
            testObj["astring"] = "stringval";
            var innerObj = new List<Expando>();
            var innerInnerObject = new Expando();
            innerInnerObject["innerInnerValue"] = "aStringOfDoom";
            innerObj.Add(innerInnerObject);
            testObj["InnerObject"] = innerObj;
            var testBytes = BsonSerializer.Serialize(testObj);
            var hydrated = BsonDeserializer.Deserialize<Expando>(testBytes);
            Assert.AreEqual(testObj["InnerObject"].GetType(),hydrated["InnerObject"].GetType());
        }
    }
}
