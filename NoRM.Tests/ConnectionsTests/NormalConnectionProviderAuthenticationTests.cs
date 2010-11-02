using NUnit.Framework;

namespace Norm.Tests
{
    [TestFixture]
    public class NormalConnectionProviderAuthenticationTests : AuthenticatedFixture
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

        //[Test(Skip="authenticated connection seems to be hanging when we run this in sequence")]
        public void ThrowsExceptionIfConnectingWithInvalidCredentials()
        {
            var provider = new NormalConnectionProvider(ConnectionOptions.Create(AuthenticatedConnectionString("bad", "boy")));

            var ex = Assert.Throws<MongoException>(() => provider.Open(null));
            Assert.AreEqual("auth fails", ex.Message);
        }

        //won't pass on some 1.3.x builds of the server, but will pass against newest, or stable (1.2.3).
        [Test]
        public void AuthenticatesWithProperCredentials()
        {
            var provider = new NormalConnectionProvider(ConnectionOptions.Create(AuthenticatedConnectionString("usr", "pss")));
            var connection = provider.Open(null);
            Assert.AreEqual(true, connection.Client.Connected);
            provider.Close(connection);
        }

        
    }
}