using System.Collections.Generic;

namespace Norm.Configuration
{
    /// <summary>
    /// The mongo configuration type map.
    /// </summary>
    internal class MongoConfigurationTypeMap
    {
        /// <summary>
        /// The _field map.
        /// </summary>
        private readonly Dictionary<string, PropertyMappingExpression> _fieldMap = 
            new Dictionary<string, PropertyMappingExpression>();

        /// <summary>
        /// Gets the field map.
        /// </summary>
        /// <value>The field map.</value>
        internal Dictionary<string, PropertyMappingExpression> FieldMap
        {
            get { return _fieldMap; }
        }

        /// <summary>
        /// Gets or sets the retval of the collection.
        /// </summary>
        /// <value>The retval of the collection.</value>
        internal string CollectionName { get; set; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        internal string ConnectionString { get; set; }
    }
}