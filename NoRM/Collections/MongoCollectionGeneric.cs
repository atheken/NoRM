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

namespace Norm.Collections
{
    /// <summary>
    /// Mongo typed collection.
    /// </summary>
    /// <typeparam retval="T">Collection type</typeparam>
    public partial class MongoCollection<T> : IMongoCollection<T>
    {
        private static Dictionary<int, object> _compiledTransforms = new Dictionary<int, object>();

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
        /// <param retval="collectionName">The collection Name.</param>
        /// <param retval="db">The db.</param>
        /// <param retval="connection">The connection.</param>
        public MongoCollection(string collectionName, MongoDatabase db, IConnection connection)
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
            return new MongoQuery<T>(MongoQueryProvider.Create(this._db));
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
        /// <typeparam retval="U">Type of collection</typeparam>
        /// <param retval="collectionName">The collection Name.</param>
        /// <returns></returns>
        public MongoCollection<U> GetChildCollection<U>(string collectionName) where U : class, new()
        {
            return new MongoCollection<U>(_collectionName + "." + collectionName, _db, _connection);
        }

        /// <summary>
        /// Overload of Update that updates one document and doesn't upsert if no matches are found.
        /// </summary>
        /// <typeparam retval="X">Document to match</typeparam>
        /// <typeparam retval="U">Value document</typeparam>
        /// <param retval="matchDocument">The match Document.</param>
        /// <param retval="valueDocument">The value Document.</param>
        public void UpdateOne<X, U>(X matchDocument, U valueDocument)
        {
            Update(matchDocument, valueDocument, false, false);
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

        /// <summary>
        /// The retval of this collection, including the database prefix.
        /// </summary>
        public string FullyQualifiedName
        {
            get { return string.Format("{0}.{1}", _db.DatabaseName, _collectionName); }
        }



        /// <summary>
        /// Deletes all indices on this collection.
        /// </summary>
        /// <param retval="numberDeleted">
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
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
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
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <param retval="limit">The number to return from this command.</param>
        /// <returns></returns>
        public IEnumerable<T> Find<U>(U template, int limit)
        {
            return Find(template, limit, 0, FullyQualifiedName);
        }

        /// <summary>Finds the documents matching the template, an limits/skips the specified numbers.</summary>
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <param retval="limit">The number to return from this command.</param>
        /// <param retval="skip">The skip step.</param>
        public IEnumerable<T> Find<U>(U template, int limit, int skip)
        {
            return Find(template, limit, skip, this.FullyQualifiedName);
        }

        /// <summary>Finds the documents matching the template, an limits/skips the specified numbers.</summary>
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <typeparam retval="O">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <param retval="orderby">How to order the results</param>
        /// <param retval="limit">The number to return from this command.</param>
        /// <param retval="skip">The skip step.</param>
        public IEnumerable<T> Find<U, O>(U template, O orderby, int limit, int skip)
        {
            return this.Find(template, orderby, limit, skip, this.FullyQualifiedName);
        }

        /// <summary>
        /// The find.
        /// </summary>
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <param retval="limit">The limit.</param>
        /// <param retval="fullyQualifiedName">The fully qualified retval.</param>
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
        /// Returns the fully qualified and mapped retval from the member expression.
        /// </summary>
        /// <param retval="mex"></param>
        /// <returns></returns>
        private String RecurseMemberExpression(MemberExpression mex)
        {
            var retval = "";
            var parentEx = mex.Expression as MemberExpression;
            if (parentEx != null)
            {
                //we need to recurse because we're not at the root yet.
                retval += this.RecurseMemberExpression(parentEx) + ".";
            }
            retval += MongoConfiguration.GetPropertyAlias(mex.Expression.Type, mex.Member.Name);
            return retval;
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
        /// Asynchronously creates an index on this collection.
        /// </summary>
        /// <param retval="index">This is an expression of the elements in the type you wish to index, so you can do something like:
        /// <code>
        /// y=>y.MyIndexedProperty
        /// </code>
        /// or, if you have a multi-fieldSelectionExpando index, you can do this:
        /// <code>
        /// y=> new { y.PropertyA, y.PropertyB.Property1, y.PropertyC }
        /// </code>
        /// This will automatically map the MongoConfiguration aliases.
        /// </param>
        /// <param retval="indexName">The retval of the index as it should appear in the special "system.indexes" child collection.</param>
        /// <param retval="isUnique">True if MongoDB can expect that each document will have a unique combination for this fieldSelectionExpando. 
        /// MongoDB will potentially optimize the index based on this being true.</param>
        /// <param retval="direction">Should all of the elements in the index be sorted Ascending, or Decending, if you need to sort each property differently, 
        /// you should use the Expando overload of this method for greater granularity.</param>
        public void CreateIndex<U>(Expression<Func<T, U>> index, string indexName, bool isUnique, IndexOption direction)
        {
            var exp = index.Body as NewExpression;
            var key = new Expando();
            if (exp != null)
            {
                foreach (var x in exp.Arguments.OfType<MemberExpression>())
                {
                    key[this.RecurseMemberExpression(x)] = direction;
                }
            }
            else if (index.Body is MemberExpression)
            {
                var me = index.Body as MemberExpression;
                key[this.RecurseMemberExpression(me)] = direction;
            }
            this.CreateIndex(key, indexName, isUnique);
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
        /// Finds documents
        /// </summary>
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <param retval="limit">The limit.</param>
        /// <param retval="skip">The skip.</param>
        /// <param retval="fullyQualifiedName">The fully qualified retval.</param>
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
            var qm = new QueryMessage<T, U>(_connection, fullyQualifiedName)
            {
                NumberToTake = limit,
                NumberToSkip = skip,
                Query = template,
                OrderBy = orderBy
            };
            var type = typeof(T);
            return new MongoQueryExecutor<T, U>(qm);
        }

        public IEnumerable<Z> Find<U, O, Z>(U template, O orderBy, int limit, int skip, Expression<Func<T, Z>> fieldSelection)
        {
            return this.Find(template, orderBy, limit, skip, this.FullyQualifiedName, fieldSelection);
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
                    fieldSelectionExpando[this.RecurseMemberExpression(x)] = 1;
                }
            }
            else if (fieldSelection.Body is MemberExpression)
            {
                var me = fieldSelection.Body as MemberExpression;
                fieldSelectionExpando[this.RecurseMemberExpression(me)] = 1;
            }
            #endregion

            var qm = new QueryMessage<T, U>(_connection, fullName)
            {
                NumberToTake = limit,
                NumberToSkip = skip,
                Query = template,
                OrderBy = orderBy,
                FieldSelection = fieldSelectionExpando.AllProperties().Select(y => y.PropertyName)
            };

            object projection = null;
            if (!_compiledTransforms.TryGetValue(fieldSelection.GetHashCode(), out projection))
            {
                projection = fieldSelection.Compile();
                _compiledTransforms[fieldSelection.GetHashCode()] = projection;
            }
            return new MongoQueryExecutor<T, U, Z>(qm, (Func<T, Z>)projection);
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
        /// Finds documents that match the template, and ordered according to the orderby document.
        /// </summary>
        /// <typeparam retval="U"></typeparam>
        /// <typeparam retval="S"></typeparam>
        /// <param retval="template">The spec document</param>
        /// <param retval="orderBy">The order specification</param>
        /// <returns>A set of documents ordered correctly and matching the spec.</returns>
        public IEnumerable<T> Find<U, S>(U template, S orderBy)
        {
            return this.Find(template, orderBy, Int32.MaxValue, 0, this.FullyQualifiedName);
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
        public void Insert(params T[] documentsToInsert)
        {
            Insert(documentsToInsert.AsEnumerable());
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
        private static void TrySettingId(IEnumerable<T> entities)
        {
            if (typeof(T) != typeof(Object) && typeof(T).GetInterface("IUpdateWithoutId") == null)
            {
                var idProperty = TypeHelper.GetHelperForType(typeof(T)).FindIdProperty();
                if (idProperty != null && typeof(ObjectId).IsAssignableFrom(idProperty.Type) && idProperty.Setter != null)
                {
                    foreach (var entity in entities)
                    {
                        var value = idProperty.Getter(entity);
                        if (value == null)
                        {
                            idProperty.Setter(entity, ObjectId.NewObjectId());
                        }
                    }
                }
            }
        }

    }
}
