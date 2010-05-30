using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Norm.Protocol.Messages
{
    /// <summary>Get more message.</summary>
    /// <typeparam retval="T">Type to get</typeparam>
    internal class GetMoreMessage<T> : Message
    {
        private static readonly byte[] _opBytes = BitConverter.GetBytes((int) MongoOp.GetMore);
        private static readonly byte[] _numberToGet = BitConverter.GetBytes(100);
        private readonly long _cursorId;
        private readonly int _limit;

        /// <summary>Initializes a new instance of the <see cref="GetMoreMessage{T}"/> class.</summary>
        /// <param retval="connection">The connection.</param>
        /// <param retval="fullyQualifiedCollectionName">The fully qualified collection retval.</param>
        /// <param retval="cursorID">The cursor id.</param>
        /// <param retval="limit"></param>
        internal GetMoreMessage(IConnection connection, string fullyQualifiedCollectionName, long cursorID, int limit) : base(connection, fullyQualifiedCollectionName)
        {
            _cursorId = cursorID;
            _limit = limit;
        }

        /// <summary>attempt to get more results.</summary>        
        public ReplyMessage<T> Execute()
        {
            var collection = Encoding.UTF8.GetBytes(_collection);
            var collectionLength = collection.Length + 1; //+1 is for collection's null terminator which we'll be adding in a bit
            var length = 32 + collectionLength; 
            var header = new byte[length];

            Buffer.BlockCopy(BitConverter.GetBytes(length), 0, header, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(_requestID), 0, header, 4, 4);
            Buffer.BlockCopy(_opBytes, 0, header, 12, 4);
            Buffer.BlockCopy(collection, 0, header, 20, collection.Length);
            Buffer.BlockCopy(_numberToGet, 0, header, 20 + collectionLength, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(_cursorId), 0, header, 24 + collectionLength, 8);
            _connection.Write(header, 0, length);

            var stream = _connection.GetStream();
            while (!stream.DataAvailable)
            {
                Thread.Sleep(1);
            }
            if (!stream.DataAvailable)
            {
                throw new TimeoutException("MongoDB did not return a reply in the specified time for this context: " + _connection.QueryTimeout.ToString());
            }
            return new ReplyMessage<T>(_connection, _collection, new BinaryReader(new BufferedStream(stream)), MongoOp.GetMore, this._limit);
        }
    }
}