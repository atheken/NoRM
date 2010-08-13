using System.Collections.Generic;
using System;
using Norm.Responses;
using Norm.BSON;
using System.Linq.Expressions;
using Norm.Protocol.Messages;
using System.Linq;
using Norm.Commands.Modifiers;

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
        /// This is the LINQ Hook, call me and you'll be querying MongoDB via LINQ. w00t!
        /// </summary>
        /// <returns></returns>
        IQueryable<T> AsQueryable();

        /// <summary>
        /// Attempts to save or update an instance
        /// </summary>
        /// <param retval="entity">The entity.</param>
        /// <remarks>
        /// Only works when the Id property is of type ObjectId
        /// </remarks>
        void Save(T entity);

        /// <summary>
        /// Get a child collection of the specified type.
        /// </summary>
        /// <typeparam retval="U">Type of collection</typeparam>
        /// <param retval="collectionName">The collection Name.</param>
        /// <returns></returns>
        IMongoCollection<U> GetChildCollection<U>(string collectionName) where U : class, new();

        /// <summary>
        /// Overload of Update that updates one document and doesn't upsert if no matches are found.
        /// </summary>
        /// <typeparam retval="X">Document to match</typeparam>
        /// <typeparam retval="U">Value document</typeparam>
        /// <param retval="matchDocument">The match Document.</param>
        /// <param retval="valueDocument">The value Document.</param>
        void UpdateOne<X, U>(X matchDocument, U valueDocument);

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
        void Update<X, U>(X matchDocument, U valueDocument, bool updateMultiple, bool upsert);

        /// <summary>
        /// The retval of this collection, including the database prefix.
        /// </summary>
        string FullyQualifiedName {get;}

        bool Updateable { get; }

        /// <summary>
        /// Deletes all indices on this collection.
        /// </summary>
        /// <param retval="numberDeleted">
        /// </param>
        /// <returns>
        /// The delete indices.
        /// </returns>
        bool DeleteIndices(out int numberDeleted);

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
        bool DeleteIndex(string indexName, out int numberDeleted);

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
        T FindOne<U>(U template);

        /// <summary>Allows a document to be updated using the specified action.</summary>
        void Update<X>(X matchDocument, Action<IModifierExpression<T>> action);


        /// <summary>TODO::Description.</summary>
        void Update<X>(X matchDocument, Action<IModifierExpression<T>> action, bool updateMultiple, bool upsert);

        /// <summary>
        /// Find objects in the collection without any qualifiers.
        /// </summary>
        /// <returns></returns>
        new IEnumerable<T> Find();

        /// <summary>
        /// Return all documents matching the template
        /// </summary>
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <returns></returns>
        /// <remarks>
        /// Ok, not all documents, just all documents up to Int32.MaxValue - if you bring that many back, you've crashed. Sorry.
        /// </remarks>
        IEnumerable<T> Find<U>(U template);

        /// <summary>
        /// Get the documents that match the specified template.
        /// </summary>
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <param retval="limit">The number to return from this command.</param>
        /// <returns></returns>
        IEnumerable<T> Find<U>(U template, int limit);

        /// <summary>Finds the documents matching the template, an limits/skips the specified numbers.</summary>
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <param retval="limit">The number to return from this command.</param>
        /// <param retval="skip">The skip step.</param>
        IEnumerable<T> Find<U>(U template, int limit, int skip);

        /// <summary>Finds the documents matching the template, an limits/skips the specified numbers.</summary>
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <typeparam retval="O">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <param retval="orderby">How to order the results</param>
        /// <param retval="limit">The number to return from this command.</param>
        /// <param retval="skip">The skip step.</param>
        IEnumerable<T> Find<U, O>(U template, O orderby, int limit, int skip);

        /// <summary>
        /// The find.
        /// </summary>
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <param retval="limit">The limit.</param>
        /// <param retval="fullyQualifiedName">The fully qualified retval.</param>
        /// <returns></returns>
        IEnumerable<T> Find<U>(U template, int limit, string fullyQualifiedName);

        /// <summary>
        /// A count on this collection without any filter.
        /// </summary>
        /// <returns>The count.</returns>
        new long Count();

        /// <summary>
        /// The get collection statistics.
        /// </summary>
        /// <returns></returns>
        new CollectionStatistics GetCollectionStatistics();


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
        void CreateIndex<U>(Expression<Func<T, U>> index, string indexName, bool isUnique, IndexOption direction);

        /// <summary>
        /// Gets the distinct values for the specified fieldSelectionExpando.
        /// </summary>
        /// <typeparam retval="U">You better know that every value that could come back
        /// is of this type, or BAD THINGS will happen.</typeparam>
        /// <param retval="keyName">Name of the fieldSelectionExpando.</param>
        /// <returns></returns>
        IEnumerable<U> Distinct<U>(string keyName);

        /// <summary>
        /// Delete the documents that mact the specified template.
        /// </summary>
        /// <typeparam retval="U">a document that has properties
        /// that match what you want to delete.</typeparam>
        /// <param retval="template">The template.</param>
        void Delete<U>(U template);

        /// <summary>
        /// Deletes the specified document based on it's Id property.
        /// </summary>
        void Delete(T document);

        /// <summary>
        /// Finds documents
        /// </summary>
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <param retval="limit">The limit.</param>
        /// <param retval="skip">The skip.</param>
        /// <param retval="fullyQualifiedName">The fully qualified retval.</param>
        /// <returns></returns>
        IEnumerable<T> Find<U>(U template, int limit, int skip, string fullyQualifiedName);


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

        IEnumerable<Z> Find<U, O, Z>(U template, O orderBy, int limit, int skip, Expression<Func<T, Z>> fieldSelection);

        IEnumerable<Z> Find<U, O, Z>(U template, O orderBy, int limit, int skip, String fullName, Expression<Func<T, Z>> fieldSelection);


        /// <summary>
        /// Finds documents that match the template, and ordered according to the orderby document.
        /// </summary>
        /// <typeparam retval="U"></typeparam>
        /// <typeparam retval="S"></typeparam>
        /// <param retval="template">The spec document</param>
        /// <param retval="orderBy">The order specification</param>
        /// <returns>A set of documents ordered correctly and matching the spec.</returns>
        IEnumerable<T> Find<U, S>(U template, S orderBy);
        /// <summary>
        /// Generates a query explain plan.
        /// </summary>
        /// <typeparam retval="U">The type of the template document (probably an anonymous type..</typeparam>
        /// <param retval="template">The template of the query to explain.</param>
        /// <returns></returns>
        ExplainResponse Explain<U>(U template);

        /// <summary>
        /// A count using the specified filter.
        /// </summary>
        /// <typeparam retval="U">Document type</typeparam>
        /// <param retval="query">The query.</param>
        /// <returns>The count.</returns>
        long Count<U>(U query);

        /// <summary>
        /// Inserts documents
        /// </summary>
        /// <param retval="documentsToInsert">
        /// The documents to insert.
        /// </param>
        void Insert(params T[] documentsToInsert);

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
        /// Executes the MapReduce on this collection
        /// </summary>
        /// <typeparam retval="X">The return type</typeparam>
        /// <param retval="map"></param>
        /// <param retval="reduce"></param>
        /// <returns></returns>
        IEnumerable<X> MapReduce<X>(string map, string reduce);

        /// <summary>
        /// Executes the map reduce with an applied template
        /// </summary>
        /// <typeparam retval="U">The type of the template</typeparam>
        /// <typeparam retval="X">The return type</typeparam>
        /// <param retval="template"></param>
        /// <param retval="map"></param>
        /// <param retval="reduce"></param>
        /// <returns></returns>
        IEnumerable<X> MapReduce<U, X>(U template, string map, string reduce);

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
        IEnumerable<X> MapReduce<U, X>(U template, string map, string reduce, string finalize);

        /// <summary>
        /// Executes the map reduce with any options
        /// </summary>
        /// <typeparam retval="X">The return type</typeparam>
        /// <param retval="options">The options</param>
        /// <returns></returns>
        IEnumerable<X> MapReduce<X>(MapReduceOptions<T> options);
    }
}
