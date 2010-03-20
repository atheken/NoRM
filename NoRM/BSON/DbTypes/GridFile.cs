using System;
using System.Collections.Generic;
using System.IO;
using Norm.Attributes;
using Norm.BSON;
using Norm.BSON.DbTypes;

namespace Norm
{
    /// <summary>
    /// Provides a mechanism to store large files in MongoDB.
    /// </summary>
    public class GridFile
    {
        private const int DEFAULT_CHUNK_SIZE = 256*1024; // 256k

        /// <summary>
        /// Initializes a new instance of the <see cref="GridFile"/> class.
        /// </summary>
        public GridFile()
        {
            _id = Guid.NewGuid();
            chunkSize = DEFAULT_CHUNK_SIZE;
        }

        /// <summary>
        /// The ID used to reference this file by the chunks.
        /// </summary>
        // [MongoName("_id")]
        public Guid? _id { get; set; }

        /// <summary>
        /// The original name of this file.
        /// </summary>
        // [MongoName("filename")]
        public string filename { get; set; }

        /// <summary>
        /// A valide MIME type for this file.
        /// </summary>
        // [MongoName("contentType")]
        public string contentType { get; set; }

        /// <summary>
        /// How bit is this file in total?
        /// </summary>
        // [MongoName("length")]
        public long? length { get; set; }

        /// <summary>
        /// How big is each piece of this file in bytes?
        /// </summary>
        // [MongoName("chunkSize")]
        public int? chunkSize { get; set; }

        /// <summary>
        /// Gets or sets ParentCollection.
        /// </summary>
        [MongoIgnore]
        public MongoCollection<GridFile> ParentCollection { get; internal set; }

        /// <summary>
        /// Gets NumberOfChunks.
        /// </summary>
        [MongoIgnore]
        public int NumberOfChunks
        {
            get { return (int) Math.Ceiling((length ?? 0)/(double) (chunkSize ?? 1)); }
        }

        /// <summary>
        /// When was this file added?
        /// </summary>
        // [MongoName("uploadDate")]
        public DateTime? uploadDate { get; set; }

        /// <summary>
        /// Other names for this file.
        /// </summary>
        public List<string> aliases { get; set; }

        /// <summary>
        /// Additional info about this file, can be empty
        /// </summary>
        public Flyweight metadata { get; set; }

        // [MongoName("md5")]
        /// <summary>
        /// Gets or sets md5.
        /// </summary>
        public string md5 { get; set; }

        /// <summary>
        /// Writes the stream to the server as chunks.
        /// </summary>
        /// <param name="stream">
        /// File stream
        /// </param>
        /// <param name="overwrite">
        /// if set to <c>true</c> [overwrite].
        /// </param>
        /// <remarks>
        /// Remember to call "save" on this file after writing the bytes to the server.
        /// </remarks>
        public void WriteToServer(Stream stream, bool overwrite)
        {
            length = stream.Length;
            var chunks = ParentCollection.GetChildCollection<GridFileChunk>("chunks");
            var read = 0;
            length = 0;
            var i = -1;
            var buffer = new byte[chunkSize.Value];
            do
            {
                var chunk = new GridFileChunk {file_id = _id, n = i};
                read = stream.Read(buffer, 0, chunkSize.Value);
                chunk.data = buffer;
                length += read;

                if (read <= 0)
                {
                    continue;
                }

                i++;
                chunks.Insert(chunk);
            }
            while (read > 0);
        }

        /// <summary>
        /// Put save this file into the db - you should call this after 
        /// you have set the length, or you have written the stream to the db.
        /// </summary>
        public void Save()
        {
            // Upsert this into the database.
            this.ParentCollection.GetChildCollection<GridFile>("files").Update(new {_id = this._id}, this, false, true);
        }

        /// <summary>
        /// Opens a stream from the server for this file.
        /// </summary>
        /// <returns>
        /// </returns>
        public GridReadStream GetFileStream()
        {
            return new GridReadStream(this, this.ParentCollection);
        }
    }
}