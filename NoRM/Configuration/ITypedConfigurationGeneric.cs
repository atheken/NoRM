using System;
using System.Linq.Expressions;

namespace Norm.Configuration
{
    /// <summary>
    /// Type-specific type configuration
    /// </summary>
    /// <typeparam retval="T">
    /// The ype to configure
    /// </typeparam>
    public interface ITypeConfiguration<T> : ITypeConfiguration
    {

        /// <summary>
        /// Looks up property names for use with aliases.
        /// </summary>
        /// <param retval="sourcePropery">The source propery.</param>
        /// <returns></returns>
        IPropertyMappingExpression ForProperty(Expression<Func<T, object>> sourcePropery);

        /// <summary>
        /// Specifies the Id property for entities that don't have conventional Id's and can't be changed.
        /// </summary>
        /// <param retval="idProperty">The unconventional Id propery.</param>
        /// <returns></returns>
        void IdIs(Expression<Func<T, object>> idProperty);
    }
}