using System;
using Xunit;

namespace Norm.Tests
{
    public class CreateTests
    {
        [Fact]
        public void ThrowsExceptionWhenProtocolIsInvalid()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionStringBuilder.Create("http://www.google.com"));
            Assert.Equal("Connection String must start with 'mongodb://' or be the name of a connection string in the app.config.", ex.Message);
        }

        [Fact]
        public void ThrowsExceptionIfAuthenticationDoesntHaveTwoParts()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionStringBuilder.Create("mongodb://notgood@host/db"));
            Assert.Equal("Invalid connection string: authentication should be in the form of username:password", ex.Message);
        }
        [Fact]
        public void ParsesAuthenticationInformation()
        {
            var builder = ConnectionStringBuilder.Create("mongodb://user:pass@host/db");
            Assert.Equal("user", builder.UserName);
            Assert.Equal("pass", builder.Password);
        }
        [Fact]
        public void DefaultAuthenticationWhenNotSpecified()
        {
            var builder = ConnectionStringBuilder.Create("mongodb://host/db");
            Assert.Equal(null, builder.UserName);
            Assert.Equal(null, builder.Password);
        }

        [Fact]
        public void DefaultDatabaseWhenNotSpecified()
        {
            var builder = ConnectionStringBuilder.Create("mongodb://host");
            Assert.Equal("admin", builder.Database);
        }
        [Fact]
        public void UsesSpecifiedDatabase()
        {
            var builder = ConnectionStringBuilder.Create("mongodb://host/vegeta");
            Assert.Equal("vegeta", builder.Database);
        }

        [Fact]
        public void ThrowsExceptionWhenNoHostIsDefined()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionStringBuilder.Create("mongodb://"));
            Assert.Equal("Invalid connection string: at least 1 server is required", ex.Message);
        }
        [Fact]
        public void ThrowsExceptionForInvalidServerConfiguration()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionStringBuilder.Create("mongodb://1:2:3"));
            Assert.Equal("Invalid connection string: 1:2:3 is not a valid server configuration", ex.Message);
        }
        [Fact]
        public void ParsesSingleHost()
        {
            var builder = ConnectionStringBuilder.Create("mongodb://host");
            Assert.Equal(1, builder.Servers.Count);
            Assert.Equal("host", builder.Servers[0].Host);
            Assert.Equal(27017, builder.Servers[0].Port);
        }
        [Fact]
        public void ParsesSingleHostWithPort()
        {
            var builder = ConnectionStringBuilder.Create("mongodb://host:123");
            Assert.Equal(1, builder.Servers.Count);
            Assert.Equal("host", builder.Servers[0].Host);
            Assert.Equal(123, builder.Servers[0].Port);
        }
        [Fact]
        public void ParsesMultipleHosts()
        {
            var builder = ConnectionStringBuilder.Create("mongodb://vegeta,freeza");
            Assert.Equal(2, builder.Servers.Count);
            Assert.Equal("vegeta", builder.Servers[0].Host);
            Assert.Equal(27017, builder.Servers[0].Port);
            Assert.Equal("freeza", builder.Servers[1].Host);
            Assert.Equal(27017, builder.Servers[1].Port);
        }
        [Fact]
        public void ParsesMultipleHostsWithPorts()
        {
            var builder = ConnectionStringBuilder.Create("mongodb://vegeta:8999,goku:9001");
            Assert.Equal(2, builder.Servers.Count);
            Assert.Equal("vegeta", builder.Servers[0].Host);
            Assert.Equal(8999, builder.Servers[0].Port);
            Assert.Equal("goku", builder.Servers[1].Host);
            Assert.Equal(9001, builder.Servers[1].Port);
        }
        
        [Fact]
        public void ThrowsExceptionForInvalidOption()
        {
            var ex = Assert.Throws<MongoException>(() => ConnectionStringBuilder.Create("mongodb://localhost?optionWithNoValue"));
            Assert.Equal("Invalid connection option: optionWithNoValue", ex.Message);
        }
        [Fact]
        public void ThrowsExceptionForInvalidStrictModeValue()
        {
            Assert.Throws<FormatException>(() => ConnectionStringBuilder.Create("mongodb://localhost?strict=giggidy"));
        }
        [Fact]
        public void ThrowsExceptionForInvalidQueryTimeoutValue()
        {
            Assert.Throws<FormatException>(() => ConnectionStringBuilder.Create("mongodb://localhost?querytimeout=glenn"));
        }
      
        [Fact]
        public void ThrowsExceptionForInvalidPoolingValue()
        {
            Assert.Throws<FormatException>(() => ConnectionStringBuilder.Create("mongodb://localhost?pooling=okthx"));
        }
        [Fact]
        public void ThrowsExceptionForInvalidPoolSizeValue()
        {
            Assert.Throws<FormatException>(() => ConnectionStringBuilder.Create("mongodb://localhost?poolsize=fifteen"));
        }
        [Fact]
        public void ThrowsExceptionForInvalidTimeoutValue()
        {
            Assert.Throws<FormatException>(() => ConnectionStringBuilder.Create("mongodb://localhost?timeout=infinite"));
        }
        [Fact]
        public void StrictModeIsOnByDefault()
        {
            Assert.Equal(true, ConnectionStringBuilder.Create("mongodb://localhost").StrictMode);
        }
        [Fact]
        public void ParsesStrictModeOption()
        {
            Assert.Equal(true, ConnectionStringBuilder.Create("mongodb://localhost?strict=true").StrictMode);
            Assert.Equal(false, ConnectionStringBuilder.Create("mongodb://localhost?strict=false").StrictMode);
        }
       
        [Fact]
        public void QueryTimeoutIs30ByDefault()
        {
            Assert.Equal(30, ConnectionStringBuilder.Create("mongodb://localhost").QueryTimeout);
        }
        [Fact]
        public void ParsesQueryTimeoutOption()
        {
            Assert.Equal(15, ConnectionStringBuilder.Create("mongodb://localhost?querytimeout=15").QueryTimeout);            
        }
        [Fact]
        public void PoolingIsOnByDefault()
        {
            Assert.Equal(true, ConnectionStringBuilder.Create("mongodb://localhost").Pooled);
        }
        [Fact]
        public void ParsesPoolingOption()
        {
            Assert.Equal(true, ConnectionStringBuilder.Create("mongodb://localhost?pooling=true").Pooled);
            Assert.Equal(false, ConnectionStringBuilder.Create("mongodb://localhost?pooling=false").Pooled);
        }
        [Fact]
        public void PoolSizeIs25ByDefault()
        {
            Assert.Equal(25, ConnectionStringBuilder.Create("mongodb://localhost").PoolSize);
        }
        [Fact]
        public void ParsesPoolSizeOption()
        {
            Assert.Equal(24, ConnectionStringBuilder.Create("mongodb://localhost?poolsize=24").PoolSize);
        }
        [Fact]
        public void TimeoutIs30ByDefault()
        {
            Assert.Equal(30, ConnectionStringBuilder.Create("mongodb://localhost").Timeout);
        }
        [Fact]
        public void ParsesTimeoutOption()
        {
            Assert.Equal(14, ConnectionStringBuilder.Create("mongodb://localhost?timeout=14").Timeout);
        }
        [Fact]
        public void LifetimeIs15ByDefault()
        {
            Assert.Equal(15, ConnectionStringBuilder.Create("mongodb://localhost").Lifetime);
        }
        [Fact]
        public void ParsesLifetimeOption()
        {
            Assert.Equal(13, ConnectionStringBuilder.Create("mongodb://localhost?lifetime=13").Lifetime);
        }
                
        [Fact]
        public void ParsesComplexConnectionString()
        {
            var builder = ConnectionStringBuilder.Create("mongodb://its:over@host,goku:9001/dbz?strict=false&pooling=true&poolsize=100");
            Assert.Equal(2, builder.Servers.Count);
            Assert.Equal("host", builder.Servers[0].Host);
            Assert.Equal(27017, builder.Servers[0].Port);
            Assert.Equal("goku", builder.Servers[1].Host);
            Assert.Equal(9001, builder.Servers[1].Port);
            Assert.Equal("dbz", builder.Database);
            Assert.Equal("its", builder.UserName);
            Assert.Equal("over", builder.Password);
            Assert.Equal(false, builder.StrictMode);
            Assert.Equal(true, builder.Pooled);
            Assert.Equal(100, builder.PoolSize);
        }
    }
}