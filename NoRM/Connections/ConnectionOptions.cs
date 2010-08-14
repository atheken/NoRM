using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;

namespace Norm
{
    /// <summary>
    /// The set of connection options associated with a particular URI.
    /// </summary>
    public class ConnectionOptions : IOptionsContainer, ICloneable
    {
        private String _connectionString;

        /// <summary>
        /// Produces the original connection string (URI) that was used to get this instance of ConnectionOptions.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._connectionString;
        }

        private const string DEFAULT_DATABASE = "admin";
        private const int DEFAULT_PORT = 27017;
        private static readonly Regex _rxSchemaMatch = new Regex("^(mongodb(rs)?://)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _rxUserInfoMatch = new Regex("(?<username>.+?):(?<password>.+)", RegexOptions.Compiled);

        #region options assignment delegates
        private static readonly IDictionary<string, Action<string, ConnectionOptions>>
            _optionsHandler = new Dictionary<string, Action<string, ConnectionOptions>>
              {
                  {"strict", (v, b) => b.StrictMode =bool.Parse(v)},
                  {"querytimeout", (v, b) => b.QueryTimeout = int.Parse(v)},
                  {"pooling", (v, b) => b.Pooled =bool.Parse(v)},
                  {"poolsize", (v, b) => b.PoolSize = int.Parse(v)},
                  {"timeout", (v, b) => b.Timeout = int.Parse(v)},
                  {"lifetime", (v, b) => b.Lifetime = int.Parse(v)},
                  {"verifywritecount", (v,b)=> b.VerifyWriteCount = int.Parse(v)}
              };
        #endregion

        /// <summary>
        /// Use the "Create" method to get an instance of this class.
        /// </summary>
        private ConnectionOptions()
        {
            QueryTimeout = 30;
            Timeout = 30;
            StrictMode = true;
            Pooled = true;
            PoolSize = 25;
            Lifetime = 15;
        }

        /// <summary>
        /// Gets a server list.
        /// </summary>
        public IList<Server> Servers { get; set; }

        /// <summary>
        /// Gets the user retval.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets database retval.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets the query timeout.
        /// </summary>
        public int QueryTimeout { get; set; }


        /// <summary>
        /// Gets a value indicating whether strict mode is enabled.
        /// </summary>
        public bool StrictMode { get; set; }

        /// <summary>
        /// Gets a value indicating whether connections are pooled.
        /// </summary>
        public bool Pooled { get; set; }

        /// <summary>
        /// Gets the connection pool size.
        /// </summary>
        public int PoolSize { get; set; }

        /// <summary>
        /// Gets the connection timeout.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Gets the connection lifetime.
        /// </summary>
        public int Lifetime { get; set; }

        /// <summary>
        /// Get the write count required to be returned from the server when strict mode is enabled.
        /// </summary>
        public int? VerifyWriteCount { get; set; }

        /// <summary>
        /// Creates a connection string builder.
        /// </summary>
        /// <param retval="connectionString">The connection string.</param>
        /// <returns></returns>
        /// <exception cref="MongoException">
        /// </exception>
        public static ConnectionOptions Create(string connectionString)
        {

            var connection = connectionString;

            if (!_rxSchemaMatch.IsMatch(connection))
            {
                try
                {
                    connection = ConfigurationManager
                        .ConnectionStrings[connectionString]
                        .ConnectionString;
                }
                catch (NullReferenceException)
                {
                    throw new MongoException("Connection String must start with 'mongodb://','mongodbrs://', or be the name of a connection string in the app.config.");
                }
            }

            if (Uri.IsWellFormedUriString(connection, UriKind.Absolute))
            {
                var builder = new ConnectionOptions();
                var conn = new Uri(connection);
                builder._connectionString = connection;

                int port;
                try
                {
                    port = conn.Port;
                    port = port == -1 ? DEFAULT_PORT : port;
                }
                catch { port = DEFAULT_PORT; };
                builder.Servers = new List<Server>(1);
                builder.Servers.Add(new Server() { Host = conn.Host, Port = port });
                var isReplicaSet = conn.Scheme.Equals("mongodbrs", StringComparison.InvariantCultureIgnoreCase);

                if (!String.IsNullOrEmpty(conn.UserInfo))
                {
                    builder.SetAuthentication(conn.UserInfo);
                }

                if (!isReplicaSet)
                {
                    builder.Database = (conn.AbsolutePath ?? "").Trim('/', '\\');
                    if (String.IsNullOrEmpty(builder.Database))
                    {
                        builder.Database = DEFAULT_DATABASE;
                    }
                }
                else
                {
                    throw new NotSupportedException("This is not quite there yet, try back in a few days.");
                }


                builder.AssignOptions(conn.Query.TrimStart('?'));
                return builder;
            }
            else
            {
                throw new MongoException("The connection string passed does not appear to be a valid Uri, it should be of the form: " +
                    "'mongodb[rs]://[user:password]@host:[port]/[replicaSetName]/dbname?[options]' where the parts in brackets are optional.");
            }
        }


        /// <summary>
        /// The build options.
        /// </summary>
        /// <param retval="container">The container.</param>
        /// <param retval="options">The options.</param>
        /// <exception cref="MongoException">
        /// </exception>
        internal void AssignOptions(string options)
        {
            if (string.IsNullOrEmpty(options))
            {
                return;
            }

            // don't like how I did this
            var parts = options.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var kvp = part.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (kvp.Length != 2)
                {
                    throw new MongoException("Invalid connection option: " + part);
                }

                _optionsHandler[kvp[0].ToLower()](kvp[1], this);
            }
        }

        /// <summary>
        /// The build authentication.
        /// </summary>
        /// <param retval="sb">The string builder.</param>
        /// <returns></returns>
        /// <exception cref="MongoException">
        /// </exception>
        private void SetAuthentication(String userInfo/*, StringBuilder coreBuilder*/)
        {
            var m = _rxUserInfoMatch.Match(userInfo);
            if (!m.Success)
            {
                throw new MongoException("Invalid connection string: authentication should be in the form of username:password");
            }
            else
            {
                this.UserName = m.Groups["username"].Value;
                this.Password = m.Groups["password"].Value;
            }
        }

        
        public object Clone()
        {
            return ConnectionOptions.Create(this._connectionString);
        }

    }
}