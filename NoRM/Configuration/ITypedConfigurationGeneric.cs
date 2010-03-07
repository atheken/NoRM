using System;
using System.Linq.Expressions;

namespace NoRM.Configuration
{

    /// <summary>
    /// Type-specific type configuration
    /// </summary>
    /// <typeparam name="T">The ype to configure</typeparam>
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
