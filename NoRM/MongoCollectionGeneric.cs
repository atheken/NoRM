using System;
using System.Collections.Generic;
using System.Linq;
using NoRM.BSON;
using NoRM.Protocol.Messages;
using NoRM.Protocol.SystemMessages.Requests;
using NoRM.Responses;

namespace NoRM
{
    /// <summary>
    /// Strongly typed mongo collection
    /// </summary>
    /// <typeparam name="T">Type of collection</typeparam>
    public class MongoCollection<T> : MongoCollection, IMongoCollection<T>
    {
        // This will have a different instance for each concrete version of MongoCollection<T>
        protected static bool? _updateable;

        /// <summary>
        /// Mongo key/value grouping
        /// </summary>
        /// <typeparam name="K">Key type</typeparam>
        /// <typeparam name="V">Value type</typeparam>
        public interface IMongoGrouping<K, V>
        {
            /// <summary>
            /// Gets or sets the key.
            /// </summary>
            /// <value>The key.</value>
            K Key { get; set; }
            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>The value.</value>
            V Value { get; set; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCollection&lt;T&gt;"/> class.
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
        /// Initializes a new instance of the <see cref="MongoCollection&lt;T&gt;"/> class.
        /// </summary>
        protected MongoCollection()
        { }

        /// <summary>
        /// True if the type of this collection can be updated 
        /// (i.e. the Type specifies "_id", "ID", or a property with the attributed "MongoIdentifier").
        /// </summary>
        public bool Updateable
        {
            get
            {
                if (_updateable == null)
                {
                    _updateable = TypeHelper.GetHelperForType(typeof(T)).FindIdProperty() != null;
                }
                return _updateable.Value;
            }
        }

        /// <summary>
        /// Get a child collection of the specified type.
        /// </summary>
        /// <typeparam name="U">Child collectino type</typeparam>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        public MongoCollection<U> GetChildCollection<U>(string collectionName) where U : class, new()
        {
            return new MongoCollection<U>(_collectionName + "." + collectionName, _db, _connection);
        }

        /// <summary>
        /// Attempts to save or update an instance
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <remarks>
        /// Only works when the Id property is of type ObjectId
        /// </remarks>
        public void Save(T entity)
        {
            var helper = TypeHelper.GetHelperForType(typeof(T));
            var idProperty = helper.FindIdProperty();
            if (idProperty == null)
            {
                throw new MongoException("Save can only be called on an entity with a property named Id or one marked with the MongoIdentifierAttribute");
            }
            var id = idProperty.Getter(entity);
            Update(new { Id = id }, entity, false, true);
        }

        /// <summary>
        /// Overload of Update that updates one document and doesn't upsert if no matches are found.
        /// </summary>
        /// <typeparam name="X">Document to match</typeparam>
        /// <typeparam name="U">Value document</typeparam>
        /// <param name="matchDocument">The match document.</param>
        /// <param name="valueDocument">The value document.</param>
        public void UpdateOne<X, U>(X matchDocument, U valueDocument)
        {
            Update(matchDocument, valueDocument, false, false);
        }

        /// <summary>
        /// Add an index for this collection.
        /// </summary>
        /// <typeparam name="U">A type that has the names of the items to be indexed, with a value of 1.0d or -1.0d depending on
        /// if you want the index to be ASC or DESC respectively.</typeparam>
        /// <param name="indexDefinition">The index definition.</param>
        /// <param name="isUnique">if set to <c>true</c> [is unique].</param>
        /// <param name="indexName">Name of the index.</param>
        public void CreateIndex<U>(U indexDefinition, bool isUnique, string indexName)
        {
            var coll = _db.GetCollection<MongoIndex<U>>("system.indexes");
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
        /// <param name="keyName">Name of the key.</param>
        /// <returns></returns>
        public IEnumerable<U> Distinct<U>(string keyName) where U : class, new()
        {
            return this._db.GetCollection<DistinctValuesResponse<U>>("$cmd")
                .FindOne(new { distinct = this._collectionName, key = keyName }).Values;
        }

        /// <summary>
        /// Overload of Update that updates all matching documents, and doesn't upsert if no matches are found.
        /// </summary>
        /// <typeparam name="X">Document to match</typeparam>
        /// <typeparam name="U">Value document</typeparam>
        /// <param name="matchDocument">The match document.</param>
        /// <param name="valueDocument">The value document.</param>
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
        /// <exception cref="NotSupportedException">This exception will be raised if the collection's type "T" doesn't define an indentifier.</exception>
        /// <exception cref="DocumentExceedsSizeLimitsException<T>">Will be thrown if any document is larger than the size allowed by MongoDB (currently, 4MB - this includes all the BSON overhead, too.)</exception>
        public void Update<X, U>(X matchDocument, U valueDocument, bool updateMultiple, bool upsert)
        {
            if (!this.Updateable)
            {
                throw new NotSupportedException("This collection is not updatable, this is due to the fact that the collection's type " + typeof(T).FullName +
                    " does not specify an identifier property");
            }
            
            var ops = UpdateOption.None;
            if (updateMultiple)
            {
                ops |= UpdateOption.MultiUpdate;
            }
            if (upsert)
            {
                ops |= UpdateOption.Upsert;
            }

            var um = new UpdateMessage<X, U>(this._connection, this.FullyQualifiedName, ops, matchDocument, valueDocument);
            um.Execute();
        }

        /// <summary>
        /// Delete the documents that mact the specified template.
        /// </summary>
        /// <typeparam name="U">a document that has properties
        /// that match what you want to delete.</typeparam>
        /// <param name="template">The template.</param>
        public void Delete<U>(U template)
        {
            var dm = new DeleteMessage<U>(this._connection, this.FullyQualifiedName, template);
            dm.Execute();
        }

        /// <summary>
        /// This will do a search on the collection using the specified template.
        /// If no documents are found, default(T) will be returned.
        /// </summary>
        /// <typeparam name="U">A type that has each member set to the value to search.
        /// Keep in mind that all the properties must either be concrete values, or the
        /// special "Qualifier"-type values.</typeparam>
        /// <param name="template">The template.</param>
        /// <returns>
        /// The first document that matched the template, or default(T)
        /// </returns>
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
            // this is a hack to get a value that will test for null into the serializer.
            return this.Find(new object(), Int32.MaxValue, this.FullyQualifiedName);
        }

        /// <summary>
        /// Return all documents matching the template
        /// </summary>
        /// <typeparam name="U">Document template type</typeparam>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        /// <remarks>
        /// Ok, not all documents, just all documents up to Int32.MaxValue - if you bring that many back, you've crashed. Sorry.
        /// </remarks>
        public IEnumerable<T> Find<U>(U template)
        {
            return this.Find(template, Int32.MaxValue);
        }

        /// <summary>
        /// Finds the specified template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        public IEnumerable<T> Find(Flyweight template)
        {
            var limit = 0;
            var skip = 0;

            var hasLimit = template.TryGet("$limit", out limit);
            //var hasSkip = template.TryGet("$skip", out skip);

            if (!hasLimit)
            {
                limit = Int32.MaxValue;
            }

            template.Delete("$limit");
            template.Delete("$skip");

            return this.Find(template, limit, skip, this.FullyQualifiedName);
        }

        /// <summary>
        /// Get the documents that match the specified template.
        /// </summary>
        /// <typeparam name="U">Document template type</typeparam>
        /// <param name="template">Document</param>
        /// <param name="limit">The number to return from this command.</param>
        /// <returns></returns>
        public IEnumerable<T> Find<U>(U template, int limit)
        {
            return this.Find(template, limit, 0, this.FullyQualifiedName);
        }

        /// <summary>
        /// Finds the specified template.
        /// </summary>
        /// <typeparam name="U">Document template type</typeparam>
        /// <param name="template">The template.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="fullyQualifiedName">Name of the fully qualified.</param>
        /// <returns></returns>
        public IEnumerable<T> Find<U>(U template, int limit, string fullyQualifiedName)
        {
            return this.Find(template, limit, 0, fullyQualifiedName);
        }

        /// <summary>
        /// Finds the specified template.
        /// </summary>
        /// <typeparam name="U">Document template type</typeparam>
        /// <param name="template">The template.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="fullyQualifiedName">Name of the fully qualified.</param>
        /// <returns></returns>
        public IEnumerable<T> Find<U>(U template, int limit, int skip, string fullyQualifiedName)
        {
            var qm = new QueryMessage<T, U>(this._connection, fullyQualifiedName)
                         {
                             NumberToTake = limit,
                             NumberToSkip = skip,
                             Query = template
                         };
            var reply = qm.Execute();

            foreach (var r in reply.Results)
            {
                yield return r;
            }
            yield break;
        }

        /// <summary>
        /// Inserts the specified documents to insert.
        /// </summary>
        /// <param name="documentsToInsert">The documents to insert.</param>
        public void Insert(params T[] documentsToInsert)
        {
            this.Insert(documentsToInsert.AsEnumerable());
        }

        /// <summary>
        /// Constructs and returns a grouping of values based on initial values
        /// </summary>
        /// <typeparam name="X">Key type</typeparam>
        /// <typeparam name="Y">Filter type</typeparam>
        /// <typeparam name="Z">Value type</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <param name="reduce">The reduce.</param>
        /// <returns></returns>
        public object GroupBy<X, Y, Z>(X key, Y filter, Z initialValue, string reduce)
        {
            return null;
        }

        /// <summary>
        /// A count on this collection without any filter.
        /// </summary>
        /// <returns></returns>
        public long Count()
        {
            return this.Count(new { });
        }

        /// <summary>
        /// A count using the specified filter.
        /// </summary>
        /// <typeparam name="U">type to count</typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public long Count<U>(U query)
        {
            var f = this._db.GetCollection<Flyweight>("$cmd")
                .FindOne(new
                {
                    count = this._collectionName,
                    query = query
                });
            var retval = (long)f.Get<double>("n");
            return retval;
        }

        /// <summary>
        /// Insert these documents into the database.
        /// </summary>
        /// <exception cref="MongoError">Will return void if all goes well, of throw an exception otherwise.</exception>
        /// <exception cref="DocumentExceedsSizeLimitsException<T>">Will be thrown if any document is larger than the size allowed by MongoDB (currently, 4MB - this includes all the BSON overhead, too.)</exception>
        /// <param name="documentsToUpsert"></param>
        public void Insert(IEnumerable<T> documentsToInsert)
        {
            if (!this.Updateable)
            {
                throw new NotSupportedException("This collection does not accept insertions, this is due to the fact that the collection's type " + typeof(T).FullName +
                    " does not specify an identifier property");
            }
            var insertMessage = new InsertMessage<T>(this._connection, this.FullyQualifiedName, documentsToInsert);
            insertMessage.Execute();
        }

        /// <summary>
        /// Gets the collection statistics.
        /// </summary>
        /// <returns></returns>
        public CollectionStatistics GetCollectionStatistics()
        {
            return this._db.GetCollectionStatistics(this._collectionName);
        }
    }
}
