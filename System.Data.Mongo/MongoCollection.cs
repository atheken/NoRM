using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Mongo.Protocol.Messages;

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

        public String FullyQualifiedName
        {
            get
            {
                return String.Format("{0}.{1}", this._db.DatabaseName, this._collectionName);
            }
        }

        /// <summary>
        /// Produces the set of documents in the collection from the 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<T> Find(T templateDocument)
        {
            var qm = new QueryMessage<T>(this._context, this.FullyQualifiedName);
            qm.Query = templateDocument;
            return qm.Execute().Results;
        }

        /// <summary>
        /// Insert these documents into the database.
        /// </summary>
        /// <exception cref="MongoError">Will return void if all goes well, of throw an exception otherwise.</exception>
        /// <param name="documentsToUpsert"></param>
        public void Insert(IEnumerable<T> documentsToInsert)
        {
            var insertMessage = new InsertMessage<T>
                (this._context, this.FullyQualifiedName, documentsToInsert);
            insertMessage.Execute();
        }
    }
}
