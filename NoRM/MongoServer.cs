using System;

namespace NoRM
{
    public class MongoServer : IDisposable
    {
        private readonly IConnectionProvider _connectionProvider;
        private readonly MongoDatabase _database;
        private bool _disposed;
        private IConnection _connection;
        
        public MongoDatabase Database
        {
            get { return _database; }
        }
                
        /// <summary>
        /// Creates a context that will connect to 127.0.0.1:27017 (MongoDB on the default port).
        /// </summary>        
        public MongoServer() : this("mongodb://127.0.0.1:27017"){}

        /// <summary>
        /// Specify the host and the port to connect to for the mongo db.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the server</param>        
        public MongoServer(string connectionString)
        {            
            _connectionProvider = ConnectionProviderFactory.Create(connectionString);
            var parsed = _connectionProvider.ConnectionString;
            _database = new MongoDatabase(parsed.Database, ServerConnection());
        }
        /// <summary>
        /// Constructs a socket to the server.
        /// </summary>
        /// <returns></returns>
        internal IConnection ServerConnection()
        {
            if (_connection == null)
            {
                _connection = _connectionProvider.Open();
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
        ~MongoServer()
        {
            Dispose(false);
        }
    }
}
