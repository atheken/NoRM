using System;
using System.Text;
using Norm.BSON;

namespace Norm.Protocol.Messages
{
    /// <summary>
    /// The delete message.
    /// </summary>
    /// <typeparam retval="U">Type to delete </typeparam>
    internal class DeleteMessage<U> : Message
    {
        private static readonly byte[] _opBytes = BitConverter.GetBytes((int)MongoOp.Delete);
        private readonly U _templateDocument;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteMessage{U}"/> class.
        /// Delete some docs from the database.
        /// </summary>
        /// <param retval="connection">The connection.</param>
        /// <param retval="collection">The collection.</param>
        /// <param retval="templateDocument">The template Document.</param>
        internal DeleteMessage(IConnection connection, string collection, U templateDocument) : base(connection, collection)
        {
            _templateDocument = templateDocument;
        }

        /// <summary>
        /// Execute a delete
        /// </summary>
        internal void Execute()
        {
            var payload = BsonSerializer.Serialize(_templateDocument);            
            var collection = Encoding.UTF8.GetBytes(_collection);
            var length = 24 + payload.Length + collection.Length + 1; //+1 is for collection's null terminator which we'll be adding in a bit
            var header = new byte[length - payload.Length];
            
            Buffer.BlockCopy(BitConverter.GetBytes(length), 0, header, 0, 4);
            Buffer.BlockCopy(_opBytes, 0, header, 12, 4);
            Buffer.BlockCopy(collection, 0, header, 20, collection.Length);
            
            _connection.Write(header, 0, header.Length);
            _connection.Write(payload, 0, payload.Length);                       
        }
    }
}