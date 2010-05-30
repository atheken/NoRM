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
        /// <param retval="message">
        /// The message.
        /// </param>
        public MongoException(string message) : base(message)
        {
        }
    }
}