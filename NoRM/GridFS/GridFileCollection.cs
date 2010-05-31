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
        private IMongoCollection<GridFile> FileSummaries { get; set; }

        internal GridFileCollection(IMongoCollection<GridFile> fileSummaries, IMongoCollection<FileChunk> fileChunks)
        {
            this.FileChunks = fileChunks;
            this.FileSummaries = fileSummaries;
        }

        public void Save(GridFile file)
        {
            this.FileSummaries.Save(file);
            this.FileChunks.Delete(new { _id = file.Id });
            if (file.CachedChunks.Any())
            {
                this.FileChunks.Insert(file.CachedChunks);
            }
        }

        /// <summary>
        /// Finds and returns the first file that matches the criteria.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public GridFile FindOne<U>(U template)
        {
            var retval = this.FileSummaries.FindOne(template);
            if(retval != null)
            {
                retval.Chunks = this.FileChunks.AsQueryable().Where(y => y.FileID == retval.Id).OrderBy(j => j.ChunkNumber);
            }
            return retval;
        }

        /// <summary>
        /// Returns all the files that match the criteria.
        /// </summary>
        /// <param name="matchCriteria"></param>
        /// <returns></returns>
        public IEnumerable<GridFile> Find<U>(U template)
        {
            foreach (var f in this.FileSummaries.Find(template))
            {
                f.Chunks = this.FileChunks.AsQueryable().Where(y=>y.FileID == f.Id).OrderBy(j => j.ChunkNumber);
                yield return f;
            }
            yield break;
        }


        /// <summary>
        /// Delete a file with the specifiedID.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="IDofFileToDelete"></param>
        public void Delete(ObjectId IDofFileToDelete)
        {
            if (IDofFileToDelete != null)
            {
                this.FileSummaries.Delete(new { _id = IDofFileToDelete });
                this.FileChunks.Delete(new { _id = IDofFileToDelete });
            }
            else
            {
                this.FileSummaries.Delete(new { });
                this.FileChunks.Delete(new { });
            }
        }
    }
}
