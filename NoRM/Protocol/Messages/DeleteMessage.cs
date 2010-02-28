using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.BSON;

namespace NoRM.Protocol.Messages
{
    internal class DeleteMessage<U> : Message
    {

        /// <summary>
        /// Delete some docs from the database.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        internal DeleteMessage(IConnection connection, String collection, U templateDocument) : base(connection, collection)
        {
            this._templateDocument = templateDocument;
            this._op = MongoOp.Delete;
        }

        private U _templateDocument;

        internal void Execute()
        {
            List<byte[]> bytes = new List<byte[]>(8);
            bytes.Add(new byte[4]);
            bytes.Add(new byte[4]);
            bytes.Add(new byte[4]);
            bytes.Add(BitConverter.GetBytes((int)this._op));
            bytes.Add(new byte[4]);
            bytes.Add(Encoding.UTF8.GetBytes(this._collection).Concat(new byte[1]).ToArray());
            bytes.Add(new byte[4]);
            bytes.Add(BsonSerializer.Serialize(this._templateDocument));
            int size = bytes.Sum(j => j.Length);
            bytes[0] = BitConverter.GetBytes(size);

            _connection.GetStream().Write(bytes.SelectMany(y => y).ToArray(), 0, size);
            
        }
    }
}
