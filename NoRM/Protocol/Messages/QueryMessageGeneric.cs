using System;
using System.IO;
using System.Text;
using System.Threading;
using Norm.BSON;
using Norm.Protocol.SystemMessages;
using System.Collections.Generic;

namespace Norm.Protocol.Messages
{
    /// <summary>
    /// A query to the db.
    /// </summary>
    /// <typeparam retval="T">The response type.</typeparam>
    /// <typeparam retval="U">The request type.</typeparam>
    public class QueryMessage<T, U> : Message
    {
        /// <summary>
        /// The available options when creating a query against Mongo.
        /// </summary>
        [Flags]
        internal enum QueryOptions
        {
            None = 0,
            TailabileCursor = 2,
            SlaveOK = 4,
            //OplogReplay = 8 -- not for use by driver implementors
            NoCursorTimeout = 16
        }
        private static readonly byte[] _opBytes = BitConverter.GetBytes((int)MongoOp.Query);

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryMessage&lt;T, U&gt;"/> class.
        /// </summary>
        /// <param retval="connection">The connection.</param>
        /// <param retval="fullyQualifiedCollName">Name of the fully qualified coll.</param>
        public QueryMessage(IConnection connection, string fullyQualifiedCollName)
            : base(connection, fullyQualifiedCollName)
        {
            NumberToTake = int.MaxValue;
        }

        /// <summary>
        /// A BSON query.
        /// </summary>
        /// <value>The Query property gets/sets the Query data member.</value>
        internal U Query
        {
            get;
            set;
        }

        /// <summary>
        /// The fields to select from each document in the current collection.
        /// </summary>
        internal object FieldSelection { get; set; }

        /// <summary>This defines </summary>
        /// <value>The OrderBy property gets/sets the OrderBy data member.</value>
        internal object OrderBy
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the number of documents to take.
        /// </summary>
        /// <value>The number of documents to take.</value>
        internal int NumberToTake
        {
            get;
            set;
        }

        /// <summary>
        /// The number of documents to skip before starting to return documents.
        /// </summary>
        /// <value>The Map property gets/sets the Map data member.</value>
        public int NumberToSkip { get; set; }

        /// <summary>
        /// Causes this message to be sent and a repsonse to be generated.
        /// </summary>
        /// <returns></returns>
        public ReplyMessage<T> Execute()
        {
            var payload1 = GetPayload();
            var payload2 = new byte[0];
            if (this.FieldSelection != null)
            {
                payload2 = BsonSerializer.Serialize(this.FieldSelection);
            }
            var collection = Encoding.UTF8.GetBytes(_collection);
            var collectionLength = collection.Length + 1; //+1 is for collection's null terminator which we'll be adding in a bit
            var headLength = 28 + collectionLength;
            var length = headLength + payload1.Length + payload2.Length;
            var header = new byte[headLength];

            Buffer.BlockCopy(BitConverter.GetBytes(length), 0, header, 0, 4);
            Buffer.BlockCopy(_opBytes, 0, header, 12, 4);
            Buffer.BlockCopy(collection, 0, header, 20, collection.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(NumberToSkip), 0, header, 20 + collectionLength, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(NumberToTake), 0, header, 24 + collectionLength, 4);

            _connection.Write(header, 0, header.Length);
            _connection.Write(payload1, 0, payload1.Length);
            _connection.Write(payload2, 0, payload2.Length);

            var stream = _connection.GetStream();
            while (!stream.DataAvailable)
            {
                Thread.Sleep(1);
            }

            if (!stream.DataAvailable)
            {
                throw new TimeoutException("MongoDB did not return a reply in the specified time for this context: " + _connection.QueryTimeout.ToString());
            }
            return new ReplyMessage<T>(_connection, this._collection, new BinaryReader(new BufferedStream(stream)), MongoOp.Query, this.NumberToTake);
        }

        /// <summary>
        /// Construct query and order by BSON.
        /// </summary>
        /// <returns></returns>
        private byte[] GetPayload()
        {
            if (Query != null && Query is ISystemQuery)
            {
                return BsonSerializer.Serialize(Query);
            }
            var fly = new Expando();
            fly["query"] = Query;
            if (OrderBy != null)
            {
                fly["orderby"] = this.OrderBy;
            }
            return BsonSerializer.Serialize(fly);
        }
    }
}
