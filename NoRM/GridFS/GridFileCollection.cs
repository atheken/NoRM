using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Norm.Collections;

namespace Norm.GridFS
{
    public class GridFileCollection
    {
        private IMongoCollection<FileChunk> FileChunks { get; set; }
        private IMongoCollection<FileSummary> FileSummaries { get; set; }

        internal GridFileCollection(IMongoCollection<FileSummary> fileSummaries, IMongoCollection<FileChunk> fileChunks)
        {
            this.FileChunks = fileChunks;
            this.FileSummaries = fileSummaries;
        }

        public void Save(GridFile file)
        {
            this.FileSummaries.Update(new { _id = file.Id }, file.Summary, false, true);
            this.FileChunks.Delete(new { _id = file.Id });
            this.FileChunks.Insert(file.CachedChunks);
        }

        /// <summary>
        /// Finds and returns the first file that matches the criteria.
        /// </summary>
        /// <param name="matchCriteria"></param>
        /// <returns></returns>
        public GridFile FindOne(Expression<Func<GridFile, bool>> matchCriteria)
        {
            return null;
        }

        /// <summary>
        /// Returns all the files that match the criteria.
        /// </summary>
        /// <param name="matchCriteria"></param>
        /// <returns></returns>
        public IEnumerable<GridFile> Find(Expression<Func<GridFile, bool>> matchCriteria)
        {

            return Enumerable.Empty<GridFile>();
        }


        /// <summary>
        /// Delete a file with the specifiedID.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="IDofFileToDelete"></param>
        public void Delete(ObjectId IDofFileToDelete)
        {
            this.FileSummaries.Delete(new { _id = IDofFileToDelete });
            this.FileChunks.Delete(new { _id = IDofFileToDelete });
        }
    }
}
