using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Protocol.SystemMessages.Responses
{
    internal class GetNonceResponse
    {
        public String Nonce { get; set; }
        public double? OK { get; set; }
    }
}
