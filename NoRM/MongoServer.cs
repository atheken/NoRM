using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using NoRM.Protocol.Messages;
using System.Security.Cryptography;
using NoRM.Protocol.SystemMessages.Responses;
using NoRM.Protocol.SystemMessages.Requests;
using NoRM.BSON;
using NoRM.Protocol.SystemMessages;

namespace NoRM
{
    public class MongoServer
    {
        private static MD5 _md5 = MD5.Create();
        private String _serverName = "127.0.0.1";

        /// <summary>
        /// This indicates if the context should load properties 
        /// that are not part of a given class definition into a 
        /// special flyweight lookup. 
        /// 
        /// Disabled by default.
        /// </summary>
        /// <remarks>
        /// This is useful when the class definition you want to use doesn't support a particular property, but the database should 
        /// still maintain it, or you do not want to squash it on save.
        /// 
        /// Enabling this will cause additinal overhead when loading/saving, as well as more memory consumption during the lifetime of the object.
        /// </remarks>
        public bool EnableExpandoProperties
        {
            get;
            private set;
        }

        public ForceSyncResponse ForceSync(bool async)
        {
            return this.GetDatabase("admin")
                .GetCollection<ForceSyncResponse>("$cmd")
                .FindOne(new { fsync = 1d, async = async });
        }

        /// <summary>
        /// Clears the last error on this connection.
        /// </summary>
        /// <returns></returns>
        public bool ResetLastError()
        {
            bool retval = false;

            var result = this.GetDatabase("admin")
                .GetCollection<GenericCommandResponse>("$cmd")
                .FindOne(new { reseterror = 1d });

            if (result != null && result.OK == 1.0)
            {
                retval = true;
            }

            return retval;
        }

        public AssertInfoResponse AssertionInfo()
        {
            return this.GetDatabase("admin")
                 .GetCollection<AssertInfoResponse>("$cmd")
                 .FindOne(new { assertinfo = 1d });

        }

        /// <summary>
        /// Get info about the previous errors on this server
        /// </summary>
        public PreviousErrorResponse PreviousErrors()
        {

            return this.GetDatabase("admin")
                .GetCollection<PreviousErrorResponse>("$cmd")
                .FindOne(new { getpreverror = 1d });

        }

        public bool RepairDatabase(bool preserveClonedFilesOnFailure, bool backupOriginalFiles)
        {
            var retval = false;
            var result = this.GetDatabase("admin")
                .GetCollection<GenericCommandResponse>("$cmd")
                .FindOne(new
                {
                    repairDatabase = 1d,
                    preserveClonedFilesOnFailure = preserveClonedFilesOnFailure,
                    backupOriginalFiles = backupOriginalFiles
                });
            if (result != null && result.OK == 1.0)
            {
                retval = true;
            }
            return retval;
        }

        protected BuildInfoResponse _buildInfo;

        private double? _profileLevel;

        public double ProfileLevel
        {
            get
            {
                if (this._profileLevel == null)
                {
                    this.ProfileLevel = -1;
                }
                return this._profileLevel.Value;
            }
            set
            {
                var result = this.GetDatabase("admin")
                        .GetCollection<ProfileLevelResponse>("$cmd")
                        .FindOne(new { profile = value });
                if (result != null && result.OK == 1.0)
                {
                    this._profileLevel = result.Was;
                }
            }
        }

        public BuildInfoResponse BuildInfo()
        {
            if (this._buildInfo == null)
            {
                this._buildInfo = this.GetDatabase("admin")
                    .GetCollection<BuildInfoResponse>("$cmd")
                    .FindOne(new { buildinfo = 1d });

            }
            return this._buildInfo;
        }

        public ServerStatusResponse ServerStatus()
        {
            return this.GetDatabase("admin")
                    .GetCollection<ServerStatusResponse>("$cmd")
                    .FindOne(new { serverStatus = 1d });
        }

        /// <summary>
        /// Number of seconds to wait for a response from the server before throwing a timeout exception.
        /// Defaults to 30.
        /// </summary>
        public int QueryTimeout
        {
            get;
            set;
        }
        /// <summary>
        /// The ip/domain name of the server.
        /// </summary>
        public String ServerName
        {
            get { return _serverName; }
            set { _serverName = value; }
        }

