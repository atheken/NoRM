
using Norm.Configuration;

namespace Norm.Protocol.SystemMessages.Requests
{
    /// <summary>
    /// The collection statistics request.
    /// </summary>
    internal class CollectionStatisticsRequest : ISystemQuery
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
        /// <value>The User property gets/sets the User data member.</value>
        /// <value>The CollectionStatistics property gets/sets the CollectionStatistics data member.</value>
        public string CollectionStatistics { get; set; }
    }
}