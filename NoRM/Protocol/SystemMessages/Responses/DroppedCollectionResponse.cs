using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoSharp.Protocol.SystemMessages.Responses
{
    public class DroppedCollectionResponse
    {
        public DroppedCollectionResponse() { }

        public string drop { get; set; }

        public double? NIndexesWas { get; set; }
        public string Msg { get; set; }
        public string Ns { get; set; }
        public double? OK { get; set; }
    }
}
