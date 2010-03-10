using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NoRM.BSON;

namespace NoRM.Protocol.Messages
{
    /// <summary>
    /// A query to the db.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <typeparam name="U">The request type.</typeparam>
    public class QueryMessage<T, U> : Message
    {
        private int _numberToTake = Int32.MaxValue;
        private U _query;
        private QueryOptions _queryOptions = QueryOptions.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryMessage{T,U}"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="fullyQualifiedCollName">The fully qualified coll name.</param>
        public QueryMessage(IConnection connection, string fullyQualifiedCollName): base(connection, fullyQualifiedCollName)
        {
            _op = MongoOp.Query;
        }

        /// <summary>
        /// A BSON query.
        /// </summary>
        public U Query
        {
            set { _query = value; }
        }

        /// <summary>
        /// Gets or sets the number to take.
        /// </summary>
        public int NumberToTake
        {
            get { return this._numberToTake; }
            set { this._numberToTake = value; }
        }

        /// <summary>
        /// The number of documents to skip before starting to return documents.
        /// </summary>
        public int NumberToSkip { get; set; }

        /// <summary>
        /// Causes this message to be sent and a repsonse to be generated.
        /// </summary>
        /// <returns>
        /// </returns>
        public ReplyMessage<T> Execute()
        {
            var messageBytes = new List<byte[]>(9)
                                   {
                                       new byte[4],
                                       BitConverter.GetBytes(_requestID),
                                       BitConverter.GetBytes(0),
                                       BitConverter.GetBytes((int) MongoOp.Query),
                                       BitConverter.GetBytes((int) _queryOptions),
                                       Encoding.UTF8.GetBytes(_collection).Concat(new byte[1]).ToArray(),
                                       BitConverter.GetBytes(NumberToSkip),
                                       BitConverter.GetBytes(_numberToTake)
                                   };

            #region Message Body

            // append the collection name and then null-terminate it.

            if (_query != null)
            {
                messageBytes.Add(BsonSerializer.Serialize(_query));
            }

            #endregion

            // now that we know the full size of the message, we can write it to the first array.
            var size = messageBytes.Sum(y => y.Length);
            messageBytes[0] = BitConverter.GetBytes(size);


            var conn = _connection;
            conn.Write(messageBytes.SelectMany(y => y).ToArray(), 0, size);

            // so, the server can accepted the query, now we do the second part.
            var stream = conn.GetStream();
            while (!stream.DataAvailable)
            {
                Thread.Sleep(1);
            }

            if (!stream.DataAvailable)
            {
                throw new TimeoutException("MongoDB did not return a reply in the specified time for this context: " +
                                           conn.QueryTimeout.ToString());
            }

            return new ReplyMessage<T>(conn, _collection, new BinaryReader(new BufferedStream(stream)));
        }

        #region Nested type: QueryOptions

        /// <summary>
        /// The available options when creating a query against Mongo.
        /// </summary>
        [Flags]
        internal enum QueryOptions : int
        {
            /// <summary>
            /// The none.
            /// </summary>
            None = 0,

            /// <summary>
            /// The tailabile cursor.
            /// </summary>
            TailabileCursor = 2,

            /// <summary>
            /// The slave ok.
            /// </summary>
            SlaveOK = 4,


            // OplogReplay = 8 -- not for use by driver implementors
            /// <summary>
            /// The no cursor timeout.
            /// </summary>
            NoCursorTimeout = 16
        }

        #endregion
    }
}
