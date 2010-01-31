using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Mongo.Protocol
{
    /// <summary>
    /// Provides information about how a particular 
    /// request/response should be managed.
    /// </summary>
    internal class MessageHeader
    {
        /// <summary>
        /// This is the total size of the
        /// message in bytes, include 4 bytes for 
        /// this MessageLength when setting.
        /// </summary>
        public int MessageLength { get; set; }
        /// <summary>
        /// A client -or- database generated identifier 
        /// that identifies this request.
        /// </summary>
        public int RequestID { get; set; }
        /// <summary>
        /// Populated by the server, indicates which
        /// request is being fulfilled with this particlar response.
        /// </summary>
        public int ResponseTo { get; set; }
        /// <summary>
        /// The action that should be taken by the DB.
        /// </summary>
        public MongoOp OpCode { get; set; }
    }
}
