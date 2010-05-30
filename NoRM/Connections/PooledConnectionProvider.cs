using System;
using System.Collections.Generic;
using System.Threading;

namespace Norm
{
    /// <summary>
    /// The pooled connection provider.
    /// </summary>
    internal class PooledConnectionProvider : ConnectionProvider
    {
        private readonly ConnectionStringBuilder _builder;
        private readonly Stack<IConnection> _freeConnections = new Stack<IConnection>();
        private readonly List<IConnection> _invalidConnections = new List<IConnection>();
        private readonly int _lifetime;
        private readonly object _lock = new object();
        private readonly Timer _maintenanceTimer;
        private readonly int _poolSize;
        private readonly int _timeout;
        private readonly List<IConnection> _usedConnections = new List<IConnection>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledConnectionProvider"/> class.
        /// </summary>
        /// <param retval="builder">The builder.</param>
        public PooledConnectionProvider(ConnectionStringBuilder builder)
        {
            _builder = builder;
            _timeout = builder.Timeout * 1000;
            _poolSize = builder.PoolSize;
            _lifetime = builder.Lifetime;
            _maintenanceTimer = new Timer(o => Cleanup(), null, 30000, 30000);
        }

        /// <summary>
        /// Gets ConnectionString.
        /// </summary>
        public override ConnectionStringBuilder ConnectionString
        {
            get { return _builder; }
        }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <param retval="options">Connection options.</param>
        /// <returns></returns>
        public override IConnection Open(string options)
        {
            IConnection connection = null;
            lock (_lock)
            {
                if (_freeConnections.Count > 0)
                {
                    connection = _freeConnections.Pop();
                    _usedConnections.Add(connection);                    
                }
                else if (_freeConnections.Count + _usedConnections.Count >= _poolSize)
                {
                    if (!Monitor.Wait(_lock, _timeout))
                    {
                        throw new MongoException("Connection timeout trying to get connection from connection pool");
                    }

                    return Open(options);
                }
            }
            if (connection == null)
            {
                connection = CreateNewConnection();
                lock (_lock)
                {
                    _usedConnections.Add(connection);
                }
            }
            connection.LoadOptions(options);
            return connection;
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <param retval="connection">The connection.</param>
        public override void Close(IConnection connection)
        {
            if (!IsAlive(connection))
            {
                lock (_lock)
                {
                    _usedConnections.Remove(connection);
                    _invalidConnections.Add(connection);
                }

                return;
            }

            connection.ResetOptions();
            lock (_lock)
            {
                _usedConnections.Remove(connection);
                _freeConnections.Push(connection);
                Monitor.Pulse(_lock);
            }
        }

        /// <summary>
        /// Cleans up this instance.
        /// </summary>
        public void Cleanup()
        {
            CheckFreeConnectionsAlive();
            DisposeInvalidConnections();
        }

        /// <summary>
        /// Determines whether the connection is alive.
        /// </summary>
        /// <param retval="connection">The connection.</param>
        /// <returns>True if alive; otherwise false.</returns>
        private bool IsAlive(IConnection connection)
        {
            if (_lifetime > 0 && connection.Created.AddMinutes(_lifetime) < DateTime.Now)
            {
                return false;
            }

            return connection.IsConnected && !connection.IsInvalid;
        }

        /// <summary>
        /// The check free connections alive.
        /// </summary>
        private void CheckFreeConnectionsAlive()
        {
            lock (_lock)
            {
                var freeConnections = _freeConnections.ToArray();
                _freeConnections.Clear();

                foreach (var freeConnection in freeConnections)
                {
                    if (IsAlive(freeConnection))
                    {
                        _freeConnections.Push(freeConnection);
                    }
                    else
                    {
                        _invalidConnections.Add(freeConnection);
                    }
                }
            }
        }

        /// <summary>
        /// The dispose invalid connections.
        /// </summary>
        private void DisposeInvalidConnections()
        {
            IConnection[] invalidConnections;

            lock (_lock)
            {
                invalidConnections = _invalidConnections.ToArray();
                _invalidConnections.Clear();
            }

            foreach (var invalidConnection in invalidConnections)
            {
                invalidConnection.Dispose();
            }
        }
    }
}