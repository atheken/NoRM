using System;
using System.Net.Sockets;
using Xunit;

namespace Norm.Tests
{
    public class ConnectionTest
    {
        [Fact]
        public void ThrowsExceptionIfCantConnect()
        {
            Assert.Throws<SocketException>(() => new Connection(ConnectionStringBuilder.Create("mongodb://localhost2/test")));
        }
        [Fact]
        public void ThrowsExceptionWhenTryingToOverridePooling()
        {
            using (var connection = new DisposableConnection(ConnectionStringBuilder.Create(TestHelper.ConnectionString())))
            {
                var ex = Assert.Throws<MongoException>(() => connection.LoadOptions("pooling=true"));
                Assert.Equal("Connection pooling cannot be provided as an override option", ex.Message);
            }
        }
        [Fact]
        public void ThrowsExceptionWhenTryingToOverridePoolSize()
        {
            using (var connection = new DisposableConnection(ConnectionStringBuilder.Create(TestHelper.ConnectionString())))
            {
                var ex = Assert.Throws<MongoException>(() => connection.LoadOptions("poolsize=23"));
                Assert.Equal("PoolSize cannot be provided as an override option", ex.Message);
            }
        }
        [Fact]
        public void ThrowsExceptionWhenTryingToOverrideTimeout()
        {
            using (var connection = new DisposableConnection(ConnectionStringBuilder.Create(TestHelper.ConnectionString())))
            {
                var ex = Assert.Throws<MongoException>(() => connection.LoadOptions("timeout=23"));
                Assert.Equal("Timeout cannot be provided as an override option", ex.Message);
            }
        }
        [Fact]
        public void ThrowsExceptionWhenTryingToOverrideLifetime()
        {
            using (var connection = new DisposableConnection(ConnectionStringBuilder.Create(TestHelper.ConnectionString())))
            {
                var ex = Assert.Throws<MongoException>(() => connection.LoadOptions("lifetime=23"));
                Assert.Equal("Lifetime cannot be provided as an override option", ex.Message);
            }
        }
        [Fact]
        public void OverridesStrictMode()
        {
            using (var connection = new DisposableConnection(ConnectionStringBuilder.Create(TestHelper.ConnectionString("?strict=true"))))
            {
                Assert.Equal(true, connection.StrictMode);
                connection.LoadOptions("strict=false");
                Assert.Equal(false, connection.StrictMode);
            }
        }
        [Fact]
        public void OverridesQueryTimeout()
        {
            using (var connection = new DisposableConnection(ConnectionStringBuilder.Create(TestHelper.ConnectionString("querytimeout=30"))))
            {
                Assert.Equal(30, connection.QueryTimeout);
                connection.LoadOptions("querytimeout=32");
                Assert.Equal(32, connection.QueryTimeout);
            }
        }
       

        [Fact]
        public void ResetsDefaults()
        {
            using (var connection = new DisposableConnection(ConnectionStringBuilder.Create(TestHelper.ConnectionString("querytimeout=30&strict=false"))))
            {
                connection.LoadOptions("querytimeout=23&strict=true");
                Assert.Equal(true, connection.StrictMode);
                Assert.Equal(23, connection.QueryTimeout);

                connection.ResetOptions();
                Assert.Equal(false, connection.StrictMode);
                Assert.Equal(30, connection.QueryTimeout);
            }
        }

        [Fact]
        public void CreatesDigestFromNonce()
        {
            using (var connection = new DisposableConnection(ConnectionStringBuilder.Create(TestHelper.ConnectionString("querytimeout=30&strict=false", "ussrr", "ppaassss"))))
            {
                Assert.Equal("08f11f775e2a8cf4248f0ae6126164f0", connection.Digest("1234abc"));
            }
        }

        private class DisposableConnection : Connection, IDisposable
        {
            private bool _disposed;

            internal DisposableConnection(ConnectionStringBuilder builder) : base(builder) { }


            new public void Dispose()
            {
                Dispose(true);
            }
            
            new private void Dispose(bool disposing)
            {
                if (_disposed && disposing)
                {
                    Client.Close();
                }
                _disposed = true;
            }
            ~DisposableConnection()
            {
                Dispose(false);
            }
        }
    }
}