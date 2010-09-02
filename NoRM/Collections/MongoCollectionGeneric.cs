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
using TypeHelper = Norm.BSON.ReflectionHelper;
using Norm.Commands.Modifiers;
using Norm;

namespace Norm.Collections
{
    /// <summary>
    /// Mongo typed collection.
    /// </summary>
    /// <remarks>
    /// This class is not (and will probably not become) thread-safe.
    /// </remarks>
    /// <typeparam retval="T">Collection type</typeparam>
    public partial class MongoCollection<T> : IMongoCollection<T>
    {
        private static Dictionary<int, object> _compiledTransforms = new Dictionary<int, object>();
        private static CollectionHiLoIdGenerator _collectionHiLoIdGenerator = new CollectionHiLoIdGenerator(20);

        /// <summary>
        /// This will have a different instance for each concrete version of <see cref="MongoCollection{T}"/>
        /// </summary>
        protected static bool? _updateable;

        /// <summary>TODO::Description.</summary>
        protected string _collectionName;

        /// <summary>TODO::Description.</summary>
        protected IConnection _connection;

        /// <summary>TODO::Description.</summary>
        protected IMongoDatabase _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCollection{T}"/> class.
        /// Represents a strongly-typed set of documents in the db.
        /// </summary>
        /// <param retval="collectionName">The collection Name.</param>
        /// <param retval="db">The db.</param>
        /// <param retval="connection">The connection.</param>
        public MongoCollection(string collectionName, IMongoDatabase db, IConnection connection)
        {
            _db = db;
            _connection = connection;
            _collectionName = collectionName;
        }

