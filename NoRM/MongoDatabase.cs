using NoRM.Configuration;

namespace NoRM
{
    using System;
    using System.Collections.Generic;   
    using Protocol.SystemMessages;
    using Protocol.SystemMessages.Request;
    using Protocol.SystemMessages.Responses;

    public class MongoDatabase
    {
        private readonly IConnection _connection;
        private readonly string _databaseName;

        public MongoDatabase(string databaseName, IConnection connection)
        {
            _databaseName = databaseName;
            _connection = connection;         
        }

        public IConnection CurrentConnection {
            get {
                return _connection;
            }
        }

        public string DatabaseName
        {
            get { return _databaseName; }
        }

        public MongoCollection<T> GetCollection<T>(string collectionName)
        {
            return new MongoCollection<T>(collectionName, this, _connection);
        }

        public MongoCollection<T> GetCollection<T>()
        {
            //return new MongoCollection<T>(typeof (T).Name, this, _connection);
            var collectionName = MongoConfiguration.GetCollectionName(typeof(T));

            return GetCollection<T>(collectionName);
        }

        public IEnumerable<CollectionInfo> GetAllCollections()
        {
            return GetCollection<CollectionInfo>("system.namespaces").Find();
        }

        public CollectionStatistics GetCollectionStatistics(string collectionName)
        {
            try
            {
                return GetCollection<CollectionStatistics>("$cmd").FindOne(new CollectionStatistics {collstats = collectionName});                
            }
            catch (MongoException exception)
            {
                if (_connection.StrictMode || exception.Message != "ns not found")
                {
                    throw;
                }
                return null;
            }            
        }

        public bool DropCollection(string collectionName)
        {
            try
            {
                return GetCollection<DroppedCollectionResponse>("$cmd").FindOne(new {drop = collectionName}).OK == 1;                
            }
            catch (MongoException exception)
            {
                if (_connection.StrictMode || exception.Message != "ns not found")
                {
                    throw;
                }
                return false;
            }
        }

        public bool CreateCollection(CreateCollectionOptions options)
        {

            try
            {
                return GetCollection<GenericCommandResponse>("$cmd").FindOne(new CreateCollectionRequest(options)).OK == 1;                
            }
            catch (MongoException exception)
            {
                if (_connection.StrictMode || exception.Message != "collection already exists")
                {
                    throw;
                }
                return false;
            }                      
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