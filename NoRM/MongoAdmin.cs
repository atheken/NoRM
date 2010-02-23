namespace NoRM
{/*
    using Protocol.SystemMessages.Responses;
       
    //todo implement these  
    public class MongoAdmin
    {
 
        /// <summary>
        /// Drop this database from the mongo server (be careful what you wish for!)
        /// </summary>
        /// <returns></returns>
        public DroppedDatabaseResponse DropDatabase(string dbName)
        {
            var result = this.GetDatabase(dbName)
                .GetCollection<DroppedDatabaseResponse>("$cmd")
                .FindOne(new DropDatabaseRequest());

            return result;
        }
      
        public PreviousErrorResponse PreviousErrors()
        {

            return this.GetDatabase("admin")
                .GetCollection<PreviousErrorResponse>("$cmd")
                .FindOne(new { getpreverror = 1d });

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

        /// <summary>
        /// Get info about the previous errors on this server
        /// </summary>
        public PreviousErrorResponse PreviousErrors()
        {

            return this.GetDatabase("admin")
                .GetCollection<PreviousErrorResponse>("$cmd")
                .FindOne(new { getpreverror = 1d });

        }

        public ForceSyncResponse ForceSync(bool async)
        {
            return this.GetDatabase("admin")
                .GetCollection<ForceSyncResponse>("$cmd")
                .FindOne(new { fsync = 1d, async = async });
        }

        public AssertInfoResponse AssertionInfo()
        {
            return this.GetDatabase("admin")
                 .GetCollection<AssertInfoResponse>("$cmd")
                 .FindOne(new { assertinfo = 1d });

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
    }*/
}