using System.Collections.Generic;
using System;
using System.Linq;
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
        /// Finds all documents.
        /// </summary>
        /// <returns></returns>
        new IEnumerable<T> Find();

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

        T FindAndModify<U, X, Y>(U query, X update, Y sort);
        IEnumerable<T> Find<U, S>(U template, S orderBy, int limit, int skip, string fullyQualifiedName);
        IEnumerable<Z> Find<U, O, Z>(U template, O orderBy, int limit, int skip, String fullName, Expression<Func<T, Z>> fieldSelection);

        string FullyQualifiedName { get; }
        IQueryable<T> AsQueryable();

        void Insert(IEnumerable<T> documentsToInsert);

        ExplainResponse Explain<U>(U template);

        void CreateIndex(Expando key, String indexName, bool isUnique);
        void Save(T entity);

        new CollectionStatistics GetCollectionStatistics();

        long Count<U>(U query);

        bool DeleteIndex(string indexName, out int numberDeleted);

        IEnumerable<U> Distinct<U>(string keyName);

        long GenerateId();
    }
}
