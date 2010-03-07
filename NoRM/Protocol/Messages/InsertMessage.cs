using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.BSON;

namespace NoRM.Protocol.Messages
{
    internal class InsertMessage<T> : Message
    {
        private const int FOUR_MEGABYTES = 4 * 1024 * 1024;
        private readonly T[] _elementsToInsert;

        public InsertMessage(IConnection connection, String collectionName, IEnumerable<T> itemsToInsert)
            : base(connection, collectionName)
        {
            _elementsToInsert = itemsToInsert.ToArray();
            _op = MongoOp.Insert;
        }

        public void Execute()
        {
            var message = new List<byte[]>(_elementsToInsert.Length + 18)
                              {
                                  new byte[4], //allocate size for header
                                  new byte[4], //allocate requestID;
                                  new byte[4],  //allocate responseID;
                                  BitConverter.GetBytes((int) _op), 
                                  new byte[4],  //allocate zero - because the docs told me to.
                                  Encoding.UTF8.GetBytes(_collection).Concat(new byte[1]).ToArray(),
                              };

            foreach (var obj in _elementsToInsert)
            {
                var data = BsonSerializer.Serialize(obj);
                if (data.Length > InsertMessage<T>.FOUR_MEGABYTES)
                {
                    throw new DocumentExceedsSizeLimitsException<T>(obj, data.Length);
                }
                message.Add(data);
            }
            var size = message.Sum(y => y.Length);
            message[0] = BitConverter.GetBytes(size);

            var bytes = message.SelectMany(y => y).ToArray();
            _connection.Write(bytes, 0, size);
            
            if (_connection.StrictMode)
            {
                AssertHasNotError();
            }
        }
    }
}
