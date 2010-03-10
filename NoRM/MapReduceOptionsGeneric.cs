
namespace NoRM
{
    /// <summary>
    /// The map reduce options.
    /// </summary>
    /// <typeparam name="T">Type to map and reduce</typeparam>
    public class MapReduceOptions<T> : MapReduceOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapReduceOptions{T}"/> class.
        /// </summary>
        public MapReduceOptions()
        {
            CollectionName = typeof(T).Name;
        }
    }
}
