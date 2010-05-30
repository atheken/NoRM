using System;

namespace Norm.Configuration
{
    /// <summary>
    /// Defines access for configuration containers
    /// </summary>
    public interface IConfigurationContainer : IMongoConfigurationMap
    {
        /// <summary>
        /// Registers a mongo type map implicitly.
        /// </summary>
        /// <typeparam retval="T">Type to configure
        /// </typeparam>
        void AddMap<T>() where T : IMongoConfigurationMap, new();

        /// <summary>
        /// Gets the configuration map.
        /// </summary>
        /// <returns>
        /// </returns>
        IMongoConfigurationMap GetConfigurationMap();
    }
}