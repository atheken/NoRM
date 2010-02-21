using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using NoRM.BSON;

namespace NoRM.Protocol
{
    internal class Message
    {
        protected MongoOp _op = MongoOp.Message;
        protected MongoContext _context;
        protected String _collection;
        protected int _requestID;
        protected int _responseID;
        protected int _messageLength;

        /// <summary>
        /// provides of messages
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fullyQualifiedConnection"></param>
        internal Message(MongoContext context, String fullyQualifiedCollName)
        {
            this._context = context;
            this._collection = fullyQualifiedCollName;
        }
    }
}
