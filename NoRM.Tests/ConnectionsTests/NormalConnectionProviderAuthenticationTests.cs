namespace NoRM.Tests
{
    using Xunit;

    public class NormalConnectionProviderAuthenticationTests : AuthenticatedFixture
    {
        [Fact]
        public void ThrowsExceptionIfConnectingWithInvalidCredentials()
        {
            var provider = new NormalConnectionProvider(ConnectionStringBuilder.Create(AuthenticatedConnectionString("bad", "boy")));
            var ex = Assert.Throws<MongoException>(() => provider.Open(null));
            Assert.Equal("Authentication Failed", ex.Message);
        }


        //this test is not passing against 1.3.2. Appears to be a server bug. Openend an issue at:
        //http://jira.mongodb.org/browse/SERVER-678
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