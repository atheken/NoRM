using System;
using System.Collections.Generic;

namespace NoRM.Configuration
{
    public class MongoConfigurationMap : IMongoConfigurationMap
    {
        private readonly Dictionary<Type, object> _typeConfigurations = new Dictionary<Type, object>();

        public void For<T>(Action<ITypeConfiguration<T>> typeConfiguration)
        {
            var typeKey = typeof (T);

            if(!_typeConfigurations.ContainsKey(typeKey))
            {
                _typeConfigurations.Add(typeKey, new MongoTypeConfiguration<T>());
            }

            typeConfiguration((MongoTypeConfiguration<T>)_typeConfigurations[typeKey]);
        }

        public string GetCollectionName(Type type)
        {
            return _typeConfigurations.ContainsKey(type)
               ? ((IConfigurationAccessor)_typeConfigurations[type]).GetCollectionName(type)
               : type.Name;
        }

        public string GetConnectionString(Type type)
        {
            return _typeConfigurations.ContainsKey(type)
               ? ((IConfigurationAccessor)_typeConfigurations[type]).GetConnectionString(type)
               : null;
        }

        public string GetPropertyAlias(Type type, string propertyName)
        {
            return _typeConfigurations.ContainsKey(type)
                ? ((IConfigurationAccessor) _typeConfigurations[type]).GetPropertyAlias(type, propertyName)
                : propertyName;
        }
    }
}
