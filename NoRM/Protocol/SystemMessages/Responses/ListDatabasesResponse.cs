using System.Collections.Generic;

namespace Norm.Responses
{
    /// <summary>
    /// The structure for the response to the "listdatabases" command.
    /// </summary>
    public class ListDatabasesResponse : BaseStatusMessage
    {
        public double? TotalSize { get; set; }
        public List<DatabaseInfo> Databases { get; set; }
    }
}