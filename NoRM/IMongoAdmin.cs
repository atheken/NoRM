using System;
using Norm.Responses;
using System.Collections.Generic;
namespace Norm
{
    /// <summary>
    /// <see cref="Norm.MongoAdmin"/>
    /// </summary>
    public interface IMongoAdmin : IDisposable
    {
        AssertInfoResponse AssertionInfo();
        BuildInfoResponse BuildInfo();
        IMongoDatabase Database { get; }
        DroppedDatabaseResponse DropDatabase();
        ForceSyncResponse ForceSync(bool async);
        IEnumerable<DatabaseInfo> GetAllDatabases();
        IEnumerable<CurrentOperationResponse> GetCurrentOperations();
        int GetProfileLevel();
        GenericCommandResponse KillOperation(double operationId);
        PreviousErrorResponse PreviousErrors();
        bool RepairDatabase(bool preserveClonedFilesOnFailure, bool backupOriginalFiles);
        bool ResetLastError();
        ServerStatusResponse ServerStatus();
        bool SetProfileLevel(int value, out int previousLevel);
        bool SetProfileLevel(int value);
    }
}
