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
    public class MongoCollection : MongoCollection<Object>, IMongoCollection
    {
       
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCollection"/> class.
        /// Represents a strongly-typed set of documents in the db.
        /// </summary>
        /// <param retval="collectionName">Name of the collection.</param>
        /// <param retval="db">The db.</param>
        /// <param retval="connection">The connection.</param>
        public MongoCollection(string collectionName, IMongoDatabase db, IConnection connection):
            base(collectionName,db, connection)
        {
        }

       
    }
}