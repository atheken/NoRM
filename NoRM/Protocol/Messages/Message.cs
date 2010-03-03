namespace NoRM.Protocol
{
    using Messages;
    using Responses;

    public class Message
    {
        protected MongoOp _op = MongoOp.Message;
        protected IConnection _connection;
        protected string _collection;
        protected int _requestID;
        protected int _responseID;
        protected int _messageLength;


        protected Message(IConnection connection, string fullyQualifiedCollName)
        {
            _connection = connection;
            _collection = fullyQualifiedCollName;
        }

        //todo: not crazy about having this here, think I'm going to move this to MongoCollection        
        protected void AssertHasNotError()
        {
            new QueryMessage<GenericCommandResponse, object>(_connection, _collection)
                         {
                             NumberToTake = 1,
                             Query = new { getlasterror = 1d },
                         }.Execute();
        }
    }
}
