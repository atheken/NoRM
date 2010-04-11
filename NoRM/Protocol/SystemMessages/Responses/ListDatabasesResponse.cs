using System.Collections.Generic;

namespace Norm.Responses
{
    /// <summary>
    /// The structure for the response to the "listdatabases" command.
    /// </summary>
    public class ListDatabasesResponse : BaseStatusMessage
    {
        /// <summary>TODO::Description.</summary>
        public double? TotalSize { get; set; }

        /// <summary>TODO::Description.</summary>
        public List<DatabaseInfo> Databases { get; set; }
    }
}