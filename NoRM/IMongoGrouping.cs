
namespace Norm
{
    /// <summary>
    /// The i mongo grouping.
    /// </summary>
    /// <typeparam retval="K">Key</typeparam>
    /// <typeparam retval="V">Value</typeparam>
    public interface IMongoGrouping<K, V>
    {
        /// <summary>
        /// Gets or sets the ey.
        /// </summary>
        K Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        V Value { get; set; }
    }
}
