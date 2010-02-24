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
    }
        
    //todo: cleanup, timeout, age hanlding
    public class Connection : IConnection    
    {        
        public TcpClient Client{ get; private set;}
        private readonly ConnectionStringBuilder _builder;      
        
        public int QueryTimeout
        {
            get { return _builder.QueryTimeout; }
        }
        public bool EnableExpandoProperties
        {
            get { return _builder.EnableExpandoProperties; }
        }
        public bool StrictMode
        {
            get { return _builder.StrictMode;  }
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

    }
}