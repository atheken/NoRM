using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Mongo.Protocol.SystemMessages.Responses
{
    /// <summary>
    /// The structure for the response to the "listdatabases" command.
    /// </summary>
    public class ListDatabasesResponse
    {
        public bool? OK { get; set; }
        public long? TotalSize { get; set; }
        public List<DatabaseInfo> Databases { get; set; }
    }

}
