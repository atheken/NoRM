namespace NoRM.Responses
{
    /// <summary>
    /// MongoDB information related to a particular collection.
    /// </summary>
    public class CollectionStatistics
    {
        public string collstats { get; set; }
        public string Ns { get; set; }
        public long? Count { get; set; }
        public long? Size { get; set; }
        public long? StorageSize { get; set; }
        public int? NIndexes { get; set; }
        public double? OK { get; set; }
    }
}
