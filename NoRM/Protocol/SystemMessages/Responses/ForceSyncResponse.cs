using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Protocol.SystemMessages.Responses
{
    public class ForceSyncResponse
    {
        public double? OK { get; set; }
        public int? NumFiles { get; set; }
    }
}
