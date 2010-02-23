using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using NoRM.BSON;

namespace NoRM.Protocol
{
    public class Message
    {
        protected MongoOp _op = MongoOp.Message;
        protected IConnection _connection;
        protected String _collection;
        protected int _requestID;
        protected int _responseID;
        protected int _messageLength;

        /// <summary>
        /// provides of messages
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fullyQualifiedConnection"></param>
        internal Message(IConnection connection, String fullyQualifiedCollName)
        {
            this._connection = connection;
            this._collection = fullyQualifiedCollName;
        }
    }
}
