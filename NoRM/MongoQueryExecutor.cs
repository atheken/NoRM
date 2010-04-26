using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Norm.BSON;
using Norm.Protocol.Messages;

namespace Norm
{
    /// <summary>
    /// Acts as a proxy for query execution so additional paramaters like
    /// hints can be added with a more fluent syntax around IEnumerable
    /// and IQueryable.
    /// </summary>
    /// <typeparam name="T">The type to query</typeparam>
    /// <typeparam name="U">Document template type</typeparam>
    public class MongoQueryExecutor<T, U> : IEnumerable<T>
    {
        private readonly Expando _hints = new Expando();

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQueryExecutor&lt;T, U&gt;"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MongoQueryExecutor(QueryMessage<T, U> message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public QueryMessage<T, U> Message { get; private set; }

        /// <summary>
        /// Adds a query hint.
        /// </summary>
        /// <param name="hint">The hint.</param>
        /// <param name="direction">The index direction; ascending or descending.</param>
        public void AddHint(string hint, IndexOption direction)
        {
            _hints.Set(hint, direction);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            ReplyMessage<T> replyMessage;

            if (_hints.AllProperties().Count() == 0) // No hints - just run the query
            {
                replyMessage = Message.Execute();
            }
            else // Add hints.  Other commands can go here as needed.
            {
                var query = Message.Query;

                var queryWithHint = new Expando();
                queryWithHint["$query"] = query;
                queryWithHint["$hint"] = _hints;
                replyMessage = Message.Execute();
            }

            foreach (var r in replyMessage.Results)
            {
                yield return r;
            }

            yield break;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
