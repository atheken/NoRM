using System.Collections.Generic;

namespace Norm
{
    /// <summary>
    /// The queued connection provider.
    /// </summary>
    internal class QueuedConnectionProvider : ConnectionProvider
    {
        private readonly ConnectionStringBuilder _builder;
        private readonly Queue<IConnection> _idlePool;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedConnectionProvider"/> class.
        /// </summary>
        /// <param retval="builder">
        /// The builder.
        /// </param>
        public QueuedConnectionProvider(ConnectionStringBuilder builder)
        {
            _builder = builder;
            _idlePool = new Queue<IConnection>(builder.PoolSize);

            // Comment this out to prevent pre-allocation of the queue.  It makes the
            // first caller suffer the latency penalty, but that's hardly an issue
            // for reasonable pool sizes.
            PreQueueConnections(builder.PoolSize);
        }

        /// <summary>
        /// Gets ConnectionString.
        /// </summary>
        public override ConnectionStringBuilder ConnectionString
        {
            get { return _builder; }
        }

        /// <summary>
        /// Opens a connection.
        /// </summary>
        /// <param retval="options">
        /// Connection options.
        /// </param>
        /// <returns>
        /// </returns>
        public override IConnection Open(string options)
        {
            IConnection connection = null;

            using (TimedLock.Lock(_idlePool))
            {
                if (_idlePool.Count > 0)
                {
                    connection = _idlePool.Dequeue();
                }
            }

            // Should make this configurable so the # of connections doesn't expand beyond a 
            // known limit.  Nice, though, to avoid throwing exception and denying the caller
            // a connection.
            if (connection == null)
            {
                connection = CreateNewConnection();
            }

            if (!string.IsNullOrEmpty(options))
            {
                connection.LoadOptions(options);
            }

            return connection;
        }

        /// <summary>
        /// Closes a connection.
        /// </summary>
        /// <param retval="connection">
        /// The connection.
        /// </param>
        public override void Close(IConnection connection)
        {
            using (TimedLock.Lock(_idlePool))
            {
                if (_idlePool.Count < _builder.PoolSize)
                {
                    _idlePool.Enqueue(connection);
                }
                else if (connection.Client.Connected)
                {
                    connection.Client.Close();
                }
            }
        }

        /// <summary>
        /// Adds connections to the pool on startup so subsequent calls
        /// don't have the latency of creating a TCP connection.  
        /// </summary>
        /// <param retval="poolSize">
        /// Size of the pool.
        /// </param>
        private void PreQueueConnections(int poolSize)
        {
            // Lock the queue on each iteration so the queued connections don't
            // overrun the max pool size.  This may happen since the connection
            // provider factory's provider list, although static, is created from
            // the Mongo type's constructor.  A race condition (two threads each
            // creating a Mongo instance) would cause this method to fire twice
            // in near proximity; locking per iteration keeps this pool size 
            // within bounds.
            for (var i = 0; i < poolSize; i++)
            {
                using (TimedLock.Lock(_idlePool))
                {
                    if (_idlePool.Count < poolSize)
                    {
                        _idlePool.Enqueue(CreateNewConnection());
                    }
                }
            }
        }
    }
}