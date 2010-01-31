using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Mongo
{
    public class MongoCollection<T> where T : class, new()
    {
        private String _collectionName;
        private MongoDatabase _db;
        private MongoContext _context;

        /// <summary>
        /// Represents a strongly-typed set of documents in the db.
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="db"></param>
        /// <param name="context"></param>
        public MongoCollection(String collectionName, MongoDatabase db, MongoContext context)
        {
            this._db = db;
            this._context = context;
            this._collectionName = collectionName;
        }

        /// <summary>
        /// Produces the set of documents in the collection from the 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<T> Find(String query)
        {
            return Enumerable.Empty<T>();
        }

    }
}
