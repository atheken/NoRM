namespace NoRM.Tests
{
    using System;
    using Xunit;

    public class PooledConnectionProviderTests
    {
        [Fact]
        public void ClosingAConnectionReturnsItToThePool()
        {
            var provider = new PooledConnectionProvider(ConnectionStringBuilder.Create(TestHelper.ConnectionString("pooling=true&poolsize=1")));
            var connection1 = provider.Open(null);
            provider.Close(connection1);
            Assert.Same(connection1, provider.Open(null));
        }

        [Fact]
        public void ThrowsExceptionIfNoConnectionsAvailable()
        {
            var provider = new PooledConnectionProvider(ConnectionStringBuilder.Create(TestHelper.ConnectionString("pooling=true&poolsize=1&timeout=1")));
            provider.Open(null);
            
            var ex = Assert.Throws<MongoException>(() => provider.Open(null));
            Assert.Equal("Connection timeout trying to get connection from connection pool", ex.Message);
        }

        [Fact]
        public void WaitsUntilTimeoutForConnectionToFreeUp()
        {
            var provider = new PooledConnectionProvider(ConnectionStringBuilder.Create(TestHelper.ConnectionString("pooling=true&poolsize=1&timeout=3")));
            provider.Open(null);

            var start = DateTime.Now; //this doesn't seem like a very good way to do this..we'll see
            Assert.Throws<MongoException>(() => provider.Open(null));
            var elasped = DateTime.Now.Subtract(start).TotalSeconds;
            Assert.True(elasped > 3);
            Assert.True(elasped < 4);
        }

        [Fact]
        public void ReturnsDifferentConnections()
        {
            var provider = new PooledConnectionProvider(ConnectionStringBuilder.Create(TestHelper.ConnectionString("pooling=true&poolsize=2")));
            Assert.NotSame(provider.Open(null), provider.Open(null));            
        }

        [Fact]
        public void PoolsUpToPoolSizeConections()
        {
            var provider = new PooledConnectionProvider(ConnectionStringBuilder.Create(TestHelper.ConnectionString("pooling=true&poolsize=4&timeout=1")));
            provider.Open(null);
            provider.Open(null);
            provider.Open(null);
            provider.Open(null);
            Assert.Throws<MongoException>(() => provider.Open(null));
        }
    }
}