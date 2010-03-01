namespace NoRM.Linq
{
    using System;

    public abstract class MongoSession : IDisposable
    {
        private bool _disposed;
        private readonly MongoQueryProvider _provider;
        protected MongoQueryProvider Provider
        {
            get { return _provider; }
        }

        protected MongoSession(string connectionString)
        {
            _provider = new MongoQueryProvider(connectionString);
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _provider.Dispose();
            }
            _disposed = true;
        }

        ~MongoSession()
        {
            Dispose(false);
        }

    }
}