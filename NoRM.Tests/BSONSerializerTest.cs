using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Norm.BSON;
using System.Linq;
using System.Globalization;
using Norm.Configuration;
using bser = Norm.BSON.BsonSerializer;

namespace Norm.Tests
{
    [TestFixture]
    public class BSONSerializerTest
    {
        [Test]
        public void DoesntSerializeIgnoredProperties()
        {
            var o = new GeneralDTO {IgnoredProperty = 4};
            Assert.AreEqual(0, BsonDeserializer.Deserialize<GeneralDTO>(bser.Serialize(o)).IgnoredProperty);
        }

        [Test]
        public void Should_Not_Serialize_When_Default_Values_Specified()
        {
            var o = new SerializerTest() { Id = 1, Message = "Test" };
            // when serialized it should produce a document {"Id": 1} 
            var o1 = BsonSerializer.Serialize(o);
            // create a object with value Id = 1
            var s1 = BsonSerializer.Serialize(new { Id = 1 });
            // both should be equal
            Assert.AreEqual(o1, s1);
        }

        [Test]
        public void SerializationOfEnumIsNotLossy()
        {
            var obj1 = new GeneralDTO{ Flags32 = Flags32.FlagOn, Flags64 = Flags64.FlagOff };
            var obj2 = new GeneralDTO();

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(obj1));
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(obj2));

            Assert.AreEqual(obj1.Flags32, hydratedObj1.Flags32);
            Assert.AreEqual(null, hydratedObj2.Flags32);

