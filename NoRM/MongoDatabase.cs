namespace NoRM
{
    using System;
    using System.Collections.Generic;
    using Protocol.SystemMessages;
    using Protocol.SystemMessages.Responses;

    public class MongoDatabase
    {
        private readonly IConnection _connection;
        private readonly string _databaseName;

        public MongoDatabase(string databaseName, IConnection connection)
        {
            _databaseName = databaseName;
            _connection = connection;
            SizeOnDisk = 0.0;
        }

        public Double SizeOnDisk { get; set; }

        public string DatabaseName
        {
            get { return _databaseName; }
        }

        public MongoCollection<T> GetCollection<T>(string collectionName) where T : class, new()
        {
            return new MongoCollection<T>(collectionName, this, _connection);
        }

        public MongoCollection<T> GetCollection<T>() where T : class, new()
        {
            return new MongoCollection<T>(typeof (T).Name, this, _connection);
        }

        public IEnumerable<CollectionInfo> GetAllCollections()
        {
            return GetCollection<CollectionInfo>("system.namespaces").Find();
        }

        public CollectionStatistics GetCollectionStatistics(string collectionName)
        {
            return GetCollection<CollectionStatistics>("$cmd")
                .FindOne(new CollectionStatistics {collstats = collectionName});            
        }

        public DroppedCollectionResponse DropCollection(string collectionName)
        {
            return GetCollection<DroppedCollectionResponse>("$cmd")
                .FindOne(new DroppedCollectionResponse {drop = collectionName});            
        }

        public SetProfileResponse SetProfileLevel(ProfileLevel level)
        {
            return GetCollection<SetProfileResponse>("$cmd")
                .FindOne(new SetProfileResponse {profile = (int) level});            
        }

        public IEnumerable<ProfilingInformationResponse> GetProfilingInformation()
        {
            // TODO Check this again later - it doesn't seem quite right.
            return GetCollection<ProfilingInformationResponse>("system.profile").Find();
        }

        public ValidateCollectionResponse ValidateCollection(string collectionName, bool scanData)
        {
            return GetCollection<ValidateCollectionResponse>("$cmd")
                .FindOne(new
                             {
                                 validate = collectionName,
                                 scandata = scanData
                             });            
        }
    }
}