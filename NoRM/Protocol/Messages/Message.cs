using Norm.Protocol.Messages;
using Norm.Responses;

namespace Norm.Protocol
{
    /// <summary>
    /// The message.
    /// </summary>
    public class Message
    {
        protected string _collection;
        protected IConnection _connection;
        protected int _messageLength;
        protected MongoOp _op = MongoOp.Message;
        protected int _requestID;
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