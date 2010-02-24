using System;

namespace NoRM
{
    public class Mongo : IDisposable
    {
        private readonly IConnectionProvider _connectionProvider;
        private readonly MongoDatabase _database;
        private bool _disposed;
        private IConnection _connection;
        private string _options;
        
        public MongoDatabase Database
        {
            get { return _database; }
        }

        public Mongo() : this("mongodb://127.0.0.1:27017") { }        
        public Mongo(string connectionString) : this(connectionString, null){}
        public Mongo(string connectionString, string options)
        {
            _options = options;
            _connectionProvider = ConnectionProviderFactory.Create(connectionString);
            var parsed = _connectionProvider.ConnectionString;
            _database = new MongoDatabase(parsed.Database, ServerConnection());            
        }
        internal IConnection ServerConnection()
        {
            if (_connection == null)
            {
                _connection = _connectionProvider.Open(_options);
            }
            return _connection;
        }   

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing && _connection != null)
            {
                _connectionProvider.Close(_connection);
            }
            _disposed = true;
        }
        ~Mongo()
        {
            Dispose(false);
        }
    }
}
