using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoSharp.Protocol.SystemMessages.Responses
{
    public class AssertInfoResponse
    {
        public double? OK { get; set; }
        public bool? DBAsserted { get; set; }
        public bool? Asserted { get; set; }
        public String Assert { get; set; }
        public String AssertW { get; set; }
        public String AssertMSG { get; set; }
        public String AssertUser { get; set; }
    }
}
