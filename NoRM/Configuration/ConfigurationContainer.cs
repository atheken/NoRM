using System;
using System.Collections.Generic;
using System.Linq;

namespace NoRM.Configuration
{
    /// <summary>
    /// Mongo configuration container
    /// </summary>
    public class ConfigurationContainer : MongoConfigurationMap, IConfigurationContainer  
    {
        /// <summary>
        /// Registers a mongo type map implicitly.
        /// </summary>
        /// <typeparam name="T">Type of configuration container to create.</typeparam>
        public void AddMap<T>() where T : IMongoConfigurationMap, new()
        {
            new T();
        }
        /// <summary>
        /// Gets the configuration map.
        /// </summary>
        /// <returns></returns>
        public IMongoConfigurationMap GetConfigurationMap()
        {
            return this;
        }
    }
}
