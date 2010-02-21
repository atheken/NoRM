using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoRM.Protocol.Messages;
using NoRM.Protocol.SystemMessages.Requests;
using NoRM.Protocol.SystemMessages.Responses;
using NoRM.Protocol.SystemMessages;

namespace NoRM
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
        /// Produces a mongodb collection that will create and
        /// manipulate objects of the specified type, 
        /// overrides the default name with the one you specify.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public MongoCollection<T> GetCollection<T>(string collectionName) where T : class, new()
        {
            return new MongoCollection<T>(collectionName, this, this._server);
        }


        /// <summary>
        /// This will return a collection of the specified type, the name of the collection in 
        /// the database is/will be tha value from typeof(T).Name, so if you change your classnames, be careful...
        /// </summary>
        /// <remarks>
        /// This short-hand was a great idea by Rob, thanks!
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public MongoCollection<T> GetCollection<T>() where T : class, new()
        {
            return new MongoCollection<T>(typeof(T).Name, this, this._server);
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
            var response = GetCollection<CollectionStatistics>("$cmd")
                .FindOne(new CollectionStatistics() { collstats = collectionName });

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

        public SetProfileResponse SetProfileLevel(ProfileLevel level)
        {
            var response = this.GetCollection<SetProfileResponse>("$cmd")
                .FindOne<SetProfileResponse>(new SetProfileResponse() { profile = (int)level });

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

        public ValidateCollectionResponse ValidateCollection(string collectionName, bool scanData)
        {
            var response = this.GetCollection<ValidateCollectionResponse>("$cmd")
                .FindOne(new
                {
                    validate = collectionName,
                    scandata = scanData
                });

            return response;
        }

    }
}
