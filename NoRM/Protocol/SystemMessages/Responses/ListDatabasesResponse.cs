using System.Collections.Generic;

namespace Norm.Responses
{
    /// <summary>
    /// The structure for the response to the "listdatabases" command.
    /// </summary>
    public class ListDatabasesResponse : BaseStatusMessage
    {
        /// <summary>?? Gets the total size of all the databases returned?? </summary>
        /// <value></value>
        public double? TotalSize { get; set; }

        /// <summary>The databases.</summary>
        /// <value></value>
        public List<DatabaseInfo> Databases { get; set; }
    }
}