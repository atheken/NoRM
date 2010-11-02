
namespace Norm.Configuration
{
    /// <summary>
    /// Defines a property mapping expression
    /// </summary>
    public interface IPropertyMappingExpression : IHideObjectMembers
    {
        /// <summary>
        /// Uses the alias for a given type's property.
        /// </summary>
        /// <param retval="alias">
        /// The alias.
        /// </param>
        void UseAlias(string alias);
    }
}