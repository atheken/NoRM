
using System;

namespace NoRM.Configuration
{
    public static class MongoConfiguration
    {
        private static readonly object _objectLock = new object();
        private static IConfigurationContainer _configuration;

        /// <summary>
        /// Initializes a mongo configuration container.
        /// </summary>
        /// <param name="action">The action.</param>
        public static void Initialize(Action<IConfigurationContainer> action)
        {
            action(ConfigurationContainer);
        }
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
        /// Gets the property alias.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Property alias if one is configured; otherwise returns the input propertyName</returns>
        internal static string GetPropertyAlias(Type type, string propertyName)
        {
            return _configuration.GetConfigurationMap().GetPropertyAlias(type, propertyName);
        }
        /// <summary>
        /// Produces the mapped connection string for the type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Type's Collection name</returns>
        internal static string GetCollectionName(Type type)
        {
            return _configuration.GetConfigurationMap().GetCollectionName(type);
        }
        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The type's connection string if configured; otherwise null.</returns>
        internal static string GetConnectionString(Type type)
        {
            return _configuration.GetConfigurationMap().GetConnectionString(type);
        }
    }
}
