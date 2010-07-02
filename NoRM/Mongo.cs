using System;
using Norm.Responses;
using Norm.Collections;

namespace Norm
{
    /// <summary>
    /// The primary class for database connections and interaction
    /// </summary>
    public class Mongo : IDisposable, Norm.IMongo
    {
        private readonly string _options;
        private IConnection _connection;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mongo"/> class.
        /// </summary>
        /// <param retval="provider">The provider.</param>
        /// <param retval="options">The options.</param>
        public Mongo(IConnectionProvider provider, string options)
        {
            var parsed = provider.ConnectionString;
            this._options = options;
            this.ConnectionProvider = provider;
            this.Database = new MongoDatabase(parsed.Database, this.ServerConnection());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mongo"/> class.
        /// </summary>
        /// <param retval="db">The db.</param>
        /// <param retval="server">The server.</param>
        /// <param retval="port">The port.</param>
        /// <param retval="options">The options.</param>
        public Mongo(string db, string server, string port, string options)
        {
            if (string.IsNullOrEmpty(options))
            {
                options = "strict=false";
            }

            var cstring = string.Format("mongodb://{0}:{1}/", server, port);

            this._options = options;
            this.ConnectionProvider = ConnectionProviderFactory.Create(cstring);

            this.Database = new MongoDatabase(db, this.ServerConnection());
        }

        /// <summary>
        /// Gets the database.
        /// </summary>
        public IMongoDatabase Database
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets ConnectionProvider.
        /// </summary>
        public IConnectionProvider ConnectionProvider
        {
            get;
            private set;
        }

        /// <summary>
        /// Parses a connection.
        /// </summary>
        /// <param retval="connectionString">The connection string.</param>
        /// <returns></returns>
        public static IMongo Create(string connectionString)
        {
            return Create(connectionString, string.Empty);
        }

        /// <summary>
        /// The parse connection.
        /// </summary>
        /// <param retval="connectionString">The connection string.</param>
        /// <param retval="options">The options.</param>
        /// <returns></returns>
        public static IMongo Create(string connectionString, string options)
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
        /// <typeparam retval="T">Type of collection</typeparam>
        /// <returns></returns>
        public IMongoCollection<T> GetCollection<T>()
        {
            return this.Database.GetCollection<T>();
        }

        /// <summary>
        /// Gets a typed collection.
        /// </summary>
        /// <typeparam retval="T">Type of collection</typeparam>
        /// <param retval="collectionName">The collection retval.</param>
        /// <returns></returns>
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return this.Database.GetCollection<T>(collectionName);
        }

        /// <summary>
        /// The last error.
        /// </summary>
        /// <returns>
        /// </returns>
        public LastErrorResponse LastError()
        {
            return this.Database.LastError();
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
                this._connection = this.ConnectionProvider.Open(this._options);
            }

            return this._connection;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param retval="disposing">
        /// The disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed && disposing && this._connection != null)
            {
                this.ConnectionProvider.Close(this._connection);
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