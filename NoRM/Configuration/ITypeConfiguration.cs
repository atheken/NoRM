using System;
using System.Linq.Expressions;

namespace NoRM.Configuration
{
    public interface ITypeConfiguration
    {
        /// <summary>
        /// Uses a collection name for a given type.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        void UseCollectionNamed(string collectionName);
        /// <summary>
        /// Uses a connection string for a given type.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        void UseConnectionString(string connectionString);
    }

    public interface ITypeConfiguration<T> : ITypeConfiguration
    {
        /// <summary>
        /// Looks up property names for use with aliases.
        /// </summary>
        /// <param name="sourcePropery">The source propery.</param>
        /// <returns></returns>
        IPropertyMappingExpression ForProperty(Expression<Func<T, object>> sourcePropery);
    }
}
