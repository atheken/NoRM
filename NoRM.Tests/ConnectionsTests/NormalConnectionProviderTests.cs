using NUnit.Framework;

namespace Norm.Tests
{
    [TestFixture]
    public class NormalConnectionProviderTests
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
        public void CreatesANewConnectionForEachOpen()
        {
            IConnection connection1 = null;
            IConnection connection2 = null;
            var provider = new NormalConnectionProvider(ConnectionOptions.Create(TestHelper.ConnectionString()));
            
            try
            {            
                connection1 = provider.Open(null);
                connection2 = provider.Open(null);
                Assert.AreNotSame(connection1, connection2);
            }
            finally
            {
                if (connection1 != null) { provider.Close(connection1); }
                if (connection2 != null) { provider.Close(connection2); }                
            }
        }

        [Test]
        public void ClosesTheUnderlyingConnection()
        {
            var provider = new NormalConnectionProvider(ConnectionOptions.Create(TestHelper.ConnectionString()));
            var connection = provider.Open(null);
            provider.Close(connection);
            Assert.Null(connection.Client.Client);
        }
    }
}