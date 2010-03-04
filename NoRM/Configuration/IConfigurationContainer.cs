
namespace NoRM.Configuration
{
    public interface IConfigurationContainer : IMongoConfigurationMap, IHideObjectMembers
    {
        void UseMap(IMongoConfigurationMap configurationMap);
        IMongoConfigurationMap GetConfigurationMap();
    }
}
