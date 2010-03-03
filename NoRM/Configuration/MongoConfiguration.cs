
using System;

namespace NoRM.Configuration
{
    public static class MongoConfiguration
    {
        private static readonly object _objectLock = new object();
        private static IConfigurationContainer _configuration;

        public static void Initialize(Action<IConfigurationContainer> action)
        {
            action(ConfigurationProvider);
        }

        private static IConfigurationContainer ConfigurationProvider
        {
            get
            {
                //if (_configuration == null)
                //{
                    lock (_objectLock)
                    {
                        //if (_configuration == null)
                        //{

                            _configuration = new ConfigurationContainer();
                        //}
                    }
                //}

                return _configuration;
            }
        }

        internal static string GetPropertyAlias(Type type, string propertyName)
        {
            return _configuration != null
                ? _configuration.GetConfigurationMap().GetPropertyAlias(type, propertyName)
                : propertyName;
        }

        internal static string GetCollectionName(Type type)
        {
            return _configuration != null
                       ? _configuration.GetConfigurationMap().GetCollectionName(type)
                       : type.Name;
        }

        internal static string GetConnectionString(Type type)
        {
            return _configuration != null
                      ? _configuration.GetConfigurationMap().GetConnectionString(type)
                      : null;
        }
    }
}
