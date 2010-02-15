using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoSharp.Protocol.SystemMessages.Responses
{
    public class GenericCommandResponse
    {
        public double? OK { get; set; }
        public string Info { get; set; }
    }
}
