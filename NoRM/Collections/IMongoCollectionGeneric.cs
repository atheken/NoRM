using System.Collections.Generic;
using System;

namespace Norm.Collections
{
    public interface IMongoCollection : IMongoCollection<Object>
    {

    }

    /// <summary>
    /// Generic collection interface
    /// </summary>
    /// <typeparam name="T">The type of collection</typeparam>
    public interface IMongoCollection<T> 
    {
        /// <summary>
        /// Finds the specified document.
        /// </summary>
        /// <typeparam name="U">Type of document</typeparam>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        IEnumerable<T> Find<U>(U template);

        /// <summary>
        /// Finds all documents.
        /// </summary>
        /// <returns></returns>
        new IEnumerable<T> Find();

        /// <summary>
        /// Finds the specified document with a limited result set.
        /// </summary>
        /// <typeparam name="U">Type of document</typeparam>
        /// <param name="template">The template.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="fullyQualifiedName">Name of the fully qualified.</param>
        /// <returns></returns>
        IEnumerable<T> Find<U>(U template, int limit, string fullyQualifiedName);

        /// <summary>
        /// Finds the specified document with a limited result set.
        /// </summary>
        /// <typeparam name="U">Type of document</typeparam>
        /// <param name="template">The template.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        IEnumerable<T> Find<U>(U template, int limit);


        /// <summary>
        /// Find using the template, but skip some and limit how many you want.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="template"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        IEnumerable<T> Find<U>(U template, int limit, int skip);

        /// <summary>
        /// Finds one document.
        /// </summary>
        /// <typeparam name="U">Type to find</typeparam>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        T FindOne<U>(U template);

        /// <summary>
        /// Gets a nested collection with the type specified and the name specified.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        MongoCollection<U> GetChildCollection<U>(string collectionName) where U : class, new();

        /// <summary>
        /// Updates the specified document.
        /// </summary>
        /// <typeparam name="X">Document to match</typeparam>
        /// <typeparam name="U">Document to update</typeparam>
        /// <param name="matchDocument">The match document.</param>
        /// <param name="valueDocument">The value document.</param>
        /// <param name="updateMultiple">if set to <c>true</c> update all matching documents.</param>
        /// <param name="upsert">if set to <c>true</c> upsert.</param>
        void Update<X, U>(X matchDocument, U valueDocument, bool updateMultiple, bool upsert);

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMongoCollection&lt;T&gt;"/> is updateable.
        /// </summary>
        /// <value><c>true</c> if updateable; otherwise, <c>false</c>.</value>
        bool Updateable { get; }

        /// <summary>
        /// Updates one document.
        /// </summary>
        /// <typeparam name="X">Document to match</typeparam>
        /// <typeparam name="U">Document to update</typeparam>
        /// <param name="matchDocument">The match document.</param>
        /// <param name="valueDocument">The value document.</param>
        void UpdateOne<X, U>(X matchDocument, U valueDocument);


        /// <summary>
        /// Delete the documents that mact the specified template.
        /// </summary>
        /// <typeparam name="U">a document that has properties
        /// that match what you want to delete.</typeparam>
        /// <param name="template">The template.</param>
        void Delete<U>(U template);

        /// <summary>
        /// Delete the entity
        /// </summary>
        void Delete(T entity);


        /// <summary>
        /// Execute the mapreduce on this collection.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="reduce"></param>
        /// <returns></returns>
        IEnumerable<X> MapReduce<X>(string map, string reduce);

        /// <summary>
        /// Execute the mapreduce with a limiting query on this collection.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="map"></param>
        /// <param name="reduce"></param>
        /// <returns></returns>
        IEnumerable<X> MapReduce<U, X>(U template, string map, string reduce);

        /// <summary>
        /// Execute the mapreduce with a limiting query and finalize on this collection.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="map"></param>
        /// <param name="reduce"></param>
        /// <param name="finalize"></param>
        /// <returns></returns>
        IEnumerable<X> MapReduce<U, X>(U template, string map, string reduce, string finalize);

        /// <summary>
        /// Execute the mapreduce with the supplied options on this collection.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        IEnumerable<X> MapReduce<X>(MapReduceOptions<T> options);
    }
}
