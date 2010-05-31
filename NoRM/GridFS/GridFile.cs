using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Collections;
using System.IO;
using System.Security.Cryptography;

namespace Norm.GridFS
{
    /// <summary>
    /// Represents a large binary object in the database.
    /// </summary>
    public class GridFile
    {
        /// <summary>
        /// The GridFS representation of this file.
        /// </summary>
        internal FileSummary Summary { get; set; }

        /// <summary>
        /// The collection in which this file's chunks live.
        /// </summary>
        internal IQueryable<FileChunk> Chunks { get; set; }
        /// <summary>
        /// Lazily load the queryable chunks.
        /// </summary>
        internal List<FileChunk> CachedChunks
        {
            get
            {
                if (this._cachedChunks == null)
                {
                    this._cachedChunks = this.Chunks.ToList();
                }
                return this._cachedChunks;
            }
            private set
            {
                this._cachedChunks = value.ToList();
            }
        }
        
        private List<FileChunk> _cachedChunks;

        /// <summary>
        /// The unique id associated with this file.
        /// </summary>
        /// <remarks>Required.</remarks>
        public ObjectId Id
        {
            get { return this.Summary.Id; }
            set { this.Summary.Id = value; }
        }

        /// <summary>
        /// When was this file created?
        /// </summary>
        /// <remarks>Required.</remarks>
        public DateTime UploadDate
        {
            get { return this.Summary.UploadDate; }
            set { this.Summary.UploadDate = value; }
        }

        /// <summary>
        /// The MD5 checksum.
        /// </summary>
        /// <remarks>Required.</remarks>
        public String MD5Checksum
        {
            get { return this.Summary.MD5; }
            set { this.Summary.MD5 = value; }
        }

        /// <summary>
        /// The original file name for this file.
        /// </summary>
        /// <remarks>Optional.</remarks>
        public String FileName
        {

            get { return this.Summary.FileName; }
            set { this.Summary.FileName = value; }
        }

        /// <summary>
        /// The MIME type for this document.
        /// </summary>
        /// <remarks>
        /// Optional.
        /// </remarks>
        public String ContentType
        {
            get { return this.Summary.ContentType; }
            set { this.Summary.ContentType = value; }
        }

        /// <summary>
        /// File Aliases.
        /// </summary>
        /// <remarks>Optional.</remarks>
        public IEnumerable<String> Aliases
        {
            get { return this.Summary.Aliases; }
            set { this.Summary.Aliases = value; }
        }

        /// <summary>
        /// The content of this file.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<byte> Content
        {
            get
            {
                return this.CachedChunks.SelectMany(y => y.BinaryData);
            }
            set
            {
                var hasher = MD5.Create();
                this.CachedChunks.Clear();
                int takeCount = 0;
                int cursor = 0;
                int chunkNumber = 0;
                do
                {
                    var binary = value.Skip(cursor)
                        .Take(this.Summary.ChunkSize).ToArray();

                    takeCount = binary.Length;
                    cursor += takeCount;
                    if (takeCount > 0)
                    {
                        var c = new FileChunk();
                        c.ChunkNumber = chunkNumber;
                        c.FileID = this.Id;
                        c.BinaryData = binary;
                        chunkNumber++;
                    }
                } while (takeCount > 0);
                this.Summary.Length = cursor;
                var hash = hasher.ComputeHash(this.CachedChunks.SelectMany(y => y.BinaryData).ToArray());
                this.MD5Checksum = BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}
