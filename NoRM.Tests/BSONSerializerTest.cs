using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Text.RegularExpressions;
using NoRM.BSON;
using NoRM.Attributes;
using NoRM.BSON.DbTypes;

namespace NoRM.Tests
{
    [TestFixture]
    [Category("In Memory Only")]
    public class BSONSerializerTest
    {
        
        protected enum Flags32
        {
            FlagNone = 0,
            FlagOn = 1,
            FlagOff = 2
        }

        protected enum Flags64 : long
        {
            FlagNone = 0,
            FlagOn = 1,
            FlagOff = 2
        }

        protected class GeneralDTO
        {
            public double? Pi { get; set; }
            public int? AnInt { get; set; }
            public String Title { get; set; }
            public bool? ABoolean { get; set; }
            public byte[] Bytes { get; set; }
            public Guid? AGuid { get; set; }
            public Regex ARex { get; set; }
            public DateTime? ADateTime { get; set; }
            public GeneralDTO Nester { get; set; }
            public ScopedCode Code {get;set;}
            public Flags32? Flags32 { get; set; }
            public Flags64? Flags64 { get; set; }

            [MongoIgnore]
            public int IgnoredProperty { get; set; }
        }

        [Test]
        public void Serialization_Of_Enum_Is_Not_Lossy()
        {
            var obj1 = new GeneralDTO{ Flags32 = Flags32.FlagOn, Flags64 = Flags64.FlagOff };
            var obj2 = new GeneralDTO();

            var hydratedObj1 = BSONSerializer.Deserialize<GeneralDTO>(BSONSerializer.Serialize(obj1));
            var hydratedObj2 = BSONSerializer.Deserialize<GeneralDTO>(BSONSerializer.Serialize(obj2));

            Assert.AreEqual(obj1.Flags32, hydratedObj1.Flags32);
            Assert.AreEqual(null, hydratedObj2.Flags32);

            Assert.AreEqual(obj1.Flags64, hydratedObj1.Flags64);
            Assert.AreEqual(null, hydratedObj2.Flags64);
        }

        [Test]
        public void Serialization_Of_Flyweight_Is_Not_Lossy()
        {
            var testObj = new Flyweight();
            testObj["astring"] = "stringval";
            var testBytes = BSONSerializer.Serialize(testObj);
            var hydrated = BSONSerializer.Deserialize<Flyweight>(testBytes);

            Assert.AreEqual(testObj["astring"], hydrated["astring"]);
        }

        [Test]
        public void MongoIgnored_Properties_Are_Ignored()
        {
            var test = new GeneralDTO();
            test.IgnoredProperty = 42;
            var hydrated = BSONSerializer.Deserialize<GeneralDTO>(BSONSerializer.Serialize(test));

            Assert.AreEqual(0, hydrated.IgnoredProperty);
        }

        [Test]
        public void Serializing_POCO_Generates_Bytes()
        {
            var dummy = new GeneralDTO { Title = "Testing" };
            Assert.IsNotEmpty(BSONSerializer.Serialize(dummy));
        }


        [Test]
        public void Serialization_Of_Dates_Has_Millisecond_Precision()
        {
            var obj1 = new GeneralDTO() { ADateTime = null };
            var obj2 = new GeneralDTO() { ADateTime = DateTime.Now };

            var obj1Bytes = BSONSerializer.Serialize(obj1);
            var obj2Bytes = BSONSerializer.Serialize(obj2);

            var hydratedObj1 = BSONSerializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BSONSerializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(null, hydratedObj1.ADateTime);

            //Mongo stores dates as long, therefore, we have to use double->long rounding.
            Assert.AreEqual((long)(obj2.ADateTime.Value - DateTime.MinValue).TotalMilliseconds,
                (long)(hydratedObj2.ADateTime.Value - DateTime.MinValue).TotalMilliseconds);

        }

