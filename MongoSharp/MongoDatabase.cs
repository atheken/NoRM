using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoSharp.Protocol.Messages;
using MongoSharp.Protocol.SystemMessages.Requests;
using MongoSharp.Protocol.SystemMessages.Responses;

namespace MongoSharp
{
    public class MongoDatabase
    {
        private String _dbName;
        private MongoServer _server;
        /// <summary>
        /// A reference to the database found using the specified context.
        /// </summary>
        /// <param name="dbname"></param>
        /// <param name="context"></param>
        public MongoDatabase(String dbname, MongoServer server)
        {
            this._dbName = dbname;
            this._server = server;
            this.SizeOnDisk = 0.0;
        }

        public Double SizeOnDisk { get; set; }

        /// <summary>
        /// The database name for this database.
        /// </summary>
        public String DatabaseName
        {
            get
            {
                return this._dbName;
            }
        }

        /// <summary>
        /// Produces a mongodb collection that will produce and
        /// manipulate objects of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public MongoCollection<T> GetCollection<T>(string collectionName) where T : class, new()
        {
            return new MongoCollection<T>(collectionName, this, this._server);
        }


        /// <summary>
        /// Produces a list of all collections currently in this database.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CollectionInfo> GetAllCollections()
        {
            var results = this.GetCollection<CollectionInfo>("system.namespaces").Find();                

            return results;
        }

        /// <summary>
        /// Returns a class containing information about this particular collection.
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public CollectionStatistics GetCollectionStatistics(string collectionName)
        {
            var response = this.GetCollection<CollectionStatistics>("$cmd")
                .FindOne<CollectionStatistics>(new CollectionStatistics() { collstats = collectionName });

            return response;
        }

        /// <summary>
        /// Drops the given collection name from the database.
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public DroppedCollectionResponse DropCollection(string collectionName)
        {
            var response = this.GetCollection<DroppedCollectionResponse>("$cmd")
                .FindOne<DroppedCollectionResponse>(new DroppedCollectionResponse() { drop = collectionName });

            return response;
        }

        /// <summary>
        /// Returns profiling information, IF profiling is enabled for this database. 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ProfilingInformationResponse> GetProfilingInformation()
        {
            // TODO Check this again later - it doesn't seem quite right.
            var response = this.GetCollection<ProfilingInformationResponse>("system.profile")
                .Find();

            return response;
        }

        public ValidateCollectionResponse ValidateCollection(string collectionName, bool? scanData)
        {
            var response = this.GetCollection<ValidateCollectionResponse>("$cmd")
                .FindOne<ValidateCollectionResponse>(new ValidateCollectionResponse { validate = collectionName, scandata = scanData });

            return response;
        }

    }
}
