using NUnit.Framework;

namespace Norm.Tests
{
    
    [TestFixture]
    public class PooledConnectionProviderAuthenticationTests : AuthenticatedFixture
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
        public void ThrowsExceptionIfConnectingWithInvalidCredentials()
        {
            var provider = new PooledConnectionProvider(ConnectionOptions.Create(AuthenticatedConnectionString("bad", "boy")));
            var ex = Assert.Throws<MongoException>(() => provider.Open(null));
            Assert.AreEqual("auth fails", ex.Message);
        }

        //won't pass on some 1.3.x builds of the server, but will pass against newest, or stable (1.2.3).
        //[Fact(Skip="Xunit misbehaves on this test, need to investigate")]
        public void AuthenticatesWithProperCredentials()
        {
            var provider = new PooledConnectionProvider(ConnectionOptions.Create(AuthenticatedConnectionString("usr", "pss")));
            var connection = provider.Open(null);
            Assert.AreEqual(true, connection.Client.Connected);
            provider.Close(connection);           
            
        }
    }
}