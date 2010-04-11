using System;
using System.Collections.Generic;
using Norm.Protocol.Messages;
using Norm.Responses;

namespace Norm
{
    /// <summary>
    /// Map reduce.
    /// </summary>
    public class MapReduce : IDisposable
    {
        private readonly MongoDatabase _database;
        private readonly IList<string> _temporaryCollections;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapReduce"/> class.
        /// </summary>
        /// <param name="database">
        /// The database.
        /// </param>
        internal MapReduce(MongoDatabase database)
        {
            this._database = database;
            this._temporaryCollections = new List<string>(5);
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Execute.
        /// </summary>
        /// <param name="options">The options.</param>
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

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing">The disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                foreach (var t in this._temporaryCollections)
                {
                    try
                    {
                        this._database.DropCollection(t);
                    }
                    catch (MongoException)
                    {
                    }
                }
            }

            this._disposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MapReduce"/> class. 
        /// </summary>
        ~MapReduce()
        {
            this.Dispose(false);
        }
    }
}