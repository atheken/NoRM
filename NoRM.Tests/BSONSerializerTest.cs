using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xunit;
using Norm.BSON;
using System.Linq;

namespace Norm.Tests
{
    public class BSONSerializerTest
    {
        [Fact]
        public void DoesntSerializeIgnoredProperties()
        {
            var o = new GeneralDTO {IgnoredProperty = 4};
            Assert.Equal(0, BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(o)).IgnoredProperty);
        }

        [Fact]
        public void Should_Not_Serialize_When_Default_Values_Specified()
        {
            var o = new SerializerTest() { Id = 1, Message = "Test" };
            // when serialized it should produce a document {"Id": 1} 
            var o1 = BsonSerializer.Serialize(o);
            // create a object with value Id = 1
            var s1 = BsonSerializer.Serialize(new { Id = 1 });
            // both should be equal
            Assert.Equal(o1, s1);
        }

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
            var testObj = new Expando();
            testObj["astring"] = "stringval";
            var testBytes = BsonSerializer.Serialize(testObj);
            var hydrated = BsonDeserializer.Deserialize<Expando>(testBytes);
            Assert.Equal(testObj["astring"], hydrated["astring"]);
        }
        
        [Fact]
        public void SerializesAndDeserializesAFloat()
        {
            var o = new GeneralDTO {AFloat = 1.4f};
            Assert.Equal(1.4f, BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(o)).AFloat);
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
            Assert.Equal((long)((obj2.ADateTime.Value.ToUniversalTime() - DateTime.MinValue)).TotalMilliseconds,
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
        public void SerializationOfIEnumerableTIsNotLossy()
        {
            var gto = new GeneralDTO{ AnIEnumerable = new List<Person>(){ new Person(), new Person()}};
            var bytes = BsonSerializer.Serialize(gto);
        
            var gto2 = BsonDeserializer.Deserialize<GeneralDTO>(bytes);
            Assert.Equal(2, gto2.AnIEnumerable.Count());
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
        public void SerializationOfInheritenceIsNotLossy()
        {
            var obj1 = new SubClassedObject {Title = "Subclassed", ABool = true};
            var hydratedObj1 = BsonDeserializer.Deserialize<SubClassedObject>(BsonSerializer.Serialize(obj1));
            Assert.Equal(obj1.Title, hydratedObj1.Title);
            Assert.Equal(obj1.ABool, hydratedObj1.ABool);
        }

        [Fact]
        public void SerializationOfInheritenceIsNotLossy_EvenWhenWeAskForTheBaseType()
        {
            var obj1 = new SubClassedObject { Title = "The Title", ABool = true };
            var hydratedObj1 = (SubClassedObject)BsonDeserializer.Deserialize<SuperClassObject>(BsonSerializer.Serialize(obj1));
            Assert.Equal(obj1.Title, hydratedObj1.Title);
            Assert.Equal(obj1.ABool, hydratedObj1.ABool);
        }

        [Fact]
        public void SerializationOfInheritenceIsNotLossy_EvenWhenDiscriminatorIsOnAnInterface()
        {
            var obj1 = new InterfaceDiscriminatedClass();
            var hydratedObj1 = BsonDeserializer.Deserialize<InterfaceDiscriminatedClass>(BsonSerializer.Serialize(obj1));

            Assert.Equal(obj1.Id, hydratedObj1.Id);
        }

        [Fact]
        public void SerializationOfInheritenceIsNotLossy_EvenWhenDiscriminatorIsOnAnInterfaceAndWeTryToDeserializeUsingTheInterface()
        {
            var obj1 = new InterfaceDiscriminatedClass();
            var hydratedObj1 = (InterfaceDiscriminatedClass)BsonDeserializer.Deserialize<IDiscriminated>(BsonSerializer.Serialize(obj1));

            Assert.Equal(obj1.Id, hydratedObj1.Id);
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
        [Fact]
        public void SerializationOfScopedCodeIsNotLossy()
        {
            var obj1 = new GeneralDTO {Code = new ScopedCode {CodeString = "function(){return 'hello world!'}"}};
            var scope = new Expando();
            scope["$ns"] = "root";
            obj1.Code.Scope = scope;

            var obj2 = BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(obj1));

            Assert.Equal(obj1.Code.CodeString, obj2.Code.CodeString);
            Assert.Equal(((Expando)obj1.Code.Scope)["$ns"],((Expando)obj2.Code.Scope)["$ns"]);
        }
        [Fact]
        public void SerializesAndDeserializesAComplexObject()
        {
            var obj1 = new GeneralDTO
            {
                Flags64 = Flags64.FlagOff,
                Flags32 = Flags32.FlagOn,
                Pi = 2d,
                AnInt = 3,
                Title = "telti",
                ABoolean = false,
                Strings = new[] { "a", "bb", "abc" },
                Bytes = new byte[] { 1, 2, 3 },
                AGuid = Guid.NewGuid(),
                ADateTime = new DateTime(2001, 4, 8, 10, 43, 23, 104),
                ARex = new Regex("it's over (9000)", RegexOptions.IgnoreCase)
            };
            var nested = new GeneralDTO { Pi = 43d, Title = "little", ARex = new Regex("^over (9000)$") };
            obj1.Nester = nested;
            var obj2 = new GeneralDTO();

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(obj1));
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(obj2));

            Assert.Equal(obj1.Pi, hydratedObj1.Pi);
            Assert.Equal(obj1.AnInt, hydratedObj1.AnInt);
            Assert.Equal(obj1.Title, hydratedObj1.Title);
            Assert.Equal(obj1.ABoolean, hydratedObj1.ABoolean);
            Assert.Equal(obj1.Bytes, hydratedObj1.Bytes);
            Assert.Equal(obj1.AGuid, hydratedObj1.AGuid);
            Assert.Equal(obj1.ADateTime.Value.ToUniversalTime().Ticks, 
                hydratedObj1.ADateTime.Value.ToUniversalTime().Ticks);
            Assert.Equal(obj1.Strings, hydratedObj1.Strings);
            Assert.Equal(obj1.Flags32, hydratedObj1.Flags32);
            Assert.Equal(obj1.Flags64, hydratedObj1.Flags64);
            Assert.Equal(obj1.Nester.Title, hydratedObj1.Nester.Title);
            Assert.Equal(obj1.Nester.Pi, hydratedObj1.Nester.Pi);
            Assert.Equal(obj1.ARex.ToString(), hydratedObj1.ARex.ToString());
            Assert.Equal(obj1.ARex.Options, hydratedObj1.ARex.Options);

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

        [Fact(Skip="Slow test")]
        public void SerializationSpeedTest()
        {
            for (var i = 0; i < 5; i++)
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                for (var j = 0; j < 10000; j++)
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

        [Fact]
        public void SerializationOfEmptyListIsNotLossy()
        {
            var obj1Bytes = BsonSerializer.Serialize(new GeneralDTO { AList = new List<string>() });
            var obj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            Assert.True(obj2.AList != null);
        } 
        
        [Fact]
        public void SerializesWithPrivateSetter()
        {
            var start = new PrivateSetter(4);
            var bytes = BsonSerializer.Serialize(start);
            var end = BsonDeserializer.Deserialize<PrivateSetter>(bytes);  
            Assert.Equal(start.Id, end.Id);
        }

        [Fact]
        public void SerializesReadonlyList()
        {
            var start = new ReadOnlyList();
            start.Names.Add("Duncan Idaho");
            var bytes = BsonSerializer.Serialize(start);
            var end = BsonDeserializer.Deserialize<ReadOnlyList>(bytes);
            Assert.Equal(1, end.Names.Count);
            Assert.Equal("Duncan Idaho", end.Names[0]);
        }
        [Fact]
        public void SerializesReadonlyICollection()
        {
            var start = new HashSetList();
            start.Names.Add("Duncan");
            start.Names.Add("Idaho");
            var bytes = BsonSerializer.Serialize(start);
            var end = BsonDeserializer.Deserialize<HashSetList>(bytes);
            Assert.Equal(2, end.Names.Count);
            Assert.Equal("Duncan", end.Names.ElementAt(0));
            Assert.Equal("Idaho", end.Names.ElementAt(1));
        }

        [Fact]
        public void SerializesDictionary()
        {
            var start = new DictionaryObject();
            start.Names.Add("Duncan Idaho", 2);
            var bytes = BsonSerializer.Serialize(start);
            var end = BsonDeserializer.Deserialize<DictionaryObject>(bytes);
            Assert.Equal(1, end.Names.Count);
            Assert.Equal("Duncan Idaho", end.Names.ElementAt(0).Key);
            Assert.Equal(2, end.Names.ElementAt(0).Value);
        }
        [Fact]
        public void SerializesIDictionary()
        {
            var start = new IDictionaryObject();
            start.Names.Add("Duncan Idaho", 2);
            var bytes = BsonSerializer.Serialize(start);
            var end = BsonDeserializer.Deserialize<IDictionaryObject>(bytes);
            Assert.Equal(1, end.Names.Count);
            Assert.Equal("Duncan Idaho", end.Names.ElementAt(0).Key);
            Assert.Equal(2, end.Names.ElementAt(0).Value);
        }
       
        [Fact]
        public void SerializesReadonlyDictionary()
        {
            var start = new ReadOnlyDictionary();
            start.Names.Add("Duncan Idaho", 2);
            var bytes = BsonSerializer.Serialize(start);
            var end = BsonDeserializer.Deserialize<ReadOnlyDictionary>(bytes);
            Assert.Equal(1, end.Names.Count);
            Assert.Equal("Duncan Idaho", end.Names.ElementAt(0).Key);
            Assert.Equal(2, end.Names.ElementAt(0).Value);
        }
        
        [Fact]
        public void WillDeserializeObjectWithPrivateConstructor()
        {
            var start = PrivateConstructor.Create("Darren Kopp");
            var bytes = BsonSerializer.Serialize(start);
            var end = BsonDeserializer.Deserialize<PrivateConstructor>(bytes);
            Assert.Equal(start.Name, end.Name);            
        }
        
        [Fact]
        public void SerializesExtraPropertiesToExpandoCollection()
        {
            var address = new Address {City = "Arrakeen", State = "Arrakis", Street = "1 Grand Palace Way", Zip = "sp1c3"};
            var bytes = BsonSerializer.Serialize(address);
            var expando = BsonDeserializer.Deserialize<ExpandoAddress>(bytes);
            Assert.Equal(expando.City, address.City);
            Assert.Equal(expando.Street, address.Street);
            Assert.Equal(expando["State"], address.State);
            Assert.Equal(expando["Zip"], address.Zip);
        }
        
    }
}
