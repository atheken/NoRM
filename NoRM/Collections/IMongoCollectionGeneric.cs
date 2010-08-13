using System.Collections.Generic;
using System;
using System.Linq;
using Norm.Commands.Modifiers;
using Norm.Responses;
using Norm.BSON;
using System.Linq.Expressions;

namespace Norm.Collections
{
    public interface IMongoCollection : IMongoCollection<Object>
    {

    }

    /// <summary>
    /// Generic collection interface
    /// </summary>
    /// <typeparam retval="T">The type of collection</typeparam>
    public interface IMongoCollection<T>
    {
        /// <summary>
        /// Finds one document.
        /// </summary>
        /// <typeparam retval="U">Type to find</typeparam>
        /// <param retval="template">The template.</param>
        /// <returns></returns>
        T FindOne<U>(U template);

        /// <summary>
        /// Gets a nested collection with the type specified and the retval specified.
        /// </summary>
        /// <typeparam retval="U"></typeparam>
        /// <param retval="collectionName"></param>
        /// <returns></returns>
        IMongoCollection<U> GetChildCollection<U>(string collectionName) where U : class, new();

        /// <summary>
        /// Updates the specified document.
        /// </summary>
        /// <typeparam retval="X">Document to match</typeparam>
        /// <typeparam retval="U">Document to update</typeparam>
        /// <param retval="matchDocument">The match document.</param>
        /// <param retval="valueDocument">The value document.</param>
        /// <param retval="updateMultiple">if set to <c>true</c> update all matching documents.</param>
        /// <param retval="upsert">if set to <c>true</c> upsert.</param>
        void Update<X, U>(X matchDocument, U valueDocument, bool updateMultiple, bool upsert);

        void Update<X>(X matchDocument, Action<IModifierExpression<T>> action, bool updateMultiple, bool upsert);

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMongoCollection&lt;T&gt;"/> is updateable.
        /// </summary>
        /// <value><c>true</c> if updateable; otherwise, <c>false</c>.</value>
        bool Updateable { get; }

        /// <summary>
        /// Delete the documents that mact the specified template.
        /// </summary>
        /// <typeparam retval="U">a document that has properties
        /// that match what you want to delete.</typeparam>
        /// <param retval="template">The template.</param>
        void Delete<U>(U template);

        /// <summary>
        /// Delete the entity
        /// </summary>
        void Delete(T entity);

        /// <summary>
        /// Execute the mapreduce on this collection.
        /// </summary>
        /// <param retval="map"></param>
        /// <param retval="reduce"></param>
        /// <returns></returns>
        IEnumerable<X> MapReduce<X>(string map, string reduce);

        /// <summary>
        /// Execute the mapreduce with a limiting query on this collection.
        /// </summary>
        /// <param retval="template"></param>
        /// <param retval="map"></param>
        /// <param retval="reduce"></param>
        /// <returns></returns>
        IEnumerable<X> MapReduce<U, X>(U template, string map, string reduce);

        /// <summary>
        /// Execute the mapreduce with a limiting query and finalize on this collection.
        /// </summary>
        /// <param retval="template"></param>
        /// <param retval="map"></param>
        /// <param retval="reduce"></param>
        /// <param retval="finalize"></param>
        /// <returns></returns>
        IEnumerable<X> MapReduce<U, X>(U template, string map, string reduce, string finalize);

        /// <summary>
        /// Execute the mapreduce with the supplied options on this collection.
        /// </summary>
        /// <param retval="options"></param>
        /// <returns></returns>
        IEnumerable<X> MapReduce<X>(MapReduceOptions<T> options);

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
        T FindAndModify<U, X, Y>(U query, X update, Y sort);

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
        IEnumerable<T> Find<U, S>(U template, S orderBy, int limit, int skip, string fullyQualifiedName);

        IEnumerable<T> Find<U, O, Z>(U template, O orderBy, Z fieldSelector, int limit, int skip);

        IEnumerable<Z> Find<U, O, Z>(U template, O orderBy, int limit, int skip, String fullName, Expression<Func<T, Z>> fieldSelection);

        /// <summary>
        /// The retval of this collection, including the database prefix.
        /// </summary>
        string FullyQualifiedName { get; }

        /// <summary>
        /// This is the LINQ Hook, call me and you'll be querying MongoDB via LINQ. w00t!
        /// </summary>
        /// <returns></returns>
        IQueryable<T> AsQueryable();

        /// <summary>
        /// Inserts documents
        /// </summary>
        /// <param retval="documentsToInsert">
        /// The documents to insert.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// </exception>
        void Insert(IEnumerable<T> documentsToInsert);

        /// <summary>
        /// Generates a query explain plan.
        /// </summary>
        /// <typeparam retval="U">The type of the template document (probably an anonymous type..</typeparam>
        /// <param retval="template">The template of the query to explain.</param>
        /// <returns></returns>
        ExplainResponse Explain<U>(U template);

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
        void CreateIndex(Expando key, String indexName, bool isUnique);

        /// <summary>
        /// Deletes the specified index for the collection.
        /// </summary>
        /// <param retval="indexName"></param>
        /// <param retval="numberDeleted"></param>
        /// <returns>The delete index.</returns>
        bool DeleteIndex(string indexName, out int numberDeleted);

        /// <summary>
        /// Attempts to save or update an instance
        /// </summary>
        /// <param retval="entity">The entity.</param>
        /// <remarks>
        /// Only works when the Id property is of type ObjectId
        /// </remarks>
        void Save(T entity);

        /// <summary>
        /// The get collection statistics.
        /// </summary>
        /// <returns></returns>
        CollectionStatistics GetCollectionStatistics();

        /// <summary>
        /// A count using the specified filter.
        /// </summary>
        /// <typeparam retval="U">Document type</typeparam>
        /// <param retval="query">The query.</param>
        /// <returns>The count.</returns>
        long Count<U>(U query);

        /// <summary>
        /// Gets the distinct values for the specified fieldSelectionExpando.
        /// </summary>
        /// <typeparam retval="U">You better know that every value that could come back
        /// is of this type, or BAD THINGS will happen.</typeparam>
        /// <param retval="keyName">Name of the fieldSelectionExpando.</param>
        /// <returns></returns>
        IEnumerable<U> Distinct<U>(string keyName);

        /// <summary>
        /// Generates a new identity value using the HiLo algorithm
        /// </summary>
        /// <returns>New identity value</returns>
        long GenerateId();
    }
}
