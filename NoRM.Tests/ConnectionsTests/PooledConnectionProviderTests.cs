using System.Threading;
using System;
using NUnit.Framework;


namespace Norm.Tests
{
    [TestFixture]
    public class PooledConnectionProviderTests
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
        public void ClosingAConnectionReturnsItToThePool()
        {
            var provider = new PooledConnectionProvider(ConnectionOptions.Create(TestHelper.ConnectionString("pooling=true&poolsize=1")));
            var connection1 = provider.Open(null);
            provider.Close(connection1);
            Assert.AreSame(connection1, provider.Open(null));
        }

        [Test]
        public void ThrowsExceptionIfNoConnectionsAvailable()
        {
            var provider = new PooledConnectionProvider(ConnectionOptions.Create(TestHelper.ConnectionString("pooling=true&poolsize=1&timeout=1")));
            provider.Open(null);

            var ex = Assert.Throws<MongoException>(() => provider.Open(null));
            Assert.AreEqual("Connection timeout trying to get connection from connection pool", ex.Message);
        }

        //[Fact(Skip = "This test seems to be causing NUnit.Framework to hang, will return.")]
        public void WaitsUntilTimeoutForConnectionToFreeUpAndThrowsExceptionIfNot()
        {
            var provider = new PooledConnectionProvider(ConnectionOptions.Create(TestHelper.ConnectionString("pooling=true&poolsize=1&timeout=3")));
            provider.Open(null);

            var start = DateTime.Now; //this doesn't seem like a very good way to do this..we'll see
            Assert.Throws<MongoException>(() => provider.Open(null));
            var elasped = DateTime.Now.Subtract(start).TotalSeconds;
            Assert.True(elasped > 3);
            Assert.True(elasped < 4);
        }
        [Test]
        public void WaitsUntilTimeoutForConnectionToFreeUp()
        {
            var provider = new PooledConnectionProvider(ConnectionOptions.Create(TestHelper.ConnectionString("pooling=true&poolsize=1&timeout=3")));
            var connection = provider.Open(null);
            new Timer(c => provider.Close(connection), null, 2000, 0);
            Assert.AreSame(connection, provider.Open(null));
        }

        [Test]
        public void ReturnsDifferentConnections()
        {
            var provider = new PooledConnectionProvider(ConnectionOptions.Create(TestHelper.ConnectionString("pooling=true&poolsize=2")));
            Assert.AreNotSame(provider.Open(null), provider.Open(null));
        }

        [Test]
        public void PoolsUpToPoolSizeConections()
        {
            var provider = new PooledConnectionProvider(ConnectionOptions.Create(TestHelper.ConnectionString("pooling=true&poolsize=4&timeout=1")));
            provider.Open(null);
            provider.Open(null);
            provider.Open(null);
            provider.Open(null);
            Assert.Throws<MongoException>(() => provider.Open(null));
        }
    }
}
