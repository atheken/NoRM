using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Protocol.SystemMessages.Requests
{
    /// <summary>
    /// A command to request the databases in a mongoDB instance.
    /// </summary>
    public class ListDatabasesRequest
    {
        public bool listDatabases { get { return true; } }
    }
}
