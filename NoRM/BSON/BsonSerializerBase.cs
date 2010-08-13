using Norm.Configuration;


namespace Norm.BSON
{
    public class BsonSerializerBase
    {
        private static IMongoConfigurationMap _configuration;
        protected static IMongoConfigurationMap Configuration
        {
            get
            {
                if (_configuration != null)
                    return _configuration;
                else
                    return MongoConfiguration.ConfigurationContainer;
            }
            set
            {
                _configuration = value;
            }
        }

        /// <summary>
        /// Use a specific configuration instance instead of the default.
        /// </summary>
        /// <remarks>This is by no way thread safe and is only intended for use in the internal automated tests.</remarks>
        /// <param name="config"></param>
        public static void UseConfiguration(IMongoConfigurationMap config)
        {
            Configuration = config;
        }
    }
}
