using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Collections;
using System.Linq.Expressions;
using Norm.BSON;
using Norm.Configuration;
using Norm.Protocol.Messages;
using Norm.Linq;
using Norm.Commands.Modifiers;

namespace Norm
{
    public static class MongoCollectionExtensions
    {
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
        public static void CreateIndex<T, U>(this IMongoCollection<T> collection, Expression<Func<T, U>> index, string indexName, bool isUnique, IndexOption direction)
        {
            var exp = index.Body as NewExpression;
            var key = new Expando();
            if (exp != null)
            {
                foreach (var x in exp.Arguments.OfType<MemberExpression>())
                {
                    key[x.GetPropertyAlias()] = direction;
                }
            }
            else if (index.Body is MemberExpression)
            {
                var me = index.Body as MemberExpression;
                key[me.GetPropertyAlias()] = direction;
            }
            collection.CreateIndex(key, indexName, isUnique);
        }

        public static IEnumerable<Z> Find<T, U, O, Z>(this IMongoCollection<T> collection, U template, O orderBy, int limit, int skip, Expression<Func<T, Z>> fieldSelection)
        {
            return collection.Find(template, orderBy, limit, skip, collection.FullyQualifiedName, fieldSelection);
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
        public static IEnumerable<T> Find<T, U>(this IMongoCollection<T> collection, U template, int limit, int skip, string fullyQualifiedName)
        {
            return collection.Find<U, Object>(template, null, limit, skip, fullyQualifiedName);
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
        public static IEnumerable<T> Find<T, U>(this IMongoCollection<T> collection, U template)
        {
            return collection.Find(template, Int32.MaxValue);
        }

        /// <summary>
        /// Get the documents that match the specified template.
        /// </summary>
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <param retval="limit">The number to return from this command.</param>
        /// <returns></returns>
        public static IEnumerable<T> Find<T, U>(this IMongoCollection<T> collection, U template, int limit)
        {
            return collection.Find(template, limit, 0, collection.FullyQualifiedName);
        }

        /// <summary>Finds the documents matching the template, an limits/skips the specified numbers.</summary>
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <param retval="limit">The number to return from this command.</param>
        /// <param retval="skip">The skip step.</param>
        public static IEnumerable<T> Find<T, U>(this IMongoCollection<T> collection, U template, int limit, int skip)
        {
            return collection.Find(template, limit, skip, collection.FullyQualifiedName);
        }

        /// <summary>Finds the documents matching the template, an limits/skips the specified numbers.</summary>
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <typeparam retval="O">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <param retval="orderby">How to order the results</param>
        /// <param retval="limit">The number to return from this command.</param>
        /// <param retval="skip">The skip step.</param>
        public static IEnumerable<T> Find<T, U, O>(this IMongoCollection<T> collection, U template, O orderby, int limit, int skip)
        {
            return collection.Find(template, orderby, limit, skip, collection.FullyQualifiedName);
        }

        /// <summary>
        /// The find.
        /// </summary>
        /// <typeparam retval="U">Type of document to find.</typeparam>
        /// <param retval="template">The template.</param>
        /// <param retval="limit">The limit.</param>
        /// <param retval="fullyQualifiedName">The fully qualified retval.</param>
        /// <returns></returns>
        public static IEnumerable<T> Find<T, U>(this IMongoCollection<T> collection, U template, int limit, string fullyQualifiedName)
        {
            return collection.Find(template, limit, 0, fullyQualifiedName);
        }

        /// <summary>
        /// Finds documents that match the template, and ordered according to the orderby document.
        /// </summary>
        /// <typeparam retval="U"></typeparam>
        /// <typeparam retval="S"></typeparam>
        /// <param retval="template">The spec document</param>
        /// <param retval="orderBy">The order specification</param>
        /// <returns>A set of documents ordered correctly and matching the spec.</returns>
        public static IEnumerable<T> Find<T, U, S>(this IMongoCollection<T> collection, U template, S orderBy)
        {
            return collection.Find(template, orderBy, Int32.MaxValue, 0, collection.FullyQualifiedName);
        }

        /// <summary>
        /// Find objects in the collection without any qualifiers.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<T> Find<T>(this IMongoCollection<T> collection)
        {
            // this is a hack to get a value that will test for null into the serializer.
            return collection.Find(new object(), Int32.MaxValue, collection.FullyQualifiedName);
        }

        /// <summary>
        /// Inserts documents
        /// </summary>
        /// <param retval="documentsToInsert">
        /// The documents to insert.
        /// </param>
        public static void Insert<T>(this IMongoCollection<T> collection, params T[] documentsToInsert)
        {
            collection.Insert(documentsToInsert.AsEnumerable());
        }

        public static T FindAndModify<T, U, X>(this IMongoCollection<T> collection, U query, X update)
        {
            return collection.FindAndModify<U, X, object>(query, update, new { });
        }


        /// <summary>
        /// Overload of Update that updates one document and doesn't upsert if no matches are found.
        /// </summary>
        /// <typeparam retval="X">Document to match</typeparam>
        /// <typeparam retval="U">Value document</typeparam>
        /// <param retval="matchDocument">The match Document.</param>
        /// <param retval="valueDocument">The value Document.</param>
        public static void UpdateOne<T, X, U>(this IMongoCollection<T> collection, X matchDocument, U valueDocument)
        {
            collection.Update(matchDocument, valueDocument, false, false);
        }

        /// <summary>Allows a document to be updated using the specified action.</summary>
        public static void Update<T, X>(this IMongoCollection<T> collection, X matchDocument, Action<IModifierExpression<T>> action)
        {
            collection.Update(matchDocument, action, false, false);
        }

        /// <summary>
        /// A count on this collection without any filter.
        /// </summary>
        /// <returns>The count.</returns>
        public static long Count<T>(this IMongoCollection<T> collection)
        {
            return collection.Count(new { });
        }

        /// <summary>
        /// Deletes all indices on this collection.
        /// </summary>
        /// <param retval="numberDeleted">
        /// </param>
        /// <returns>
        /// The delete indices.
        /// </returns>
        public static bool DeleteIndices<T>(this IMongoCollection<T> collection, out int numberDeleted)
        {
            return collection.DeleteIndex("*", out numberDeleted);
        }

        //public static IEnumerable<X> MapReduce<T, X>(this IMongoCollection<T> collection, string map, string reduce)
        //{
        //    return collection.MapReduce<X>(new MapReduceOptions<T> { Map = map, Reduce = reduce });
        //}

        //public static IEnumerable<X> MapReduce<T, U, X>(this IMongoCollection<T> collection, U template, string map, string reduce)
        //{
        //    return collection.MapReduce<X>(new MapReduceOptions<T>() { Query = template, Map = map, Reduce = reduce });
        //}

        //public static IEnumerable<X> MapReduce<T, U, X>(this IMongoCollection<T> collection, U template, string map, string reduce, string finalize)
        //{
        //    return collection.MapReduce<X>(new MapReduceOptions<T> { Query = template, Map = map, Reduce = reduce, Finalize = finalize });
        //}

    }
}
