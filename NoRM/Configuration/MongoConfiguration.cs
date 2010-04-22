using System;

namespace Norm.Configuration
{
    /// <summary>
    /// This is a singleton with which all property maps should be registered.
    /// </summary>
    /// <remarks>
    /// The BSON Serializer and LINQ-to-Mongo both use this in order to correctly map the property
    /// name on the POCO to its correspondent field name in the database.
    /// 
    /// This is slightly thread-scary.
    /// </remarks>
    public static class MongoConfiguration
    {
        internal static event Action<Type> TypeConfigurationChanged;

        private static readonly object _objectLock = new object();
        private static IConfigurationContainer _configuration;

        /// <summary>
        /// Gets the configuration provider instance.
        /// </summary>
        /// <value>The configuration provider.</value>
        internal static IConfigurationContainer ConfigurationContainer
        {
            get
            {
                if (_configuration == null)
                {
                    lock (_objectLock)
                    {
                        if (_configuration == null)
                        {
                            _configuration = new ConfigurationContainer();
                        }
                    }
                }

                return _configuration;
            }
        }

        /// <summary>
        /// Kill a map for the specified type.
        /// </summary>
        /// <remarks>This is here for unit testing support, use at your own risk.</remarks>
        /// <typeparam name="T"></typeparam>
        public static void RemoveMapFor<T>()
        {
            if (_configuration != null)
            {
                _configuration.RemoveFor<T>();
            }
        }

        /// <summary>
        /// Allows various objects to fire type change event.
        /// </summary>
        /// <param name="t"></param>
        internal static void FireTypeChangedEvent(Type t)
        {
            if (TypeConfigurationChanged != null)
            {
                MongoConfiguration.TypeConfigurationChanged(t);
            }
        }

        /// <summary>
        /// Given this singleton IConfigurationContainer, add a fluently-defined map.
        /// </summary>
        /// <param name="action">The action.</param>
        public static void Initialize(Action<IConfigurationContainer> action)
        {
            action(ConfigurationContainer);
        }

        /// <summary>
        /// Given the type, and the property name,
        /// get the alias as it has been defined by Initialization calls of "add"
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// Property alias if one is configured; otherwise returns the input propertyName
        /// </returns>
        internal static string GetPropertyAlias(Type type, string propertyName)
        {
            return _configuration != null ? _configuration.GetConfigurationMap().GetPropertyAlias(type, propertyName) : propertyName;
        }

        /// <summary>
        /// Given the type, get the fluently configured collection type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type's Collection name</returns>
        internal static string GetCollectionName(Type type)
        {
        	var discriminatingType = MongoDiscriminatedAttribute.GetDiscriminatingTypeFor(type);
            if (discriminatingType != null)
                return discriminatingType.Name;

            var realType = SummaryTypeFor(type) ?? type;
            return _configuration != null ? _configuration.GetConfigurationMap().GetCollectionName(realType) : realType.Name;
        }

        /// <summary>
        /// Given a type, get the connection string defined for it.
        /// </summary>
        /// <remarks>
        /// ATT: Not sure this is needed, should potentially be removed if possible.
        /// </remarks>
        /// <param name="type">The type for whicht to get the connection string.</param>
        /// <returns>
        /// The type's connection string if configured; otherwise null.
        /// </returns>
        internal static string GetConnectionString(Type type)
        {
            return _configuration != null ? _configuration.GetConfigurationMap().GetConnectionString(type) : null;
        }

        /// <summary>
        /// If the given type is a summary objet, the underlying type is returned (else null)
        /// </summary>
        /// <remarks>
        internal static Type SummaryTypeFor(Type type)
        {
            return _configuration != null ? _configuration.GetConfigurationMap().SummaryTypeFor(type) : null;
        }
    }
}