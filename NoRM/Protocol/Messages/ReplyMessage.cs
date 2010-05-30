using System;
using System.Collections.Generic;
using System.IO;
using Norm.BSON;

namespace Norm.Protocol.Messages
{
    /// <summary>
    /// The reply message.
    /// </summary>
    /// <typeparam retval="T">
    /// </typeparam>
    public class ReplyMessage<T> : Message, IDisposable
    {
        private readonly List<T> _results;
        private readonly int _limit;
        
        private MongoOp _originalOperation;
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplyMessage{T}"/> class.
        /// Processes a response stream.
        /// </summary>
        /// <param retval="connection">The connection.</param>
        /// <param retval="fullyQualifiedCollestionName">The fully Qualified Collestion Name.</param>
        /// <param retval="reply">The reply.</param>
        /// <param retval="originalOperation"></param>
        /// <param retval="limit"></param>
        internal ReplyMessage(IConnection connection, string fullyQualifiedCollestionName, BinaryReader reply, MongoOp originalOperation, int limit)
            : base(connection, fullyQualifiedCollestionName)
        {
            this._originalOperation = originalOperation;
            this._messageLength = reply.ReadInt32();
            this._requestID = reply.ReadInt32();
            this._responseID = reply.ReadInt32();
            this._op = (MongoOp)reply.ReadInt32();
            this._limit = limit;
            this.HasError = reply.ReadInt32() == 1 ? true : false;
            this.CursorID = reply.ReadInt64();
            this.CursorPosition = reply.ReadInt32();

            var count = reply.ReadInt32();

            // decrement the length for all the reads.
            _messageLength -= 4 + 4 + 4 + 4 + 4 + 4 + 8 + 4 + 4;

            _results = new List<T>(count);

            if (this.HasError)
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
                        IDictionary<WeakReference, Expando> outProps = new Dictionary<WeakReference, Expando>(0);
                        var obj = BsonDeserializer.Deserialize<T>(length, reply, ref outProps);
                        this._results.Add(obj);
                    }

                    _messageLength -= length;
                }
            }
        }

        /// <summary>
        /// The cursor to be used in future calls to "get more"
        /// </summary>
        /// <value>The CursorID property gets/sets the CursorID data member.</value>
        public long CursorID { get; protected set; }

        /// <summary>
        /// The location of the cursor.
        /// </summary>
        /// <value>The CursorPosition property gets/sets the CursorPosition data member.</value>
        public int CursorPosition { get; protected set; }

        /// <summary>
        /// If "HasError" is set, 
        /// </summary>
        /// <value>The HasError property gets/sets the HasError data member.</value>
        public bool HasError { get; protected set; }

        private ReplyMessage<T> _addedReturns = null;

        /// <summary>
        /// An enumerable result set.
        /// </summary>
        /// <value>The Results property gets the Results data member.</value>
        public IEnumerable<T> Results
        {
            get
            {
                foreach (var r in this._results)
                {
                    yield return r;
                }
                if (this.CursorID != 0 && this._results.Count > 0 && this._limit - this._results.Count > 0)
                {
                    this._addedReturns = new GetMoreMessage<T>(this._connection, 
                        this._collection, this.CursorID, this._limit - this._results.Count).Execute();
                }
                if (this._addedReturns != null)
                {
                    foreach (var r in this._addedReturns.Results)
                    {
                        yield return r;
                    }
                }
                yield break;
            }
        }

        #region IDisposable Members

        /// <summary>TODO::Description.</summary>
        public void Dispose()
        {
            //this should kill the cursor if it exists.
        }

        #endregion
    }
}