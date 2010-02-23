namespace NoRM
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    //todo: review (thrown together quickly)
    internal class PooledConnectionProvider : ConnectionProvider
    {
        private const int MAXIMUM_POOL_SIZE = 5; //todo: make configurable via connection string
        private const int TIMEOUT = 15000; //todo: make configurable via connection string

        private readonly ConnectionStringBuilder _builder;
        private readonly Queue<IConnection> _idlePool;
        private readonly Semaphore _tracker; //todo: semaphore a little heavy?
        
        public override ConnectionStringBuilder ConnectionString
        {
            get { return _builder; }
        }
                
        public PooledConnectionProvider(ConnectionStringBuilder builder)
        {
            _builder = builder;
            _idlePool = new Queue<IConnection>(MAXIMUM_POOL_SIZE);
            _tracker = new Semaphore(0, MAXIMUM_POOL_SIZE);
            for(var i = 0; i < MAXIMUM_POOL_SIZE; ++i)
            {
                EnqueueIdle(CreateNewConnection());
            }            
        }

        public override IConnection Open()
        {
            if (!_tracker.WaitOne(TIMEOUT))
            {
                throw new TimeoutException();
            }
            return _idlePool.Dequeue();
        }

        public override void Close(IConnection connection)
        {
            EnqueueIdle(connection);
        }

        private IConnection CreateNewConnection()
        {
            var connection = new Connection(_builder);
            if (!Authenticate(connection))
            {
                throw new MongoException("Authentication Failed");
            }
            return connection;

        }
        private void EnqueueIdle(IConnection connection)
        {
            _idlePool.Enqueue(connection);
            _tracker.Release();
        }
    }
}