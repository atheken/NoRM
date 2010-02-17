using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Protocol.SystemMessages.Responses
{
    public class CollectionStatistics
    {
        public CollectionStatistics() { }

        public string collstats { get; set; }
        public string Ns { get; set; }
        public long? Count { get; set; }
        public long? Size { get; set; }
        public long? StorageSize { get; set; }
        public int? NIndexes { get; set; }
        public double? OK { get; set; }
    }
}
