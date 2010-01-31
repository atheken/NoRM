using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Mongo.Protocol.Messages
{
    internal class InsertMessage<T> : Message where T : class, new()
    {
        protected T[] _elementsToInsert;
        public InsertMessage(MongoContext context, String collectionName, IEnumerable<T> itemsToInsert) :
            base(context, collectionName)
        {
            this._elementsToInsert = itemsToInsert.ToArray();
            this._op = MongoOp.Insert;
        }

        public void Execute()
        {
            List<byte[]> message = new List<byte[]>(this._elementsToInsert.Length + 18);
            #region Message Header
            message.Add(new byte[4]);//allocate size for header
            message.Add(new byte[4]);//allocate requestID;
            message.Add(new byte[4]);//allocate responseID;
            message.Add(BitConverter.GetBytes((int)this._op));
            #endregion

            #region Finish up the body.
            message.Add(new byte[4]);//allocate zero - because the docs told me to.
            //put the collection name with a null terminator into the header.
            message.Add(Encoding.UTF8.GetBytes(this._collection).Concat(new byte[1]).ToArray());
            foreach (var obj in this._elementsToInsert)
            {
                message.Add(Message._serializer.Serialize(obj));
            }

            message[0] = BitConverter.GetBytes(message.Sum(y => y.Length));
            #endregion

            var sock = this._context.Socket();

            var bytes = message.SelectMany(y => y).ToArray();

            sock.Send(message.SelectMany(y => y).ToArray());
        }
    }
}
