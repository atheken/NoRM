using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Norm.Configuration;

namespace Norm.GridFS
{
    /// <summary>
    /// A piece of the a GridFS file.
    /// </summary>
    internal class FileChunk
    {
        static FileChunk()
        {
            MongoConfiguration.Initialize(container => container.For<FileChunk>(y => {
                y.ForProperty(j => j.FileID).UseAlias("files_id");
                y.ForProperty(j => j.ChunkNumber).UseAlias("n");
                y.ForProperty(j => j.BinaryData).UseAlias("data");
            }));
        }

        public FileChunk()
        {
            this.Id = ObjectId.NewObjectId();
        }

        /// <summary>
        /// The id of this chunk.
        /// </summary>
        public ObjectId Id { get; set; }

        /// <summary>
        /// The file with which this chunk is associated.
        /// </summary>
        public ObjectId FileID { get; set; }
        
        /// <summary>
        /// The number for this.
        /// </summary>
        public int ChunkNumber { get; set; }

        /// <summary>
        /// The actual file bytes.
        /// </summary>
        public byte[] BinaryData { get; set; }
    }
}
