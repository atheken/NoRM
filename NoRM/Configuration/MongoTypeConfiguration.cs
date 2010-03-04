using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NoRM.BSON;

namespace NoRM.Configuration
{
    public class MongoTypeConfiguration<T> : ITypeConfiguration<T>
    {
        private readonly Dictionary<Type, MongoConfigurationTypeMap> _maps = new Dictionary<Type, MongoConfigurationTypeMap>();

        public IPropertyMappingExpression ForProperty(Expression<Func<T, object>> sourcePropery)
        {
            var typeMap = GetTypeMap();

            var propertyName = TypeHelper.FindProperty(sourcePropery);
            var expression = new PropertyMappingExpression { SourcePropertyName = propertyName };

            typeMap.FieldMap.Add(propertyName, expression);

            return expression;
        }

        public void UseCollectionNamed(string collectionName)
        {
            GetTypeMap().CollectionName = collectionName;
        }

        public void UseConnectionString(string connectionString)
        {
            GetTypeMap().ConnectionString = connectionString;
        }

        private MongoConfigurationTypeMap GetTypeMap()
        {
            var mapType = typeof(T);

            if (!_maps.ContainsKey(mapType))
            {
                _maps.Add(mapType, new MongoConfigurationTypeMap());
            }

            return _maps[mapType];
        }

        public string GetPropertyAlias(Type type, string propertyName)
        {
            return _maps.ContainsKey(type) && _maps[type].FieldMap.ContainsKey(propertyName)
                ? _maps[type].FieldMap[propertyName].Alias
                : propertyName;
        }
        public string GetCollectionName(Type type)
        {
            return _maps[type].CollectionName;
        }

        public string GetConnectionString(Type type)
        {
            return _maps[type].ConnectionString;
        }
    }
}
