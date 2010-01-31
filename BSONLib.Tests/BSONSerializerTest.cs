using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace BSONLib.Tests
{
    [TestFixture]
    public class BSONSerializerTest
    {
        protected class GeneralDTO
        {
            public double? Pi { get; set; }
            public int? AnInt { get; set; }
            public String Title { get; set; }
            public bool? ABoolean { get; set; }
        }

        protected class EmptyDTO
        {

        }

        [Test]
        public void Serializing_POCO_Generates_Bytes()
        {
            BSONSerializer serializer = new BSONSerializer();
            GeneralDTO dummy = new GeneralDTO(){Title ="Testing"};
            Assert.IsNotEmpty(serializer.Serialize(dummy));
        }

        [Test]
        public void Serialization_Of_Strings_Are_Not_Lossy()
        {
            var obj1 = new GeneralDTO() { Title = null };
            var obj2 = new GeneralDTO() { Title = "Hello World" };

            BSONSerializer serializer = new BSONSerializer();
            var obj1Bytes = serializer.Serialize(obj1);
            var obj2Bytes = serializer.Serialize(obj2);

            var hydratedObj1 = serializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = serializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(null, hydratedObj1.Title);
            Assert.AreEqual(obj2.Title, hydratedObj2.Title);
        }

        [Test]
        public void Serialization_Of_Doubles_Are_Not_Lossy()
        {
            var obj1 = new GeneralDTO() { Pi = 3.1415927d };
            var obj2 = new GeneralDTO() { Pi = null };

            BSONSerializer serializer = new BSONSerializer();
            var obj1Bytes = serializer.Serialize(obj1);
            var obj2Bytes = serializer.Serialize(obj2);

            var hydratedObj1 = serializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = serializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.Pi, hydratedObj1.Pi);
            Assert.AreEqual(null, hydratedObj2.Pi);
        }

        [Test]
        public void Serialization_Of_Ints_Are_Not_Lossy()
        {
            var obj1 = new GeneralDTO() { AnInt = 100 };
            var obj2 = new GeneralDTO() { AnInt = null };
            

            BSONSerializer serializer = new BSONSerializer();
            var obj1Bytes = serializer.Serialize(obj1);
            var obj2Bytes = serializer.Serialize(obj2);

            var hydratedObj1 = serializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = serializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.AnInt, hydratedObj1.AnInt);
            Assert.AreEqual(null, hydratedObj2.AnInt);
        }

        [Test]
        public void Serialization_Of_Booleans_Are_Not_Lossy()
        {
            var obj1 = new GeneralDTO() { ABoolean = true };
            var obj2 = new GeneralDTO() { ABoolean = null };


            BSONSerializer serializer = new BSONSerializer();
            var obj1Bytes = serializer.Serialize(obj1);
            var obj2Bytes = serializer.Serialize(obj2);

            var hydratedObj1 = serializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = serializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.ABoolean, hydratedObj1.ABoolean);
            Assert.AreEqual(null, hydratedObj2.ABoolean);
        }
    }
}
