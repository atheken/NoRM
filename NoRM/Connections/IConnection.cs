namespace NoRM
{
    using System;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;

    public interface IConnection
    {
        TcpClient Client{ get;}
        NetworkStream GetStream();
        int QueryTimeout{ get;}       
        bool EnableExpandoProperties{ get;}
        bool StrictMode { get; }
        string UserName { get; }
        string Database { get; }
        string Digest(string nounce);
        
        void LoadOptions(string options);
        void ResetOptions();
    }
        
    //todo: cleanup, timeout, age hanlding
    public class Connection : IConnection, IOptionsContainer  
    {        
        public TcpClient Client{ get; private set;}
        private readonly ConnectionStringBuilder _builder;
        private int? _queryTimeout;
        private bool? _enableExpandoProperties;
        private bool? _strictMode;
        
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
            Client = new TcpClient();
            Client.Connect(builder.Servers[0].Host, builder.Servers[0].Port);            
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
    }
}