using Norm.Configuration;
using Norm.Collections;

namespace Norm.Protocol.SystemMessages.Request
{
    /// <summary>
    /// The create collection request.
    /// </summary>
    internal class CreateCollectionRequest : ISystemQuery
    {
        /// <summary>
        /// Initializes the <see cref="CreateCollectionRequest"/> class.
        /// </summary>
        static CreateCollectionRequest()
        {
            MongoConfiguration.Initialize(c =>
                c.For<CreateCollectionRequest>(a =>
                                                   {
                                                       a.ForProperty(auth => auth.Create).UseAlias("create");
                                                       a.ForProperty(auth => auth.Size).UseAlias("size");
                                                       a.ForProperty(auth => auth.Max).UseAlias("max");
                                                       a.ForProperty(auth => auth.Capped).UseAlias("capped");
                                                       a.ForProperty(auth => auth.AutoIndexId).UseAlias("autoIndexId");
                                                   })
                );
        }

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
        public string Create
        {
            get { return this._options.Name; }
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        public int Size
        {
            get { return this._options.Size; }
        }

        /// <summary>
        /// Gets the max.
        /// </summary>
        public long? Max
        {
            get { return this._options.Max; }
        }

        /// <summary>
        /// Gets a value indicating whether capped.
        /// </summary>
        public bool Capped
        {
            get { return this._options.Capped; }
        }

        /// <summary>
        /// Gets a value indicating whether autoIndexId.
        /// </summary>
        public bool AutoIndexId
        {
            get { return this._options.AutoIndexId; }
        }
    }
}