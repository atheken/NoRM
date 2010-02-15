using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoSharp.Protocol.Messages;
using MongoSharp.Protocol.SystemMessages.Requests;
using MongoSharp.Protocol.SystemMessages.Responses;

namespace MongoSharp
{
    public class MongoCollection<T> where T : class, new()
    {
        private String _collectionName;
        private MongoDatabase _db;
        private MongoServer _server;

        /// <summary>
        /// Represents a strongly-typed set of documents in the db.
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="db"></param>
        /// <param name="context"></param>
        public MongoCollection(String collectionName, MongoDatabase db, MongoServer server)
        {
            this._db = db;
            this._server = server;
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
        /// Add an index for this collection.
        /// </summary>
        /// <typeparam name="U">A type that has the names of the items to be indexed, with a value of 1.0d or -1.0d depending on 
        /// if you want the index to be ASC or DESC respectively.</typeparam>
        /// <param name="indexDefinition"></param>
        /// <param name="isUnique"></param>
        /// <param name="indexName"></param>
        public void CreateIndex<U>(U indexDefinition, bool isUnique, String indexName)
        {
            var coll = this._db.GetCollection<MongoIndex<U>>("system.indexes");
            coll.Insert(new MongoIndex<U>()
            {
                key = indexDefinition,
                ns = this.FullyQualifiedName,
                name = indexName,
                unique = isUnique
            });

        }

        /// <summary>
        /// Gets the distinct values for the specified key.
        /// </summary>
        /// <typeparam name="U">You better know that every value that could come back 
        /// is of this type, or BAD THINGS will happen.</typeparam>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public IEnumerable<U> Distinct<U>(String keyName) where U : class, new()
        {
            return this._db.GetCollection<DistinctValuesResponse<U>>("$cmd")
                .FindOne(new { distinct = this._collectionName, key = keyName }).Values;
        }

        /// <summary>
        /// Deletes all indices on this collection.
        /// </summary>
        /// <param name="numberDeleted"></param>
        /// <returns></returns>
        public bool DeleteIndices(out int numberDeleted)
        {
            return this.DeleteIndex("*", out numberDeleted);
        }

        /// <summary>
        /// Deletes the specified index for the collection.
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="numberDeleted"></param>
        /// <returns></returns>
        public bool DeleteIndex(String indexName, out int numberDeleted)
        {
            bool retval = false;
            var coll = this._db.GetCollection<DeleteIndicesResponse>("$cmd");
            var result = coll.FindOne(new { deleteIndexes = this._collectionName, index = indexName });
            numberDeleted = 0;

            if (result != null && result.OK == 1.0)
            {
                retval = true;
                numberDeleted = result.NIndexesWas.Value;
            }

            return retval;
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
            if (updateMultiple)
            {
                ops |= UpdateOption.MultiUpdate;
            }
            if (upsert)
            {
                ops |= UpdateOption.Upsert;
            }

            var um = new UpdateMessage<X, U>(this._server, this.FullyQualifiedName, ops, matchDocument, valueDocument);
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
            var dm = new DeleteMessage<U>(this._server, this.FullyQualifiedName, template);
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

        /// <summary>
        /// Find objects in the collection without any qualifiers.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Find()
        {
            //this is a hack to get a value that will test for null into the serializer.
            return this.Find(new Object(), Int32.MaxValue, this.FullyQualifiedName);
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
            var qm = new QueryMessage<T, U>(this._server, fullyQualifiedName);
            qm.NumberToTake = limit;
            qm.Query = template;
            var reply = qm.Execute();

            foreach (var r in reply.Results)
            {
                yield return r;
            }
            yield break;
        }

        public void Insert(params T[] documentsToInsert)
        {
            this.Insert(documentsToInsert.AsEnumerable());
        }


        /// <summary>
        /// Insert these documents into the database.
        /// </summary>
        /// <exception cref="MongoError">Will return void if all goes well, of throw an exception otherwise.</exception>
        /// <param name="documentsToUpsert"></param>
        public void Insert(IEnumerable<T> documentsToInsert)
        {
            var insertMessage = new InsertMessage<T>
                (this._server, this.FullyQualifiedName, documentsToInsert);
            insertMessage.Execute();
        }

        public CollectionStatistics GetStatistics()
        {
            var response = this._server.GetDatabase(this._db.DatabaseName)
                .GetCollection<CollectionStatistics>("$cmd")
                .FindOne<CollectionStatistics>(new CollectionStatistics() { collstats = this._collectionName });


            return response;

        }
    }
}
