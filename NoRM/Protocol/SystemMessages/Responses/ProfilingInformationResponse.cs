using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Protocol.SystemMessages.Responses
{
    public class ProfilingInformationResponse
    {
        public ProfilingInformationResponse() { }

        public DateTime? Ts { get; set; }
        public string Info { get; set; }
        public double? Millis { get; set; }
    }
}
