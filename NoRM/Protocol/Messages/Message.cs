using Norm.Protocol.Messages;
using Norm.Responses;

namespace Norm.Protocol
{
    /// <summary>
    /// The message.
    /// </summary>
    public class Message
    {
        /// <summary>TODO::Description.</summary>
        protected string _collection;

        /// <summary>TODO::Description.</summary>
        protected IConnection _connection;

        /// <summary>TODO::Description.</summary>
        protected int _messageLength;

        /// <summary>TODO::Description.</summary>
        protected MongoOp _op = MongoOp.Message;

        /// <summary>TODO::Description.</summary>
        protected int _requestID;

        /// <summary>TODO::Description.</summary>
        protected int _responseID;

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        /// <param name="fullyQualifiedCollName">
        /// The fully qualified coll name.
        /// </param>
        protected Message(IConnection connection, string fullyQualifiedCollName)
        {
            _connection = connection;
            _collection = fullyQualifiedCollName;
        }
    }
}