using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Norm.Tests
{
    public class QueuedConnectionProviderTests
    {
        [Fact]
        public void ClosingAConnectionReturnsItToThePool()
        {
            var provider = new QueuedConnectionProvider(ConnectionStringBuilder.Create("mongodb://localhost?pooling=true&poolsize=1"));
            var connection1 = provider.Open(null);
            provider.Close(connection1);
            Assert.Same(connection1, provider.Open(null));
        }

        [Fact]
        public void NewConnectionsAreCreatedWhenQueueIsEmpty()
        {
            var provider = new QueuedConnectionProvider(ConnectionStringBuilder.Create("mongodb://localhost?pooling=true&poolsize=1&timeout=1"));
            var connection1 = provider.Open(null);

            var connection2 = provider.Open(null);

            Assert.NotNull(connection2);
            Assert.NotSame(connection1, connection2);
        }

        [Fact]
        public void ReturnsDifferentConnections()
        {
            var provider = new QueuedConnectionProvider(ConnectionStringBuilder.Create("mongodb://localhost?pooling=true&poolsize=2"));
            Assert.NotSame(provider.Open(null), provider.Open(null));
        }

        [Fact]
        public void PoolsUpToPoolSizeConections()
        {
            var provider = new QueuedConnectionProvider(ConnectionStringBuilder.Create("mongodb://localhost?pooling=true&poolsize=4"));
            provider.Open(null);
            provider.Open(null);
            provider.Open(null);
            provider.Open(null);

            // Creating a new connection instead of throwing an exception.  Not sure if it's a
            // good idea yet, but in theory it supports sudden surges in activity.
            Assert.DoesNotThrow(() => provider.Open(null));
        }

        [Fact]
        public void PoolReusesConnectionsUponConnectionClose()
        {
            var provider = new QueuedConnectionProvider(ConnectionStringBuilder.Create("mongodb://localhost?pooling=true&poolsize=2"));
            var connection1 = provider.Open(null);
            var conection1Hash = connection1.GetHashCode();
            provider.Close(connection1);
            
            var connection2 = provider.Open(null);
            var conection2Hash = connection2.GetHashCode();
            provider.Close(connection2);

            var connection3 = provider.Open(null);
            var conection3Hash = connection3.GetHashCode();

            Assert.NotEqual(conection1Hash, conection2Hash);
            Assert.Equal(conection1Hash, conection3Hash);
        }

        [Fact]
        public void PoolReusesQueuedConnections()
        {
            var provider = new QueuedConnectionProvider(ConnectionStringBuilder.Create("mongodb://localhost?pooling=true&poolsize=2"));
            var connection1 = provider.Open(null);
            var conection1Hash = connection1.GetHashCode();
            
            // Don't close connection 1 yet, now open and close connection 2
            var connection2 = provider.Open(null);
            var conection2Hash = connection2.GetHashCode();
            provider.Close(connection2);
            
            var connection3 = provider.Open(null);
            var conection3Hash = connection3.GetHashCode();

            // Connection 2 should have been re-queued and returned
            Assert.NotEqual(conection1Hash, conection2Hash);
            Assert.Equal(conection2Hash, conection3Hash);
        }

        [Fact]
        public void ThreadsSharePooledConnections()
        {
            // This test should probably test the Mongo class instead of testing the pool
            // provider directly!?
            var provider = new QueuedConnectionProvider(ConnectionStringBuilder.Create("mongodb://localhost?pooling=true&poolsize=3"));
            var idList = new List<int>();

            for(var i = 0; i < 10; i++)
            {
                ThreadPool.QueueUserWorkItem(w =>
                                                 {
                                                     var connection = provider.Open(null);
                                                     idList.Add(connection.GetHashCode());
                                                     provider.Close(connection);
                                                 });
            }

            // Simply wait - don't need to join monitor callbacks this simplistic unit test.
            System.Threading.Thread.Sleep(1000);
            Assert.Equal(10, idList.Count);

            // Count could be more than the pool size since new connections are created
            // for an empty pool, but it shold generally matchthe pool size in the
            // connection string.
            var connectionCount = idList.Distinct().Count();
            Assert.True(connectionCount < 10);
            Console.WriteLine("Threading produced {0} connections for 10 threads", connectionCount);
        }
    }
}
