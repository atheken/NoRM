using Xunit;

namespace Norm.Tests
{
    public class NormalConnectionProviderAuthenticationTests : AuthenticatedFixture
    {
        [Fact(Skip="authenticated connection seems to be hanging when we run this in sequence")]
        public void ThrowsExceptionIfConnectingWithInvalidCredentials()
        {
            var provider = new NormalConnectionProvider(ConnectionStringBuilder.Create(AuthenticatedConnectionString("bad", "boy")));

            var ex = Assert.Throws<MongoException>(() => provider.Open(null));
            Assert.Equal("auth fails", ex.Message);
        }

        //won't pass on some 1.3.x builds of the server, but will pass against newest, or stable (1.2.3).
        [Fact]
        public void AuthenticatesWithProperCredentials()
        {
            var provider = new NormalConnectionProvider(ConnectionStringBuilder.Create(AuthenticatedConnectionString("usr", "8e156e298e19afdc3a104ddd172a2bee")));
            var connection = provider.Open(null);
            Assert.Equal(true, connection.Client.Connected);
            provider.Close(connection);
        }

        
    }
}