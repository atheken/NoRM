using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Protocol.SystemMessages.Responses
{
    public class SetProfileResponse
    {
        public SetProfileResponse() { }

        public int? profile { get; set; }

        public double? Was { get; set; }
        public double? OK { get; set; }
    }
}
