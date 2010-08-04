
using System.Globalization;
using Norm.BSON.TypeConverters;
namespace Norm.Configuration
{
    /// <summary>
    /// Mongo configuration container
    /// </summary>
    public class ConfigurationContainer : MongoConfigurationMap, IConfigurationContainer
    {
        public ConfigurationContainer()
        {
            TypeConverterFor<CultureInfo, CultureInfoTypeConverter>();
        }

        /// <summary>
        /// Registers a Mongo Configuration Map by calling the default 
        /// constructor of T (so that's where you should add your mapping logic)
        /// </summary>
        /// <remarks>
        /// BY CONVENTION, the default constructor of T should register the mappings that are relevant.
        /// </remarks>
        /// <typeparam retval="T">
        /// The type of the map that should be added.
        /// </typeparam>
        public void AddMap<T>() where T : IMongoConfigurationMap, new()
        {
            //this is semi-magical, look at remarks as to why this does anything.
            new T();
        }

        /// <summary>
        /// Gets the configuration map.
        /// </summary>
        /// <returns>
        /// </returns>
        public IMongoConfigurationMap GetConfigurationMap()
        {
            return this;
        }
    }
}