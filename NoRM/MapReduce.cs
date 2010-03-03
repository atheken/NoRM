namespace NoRM
{
    using Protocol.Messages;
    using Protocol.SystemMessages.Responses;

    public class MapReduce<T>
    {
        private readonly MongoDatabase _database;
        internal MapReduce(MongoDatabase database)
        {
            _database = database;
        }
        public MapReduceResponse Execute(string map, string reduce)
        {
            var collectionName = typeof (T).Name;
            return _database.GetCollection<MapReduceResponse>("$cmd").FindOne(new MapReduceMessage
                                                                                          {
                                                                                              map = map,
                                                                                              reduce = reduce,
                                                                                              mapreduce = collectionName,
                                                                                          });
        }
    }
}