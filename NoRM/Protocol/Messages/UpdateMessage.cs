using System;
using System.Text;
using Norm.BSON;

namespace Norm.Protocol.Messages
{
    /// <summary>
    /// The update message.
    /// </summary>
    /// <typeparam retval="T">Document template Type</typeparam>
    /// <typeparam retval="U">Value document type</typeparam>
    internal class UpdateMessage<T, U> : Message
    {
        private static readonly byte[] _opBytes = BitConverter.GetBytes((int)MongoOp.Update);
        protected T _matchDocument;
        protected UpdateOption _options = UpdateOption.None;
        protected U _valueDocument;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateMessage{T,U}"/> class.
        /// </summary>
        /// <param retval="connection">The connection.</param>
        /// <param retval="collection">The collection.</param>
        /// <param retval="options">The options.</param>
        /// <param retval="matchDocument">The match document.</param>
        /// <param retval="valueDocument">The value document.</param>
        internal UpdateMessage(IConnection connection, string collection, UpdateOption options, T matchDocument, U valueDocument) : base(connection, collection)
        {
            this._options = options;
            this._matchDocument = matchDocument;
            this._valueDocument = valueDocument;            
        }

        /// <summary>
        /// Execute.
        /// </summary>
        /// <exception cref="DocumentExceedsSizeLimitsException{T}">
        /// </exception>
        /// <exception cref="DocumentExceedsSizeLimitsException{T}">
        /// </exception>
        public void Execute()
        {
            var payload1 = GetPayload(_matchDocument);
            var payload2 = GetPayload(_valueDocument);
            
            var collection = Encoding.UTF8.GetBytes(_collection);
            var collectionLength = collection.Length + 1; //+1 is for collection's null terminator which we'll be adding in a bit
            var length = 24 + payload1.Length + payload2.Length + collectionLength;
            var header = new byte[length - payload1.Length - payload2.Length];

            Buffer.BlockCopy(BitConverter.GetBytes(length), 0, header, 0, 4);
            Buffer.BlockCopy(_opBytes, 0, header, 12, 4);
            Buffer.BlockCopy(collection, 0, header, 20, collection.Length);
            Buffer.BlockCopy(BitConverter.GetBytes((int) _options), 0, header, 20 + collectionLength, 4);
            
            _connection.Write(header, 0, header.Length);
            _connection.Write(payload1, 0, payload1.Length);
            _connection.Write(payload2, 0, payload2.Length);
        }
    }
}