        [Test]
        public void Serialization_Of_Strings_Are_Not_Lossy()
        {
            var obj1 = new GeneralDTO { Title = null };
            var obj2 = new GeneralDTO { Title = "Hello World" };

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
            var obj1 = new GeneralDTO { Title = "Hello World", Nester = new GeneralDTO { Title = "Bob", AnInt = 42 } };

            var obj1Bytes = BSONSerializer.Serialize(obj1);

            var hydratedObj1 = BSONSerializer.Deserialize<GeneralDTO>(obj1Bytes);

            Assert.AreEqual(obj1.Title, hydratedObj1.Title);
            Assert.AreEqual(obj1.Nester.Title, hydratedObj1.Nester.Title);
            Assert.AreEqual(obj1.Nester.AnInt, hydratedObj1.Nester.AnInt);

        }

        [Test]
        public void Recursive_NestedTypes_Dont_Cause_Infinite_Loop()
        {
            var obj1 = new GeneralDTO { Title = "Hello World", Nester = new GeneralDTO { Title = "Bob", AnInt = 42 } };
            var obj1Bytes = BSONSerializer.Serialize(obj1);
            BSONSerializer.Deserialize<GeneralDTO>(obj1Bytes);

        }

        [Test]
        public void Serialization_Of_Doubles_Are_Not_Lossy()
        {
            var obj1 = new GeneralDTO { Pi = 3.1415927d };
            var obj2 = new GeneralDTO { Pi = null };

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
            var obj1 = new GeneralDTO { AnInt = 100 };
            var obj2 = new GeneralDTO { AnInt = null };


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
            var obj1 = new GeneralDTO { ABoolean = true };
            var obj2 = new GeneralDTO { ABoolean = null };

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
            var obj1 = new GeneralDTO { Bytes = BitConverter.GetBytes(Int32.MaxValue) };
            var obj2 = new GeneralDTO { Bytes = null };

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
            var obj1 = new GeneralDTO { AGuid = Guid.NewGuid() };
            var obj2 = new GeneralDTO { AGuid = null };

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
            var obj1 = new GeneralDTO { ARex = new Regex("[0-9]{5}", RegexOptions.Multiline) };
            var obj2 = new GeneralDTO { ARex = null };

            var obj1Bytes = BSONSerializer.Serialize(obj1);
            var obj2Bytes = BSONSerializer.Serialize(obj2);

            var hydratedObj1 = BSONSerializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BSONSerializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.ARex.ToString(), hydratedObj1.ARex.ToString());
            Assert.AreEqual(obj1.ARex.Options, hydratedObj1.ARex.Options);
            Assert.AreEqual(null, hydratedObj2.ARex);
            //more tests would be useful for all the options.
        }

        [Test]
        public void Serialization_Of_Scoped_Code_Is_Not_Lossy()
        {
            var obj1 = new GeneralDTO();
            obj1.Code = new ScopedCode();
            obj1.Code.CodeString = "function(){return 'hello world!'}";
            var scope = new Flyweight();
            scope["$ns"] = "root";
            obj1.Code.Scope = scope;

            var obj2 = BSONSerializer.Deserialize<GeneralDTO>(BSONSerializer.Serialize(obj1));

            Assert.AreEqual(obj1.Code.CodeString, obj2.Code.CodeString);
            Assert.AreEqual(((Flyweight)obj1.Code.Scope)["$ns"],((Flyweight)obj2.Code.Scope)["$ns"]);
        }

        [Test]
        [Category("Benchmark")]
        public void Serialization_Speed_Test()
        {
            /*
             
            5832 - 4598 - 4653 - 4879 - 4516 - 4657 - 4346 - 4601 - 4349 - 4498            
             
            */

            for (var i2 = 0; i2 < 10; i2++)
            {
                var stopWatch = new Stopwatch();

                stopWatch.Start();

                for (var i = 0; i < 10000; i++)
                {
                    var obj1 = new GeneralDTO
                                   {
                                       Title = null,
                                       ABoolean = true,
                                       AGuid = Guid.NewGuid(),
                                       AnInt = 1,
                                       Pi = 3.14,
                                       Nester = new GeneralDTO { Title = "Bob", AnInt = 42 }
                                   };
                    var obj1Bytes = BSONSerializer.SerializeFast(obj1, false);
                    var hydratedObj1 = BSONSerializer.Deserialize<GeneralDTO>(obj1Bytes);
                }
                stopWatch.Stop();

                Console.WriteLine(stopWatch.ElapsedMilliseconds);
            }
        }

        
    }
}
