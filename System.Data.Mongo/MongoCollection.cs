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
        /// Delete the documents that mact the specified template.
        /// </summary>
        /// <typeparam name="U">a document that has properties 
        /// that match what you want to delete.</typeparam>
        /// <param name="template"></param>
        public void Delete<U>(U template)
        {
            var dm = new DeleteMessage<U>(this._context, this.FullyQualifiedName, template);
            dm.Execute();
        }

        /// <summary>
        /// Get the documents that match the specified template.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="template"></param>
        /// <returns></returns>
        public IEnumerable<T> Find<U>(U template)
        {
            var qm = new QueryMessage<T, U>(this._context, this.FullyQualifiedName);
            qm.Query = template;
            var reply = qm.Execute();

            //while (reply.ResultsReturned > 0 && !reply.HasError)
            //{
            foreach (var r in reply.Results)
            {
                yield return r;
            }
            //    var getMore = new GetMoreMessage<T>(this._context,
            //        this._collectionName, reply.CursorID);
            //    reply = getMore.Execute();
            //}
            yield break;
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
