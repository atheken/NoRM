using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NoRM.BSON;
using NoRM.Protocol.Messages;
using NoRM.Responses;

namespace NoRM
{
    /// <summary>
    /// The mongo collection.
    /// </summary>
    public class MongoCollection : IMongoCollection
    {
        protected string _collectionName;
        protected IConnection _connection;
        protected MongoDatabase _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCollection"/> class.
        /// </summary>
        protected MongoCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCollection"/> class.
        /// Represents a strongly-typed set of documents in the db.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="db">The db.</param>
        /// <param name="connection">The connection.</param>
        public MongoCollection(string collectionName, MongoDatabase db, IConnection connection)
        {
            _db = db;
            _connection = connection;
            _collectionName = collectionName;
        }

        /// <summary>
        /// The document count.
        /// </summary>
        /// <returns>The count.</returns>
        public long Count()
        {
            return Count(new {});
        }

        /// <summary>
        /// The document count.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The count.</returns>
        public long Count(object query)
        {
            long retval = 0;

            var f = _db.GetCollection<Flyweight>("$cmd")
                .FindOne(new {count = _collectionName, query = query});

            if (f != null)
            {
                retval = (long) f.Get<double>("n");
            }

            return retval;
        }

        /// <summary>
        /// The document group.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The group.</returns>
        public object Group(object query)
        {
            //long retval = 0;

            var f = _db.GetCollection<Flyweight>("$cmd")
                .FindOne(new {group = _collectionName, query = query});

            return null;
        }

        /// <summary>
        /// Deletes all indices on this collection.
        /// </summary>
        /// <param name="numberDeleted">
        /// </param>
        /// <returns>
        /// The delete indices.
        /// </returns>
        public bool DeleteIndices(out int numberDeleted)
        {
            return DeleteIndex("*", out numberDeleted);
        }

        /// <summary>
        /// Deletes the specified index for the collection.
        /// </summary>
        /// <param name="indexName">
        /// </param>
        /// <param name="numberDeleted">
        /// </param>
        /// <returns>
        /// The delete index.
        /// </returns>
        public bool DeleteIndex(string indexName, out int numberDeleted)
        {
            var retval = false;
            var coll = _db.GetCollection<DeleteIndicesResponse>("$cmd");
            var result = coll.FindOne(new {deleteIndexes = _collectionName, index = indexName});
            numberDeleted = 0;

            if (result != null && result.OK == 1.0)
            {
                retval = true;
                numberDeleted = result.NIndexesWas.Value;
            }

            return retval;
        }

        /// <summary>
        /// The name of this collection, including the database prefix.
        /// </summary>
        public string FullyQualifiedName
        {
            get { return string.Format("{0}.{1}", _db.DatabaseName, _collectionName); }
        }

        /// <summary>
        /// The get collection statistics.
        /// </summary>
        /// <returns>
        /// </returns>
        public CollectionStatistics GetCollectionStatistics()
        {
            return _db.GetCollectionStatistics(_collectionName);
        }

        /// <summary>
        /// Finds one document.
        /// </summary>
        /// <param name="template">The document template.</param>
        /// <returns>The find one.</returns>
        public object FindOne(object template)
        {
            return Find(template, 1);
        }

        /// <summary>
        /// Find objects in the collection without any qualifiers.
        /// </summary>
        /// <returns>
        /// </returns>
        public IEnumerable Find()
        {
            // this is a hack to get a value that will test for null into the serializer.
            return Find(new object(), Int32.MaxValue, FullyQualifiedName);
        }

        /// <summary>
        /// Finds documents.
        /// </summary>
        /// <param name="template">The document template.</param>
        /// <returns></returns>
        public IEnumerable Find(object template)
        {
            return Find(template, Int32.MaxValue);
        }

        /// <summary>
        /// Get the documents that match the specified template.
        /// </summary>
        /// <param name="template">The document template.</param>
        /// <param name="limit">The number to return from this command.</param>
        /// <returns></returns>
        public IEnumerable Find(object template, int limit)
        {
            return Find(template, limit, FullyQualifiedName);
        }

        /// <summary>
        /// Finds documents.
        /// </summary>
        /// <param name="template">The document template.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="fullyQualifiedName">The fully qualified name.</param>
        /// <returns></returns>
        public IEnumerable Find(object template, int limit, string fullyQualifiedName)
        {
            var qm = new QueryMessage<object, object>(this._db.CurrentConnection, fullyQualifiedName)
                         {
                             NumberToTake = limit,
                             Query = template
                         };
            var reply = qm.Execute();

            foreach (var r in reply.Results)
            {
                yield return r;
            }

            yield break;
        }
    }
}