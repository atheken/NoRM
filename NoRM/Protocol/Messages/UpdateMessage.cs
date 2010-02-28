using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.BSON;

namespace NoRM.Protocol.Messages
{
    internal class UpdateMessage<T,U> : Message
    {
        protected UpdateOption _options = UpdateOption.None;
        protected T _matchDocument;
        protected U _valueDocument;

        internal UpdateMessage(IConnection connection, String collection, UpdateOption options, T matchDocument, U valueDocument) : base(connection, collection)
        {
            this._options = options;
            this._matchDocument = matchDocument;
            this._valueDocument = valueDocument;
            this._op = MongoOp.Update;
        }

        public void Execute()
        {
            List<byte[]> message = new List<byte[]>(9);
            message.Add(new byte[4]);//allocate message length
            message.Add(new byte[4]);//allocate requestid
            message.Add(new byte[4]);//allocate responseid
            message.Add(BitConverter.GetBytes((int)this._op));//set message type.
            message.Add(new byte[4]);//required by docs
            message.Add(Encoding.UTF8.GetBytes(this._collection).Concat(new byte[1]).ToArray());
            message.Add(BitConverter.GetBytes((int)this._options));
            message.Add(BSONSerializer.Serialize(this._matchDocument));
            message.Add(BSONSerializer.Serialize(this._valueDocument));
            var size = message.Sum(y=>y.Length);
            message[0] = BitConverter.GetBytes(size);

            //write update to server.
            _connection.GetStream().Write(message.SelectMany(h => h).ToArray(), 0, size);
            }
    }
}
