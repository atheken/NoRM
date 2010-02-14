using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoSharp.BSON;

namespace MongoSharp.Protocol.SystemMessages.Responses
{
    public class DroppedDatabaseResponse : IFlyweight
    {
        public String Dropped { get; set; }
        public double? OK { get; set; }
    }
}
