using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoSharp.Protocol.SystemMessages.Responses
{
    public class CurrentOperationResponse 
    {
        public CurrentOperationResponse() { }

        public double? OpID { get; set; }
        public string Op { get; set; }
        public string Ns { get; set; }
        public string Query { get; set; }
        public double? InLock { get; set; }
    }
}
