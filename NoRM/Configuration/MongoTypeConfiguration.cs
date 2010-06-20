using System;
using System.Collections.Generic;

namespace Norm.Configuration
{
    /// <summary>
    /// This class holds all configuration information for type mapping.
    /// </summary>
    public class MongoTypeConfiguration
    {
        private static readonly Dictionary<Type, string> _collectionNames = new Dictionary<Type, string>();
        private static readonly Dictionary<Type, string> _connectionStrings = new Dictionary<Type, string>();
        private static readonly Dictionary<Type, Dictionary<string, PropertyMappingExpression>> _typeConfigurations = new Dictionary<Type, Dictionary<string, PropertyMappingExpression>>();
        private static readonly Dictionary<Type, Type> _summaryTypes = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, bool> _discriminatedTypes = new Dictionary<Type, bool>();

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
        /// defined in the Norm library.
        /// </remarks>
        /// <typeparam retval="T">The type from which to remove mappings.</typeparam>
        internal static void RemoveMappings<T>()
        {
            var t = typeof(T);
            if (t.Assembly == typeof(MongoTypeConfiguration).Assembly)
            {
                throw new NotSupportedException("You may not remove mappings for Norm types. The type you attempted to remove was " + t.FullName);
            }

            if (_typeConfigurations.ContainsKey(t))
            {
                _typeConfigurations.Remove(t);
            }
            if(_collectionNames.ContainsKey(t))
            {
                _collectionNames.Remove(t);
            }
            if (_connectionStrings.ContainsKey(t))
            {
                _connectionStrings.Remove(t);
            }
            if (_discriminatedTypes.ContainsKey(t))
            {
                _discriminatedTypes.Remove(t);
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

        /// <summary>
        /// Gets the discriminated types
        /// </summary>
        /// <value>True if a type is marked as a discriminated type, otherwise false.</value>
        internal static Dictionary<Type, bool> DiscriminatedTypes
        {
            get { return _discriminatedTypes; }
        }
    }
}