namespace NoRM.Tests
{
    using Xunit;

    public class OIDTests
    {
        [Fact]
        public void TryParseReturnsFalseIfOIDIsNull()
        {
            OID oid;
            Assert.Equal(false, OID.TryParse(null, out oid));
        }
        [Fact]
        public void TryParseReturnsFalseIfOIDIsEmpty()
        {
            OID oid;
            Assert.Equal(false, OID.TryParse(string.Empty, out oid));
        }
        [Fact]
        public void TryParseReturnsFalseIfOIDIsnt24Characters()
        {
            OID oid;
            Assert.Equal(false, OID.TryParse("a", out oid));
            Assert.Equal(false, OID.TryParse(new string('b', 23), out oid));
            Assert.Equal(false, OID.TryParse(new string('b', 25), out oid));
        }
        [Fact]
        public void TryParseReturnsFalseIfOIDIsinvalid()
        {
            OID oid;
            Assert.Equal(false, OID.TryParse(new string('*', 24), out oid));
            Assert.Equal(false, OID.TryParse(new string('1', 23) + '-', out oid));
        }
        [Fact]
        public void ReturnsParsedOID()
        {
            OID oid;
            Assert.Equal(true, OID.TryParse("4b883faad657000000002665", out oid));
            Assert.NotEqual(OID.EMPTY, oid);
            Assert.Equal(true, OID.TryParse("1234567890abcdef123456ab", out oid));
            Assert.NotEqual(OID.EMPTY, oid);
            Assert.Equal(true, OID.TryParse("1234567890abCDEf123456ab", out oid));
            Assert.NotEqual(OID.EMPTY, oid);
        }
    }
}