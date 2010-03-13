
using NoRM.Configuration;

namespace NoRM.Protocol.SystemMessages.Requests
{
    /// <summary>
    /// The collection statistics request.
    /// </summary>
    public class CollectionStatisticsRequest
    {
        /// <summary>
        /// Initializes the <see cref="CollectionStatisticsRequest"/> class.
        /// </summary>
        static CollectionStatisticsRequest()
        {
            MongoConfiguration.Initialize(c => 
                c.For<CollectionStatisticsRequest>(a => a.ForProperty(auth => auth.CollectionStatistics).UseAlias("collstats")));
        }

        /// <summary>
        /// Gets or sets the collection statistics.
        /// </summary>
        public string CollectionStatistics { get; set; }
    }
}