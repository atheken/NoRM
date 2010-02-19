using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net.Sockets;

namespace NoRM.Protocol.Messages
{
    internal class GetMoreMessage<T> : Message where T : class, new()
    {
        protected long _cursorID;
        internal GetMoreMessage(MongoContext context,
            String fullyQualifiedCollectionName, long cursorID) :
            base(context, fullyQualifiedCollectionName)
        {
            this._op = MongoOp.GetMore;
            this._cursorID = cursorID;
        }

        /// <summary>
        /// attempt to get more results.
        /// </summary>
        /// <returns></returns>
        public ReplyMessage<T> Execute()
        {
            List<byte[]> requestBytes = new List<byte[]>(8);
            requestBytes.Add(new byte[4]);//allocate size;
            requestBytes.Add(BitConverter.GetBytes(this._requestID));//allocate requestID;
            requestBytes.Add(new byte[4]);//allocate responseID;
            requestBytes.Add(BitConverter.GetBytes((int)this._op));
            requestBytes.Add(new byte[4]);//allocate ZERO because we were told to.
            requestBytes.Add(Encoding.UTF8.GetBytes(this._collection)
                .Concat(new byte[1]).ToArray());
            requestBytes.Add(BitConverter.GetBytes(100));//number to return.
            requestBytes.Add(BitConverter.GetBytes(this._cursorID));
            int size = requestBytes.Sum(h => h.Length);
            requestBytes[0] = BitConverter.GetBytes(size);

            this._context.ServerConnection().GetStream().Write(requestBytes.SelectMany(h => h).ToArray(), 0, size);

            var stream = this._context.ServerConnection().GetStream();

            // so, the server can accepted the query,
            // now we do the second part.
            int timeout = this._context.QueryTimeout;
            while (!stream.DataAvailable && timeout > 0)
            {
                timeout--;
                Thread.Sleep(1000);
            }
            if (this._context.ServerConnection().Available == 0)
            {
                throw new TimeoutException("MongoDB did not return a reply in the specified time for this context: " + this._context.QueryTimeout.ToString());
            }

            return new ReplyMessage<T>(this._context, this._collection, new BinaryReader(new BufferedStream(stream)));

        }
    }
}
