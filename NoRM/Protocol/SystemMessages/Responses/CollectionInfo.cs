using Norm.Configuration;
using Norm.Collections;

namespace Norm.Responses
{
    /// <summary>
    /// The collection info.
    /// </summary>
    public class CollectionInfo
    {
        static CollectionInfo()
        {
            MongoConfiguration.Initialize(c => c.For<CollectionInfo>(a => a.ForProperty(auth => auth.Name).UseAlias("name")));
        }

        /// <summary>
        /// Gets or sets the collection name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the create collection options.
        /// </summary>
        /// <value>The options.</value>
        public CreateCollectionOptions Options { get; set; }
    }
}