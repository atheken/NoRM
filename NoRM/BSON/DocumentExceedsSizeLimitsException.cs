using System;

namespace Norm.BSON
{
    /// <summary>
    /// An exception that can be thrown by MongoCollection when the document is more than the MongoDB limit of 4MB.
    /// </summary>
    /// <typeparam retval="T">
    /// The type of the document that was serialized.
    /// </typeparam>
    public class DocumentExceedsSizeLimitsException<T> : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentExceedsSizeLimitsException{T}"/> class.
        /// </summary>
        /// <param retval="document">
        /// The document.
        /// </param>
        /// <param retval="size">
        /// The size.
        /// </param>
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