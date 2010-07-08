using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Collections;
using System.IO;
using System.Security.Cryptography;
using Norm.Attributes;
using Norm.Configuration;

namespace Norm.GridFS
{
    /// <summary>
    /// Represents a large binary object in the database.
    /// </summary>
    public class GridFile
    {
        static GridFile()
        {
            MongoConfiguration.Initialize(container => container.For<GridFile>(y =>
            {
                y.ForProperty(j => j.Length).UseAlias("length");
                y.ForProperty(j => j.ChunkSize).UseAlias("chunkSize");
                y.ForProperty(j => j.UploadDate).UseAlias("uploadDate");
                y.ForProperty(j => j.MD5Checksum).UseAlias("md5");
                y.ForProperty(j => j.FileName).UseAlias("filename");
                y.ForProperty(j => j.ContentType).UseAlias("contentType");
                y.ForProperty(j => j.Aliases).UseAlias("aliases");
            }));
        }


        public GridFile()
        {
            this.UploadDate = DateTime.Now.ToUniversalTime();
            this.ChunkSize = (256 * 1024);//the suggested 256kB chunk size.
            this.Id = ObjectId.NewObjectId();
        }

        private int Length { get; set; }
        private int ChunkSize { get; set; }

        /// <summary>
        /// The collection in which this file's chunks live.
        /// </summary>
        [MongoIgnore]
        internal IQueryable<FileChunk> Chunks { get; set; }
        /// <summary>
        /// Lazily load the queryable chunks.
        /// </summary>
        [MongoIgnore]
        internal List<FileChunk> CachedChunks
        {
            get
            {
                if (this._cachedChunks == null)
                {
                    if (this.Chunks != null)
                    {
                        this._cachedChunks = this.Chunks.ToList();
                    }
                    else
                    {
                        this._cachedChunks = new List<FileChunk>(0);
                    }
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
            get;
            set;
        }

        /// <summary>
        /// When was this file created?
        /// </summary>
        /// <remarks>Required.</remarks>
        public DateTime UploadDate
        {
            get;
            set;
        }

        /// <summary>
        /// The MD5 checksum.
        /// </summary>
        /// <remarks>Required.</remarks>
        public String MD5Checksum
        {
            get;
            set;
        }

        /// <summary>
        /// The original file name for this file.
        /// </summary>
        /// <remarks>Optional.</remarks>
        public String FileName
        {
            get;
            set;
        }

        /// <summary>
        /// The MIME type for this document.
        /// </summary>
        /// <remarks>
        /// Optional.
        /// </remarks>
        public String ContentType
        {
            get;
            set;
        }

        /// <summary>
        /// File Aliases.
        /// </summary>
        /// <remarks>Optional.</remarks>
        public IEnumerable<String> Aliases
        {
            get;
            set;
        }

        /// <summary>
        /// The content of this file.
        /// </summary>
        /// <returns></returns>
        [MongoIgnore]
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
                        .Take(this.ChunkSize).ToArray();

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
                this.Length = cursor;
                var hash = hasher.ComputeHash(this.CachedChunks.SelectMany(y => y.BinaryData).ToArray());
                this.MD5Checksum = BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}
