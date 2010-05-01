using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Norm.BSON;
using Norm.Protocol.SystemMessages;

namespace Norm.Protocol.Messages
{
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
        internal enum QueryOptions
        {
            None = 0,
            TailabileCursor = 2,
            SlaveOK = 4,
            //OplogReplay = 8 -- not for use by driver implementors
            NoCursorTimeout = 16
        }

        private QueryOptions _queryOptions = QueryOptions.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryMessage&lt;T, U&gt;"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="fullyQualifiedCollName">Name of the fully qualified coll.</param>
        public QueryMessage(IConnection connection, string fullyQualifiedCollName)
            : base(connection, fullyQualifiedCollName)
        {
            this._op = MongoOp.Query;
            NumberToTake = int.MaxValue;
        }

        /// <summary>
        /// A BSON query.
        /// </summary>
        /// <value>The Query property gets/sets the Query data member.</value>
        public U Query
        {
            get;
            set;
        }

        /// <summary>
        /// The fields to select
        /// </summary>
        internal FieldSelectionList FieldSelection { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>The OrderBy property gets/sets the OrderBy data member.</value>
        public object OrderBy
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the number of documents to take.
        /// </summary>
        /// <value>The number of documents to take.</value>
        public int NumberToTake
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
            var messageBytes = new List<byte[]>(9){
                    new byte[4],
                    BitConverter.GetBytes(this._requestID),
                    BitConverter.GetBytes(0),
                    BitConverter.GetBytes((int) MongoOp.Query),
                    BitConverter.GetBytes((int) this._queryOptions),
                    Encoding.UTF8.GetBytes(this._collection).Concat(new byte[1]).ToArray(),
                    BitConverter.GetBytes(this.NumberToSkip),
                    BitConverter.GetBytes(this.NumberToTake)
            };

            #region Message Body
            //append the collection name and then null-terminate it.
            if (this.Query != null && this.Query is ISystemQuery)
            {
                messageBytes.Add(BsonSerializer.Serialize(this.Query));
            }
            else
            {
                var fly = new Expando();

                //if (this.Query is Flyweight)
                //{
                //    var properties = (this.Query as Flyweight).AllProperties();
                //    properties.ToList().ForEach(p => fly.Set(p.PropertyName, p.Value));
                //}
                fly["query"] = this.Query;

                //null for this is WasSuccessful. needs to be here, though.
                if (this.OrderBy != null)
                {
                    fly["orderby"] = this.OrderBy;
                }                
                //add more query options here, as needed.
                messageBytes.Add(BsonSerializer.Serialize(fly));
                
                if (FieldSelection != null)
                {
                    messageBytes.Add(BsonSerializer.Serialize(FieldSelection));
                }
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
            return new ReplyMessage<T>(conn, this._collection, new BinaryReader(new BufferedStream(stream)), this._op, this.NumberToTake);
        }
    }
}
