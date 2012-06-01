using System;
using System.Linq.Expressions;

namespace Norm.Configuration
{
    /// <summary>
    /// Type-specific type configuration
    /// </summary>
    /// <typeparam retval="T">
    /// The type to configure
    /// </typeparam>
    public interface ITypeConfiguration<T> : ITypeConfiguration
    {

        /// <summary>
        /// Looks up property names for use with aliases.
        /// </summary>
        /// <param retval="sourcePropery">The source property.</param>
        /// <returns></returns>
        IPropertyMappingExpression ForProperty(Expression<Func<T, object>> sourcePropery);

        /// <summary>
        /// Specifies the Id property for entities that don't have conventional Id's and can't be changed.
        /// </summary>
        /// <param retval="idFunction">The unconventional Id functions.</param>
        /// <returns></returns>
        void IdIs<TValue>(Func<T, TValue> idGetter, Action<T, TValue> idSetter);

        /// <summary>
        /// Specifies the Id function for entities that don't have conventional Id's and can't be changed.
        /// </summary>
        /// <param retval="idFunction">The unconventional Id property.</param>
        /// <returns></returns>
        void IdIs(Expression<Func<T, object>> idProperty);
    }
}