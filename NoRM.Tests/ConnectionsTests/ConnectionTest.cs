using System;
using System.Net.Sockets;
using NUnit.Framework;

namespace Norm.Tests
{
    [TestFixture]
    public class ConnectionTest
    {
		private Mongod _proc;

		[TestFixtureSetUp]
		public void SetUp ()
		{
			_proc = new Mongod ();
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			_proc.Dispose ();
		}

        [Test]
        public void ThrowsExceptionIfCantConnect()
        {
            Assert.Throws<SocketException>(() => new Connection(ConnectionOptions.Create("mongodb://localhost2/test")));
        }
        [Test]
        public void ThrowsExceptionWhenTryingToOverridePooling()
        {
            using (var connection = new DisposableConnection(ConnectionOptions.Create(TestHelper.ConnectionString())))
            {
                var ex = Assert.Throws<MongoException>(() => connection.LoadOptions("pooling=true"));
                Assert.AreEqual("Connection pooling cannot be provided as an override option", ex.Message);
            }
        }
        [Test]
        public void ThrowsExceptionWhenTryingToOverridePoolSize()
        {
            using (var connection = new DisposableConnection(ConnectionOptions.Create(TestHelper.ConnectionString())))
            {
                var ex = Assert.Throws<MongoException>(() => connection.LoadOptions("poolsize=23"));
                Assert.AreEqual("PoolSize cannot be provided as an override option", ex.Message);
            }
        }
        [Test]
        public void ThrowsExceptionWhenTryingToOverrideTimeout()
        {
            using (var connection = new DisposableConnection(ConnectionOptions.Create(TestHelper.ConnectionString())))
            {
                var ex = Assert.Throws<MongoException>(() => connection.LoadOptions("timeout=23"));
                Assert.AreEqual("Timeout cannot be provided as an override option", ex.Message);
            }
        }
        [Test]
        public void ThrowsExceptionWhenTryingToOverrideLifetime()
        {
            using (var connection = new DisposableConnection(ConnectionOptions.Create(TestHelper.ConnectionString())))
            {
                var ex = Assert.Throws<MongoException>(() => connection.LoadOptions("lifetime=23"));
                Assert.AreEqual("Lifetime cannot be provided as an override option", ex.Message);
            }
        }
        [Test]
        public void OverridesStrictMode()
        {
            using (var connection = new DisposableConnection(ConnectionOptions.Create(TestHelper.ConnectionString("?strict=true"))))
            {
                Assert.AreEqual(true, connection.StrictMode);
                connection.LoadOptions("strict=false");
                Assert.AreEqual(false, connection.StrictMode);
            }
        }
        [Test]
        public void OverridesQueryTimeout()
        {
            using (var connection = new DisposableConnection(ConnectionOptions.Create(TestHelper.ConnectionString("querytimeout=30"))))
            {
                Assert.AreEqual(30, connection.QueryTimeout);
                connection.LoadOptions("querytimeout=32");
                Assert.AreEqual(32, connection.QueryTimeout);
            }
        }
       

        [Test]
        public void ResetsDefaults()
        {
            using (var connection = new DisposableConnection(ConnectionOptions.Create(TestHelper.ConnectionString("querytimeout=30&strict=false"))))
            {
                connection.LoadOptions("querytimeout=23&strict=true");
                Assert.AreEqual(true, connection.StrictMode);
                Assert.AreEqual(23, connection.QueryTimeout);

                connection.ResetOptions();
                Assert.AreEqual(false, connection.StrictMode);
                Assert.AreEqual(30, connection.QueryTimeout);
            }
        }

        [Test]
        public void CreatesDigestFromNonce()
        {
            using (var connection = new DisposableConnection(ConnectionOptions.Create(TestHelper.ConnectionString("querytimeout=30&strict=false", "ussrr", "ppaassss"))))
            {
                Assert.AreEqual("21069b52452d123b3f4885400c1c9581", connection.Digest("1234abc"));
            }
        }

        private class DisposableConnection : Connection, IDisposable
        {
            private bool _disposed;

            internal DisposableConnection(ConnectionOptions builder) : base(builder) { }


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