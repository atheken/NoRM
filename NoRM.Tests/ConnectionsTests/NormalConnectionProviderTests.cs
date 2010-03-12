using Xunit;

namespace NoRM.Tests
{
    public class NormalConnectionProviderTests
    {
        [Fact]
        public void CreatesANewConnectionForEachOpen()
        {
            IConnection connection1 = null;
            IConnection connection2 = null;
            var provider = new NormalConnectionProvider(ConnectionStringBuilder.Create(TestHelper.ConnectionString()));
            
            try
            {            
                connection1 = provider.Open(null);
                connection2 = provider.Open(null);
                Assert.NotSame(connection1, connection2);
            }
            finally
            {
                if (connection1 != null) { provider.Close(connection1); }
                if (connection2 != null) { provider.Close(connection2); }                
            }
        }

        [Fact]
        public void ClosesTheUnderlyingConnection()
        {
            var provider = new NormalConnectionProvider(ConnectionStringBuilder.Create(TestHelper.ConnectionString()));
            var connection = provider.Open(null);
            provider.Close(connection);
            Assert.Null(connection.Client.Client);
        }
    }
}