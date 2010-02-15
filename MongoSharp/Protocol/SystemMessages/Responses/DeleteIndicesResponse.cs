using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoSharp.Protocol.SystemMessages.Responses
{
    class DeleteIndicesResponse
    {
        /// <summary>
        /// Did this command execute successfully.
        /// </summary>
        public double? OK { get; set; }

        /// <summary>
        /// Number of deleted indices.
        /// </summary>
        public int? NIndexesWas { get; set; }
    }
}
