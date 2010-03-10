using System;
using System.Collections.Generic;

namespace NoRM.Configuration
{
    /// <summary>
    /// The mongo type configuration.
    /// </summary>
    public class MongoTypeConfiguration
    {
        private static readonly Dictionary<Type, string> _collectionNames = new Dictionary<Type, string>();
        private static readonly Dictionary<Type, string> _connectionStrings = new Dictionary<Type, string>();
        private static readonly Dictionary<Type, Dictionary<string, PropertyMappingExpression>> _typeConfigurations =
            new Dictionary<Type, Dictionary<string, PropertyMappingExpression>>();

        /// <summary>
        /// Gets the property maps.
        /// </summary>
        /// <value>The property maps.</value>
        internal static Dictionary<Type, Dictionary<string, PropertyMappingExpression>> PropertyMaps
        {
            get { return _typeConfigurations; }
        }

        /// <summary>
        /// Gets the connection strings.
        /// </summary>
        /// <value>The connection strings.</value>
        internal static Dictionary<Type, string> ConnectionStrings
        {
            get { return _connectionStrings; }
        }

        /// <summary>
        /// Gets the collection names.
        /// </summary>
        /// <value>The collection names.</value>
        internal static Dictionary<Type, string> CollectionNames
        {
            get { return _collectionNames; }
        }
    }
}