using NUnit.Framework;
using System.ComponentModel;
using System;

namespace Norm.Tests
{
    [TestFixture]
    public class ObjectIdTests
    {
        [Test]
        public void ObjectIDs_Can_Convert_To_And_From_Strings()
        {
            ObjectIdTypeConverter tc = new ObjectIdTypeConverter();
            var obj = ObjectId.NewObjectId();

            Assert.True(tc.CanConvertFrom(typeof(String)));
            var conv = tc.ConvertFrom(obj.ToString());
            Assert.AreEqual(obj, conv);
            Assert.Throws<NotSupportedException>(()=>tc.ConvertFrom(Guid.NewGuid()));
        }

        [Test]
        public void ObjectIDs_Return_Unique_Hashcode()
        {
            var obj = ObjectId.NewObjectId();
            Assert.AreNotEqual(0, obj.GetHashCode());
        }

        [Test]
        public void TryParseReturnsFalseIfObjectIdIsNull()
        {
            ObjectId objectId;
            Assert.AreEqual(false, ObjectId.TryParse(null, out objectId));
        }

        [Test]
        public void ImplicitConversionOfOIDToAndFromStringWorks()
        {
            ObjectId oid = ObjectId.NewObjectId();
            string str = oid;
            Assert.AreEqual(oid, (ObjectId)str);

            str = null;
            Assert.AreEqual(ObjectId.Empty, (ObjectId)str);
            Assert.AreEqual(ObjectId.Empty, (ObjectId)"");
        }

        [Test]
        public void TryParseReturnsFalseIfObjectIdIsEmpty()
        {
            ObjectId objectId;
            Assert.AreEqual(false, ObjectId.TryParse(string.Empty, out objectId));
        }
        [Test]
        public void TryParseReturnsFalseIfObjectIdIsnt24Characters()
        {
            ObjectId objectId;
            Assert.AreEqual(false, ObjectId.TryParse("a", out objectId));
            Assert.AreEqual(false, ObjectId.TryParse(new string('b', 23), out objectId));
            Assert.AreEqual(false, ObjectId.TryParse(new string('b', 25), out objectId));
        }
        [Test]
        public void TryParseReturnsFalseIfObjectIdIsinvalid()
        {
            ObjectId objectId;
            Assert.AreEqual(false, ObjectId.TryParse(new string('*', 24), out objectId));
            Assert.AreEqual(false, ObjectId.TryParse(new string('1', 23) + '-', out objectId));
        }
        [Test]
        public void ReturnsParsedObjectId()
        {
            ObjectId objectId;
            Assert.AreEqual(true, ObjectId.TryParse("4b883faad657000000002665", out objectId));
            Assert.AreNotEqual(ObjectId.Empty, objectId);
            Assert.AreEqual(true, ObjectId.TryParse("1234567890abcdef123456ab", out objectId));
            Assert.AreNotEqual(ObjectId.Empty, objectId);
            Assert.AreEqual(true, ObjectId.TryParse("1234567890abCDEf123456ab", out objectId));
            Assert.AreNotEqual(ObjectId.Empty, objectId);
        }
        [Test]
        public void ObjectIdWithSameValueAreEqual()
        {
            var a = new ObjectId("4b883faad657000000002665");
            var b = new ObjectId("4b883faad657000000002665");
            Assert.AreEqual(a, b);
            Assert.True(a == b);
        }
        [Test]
        public void ObjectIdWithDifferentValuesAreNotEqual()
        {
            var a = new ObjectId("4b883faad657000000002665");
            var b = new ObjectId("4b883faad657000000002666");
            Assert.AreNotEqual(a, b);
            Assert.True(a != b);
        }
        [Test]
        public void ConversionFromStringToOIDUsingTypeConverterWorks()
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(ObjectId));
            Assert.True(converter.CanConvertFrom(typeof(string)));
            string value = "4b883faad657000000002665";
            ObjectId objectId = converter.ConvertFrom(value) as ObjectId;
            Assert.NotNull(objectId);
            Assert.AreEqual(value, objectId.ToString());
        }
        [Test]
        public void ConversionToStringWithNullObjIsNull()
        {
            var obj = new { Id = (ObjectId)null };
            string implicitString = obj.Id;
            Assert.Null(implicitString);
        }
    }
}