            Assert.AreEqual(obj1.Flags64, hydratedObj1.Flags64);
            Assert.AreEqual(null, hydratedObj2.Flags64);
        }

        [Test]
        public void SerializationOfFlyweightIsNotLossy()
        {
            var testObj = new Expando();
            testObj["astring"] = "stringval";
            var testBytes = BsonSerializer.Serialize(testObj);
            var hydrated = BsonDeserializer.Deserialize<Expando>(testBytes);
            Assert.AreEqual(testObj["astring"], hydrated["astring"]);
        }
        
        [Test]
        public void SerializesAndDeserializesAFloat()
        {
            var o = new GeneralDTO {AFloat = 1.4f};
            Assert.AreEqual(1.4f, BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(o)).AFloat);
        }
        [Test]
        public void SerializingPocoGeneratesBytes()
        {
            var dummy = new GeneralDTO { Title = "Testing" };
            Assert.IsTrue(BsonSerializer.Serialize(dummy).Length > 0);
        }
        
        [Test]
        public void SerializationOfDatesHasMillisecondPrecision ()
        {
            var n = DateTime.Now;
            //some rounding issues when ticks are involved.
            n = n - new TimeSpan (n.Ticks);
            
            var obj1 = new GeneralDTO { ADateTime = null };
            var obj2 = new GeneralDTO { ADateTime = n };

            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(null, hydratedObj1.ADateTime);

            //Mongo stores dates as long, therefore, we have to use double->long rounding.
            Assert.AreEqual((long)((obj2.ADateTime.Value.ToUniversalTime() - DateTime.MinValue)).TotalMilliseconds,
                (long)(hydratedObj2.ADateTime.Value - DateTime.MinValue).TotalMilliseconds);

        }

        [Test]
        public void SerializationOfStringsAreNotLossy()
        {
            var obj1 = new GeneralDTO { Title = null };
            var obj2 = new GeneralDTO { Title = "Hello World" };

            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(null, hydratedObj1.Title);
            Assert.AreEqual(obj2.Title, hydratedObj2.Title);
        }

        [Test]
        public void SerializationOfNestedObjectsIsNotLossy()
        {
            var obj1 = new GeneralDTO { Title = "Hello World", Nester = new GeneralDTO { Title = "Bob", AnInt = 42 } };

            var obj1Bytes = BsonSerializer.Serialize(obj1);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);

            Assert.AreEqual(obj1.Title, hydratedObj1.Title);
            Assert.AreEqual(obj1.Nester.Title, hydratedObj1.Nester.Title);
            Assert.AreEqual(obj1.Nester.AnInt, hydratedObj1.Nester.AnInt);

        }

        [Test]
        public void RecursiveNestedTypesDontCauseInfiniteLoop()
        {
            var obj1 = new GeneralDTO { Title = "Hello World", Nester = new GeneralDTO { Title = "Bob", AnInt = 42 } };
            var obj1Bytes = BsonSerializer.Serialize(obj1);
            BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);

        }

        [Test]
        public void SerializationOfDoublesAreNotLossy()
        {
            var obj1 = new GeneralDTO { Pi = 3.1415927d };
            var obj2 = new GeneralDTO { Pi = null };

            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.Pi, hydratedObj1.Pi);
            Assert.AreEqual(null, hydratedObj2.Pi);
        }

        [Test]
        public void SerializationOfIEnumerableTIsNotLossy()
        {
            var gto = new GeneralDTO{ AnIEnumerable = new List<Person>(){ new Person(), new Person()}};
            var bytes = BsonSerializer.Serialize(gto);
        
            var gto2 = BsonDeserializer.Deserialize<GeneralDTO>(bytes);
            Assert.AreEqual(2, gto2.AnIEnumerable.Count());
        }

        [Test]
        public void SerializationOfIntsAreNotLossy()
        {
            var obj1 = new GeneralDTO { AnInt = 100 };
            var obj2 = new GeneralDTO { AnInt = null };


            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.AnInt, hydratedObj1.AnInt);
            Assert.AreEqual(null, hydratedObj2.AnInt);
        }

        [Test]
        public void SerializationOfBooleansAreNotLossy()
        {
            var obj1 = new GeneralDTO { ABoolean = true };
            var obj2 = new GeneralDTO { ABoolean = null };

            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.ABoolean, hydratedObj1.ABoolean);
            Assert.AreEqual(null, hydratedObj2.ABoolean);
        }

        [Test]
        public void SerializationOfBytesIsNotLossy()
        {
            var bin = new List<byte>();
            for(int i = 1; i < 1000; i++)
            {
                bin.AddRange(BitConverter.GetBytes(i));
            }

            var obj1 = new GeneralDTO { Bytes = bin.ToArray() };
            var obj2 = new GeneralDTO { Bytes = null };

            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.True(obj1.Bytes.SequenceEqual(hydratedObj1.Bytes));
            Assert.AreEqual(null, hydratedObj2.Bytes);
        }

        [Test]
        public void SerializationOfGuidIsNotLossy()
        {
            var obj1 = new GeneralDTO { AGuid = Guid.NewGuid() };
            var obj2 = new GeneralDTO { AGuid = null };

            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.AGuid, hydratedObj1.AGuid);
            Assert.AreEqual(null, hydratedObj2.AGuid);
        }

        [Test]
        public void SerializationOfInheritenceIsNotLossy()
        {
            var obj1 = new SubClassedObject {Title = "Subclassed", ABool = true};
            var hydratedObj1 = BsonDeserializer.Deserialize<SubClassedObject>(BsonSerializer.Serialize(obj1));
            Assert.AreEqual(obj1.Title, hydratedObj1.Title);
            Assert.AreEqual(obj1.ABool, hydratedObj1.ABool);
        }

        [Test]
        public void SerializationOfInheritenceIsNotLossy_EvenWhenWeAskForTheBaseType()
        {
            var obj1 = new SubClassedObject { Title = "The Title", ABool = true };
            var hydratedObj1 = (SubClassedObject)BsonDeserializer.Deserialize<SuperClassObject>(BsonSerializer.Serialize(obj1));
            Assert.AreEqual(obj1.Title, hydratedObj1.Title);
            Assert.AreEqual(obj1.ABool, hydratedObj1.ABool);
        }

        [Test]
        public void SerializationOfInheritenceIsNotLossy_EvenWhenDiscriminatorIsOnAnInterface()
        {
            var obj1 = new InterfaceDiscriminatedClass();
            var hydratedObj1 = BsonDeserializer.Deserialize<InterfaceDiscriminatedClass>(BsonSerializer.Serialize(obj1));

            Assert.AreEqual(obj1.Id, hydratedObj1.Id);
        }

        [Test]
        public void SerializationOfInheritenceIsNotLossy_EvenWhenDiscriminatorIsOnAnInterfaceAndWeTryToDeserializeUsingTheInterface()
        {
            var obj1 = new InterfaceDiscriminatedClass();
            var hydratedObj1 = (InterfaceDiscriminatedClass)BsonDeserializer.Deserialize<IDiscriminated>(BsonSerializer.Serialize(obj1));

            Assert.AreEqual(obj1.Id, hydratedObj1.Id);
        }

        [Test]
        public void SerializationOfRegexIsNotLossy()
        {
            var obj1 = new GeneralDTO { ARex = new Regex("[0-9]{5}", RegexOptions.Multiline) };
            var obj2 = new GeneralDTO { ARex = null };

            var obj1Bytes = BsonSerializer.Serialize(obj1);
            var obj2Bytes = BsonSerializer.Serialize(obj2);

            var hydratedObj1 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            var hydratedObj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj2Bytes);

            Assert.AreEqual(obj1.ARex.ToString(), hydratedObj1.ARex.ToString());
            Assert.AreEqual(obj1.ARex.Options, hydratedObj1.ARex.Options);
            Assert.AreEqual(null, hydratedObj2.ARex);
            //more tests would be useful for all the options.
        }
        [Test]
        public void SerializationOfScopedCodeIsNotLossy()
        {
            var obj1 = new GeneralDTO {Code = new ScopedCode {CodeString = "function(){return 'hello world!'}"}};
            var scope = new Expando();
            scope["$ns"] = "root";
            obj1.Code.Scope = scope;

            var obj2 = BsonDeserializer.Deserialize<GeneralDTO>(BsonSerializer.Serialize(obj1));

            Assert.AreEqual(obj1.Code.CodeString, obj2.Code.CodeString);
            Assert.AreEqual(((Expando)obj1.Code.Scope)["$ns"],((Expando)obj2.Code.Scope)["$ns"]);
        }
        [Test]
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

            Assert.AreEqual(obj1.Pi, hydratedObj1.Pi);
            Assert.AreEqual(obj1.AnInt, hydratedObj1.AnInt);
            Assert.AreEqual(obj1.Title, hydratedObj1.Title);
            Assert.AreEqual(obj1.ABoolean, hydratedObj1.ABoolean);
            Assert.AreEqual(obj1.Bytes, hydratedObj1.Bytes);
            Assert.AreEqual(obj1.AGuid, hydratedObj1.AGuid);
            Assert.AreEqual(obj1.ADateTime.Value.ToUniversalTime().Ticks, 
                hydratedObj1.ADateTime.Value.ToUniversalTime().Ticks);
            Assert.AreEqual(obj1.Strings, hydratedObj1.Strings);
            Assert.AreEqual(obj1.Flags32, hydratedObj1.Flags32);
            Assert.AreEqual(obj1.Flags64, hydratedObj1.Flags64);
            Assert.AreEqual(obj1.Nester.Title, hydratedObj1.Nester.Title);
            Assert.AreEqual(obj1.Nester.Pi, hydratedObj1.Nester.Pi);
            Assert.AreEqual(obj1.ARex.ToString(), hydratedObj1.ARex.ToString());
            Assert.AreEqual(obj1.ARex.Options, hydratedObj1.ARex.Options);

            Assert.AreEqual(obj2.Pi, hydratedObj2.Pi);
            Assert.AreEqual(obj2.AnInt, hydratedObj2.AnInt);
            Assert.AreEqual(obj2.Title, hydratedObj2.Title);
            Assert.AreEqual(obj2.ABoolean, hydratedObj2.ABoolean);
            Assert.AreEqual(obj2.Bytes, hydratedObj2.Bytes);
            Assert.AreEqual(obj2.AGuid, hydratedObj2.AGuid);
            Assert.AreEqual(obj2.ADateTime, hydratedObj2.ADateTime);
            Assert.AreEqual(obj2.Strings, hydratedObj2.Strings);
            Assert.AreEqual(obj2.Flags32, hydratedObj2.Flags32);
            Assert.AreEqual(obj2.Flags64, hydratedObj2.Flags64);
            Assert.AreEqual(obj2.Nester, hydratedObj2.Nester);
            Assert.AreEqual(obj2.ARex, hydratedObj2.ARex);
        }

        //[Fact(Skip="Slow test")]
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

        [Test]
        public void SerializationOfEmptyListIsNotLossy()
        {
            var obj1Bytes = BsonSerializer.Serialize(new GeneralDTO { AList = new List<string>() });
            var obj2 = BsonDeserializer.Deserialize<GeneralDTO>(obj1Bytes);
            Assert.True(obj2.AList != null);
        } 
        
        [Test]
        public void SerializesWithPrivateSetter()
        {
            var start = new PrivateSetter(4);
            var bytes = BsonSerializer.Serialize(start);
            var end = BsonDeserializer.Deserialize<PrivateSetter>(bytes);  
            Assert.AreEqual(start.Id, end.Id);
        }

        [Test]
        public void SerializesReadonlyList()
        {
            var start = new ReadOnlyList();
            start.Names.Add("Duncan Idaho");
            var bytes = BsonSerializer.Serialize(start);
            var end = BsonDeserializer.Deserialize<ReadOnlyList>(bytes);
            Assert.AreEqual(1, end.Names.Count);
            Assert.AreEqual("Duncan Idaho", end.Names[0]);
        }
        [Test]
        public void SerializesReadonlyICollection()
        {
            var start = new HashSetList();
            start.Names.Add("Duncan");
            start.Names.Add("Idaho");
            var bytes = BsonSerializer.Serialize(start);
            var end = BsonDeserializer.Deserialize<HashSetList>(bytes);
            Assert.AreEqual(2, end.Names.Count);
            Assert.AreEqual("Duncan", end.Names.ElementAt(0));
            Assert.AreEqual("Idaho", end.Names.ElementAt(1));
        }

        [Test]
        public void SerializesDictionary()
        {
            var start = new DictionaryObject();
            start.Names.Add("Duncan Idaho", 2);
            var bytes = BsonSerializer.Serialize(start);
            var end = BsonDeserializer.Deserialize<DictionaryObject>(bytes);
            Assert.AreEqual(1, end.Names.Count);
            Assert.AreEqual("Duncan Idaho", end.Names.ElementAt(0).Key);
            Assert.AreEqual(2, end.Names.ElementAt(0).Value);
        }
        [Test]
        public void SerializesIDictionary()
        {
            var start = new IDictionaryObject();
            start.Names.Add("Duncan Idaho", 2);
            var bytes = BsonSerializer.Serialize(start);
            var end = BsonDeserializer.Deserialize<IDictionaryObject>(bytes);
            Assert.AreEqual(1, end.Names.Count);
            Assert.AreEqual("Duncan Idaho", end.Names.ElementAt(0).Key);
            Assert.AreEqual(2, end.Names.ElementAt(0).Value);
        }
       
        [Test]
        public void SerializesReadonlyDictionary()
        {
            var start = new ReadOnlyDictionary();
            start.Names.Add("Duncan Idaho", 2);
            var bytes = BsonSerializer.Serialize(start);
            var end = BsonDeserializer.Deserialize<ReadOnlyDictionary>(bytes);
            Assert.AreEqual(1, end.Names.Count);
            Assert.AreEqual("Duncan Idaho", end.Names.ElementAt(0).Key);
            Assert.AreEqual(2, end.Names.ElementAt(0).Value);
        }
        
        [Test]
        public void WillDeserializeObjectWithPrivateConstructor()
        {
            var start = PrivateConstructor.Create("Darren Kopp");
            var bytes = BsonSerializer.Serialize(start);
            var end = BsonDeserializer.Deserialize<PrivateConstructor>(bytes);
            Assert.AreEqual(start.Name, end.Name);            
        }
        
        [Test]
        public void SerializesExtraPropertiesToExpandoCollection()
        {
            var address = new Address {City = "Arrakeen", State = "Arrakis", Street = "1 Grand Palace Way", Zip = "sp1c3"};
            var bytes = BsonSerializer.Serialize(address);
            var expando = BsonDeserializer.Deserialize<ExpandoAddress>(bytes);
            Assert.AreEqual(expando.City, address.City);
            Assert.AreEqual(expando.Street, address.Street);
            Assert.AreEqual(expando["State"], address.State);
            Assert.AreEqual(expando["Zip"], address.Zip);
        }

        [Test]
        public void SerializesCultureInfo()
        {
            var s1 = new CultureInfoDTO() { Culture = CultureInfo.GetCultureInfo("en-US") };
            var bytes = BsonSerializer.Serialize(s1);
            var s2 = BsonDeserializer.Deserialize<CultureInfoDTO>(bytes);
            Assert.AreEqual(s1.Culture, s2.Culture);
        }

        [Test]
        public void SerializesClassWithCustomValueObjectUsingCustomTypeConverter()
        {
            IMongoConfigurationMap cfg = new MongoConfigurationMap();
            cfg.TypeConverterFor<NonSerializableValueObject, NonSerializableValueObjectTypeConverter>();
            BsonSerializer.UseConfiguration(cfg);

            // Verify that a contained, normally unserializable, value can be serialized with a proper type converter
            var s1 = new NonSerializableClass() 
                { 
                    Value = new NonSerializableValueObject("12345"),
                    Text = "Abc"
                };
            var bytes = BsonSerializer.Serialize(s1);
            var s2 = BsonDeserializer.Deserialize<NonSerializableClass>(bytes);
            Assert.AreEqual(s1.Value.Number, s2.Value.Number);
            Assert.AreEqual(s1.Text, s2.Text);

            BsonSerializer.UseConfiguration(null);
        }
    }
}
