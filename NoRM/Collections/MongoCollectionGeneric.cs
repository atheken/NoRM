using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Norm.BSON;
using Norm.Configuration;
using Norm.Linq;
using Norm.Protocol;
using Norm.Protocol.Messages;
using Norm.Protocol.SystemMessages.Requests;
using Norm.Responses;
using TypeHelper = Norm.BSON.TypeHelper;
using Norm.Commands.Modifiers;

namespace Norm.Collections
{
    /// <summary>
    /// Mongo typed collection.
    /// </summary>
    /// <typeparam name="T">Collection type</typeparam>
    public partial class MongoCollection<T> : IMongoCollection<T>
    {
        /// <summary>
        /// This will have a different instance for each concrete version of <see cref="MongoCollection{T}"/>
        /// </summary>
        protected static bool? _updateable;

        /// <summary>TODO::Description.</summary>
        protected string _collectionName;

        /// <summary>TODO::Description.</summary>
        protected IConnection _connection;

        /// <summary>TODO::Description.</summary>
        protected MongoDatabase _db;

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
            //_queryContext = new MongoQuery<T>(MongoQueryProvider.Create(_connection.ConnectionString), _collectionName);
        }

        /// <summary>
        /// True if the type of this collection can be updated 
        /// (i.e. the Type specifies "_id", "ID", or a property with the attributed "MongoIdentifier").
        /// </summary>
        public bool Updateable
        {
            get
            {
                if (CanUpdateWithoutId(typeof(T)))
                {
                    return true;
                }

                if (_updateable == null)
                {
                    _updateable = TypeHelper.GetHelperForType(typeof(T)).FindIdProperty() != null;
                }

                return _updateable.Value;
            }
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
            AssertUpdatable();

            var helper = TypeHelper.GetHelperForType(typeof(T));
            var idProperty = helper.FindIdProperty();
            var id = idProperty.Getter(entity);
            if (id == null && typeof(ObjectId).IsAssignableFrom(idProperty.Type))
            {
                Insert(entity);
            }
            else
            {
                Update(new { Id = id }, entity, false, true);
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
            AssertUpdatable();

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
        /// The name of this collection, including the database prefix.
        /// </summary>
        public string FullyQualifiedName
        {
            get { return string.Format("{0}.{1}", _db.DatabaseName, _collectionName); }
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
            var result = coll.FindOne(new { deleteIndexes = _collectionName, index = indexName });
            numberDeleted = 0;

            if (result != null && result.WasSuccessful)
            {
                retval = true;
                numberDeleted = result.NumberIndexesWas.Value;
            }

            return retval;
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

        /// <summary>Allows a document to be updated using the specified action.</summary>
        public void Update<X>(X matchDocument, Action<IModifierExpression<T>> action)
        {
            Update(matchDocument, action, false, false);

        }


        /// <summary>TODO::Description.</summary>
        public void Update<X>(X matchDocument, Action<IModifierExpression<T>> action, bool updateMultiple, bool upsert)
        {
            var modifierExpression = new ModifierExpression<T>();
            action(modifierExpression);
            if (matchDocument is ObjectId)
            {
                Update(new { _id = matchDocument }, modifierExpression.Expression, updateMultiple, upsert);
            }
            else
            {
                Update(matchDocument, modifierExpression.Expression, updateMultiple, upsert);

            }
        }

        /// <summary>
        /// Find objects in the collection without any qualifiers.
        /// </summary>
        /// <returns></returns>
        new public IEnumerable<T> Find()
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

        /// <summary>Finds the documents matching the template, an limits/skips the specified numbers.</summary>
        /// <typeparam name="U">Type of document to find.</typeparam>
        /// <param name="template">The template.</param>
        /// <param name="limit">The number to return from this command.</param>
        /// <param name="skip">The skip step.</param>
        public IEnumerable<T> Find<U>(U template, int limit, int skip)
        {
            return Find(template, limit, skip, this.FullyQualifiedName);
        }

        /// <summary>Finds the documents matching the template, an limits/skips the specified numbers.</summary>
        /// <typeparam name="U">Type of document to find.</typeparam>
        /// <typeparam name="O">Type of document to find.</typeparam>
        /// <param name="template">The template.</param>
        /// <param name="orderby">How to order the results</param>
        /// <param name="limit">The number to return from this command.</param>
        /// <param name="skip">The skip step.</param>
        public IEnumerable<T> Find<U, O>(U template, O orderby, int limit, int skip)
        {
            return this.Find(template, orderby, limit, skip, this.FullyQualifiedName);
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
        new public long Count()
        {
            return Count(new { });
        }

        /// <summary>
        /// The get collection statistics.
        /// </summary>
        /// <returns></returns>
        public new CollectionStatistics GetCollectionStatistics()
        {
            return _db.GetCollectionStatistics(_collectionName);
        }

        /// <summary>
        /// Creates an index for a given collection.
        /// </summary>
        /// <param name="index">The property to index.</param>
        /// <param name="indexName">The index Name.</param>
        /// <param name="isUnique">Unique index flag.</param>
        /// <param name="direction">Ascending or descending.</param>
        public void CreateIndex(Expression<Func<T, object>> index, string indexName, bool isUnique, IndexOption direction)
        {
            var translator = new MongoQueryTranslator();
            // Index values should contain the full namespace without "this."
            var indexProperty = translator.Translate(index, false);

            var key = new Expando();
            key.Set(indexProperty.Query, direction);

            var collection = _db.GetCollection<MongoIndex<T>>("system.indexes");
            collection.Insert(new MongoIndex<T>
                                  {
                                      Key = key,
                                      Namespace = FullyQualifiedName,
                                      Name = indexName,
                                      Unique = isUnique
                                  });
        }

        /// <summary>
        /// Gets the distinct values for the specified key.
        /// </summary>
        /// <typeparam name="U">You better know that every value that could come back
        /// is of this type, or BAD THINGS will happen.</typeparam>
        /// <param name="keyName">Name of the key.</param>
        /// <returns></returns>
        public IEnumerable<U> Distinct<U>(string keyName)
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

        /// <summary>TODO::Description.</summary>
        public void Delete(T entity)
        {
            var helper = TypeHelper.GetHelperForType(typeof(T));
            var idProperty = helper.FindIdProperty();
            if (idProperty == null)
            {
                throw new MongoException(string.Format("Cannot delete {0} since it has no id property", typeof(T).FullName));
            }
            Delete(new { Id = idProperty.Getter(entity) });
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
            return this.Find<U, Object>(template, null, limit, skip, fullyQualifiedName);
        }


        /// <summary>
        /// Locates documents that match the template, in the order specified.
        /// </summary>
        /// <remarks>
        /// remember that "orderby" is the mongo notation where the following would sort by Name ascending,
        /// then by Date descending
        /// 
        /// new {Name=1, Date-1}
        /// </remarks>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="template">Passing null for this means it will be ignored.</param>
        /// <param name="orderBy">Passing null for this means it will be ignored.</param>
        /// <param name="limit">The maximum number of documents to return.</param>
        /// <param name="skip">The number to skip before returning any.</param>
        /// <param name="fullyQualifiedName">The collection from which to pull the documents.</param>
        /// <returns></returns>
        public IEnumerable<T> Find<U, S>(U template, S orderBy, int limit, int skip, string fullyQualifiedName)
        {
            var qm = new QueryMessage<T, U>(_connection, fullyQualifiedName)
            {
                NumberToTake = limit,
                NumberToSkip = skip,
                Query = template,
                OrderBy = orderBy
            };
            var type = typeof(T);
            if (MongoConfiguration.SummaryTypeFor(type) != null)
            {
                qm.FieldSelection = GetSelectionFields(type);
            }
            return new MongoQueryExecutor<T, U>(qm);
        }

        private static FieldSelectionList GetSelectionFields(Type type)
        {
            var properties = TypeHelper.GetHelperForType(type).GetProperties();
            var fields = new FieldSelectionList(properties.Count);
            foreach (var property in properties)
            {
                fields.Add(property.Name);
            }
            return fields;
        }

        /// <summary>
        /// Finds documents that match the template, and ordered according to the orderby document.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="template">The spec document</param>
        /// <param name="orderBy">The order specification</param>
        /// <returns>A set of documents ordered correctly and matching the spec.</returns>
        public IEnumerable<T> Find<U, S>(U template, S orderBy)
        {
            return this.Find(template, orderBy, Int32.MaxValue, 0, this.FullyQualifiedName);
        }
        /// <summary>
        /// Generates a query explain plan.
        /// </summary>
        /// <typeparam name="U">The type of the template document (probably an anonymous type..</typeparam>
        /// <param name="template">The template of the query to explain.</param>
        /// <returns></returns>
        public ExplainResponse Explain<U>(U template)
        {
            return this._db.GetCollection<ExplainResponse>(this._collectionName)
                .FindOne(new ExplainRequest<U>(template));
        }

        /// <summary>
        /// A count using the specified filter.
        /// </summary>
        /// <typeparam name="U">Document type</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>The count.</returns>
        public long Count<U>(U query)
        {
            var f = _db.GetCollection<Expando>("$cmd")
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
        public void Insert(params T[] documentsToInsert)
        {
            Insert(documentsToInsert.AsEnumerable());
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
            AssertUpdatable();
            TrySettingId(documentsToInsert);
            var insertMessage = new InsertMessage<T>(_connection, FullyQualifiedName, documentsToInsert);
            insertMessage.Execute();
            if (_connection.StrictMode)
            {
                var error = _db.LastError();
                if (error.Code > 0)
                {
                    throw new MongoException(error.Error);
                }
            }
        }


        /// <summary>
        /// Executes the MapReduce on this collection
        /// </summary>
        /// <typeparam name="X">The return type</typeparam>
        /// <param name="map"></param>
        /// <param name="reduce"></param>
        /// <returns></returns>
        public IEnumerable<X> MapReduce<X>(string map, string reduce)
        {
            return MapReduce<X>(new MapReduceOptions<T> { Map = map, Reduce = reduce });
        }

        /// <summary>
        /// Executes the map reduce with an applied template
        /// </summary>
        /// <typeparam name="U">The type of the template</typeparam>
        /// <typeparam name="X">The return type</typeparam>
        /// <param name="template"></param>
        /// <param name="map"></param>
        /// <param name="reduce"></param>
        /// <returns></returns>
        public IEnumerable<X> MapReduce<U, X>(U template, string map, string reduce)
        {
            return MapReduce<X>(new MapReduceOptions<T> { Query = template, Map = map, Reduce = reduce });

        }

        /// <summary>
        /// Executes the map reduce with an applied template and finalize
        /// </summary>
        /// <typeparam name="U">The type of the template</typeparam>
        /// <typeparam name="X">The return type</typeparam>
        /// <param name="template">The template</param>
        /// <param name="map"></param>
        /// <param name="reduce"></param>
        /// <param name="finalize"></param>
        /// <returns></returns>
        public IEnumerable<X> MapReduce<U, X>(U template, string map, string reduce, string finalize)
        {
            return MapReduce<X>(new MapReduceOptions<T> { Query = template, Map = map, Reduce = reduce, Finalize = finalize });

        }

        /// <summary>
        /// Executes the map reduce with any options
        /// </summary>
        /// <typeparam name="X">The return type</typeparam>
        /// <param name="options">The options</param>
        /// <returns></returns>
        public IEnumerable<X> MapReduce<X>(MapReduceOptions<T> options)
        {
            var mr = new MapReduce(_db);
            var response = mr.Execute(options);
            var collection = response.GetCollection<X>();
            return collection.Find().ToList();

        }



        private void AssertUpdatable()
        {
            if (!Updateable)
            {
                throw new MongoException("This collection does not accept insertions/updates, this is due to the fact that the collection's type " + typeof(T).FullName + " does not specify an identifier property");
            }
        }

        /// <summary>
        /// Tries the setting id property.
        /// </summary>
        /// <param name="entities">The entities.</param>
        private static void TrySettingId(IEnumerable<T> entities)
        {
            if (CanUpdateWithoutId(typeof(T)))
            {
                return;
            }

            var idProperty = TypeHelper.GetHelperForType(typeof(T)).FindIdProperty();
            if (!typeof(ObjectId).IsAssignableFrom(idProperty.Type) || idProperty.Setter == null)
            {
                return;
            }

            foreach (var entity in entities)
            {
                var value = idProperty.Getter(entity);
                if (value == null)
                {
                    idProperty.Setter(entity, ObjectId.NewObjectId());
                }
            }
            return;
        }

        /// <summary>
        /// Checks whether type implements IUpdateWithoutId.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if implemented; otherwise false.</returns>
        private static bool CanUpdateWithoutId(Type type)
        {
            return typeof(T).GetInterface("IUpdateWithoutId") != null;
        }
    }
}
