using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Collections;

namespace Norm.GridFS
{
    /// <summary>
    /// Extension methods for gaining access to GridFS.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Gets a file collection from the specified collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootCollection"></param>
        /// <returns></returns>
        public static GridFileCollection Files<T>(this IMongoCollection<T> rootCollection)
        {
            return new GridFileCollection(rootCollection.GetChildCollection<GridFile>("files"),
                rootCollection.GetChildCollection<FileChunk>("chunks"));
        }

        /// <summary>
        /// Gets the file collection from the specified database.
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static GridFileCollection Files(this IMongoDatabase database)
        {
            return new GridFileCollection(database.GetCollection<GridFile>("files"),
                database.GetCollection<FileChunk>("chunks"));
        }
    }
}
