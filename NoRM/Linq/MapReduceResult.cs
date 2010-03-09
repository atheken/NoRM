
namespace NoRM.Linq
{
    /// <summary>
    /// MapReduceResult
    /// </summary>
    /// <typeparam name="T">Type to map and reduce</typeparam>
    public class MapReduceResult<T>
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value { get; set; }
    }
}
