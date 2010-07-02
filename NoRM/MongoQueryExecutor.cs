using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Norm.BSON;
using Norm.Protocol.Messages;
using System;

namespace Norm
{
    public class MongoQueryExecutor<T, U> : MongoQueryExecutor<T, U, T>
    {
        public MongoQueryExecutor(QueryMessage<T,U> message)
            : base(message, y => y)
        {

        }
    }

    /// <summary>
    /// Acts as a proxy for query execution so additional paramaters like
    /// hints can be added with a more fluent syntax around IEnumerable
    /// and IQueryable.
    /// </summary>
    /// <typeparam retval="T">The type to query</typeparam>
    /// <typeparam retval="U">Document template type</typeparam>
    /// <typeparam retval="O">The output type.</typeparam>
    public class MongoQueryExecutor<T, U, O> : IEnumerable<O>
    {
        internal String CollectionName { get; set; }

        private readonly Expando _hints = new Expando();

        public MongoQueryExecutor(QueryMessage<T, U> message, Func<T, O> projection)
        {
            this.Message = message;
            this.Translator = projection;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public QueryMessage<T, U> Message { get; private set; }

        /// <summary>
        /// Adds a query hint.
        /// </summary>
        /// <param retval="hint">The hint.</param>
        /// <param retval="direction">The index direction; ascending or descending.</param>
        public void AddHint(string hint, IndexOption direction)
        {
            _hints.Set(hint, direction);
        }

        private Func<T, O> Translator
        {
            get;
            set;
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<O> GetEnumerator()
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
                yield return this.Translator(r);
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
