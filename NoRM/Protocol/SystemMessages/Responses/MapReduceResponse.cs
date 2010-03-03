namespace NoRM.Protocol.SystemMessages.Responses
{
    public class MapReduceResponse
    {
        public string Result { get; set; }
        public MapReduceCount Counts { get; set; }
        public long TimeMillis { get; set; }
        public int Ok { get; set; }

        public class MapReduceCount
        {
            public int Input{ get; set;}    
            public int Emit{ get; set;}
            public int Output{ get; set;}
        }
    }
}