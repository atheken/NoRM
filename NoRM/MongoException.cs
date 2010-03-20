using System;
using System.Runtime.Serialization;

namespace Norm
{
    /// <summary>
    /// Indicates an issue with some part of the messaging between C# and the MongoDB.
    /// </summary>
    public class MongoException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoException"/> class.
        /// </summary>
        public MongoException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public MongoException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        public MongoException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoException"/> class.
        /// </summary>
        /// <param name="info">
        /// The info.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        protected MongoException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}