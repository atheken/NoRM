using System;

namespace NoRM.BSON.DbTypes
{
    /// <summary>
    /// Represents a subset of the file.
    /// </summary>
    public class GridFileChunk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GridFileChunk"/> class.
        /// </summary>
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