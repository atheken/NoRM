using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.BSON;

namespace NoRM.Protocol.Messages
{
    /// <summary>
    /// The delete message.
    /// </summary>
    /// <typeparam name="U">Type to delete </typeparam>
    internal class DeleteMessage<U> : Message
    {
        private readonly U _templateDocument;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteMessage{U}"/> class.
        /// Delete some docs from the database.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="collection">The collection.</param>
        /// <param name="templateDocument">The template Document.</param>
        internal DeleteMessage(IConnection connection, string collection, U templateDocument) : base(connection, collection)
        {
            _templateDocument = templateDocument;
            _op = MongoOp.Delete;
        }

        /// <summary>
        /// Execute a delete
        /// </summary>
        internal void Execute()
        {
            var bytes = new List<byte[]>(8)
                            {
                                new byte[4],
                                new byte[4],
                                new byte[4],
                                BitConverter.GetBytes((int) _op),
                                new byte[4],
                                Encoding.UTF8.GetBytes(_collection).Concat(new byte[1]).ToArray(),
                                new byte[4],
                                BsonSerializer.Serialize(_templateDocument)
                            };
            var size = bytes.Sum(j => j.Length);
            bytes[0] = BitConverter.GetBytes(size);

            _connection.Write(bytes.SelectMany(y => y).ToArray(), 0, size);
        }
    }
}