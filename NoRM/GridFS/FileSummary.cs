using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Configuration;

namespace Norm.GridFS
{
    /// <summary>
    /// Some metadata about a GridFile.
    /// </summary>
    internal class FileSummary
    {
        static FileSummary()
        {
            MongoConfiguration.Initialize(container => container.For<FileSummary>(y =>
            {
                y.ForProperty(j => j.Length).UseAlias("length");
                y.ForProperty(j => j.ChunkSize).UseAlias("chunkSize");
                y.ForProperty(j => j.UploadDate).UseAlias("uploadDate");
                y.ForProperty(j => j.MD5).UseAlias("md5");
                y.ForProperty(j => j.FileName).UseAlias("filename");
                y.ForProperty(j => j.ContentType).UseAlias("contentType");
                y.ForProperty(j => j.Aliases).UseAlias("aliases");
            }));
        }

        public FileSummary()
        {
            this.UploadDate = DateTime.Now.ToUniversalTime();
            this.ChunkSize = (256 * 1024);//the suggested 256kB chunk size.
            this.Id = ObjectId.NewObjectId();
        }

        /// <summary>
        /// The unique id associated with this file.
        /// </summary>
        /// <remarks>Required.</remarks>
        public ObjectId Id { get; set; }

        /// <summary>
        /// The total size of this file.
        /// </summary>
        /// <remarks>Required.</remarks>
        public long Length { get; set; }

        /// <summary>
        /// The size of each chunk in the database, should be no more than 4MB.
        /// </summary>
        /// <remarks>Required.</remarks>
        public int ChunkSize { get; set; }

        /// <summary>
        /// When was this file created?
        /// </summary>
        /// <remarks>Required.</remarks>
        public DateTime UploadDate { get; set; }

        /// <summary>
        /// The MD5 checksum.
        /// </summary>
        /// <remarks>Required.</remarks>
        public String MD5 { get; set; }

        /// <summary>
        /// The original file name for this file.
        /// </summary>
        /// <remarks>Optional.</remarks>
        public String FileName { get; set; }

        /// <summary>
        /// The MIME type for this document.
        /// </summary>
        /// <remarks>
        /// Optional.
        /// </remarks>
        public String ContentType { get; set; }

        /// <summary>
        /// File Aliases.
        /// </summary>
        /// <remarks>Optional.</remarks>
        public IEnumerable<String> Aliases { get; set; }
    }
}
