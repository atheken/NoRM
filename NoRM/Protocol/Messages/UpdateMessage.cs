using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.BSON;

namespace NoRM.Protocol.Messages
{
    /// <summary>
    /// The update message.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    internal class UpdateMessage<T, U> : Message
    {
        private const int FOUR_MEGABYTES = 4*1024*1024;
        protected T _matchDocument;
        protected UpdateOption _options = UpdateOption.None;
        protected U _valueDocument;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateMessage{T,U}"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="collection">The collection.</param>
        /// <param name="options">The options.</param>
        /// <param name="matchDocument">The match document.</param>
        /// <param name="valueDocument">The value document.</param>
        internal UpdateMessage(IConnection connection, string collection, UpdateOption options, T matchDocument, U valueDocument) 
            : base(connection, collection)
        {
            this._options = options;
            this._matchDocument = matchDocument;
            this._valueDocument = valueDocument;
            _op = MongoOp.Update;
        }

        /// <summary>
        /// Execute.
        /// </summary>
        /// <exception cref="DocumentExceedsSizeLimitsException{T}">
        /// </exception>
        /// <exception cref="DocumentExceedsSizeLimitsException{T}">
        /// </exception>
        public void Execute()
        {
            var message = new List<byte[]>(9)
                              {
                                  new byte[4],
                                  new byte[4],
                                  new byte[4],
                                  BitConverter.GetBytes((int) _op),
                                  new byte[4],
                                  Encoding.UTF8.GetBytes(_collection).Concat(new byte[1]).ToArray(),
                                  BitConverter.GetBytes((int) this._options)
                              };

            var matchData = BsonSerializer.Serialize(this._matchDocument);
            if (matchData.Length > FOUR_MEGABYTES)
            {
                throw new DocumentExceedsSizeLimitsException<T>(this._matchDocument, matchData.Length);
            }

            message.Add(matchData);

            var valueData = BsonSerializer.Serialize(this._valueDocument);
            message.Add(valueData);
            if (valueData.Length > FOUR_MEGABYTES)
            {
                throw new DocumentExceedsSizeLimitsException<U>(this._valueDocument, valueData.Length);
            }

            var size = message.Sum(y => y.Length);
            message[0] = BitConverter.GetBytes(size);

            // write update to server.            
            _connection.Write(message.SelectMany(h => h).ToArray(), 0, size);
        }
    }
}