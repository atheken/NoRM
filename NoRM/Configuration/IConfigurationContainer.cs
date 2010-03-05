
namespace NoRM.Configuration
{
    /// <summary>
    /// Outlines methods for accessing configuration containers
    /// </summary>
    public interface IConfigurationContainer : IMongoConfigurationMap
    {
        /// <summary>
        /// Registers a mongo type map.
        /// </summary>
        /// <param name="configurationMap">The configuration map.</param>
        //void AddMap(IMongoConfigurationMap configurationMap);
        /// <summary>
        /// Registers a mongo type map implicitly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void AddMap<T>() where T : IMongoConfigurationMap, new();
        /// <summary>
        /// Gets the configuration map.
        /// </summary>
        /// <returns></returns>
        IMongoConfigurationMap GetConfigurationMap();
    }
}
