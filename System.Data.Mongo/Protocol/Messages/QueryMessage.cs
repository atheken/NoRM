using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BSONLib;
using System.Net.Sockets;

namespace System.Data.Mongo.Protocol.Messages
{
    internal class QueryMessage<T> : Message where T : class, new()
    {
        private static BSONSerializer _serializer = new BSONSerializer();
        private MongoOp _op = MongoOp.Query;
        private MongoContext _context;
        private String _collection;
        private int _messageID = 7890339;//random number.
        private byte[] _header = new byte[16];
        private int _options = 4;
        private int _numberToSkip;
        private int _numberToTake;
        
        internal QueryMessage(MongoContext context, String fullyQualifiedCollName)
        {
            this._context = context;
            this._collection = fullyQualifiedCollName;

            var opCode = BitConverter.GetBytes((int)this._op);
            //header[0-3] = length (+12 for header.)
            //header[4-7] = requestID ("unique" identifier) for request.
            //header[8-11] = responseTo ("unique" identifier) from the server
            //header[12-15] = message type (MongoOp)
        }

        /// <summary>
        /// The id that was/will be used in the request to the server.
        /// </summary>
        public int MessageID
        {
            get
            {
                return this._messageID;
            }
        }

        private byte[] _query;

        /// <summary>
        /// A BSON query.
        /// </summary>
        public T Query
        {
            get
            {
                return QueryMessage<T>._serializer.Deserialize<T>(this._query);
            }
            set
            {
                this._query = QueryMessage<T>._serializer.Serialize<T>(value);
            }
        }

        public IEnumerable<T> Execute()
        {
            List<byte[]> messageBytes = new List<byte[]>(9);
            #region Message Header
            messageBytes.Add(new byte[4]);//allocate 4 bytes for the query.
            messageBytes.Add(BitConverter.GetBytes(this.MessageID));//the requestid
            messageBytes.Add(BitConverter.GetBytes(0));//the response id
            messageBytes.Add(BitConverter.GetBytes((int)MongoOp.Query));//the op type. 
            #endregion

            #region Message Body
            messageBytes.Add(BitConverter.GetBytes(0));//sets option to "none"
            //append the collection name and then null-terminate it.
            messageBytes.Add(Encoding.UTF8.GetBytes(this._collection).Concat(new byte[0]).ToArray());
            messageBytes.Add(this._query);
            #endregion

            //now that we know the full size of the message, we can write it to the first array.
            var size = messageBytes.Sum(y => y.Length);
            messageBytes[0] = BitConverter.GetBytes(size);

            var sock = this._context.Socket();

            sock.Send(messageBytes.SelectMany(y => y).ToArray());


            return Enumerable.Empty<T>();
        }

    }
}
