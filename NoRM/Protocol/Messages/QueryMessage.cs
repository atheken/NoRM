using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NoRM.BSON;
using System.Net.Sockets;
using System.Threading;

namespace NoRM.Protocol.Messages
{
    /// <summary>
    /// A query to the db.
    /// </summary>
    /// <typeparam name="T">The request document type, and the response document type.</typeparam>
    public class QueryMessage<T> : QueryMessage<T, T> where T : class, new()
    {
        public QueryMessage(IConnection connection)
            : base(connection, typeof(T).Name) {

        }
        public QueryMessage(IConnection connection, String fullyQualifiedCollName) : base(connection, fullyQualifiedCollName)
        {

        }
    }

    /// <summary>
    /// A query to the db.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <typeparam name="U">The request type.</typeparam>
    public class QueryMessage<T, U> : Message 
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
        private int _numberToTake = Int32.MaxValue;

        public QueryMessage(IConnection connection, String fullyQualifiedCollName) : base(connection, fullyQualifiedCollName)
        {
            this._op = MongoOp.Query;
        }

        private U _query;

        /// <summary>
        /// A BSON query.
        /// </summary>
        public U Query
        {
            set
            {
                this._query = value;
            }
        }

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
            messageBytes.Add(BitConverter.GetBytes(this._numberToTake));//number to take.

            if (this._query != null)
            {
                messageBytes.Add(BsonSerializer.Serialize(this._query));
            }
            #endregion

            //now that we know the full size of the message, we can write it to the first array.
            var size = messageBytes.Sum(y => y.Length);
            messageBytes[0] = BitConverter.GetBytes(size);


            var conn = _connection;
            conn.Write(messageBytes.SelectMany(y => y).ToArray(), 0, size);
            
            //so, the server can accepted the query, now we do the second part.
            
            var stream = conn.GetStream();
            while (!stream.DataAvailable)
            {                
                Thread.Sleep(1);
            }

            if (!stream.DataAvailable)
            {
                throw new TimeoutException("MongoDB did not return a reply in the specified time for this context: " + conn.QueryTimeout.ToString());
            }
            return new ReplyMessage<T>(conn, this._collection, new BinaryReader(new BufferedStream(stream)));

        }

    }
}
