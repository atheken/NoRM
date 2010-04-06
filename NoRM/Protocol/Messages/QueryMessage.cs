
using Norm.Configuration;

namespace Norm.Protocol.Messages
{
    /// <summary>
    /// A query to the db.
    /// </summary>
    /// <typeparam name="T">
    /// The request document type, and the response document type.
    /// </typeparam>
    public class QueryMessage<T> : QueryMessage<T, T> where T : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryMessage{T}"/> class.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        public QueryMessage(IConnection connection) : base(connection, MongoConfiguration.GetCollectionName(typeof(T)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryMessage{T}"/> class.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        /// <param name="fullyQualifiedCollName">
        /// The fully qualified coll name.
        /// </param>
        public QueryMessage(IConnection connection, string fullyQualifiedCollName) : base(connection, fullyQualifiedCollName)
        {
        }
    }
}