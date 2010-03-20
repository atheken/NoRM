using Norm.Collections;

namespace Norm.Protocol.SystemMessages.Request
{
    /// <summary>
    /// The create collection request.
    /// </summary>
    internal class CreateCollectionRequest
    {
        private readonly CreateCollectionOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCollectionRequest"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public CreateCollectionRequest(CreateCollectionOptions options)
        {
            this._options = options;
        }

        /// <summary>
        /// Gets options name.
        /// </summary>
        public string create
        {
            get { return this._options.Name; }
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        public int size
        {
            get { return this._options.Size; }
        }

        /// <summary>
        /// Gets the max.
        /// </summary>
        public long? max
        {
            get { return this._options.Max; }
        }

        /// <summary>
        /// Gets a value indicating whether capped.
        /// </summary>
        public bool capped
        {
            get { return this._options.Capped; }
        }

        /// <summary>
        /// Gets a value indicating whether autoIndexId.
        /// </summary>
        public bool autoIndexId
        {
            get { return this._options.AutoIndexId; }
        }
    }
}