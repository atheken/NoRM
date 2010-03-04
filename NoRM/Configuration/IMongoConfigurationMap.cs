using System;

namespace NoRM.Configuration
{
    public interface IMongoConfigurationMap : IHideObjectMembers
    {
        void For<T>(Action<ITypeConfiguration<T>> typeConfiguration);
        string GetCollectionName(Type type);
        string GetConnectionString(Type type);
        string GetPropertyAlias(Type type, string propertyName);
    }
}
