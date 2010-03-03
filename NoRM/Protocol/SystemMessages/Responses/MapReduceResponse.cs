namespace NoRM.Responses
{
    public class MapReduceResponse
    {
        private MongoDatabase _database;
        public string Result { get; set; }
        public MapReduceCount Counts { get; set; }
        public long TimeMillis { get; set; }
        public int Ok { get; set; }

        
        public class MapReduceCount
        {
            public int Input { get; set; }
            public int Emit { get; set; }
            public int Output { get; set; }
        }

        internal void PrepareForQuerying(MongoDatabase database)
        {
            _database = database;
        }

        public MongoCollection<T> GetCollection<T>()
        {
            return _database.GetCollection<T>(Result);
        }
    }
}