
using System.Collections.Generic;

namespace NoRM
{
    /// <summary>
    /// Mongo collection.
    /// </summary>
    /// <typeparam name="T">Type of collection</typeparam>
    public interface IMongoCollection<T> : IMongoCollection
    {
        /// <summary>
        /// Gets a value indicating whether the document is updatable (has an Oid).
        /// </summary>
        bool Updateable { get; }

        /// <summary>
        /// Finds documents.
        /// </summary>
        /// <typeparam name="U">Document type to find.</typeparam>
        /// <param name="template">The document template.</param>
        /// <returns></returns>
        IEnumerable<T> Find<U>(U template);

        /// <summary>
        /// Finds documents.
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> Find();

        /// <summary>
        /// Finds documents.
        /// </summary>
        /// <typeparam name="U">Document type to find.</typeparam>
        /// <param name="template">The document template.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="fullyQualifiedName">The fully qualified name.</param>
        /// <returns></returns>
        IEnumerable<T> Find<U>(U template, int limit, string fullyQualifiedName);

        /// <summary>
        /// Finds documents.
        /// </summary>
        /// <typeparam name="U">Document type to find.</typeparam>
        /// <param name="template">The document template.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        IEnumerable<T> Find<U>(U template, int limit);

        /// <summary>
        /// Finds one document.
        /// </summary>
        /// <typeparam name="U">Document type to find.</typeparam>
        /// <param name="template">The document template.</param>
        /// <returns></returns>
        T FindOne<U>(U template);

        /// <summary>
        /// Gets a child collection.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="collectionName">The collection name.</param>
        /// <returns></returns>
        MongoCollection<U> GetChildCollection<U>(string collectionName) where U : class, new();

        /// <summary>
        /// Updates documents.
        /// </summary>
        /// <typeparam name="X">Document to match.</typeparam>
        /// <typeparam name="U">Value document.</typeparam>
        /// <param name="matchDocument">The match document.</param>
        /// <param name="valueDocument">The value document.</param>
        /// <param name="updateMultiple">The update multiple.</param>
        /// <param name="upsert">The upsert.</param>
        void Update<X, U>(X matchDocument, U valueDocument, bool updateMultiple, bool upsert);

        /// <summary>
        /// The update multiple documents.
        /// </summary>
        /// <typeparam name="X">Document type to match.</typeparam>
        /// <typeparam name="U">Value document</typeparam>
        /// <param name="matchDocument">The match document.</param>
        /// <param name="valueDocument">The value document.</param>
        void UpdateMultiple<X, U>(X matchDocument, U valueDocument);

        /// <summary>
        /// The update one.
        /// </summary>
        /// <typeparam name="X">Document type to match.</typeparam>
        /// <typeparam name="U">Value document.</typeparam>
        /// <param name="matchDocument">The match document.</param>
        /// <param name="valueDocument">The value document.</param>
        void UpdateOne<X, U>(X matchDocument, U valueDocument);
    }
}
