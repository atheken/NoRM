
namespace NoRM.BSON
{
    /// <summary>
    /// The expando property.
    /// </summary>
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
        /// Gets PropertyName.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets Value.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; private set; }
    }
}
