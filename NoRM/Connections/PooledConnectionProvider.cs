using System;
using System.Collections.Generic;
using System.Threading;

namespace NoRM
{
    internal class PooledConnectionProvider : ConnectionProvider
    {
        private readonly object _lock = new object();
        private readonly int _timeout;
        private readonly int _poolSize;
        private readonly int _lifetime;
        private readonly Timer _maintenanceTimer;
        private readonly ConnectionStringBuilder _builder;
        private readonly Queue<IConnection> _freeConnections = new Queue<IConnection>();
        private readonly List<IConnection> _usedConnections = new List<IConnection>();
        private readonly List<IConnection> _invalidConnections = new List<IConnection>();

        public override ConnectionStringBuilder ConnectionString
        {
            get { return _builder; }
        }

        public PooledConnectionProvider(ConnectionStringBuilder builder)
        {
            _builder = builder;
            _timeout = builder.Timeout * 1000;
            _poolSize = builder.PoolSize;
            _lifetime = builder.Lifetime;
            _maintenanceTimer = new Timer(o => Cleanup(), null, 30000, 30000);            
        }

        public override IConnection Open(string options)
        {
            IConnection connection;
            lock (_lock)
            {
                if (_freeConnections.Count > 0)
                {
                    connection = _freeConnections.Dequeue();                    
                    _usedConnections.Add(connection);
                    return connection;
                }

                if (_freeConnections.Count + _usedConnections.Count >= _poolSize)
                {
                    if (!Monitor.Wait(_lock, _timeout))
                    {
                        throw new MongoException("Connection timeout trying to get connection from connection pool");
                    }
                    return Open(options);
                }
            }

            connection = CreateNewConnection();
            lock (_lock)
            {
                _usedConnections.Add(connection);
            }
            connection.LoadOptions(options);
            return connection;
        }
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
                _freeConnections.Enqueue(connection);
                Monitor.Pulse(_lock);
            }
        }
        private bool IsAlive(IConnection connection)
        {
            if (_lifetime > 0 && connection.Created.AddMinutes(_lifetime) < DateTime.Now)
            {
                return false;
            }         
            return connection.IsConnected && !connection.IsInvalid;
        }

        public void Cleanup()
        {
            CheckFreeConnectionsAlive();
            DisposeInvalidConnections();
        }        
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
                        _freeConnections.Enqueue(freeConnection);
                    }
                    else
                    {
                        _invalidConnections.Add(freeConnection);
                    }
                }
            }
        }
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