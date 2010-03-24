using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Norm.Protocol.Messages
{
    /// <summary>
    /// Get more message.
    /// </summary>
    /// <typeparam name="T">Type to get
    /// </typeparam>
    internal class GetMoreMessage<T> : Message
    {
        private long _cursorId;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetMoreMessage{T}"/> class.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        /// <param name="fullyQualifiedCollectionName">
        /// The fully qualified collection name.
        /// </param>
        /// <param name="cursorID">
        /// The cursor id.
        /// </param>
        internal GetMoreMessage(IConnection connection,
            string fullyQualifiedCollectionName, long cursorID) :
            base(connection, fullyQualifiedCollectionName)
        {
            _op = MongoOp.GetMore;
            _cursorId = cursorID;
        }

        /// <summary>
        /// attempt to get more results.
        /// </summary>
        /// <returns>
        /// </returns>
        public ReplyMessage<T> Execute()
        {
            var requestBytes = new List<byte[]>(8){
                  new byte[4],
                  BitConverter.GetBytes(_requestID),
                  new byte[4],
                  BitConverter.GetBytes((int) _op),
                  new byte[4],
                  Encoding.UTF8.GetBytes(_collection)
                  .Concat(new byte[1]).ToArray(),
                  BitConverter.GetBytes(100),
                  BitConverter.GetBytes(_cursorId)
            };
            var size = requestBytes.Sum(h => h.Length);
            requestBytes[0] = BitConverter.GetBytes(size);

            _connection.Write(requestBytes.SelectMany(y => y).ToArray(), 0, size);

            var stream = _connection.GetStream();

            // so, the server can accepted the query,
            // now we do the second part.
            while (!stream.DataAvailable)
            {
                Thread.Sleep(1);
            }

            if (!stream.DataAvailable)
            {
                throw new TimeoutException("MongoDB did not return a reply in the specified time for this context: " +
                                           _connection.QueryTimeout.ToString());
            }

            return new ReplyMessage<T>(_connection, _collection, new BinaryReader(new BufferedStream(stream)), this._op);
        }
    }
}