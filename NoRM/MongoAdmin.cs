namespace NoRM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Protocol.SystemMessages.Requests;
    using Responses;


    /// <summary>
    /// This class is used to connect to the MongoDB server and send special Administrative commands to it.
    /// </summary>
    public class MongoAdmin : IDisposable
    {
        private readonly IConnectionProvider _connectionProvider;
        private bool _disposed;
        private readonly MongoDatabase _database;
        private readonly IConnection _connection;
        
        public MongoDatabase Database
        {
            get { return _database;}
        }
        
        public MongoAdmin() : this("mongodb://127.0.0.1:27017") { }        
        public MongoAdmin(string connectionString)
        {            
            _connectionProvider = ConnectionProviderFactory.Create(connectionString);
            _connection = _connectionProvider.Open(null);
            _database = new MongoDatabase(_connectionProvider.ConnectionString.Database, _connection);      
        }        
        
        /// <summary>
        /// Drop the admin database?
        /// </summary>
        /// <returns></returns>
        public DroppedDatabaseResponse DropDatabase()
        {
            return _database.GetCollection<DroppedDatabaseResponse>("$cmd").FindOne(new DropDatabaseRequest());
        }
       
        /// <summary>
        /// Yields a list of information about what processes are currently running on MongoDB.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CurrentOperationResponse> GetCurrentOperations()
        {
            return _database.GetCollection<CurrentOperationResponse>("$cmd.sys.inprog").Find();            
        }        

        /// <summary>
        /// Clear the last error on the server.
        /// </summary>
        /// <returns></returns>
        public bool ResetLastError()
        {
            var result = _database.GetCollection<GenericCommandResponse>("$cmd").FindOne(new { reseterror = 1d });
            return result != null && result.OK == 1.0;            
        }

        /// <summary>
        /// Get the previous error from the server.
        /// </summary>
        /// <returns></returns>
        public PreviousErrorResponse PreviousErrors()
        {
            return _database.GetCollection<PreviousErrorResponse>("$cmd").FindOne(new { getpreverror = 1d });
        }

        public AssertInfoResponse AssertionInfo()
        {
            return _database.GetCollection<AssertInfoResponse>("$cmd").FindOne(new { assertinfo = 1d });
        }
        
        /// <summary>
        /// Get information about the condition of the server.
        /// </summary>
        /// <returns></returns>
        public ServerStatusResponse ServerStatus()
        {
            return  _database.GetCollection<ServerStatusResponse>("$cmd").FindOne(new { serverStatus = 1d });
        }

        /// <summary>
        /// Set the profiling currently defined for the server, default is 1.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetProfileLevel(int value)
        {
            var result = _database.GetCollection<ProfileLevelResponse>("$cmd").FindOne(new { profile = value });
            return result != null && result.OK == 1.0;
        }

        /// <summary>
        /// Find out the profile level on the server.
        /// </summary>
        /// <returns></returns>
        public double? GetProfileLevel()
        {
            var result = _database.GetCollection<ProfileLevelResponse>("$cmd").FindOne(new { profile = -1 });
            return result.Was;
        }

        /// <summary>
        /// Ask the server to do a consistency check/repair on the database.
        /// </summary>
        /// <param name="preserveClonedFilesOnFailure"></param>
        /// <param name="backupOriginalFiles"></param>
        /// <returns></returns>
        public bool RepairDatabase(bool preserveClonedFilesOnFailure, bool backupOriginalFiles)
        {         
            var result = _database.GetCollection<GenericCommandResponse>("$cmd")
                .FindOne(new {repairDatabase = 1d, preserveClonedFilesOnFailure, backupOriginalFiles});
            return result != null && result.OK == 1.0;
        }
        
        /// <summary>
        /// Request that the server stops executing a currently running process.
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        public GenericCommandResponse KillOperation(double operationId)
        {
            this.AssertConnectedToAdmin();
            return _database.GetCollection<GenericCommandResponse>("$cmd.sys.killop").FindOne(new { op = operationId });
        }

        /// <summary>
        /// What databases are available on the server?
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DatabaseInfo> GetAllDatabases()
        {
            this.AssertConnectedToAdmin();
            var response = _database.GetCollection<ListDatabasesResponse>("$cmd").FindOne(new ListDatabasesRequest());
            return response != null && response.OK == 1.0 ? response.Databases : Enumerable.Empty<DatabaseInfo>();
        }

        /// <summary>
        /// Request that the server write any uncommitted writes to the filesystem.
        /// </summary>
        /// <param name="async"></param>
        /// <returns></returns>
        public ForceSyncResponse ForceSync(bool async)
        {
            this.AssertConnectedToAdmin();
            return _database.GetCollection<ForceSyncResponse>("$cmd").FindOne(new { fsync = 1d, async });
        }

        /// <summary>
        /// The information about this MongoDB, when and how it was built.
        /// </summary>
        /// <returns></returns>
        public BuildInfoResponse BuildInfo()
        {
            this.AssertConnectedToAdmin();
            return _database.GetCollection<BuildInfoResponse>("$cmd").FindOne(new { buildinfo = 1d });
        }

        /// <summary>
        /// Verify that we're in admin.
        /// </summary>
        private void AssertConnectedToAdmin()
        {
            if (_connectionProvider.ConnectionString.Database != "admin")
            {
                throw new MongoException("This command is only valid when connected to admin");
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }        

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing && _connection != null)
            {
                _connectionProvider.Close(_connection);
            }
            _disposed = true;
        }
        ~MongoAdmin()
        {
            Dispose(false);
        }
    }
}