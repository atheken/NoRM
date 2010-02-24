namespace NoRM
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class ConnectionStringBuilder : IOptionsContainer
    {
        private static readonly IDictionary<string, Action<string, IOptionsContainer>> _optionsHandler = new Dictionary<string, Action<string, IOptionsContainer>>
              {
                  {"strict", (v, b) => b.SetStrictMode(bool.Parse(v))},
                  {"querytimeout", (v, b) => b.SetQueryTimeout(int.Parse(v))},
                  {"expando", (v, b) => b.SetEnableExpandoProperties(bool.Parse(v))},
                  {"pooling", (v, b) => b.SetPooled(bool.Parse(v))},
                  {"poolsize", (v, b) => b.SetPoolSize(int.Parse(v))},
              };            
        private const int DEFAULT_PORT = 27017;
        private const string DEFAULT_DATABASE = "admin";
        private const string PROTOCOL = "mongodb://";
        
        private IList<Server> _servers;
        public IList<Server> Servers
        {
            get { return _servers; }
        }        
        public string UserName{ get; private set;}        
        public string Password{ get; private set; }        
        public string Database{ get; private set; }
        public int QueryTimeout { get; private set; }
        public bool EnableExpandoProperties { get; private set; }
        public bool StrictMode{ get; private set; }    
        public bool Pooled{ get; private set;}   
        public int PoolSize{ get; private set;}
        
        private string _coreConnectionString;
        
        private ConnectionStringBuilder(){}
        
        public static ConnectionStringBuilder Create(string connectionString)
        {
            if (!connectionString.StartsWith(PROTOCOL, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new MongoException("Invalid connection string: the protocol must be mongodb://");
            }
            var parts = connectionString.Split(new[]{'?'}, StringSplitOptions.RemoveEmptyEntries);
            var options = parts.Length == 2 ? parts[1] : null;
            var sb = new StringBuilder(parts[0].Substring(PROTOCOL.Length));
            var builder = new ConnectionStringBuilder
                              {
                                  QueryTimeout = 30,
                                  EnableExpandoProperties = false,
                                  StrictMode = true,
                                  Pooled =  true,
                                  PoolSize = 25,
                              };
            var coreBuilder = new StringBuilder();
            builder.BuildAuthentication(sb, coreBuilder)
                .BuildDatabase(sb)
                .BuildServerList(sb);

            BuildOptions(builder, options);

            builder._coreConnectionString = builder.BuildCoreConnectionString();                         
            return builder;            
        }

        private ConnectionStringBuilder BuildAuthentication(StringBuilder sb, StringBuilder coreBuilder)
        {
            var connectionString = sb.ToString();
            var separator = connectionString.IndexOf('@');
            if (separator == -1) { return this; }
            var parts = connectionString.Substring(0, separator).Split(':');
            if (parts.Length != 2)
            {
                throw new MongoException("Invalid connection string: authentication should be in the form of username:password");
            }
            UserName = parts[0];
            Password = parts[1];            
            sb.Remove(0, separator+1);
            return this;           
        }
        private ConnectionStringBuilder BuildDatabase(StringBuilder sb)
        {
            var connectionString = sb.ToString();
            var separator = connectionString.IndexOf('/');
            if (separator == -1)
            {
                Database = DEFAULT_DATABASE;
            }
            else
            {
                Database = connectionString.Substring(separator + 1);
                sb.Remove(separator, sb.Length - separator);
            }
            return this;
        }
        private void BuildServerList(StringBuilder sb)
        {
            var connectionString = sb.ToString();
            var servers = connectionString.Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries);
            if (servers.Length == 0) { throw new MongoException("Invalid connection string: at least 1 server is required"); }
            
            var list = new List<Server>(servers.Length);
            foreach(var server in servers)
            {
                var parts = server.Split(new[]{':'}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 2) { throw new MongoException(string.Format("Invalid connection string: {0} is not a valid server configuration", server));}
                
                list.Add(new Server
                             {
                                 Host = parts[0], 
                                 Port = parts.Length == 2 ? int.Parse(parts[1]) : DEFAULT_PORT
                             });
            }
            _servers = list.AsReadOnly();
            return;
        }
        
        internal static void BuildOptions(IOptionsContainer container, string options)
        {
            if (string.IsNullOrEmpty(options)) { return; }
            //don't like how I did this
            var parts = options.Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries);
            foreach(var part in parts)
            {
                var kvp = part.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                if (kvp.Length != 2)
                {
                    throw new MongoException("Invalid connection option: " + part);
                }
                _optionsHandler[kvp[0].ToLower()](kvp[1], container);
            }
        }
        private string BuildCoreConnectionString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}/{1}/", UserName ?? " ", Password ?? " ");
            foreach(var server in Servers)
            {
                sb.AppendFormat("{0}/{1}/", server.Host, server.Port);
            }
            sb.AppendFormat("/{0}/{1}/{2}", Database, Pooled, PoolSize);
            return sb.ToString();
        }
        
        public class Server
        {
            public string Host{get; set;}
            public int Port{get; set;}
        }
        public void SetQueryTimeout(int timeout)
        {
            QueryTimeout = timeout;
        }
        public void SetEnableExpandoProperties(bool enabled)
        {
            EnableExpandoProperties = enabled;
        }
        public void SetStrictMode(bool strict)
        {
            StrictMode = strict;
        }
        public void SetPoolSize(int size)
        {
            PoolSize = size;
        }
        public void SetPooled(bool pooled)
        {
            Pooled = pooled;
        }

        //todo this and BuildCoreConnectionString really need test coverage
        public override bool Equals(object obj)
        {
            var left = obj as ConnectionStringBuilder;
            if (left == null) { return false; }
            return string.Compare(left._coreConnectionString, _coreConnectionString, true) == 0;
        }
        public override int GetHashCode()
        {
            return _coreConnectionString.GetHashCode();
        }
    }
}