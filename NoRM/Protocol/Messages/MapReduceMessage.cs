using NoRM.Attributes;
using NoRM.Configuration;

namespace NoRM.Protocol.Messages
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
                                              });
        }

        public string MapReduce { get; set; }
        public string Map { get; set; }
        public string Reduce { get; set; }
        public bool KeepTemp { get; set; }
        public string Out { get; set; }
        public int? Limit { get; set; }
        [MongoIgnoreIfNull]
        public string Finalize { get; set; }
    }
}