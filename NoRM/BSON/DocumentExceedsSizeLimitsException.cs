using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace NoRM.BSON
{
    /// <summary>
    /// An exception that can be thrown by MongoCollection when the document is more than the MongoDB limit of 4MB.
    /// </summary>
    /// <typeparam name="T">The type of the document that was serialized.</typeparam>
    public class DocumentExceedsSizeLimitsException<T> : Exception
    {
        public DocumentExceedsSizeLimitsException(T document, int size)
        {
            this.DocumentSize = size;
            this.Document = document;
        }

        /// <summary>
        /// The size in bytes of the document after serialization.
        /// </summary>
        public int DocumentSize { get; private set; }

        /// <summary>
        /// The document that was serialized.
        /// </summary>
        public T Document { get; private set; }

    }
}
