using System;

namespace NoRM
{
    using Protocol.SystemMessages.Responses;
    using System.Reflection;
    using System.Configuration;

    public class Mongo : IDisposable
    {
        private readonly IConnectionProvider _connectionProvider;
        private bool _disposed;
        private IConnection _connection;

        private readonly MongoDatabase _database;
        private readonly string _options;

        public MongoDatabase Database
        {
            get { return _database; }
        }

        public IConnectionProvider ConnectionProvider {
            get {
                return _connectionProvider;
            }
        }
        public static Mongo ParseConnection(string connectionString) {
            return ParseConnection(connectionString, "");
        }
        public static Mongo ParseConnection(string connectionString, string options) {

            return new Mongo(ConnectionProviderFactory.Create(connectionString),options);

        }
        public Mongo(IConnectionProvider provider, string options) {
            var parsed = provider.ConnectionString;
            var db = new MongoDatabase(parsed.Database, provider.Open(options));
            _options = options;
            _connectionProvider = provider;
            _database = new MongoDatabase(parsed.Database, ServerConnection());            
        }
        public Mongo() : this("", "127.0.0.1", "27017", "") { }
        public Mongo(string db) : this(db,"127.0.0.1","27017", "") { }
        public Mongo(string db, string server) : this(db,server,"27017","") { }
        public Mongo(string db, string server, string port) : this(db,server,port, "") { }
        public Mongo(string db, string server, string port, string options) {

            if (string.IsNullOrEmpty(options)) {
                options = "strict=false";
            }
            var cstring = string.Format("mongodb://{0}:{1}", server, port); ;
            _options = options;
            _connectionProvider = ConnectionProviderFactory.Create(cstring);
            
            _database = new MongoDatabase(db, ServerConnection());

        }

        internal IConnection ServerConnection()
        {
            if (_connection == null)
            {
                _connection = _connectionProvider.Open(_options);
            }
            return _connection;
        }

        public MongoCollection<T> GetCollection<T>() where T : class, new()
        {
            return _database.GetCollection<T>();
        }

        public MongoCollection<T> GetCollection<T>(string collectionName) where T : class, new()
        {
            return _database.GetCollection<T>(collectionName);
        }
        public MapReduce<T> MapReduce<T>() where T : class, new()
        {
            return new MapReduce<T>(_database);
        }
       
        public LastErrorResponse LastError()
        {
            return _database.GetCollection<LastErrorResponse>("$cmd").FindOne(new { getlasterror = 1 });
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
