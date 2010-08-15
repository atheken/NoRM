using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Norm.Protocol;
using System.Timers;

namespace Norm
{
    /// <summary>
    /// The set of connection options associated with a particular URI.
    /// </summary>
    public class ConnectionOptions : IOptionsContainer, ICloneable
    {
        private String _connectionString;
        private bool _isNew = true;

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
        private static readonly Regex _rxSchemaMatch = new Regex("^((mongodb://)|(mongodbrs://))", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _rxUserInfoMatch = new Regex("(?<username>.+?):(?<password>.+)", RegexOptions.Compiled);

        #region options assignment delegates
        private static readonly IDictionary<string, Action<string, ConnectionOptions>>
            _optionsHandler = new Dictionary<string, Action<string, ConnectionOptions>>
              {

                  {"strict", (v, b) => b.StrictMode =bool.Parse(v)},
                  {"querytimeout", (v, b) => b.QueryTimeout = int.Parse(v)},
                  {"pooling", (v, b) =>{ if(!b._isNew){throw new MongoException("Connection pooling cannot be provided as an override option");} b.Pooled =bool.Parse(v);}},
                  {"poolsize", (v, b) =>{ if(!b._isNew){throw new MongoException("PoolSize cannot be provided as an override option");} b.PoolSize = int.Parse(v);}},
                  {"timeout", (v, b) =>{ if(!b._isNew){throw new MongoException("Timeout cannot be provided as an override option");} b.Timeout = int.Parse(v);}},
                  {"lifetime", (v, b) =>{ if(!b._isNew){throw new MongoException("Lifetime cannot be provided as an override option");} b.Lifetime = int.Parse(v);}},
                  {"verifywritecount", (v,b)=> b.VerifyWriteCount = int.Parse(v)},
                  {"readfromany",(v,b)=>b.ReadFromAny = bool.Parse(v)}
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

        private List<ClusterMember> _servers;
        private ClusterMember _primaryServer;
        private List<ClusterMember> _secondaryServers;

        /// <summary>
        /// Gets a server list.
        /// </summary>
        public IList<ClusterMember> Servers
        {
            get { return this._servers; }
            set
            {
                this._servers = value.OrderBy(y => y.State == MemberStatus.Primary).ToList();
                this._primaryServer = _servers.SingleOrDefault(y => y.State == MemberStatus.Primary);
                this._secondaryServers = _servers.Where(y => y.State == MemberStatus.Secondary).ToList();
            }
        }

        /// <summary>
        /// Returns the primary server in the replica set, if there is such a thing.
        /// </summary>
        public ClusterMember PrimaryServer
        {
            get
            {
                return this._primaryServer;
            }
        }

        /// <summary>
        /// Returns the the Secondary servers in the Replica Set
        /// (if these conntection options are taking part in in one)
        /// </summary>
        public IEnumerable<ClusterMember> SecondaryServers
        {
            get
            {
                return this._secondaryServers;
            }
        }

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
        /// Creates a connection string retval.
        /// </summary>
        /// <param retval="connectionString">The connection string.</param>
        /// <returns></returns>
        /// <exception cref="MongoException">
        /// Thrown when either an incorrect connection string is passed. 
        /// Or when using a replica set, a connection cannot be made to the Admin 
        /// database using the settings provided (in order to find the other servers).
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

            if (Uri.IsWellFormedUriString(connection, UriKind.Absolute)
                && !Regex.IsMatch("(^mongodb://$)|(^mongodbrs://$)", connection))
            {
                var retval = new ConnectionOptions();
                var conn = new Uri(connection, true);
                retval._connectionString = connection;

                int port;
                try
                {
                    port = conn.Port;
                    port = port == -1 ? DEFAULT_PORT : port;
                }
                catch
                {
                    port = DEFAULT_PORT;
                };

                retval.Servers = new List<ClusterMember>(1)
                {
                    new ClusterMember() {
                        ServerName = conn.Host + ":" + port.ToString(),
                        State = MemberStatus.Primary, 
                        Health = 1, 
                        ErrorMessage = "", 
                        LastHeartbeat = DateTime.Now, 
                        Uptime = 0 
                    }
                };
                var isReplicaSet = conn.Scheme.Equals("mongodbrs", StringComparison.InvariantCultureIgnoreCase);

                if (!String.IsNullOrEmpty(conn.UserInfo))
                {
                    retval.SetAuthentication(conn.UserInfo);
                }
                retval.Database = (conn.AbsolutePath ?? "").Trim('/', '\\');
                if (String.IsNullOrEmpty(retval.Database))
                {
                    retval.Database = DEFAULT_DATABASE;
                }

                if (isReplicaSet)
                {
                    retval.UseReplicaSets = true;
                    retval.RequeryMemberServers();
                    retval._updateTimer = new Timer(60000);
                    retval._updateTimer.Elapsed += (o, e) => retval.RequeryMemberServers();
                    retval._updateTimer.Start();
                }


                retval.AssignOptions(conn.Query.TrimStart('?'));
                retval._isNew = false;
                return retval;
            }
            else
            {
                throw new MongoException("The connection string passed does not appear to be a valid Uri, it should be of the form: " +
                    "'mongodb[rs]://[user:password]@host:[port]/[replicaSetName]/dbname?[options]' where the parts in brackets are optional.");
            }
        }

        /// <summary>
        /// Should be executed periodically to make sure everything is active.
        /// </summary>
        private void RequeryMemberServers()
        {
            if (UseReplicaSets)
            {

                //do some special replica set stuff.
                var format = "mongodb://{0}:{1}/admin?pooling=false";
                if (!String.IsNullOrEmpty(this.UserName) && !String.IsNullOrEmpty(this.Password))
                {
                    format = "mongodb://{2}:{3}@{0}:{1}/admin?pooling=false";
                }

                var servers = this.Servers;

                foreach (var s in servers)
                {
                    using (var admin = new MongoAdmin(String.Format(format, s.GetHost(),
                        s.GetPort(), this.UserName, this.Password)))
                    {
                        try
                        {
                            this.Servers = admin.GetReplicaSetStatus().Members
                                .OrderBy(y => y.State == MemberStatus.Primary).ToList();
                            break;
                        }
                        catch
                        {
                            //try the next server on the list.
                        }
                    }
                }
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

                _optionsHandler[Uri.UnescapeDataString(kvp[0].ToLower())](Uri.UnescapeDataString(kvp[1]), this);
            }
        }

        /// <summary>
        /// The build authentication.
        /// </summary>
        /// <param retval="sb">The string retval.</param>
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
                this.UserName = Uri.UnescapeDataString(m.Groups["username"].Value);
                this.Password = Uri.UnescapeDataString(m.Groups["password"].Value);
            }
        }


        public object Clone()
        {
            return ConnectionOptions.Create(this._connectionString);
        }

        public bool UseReplicaSets
        {
            get;
            private set;
        }


        public bool ReadFromAny
        {
            get;
            private set;
        }

        private bool _disposed;
        private Timer _updateTimer;

        ~ConnectionOptions(){
            _updateTimer.Stop();
            _updateTimer.Dispose();
        }

    }
}