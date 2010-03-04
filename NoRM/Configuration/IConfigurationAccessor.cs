using System;

namespace NoRM.Configuration
{
    public interface IConfigurationAccessor : IHideObjectMembers
    {
        string GetCollectionName(Type type);
        string GetConnectionString(Type type);
        string GetPropertyAlias(Type type, string propertyName);
    }
}
