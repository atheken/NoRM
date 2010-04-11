using Norm.Attributes;
using Norm.Configuration;

namespace Norm.Protocol.Messages
{
    /// <summary>
    /// The map reduce message.
    /// </summary>
    public class MapReduceMessage
    {
        static MapReduceMessage()
        {
            MongoConfiguration.Initialize(c =>
                                              {
                                                  c.For<MapReduceMessage>(mrm => mrm.ForProperty(m => m.MapReduce).UseAlias("mapreduce"));
                                                  c.For<MapReduceMessage>(mrm => mrm.ForProperty(m => m.Map).UseAlias("map"));
                                                  c.For<MapReduceMessage>(mrm => mrm.ForProperty(m => m.Reduce).UseAlias("reduce"));
                                                  c.For<MapReduceMessage>(mrm => mrm.ForProperty(m => m.KeepTemp).UseAlias("keeptemp"));
                                                  c.For<MapReduceMessage>(mrm => mrm.ForProperty(m => m.Out).UseAlias("out"));
                                                  c.For<MapReduceMessage>(mrm => mrm.ForProperty(m => m.Limit).UseAlias("limit"));
                                                  c.For<MapReduceMessage>(mrm => mrm.ForProperty(m => m.Finalize).UseAlias("finalize"));
                                                  c.For<MapReduceMessage>(mrm => mrm.ForProperty(m => m.Query).UseAlias("query"));
                                              });
        }

        /// <summary>TODO::Description.</summary>
        /// <value>The MapReduce property gets/sets the MapReduce data member.</value>
        public string MapReduce { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>The Map property gets/sets the Map data member.</value>
        public string Map { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>The Reduce property gets/sets the Reduce data member.</value>
        public string Reduce { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>The KeepTemp property gets/sets the KeepTemp data member.</value>
        public bool KeepTemp { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>The Out property gets/sets the Out data member.</value>
        public string Out { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>The Limit property gets/sets the Limit data member.</value>
        public int? Limit { get; set; }

        /// <summary>
        /// Gets or sets the Query
        /// </summary>
        public object Query { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value>The Finalize property gets/sets the Finalize data member.</value>
        [MongoIgnoreIfNull]
        public string Finalize { get; set; }
    }
}