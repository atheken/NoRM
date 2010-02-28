using Xunit;

namespace NoRM.Tests
{
    public class OIDTests
    {
        [Fact]
        public void TryParseReturnsFalseIfOIDIsNull()
        {
            ObjectId objectId;
            Assert.Equal(false, ObjectId.TryParse(null, out objectId));
        }
        [Fact]
        public void TryParseReturnsFalseIfOIDIsEmpty()
        {
            ObjectId objectId;
            Assert.Equal(false, ObjectId.TryParse(string.Empty, out objectId));
        }
        [Fact]
        public void TryParseReturnsFalseIfOIDIsnt24Characters()
        {
            ObjectId objectId;
            Assert.Equal(false, ObjectId.TryParse("a", out objectId));
            Assert.Equal(false, ObjectId.TryParse(new string('b', 23), out objectId));
            Assert.Equal(false, ObjectId.TryParse(new string('b', 25), out objectId));
        }
        [Fact]
        public void TryParseReturnsFalseIfOIDIsinvalid()
        {
            ObjectId objectId;
            Assert.Equal(false, ObjectId.TryParse(new string('*', 24), out objectId));
            Assert.Equal(false, ObjectId.TryParse(new string('1', 23) + '-', out objectId));
        }
        [Fact]
        public void ReturnsParsedOID()
        {
            ObjectId objectId;
            Assert.Equal(true, ObjectId.TryParse("4b883faad657000000002665", out objectId));
            Assert.NotEqual(ObjectId.EMPTY, objectId);
            Assert.Equal(true, ObjectId.TryParse("1234567890abcdef123456ab", out objectId));
            Assert.NotEqual(ObjectId.EMPTY, objectId);
            Assert.Equal(true, ObjectId.TryParse("1234567890abCDEf123456ab", out objectId));
            Assert.NotEqual(ObjectId.EMPTY, objectId);
        }
    }
}