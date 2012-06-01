using NUnit.Framework;

namespace Norm.Tests
{
    [TestFixture]
    public class ConnectionProviderFactoryTests
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
        public void ReturnsAPooledConnectionProvider()
        {
            Assert.True(ConnectionProviderFactory.Create("mongodb://localhost/test?pooling=true") is PooledConnectionProvider);
        }
        [Test]
        public void ReturnsNormalConnectionProvider()
        {
            Assert.True(ConnectionProviderFactory.Create("mongodb://localhost/test?pooling=false") is NormalConnectionProvider);
        }
        
        [Test]
        public void ReturnsTheSameProviderForTheSameConnectionString()
        {
            const string connectionString = "mongodb://localhost/test?pooling=false";
            var original = ConnectionProviderFactory.Create(connectionString);
            Assert.AreSame(original, ConnectionProviderFactory.Create(connectionString));
        }
        [Test]
        public void ReturnsDifferentProvidersForDifferentConnectionStrings()
        {            
            var original = ConnectionProviderFactory.Create("mongodb://localhost/test?pooling=false");
            Assert.AreNotSame(original, ConnectionProviderFactory.Create("mongodb://localhost/test?pooling=false&strict=false"));
        }
        [Test]
        public void ConnectionProviderSupportsConfigFileValues()
        {
            var provider = ConnectionProviderFactory.Create("NormTests");
            Assert.NotNull(provider);
        }
        [Test]
        public void ConnectionProviderConfigFileValuesMatchConnectionStringGrammar()
        {
            Assert.Throws<MongoException>(() => ConnectionProviderFactory.Create("NormTestsFail"));
        }
        [Test]
        public void ConnectionProviderConfigFailsForMissingConnectionString()
        {
            Assert.Throws<MongoException>(() =>  ConnectionProviderFactory.Create("NormTestsFail") );
        }
    }
}