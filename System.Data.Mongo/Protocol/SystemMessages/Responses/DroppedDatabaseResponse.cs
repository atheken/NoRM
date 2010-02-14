using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Mongo.Protocol.SystemMessages.Responses
{
    public class DroppedDatabaseResponse
    {
        public String Dropped { get; set; }
        public double? OK { get; set; }
    }
}
