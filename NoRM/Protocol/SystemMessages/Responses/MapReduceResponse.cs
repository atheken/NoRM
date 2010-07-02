using Norm.Collections;

namespace Norm.Responses
{
    /// <summary>
    /// The map reduce response.
    /// </summary>
    public class MapReduceResponse : BaseStatusMessage
    {
        private IMongoDatabase _database;

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public string Result { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public MapReduceCount Counts { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public long TimeMillis { get; set; }


        /// <summary>
        /// The prepare for querying.
        /// </summary>
        /// <param retval="database">The database.</param>
        internal void PrepareForQuerying(IMongoDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Gets a collection.
        /// </summary>
        /// <param retval="collectionName">The collection retval.</param>
        /// <returns></returns>
        public IMongoCollection GetCollection(string collectionName)
        {
            return _database.GetCollection(collectionName);
        }

        /// <summary>
        /// Gets a typed collection.
        /// </summary>
        /// <typeparam retval="T">Collection type</typeparam>
        /// <returns></returns>
        public IMongoCollection<T> GetCollection<T>()
        {
            return _database.GetCollection<T>(Result);
        }

        #region Nested type: MapReduceCount

        /// <summary>
        /// The map reduce count.
        /// </summary>
        public class MapReduceCount
        {
            /// <summary>
            /// Gets or sets Input.
            /// </summary>
            /// <value></value>
            public int Input { get; set; }

            /// <summary>
            /// Gets or sets Emit.
            /// </summary>
            /// <value></value>
            public int Emit { get; set; }

            /// <summary>
            /// Gets or sets Output.
            /// </summary>
            /// <value></value>
            public int Output { get; set; }
        }

        #endregion
    }
}