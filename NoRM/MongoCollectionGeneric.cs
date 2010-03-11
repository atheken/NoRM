using System;
using System.Collections.Generic;
using System.Linq;
using NoRM.BSON;
using NoRM.Protocol.Messages;
using NoRM.Protocol.SystemMessages.Requests;
using NoRM.Protocol.SystemMessages.Responses;
using NoRM.Responses;

namespace NoRM
{
    using Commands.Modifiers;


    /// <summary>
    /// Mongo typed collection.
    /// </summary>
    /// <typeparam name="T">Collection type</typeparam>
    public class MongoCollection<T> : MongoCollection, IMongoCollection<T>
    {
        // this will have a different instance for each concrete version of MongoCollection<T>
        protected static bool? _updateable;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCollection{T}"/> class.
        /// Represents a strongly-typed set of documents in the db.
        /// </summary>
        /// <param name="collectionName">The collection Name.</param>
        /// <param name="db">The db.</param>
        /// <param name="connection">The connection.</param>
        public MongoCollection(string collectionName, MongoDatabase db, IConnection connection)
        {
            _db = db;
            _connection = connection;
            _collectionName = collectionName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCollection{T}"/> class.
        /// </summary>
        protected MongoCollection()
        {
        }

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
        /// <typeparam name="U">Type of collection</typeparam>
        /// <param name="collectionName">The collection Name.</param>
        /// <returns></returns>
        public MongoCollection<U> GetChildCollection<U>(string collectionName) where U : class, new()
        {
            return new MongoCollection<U>(_collectionName + "." + collectionName, _db, _connection);
        }

        /// <summary>
        /// Overload of Update that updates one document and doesn't upsert if no matches are found.
        /// </summary>
        /// <typeparam name="X">Document to match</typeparam>
        /// <typeparam name="U">Value document</typeparam>
        /// <param name="matchDocument">The match Document.</param>
        /// <param name="valueDocument">The value Document.</param>
        public void UpdateOne<X, U>(X matchDocument, U valueDocument)
        {
            Update(matchDocument, valueDocument, false, false);
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
            Update(matchDocument, valueDocument, true, false);
        }

        /// <summary>
        /// The update.
        /// </summary>
        /// <typeparam name="X">Document to match</typeparam>
        /// <typeparam name="U">Value document</typeparam>
        /// <param name="matchDocument">The match document.</param>
        /// <param name="valueDocument">The value document.</param>
        /// <param name="updateMultiple">The update multiple.</param>
        /// <param name="upsert">The upsert.</param>
        /// <exception cref="NotSupportedException">
        /// </exception>
        public void Update<X, U>(X matchDocument, U valueDocument, bool updateMultiple, bool upsert)
        {
            if (!this.Updateable)
            {
                throw new NotSupportedException(
                    "This collection is not updatable, this is due to the fact that the collection's type " +
                    typeof(T).FullName +
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

            var um = new UpdateMessage<X, U>(_connection, FullyQualifiedName, ops, matchDocument, valueDocument);
            um.Execute();
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
            return Find(template, 1).FirstOrDefault();
        }


        public void UpdateWithModifier<X>(X matchDocument, Action<IModifierExpression<T>> action)
        {
            UpdateWithModifier(matchDocument,action,false,false);

        }
        public void UpdateWithModifier<X>(X matchDocument, Action<IModifierExpression<T>> action,bool updateMultiple, bool upsert)
        {
            var modifierExpression = new ModifierExpression<T>();
            action(modifierExpression);
            if (matchDocument is ObjectId)
            {
                Update(new { _id = matchDocument }, modifierExpression.Fly,updateMultiple,upsert);
            }
            else
            {
                Update(matchDocument, modifierExpression.Fly, updateMultiple, upsert);

            }
        }

        /// <summary>
        /// Find objects in the collection without any qualifiers.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Find()
        {
            // this is a hack to get a value that will test for null into the serializer.
            return Find(new object(), Int32.MaxValue, FullyQualifiedName);
        }

        /// <summary>
        /// Return all documents matching the template
        /// </summary>
        /// <typeparam name="U">Type of document to find.</typeparam>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        /// <remarks>
        /// Ok, not all documents, just all documents up to Int32.MaxValue - if you bring that many back, you've crashed. Sorry.
        /// </remarks>
        public IEnumerable<T> Find<U>(U template)
        {
            return Find(template, Int32.MaxValue);
        }

        /// <summary>
        /// Get the documents that match the specified template.
        /// </summary>
        /// <typeparam name="U">Type of document to find.</typeparam>
        /// <param name="template">The template.</param>
        /// <param name="limit">The number to return from this command.</param>
        /// <returns></returns>
        public IEnumerable<T> Find<U>(U template, int limit)
        {
            return Find(template, limit, 0, FullyQualifiedName);
        }

        /// <summary>
        /// The find.
        /// </summary>
        /// <typeparam name="U">Type of document to find.</typeparam>
        /// <param name="template">The template.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="fullyQualifiedName">The fully qualified name.</param>
        /// <returns></returns>
        public IEnumerable<T> Find<U>(U template, int limit, string fullyQualifiedName)
        {
            return Find(template, limit, 0, fullyQualifiedName);
        }

        /// <summary>
        /// A count on this collection without any filter.
        /// </summary>
        /// <returns>The count.</returns>
        public long Count()
        {
            return Count(new { });
        }

        /// <summary>
        /// The get collection statistics.
        /// </summary>
        /// <returns></returns>
        public CollectionStatistics GetCollectionStatistics()
        {
            return _db.GetCollectionStatistics(_collectionName);
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
        /// Add an index for this collection.
        /// </summary>
        /// <typeparam name="U">A type that has the names of the items to be indexed, with a value of 1.0d or -1.0d depending on
        /// if you want the index to be ASC or DESC respectively.</typeparam>
        /// <param name="indexDefinition">The index Definition.</param>
        /// <param name="isUnique">The is Unique.</param>
        /// <param name="indexName">The index Name.</param>
        public void CreateIndex<U>(U indexDefinition, bool isUnique, string indexName)
        {
            var coll = _db.GetCollection<MongoIndex<U>>("system.indexes");
            coll.Insert(new MongoIndex<U>()
            {
                key = indexDefinition,
                ns = FullyQualifiedName,
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
            return _db.GetCollection<DistinctValuesResponse<U>>("$cmd")
                .FindOne(new { distinct = _collectionName, key = keyName }).Values;
        }

        /// <summary>
        /// Delete the documents that mact the specified template.
        /// </summary>
        /// <typeparam name="U">a document that has properties
        /// that match what you want to delete.</typeparam>
        /// <param name="template">The template.</param>
        public void Delete<U>(U template)
        {
            var dm = new DeleteMessage<U>(_connection, FullyQualifiedName, template);
            dm.Execute();
        }

        /// <summary>
        /// The find.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        public IEnumerable<T> Find(Flyweight template)
        {
            var limit = 0;
            var skip = 0;

            var hasLimit = template.TryGet<int>("$limit", out limit);
            //var hasSkip = template.TryGet<int>("$skip", out skip);

            if (!hasLimit)
            {
                limit = Int32.MaxValue;
            }

            template.Delete("$limit");
            template.Delete("$skip");

            return Find(template, limit, skip, FullyQualifiedName);
        }

        /// <summary>
        /// Finds documents
        /// </summary>
        /// <typeparam name="U">Type of document to find.</typeparam>
        /// <param name="template">The template.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="fullyQualifiedName">The fully qualified name.</param>
        /// <returns></returns>
        public IEnumerable<T> Find<U>(U template, int limit, int skip, string fullyQualifiedName)
        {
            var qm = new QueryMessage<T, U>(_connection, fullyQualifiedName)
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

        public ExplainResponse Explain<T>(T template)
        {
            var query = new Flyweight();
            query["$explain"] = true;
            query["$query"] = template;
            var explainPlan = new MongoCollection<ExplainResponse>(_collectionName, _db, _connection).Find(query);
            return explainPlan.FirstOrDefault();
        }

        //public ExplainResponse Explain(Flyweight template)
        //{
        //    var query = new Flyweight();
        //    query["$explain"] = true;
        //    query["$query"] = template;
        //    var explainPlan = new MongoCollection<ExplainResponse>(_collectionName, _db, _connection).Find(query);
        //    return explainPlan.FirstOrDefault();
        //}

        /// <summary>
        /// Inserts documents
        /// </summary>
        /// <param name="documentsToInsert">
        /// The documents to insert.
        /// </param>
        public void Insert(params T[] documentsToInsert)
        {
            Insert(documentsToInsert.AsEnumerable());
        }

        /// <summary>
        /// Constructs and returns a grouping of values based on initial values
        /// </summary>
        /// <typeparam name="X">Key</typeparam>
        /// <typeparam name="Y">Filter</typeparam>
        /// <typeparam name="Z">Initial value</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <param name="reduce">The reduce.</param>
        /// <returns>The group by.</returns>
        public object GroupBy<X, Y, Z>(X key, Y filter, Z initialValue, string reduce)
        {
            //TODO: implement it.
            return null;
        }

        /// <summary>
        /// A count using the specified filter.
        /// </summary>
        /// <typeparam name="U">Document type</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>The count.</returns>
        public long Count<U>(U query)
        {
            var f = _db.GetCollection<Flyweight>("$cmd")
                .FindOne(new
                {
                    count = _collectionName,
                    query = query
                });
            var retval = (long)f.Get<double>("n");
            return retval;
        }

        /// <summary>
        /// Inserts documents
        /// </summary>
        /// <param name="documentsToInsert">
        /// The documents to insert.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// </exception>
        public void Insert(IEnumerable<T> documentsToInsert)
        {
            if (!this.Updateable)
            {
                throw new NotSupportedException(
                    "This collection does not accept insertions, this is due to the fact that the collection's type " +
                    typeof(T).FullName +
                    " does not specify an identifier property");
            }

            var insertMessage = new InsertMessage<T>(_connection, FullyQualifiedName, documentsToInsert);
            insertMessage.Execute();
        }
    }
}
