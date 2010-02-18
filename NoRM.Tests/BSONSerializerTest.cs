using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Text.RegularExpressions;
using NoRM.BSON;

namespace NoRM.BSON.Tests
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
            public byte[] Bytes { get; set; }
            public Guid? AGuid { get; set; }
            public Regex ARex { get; set; }
            public GeneralDTO Nester { get; set; }
        }

        protected class NestedGeneralDTO 
        {
            public int? AnInt { get; set; }
            public String Title { get; set; }
        }

        protected class EmptyDTO
        {

        }

        [Test]
        public void Serializing_POCO_Generates_Bytes()
        {
            var dummy = new GeneralDTO() { Title = "Testing" };
            Assert.IsNotEmpty(BSONSerializer.Serialize(dummy));
        }

        [Test]
        public void Serialization_Of_Strings_Are_Not_Lossy()
        {
            var obj1 = new GeneralDTO() { Title = null };
            var obj2 = new GeneralDTO() { Title = "Hello World" };

            var obj1Bytes = BSONSerializer.Serialize(obj1);
            var obj2Bytes = BSONSerializer.Serialize(obj2);

            var hydratedObj1 = BSONSerializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BSONSerializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(null, hydratedObj1.Title);
            Assert.AreEqual(obj2.Title, hydratedObj2.Title);
        }

        [Test]
        public void Serialization_Of_NestedObjects_Is_Not_Lossy()
        {
            var obj1 = new GeneralDTO() { Title = "Hello World", Nester = new GeneralDTO() { Title = "Bob", AnInt = 42 } };

            var obj1Bytes = BSONSerializer.Serialize(obj1);

            var hydratedObj1 = BSONSerializer.Deserialize<GeneralDTO>(obj1Bytes);

            Assert.AreEqual(obj1.Title, hydratedObj1.Title);
            Assert.AreEqual(obj1.Nester.Title, hydratedObj1.Nester.Title);
            Assert.AreEqual(obj1.Nester.AnInt, hydratedObj1.Nester.AnInt);

        }

        [Test]
        public void Recursive_NestedTypes_Dont_Cause_Infinite_Loop()
        {
            var obj1 = new GeneralDTO() { Title = "Hello World", Nester = new GeneralDTO() { Title = "Bob", AnInt = 42 } };
            var obj1Bytes = BSONSerializer.Serialize(obj1);
            var obj2 = BSONSerializer.Deserialize<GeneralDTO>(obj1Bytes);
            
        }

        [Test]
        public void Serialization_Of_Doubles_Are_Not_Lossy()
        {
            var obj1 = new GeneralDTO() { Pi = 3.1415927d };
            var obj2 = new GeneralDTO() { Pi = null };

            var obj1Bytes = BSONSerializer.Serialize(obj1);
            var obj2Bytes = BSONSerializer.Serialize(obj2);

            var hydratedObj1 = BSONSerializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BSONSerializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.Pi, hydratedObj1.Pi);
            Assert.AreEqual(null, hydratedObj2.Pi);
        }

        [Test]
        public void Serialization_Of_Ints_Are_Not_Lossy()
        {
            var obj1 = new GeneralDTO() { AnInt = 100 };
            var obj2 = new GeneralDTO() { AnInt = null };


            var obj1Bytes = BSONSerializer.Serialize(obj1);
            var obj2Bytes = BSONSerializer.Serialize(obj2);

            var hydratedObj1 = BSONSerializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BSONSerializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.AnInt, hydratedObj1.AnInt);
            Assert.AreEqual(null, hydratedObj2.AnInt);
        }

        [Test]
        public void Serialization_Of_Booleans_Are_Not_Lossy()
        {
            var obj1 = new GeneralDTO() { ABoolean = true };
            var obj2 = new GeneralDTO() { ABoolean = null };

            var obj1Bytes = BSONSerializer.Serialize(obj1);
            var obj2Bytes = BSONSerializer.Serialize(obj2);

            var hydratedObj1 = BSONSerializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BSONSerializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.ABoolean, hydratedObj1.ABoolean);
            Assert.AreEqual(null, hydratedObj2.ABoolean);
        }

        [Test]
        public void Serialization_Of_Bytes_Is_Not_Lossy()
        {
            var obj1 = new GeneralDTO() { Bytes = BitConverter.GetBytes(Int32.MaxValue) };
            var obj2 = new GeneralDTO() { Bytes = null };

            var obj1Bytes = BSONSerializer.Serialize(obj1);
            var obj2Bytes = BSONSerializer.Serialize(obj2);

            var hydratedObj1 = BSONSerializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BSONSerializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.Bytes, hydratedObj1.Bytes);
            Assert.AreEqual(null, hydratedObj2.Bytes);
        }

        [Test]
        public void Serialization_Of_Guid_Is_Not_Lossy()
        {
            var obj1 = new GeneralDTO() { AGuid = Guid.NewGuid() };
            var obj2 = new GeneralDTO() { AGuid = null };

            var obj1Bytes = BSONSerializer.Serialize(obj1);
            var obj2Bytes = BSONSerializer.Serialize(obj2);

            var hydratedObj1 = BSONSerializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BSONSerializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.AGuid, hydratedObj1.AGuid);
            Assert.AreEqual(null, hydratedObj2.AGuid);
        }

        [Test]
        public void Serialization_Of_Regex_Is_Not_Lossy()
        {
            var obj1 = new GeneralDTO() { ARex = new Regex("[0-9]{5}", RegexOptions.Multiline) };
            var obj2 = new GeneralDTO() { ARex = null };

            var obj1Bytes = BSONSerializer.Serialize(obj1);
            var obj2Bytes = BSONSerializer.Serialize(obj2);

            var hydratedObj1 = BSONSerializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BSONSerializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.ARex.ToString(), hydratedObj1.ARex.ToString());
            Assert.AreEqual(obj1.ARex.Options, hydratedObj1.ARex.Options);
            Assert.AreEqual(null, hydratedObj2.ARex);
            //more tests would be useful for all the options.
        }
    }
}