        /// <summary>
        /// The port on which the server is accessible.
        /// </summary>
        private int _serverPort = 27017;

        public int ServerPort
        {
            get { return _serverPort; }
            set { _serverPort = value; }
        }

        protected IPEndPoint _endPoint;

        /// <summary>
        /// Specify the host and the port to connect to for the mongo db.
        /// </summary>
        /// <param name="server">The server IP or hostname (127.0.0.1 is the default)</param>
        /// <param name="port">The port on which mongo is running (27017 is the default)</param>
        /// <param name="enableExpandoProps">Should requests to this database push/pull props from the DB that are not part of the specified object?</param>
        public MongoServer(String server, int port, bool enableExpandoProps)
        {
            this.QueryTimeout = 30;
            this._serverName = server;
            this._serverPort = port;
            this.EnableExpandoProperties = enableExpandoProps;
        }

        /// <summary>
        /// Drop this database from the mongo server (be careful what you wish for!)
        /// </summary>
        /// <returns></returns>
        public DroppedDatabaseResponse DropDatabase(String dbName)
        {
            var result = this.GetDatabase(dbName)
                .GetCollection<DroppedDatabaseResponse>("$cmd")
                .FindOne(new DropDatabaseRequest());

            return result;
        }


        /// <summary>
        /// Attempt to authenticate this user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Authenticate(String username, String password)
        {
            bool retval = false;

            //HACK: not sure how to send a query without selecting a database, so I am hooking admin. my bad.
            var nonce = this.GetDatabase("admin")
                .GetCollection<GetNonceResponse>("$cmd")
                .FindOne(new { getnonce = true });

            if (nonce.OK == 1)
            {
                //TODO: arg! the docs for this on Mongo's site are terrible!
            }
            return retval;
        }

        
        /// <summary>
        /// Creates a context that will connect to 127.0.0.1:27017 (MongoDB on the default port).
        /// </summary>
        /// <remarks>
        /// This also disabled Expando props for documents.
        /// </remarks>
        public MongoServer()
            : this("127.0.0.1", 27017, false)
        {

        }

        /// <summary>
        /// Will provide an object reference to a DB within the current context.
        /// </summary>
        /// <remarks>
        /// I would recommend adding extension methods, or subclassing MongoContext 
        /// to provide strongly-typed members for each database in your context, this
        /// will localize Strings to a few places - reducing typo issues.
        /// </remarks>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public MongoDatabase GetDatabase(String dbName)
        {
            var retval = new MongoDatabase(dbName, this);
            return retval;
        }

        /// <summary>
        /// Constructs a socket to the server.
        /// </summary>
        /// <returns></returns>
        internal TcpClient ServerConnection()
        {
            return new TcpClient(this._serverName, this._serverPort);
        }

        /// <summary>
        /// This simply forces the TcpClient to initialize, and returns the connected status. If the server/port is invalid, a SocketException
        /// will be thrown.
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            return ServerConnection().Connected;
        }

        /// <summary>
        /// Returns a list of databases that already exist on this context.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DatabaseInfo> GetAllDatabases()
        {
            var retval = Enumerable.Empty<DatabaseInfo>();

            var response = this.GetDatabase("admin")
                .GetCollection<ListDatabasesResponse>("$cmd")
                .FindOne(new ListDatabasesRequest());

            if (response != null && response.OK == 1.0)
            {
                retval = response.Databases;
            }
            return retval;
        }

        /// <summary>
        /// Returns a list of all operations currently going on the server.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CurrentOperationResponse> GetCurrentOperations()
        {
            var response = this.GetDatabase("admin")
                .GetCollection<CurrentOperationResponse>("$cmd.sys.inprog")
                .Find();

            return response;
        }

        /// <summary>
        /// Takes an operation ID and attempts to kill the running operation.
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        public GenericCommandResponse KillOperation(double operationId)
        {
            var response = this.GetDatabase("admin")
                .GetCollection<GenericCommandResponse>("$cmd.sys.killop")
                .FindOne(new { op = operationId });

            return response;
        }

    }
}
