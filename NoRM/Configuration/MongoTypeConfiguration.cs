using System;
using System.Collections.Generic;
using System.Reflection;

namespace Norm.Configuration
{
    /// <summary>
    /// This class holds all configuration information for type mapping.
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
        /// Remove mappings for the specifed type.
        /// </summary>
        /// <remarks>
        /// This is primarily defined for support of unit testing, 
        /// you may use it for client code, but you should *NEVER* call it with types
        /// defined in the NoRM library.
        /// </remarks>
        /// <typeparam name="T">The type from which to remove mappings.</typeparam>
        internal static void RemoveMappings<T>()
        {
            //TODO: this should throw an exception if someone attempts to call it on one of our defined types.
            if (_typeConfigurations.ContainsKey(typeof(T)))
            {
                _typeConfigurations.Remove(typeof(T));
            }
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