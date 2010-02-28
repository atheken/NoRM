namespace NoRM.Tests
{
    using System.Text.RegularExpressions;
    using BSON;
    using Attributes;
    using Xunit;
    using System;
    using System.Diagnostics;
    
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
            public ObjectId Id { get; set; }
            public double? Pi { get; set; }
            public int? AnInt { get; set; }
            public String Title { get; set; }
            public bool? ABoolean { get; set; }
            public byte[] Bytes { get; set; }
            public Guid? AGuid { get; set; }
            public DateTime? ADateTime { get; set; }
            public string[] Strings { get; set; }
            public Flags32? Flags32 { get; set; }
            public Flags64? Flags64 { get; set; }
            public GeneralDTO Nester { get; set; }
            public Regex ARex { get; set; }
            

            [MongoIgnore]
            public int IgnoredProperty { get; set; }
        }

        [Fact]
        public void SerializationOfEnumIsNotLossy()
        {                        
            var obj1 = new GeneralDTO { Id = ObjectId.NewObjectId(), Pi = 3.4d, ABoolean = true, AnInt = 55, Title = "little", Bytes = new byte[] { 1, 2, 3 }, AGuid = Guid.NewGuid(), ADateTime = new DateTime(2004, 10, 21), Strings = new[] { "a", "bb", "abc" }, Flags32 = Flags32.FlagOff, Flags64 = Flags64.FlagOn, ARex = new Regex("its over (9000)", RegexOptions.Multiline | RegexOptions.IgnoreCase)};
            var nested = new GeneralDTO {Pi = 43d, Title = "little"};
            obj1.Nester = nested;
            var obj2 = new GeneralDTO();

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(obj1));
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(obj2));

            Assert.Equal(obj1.Id.ToString(), hydratedObj1.Id.ToString());
            Assert.Equal(obj1.Pi, hydratedObj1.Pi);
            Assert.Equal(obj1.AnInt, hydratedObj1.AnInt);
            Assert.Equal(obj1.Title, hydratedObj1.Title);
            Assert.Equal(obj1.ABoolean, hydratedObj1.ABoolean);
            Assert.Equal(obj1.Bytes, hydratedObj1.Bytes);
            Assert.Equal(obj1.AGuid, hydratedObj1.AGuid);
            Assert.Equal(obj1.ADateTime.Value.Ticks, hydratedObj1.ADateTime.Value.Ticks);
            Assert.Equal(obj1.Strings, hydratedObj1.Strings);
            Assert.Equal(obj1.Flags32, hydratedObj1.Flags32);
            Assert.Equal(obj1.Flags64, hydratedObj1.Flags64);
            Assert.Equal(obj1.Nester.Title, hydratedObj1.Nester.Title);
            Assert.Equal(obj1.Nester.Pi, hydratedObj1.Nester.Pi);
            Assert.Equal(obj1.ARex.ToString(), hydratedObj1.ARex.ToString());
            Assert.Equal(obj1.ARex.Options, hydratedObj1.ARex.Options);

            Assert.Equal(obj2.Id, hydratedObj2.Id);
            Assert.Equal(obj2.Pi, hydratedObj2.Pi);
            Assert.Equal(obj2.AnInt, hydratedObj2.AnInt);
            Assert.Equal(obj2.Title, hydratedObj2.Title);
            Assert.Equal(obj2.ABoolean, hydratedObj2.ABoolean);
            Assert.Equal(obj2.Bytes, hydratedObj2.Bytes);
            Assert.Equal(obj2.AGuid, hydratedObj2.AGuid);
            Assert.Equal(obj2.ADateTime, hydratedObj2.ADateTime);
            Assert.Equal(obj2.Strings, hydratedObj2.Strings);
            Assert.Equal(obj2.Flags32, hydratedObj2.Flags32);
            Assert.Equal(obj2.Flags64, hydratedObj2.Flags64);
            Assert.Equal(obj2.Nester, hydratedObj2.Nester);
            Assert.Equal(obj2.ARex, hydratedObj2.ARex);
        }
        /*
        [Fact]
        public void SerializationOfEnumIsNotLossy()
        {
            var obj1 = new GeneralDTO{ Flags32 = Flags32.FlagOn, Flags64 = Flags64.FlagOff };
            var obj2 = new GeneralDTO();

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(obj1));
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(obj2));

            Assert.Equal(obj1.Flags32, hydratedObj1.Flags32);
            Assert.Equal(null, hydratedObj2.Flags32);

            Assert.Equal(obj1.Flags64, hydratedObj1.Flags64);
            Assert.Equal(null, hydratedObj2.Flags64);
        }

        [Fact]
        public void SerializationOfFlyweightIsNotLossy()
        {
            var testObj = new Flyweight();
            testObj["astring"] = "stringval";
            var testBytes = BsonSerializer.Serialize(testObj);
            var hydrated = BsonDeserializer.Deserialize<Flyweight>(testBytes);

            Assert.Equal(testObj["astring"], hydrated["astring"]);
        }

        [Fact]
        public void MongoIgnoredPropertiesAreIgnored()
        {
            var test = new GeneralDTO {IgnoredProperty = 42};
            var hydrated = BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(test));

            Assert.Equal(0, hydrated.IgnoredProperty);
        }

        [Fact]
        public void SerializingPocoGeneratesBytes()
        {
            var dummy = new GeneralDTO { Title = "Testing" };
            Assert.NotEmpty(BsonSerializer.Serialize(dummy));
        }


        [Fact]
        public void SerializationOfDatesHasMillisecondPrecision()
        {
            var obj1 = new GeneralDTO { ADateTime = null };
            var obj2 = new GeneralDTO { ADateTime = DateTime.Now };

            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.Equal(null, hydratedObj1.ADateTime);

            //Mongo stores dates as long, therefore, we have to use double->long rounding.
            Assert.Equal((long)(obj2.ADateTime.Value - DateTime.MinValue).TotalMilliseconds,
                (long)(hydratedObj2.ADateTime.Value - DateTime.MinValue).TotalMilliseconds);

        }

        [Fact]
        public void SerializationOfStringsAreNotLossy()
        {
            var obj1 = new GeneralDTO { Title = null };
            var obj2 = new GeneralDTO { Title = "Hello World" };

            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.Equal(null, hydratedObj1.Title);
            Assert.Equal(obj2.Title, hydratedObj2.Title);
        }

        [Fact]
        public void SerializationOfNestedObjectsIsNotLossy()
        {
            var obj1 = new GeneralDTO { Title = "Hello World", Nester = new GeneralDTO { Title = "Bob", AnInt = 42 } };

            var obj1Bytes = BsonSerializer.Serialize(obj1);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);

            Assert.Equal(obj1.Title, hydratedObj1.Title);
            Assert.Equal(obj1.Nester.Title, hydratedObj1.Nester.Title);
            Assert.Equal(obj1.Nester.AnInt, hydratedObj1.Nester.AnInt);

        }

        [Fact]
        public void RecursiveNestedTypesDontCauseInfiniteLoop()
        {
            var obj1 = new GeneralDTO { Title = "Hello World", Nester = new GeneralDTO { Title = "Bob", AnInt = 42 } };
            var obj1Bytes = BsonSerializer.Serialize(obj1);
            BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);

        }

        [Fact]
        public void SerializationOfDoublesAreNotLossy()
        {
            var obj1 = new GeneralDTO { Pi = 3.1415927d };
            var obj2 = new GeneralDTO { Pi = null };

            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.Equal(obj1.Pi, hydratedObj1.Pi);
            Assert.Equal(null, hydratedObj2.Pi);
        }

        [Fact]
        public void SerializationOfIntsAreNotLossy()
        {
            var obj1 = new GeneralDTO { AnInt = 100 };
            var obj2 = new GeneralDTO { AnInt = null };


            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.Equal(obj1.AnInt, hydratedObj1.AnInt);
            Assert.Equal(null, hydratedObj2.AnInt);
        }

        [Fact]
        public void SerializationOfBooleansAreNotLossy()
        {
            var obj1 = new GeneralDTO { ABoolean = true };
            var obj2 = new GeneralDTO { ABoolean = null };

            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.Equal(obj1.ABoolean, hydratedObj1.ABoolean);
            Assert.Equal(null, hydratedObj2.ABoolean);
        }

        [Fact]
        public void SerializationOfBytesIsNotLossy()
        {
            var obj1 = new GeneralDTO { Bytes = BitConverter.GetBytes(Int32.MaxValue) };
            var obj2 = new GeneralDTO { Bytes = null };

            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.Equal(obj1.Bytes, hydratedObj1.Bytes);
            Assert.Equal(null, hydratedObj2.Bytes);
        }

        [Fact]
        public void SerializationOfGuidIsNotLossy()
        {
            var obj1 = new GeneralDTO { AGuid = Guid.NewGuid() };
            var obj2 = new GeneralDTO { AGuid = null };

            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.Equal(obj1.AGuid, hydratedObj1.AGuid);
            Assert.Equal(null, hydratedObj2.AGuid);
        }

        [Fact]
        public void SerializationOfRegexIsNotLossy()
        {
            var obj1 = new GeneralDTO { ARex = new Regex("[0-9]{5}", RegexOptions.Multiline) };
            var obj2 = new GeneralDTO { ARex = null };

            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.Equal(obj1.ARex.ToString(), hydratedObj1.ARex.ToString());
            Assert.Equal(obj1.ARex.Options, hydratedObj1.ARex.Options);
            Assert.Equal(null, hydratedObj2.ARex);
            //more tests would be useful for all the options.
        }

/*        [Fact]
        public void SerializationOfScopedCodeIsNotLossy()
        {
            var obj1 = new GeneralDTO {Code = new ScopedCode {CodeString = "function(){return 'hello world!'}"}};
            var scope = new Flyweight();
            scope["$ns"] = "root";
            obj1.Code.Scope = scope;

            var obj2 = BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(obj1));

            Assert.Equal(obj1.Code.CodeString, obj2.Code.CodeString);
            Assert.Equal(((Flyweight)obj1.Code.Scope)["$ns"],((Flyweight)obj2.Code.Scope)["$ns"]);
        }

        */
        [Fact]
        public void SerializationSpeedTest()
        {
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
                    var obj1Bytes = BsonSerializer.Serialize(obj1);
                    BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
                }
                stopWatch.Stop();

                Console.WriteLine(stopWatch.ElapsedMilliseconds);
            }
        }

        
    }
}
