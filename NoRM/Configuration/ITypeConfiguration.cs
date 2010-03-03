using System;
using System.Linq.Expressions;

namespace NoRM.Configuration
{
    public interface ITypeConfiguration<T> : IConfigurationAccessor
    {
        IPropertyMappingExpression ForProperty(Expression<Func<T, object>> sourcePropery);
        void UseCollectionNamed(string collectionName);
        void UseConnectionString(string connectionString);
    }
}
