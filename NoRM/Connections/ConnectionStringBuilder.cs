namespace NoRM
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class ConnectionStringBuilder
    {
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
        public int QueryTimeout { get { return 30; } }//todo make configurable
        public bool EnableExpandoProperties { get { return false; } }//todo make configurable

        private ConnectionStringBuilder(){}
        
        public static ConnectionStringBuilder Create(string connectionString)
        {
            if (!connectionString.StartsWith(PROTOCOL, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new MongoException("Invalid connection string: the protocol must be mongodb://");
            }
            var sb = new StringBuilder(connectionString.Substring(PROTOCOL.Length));
            var builder = new ConnectionStringBuilder();
            builder.BuildAuthentication(sb)
                   .BuildDatabase(sb)
                   .BuildServerList(sb);
            return builder;            
        }

        private ConnectionStringBuilder BuildAuthentication(StringBuilder sb)
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
        }
        
        public class Server
        {
            public string Host{get; set;}
            public int Port{get; set;}
        }
    }
}