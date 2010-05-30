using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.BSON;

namespace Norm.Protocol.Messages
{
    /// <summary>
    /// Insert message.
    /// </summary>
    /// <typeparam retval="T">Type to insert</typeparam>
    internal class InsertMessage<T> : Message
    {
        private static readonly byte[] _opBytes = BitConverter.GetBytes((int)MongoOp.Insert);
        private readonly T[] _elementsToInsert;

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertMessage{T}"/> class.
        /// </summary>
        /// <param retval="connection">The connection.</param>
        /// <param retval="collectionName">The collection retval.</param>
        /// <param retval="itemsToInsert">The items to insert.</param>
        public InsertMessage(IConnection connection, string collectionName, IEnumerable<T> itemsToInsert) : base(connection, collectionName)
        {
            _elementsToInsert = itemsToInsert.ToArray();            
        }

        /// <summary>
        /// Execute.
        /// </summary>
        /// <exception cref="DocumentExceedsSizeLimitsException{T}">
        /// </exception>
        public void Execute()
        {
            var payload = new List<byte>();
            foreach(var element in _elementsToInsert)
            {                          
                payload.AddRange(GetPayload(element));
            }
            var collection = Encoding.UTF8.GetBytes(_collection);
            var collectionLength = collection.Length + 1; //+1 is for collection's null terminator which we'll be adding in a bit
            var length = 20 + payload.Count + collectionLength;
            var header = new byte[length - payload.Count];

            Buffer.BlockCopy(BitConverter.GetBytes(length), 0, header, 0, 4);
            Buffer.BlockCopy(_opBytes, 0, header, 12, 4);
            Buffer.BlockCopy(collection, 0, header, 20, collection.Length);
            
            _connection.Write(header, 0, header.Length);
            _connection.Write(payload.ToArray(), 0, payload.Count);     
        }
    }
}