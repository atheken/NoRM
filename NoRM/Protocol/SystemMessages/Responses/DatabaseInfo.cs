using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoSharp.Protocol.SystemMessages.Responses
{
    public class DatabaseInfo
    {
        public DatabaseInfo() { }

        public string Name { get; set; }
        public double? SizeOnDisk { get; set; }
    }
}
