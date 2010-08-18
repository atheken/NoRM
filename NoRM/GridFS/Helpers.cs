using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.BSON;
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
        	var fileChunks = rootCollection.GetChildCollection<FileChunk>("chunks");
			createGridFsIndexes(fileChunks);
        	return new GridFileCollection(rootCollection.GetChildCollection<GridFile>("files"),
                fileChunks);
        }

    	private static void createGridFsIndexes(IMongoCollection<FileChunk> fileChunkCollection)
		{
			fileChunkCollection.CreateIndex(new Expando(new { n = 1, files_id = 1 }), "n_files_id_index", false);
		}

    	/// <summary>
        /// Gets the file collection from the specified database.
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static GridFileCollection Files(this IMongoDatabase database)
    	{
    		var fileChunks = database.GetCollection<FileChunk>("chunks");
			createGridFsIndexes(fileChunks);
    		return new GridFileCollection(database.GetCollection<GridFile>("files"),
                fileChunks);
    	}
    }
}
