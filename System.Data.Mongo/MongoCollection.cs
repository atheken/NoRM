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


        /// <summary>
        /// Overload of Update that updates one document and doesn't upsert if no matches are found.
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="matchDocument"></param>
        /// <param name="valueDocument"></param>
        public void UpdateOne<X, U>(X matchDocument, U valueDocument)
        {
            this.Update(matchDocument, valueDocument, false, false);
        }


        /// <summary>
        /// Overload of Update that updates all matching documents, and doesn't upsert if no matches are found.
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="matchDocument"></param>
        /// <param name="valueDocument"></param>
        public void UpdateMultiple<X, U>(X matchDocument, U valueDocument)
        {
            this.Update(matchDocument, valueDocument, true, false);
        }


        /// <summary>
        /// Updates documents in the db.
        /// </summary>
        /// <typeparam name="X">A BSON serializable type.</typeparam>
        /// <typeparam name="U">A BSON serializable type.(more to this)</typeparam>
        /// <param name="matchDocument">A document that has the values that must match in order for the document to match.</param>
        /// <param name="valueDocument">A document that has the values that should be set on matching documents in the db.</param>
        /// <param name="updateMultiple">true if you want to update all documents that match, not just the first</param>
        /// <param name="upsert">true if you want to insert the value document if no matches are found.</param>
        public void Update<X, U>(X matchDocument, U valueDocument, bool updateMultiple, bool upsert)
        {
            UpdateOption ops = UpdateOption.None;
            if(updateMultiple)
            {
                ops |= UpdateOption.MultiUpdate;
            }
            if(upsert)
            {
                ops |= UpdateOption.Upsert;
            }

            var um = new UpdateMessage<X, U>(this._context, this.FullyQualifiedName, ops, matchDocument, valueDocument);
            um.Execute();
        }

        /// <summary>
        /// The name of this collection, including the database prefix.
        /// </summary>
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
        /// This will do a search on the collection using the specified template. 
        /// If no documents are found, default(T) will be returned.
        /// </summary>
        /// <typeparam name="U">A type that has each member set to the value to search. 
        /// Keep in mind that all the properties must either be concrete values, or the 
        /// special "Qualifier"-type values.</typeparam>
        /// <param name="template"></param>
        /// <returns>The first document that matched the template, or default(T)</returns>
        public T FindOne<U>(U template)
        {
            return this.Find(template, 1).FirstOrDefault();
        }

        public IEnumerable<T> Find<U>(U template)
        {
            return this.Find(template, Int32.MaxValue);
        }

        /// <summary>
        /// Get the documents that match the specified template.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="template"></param>
        /// <param name="limit">The number to return from this command.</param>
        /// <returns></returns>
        public IEnumerable<T> Find<U>(U template, int limit)
        {
            return Find(template, limit, this.FullyQualifiedName);
        }

        public IEnumerable<T> Find<U>(U template, int limit, string fullyQualifiedName)
        {
            var qm = new QueryMessage<T, U>(this._context, fullyQualifiedName);
            qm.NumberToTake = limit;
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
