using Xunit;
using System.ComponentModel;
using System;

namespace Norm.Tests
{
    public class ObjectIdTests
    {
        [Fact]
        public void ObjectIDs_Can_Convert_To_And_From_Strings()
        {
            ObjectIdTypeConverter tc = new ObjectIdTypeConverter();
            var obj = ObjectId.NewObjectId();

            Assert.True(tc.CanConvertFrom(typeof(String)));
            var conv = tc.ConvertFrom(obj.ToString());
            Assert.Equal(obj, conv);
            Assert.Throws<NotSupportedException>(()=>tc.ConvertFrom(Guid.NewGuid()));
        }

        [Fact]
        public void ObjectIDs_Return_Unique_Hashcode()
        {
            var obj = ObjectId.NewObjectId();
            Assert.NotEqual(0, obj.GetHashCode());
        }

        [Fact]
        public void TryParseReturnsFalseIfObjectIdIsNull()
        {
            ObjectId objectId;
            Assert.Equal(false, ObjectId.TryParse(null, out objectId));
        }

        [Fact]
        public void ImplicitConversionOfOIDToAndFromStringWorks()
        {
            ObjectId oid = ObjectId.NewObjectId();
            string str = oid;
            Assert.Equal(oid, (ObjectId)str);

            str = null;
            Assert.Equal(ObjectId.Empty, (ObjectId)str);
            Assert.Equal(ObjectId.Empty, (ObjectId)"");
        }

        [Fact]
        public void TryParseReturnsFalseIfObjectIdIsEmpty()
        {
            ObjectId objectId;
            Assert.Equal(false, ObjectId.TryParse(string.Empty, out objectId));
        }
        [Fact]
        public void TryParseReturnsFalseIfObjectIdIsnt24Characters()
        {
            ObjectId objectId;
            Assert.Equal(false, ObjectId.TryParse("a", out objectId));
            Assert.Equal(false, ObjectId.TryParse(new string('b', 23), out objectId));
            Assert.Equal(false, ObjectId.TryParse(new string('b', 25), out objectId));
        }
        [Fact]
        public void TryParseReturnsFalseIfObjectIdIsinvalid()
        {
            ObjectId objectId;
            Assert.Equal(false, ObjectId.TryParse(new string('*', 24), out objectId));
            Assert.Equal(false, ObjectId.TryParse(new string('1', 23) + '-', out objectId));
        }
        [Fact]
        public void ReturnsParsedObjectId()
        {
            ObjectId objectId;
            Assert.Equal(true, ObjectId.TryParse("4b883faad657000000002665", out objectId));
            Assert.NotEqual(ObjectId.Empty, objectId);
            Assert.Equal(true, ObjectId.TryParse("1234567890abcdef123456ab", out objectId));
            Assert.NotEqual(ObjectId.Empty, objectId);
            Assert.Equal(true, ObjectId.TryParse("1234567890abCDEf123456ab", out objectId));
            Assert.NotEqual(ObjectId.Empty, objectId);
        }
        [Fact]
        public void ObjectIdWithSameValueAreEqual()
        {
            var a = new ObjectId("4b883faad657000000002665");
            var b = new ObjectId("4b883faad657000000002665");
            Assert.Equal(a, b);
            Assert.True(a == b);
        }
        [Fact]
        public void ObjectIdWithDifferentValuesAreNotEqual()
        {
            var a = new ObjectId("4b883faad657000000002665");
            var b = new ObjectId("4b883faad657000000002666");
            Assert.NotEqual(a, b);
            Assert.True(a != b);
        }
        [Fact]
        public void ConversionFromStringToOIDUsingTypeConverterWorks()
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(ObjectId));
            Assert.True(converter.CanConvertFrom(typeof(string)));
            string value = "4b883faad657000000002665";
            ObjectId objectId = converter.ConvertFrom(value) as ObjectId;
            Assert.NotNull(objectId);
            Assert.Equal(value, objectId.ToString());
        }
		[Fact]
		public void ConversionToStringWithNullObjIsNull()
		{
			var obj = new { Id = (ObjectId)null };
			string implicitString = obj.Id;
			Assert.Null(implicitString);
		}
    }
}