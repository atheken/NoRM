namespace NoRM.Protocol.Messages
{
    public class MapReduceMessage
    {
        public string mapreduce { get; set; }
        public string map { get; set; }
        public string reduce { get; set; }
        public bool keeptemp { get; set; }
        public string @out{ get; set;}
    }
}