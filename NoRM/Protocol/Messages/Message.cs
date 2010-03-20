using Norm.Protocol.Messages;
using Norm.Responses;

namespace Norm.Protocol
{
    /// <summary>
    /// The message.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The _collection.
        /// </summary>
        protected string _collection;

        /// <summary>
        /// The _connection.
        /// </summary>
        protected IConnection _connection;

        /// <summary>
        /// The _message length.
        /// </summary>
        protected int _messageLength;

        /// <summary>
        /// The _op.
        /// </summary>
        protected MongoOp _op = MongoOp.Message;

        /// <summary>
        /// The _request id.
        /// </summary>
        protected int _requestID;

        /// <summary>
        /// The _response id.
        /// </summary>
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

        // todo: not crazy about having this here, think I'm going to move this to MongoCollection        
        /// <summary>
        /// The assert has not error.
        /// </summary>
        protected void AssertHasNotError()
        {
            new QueryMessage<GenericCommandResponse, object>(_connection, _collection)
                {
                    NumberToTake = 1,
                    Query = new {getlasterror = 1d},
                }.Execute();
        }
    }
}