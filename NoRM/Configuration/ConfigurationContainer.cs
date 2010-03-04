
namespace NoRM.Configuration
{
    public class ConfigurationContainer : MongoConfigurationMap, IConfigurationContainer
    {
        private IMongoConfigurationMap _configuration;

        public ConfigurationContainer()
        {
            _configuration = this;
        }

        public void UseMap(IMongoConfigurationMap configurationMap)
        {
            _configuration = configurationMap;
        }

        public IMongoConfigurationMap GetConfigurationMap()
        {
            return _configuration;
        }
    }
}
