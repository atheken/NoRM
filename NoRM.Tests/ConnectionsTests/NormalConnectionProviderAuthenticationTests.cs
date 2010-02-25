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


        //this test is not passing, getnounce fails
        //if you set a breakpoint before trying to getnounce, connect to the db from a shell, and issue any command like show users (which will fail due to authentication) then let the code through, this will pass
        //this may be a mongo bug, because its reproducible on their shell
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