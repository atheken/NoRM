
namespace NoRM.Responses
{
    /// <summary>
    /// The map reduce response.
    /// </summary>
    public class MapReduceResponse
    {
        private MongoDatabase _database;
        public string Result { get; set; }
        public MapReduceCount Counts { get; set; }
        public long TimeMillis { get; set; }
        public int Ok { get; set; }


        /// <summary>
        /// The prepare for querying.
        /// </summary>
        /// <param name="database">The database.</param>
        internal void PrepareForQuerying(MongoDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Gets a collection.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <returns></returns>
        public MongoCollection GetCollection(string collectionName)
        {
            return _database.GetCollection(collectionName);
        }

        /// <summary>
        /// Gets a typed collection.
        /// </summary>
        /// <typeparam name="T">Collection type</typeparam>
        /// <returns></returns>
        public MongoCollection<T> GetCollection<T>()
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
            public int Input { get; set; }

            /// <summary>
            /// Gets or sets Emit.
            /// </summary>
            public int Emit { get; set; }

            /// <summary>
            /// Gets or sets Output.
            /// </summary>
            public int Output { get; set; }
        }

        #endregion
    }
}