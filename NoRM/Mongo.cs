using System;
using NoRM.Responses;

namespace NoRM
{
    /// <summary>
    /// The primary class for database connections and interaction
    /// </summary>
    public class Mongo : IDisposable
    {
        private readonly IConnectionProvider _connectionProvider;
        private readonly MongoDatabase _database;
        private readonly string _options;
        private IConnection _connection;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mongo"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="options">The options.</param>
        public Mongo(IConnectionProvider provider, string options)
        {
            var parsed = provider.ConnectionString;
            this._options = options;
            this._connectionProvider = provider;
            this._database = new MongoDatabase(parsed.Database, this.ServerConnection());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mongo"/> class.
        /// </summary>
        public Mongo() : this(string.Empty, "127.0.0.1", "27017", string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mongo"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public Mongo(string db) : this(db, "127.0.0.1", "27017", string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mongo"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        /// <param name="server">
        /// The server.
        /// </param>
        public Mongo(string db, string server) : this(db, server, "27017", string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mongo"/> class.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="server">The server.</param>
        /// <param name="port">The port.</param>
        public Mongo(string db, string server, string port) : this(db, server, port, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mongo"/> class.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="server">The server.</param>
        /// <param name="port">The port.</param>
        /// <param name="options">The options.</param>
        public Mongo(string db, string server, string port, string options)
        {
            if (string.IsNullOrEmpty(options))
            {
                options = "strict=false";
            }

            var cstring = string.Format("mongodb://{0}:{1}/", server, port);

            this._options = options;
            this._connectionProvider = ConnectionProviderFactory.Create(cstring);

            this._database = new MongoDatabase(db, this.ServerConnection());
        }

        /// <summary>
        /// Gets the database.
        /// </summary>
        public MongoDatabase Database
        {
            get { return this._database; }
        }

        /// <summary>
        /// Gets ConnectionProvider.
        /// </summary>
        public IConnectionProvider ConnectionProvider
        {
            get { return this._connectionProvider; }
        }

        /// <summary>
        /// Parses a connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public static Mongo ParseConnection(string connectionString)
        {
            return ParseConnection(connectionString, string.Empty);
        }

        /// <summary>
        /// The parse connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static Mongo ParseConnection(string connectionString, string options)
        {
            return new Mongo(ConnectionProviderFactory.Create(connectionString), options);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets a typed collection.
        /// </summary>
        /// <typeparam name="T">Type of collection</typeparam>
        /// <returns></returns>
        public MongoCollection<T> GetCollection<T>()
        {
            return this._database.GetCollection<T>();
        }

        /// <summary>
        /// Gets a typed collection.
        /// </summary>
        /// <typeparam name="T">Type of collection</typeparam>
        /// <param name="collectionName">The collection name.</param>
        /// <returns></returns>
        public MongoCollection<T> GetCollection<T>(string collectionName)
        {
            return this._database.GetCollection<T>(collectionName);
        }

        /// <summary>
        /// The create map reduce.
        /// </summary>
        /// <returns>
        /// </returns>
        public MapReduce CreateMapReduce()
        {
            return new MapReduce(this._database);
        }

        /// <summary>
        /// The last error.
        /// </summary>
        /// <returns>
        /// </returns>
        public LastErrorResponse LastError()
        {
            return this._database.GetCollection<LastErrorResponse>("$cmd").FindOne(new { getlasterror = 1 });
        }

        /// <summary>
        /// The server connection.
        /// </summary>
        /// <returns>
        /// </returns>
        internal IConnection ServerConnection()
        {
            if (this._connection == null)
            {
                this._connection = this._connectionProvider.Open(this._options);
            }

            return this._connection;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed && disposing && this._connection != null)
            {
                this._connectionProvider.Close(this._connection);
            }

            this._disposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Mongo"/> class. 
        /// </summary>
        ~Mongo()
        {
            this.Dispose(false);
        }
    }
}