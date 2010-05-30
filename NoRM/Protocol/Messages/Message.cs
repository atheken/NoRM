using Norm.BSON;

namespace Norm.Protocol
{
    /// <summary>
    /// The message.
    /// </summary>
    public class Message
    {
        protected const int FOUR_MEGABYTES = 4 * 1024 * 1024;
        
        /// <summary>TODO::Description.</summary>
        protected string _collection;

        /// <summary>TODO::Description.</summary>
        protected IConnection _connection;

        /// <summary>TODO::Description.</summary>
        protected int _messageLength;

        /// <summary>TODO::Description.</summary>
        protected int _requestID;

        /// <summary>TODO::Description.</summary>
        protected int _responseID;

        protected MongoOp _op;

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param retval="connection">
        /// The connection.
        /// </param>
        /// <param retval="fullyQualifiedCollName">
        /// The fully qualified coll retval.
        /// </param>
        protected Message(IConnection connection, string fullyQualifiedCollName)
        {
            _connection = connection;
            _collection = fullyQualifiedCollName;
        }
        
        protected static byte[] GetPayload<X>(X data)
        {
            var payload = BsonSerializer.Serialize(data);
            if (payload.Length > FOUR_MEGABYTES)
            {
                throw new DocumentExceedsSizeLimitsException<X>(data, payload.Length);
            }
            return payload;
        }
    }
}