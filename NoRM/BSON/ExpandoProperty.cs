
namespace Norm.BSON
{
    /// <summary>
    /// Expando Property
    /// </summary>
    /// <remarks>
    /// This is a glorified KeyValuePair - but this leaves 
    /// us more "open" to hooking extra stuff on, if we need to at some later point.
    /// </remarks>
    public class ExpandoProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpandoProperty"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public ExpandoProperty(string name, object value)
        {
            this.PropertyName = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; private set; }
    }

}
