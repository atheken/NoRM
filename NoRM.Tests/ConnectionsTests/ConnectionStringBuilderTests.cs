using System;
using NUnit.Framework;

namespace Norm.Tests
{
    [TestFixture]
    public class CreateTests
    {
        [Test]
        public void ThrowsExceptionWhenProtocolIsInvalid()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionOptions.Create("http://www.google.com"));
            Assert.AreEqual("Connection String must start with 'mongodb://','mongodbrs://', or be the name of a connection string in the app.config.", ex.Message);
        }

        [Test]
        public void Properly_Decodes_Username_and_Password_URLEncoded_Values()
        {
            var opts = ConnectionOptions.Create("mongodb://username:P%40$$w0rd@localhost/ProjectEuler");
            Assert.AreEqual("P@$$w0rd", opts.Password);
        }

        [Test]
        public void ThrowsExceptionIfAuthenticationDoesntHaveTwoParts()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionOptions.Create("mongodb://notgood@host/db"));
            Assert.AreEqual("Invalid connection string: authentication should be in the form of username:password", ex.Message);
        }
        [Test]
        public void ParsesAuthenticationInformation()
        {
            var builder = ConnectionOptions.Create("mongodb://user:pass@host/db");
            Assert.AreEqual("user", builder.UserName);
            Assert.AreEqual("pass", builder.Password);
        }
        [Test]
        public void DefaultAuthenticationWhenNotSpecified()
        {
            var builder = ConnectionOptions.Create("mongodb://host/db");
            Assert.AreEqual(null, builder.UserName);
            Assert.AreEqual(null, builder.Password);
        }

        [Test]
        public void DefaultDatabaseWhenNotSpecified()
        {
            var builder = ConnectionOptions.Create("mongodb://host");
            Assert.AreEqual("admin", builder.Database);
        }
        [Test]
        public void UsesSpecifiedDatabase()
        {
            var builder = ConnectionOptions.Create("mongodb://host/vegeta");
            Assert.AreEqual("vegeta", builder.Database);
        }

        [Test]
        public void ThrowsExceptionWhenNoHostIsDefined()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionOptions.Create("mongodb://"));
            Assert.AreEqual("The connection string passed does not appear to be a valid Uri, it should be of the form: 'mongodb[rs]://[user:password]@host:[port]/[replicaSetName]/dbname?[options]' where the parts in brackets are optional.", ex.Message);
        }
        [Test]
        public void ThrowsExceptionForInvalidServerConfiguration()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionOptions.Create("mongodb://1:2:3"));
            Assert.AreEqual("The connection string passed does not appear to be a valid Uri, it should be of the form: 'mongodb[rs]://[user:password]@host:[port]/[replicaSetName]/dbname?[options]' where the parts in brackets are optional.", ex.Message);
        }
        [Test]
        public void ParsesSingleHost()
        {
            var builder = ConnectionOptions.Create("mongodb://host");
            Assert.AreEqual(1, builder.Servers.Count);
            Assert.AreEqual("host", builder.Servers[0].GetHost());
            Assert.AreEqual(27017, builder.Servers[0].GetPort());
        }
        [Test]
        public void ParsesSingleHostWithPort()
        {
            var builder = ConnectionOptions.Create("mongodb://host:123");
            Assert.AreEqual(1, builder.Servers.Count);
            Assert.AreEqual("host", builder.Servers[0].GetHost());
            Assert.AreEqual(123, builder.Servers[0].GetPort());
        }

        [Test]
        public void ThrowsExceptionForInvalidOption()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionOptions.Create("mongodb://localhost?optionWithNoValue"));
            Assert.AreEqual("Invalid connection option: optionWithNoValue", ex.Message);
        }
        [Test]
        public void ThrowsExceptionForInvalidStrictModeValue()
        {
            Assert.Throws<FormatException>(() => ConnectionOptions.Create("mongodb://localhost?strict=giggidy"));
        }
        [Test]
        public void ThrowsExceptionForInvalidQueryTimeoutValue()
        {
            Assert.Throws<FormatException>(() => ConnectionOptions.Create("mongodb://localhost?querytimeout=glenn"));
        }

        [Test]
        public void ThrowsExceptionForInvalidPoolingValue()
        {
            Assert.Throws<FormatException>(() => ConnectionOptions.Create("mongodb://localhost?pooling=okthx"));
        }
        [Test]
        public void ThrowsExceptionForInvalidPoolSizeValue()
        {
            Assert.Throws<FormatException>(() => ConnectionOptions.Create("mongodb://localhost?poolsize=fifteen"));
        }
        [Test]
        public void ThrowsExceptionForInvalidTimeoutValue()
        {
            Assert.Throws<FormatException>(() => ConnectionOptions.Create("mongodb://localhost?timeout=infinite"));
        }
        [Test]
        public void StrictModeIsOnByDefault()
        {
            Assert.AreEqual(true, ConnectionOptions.Create("mongodb://localhost").StrictMode);
        }
        [Test]
        public void ParsesStrictModeOption()
        {
            Assert.AreEqual(true, ConnectionOptions.Create("mongodb://localhost?strict=true").StrictMode);
            Assert.AreEqual(false, ConnectionOptions.Create("mongodb://localhost?strict=false").StrictMode);
        }

        [Test]
        public void QueryTimeoutIs30ByDefault()
        {
            Assert.AreEqual(30, ConnectionOptions.Create("mongodb://localhost").QueryTimeout);
        }
        [Test]
        public void ParsesQueryTimeoutOption()
        {
            Assert.AreEqual(15, ConnectionOptions.Create("mongodb://localhost?querytimeout=15").QueryTimeout);
        }
        [Test]
        public void PoolingIsOnByDefault()
        {
            Assert.AreEqual(true, ConnectionOptions.Create("mongodb://localhost").Pooled);
        }
        [Test]
        public void ParsesPoolingOption()
        {
            Assert.AreEqual(true, ConnectionOptions.Create("mongodb://localhost?pooling=true").Pooled);
            Assert.AreEqual(false, ConnectionOptions.Create("mongodb://localhost?pooling=false").Pooled);
        }
        [Test]
        public void PoolSizeIs25ByDefault()
        {
            Assert.AreEqual(25, ConnectionOptions.Create("mongodb://localhost").PoolSize);
        }
        [Test]
        public void ParsesPoolSizeOption()
        {
            Assert.AreEqual(24, ConnectionOptions.Create("mongodb://localhost?poolsize=24").PoolSize);
        }
        [Test]
        public void TimeoutIs30ByDefault()
        {
            Assert.AreEqual(30, ConnectionOptions.Create("mongodb://localhost").Timeout);
        }
        [Test]
        public void ParsesTimeoutOption()
        {
            Assert.AreEqual(14, ConnectionOptions.Create("mongodb://localhost?timeout=14").Timeout);
        }

        [Test]
        public void LifetimeIs15ByDefault()
        {
            Assert.AreEqual(15, ConnectionOptions.Create("mongodb://localhost").Lifetime);
        }

        [Test]
        public void ParsesLifetimeOption()
        {
            Assert.AreEqual(13, ConnectionOptions.Create("mongodb://localhost?lifetime=13").Lifetime);
        }

        [Test]
        public void ParsesComplexConnectionString()
        {
            var builder = ConnectionOptions.Create("mongodb://its:over@host:9001/dbz?strict=false&pooling=true&poolsize=100");
            Assert.AreEqual("host", builder.Servers[0].GetHost());
            Assert.AreEqual(9001, builder.Servers[0].GetPort());
            Assert.AreEqual("dbz", builder.Database);
            Assert.AreEqual("its", builder.UserName);
            Assert.AreEqual("over", builder.Password);
            Assert.AreEqual(false, builder.StrictMode);
            Assert.AreEqual(true, builder.Pooled);
            Assert.AreEqual(100, builder.PoolSize);
        }

    }
}