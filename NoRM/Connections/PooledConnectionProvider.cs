namespace NoRM
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    //todo: review (thrown together quickly)
    internal class PooledConnectionProvider : ConnectionProvider
    {        
        private readonly ConnectionStringBuilder _builder;        
        private readonly Queue<IConnection> _idlePool;
        private readonly Semaphore _tracker; //todo: semaphore a little heavy?
        private readonly int _timeout;
        
        public override ConnectionStringBuilder ConnectionString
        {
            get { return _builder; }
        }
                
        public PooledConnectionProvider(ConnectionStringBuilder builder)
        {
            _builder = builder;
            _idlePool = new Queue<IConnection>(builder.PoolSize);
            _tracker = new Semaphore(builder.PoolSize, builder.PoolSize);
            _timeout = builder.Timeout*1000;          
        }

        public override IConnection Open(string options)
        {
            if (!_tracker.WaitOne(_timeout))
            {
                throw new MongoException("Connection timeout trying to get connection from connection pool");
            }
            IConnection connection;
            try
            {
                connection = _idlePool.Dequeue();
            }
            catch (InvalidOperationException) //todo: disgusting!
            {
                connection = CreateNewConnection();                
            }
            if (!string.IsNullOrEmpty(options))
            {
                connection.LoadOptions(options);
            }
            return connection;
        }

        public override void Close(IConnection connection)
        {
            EnqueueIdle(connection);
        }
        private void EnqueueIdle(IConnection connection)
        {
            connection.ResetOptions();
            _idlePool.Enqueue(connection);
            _tracker.Release();
        }
    }
}