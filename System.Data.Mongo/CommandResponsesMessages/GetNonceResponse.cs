using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Mongo.CommandResponsesMessages
{
    internal class GetNonceResponse
    {
        public String Nonce { get; set; }
        public double? OK { get; set; }
    }
}
