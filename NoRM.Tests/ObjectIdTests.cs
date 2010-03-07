namespace NoRM.Tests
{
    using Xunit;

    public class ObjectIdTests
    {
        [Fact]
        public void TryParseReturnsFalseIfObjectIdIsNull()
        {
            ObjectId objectId;
            Assert.Equal(false, ObjectId.TryParse(null, out objectId));
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
        }
        [Fact]
        public void ObjectIdWithDifferentValuesAreNotEqual()
        {
            var a = new ObjectId("4b883faad657000000002665");
            var b = new ObjectId("4b883faad657000000002666");
            Assert.NotEqual(a, b);
        }
    }
}