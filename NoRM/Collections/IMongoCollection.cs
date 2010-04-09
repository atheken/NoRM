using System.Collections;
using System.Collections.Generic;
using Norm.Responses;

namespace Norm.Collections
{
    /// <summary>
    /// Mongo collection.
    /// </summary>
    public interface IMongoCollection
    {
        /// <summary>
        /// Gets the fully qualified name.
        /// </summary>
        string FullyQualifiedName { get; }

        /// <summary>
        /// The document count.
        /// </summary>
        /// <returns>The count.</returns>
        long Count();

        /// <summary>
        /// The document count.
        /// </summary>
        /// <param name="template">The document template.</param>
        /// <returns>The count.</returns>
        long Count(object template);

        /// <summary>
        /// The document group.
        /// </summary>
        /// <param name="template">The document template.</param>
        /// <returns>The group.</returns>
        object Group(object template);

        /// <summary>
        /// Delete an index.
        /// </summary>
        /// <param name="indexName">The index name.</param>
        /// <param name="numberDeleted">The number deleted.</param>
        /// <returns>Index deleted.</returns>
        bool DeleteIndex(string indexName, out int numberDeleted);

        /// <summary>
        /// Deletes indices.
        /// </summary>
        /// <param name="numberDeleted">The number deleted.</param>
        /// <returns>deleted indices.</returns>
        bool DeleteIndices(out int numberDeleted);

        /// <summary>
        /// Gets collection statistics.
        /// </summary>
        /// <returns></returns>
        CollectionStatistics GetCollectionStatistics();

        /// <summary>
        /// Finds one document.
        /// </summary>
        /// <param name="template">The document template.</param>
        /// <returns>The find one.</returns>
        object FindOne(object template);

        /// <summary>
        /// Finds documents.
        /// </summary>
        /// <param name="template">The document template.</param>
        /// <returns></returns>
        IEnumerable Find(object template);

        /// <summary>
        /// Finds documents.
        /// </summary>
        /// <returns></returns>
        IEnumerable Find();

        /// <summary>
        /// Finds document.
        /// </summary>
        /// <param name="template">The document template.</param>
        /// <param name="limit">The result limit.</param>
        /// <param name="fullyQualifiedName">The fully qualified name.</param>
        /// <returns></returns>
        IEnumerable Find(object template, int limit, string fullyQualifiedName);

        /// <summary>
        /// Finds documents.
        /// </summary>
        /// <param name="template">The document template.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        IEnumerable Find(object template, int limit);


        /// <summary>
        /// Find some documents matching the template, but skip the top X and take the next top Y.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        IEnumerable Find(object template, int limit, int skip);
    }
}