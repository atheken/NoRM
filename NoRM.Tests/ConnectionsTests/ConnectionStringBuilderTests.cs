using System;
using Xunit;

namespace Norm.Tests
{
    public class CreateTests
    {
        [Fact]
        public void ThrowsExceptionWhenProtocolIsInvalid()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionOptions.Create("http://www.google.com"));
            Assert.Equal("Connection String must start with 'mongodb://','mongodbrs://', or be the name of a connection string in the app.config.", ex.Message);
        }

        [Fact]
        public void ThrowsExceptionIfAuthenticationDoesntHaveTwoParts()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionOptions.Create("mongodb://notgood@host/db"));
            Assert.Equal("Invalid connection string: authentication should be in the form of username:password", ex.Message);
        }
        [Fact]
        public void ParsesAuthenticationInformation()
        {
            var builder = ConnectionOptions.Create("mongodb://user:pass@host/db");
            Assert.Equal("user", builder.UserName);
            Assert.Equal("pass", builder.Password);
        }
        [Fact]
        public void DefaultAuthenticationWhenNotSpecified()
        {
            var builder = ConnectionOptions.Create("mongodb://host/db");
            Assert.Equal(null, builder.UserName);
            Assert.Equal(null, builder.Password);
        }

        [Fact]
        public void DefaultDatabaseWhenNotSpecified()
        {
            var builder = ConnectionOptions.Create("mongodb://host");
            Assert.Equal("admin", builder.Database);
        }
        [Fact]
        public void UsesSpecifiedDatabase()
        {
            var builder = ConnectionOptions.Create("mongodb://host/vegeta");
            Assert.Equal("vegeta", builder.Database);
        }

        [Fact]
        public void ThrowsExceptionWhenNoHostIsDefined()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionOptions.Create("mongodb://"));
            Assert.Equal("The connection string passed does not appear to be a valid Uri, it should be of the form: 'mongodb[rs]://[user:password]@host:[port]/[replicaSetName]/dbname?[options]' where the parts in brackets are optional.", ex.Message);
        }
        [Fact]
        public void ThrowsExceptionForInvalidServerConfiguration()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionOptions.Create("mongodb://1:2:3"));
            Assert.Equal("The connection string passed does not appear to be a valid Uri, it should be of the form: 'mongodb[rs]://[user:password]@host:[port]/[replicaSetName]/dbname?[options]' where the parts in brackets are optional.", ex.Message);
        }
        [Fact]
        public void ParsesSingleHost()
        {
            var builder = ConnectionOptions.Create("mongodb://host");
            Assert.Equal(1, builder.Servers.Count);
            Assert.Equal("host", builder.Servers[0].Host);
            Assert.Equal(27017, builder.Servers[0].Port);
        }
        [Fact]
        public void ParsesSingleHostWithPort()
        {
            var builder = ConnectionOptions.Create("mongodb://host:123");
            Assert.Equal(1, builder.Servers.Count);
            Assert.Equal("host", builder.Servers[0].Host);
            Assert.Equal(123, builder.Servers[0].Port);
        }
       
        [Fact]
        public void ThrowsExceptionForInvalidOption()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionOptions.Create("mongodb://localhost?optionWithNoValue"));
            Assert.Equal("Invalid connection option: optionWithNoValue", ex.Message);
        }
        [Fact]
        public void ThrowsExceptionForInvalidStrictModeValue()
        {
            Assert.Throws<FormatException>(() => ConnectionOptions.Create("mongodb://localhost?strict=giggidy"));
        }
        [Fact]
        public void ThrowsExceptionForInvalidQueryTimeoutValue()
        {
            Assert.Throws<FormatException>(() => ConnectionOptions.Create("mongodb://localhost?querytimeout=glenn"));
        }
      
        [Fact]
        public void ThrowsExceptionForInvalidPoolingValue()
        {
            Assert.Throws<FormatException>(() => ConnectionOptions.Create("mongodb://localhost?pooling=okthx"));
        }
        [Fact]
        public void ThrowsExceptionForInvalidPoolSizeValue()
        {
            Assert.Throws<FormatException>(() => ConnectionOptions.Create("mongodb://localhost?poolsize=fifteen"));
        }
        [Fact]
        public void ThrowsExceptionForInvalidTimeoutValue()
        {
            Assert.Throws<FormatException>(() => ConnectionOptions.Create("mongodb://localhost?timeout=infinite"));
        }
        [Fact]
        public void StrictModeIsOnByDefault()
        {
            Assert.Equal(true, ConnectionOptions.Create("mongodb://localhost").StrictMode);
        }
        [Fact]
        public void ParsesStrictModeOption()
        {
            Assert.Equal(true, ConnectionOptions.Create("mongodb://localhost?strict=true").StrictMode);
            Assert.Equal(false, ConnectionOptions.Create("mongodb://localhost?strict=false").StrictMode);
        }
       
        [Fact]
        public void QueryTimeoutIs30ByDefault()
        {
            Assert.Equal(30, ConnectionOptions.Create("mongodb://localhost").QueryTimeout);
        }
        [Fact]
        public void ParsesQueryTimeoutOption()
        {
            Assert.Equal(15, ConnectionOptions.Create("mongodb://localhost?querytimeout=15").QueryTimeout);            
        }
        [Fact]
        public void PoolingIsOnByDefault()
        {
            Assert.Equal(true, ConnectionOptions.Create("mongodb://localhost").Pooled);
        }
        [Fact]
        public void ParsesPoolingOption()
        {
            Assert.Equal(true, ConnectionOptions.Create("mongodb://localhost?pooling=true").Pooled);
            Assert.Equal(false, ConnectionOptions.Create("mongodb://localhost?pooling=false").Pooled);
        }
        [Fact]
        public void PoolSizeIs25ByDefault()
        {
            Assert.Equal(25, ConnectionOptions.Create("mongodb://localhost").PoolSize);
        }
        [Fact]
        public void ParsesPoolSizeOption()
        {
            Assert.Equal(24, ConnectionOptions.Create("mongodb://localhost?poolsize=24").PoolSize);
        }
        [Fact]
        public void TimeoutIs30ByDefault()
        {
            Assert.Equal(30, ConnectionOptions.Create("mongodb://localhost").Timeout);
        }
        [Fact]
        public void ParsesTimeoutOption()
        {
            Assert.Equal(14, ConnectionOptions.Create("mongodb://localhost?timeout=14").Timeout);
        }

        [Fact]
        public void LifetimeIs15ByDefault()
        {
            Assert.Equal(15, ConnectionOptions.Create("mongodb://localhost").Lifetime);
        }

        [Fact]
        public void ParsesLifetimeOption()
        {
            Assert.Equal(13, ConnectionOptions.Create("mongodb://localhost?lifetime=13").Lifetime);
        }
                
        [Fact]
        public void ParsesComplexConnectionString()
        {
            var builder = ConnectionOptions.Create("mongodb://its:over@host:9001/dbz?strict=false&pooling=true&poolsize=100");
            Assert.Equal("host", builder.Servers[0].Host);
            Assert.Equal(9001, builder.Servers[0].Port);
            Assert.Equal("dbz", builder.Database);
            Assert.Equal("its", builder.UserName);
            Assert.Equal("over", builder.Password);
            Assert.Equal(false, builder.StrictMode);
            Assert.Equal(true, builder.Pooled);
            Assert.Equal(100, builder.PoolSize);
        }

    }
}