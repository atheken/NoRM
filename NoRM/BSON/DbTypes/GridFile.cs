using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.Attributes;
using System.IO;
using NoRM.BSON;
using NoRM.BSON.DbTypes;
using System.Security.Cryptography;

namespace NoRM
{
    /// <summary>
    /// Provides a mechanism to store large files in MongoDB.
    /// </summary>
    public class GridFile
    {
        private const int DEFAULT_CHUNK_SIZE = 256 * 1024;//256k

        public GridFile()
        {
            this._id = Guid.NewGuid();
            this.chunkSize = GridFile.DEFAULT_CHUNK_SIZE;
        }

        /// <summary>
        /// The ID used to reference this file by the chunks.
        /// </summary>
        [MongoName("_id")]
        public Guid? _id { get; set; }

        /// <summary>
        /// The original name of this file.
        /// </summary>
        [MongoName("filename")]
        public String filename { get; set; }

        /// <summary>
        /// A valide MIME type for this file.
        /// </summary>
        [MongoName("contentType")]
        public String contentType { get; set; }

        /// <summary>
        /// How bit is this file in total?
        /// </summary>
        [MongoName("length")]
        public long? length { get; set; }

        /// <summary>
        /// How big is each piece of this file in bytes?
        /// </summary>
        [MongoName("chunkSize")]
        public int? chunkSize { get; set; }

        [MongoIgnore]
        public MongoCollection<GridFile> ParentCollection { get; internal set; }

        [MongoIgnore]
        public int NumberOfChunks
        {
            get
            {
                return (int)Math.Ceiling((this.length ?? 0) / (double)(chunkSize ?? 1));
            }
        }

        /// <summary>
        /// When was this file added?
        /// </summary>
        [MongoName("uploadDate")]
        public DateTime? uploadDate { get; set; }

        /// <summary>
        /// Writes the stream to the server as chunks.
        /// </summary>
        /// <remarks>
        /// Remember to call "save" on this file after writing the bytes to the server.
        /// </remarks>
        /// <param name="stream"></param>
        /// <param name="server"></param>
        public void WriteToServer(Stream stream, bool overwrite)
        {
            this.length = stream.Length;
            var chunks = this.ParentCollection.GetChildCollection<GridFileChunk>("chunks");
            int read = 0;
            this.length = 0;
            int i = -1;
            byte[] buffer = new byte[this.chunkSize.Value];
            do
            {
                GridFileChunk chunk = new GridFileChunk();
                chunk.file_id = this._id;
                chunk.n = i;
                read = stream.Read(buffer, 0, this.chunkSize.Value);
                chunk.data = buffer;
                this.length += read;
                if (read > 0)
                {
                    i++;
                    chunks.Insert(chunk);
                }
            } while (read > 0);

        }

        /// <summary>
        /// Put save this file into the db - you should call this after 
        /// you have set the length, or you have written the stream to the db.
        /// </summary>
        public void Save()
        {
            //upsert this into the database.
            this.ParentCollection.GetChildCollection<GridFile>("files")
                .Update(new { _id = this._id }, this, false, true);
        }

        /// <summary>
        /// Opens a stream from the server for this file.
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public GridReadStream GetFileStream()
        {
            return new GridReadStream(this, this.ParentCollection);
        }

        /// <summary>
        /// Other names for this file.
        /// </summary>
        public List<String> aliases { get; set; }

        /// <summary>
        /// Additional info about this file, can be empty
        /// </summary>
        public Flyweight metadata { get; set; }

        [MongoName("md5")]
        public String md5 { get; set; }
    }


    /// <summary>
    /// Represents a subset of the file.
    /// </summary>
    public class GridFileChunk
    {
        public GridFileChunk()
        {
            this._id = Guid.NewGuid();
        }

        /// <summary>
        /// The id for this chunk.
        /// </summary>
        public Guid? _id { get; set; }

        /// <summary>
        /// the file that this is connected with.
        /// </summary>
        public Guid? file_id { get; set; }

        /// <summary>
        /// Which chunk this is.
        /// </summary>
        public int? n { get; set; }

        /// <summary>
        /// The data in this chunk.
        /// </summary>
        public byte[] data { get; set; }
    }
}
