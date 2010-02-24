namespace NoRM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Protocol.SystemMessages.Requests;
    using Protocol.SystemMessages.Responses;

    public class MongoAdmin : IDisposable
    {
        private readonly IConnectionProvider _connectionProvider;
        private bool _disposed;
        private readonly MongoDatabase _database;
        private readonly IConnection _connection;
        
        public MongoAdmin() : this("mongodb://127.0.0.1:27017") { }        
        public MongoAdmin(string connectionString)
        {            
            _connectionProvider = ConnectionProviderFactory.Create(connectionString);
            _connection = _connectionProvider.Open(null);
            _database = new MongoDatabase(_connectionProvider.ConnectionString.Database, _connection);      
        }        
        
        public DroppedDatabaseResponse DropDatabase()
        {
            return _database.GetCollection<DroppedDatabaseResponse>("$cmd").FindOne(new DropDatabaseRequest());
        }
        public IEnumerable<CurrentOperationResponse> GetCurrentOperations()
        {
            return _database.GetCollection<CurrentOperationResponse>("$cmd.sys.inprog").Find();            
        }        
        public bool ResetLastError()
        {
            var result = _database.GetCollection<GenericCommandResponse>("$cmd").FindOne(new { reseterror = 1d });
            return result != null && result.OK == 1.0;            
        }
        public PreviousErrorResponse PreviousErrors()
        {
            return _database.GetCollection<PreviousErrorResponse>("$cmd").FindOne(new { getpreverror = 1d });
        }
        public AssertInfoResponse AssertionInfo()
        {
            return _database.GetCollection<AssertInfoResponse>("$cmd").FindOne(new { assertinfo = 1d });
        }
        public ServerStatusResponse ServerStatus()
        {
            return  _database.GetCollection<ServerStatusResponse>("$cmd").FindOne(new { serverStatus = 1d });
        }
        public bool SetProfileLevel(int value)
        {
            var result = _database.GetCollection<ProfileLevelResponse>("$cmd").FindOne(new { profile = value });
            return result != null && result.OK == 1.0;
        }
        public double? GetProfileLevel()
        {
            var result = _database.GetCollection<ProfileLevelResponse>("$cmd").FindOne(new { profile = -1 });
            return result.Was;
        }
        public bool RepairDatabase(bool preserveClonedFilesOnFailure, bool backupOriginalFiles)
        {         
            var result = _database.GetCollection<GenericCommandResponse>("$cmd")
                .FindOne(new {repairDatabase = 1d, preserveClonedFilesOnFailure, backupOriginalFiles});
            return result != null && result.OK == 1.0;
        }

        public GenericCommandResponse KillOperation(double operationId)
        {
            AssertConnectedToAdmin();
            return _database.GetCollection<GenericCommandResponse>("$cmd.sys.killop").FindOne(new { op = operationId });
        }
        public IEnumerable<DatabaseInfo> GetAllDatabases()
        {
            AssertConnectedToAdmin();
            var response = _database.GetCollection<ListDatabasesResponse>("$cmd").FindOne(new ListDatabasesRequest());
            return response != null && response.OK == 1.0 ? response.Databases : Enumerable.Empty<DatabaseInfo>();
        }
        public ForceSyncResponse ForceSync(bool async)
        {
            AssertConnectedToAdmin();
            return _database.GetCollection<ForceSyncResponse>("$cmd").FindOne(new { fsync = 1d, async });
        }
        public BuildInfoResponse BuildInfo()
        {
            AssertConnectedToAdmin();
            return _database.GetCollection<BuildInfoResponse>("$cmd").FindOne(new { buildinfo = 1d });
        }

        
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