using Norm.Attributes;
using Norm.Configuration;

namespace Norm.Protocol.Messages
{
    /// <summary>
    /// The map reduce message.
    /// </summary>
    public class MapReduceMessage
    {
        /// <summary>TODO::Description.</summary>
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
        public string MapReduce { get; set; }

        /// <summary>TODO::Description.</summary>
        public string Map { get; set; }

        /// <summary>TODO::Description.</summary>
        public string Reduce { get; set; }

        /// <summary>TODO::Description.</summary>
        public bool KeepTemp { get; set; }

        /// <summary>TODO::Description.</summary>
        public string Out { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Gets or sets the Query
        /// </summary>
        public object Query { get; set; }

        /// <summary>TODO::Description.</summary>
        [MongoIgnoreIfNull]
        public string Finalize { get; set; }
    }
}