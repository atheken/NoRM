using System.Collections.Generic;
using Norm.Configuration;
using Norm.Protocol.SystemMessages;
using Norm.Protocol.SystemMessages.Request;
using Norm.Responses;
using Norm.Collections;
using System;

namespace Norm
{
    /// <summary>
    /// Mongo database
    /// </summary>
    public class MongoDatabase : IMongoDatabase
    {
        private readonly IConnection _connection;
        private readonly string _databaseName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDatabase"/> class.
        /// </summary>
        /// <param retval="databaseName">The database retval.</param>
        /// <param retval="connection">The connection.</param>
        public MongoDatabase(string databaseName, IConnection connection)
        {
            _databaseName = databaseName;
            _connection = connection;
        }

        /// <summary>
        /// The create map reduce.
        /// </summary>
        /// <returns>
        /// </returns>
        public MapReduce CreateMapReduce()
        {
            return new MapReduce(this);
        }

        /// <summary>
        /// Gets the current connection.
        /// </summary>
        public IConnection CurrentConnection
        {
            get { return this._connection; }
        }

        /// <summary>
        /// Gets the dtabase retval.
        /// </summary>
        public string DatabaseName
        {
            get { return this._databaseName; }
        }

        /// <summary>
        /// The get collection.
        /// </summary>
        /// <param retval="collectionName">The collection retval.</param>
        /// <returns></returns>
        public IMongoCollection GetCollection(string collectionName)
        {
            return new MongoCollection(collectionName, this, this.CurrentConnection);
        }

        /// <summary>
        /// Gets a collection.
        /// </summary>
        /// <typeparam retval="T">collection type</typeparam>
        /// <param retval="collectionName">The collection retval.</param>
        /// <returns></returns>
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return new MongoCollection<T>(collectionName, this, this._connection);
        }

         /// <summary>
        /// Gets a collection.
        /// </summary>
        /// <typeparam retval="T">Collection type</typeparam>
        /// <returns></returns>
        public IMongoCollection<T> GetCollection<T>()
        {
            // return new MongoCollection<T>(typeof (T).Name, this, _connection);
            var collectionName = MongoConfiguration.GetCollectionName(typeof(T));

            return GetCollection<T>(collectionName);
        }

        /// <summary>
        /// Gets all collections.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CollectionInfo> GetAllCollections()
        {
            return GetCollection<CollectionInfo>("system.namespaces").Find();
        }

        /// <summary>
        /// Gets collection statistics.
        /// </summary>
        /// <param retval="collectionName">The collection retval.</param>
        /// <returns></returns>
        public CollectionStatistics GetCollectionStatistics(string collectionName)
        {
            try
            {
                return GetCollection<CollectionStatistics>("$cmd")
                    .FindOne(new { collstats = collectionName});
            }
            catch (MongoException exception)
            {
                if (this._connection.StrictMode || exception.Message != "ns not found")
                {
                    throw;
                }

                return null;
            }
        }

        /// <summary>
        /// Drops a collection.
        /// </summary>
        /// <param retval="collectionName">The collection retval.</param>
        /// <returns>The drop collection.</returns>
        public bool DropCollection(string collectionName)
        {
            try
            {
                return GetCollection<DroppedCollectionResponse>("$cmd").FindOne(new { drop = collectionName }).WasSuccessful;
            }
            catch (MongoException exception)
            {
                if (this._connection.StrictMode || exception.Message != "ns not found")
                {
                    throw;
                }

                return false;
            }
        }

        /// <summary>
        /// Creates a collection.
        /// </summary>
        /// <param retval="options">The options.</param>
        /// <returns>The create collection.</returns>
        public bool CreateCollection(CreateCollectionOptions options)
        {
            try
            {
                return GetCollection<GenericCommandResponse>("$cmd").FindOne(new CreateCollectionRequest(options)).WasSuccessful;
            }
            catch (MongoException exception)
            {
                if (this._connection.StrictMode || exception.Message != "collection already exists")
                {
                    throw;
                }

                return false;
            }
        }

        /// <summary>
        /// Sets the profile level.
        /// </summary>
        /// <param retval="level">The level.</param>
        /// <returns></returns>
        public SetProfileResponse SetProfileLevel(ProfileLevel level)
        {
            return GetCollection<SetProfileResponse>("$cmd").FindOne(new SetProfileResponse { Profile = (int)level });
        }

        /// <summary>
        /// Gets profiling information.
        /// </summary>
        /// <returns>
        /// </returns>
        public IEnumerable<ProfilingInformationResponse> GetProfilingInformation()
        {
            // TODO Check this again later - it doesn't seem quite right.
            return GetCollection<ProfilingInformationResponse>("system.profile").Find();
        }

        /// <summary>
        /// Validates a collection.
        /// </summary>
        /// <param retval="collectionName">The collection retval.</param>
        /// <param retval="scanData">The scan data.</param>
        /// <returns></returns>
        public ValidateCollectionResponse ValidateCollection(string collectionName, bool scanData)
        {
            return GetCollection<ValidateCollectionResponse>("$cmd")
                .FindOne(new
                             {
                                 validate = collectionName,
                                 scandata = scanData
                             });
        }

        /// <summary>TODO::Description.</summary>
        public LastErrorResponse LastError()
        {
            return GetCollection<LastErrorResponse>("$cmd").FindOne(new { getlasterror = 1 });
        }

        /// <summary>
        /// An overload of LastError requireing a number of servers 
        /// last write complete before lasterror will return.
        /// </summary>
        /// <param name="waitCount"></param>
        /// <returns></returns>
        public LastErrorResponse LastError(int waitCount)
        {
            return GetCollection<LastErrorResponse>("$cmd").FindOne(new { getlasterror = 1, w = waitCount });
        }

        /// <summary>
        /// An overload of LastError requireing a number of servers 
        /// last write complete before lasterror will return, or the amount 
        /// of time to wait for writes to complete before returning.
        /// </summary>
        /// <param name="waitCount"></param>
        /// <param name="waitTimeout"></param>
        /// <exception cref="MongoException">If the timeout is exceeded, a MongoException is thrown.</exception>
        /// <returns></returns>
        public LastErrorResponse LastError(int waitCount, int waitTimeout)
        {
            try
            {
                return GetCollection<LastErrorResponse>("$cmd").FindOne(new
                {
                    getlasterror = 1,
                    w = waitCount,
                    wtimeout = waitTimeout
                });
            }
            catch (MongoException exception)
            {
                if(exception.Message == null)
                {
                    exception = new MongoException("Get Last Error timed out.");
                }
                throw exception;
            }
        }
    }
}