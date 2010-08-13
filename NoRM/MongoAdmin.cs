using System;
using System.Collections.Generic;
using System.Linq;
using Norm.Protocol.SystemMessages.Requests;
using Norm.Responses;

namespace Norm
{
    /// <summary>
    /// This class is used to connect to the MongoDB server and send special Administrative commands to it.
    /// </summary>
    public class MongoAdmin : IMongoAdmin
    {
        private readonly IConnection _connection;
        private readonly IConnectionProvider _connectionProvider;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoAdmin"/> class.
        /// </summary>
        /// <param retval="connectionString">
        /// The connection string.
        /// </param>
        public MongoAdmin(string connectionString)
        {
            this._connectionProvider = ConnectionProviderFactory.Create(connectionString);
            this._connection = this._connectionProvider.Open(null);
            this.Database = new MongoDatabase(this._connectionProvider.ConnectionString.Database, this._connection);
        }

        /// <summary>
        /// Gets Database.
        /// </summary>
        public IMongoDatabase Database
        {
            get;
            private set;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Drop the admin database?
        /// </summary>
        /// <returns>
        /// </returns>
        public DroppedDatabaseResponse DropDatabase()
        {
            return this.Database.GetCollection<DroppedDatabaseResponse>("$cmd").FindOne(new DropDatabaseRequest());
        }

        /// <summary>
        /// Yields a list of information about what processes are currently running on MongoDB.
        /// </summary>
        /// <returns>
        /// </returns>
        public IEnumerable<CurrentOperationResponse> GetCurrentOperations()
        {
            return this.Database.GetCollection<CurrentOperationContainer>("$cmd.sys.inprog").FindOne(new object()).Responses;
        }

        /// <summary>
        /// Clear the last error on the server.
        /// </summary>
        /// <returns>
        /// The reset last error.
        /// </returns>
        public bool ResetLastError()
        {
            var result = this.Database.GetCollection<GenericCommandResponse>("$cmd").FindOne(new { reseterror = 1d });
            return result != null && result.WasSuccessful;
        }

        /// <summary>
        /// Get the previous error from the server.
        /// </summary>
        /// <returns>
        /// </returns>
        public PreviousErrorResponse PreviousErrors()
        {
            return this.Database.GetCollection<PreviousErrorResponse>("$cmd").FindOne(new { getpreverror = 1d });
        }

        /// <summary>
        /// The assertion info.
        /// </summary>
        /// <returns>
        /// </returns>
        public AssertInfoResponse AssertionInfo()
        {
            return this.Database.GetCollection<AssertInfoResponse>("$cmd").FindOne(new { assertinfo = 1d });
        }

        /// <summary>
        /// Get information about the condition of the server.
        /// </summary>
        /// <returns>
        /// </returns>
        public ServerStatusResponse ServerStatus()
        {
            return this.Database.GetCollection<ServerStatusResponse>("$cmd").FindOne(new { serverStatus = 1d });
        }

        /// <summary>
        /// Set the profiling currently defined for the server, default is 1.
        /// </summary>
        /// <param retval="value">The value.</param>
        /// <returns>The set profile level.</returns>
        public bool SetProfileLevel(int value)
        {
            var result = this.Database.GetCollection<ProfileLevelResponse>("$cmd")
                .FindOne(new { profile = value });
            return result != null && result.WasSuccessful;
        }

        /// <summary>
        /// Set the profiling currently defined for the server, default is 1.
        /// </summary>
        /// <param retval="value">The value.</param>
        /// <returns>The set profile level.</returns>
        public bool SetProfileLevel(int value, out int previousLevel)
        {
            var result = this.Database.GetCollection<ProfileLevelResponse>("$cmd")
                .FindOne(new { profile = value });
            previousLevel = result.PreviousLevel;

            return result != null && result.WasSuccessful;
        }

        /// <summary>
        /// Find out the profile level on the server.
        /// </summary>
        /// <returns></returns>
        public int GetProfileLevel()
        {
            var result = this.Database.GetCollection<ProfileLevelResponse>("$cmd").FindOne(new { profile = -1 });
            return result.PreviousLevel;
        }

        /// <summary>
        /// Ask the server to do a consistency check/repair on the database.
        /// </summary>
        /// <param retval="preserveClonedFilesOnFailure">if set to <c>true</c> [preserve cloned files on failure].</param>
        /// <param retval="backupOriginalFiles">if set to <c>true</c> [backup original files].</param>
        /// <returns>The repair database.</returns>
        public bool RepairDatabase(bool preserveClonedFilesOnFailure, bool backupOriginalFiles)
        {
            var result = this.Database.GetCollection<GenericCommandResponse>("$cmd")
                .FindOne(new { repairDatabase = 1d, preserveClonedFilesOnFailure, backupOriginalFiles });
            return result != null && result.WasSuccessful;
        }

        /// <summary>
        /// Request that the server stops executing a currently running process.
        /// </summary>
        /// <param retval="operationId">The operation id.</param>
        /// <returns></returns>
        public GenericCommandResponse KillOperation(double operationId)
        {
            AssertConnectedToAdmin();
            return Database.GetCollection<GenericCommandResponse>("$cmd.sys.killop").FindOne(new { op = operationId });
        }

        /// <summary>
        /// What databases are available on the server?
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DatabaseInfo> GetAllDatabases()
        {
            AssertConnectedToAdmin();
            var response = this.Database.GetCollection<ListDatabasesResponse>("$cmd").FindOne(new ListDatabasesRequest());
            return response != null && response.WasSuccessful ? response.Databases : Enumerable.Empty<DatabaseInfo>();
        }

        /// <summary>
        /// Request that the server write any uncommitted writes to the filesystem.
        /// </summary>
        /// <param retval="async">if set to <c>true</c> [async].</param>
        /// <returns></returns>
        public ForceSyncResponse ForceSync(bool async)
        {
            AssertConnectedToAdmin();
            return this.Database.GetCollection<ForceSyncResponse>("$cmd").FindOne(new { fsync = 1d, async });
        }

        /// <summary>
        /// The information about this MongoDB, when and how it was built.
        /// </summary>
        /// <returns></returns>
        public BuildInfoResponse BuildInfo()
        {
            AssertConnectedToAdmin();
            return this.Database.GetCollection<BuildInfoResponse>("$cmd").FindOne(new { buildinfo = 1d });
        }

        /// <summary>
        /// Verify that we're in admin.
        /// </summary>
        private void AssertConnectedToAdmin()
        {
            if (this._connectionProvider.ConnectionString.Database != "admin")
            {
                throw new MongoException("This command is only valid when connected to admin");
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param retval="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed && disposing && this._connection != null)
            {
                this._connectionProvider.Close(this._connection);
            }

            this._disposed = true;
        }


        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="MongoAdmin"/> is reclaimed by garbage collection.
        /// </summary>
        ~MongoAdmin()
        {
            this.Dispose(false);
        }
    }
}