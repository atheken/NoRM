using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Protocol.SystemMessages.Responses
{
    public class ValidateCollectionResponse
    {
        public ValidateCollectionResponse() { }
        public string Ns { get; set; }
        public string Result { get; set; }
        public bool? Valid { get; set; }
        public double? LastExtentSize { get; set; }
        public double? OK { get; set; }
    }
}
