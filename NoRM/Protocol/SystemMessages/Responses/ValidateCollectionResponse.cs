using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoSharp.Protocol.SystemMessages.Responses
{
    public class ValidateCollectionResponse
    {
        public ValidateCollectionResponse() { }

        public string validate { get; set; }
        public bool? scandata { get; set; }

        public string Ns { get; set; }
        public string Result { get; set; }
        public double? OK { get; set; }
    }
}
