
namespace NoRM
{
    /// <summary>
    /// The i mongo grouping.
    /// </summary>
    /// <typeparam name="K">Key</typeparam>
    /// <typeparam name="V">Value</typeparam>
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
