using Xunit;

namespace NoRM.Tests
{
    public class NormalConnectionProviderAuthenticationTests : AuthenticatedFixture
    {
        [Fact]
        public void ThrowsExceptionIfConnectingWithInvalidCredentials()
        {
            var provider = new NormalConnectionProvider(ConnectionStringBuilder.Create(AuthenticatedConnectionString("bad", "boy")));
            var ex = Assert.Throws<MongoException>(() => provider.Open(null));
            Assert.Equal("auth fails", ex.Message);
        }


        //won't pass on some 1.3.x builds of the server, but will pass against newest, or stable (1.2.3).
        [Fact(Skip="XUnit misbehaves with this test. Must investigate.")]
        public void AuthenticatesWithProperCredentials()
        {
            var provider = new NormalConnectionProvider(ConnectionStringBuilder.Create(AuthenticatedConnectionString("usr", "8e156e298e19afdc3a104ddd172a2bee")));
            var connection = provider.Open(null);
            Assert.Equal(true, connection.Client.Connected);
            provider.Close(connection);           
        }
    }
}