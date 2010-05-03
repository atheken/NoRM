using System;
using System.Collections;
using Norm.BSON;
using Norm.Protocol.Messages;
using Norm.Responses;

namespace Norm.Collections
{
    /// <summary>
    /// The mongo collection.
    /// </summary>
    public class MongoCollection : MongoCollection<Object>
    {
       
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCollection"/> class.
        /// Represents a strongly-typed set of documents in the db.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="db">The db.</param>
        /// <param name="connection">The connection.</param>
        public MongoCollection(string collectionName, MongoDatabase db, IConnection connection):
            base(collectionName,db, connection)
        {
        }

       
    }
}