using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BSONLib;
using System.Net.Sockets;
using System.Threading;

namespace System.Data.Mongo.Protocol.Messages
{
    internal class QueryMessage<T> : Message where T : class, new()
    {
        /// <summary>
        /// The available options when creating a query against Mongo.
        /// </summary>
        [Flags]
        internal enum QueryOptions : int
        {
            None = 0,
            TailabileCursor = 2,
            SlaveOK = 4,
            //OplogReplay = 8 -- not for use by driver implementors
            NoCursorTimeout = 16
        }

        private QueryOptions _queryOptions = QueryOptions.None;
        private int _numberToSkip = 0;
        private int _numberToTake = int.MaxValue;

        internal QueryMessage(MongoContext context, String fullyQualifiedCollName) :
            base(context, fullyQualifiedCollName)
        {
            this._op = MongoOp.Query;
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

        /// <summary>
        /// The number requested by this query.(defaults to UInt32.MaxValue)
        /// </summary>
        public int NumberToTake
        {
            get
            {
                return this._numberToTake;
            }
            set
            {
                this._numberToTake = value;
            }
        }

        /// <summary>
        /// The number of documents to skip before starting to return documents.
        /// </summary>
        public int NumberToSkip
        {
            get
            {
                return this._numberToSkip;
            }
            set
            {
                this._numberToSkip = value;
            }
        }

        /// <summary>
        /// Causes this message to be sent and a repsonse to be generated.
        /// </summary>
        /// <returns></returns>
        public ReplyMessage<T> Execute()
        {
            List<byte[]> messageBytes = new List<byte[]>(9);
            #region Message Header
            messageBytes.Add(new byte[4]);//allocate 4 bytes for the query.
            messageBytes.Add(BitConverter.GetBytes(this._requestID));//the requestid
            messageBytes.Add(BitConverter.GetBytes(0));//the response id
            messageBytes.Add(BitConverter.GetBytes((int)MongoOp.Query));//the op type. 
            #endregion

            #region Message Body
            messageBytes.Add(BitConverter.GetBytes((int)this._queryOptions));//sets option to "none"
            //append the collection name and then null-terminate it.
            messageBytes.Add(Encoding.UTF8.GetBytes(this._collection)
                .Concat(new byte[1]).ToArray());
            messageBytes.Add(BitConverter.GetBytes(this.NumberToSkip));//number to skip.
            messageBytes.Add(BitConverter.GetBytes(this.NumberToTake));//number to take.
            messageBytes.Add(this._query);
            #endregion

            //now that we know the full size of the message, we can write it to the first array.
            var size = messageBytes.Sum(y => y.Length);
            messageBytes[0] = BitConverter.GetBytes(size);

            var sock = this._context.Socket();
            sock.Send(messageBytes.SelectMany(y => y.ToArray()).ToArray());

            //so, the server can accepted the query, now we do the second part.
            int timeout = this._context.QueryTimeout;
            while (sock.Available == 0 && timeout > 0)
            {
                timeout--;
                Thread.Sleep(1000);
            }
            if (sock.Available == 0)
            {
                throw new TimeoutException("MongoDB did not return a reply in the specified time for this context: " + this._context.QueryTimeout.ToString());
            }

            var buffer = new byte[sock.Available];
            sock.Receive(buffer);

            return new ReplyMessage<T>(this._context, this._collection, buffer);
        }

    }
}
