namespace NoRM
{
    using System;
    using System.Collections.Generic;
    using Protocol.Messages;
    using Responses;

    public class MapReduce : IDisposable
    {
        private bool _disposed;
        private readonly MongoDatabase _database;
        private readonly IList<string> _temporaryCollections;

        internal MapReduce(MongoDatabase database)
        {
            _database = database;            
            _temporaryCollections = new List<string>(5);            
        }

        public MapReduceResponse Execute(MapReduceOptions options)
        {
            var response = _database.GetCollection<MapReduceResponse>("$cmd").FindOne(new MapReduceMessage
                                                                                          {
                                                                                              map = options.Map,
                                                                                              reduce = options.Reduce,
                                                                                              mapreduce = options.CollectionName,
                                                                                              keeptemp = options.Permenant,
                                                                                              @out = options.OutputCollectionName,
                                                                                           });
           if (!options.Permenant && !string.IsNullOrEmpty(response.Result))
           {
               _temporaryCollections.Add(response.Result);
           }
            response.PrepareForQuerying(_database);
           return response;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                foreach (var t in _temporaryCollections)
                {
                    try { _database.DropCollection(t); }
                    catch (MongoException){}
                }
            }
            _disposed = true;
        }
        ~MapReduce()
        {
            Dispose(false);
        }
    }
}