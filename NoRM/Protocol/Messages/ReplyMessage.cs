using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NoRM.BSON;

namespace NoRM.Protocol.Messages
{
    /// <summary>
    /// The reply message.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class ReplyMessage<T> : Message
    {
        private readonly List<T> _results;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplyMessage{T}"/> class.
        /// Processes a response stream.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="fullyQualifiedCollestionName">The fully Qualified Collestion Name.</param>
        /// <param name="reply">The reply.</param>
        internal ReplyMessage(IConnection connection, string fullyQualifiedCollestionName, BinaryReader reply)
            : base(connection, fullyQualifiedCollestionName)
        {
            _messageLength = reply.ReadInt32();
            _requestID = reply.ReadInt32();
            _responseID = reply.ReadInt32();
            _op = (MongoOp) reply.ReadInt32();
            HasError = reply.ReadInt32() == 1 ? true : false;
            CursorID = reply.ReadInt64();
            CursorPosition = reply.ReadInt32();

            // this.ResultsReturned = reply.ReadInt32();
            var read = reply.ReadInt32();

            // decrement the length for all the reads.
            _messageLength -= 4 + 4 + 4 + 4 + 4 + 4 + 8 + 4 + 4;

            _results = new List<T>(100); // arbitrary number seems like it would be a sweet spot for many queries.

            if (HasError)
            {
                // TODO: load the error document.
            }
            else
            {
                while (_messageLength > 0)
                {
                    var length = reply.ReadInt32();
                    if (length > 0)
                    {
                        var bin = BitConverter.GetBytes(length).Concat(
                            reply.ReadBytes(length - 4)).ToArray();

                        IDictionary<WeakReference, Flyweight> outProps = new Dictionary<WeakReference, Flyweight>(0);
                        var obj = BsonDeserializer.Deserialize<T>(bin, ref outProps);
                        this._results.Add(obj);
                        if (_connection.EnableExpandoProperties)
                        {
                            ExpandoProps.SetFlyWeightObjects(outProps);
                        }
                    }

                    _messageLength -= length;
                }
            }
        }

        /// <summary>
        /// The cursor to be used in future calls to "get more"
        /// </summary>
        public long CursorID { get; protected set; }

        /// <summary>
        /// The location of the cursor.
        /// </summary>
        public int CursorPosition { get; protected set; }

        /// <summary>
        /// If "HasError" is set, 
        /// </summary>
        public bool HasError { get; protected set; }

        /// <summary>
        /// The number of results returned from this request.
        /// </summary>
        public int Count
        {
            get { return this._results.Count; }
        }

        /// <summary>
        /// Gets enumerable results.
        /// </summary>
        public IEnumerable<T> Results
        {
            get { return this._results.AsEnumerable(); }
        }
    }
}