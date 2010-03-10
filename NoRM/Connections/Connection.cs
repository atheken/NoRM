using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace NoRM
{

    // todo: cleanup, timeout, age hanlding

    /// <summary>
    /// The connection.
    /// </summary>
    public class Connection : IConnection, IOptionsContainer
    {
        private readonly ConnectionStringBuilder _builder;
        private readonly TcpClient _client;
        private bool _disposed;
        private bool? _enableExpandoProperties;
        private int? _queryTimeout;
        private bool? _strictMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="Connection"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        internal Connection(ConnectionStringBuilder builder)
        {
            _builder = builder;
            Created = DateTime.Now;
            _client = new TcpClient
            {
                NoDelay = true,
                ReceiveTimeout = builder.QueryTimeout * 1000,
                SendTimeout = builder.QueryTimeout * 1000
            };
            _client.Connect(builder.Servers[0].Host, builder.Servers[0].Port);
        }
        /// <summary>
        /// Gets the tcp client.
        /// </summary>
        /// <value></value>
        public TcpClient Client
        {
            get { return _client; }
        }

        /// <summary>
        /// Gets a value indicating whether the client is connected.
        /// </summary>
        /// <value></value>
        public bool IsConnected
        {
            get { return Client.Connected; }
        }
     
        /// <summary>
        /// Gets a value indicating whether the connection is invalid.
        /// </summary>
        /// <value></value>
        public bool IsInvalid { get; private set; }

      
        /// <summary>
        /// Gets the connection created date.
        /// </summary>
        /// <value></value>
        public DateTime Created { get; private set; }

        /// <summary>
        /// Gets the query timeout.
        /// </summary>
        /// <value></value>
        public int QueryTimeout
        {
            get { return _queryTimeout ?? _builder.QueryTimeout; }
        }

     
        /// <summary>
        /// Gets a value indicating whether to enable ExpandoProperties.
        /// </summary>
        /// <value></value>
        public bool EnableExpandoProperties
        {
            get { return _enableExpandoProperties ?? _builder.EnableExpandoProperties; }
        }

        /// <summary>
        /// Gets a value indicating whether to use strict mode.
        /// </summary>
        /// <value></value>
        public bool StrictMode
        {
            get { return _strictMode ?? _builder.StrictMode; }
        }
  
        /// <summary>
        /// Gets the user name.
        /// </summary>
        /// <value></value>
        public string UserName
        {
            get { return _builder.UserName; }
        }
    
        /// <summary>
        /// Gets the database name.
        /// </summary>
        /// <value></value>
        public string Database
        {
            get { return _builder.Database; }
        }

        /// <summary>
        /// Digest the nonce.
        /// </summary>
        /// <param name="nonce">The nonce.</param>
        /// <returns>The digest.</returns>
        public string Digest(string nonce)
        {
            using (var md5 = MD5.Create())
            {
                var rawDigest = Encoding.UTF8.GetBytes(string.Concat(nonce, UserName, _builder.Password));
                var hashed = md5.ComputeHash(rawDigest);
                var sb = new StringBuilder(hashed.Length * 2);
                Array.ForEach(hashed, b => sb.Append(b.ToString("X2")));
                return sb.ToString().ToLower();
            }
        }

        /// <summary>
        /// Gets a stream.
        /// </summary>
        /// <returns></returns>
        public NetworkStream GetStream()
        {
            return Client.GetStream();
        }

        /// <summary>
        /// Loads options.
        /// </summary>
        /// <param name="options">The options.</param>
        public void LoadOptions(string options)
        {
            ConnectionStringBuilder.BuildOptions(this, options);
        }

      
        /// <summary>
        /// Resets options.
        /// </summary>
        public void ResetOptions()
        {
            _queryTimeout = null;
            _enableExpandoProperties = null;
            _strictMode = null;
        }

        /// <summary>
        /// Writes an object.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="start">The start.</param>
        /// <param name="size">The size.</param>
        public void Write(byte[] bytes, int start, int size)
        {
            try
            {
                GetStream().Write(bytes, 0, size);
            }
            catch (IOException)
            {
                IsInvalid = true;
                throw;
            }
        }
    
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// The set query timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public void SetQueryTimeout(int timeout)
        {
            _queryTimeout = timeout;
        }

        /// <summary>
        /// The set enable expando properties.
        /// </summary>
        /// <param name="enabled">The enabled.</param>
        public void SetEnableExpandoProperties(bool enabled)
        {
            _enableExpandoProperties = enabled;
        }

        /// <summary>
        /// The set strict mode.
        /// </summary>
        /// <param name="strict">The strict.</param>
        public void SetStrictMode(bool strict)
        {
            _strictMode = strict;
        }

        /// <summary>
        /// The set pool size.
        /// </summary>
        /// <param name="size">The size.</param>
        public void SetPoolSize(int size)
        {
            throw new MongoException("PoolSize cannot be provided as an override option");
        }

        /// <summary>
        /// The set pooled flag.
        /// </summary>
        /// <param name="pooled">The pooled.</param>
        public void SetPooled(bool pooled)
        {
            throw new MongoException("Connection pooling cannot be provided as an override option");
        }

        /// <summary>
        /// The set timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public void SetTimeout(int timeout)
        {
            throw new MongoException("Timeout cannot be provided as an override option");
        }

        /// <summary>
        /// The set lifetime.
        /// </summary>
        /// <param name="lifetime">The lifetime.</param>
        public void SetLifetime(int lifetime)
        {
            throw new MongoException("Lifetime cannot be provided as an override option");
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _client.Close();
            _disposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Connection"/> class. 
        /// </summary>
        ~Connection()
        {
            Dispose(false);
        }
    }
}
