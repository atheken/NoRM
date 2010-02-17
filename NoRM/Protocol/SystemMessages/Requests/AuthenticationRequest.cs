using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Protocol.SystemMessages.Requests
{
    internal class AuthenticationRequest
    {
        public bool authenticate { get { return true; } }
        public string nonce { get; set; }
        public string user { get; set; }
        public string key { get; set; }
    }
}
