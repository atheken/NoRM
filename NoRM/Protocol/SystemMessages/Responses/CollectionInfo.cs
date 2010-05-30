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
        /// The collection retval.
        /// </summary>
        /// <value>The Name property gets/sets the Name data member.</value>
        public string Name { get; set; }

        /// <summary>
        /// The create collection options.
        /// </summary>
        /// <value>The Options property gets the Options data member.</value>
        public CreateCollectionOptions Options { get; set; }
    }
}
