using Norm.Configuration;

namespace Norm
{
    /// <summary>
    /// Map/reduce options for a given type
    /// </summary>
    /// <typeparam retval="T">Type to map and recude</typeparam>
    public class MapReduceOptions<T> : MapReduceOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapReduceOptions&lt;T&gt;"/> class.
        /// </summary>
        public MapReduceOptions()
        {
            CollectionName = MongoConfiguration.GetCollectionName(typeof(T));
        }
    }
}
