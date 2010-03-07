using System.IO;

namespace NoRM
{
    using System;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;

    public interface IConnection : IDisposable
    {
        TcpClient Client{ get;}
        NetworkStream GetStream();
        bool IsConnected { get; }
        bool IsInvalid { get; }
        DateTime Created { get; }
        int QueryTimeout{ get;}       
        bool EnableExpandoProperties{ get;}
        bool StrictMode { get; }
        string UserName { get; }
        string Database { get; }
        string Digest(string nounce);
        
        void LoadOptions(string options);
        void ResetOptions();
        void Write(byte[] bytes, int start, int size);
    }
        
    //todo: cleanup, timeout, age hanlding
    public class Connection : IConnection, IOptionsContainer  
    {
        private bool _disposed;
        private readonly ConnectionStringBuilder _builder;
        private int? _queryTimeout;
        private bool? _enableExpandoProperties;
        private bool? _strictMode;

        private TcpClient _client;

        public TcpClient Client
        {
            get { return _client; }
        }
        public bool IsConnected
        {
            get { return Client.Connected; }
        }
        public bool IsInvalid{ get; private set;}        
        public DateTime Created{get; private set;}        
        public int QueryTimeout
        {
            get { return _queryTimeout ?? _builder.QueryTimeout; }
        }
        public bool EnableExpandoProperties
        {
            get { return _enableExpandoProperties ?? _builder.EnableExpandoProperties; }
        }
        public bool StrictMode
        {
            get { return _strictMode ?? _builder.StrictMode; }
        }
        public string UserName
        {
            get { return _builder.UserName; }
        }
        public string Database
        {
            get { return _builder.Database; }
        }

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
        
        internal Connection(ConnectionStringBuilder builder)
        {
            _builder = builder;
            Created = DateTime.Now;
            _client = new TcpClient 
            {
                NoDelay = true, 
                ReceiveTimeout = builder.QueryTimeout*1000, 
                SendTimeout = builder.QueryTimeout*1000
            };
            _client.Connect(builder.Servers[0].Host, builder.Servers[0].Port);            
        }

        public NetworkStream GetStream()
        {
            return Client.GetStream();
        }
        public void LoadOptions(string options)
        {
            ConnectionStringBuilder.BuildOptions(this, options);
        }
        public void ResetOptions()
        {
            _queryTimeout = null;
            _enableExpandoProperties = null;
            _strictMode = null;
        }

        public void Write(byte[] bytes, int start, int size)
        {
            try
            {
                GetStream().Write(bytes, 0, size);        
            }
            catch(IOException)
            {
                IsInvalid = true;
                throw;
            }            
        }

        public void SetQueryTimeout(int timeout)
        {
            _queryTimeout = timeout;
        }
        public void SetEnableExpandoProperties(bool enabled)
        {
            _enableExpandoProperties = enabled;
        }
        public void SetStrictMode(bool strict)
        {
            _strictMode = strict;
        }
        public void SetPoolSize(int size)
        {
            throw new MongoException("PoolSize cannot be provided as an override option");
        }
        public void SetPooled(bool pooled)
        {
            throw new MongoException("Connection pooling cannot be provided as an override option");
        }
        public void SetTimeout(int timeout)
        {
            throw new MongoException("Timeout cannot be provided as an override option");
        }
        public void SetLifetime(int lifetime)
        {
            throw new MongoException("Lifetime cannot be provided as an override option");
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _client.Close();
                _disposed = true;
            }            
        }
        ~Connection()
        {
            Dispose(false);
        }
    }
}