
using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// MongoDB information related to a particular collection.
    /// </summary>
    public class CollectionStatistics : BaseStatusMessage
    {
        /// <summary>
        /// Initializes the <see cref="CollectionStatistics"/> class.
        /// </summary>
        static CollectionStatistics()
        {
            MongoConfiguration.Initialize(c => c.For<CollectionStatistics>(a =>
                                                   {
                                                       a.ForProperty(auth => auth.CollectionStats).UseAlias("collstats");
                                                       a.ForProperty(auth => auth.Namespace).UseAlias("ns");
                                                       a.ForProperty(auth => auth.Count).UseAlias("count");
                                                       a.ForProperty(auth => auth.Size).UseAlias("size");
                                                       a.ForProperty(auth => auth.StorageSize).UseAlias("storageSize");
                                                       a.ForProperty(auth => auth.NIndexes).UseAlias("nIndexes");
                                                       a.ForProperty(auth => auth.Ok).UseAlias("ok");
                                                   })
                );
        }

        /// <summary>
        /// Gets or sets the collection stats.
        /// </summary>
        /// <value>The collection stats.</value>
        public string CollectionStats { get; set; }
        /// <summary>
        /// Gets or sets the namespace.
        /// </summary>
        /// <value>The namespace.</value>
        public string Namespace { get; set; }
        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        /// <value>The count.</value>
        public long? Count { get; set; }
        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        public long? Size { get; set; }
        /// <summary>
        /// Gets or sets the storage size.
        /// </summary>
        /// <value>The size of the storage.</value>
        public long? StorageSize { get; set; }
        /// <summary>
        /// Gets or sets the number of indexes.
        /// </summary>
        /// <value>The N indexes.</value>
        public int? NIndexes { get; set; }
    }
}