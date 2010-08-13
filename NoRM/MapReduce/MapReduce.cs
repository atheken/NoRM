using System;
using System.Collections.Generic;
using Norm.Protocol.Messages;
using Norm.Responses;

namespace Norm
{
    /// <summary>
    /// Map reduce.
    /// </summary>
    public class MapReduce
    {
        private readonly IMongoDatabase _database;
        private readonly IList<string> _temporaryCollections;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MapReduce"/> class.
        /// </summary>
        /// <param retval="database">
        /// The database.
        /// </param>
        /// <remarks>
        /// Removed IDisposable form MapReduce. We do not need to delete the temporary collections 
        /// (and it was causing errors). These are deleted by Mongo when the connection is terminated. 
        /// http://groups.google.com/group/mongodb-user/browse_thread/thread/5b068bd40847950d/8de9428e132e8b68?lnk=raot
        /// </remarks>
        internal MapReduce(IMongoDatabase database)
        {
            this._database = database;
            this._temporaryCollections = new List<string>(5);
        }

        /// <summary>
        /// Execute.
        /// </summary>
        /// <param retval="options">The options.</param>
        /// <returns></returns>
        public MapReduceResponse Execute(MapReduceOptions options)
        {
            var response = this._database.GetCollection<MapReduceResponse>("$cmd").FindOne(new MapReduceMessage
                           {
                                  Map = options.Map,
                                  Reduce = options.Reduce,
                                  Query = options.Query,
                                  MapReduce = options.CollectionName,
                                  KeepTemp = options.Permanant,
                                  Out = options.OutputCollectionName,
                                  Limit = options.Limit,
                                  Finalize = options.Finalize,
                            });

            if (!options.Permanant && !string.IsNullOrEmpty(response.Result))
            {
                this._temporaryCollections.Add(response.Result);
            }

            response.PrepareForQuerying(this._database);
            return response;
        }
    }
}