        /// <summary>
        /// This is the LINQ Hook, call me and you'll be querying MongoDB via LINQ. w00t!
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> AsQueryable()
        {
            return new MongoQuery<T>(MongoQueryProvider.Create(this._db, this._collectionName));
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
                    var retval = false;

                    var t = typeof(T);
                    if (t == typeof(Object) || t.GetInterface("IUpdateWithoutId") != null)
                    {
                        retval = true;
                    }

                    if (!retval)
                    {
                        retval = TypeHelper.GetHelperForType(typeof(T)).FindIdProperty() != null;
                    }
                    _updateable = retval;
                }
                return _updateable.Value;
            }
        }

        /// <summary>
        /// Attempts to save or update an instance
        /// </summary>
        /// <param retval="entity">The entity.</param>
        /// <remarks>
        /// Only works when the Id property is of type ObjectId
        /// </remarks>
        public void Save(T entity)
        {
            AssertUpdatable();

            var helper = TypeHelper.GetHelperForType(typeof(T));
            var idProperty = helper.FindIdProperty();
            var id = idProperty.Getter(entity);
            if (id == null && (
                (typeof(ObjectId).IsAssignableFrom(idProperty.Type)) ||
                (typeof(long?).IsAssignableFrom(idProperty.Type)) ||
                (typeof(int?).IsAssignableFrom(idProperty.Type))))
            {
                this.Insert(entity);
            }
            else
            {
                Update(new { Id = id }, entity, false, true);
            }
        }

        /// <summary>
        /// Get a child collection of the specified type.
        /// </summary>
        /// <typeparam retval="U">Type of collection</typeparam>
        /// <param retval="collectionName">The collection Name.</param>
        /// <returns></returns>
        public IMongoCollection<U> GetChildCollection<U>(string collectionName) where U : class, new()
        {
            return new MongoCollection<U>(_collectionName + "." + collectionName, _db, _connection);
        }

        /// <summary>
        /// The update.
        /// </summary>
        /// <typeparam retval="X">Document to match</typeparam>
        /// <typeparam retval="U">Value document</typeparam>
        /// <param retval="matchDocument">The match document.</param>
        /// <param retval="valueDocument">The value document.</param>
        /// <param retval="updateMultiple">The update multiple.</param>
        /// <param retval="upsert">The upsert.</param>
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
        /// The retval of this collection, including the database prefix.
        /// </summary>
        public string FullyQualifiedName
        {
            get { return string.Format("{0}.{1}", _db.DatabaseName, _collectionName); }
        }

        /// <summary>
        /// Deletes the specified index for the collection.
        /// </summary>
        /// <param retval="indexName">
        /// </param>
        /// <param retval="numberDeleted">
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
        /// <typeparam retval="U">A type that has each member set to the value to search.
        /// Keep in mind that all the properties must either be concrete values, or the
        /// special "Qualifier"-type values.</typeparam>
        /// <param retval="template">The template.</param>
        /// <returns>
        /// The first document that matched the template, or default(T)
        /// </returns>
        public T FindOne<U>(U template)
        {
            return this.Find(template, 1).FirstOrDefault();
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
        /// Asynchronously creates an index on this collection.
        /// It is highly recommended that you use the overload of this method that accepts an expression unless you need the granularity that this method provides.
        /// </summary>
        /// <param retval="fieldSelectionExpando">The document properties that participate in this index. Each property of "fieldSelectionExpando" should be 
        /// set to either "IndexOption.Ascending" or "IndexOption.Descending", the properties can be deep aliases, like "Suppiler.Name",
        /// but remember that this will make no effort to check that what you put in for values match the MongoConfiguration.</param>
        /// <param retval="indexName">The retval of the index as it should appear in the special "system.indexes" child collection.</param>
        /// <param retval="isUnique">True if MongoDB can expect that each document will have a unique combination for this fieldSelectionExpando. 
        /// MongoDB will potentially optimize the index based on this being true.</param>
        public void CreateIndex(Expando key, String indexName, bool isUnique)
        {
            var collection = _db.GetCollection<MongoIndex>("system.indexes");
            collection.Insert(
                new MongoIndex
                {
                    Key = key,
                    Namespace = FullyQualifiedName,
                    Name = indexName,
                    Unique = isUnique
                });
        }

        /// <summary>
        /// Gets the distinct values for the specified fieldSelectionExpando.
        /// </summary>
        /// <typeparam retval="U">You better know that every value that could come back
        /// is of this type, or BAD THINGS will happen.</typeparam>
        /// <param retval="keyName">Name of the fieldSelectionExpando.</param>
        /// <returns></returns>
        public IEnumerable<U> Distinct<U>(string keyName)
        {
            return _db.GetCollection<DistinctValuesResponse<U>>("$cmd")
                .FindOne(new { distinct = _collectionName, key = keyName }).Values;
        }

        /// <summary>
        /// Delete the documents that mact the specified template.
        /// </summary>
        /// <typeparam retval="U">a document that has properties
        /// that match what you want to delete.</typeparam>
        /// <param retval="template">The template.</param>
        public void Delete<U>(U template)
        {
            var dm = new DeleteMessage<U>(_connection, FullyQualifiedName, template);
            dm.Execute();
        }

        /// <summary>
        /// Deletes the specified document based on it's Id property.
        /// </summary>
        public void Delete(T document)
        {
            var helper = TypeHelper.GetHelperForType(typeof(T));
            var idProperty = helper.FindIdProperty();
            if (idProperty == null)
            {
                throw new MongoException(string.Format("Cannot delete {0} since it has no id property", typeof(T).FullName));
            }
            Delete(new { Id = idProperty.Getter(document) });
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
        /// <typeparam retval="U"></typeparam>
        /// <typeparam retval="S"></typeparam>
        /// <param retval="template">Passing null for this means it will be ignored.</param>
        /// <param retval="orderBy">Passing null for this means it will be ignored.</param>
        /// <param retval="limit">The maximum number of documents to return.</param>
        /// <param retval="skip">The number to skip before returning any.</param>
        /// <param retval="fullyQualifiedName">The collection from which to pull the documents.</param>
        /// <returns></returns>
        public IEnumerable<T> Find<U, S>(U template, S orderBy, int limit, int skip, string fullyQualifiedName)
        {
            var type = typeof(T);

            var qm = new QueryMessage<T, U>(_connection, fullyQualifiedName)
            {
                NumberToTake = limit,
                NumberToSkip = skip,
                Query = template,
                OrderBy = orderBy
            };

            return new MongoQueryExecutor<T, U>(qm);
        }

        /// <summary>
        /// This command can be used to atomically modify a document (at most one) and return it.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="X"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <param name="query">The document template used to find the document to find and modify</param>
        /// <param name="update">A modifier object</param>
        /// <param name="sort">If multiple docs match, choose the first one in the specified sort order as the object to manipulate</param>
        /// <returns></returns>
        public T FindAndModify<U, X, Y>(U query, X update, Y sort)
        {
            var cmdColl = this._db.GetCollection<FindAndModifyResult<T>>("$cmd");

            try
            {
                var returnValue = cmdColl.FindOne(new
                {
                    findandmodify = this._collectionName,
                    query = query,
                    update = update,
                    sort = sort
                }).Value;

                return returnValue;
            }
            catch (MongoException ex)
            {
                if (ex.Message == "No matching object found")
                    return default(T);

                throw;
            }
        }

        public IEnumerable<T> Find<U, O, Z>(U template, O orderBy, Z fieldSelector, int limit, int skip)
        {

            var qm = new QueryMessage<T, U>(_connection, this.FullyQualifiedName)
            {
                NumberToTake = limit,
                NumberToSkip = skip,
                Query = template,
                OrderBy = orderBy,
                FieldSelection = fieldSelector
            };

            return new MongoQueryExecutor<T, U>(qm) { CollectionName = this._collectionName };
        }

        public IEnumerable<Z> Find<U, O, Z>(U template, O orderBy, int limit, int skip, String fullName, Expression<Func<T, Z>> fieldSelection)
        {
            #region Extract field names to request
            var exp = fieldSelection.Body as NewExpression;
            var fieldSelectionExpando = new Expando();
            if (exp != null)
            {
                foreach (var x in exp.Arguments.OfType<MemberExpression>())
                {
                    fieldSelectionExpando[x.GetPropertyAlias()] = 1;
                }
            }
            else if (fieldSelection.Body is MemberExpression)
            {
                var me = fieldSelection.Body as MemberExpression;
                fieldSelectionExpando[me.GetPropertyAlias()] = 1;
            }
            #endregion


            var qm = new QueryMessage<T, U>(_connection, fullName)
            {
                NumberToTake = limit,
                NumberToSkip = skip,
                Query = template,
                OrderBy = orderBy,
                FieldSelection = fieldSelectionExpando
            };

            object projection = null;
            if (!_compiledTransforms.TryGetValue(fieldSelection.GetHashCode(), out projection))
            {
                projection = fieldSelection.Compile();
                _compiledTransforms[fieldSelection.GetHashCode()] = projection;
            }
            return new MongoQueryExecutor<T, U, Z>(qm, (Func<T, Z>)projection) { CollectionName = this._collectionName };
        }

        /// <summary>
        /// Infrastructure, Used by Linq Provider
        /// </summary>
        /// <remarks>
        /// DO NOT change the name or signature of this method without also adjusting the LINQ Provider.
        /// </remarks>
        private IEnumerable<Z> FindFieldSelection<U, O, Z>(U template, O orderBy, int limit, int skip, String fullName, Expression<Func<T, Z>> fieldSelection)
        {
            return this.Find(template, orderBy, limit, skip, fullName, fieldSelection);
        }

        /// <summary>
        /// Generates a query explain plan.
        /// </summary>
        /// <typeparam retval="U">The type of the template document (probably an anonymous type..</typeparam>
        /// <param retval="template">The template of the query to explain.</param>
        /// <returns></returns>
        public ExplainResponse Explain<U>(U template)
        {
            return this._db.GetCollection<ExplainResponse>(this._collectionName)
                .FindOne(new ExplainRequest<U>(template));
        }

        /// <summary>
        /// A count using the specified filter.
        /// </summary>
        /// <typeparam retval="U">Document type</typeparam>
        /// <param retval="query">The query.</param>
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
        /// <param retval="documentsToInsert">
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
                var error = _db.LastError(_connection.VerifyWriteCount);
                if (error.Code > 0)
                {
                    throw new MongoException(error.Error);
                }
            }
        }

        /// <summary>
        /// Executes the MapReduce on this collection
        /// </summary>
        /// <typeparam retval="X">The return type</typeparam>
        /// <param retval="map"></param>
        /// <param retval="reduce"></param>
        /// <returns></returns>
        public IEnumerable<X> MapReduce<X>(string map, string reduce)
        {
            return MapReduce<X>(new MapReduceOptions<T> { Map = map, Reduce = reduce });
        }

        /// <summary>
        /// Executes the map reduce with an applied template
        /// </summary>
        /// <typeparam retval="U">The type of the template</typeparam>
        /// <typeparam retval="X">The return type</typeparam>
        /// <param retval="template"></param>
        /// <param retval="map"></param>
        /// <param retval="reduce"></param>
        /// <returns></returns>
        public IEnumerable<X> MapReduce<U, X>(U template, string map, string reduce)
        {
            return MapReduce<X>(new MapReduceOptions<T> { Query = template, Map = map, Reduce = reduce });
        }

        /// <summary>
        /// Executes the map reduce with an applied template and finalize
        /// </summary>
        /// <typeparam retval="U">The type of the template</typeparam>
        /// <typeparam retval="X">The return type</typeparam>
        /// <param retval="template">The template</param>
        /// <param retval="map"></param>
        /// <param retval="reduce"></param>
        /// <param retval="finalize"></param>
        /// <returns></returns>
        public IEnumerable<X> MapReduce<U, X>(U template, string map, string reduce, string finalize)
        {
            return MapReduce<X>(new MapReduceOptions<T> { Query = template, Map = map, Reduce = reduce, Finalize = finalize });
        }

        /// <summary>
        /// Executes the map reduce with any options
        /// </summary>
        /// <typeparam retval="X">The return type</typeparam>
        /// <param retval="options">The options</param>
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
        /// <param retval="entities">The entities.</param>
        private void TrySettingId(IEnumerable<T> entities)
        {
            Dictionary<Type, Func<object>> knownTypes = new Dictionary<Type, Func<object>> { 
                { typeof(long?), () => GenerateId() }, 
                { typeof(int?), () => Convert.ToInt32(GenerateId()) }, 
                { typeof(ObjectId), () => ObjectId.NewObjectId() } 
            };

            if (typeof(T) != typeof(Object) && typeof(T).GetInterface("IUpdateWithoutId") == null)
            {
                var idProperty = TypeHelper.GetHelperForType(typeof(T)).FindIdProperty();
                if (idProperty != null && knownTypes.ContainsKey(idProperty.Type) && idProperty.Setter != null)
                {
                    foreach (var entity in entities)
                    {
                        var value = idProperty.Getter(entity);
                        if (value == null)
                        {
                            idProperty.Setter(entity, knownTypes[idProperty.Type]());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates a new identity value using the HiLo algorithm
        /// </summary>
        /// <returns>New identity value</returns>
        public long GenerateId()
        {
            return _collectionHiLoIdGenerator.GenerateId(_db, _collectionName);
        }

    }
}
