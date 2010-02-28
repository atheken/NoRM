namespace NoRM
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Indicates an issue with some part of the messaging between C# and the MongoDB.
    /// </summary>
    public class MongoException : Exception
    {
        public MongoException(){}
        public MongoException(string message) : base(message){}
        public MongoException(string message, Exception innerException) : base(message, innerException){}
        protected MongoException(SerializationInfo info, StreamingContext context) : base(info, context){}
    }
}
