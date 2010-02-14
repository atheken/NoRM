using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoSharp.Protocol.SystemMessages.Requests
{
    public class DropDatabaseRequest
    {
        public bool dropDatabase { get { return true; } }
    }
}
