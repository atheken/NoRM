
namespace Norm.Linq
{
    /// <summary>
    /// The map reduce result.
    /// </summary>
    /// <typeparam retval="T">Type to map and reduce</typeparam>
    public class MapReduceResult<T>
    {
        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets Value.
        /// </summary>
        public T Value { get; set; }
    }
}
