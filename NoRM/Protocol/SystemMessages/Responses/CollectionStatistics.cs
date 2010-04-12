
using Norm.Configuration;
using System.Collections.Generic;

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
                           a.ForProperty(stat => stat.Namespace).UseAlias("ns");
                           a.ForProperty(stat => stat.Count).UseAlias("count");
                           a.ForProperty(stat => stat.Size).UseAlias("size");
                           a.ForProperty(stat => stat.StorageSize).UseAlias("storageSize");
                           a.ForProperty(stat => stat.NumberOfIndices).UseAlias("nIndexes");
                           a.ForProperty(stat => stat.PaddingFactor).UseAlias("paddingFactor");
                           a.ForProperty(stat => stat.CurrentExtents).UseAlias("numExtents");
                           a.ForProperty(stat => stat.PreviousExtentSize).UseAlias("lastExtentSize");
                           a.ForProperty(stat => stat.Flags).UseAlias("flags");
                           a.ForProperty(stat => stat.LastIndexSize).UseAlias("lIndexSize");
                           a.ForProperty(stat => stat.IndexSizes).UseAlias("indexSizes");
                           a.ForProperty(stat => stat.TotalIndexSize).UseAlias("totalIndexSize");
                       })
                );
        }

        /// <summary>
        /// this correlates to the "numExtents" value that comes back from MongoDB -
        /// not sure what this is, maybe something to do with Sharding?
        /// </summary>
        /// <value></value>
        public long? CurrentExtents { get; set; }

        /// <summary>
        /// Total size of all indices for this collection.
        /// </summary>
        /// <value></value>
        public long? TotalIndexSize { get; set; }

        /// <summary>
        /// ?? The previous size of the indices on disk before some index operation??
        /// </summary>
        /// <value></value>
        public long? LastIndexSize { get; set; }

        /// <summary>
        /// Each index and the size on disk of the index.
        /// </summary>
        /// <value></value>
        public Dictionary<string, double> IndexSizes { get; set; }

        /// <summary>
        /// Not sure what this is, correlates to "lastExtentSize"
        /// </summary>
        /// <value></value>
        public long? PreviousExtentSize { get; set; }

        /// <summary>
        /// No idea what this is.
        /// </summary>
        /// <value></value>
        public long? Flags { get; set; }

        /// <summary>
        /// The amount of space that is allocated so that 
        /// inserts can be done without moving pages on disk.
        /// </summary>
        /// <value></value>
        public double? PaddingFactor { get; set; }

        /// <summary>
        /// The namespace in which this collection lives.
        /// </summary>
        /// <value></value>
        public string Namespace { get; set; }

        /// <summary>
        /// Number of elements in this collection
        /// </summary>
        /// <value></value>
        public long? Count { get; set; }

        /// <summary>
        /// The size of the data in this collection <see cref="StorageSize"/>
        /// </summary>
        /// <value></value>
        public long? Size { get; set; }

        /// <summary>
        /// The size on disk of this collection.
        /// </summary>
        /// <value></value>
        public long? StorageSize { get; set; }

        /// <summary>
        /// The number of indices currently defined on this collection.
        /// </summary>
        /// <remarks>
        /// This number shall always be greater or equal 
        /// to 1, as  _id automatically gets an index.
        /// </remarks>
        /// <value></value>
        public int? NumberOfIndices { get; set; }
    }
}