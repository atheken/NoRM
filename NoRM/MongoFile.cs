using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.Attributes;

namespace NoRM
{
    /// <summary>
    /// Provides a mechanism to store large files in MongoDB.
    /// </summary>
    public class MongoFile<T> where T : class
    {
        /// <summary>
        /// The ID used to reference this file by the chunks.
        /// </summary>
        [MongoName("_id")]
        public Guid? FileID { get; set; }
        
        /// <summary>
        /// The original name of this file.
        /// </summary>
        [MongoName("filename")]
        public String FileName { get; set; }
       
        /// <summary>
        /// A valide MIME type for this file.
        /// </summary>
        [MongoName("contentType")]
        public String ContentType { get; set; }
        
        /// <summary>
        /// How bit is this file in total?
        /// </summary>
        [MongoName("length")]
        public long? Length { get; set; }
        
        /// <summary>
        /// How big is each piece of this file in bytes?
        /// </summary>
        [MongoName("chunkSize")]
        public long? ChunkSize { get; set; }

        [MongoIgnore]
        public int NumberOfChunks
        {
            get
            {
                return (int)Math.Ceiling((this.Length ?? 0) / (double)(ChunkSize ?? 1));
            }
        }

        /// <summary>
        /// When was this file added?
        /// </summary>
        [MongoName("uploadDate")]
        public DateTime? UploadDate { get; set; }
        
        /// <summary>
        /// Other names for this file.
        /// </summary>
        [MongoName("aliases")]
        public List<String> Aliases { get; set; }
        
        /// <summary>
        /// Additional info about this file, can be empty
        /// </summary>
        [MongoName("metadata")]
        public T MetaData { get; set; }
        
        [MongoName("md5")]
        public String MD5 { get; set; }
    }
}
