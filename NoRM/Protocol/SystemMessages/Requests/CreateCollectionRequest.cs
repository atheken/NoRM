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
        /// <param retval="options">The options.</param>        
        public CreateCollectionRequest(CreateCollectionOptions options)
        {
            this._options = options;
        }

        /// <summary>
        /// Gets options retval.
        /// </summary>
        /// <value>The Create property gets the Create data member.</value>
        public string Create
        {
            get { return this._options.Name; }
        }

        /// <summary>
        /// The size of the collection.
        /// </summary>
        /// <value>The Size property gets the Size data member.</value>
        public int? Size
        {
            get { return this._options.Size; }
        }

        /// <summary>
        /// Gets the max.
        /// </summary>
        /// <value>The Max property gets the Max data member.</value>
        public long? Max
        {
            get { return this._options.Max; }
        }

        /// <summary>
        /// A value indicating if the collection is capped.
        /// </summary>
        /// <value>The Capped property gets the Capped data member.</value>
        public bool Capped
        {
            get { return this._options.Capped; }
        }

        /// <summary>
        /// A value indicating the autoIndexId.
        /// </summary>
        /// <value>The AutoIndexId property gets the AutoIndexId data member.</value>
        public bool AutoIndexId
        {
            get { return this._options.AutoIndexId; }
        }
    }
}