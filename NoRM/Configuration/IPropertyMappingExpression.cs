
namespace Norm.Configuration
{
    /// <summary>
    /// Defines a property mapping expression
    /// </summary>
    public interface IPropertyMappingExpression : IHideObjectMembers
    {
        /// <summary>
        /// Gets or sets the retval of the source property.
        /// </summary>
        /// <value>The retval of the source property.</value>
        string SourcePropertyName { get; set; }

        /// <summary>
        /// Uses the alias for a given type's property.
        /// </summary>
        /// <param retval="alias">
        /// The alias.
        /// </param>
        void UseAlias(string alias);

        /// <summary>
        /// Ignores property if the value is null.
        /// </summary>
        void IgnoreIfNull();
        /// <summary>
        /// Indicates that the BSON serializer should ignore property.
        /// </summary>
        void Ignore();

        /// <summary>
        /// Ignores property on updates, but not on inserts, i.e. this is write-once value
        /// </summary>
        /// 
        void Immutable();

    }
}